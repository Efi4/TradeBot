using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using TradeBot.Data.Contexts;
using TradeBot.Data.Models;
using TradeBot.Data.Helpers;
using TradeBot.Core.Interfaces;
using TradeBot.Base;
using TradeBot.Base.Objects;
using TradeBot.Base.Models;
using TradeBot.Base.Configuration;


namespace TradeBot.Core.Services;

public class CheckThePricesService : ICheckThePricesService
{
    private readonly ILogger<CheckThePricesService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<RequestDataOptions> _requestData;
    private readonly TradingDbContext _dbContext;
    private readonly IAzureStorageHelper _azureStorageHelper;
    private List<Weapon> _weapons;
    private List<Armor> _armors;
    private int _dealsFound;
    private UriBuilder _initialTransactionRequestUriBuilder;
    private UriBuilder _batchRequestUriBuilder;
    private Dictionary<string,string> _headers;

    public CheckThePricesService(ILogger<CheckThePricesService> logger, HttpClient httpClient, IOptions<RequestDataOptions> requestData, TradingDbContext dbContext, IAzureStorageHelper azureStorageHelper)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression =
                DecompressionMethods.GZip |
                DecompressionMethods.Deflate |
                DecompressionMethods.Brotli
        };        
        _httpClient = new HttpClient(handler) {Timeout = TimeSpan.FromSeconds(30)};
        _logger = logger;
        _requestData = requestData;
        _dbContext = dbContext;
        _azureStorageHelper = azureStorageHelper;
        _weapons = new List<Weapon>();
        _armors = new List<Armor>();
        _initialTransactionRequestUriBuilder = new UriBuilder(requestData.Value.BaseUrl);
        _batchRequestUriBuilder = new UriBuilder(requestData.Value.BaseBatchUrl);
        _headers = requestData.Value.HttpHeadersDictionary;
    }

    public async Task<CheckPricesResult> CheckPricesAsync()
    {
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Clearing equipment tables.");
        await ClearEquipmentTables();

        _logger.LogDebug($"{nameof(CheckThePricesService)}: Starting to check prices...");
        var result = new CheckPricesResult();

        PrepareQuerryStringParameters();
        try
        {
            var armorPricesCount = await _dbContext.ArmorPrices.CountAsync();
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Found {armorPricesCount} prices of armor items.");
            var weaponPricesCount = await _dbContext.WeaponPrices.CountAsync();  
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Found {weaponPricesCount} prices of weapon items.");

            // foreach(var weaponType in Enum.GetValues<WeaponType>())
            // {
            //     if (weaponType is not WeaponType.Tank && weaponType is not WeaponType.Sniper) {continue;} //Add for testing purposes ///TODO: REMOVE
            //     var isSuccessful = await FetchAndStoreWeaponsAsync(weaponType);
            //     if(isSuccessful) 
            //     {
            //         result.Messages.Add($"{weaponType} weapons were checked.");
            //     }
            // }
            // // Insert weapons found into database
            // await InsertWeaponsAsync(_weapons);
            // result.Messages.Add($"{_weapons.Count} weapons were added in database.");

            foreach(var armorType in Enum.GetValues<ArmorType>())
            {
                if (armorType is not ArmorType.Helmet4 && armorType is not ArmorType.Boots4 && armorType is not ArmorType.Gloves4) {continue;} //Add for testing purposes ///TODO: REMOVE
                var isSuccessful = await FetchAndStoreArmorsAsync(armorType);
                if(isSuccessful) 
                {
                    result.Messages.Add($"{armorType} armor were checked.");
                }
            }
            // Insert armor found into database
            await InsertArmorsAsync(_armors);
            result.Messages.Add($"{_armors.Count} armor items were added in database.");

            result.ItemsChecked = _weapons.Count + _armors.Count;
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Items checked: {result.ItemsChecked}");  
            result.DealsFound = _dealsFound;
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Found deals:{_dealsFound}");            

            result.Success = true;
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Price check completed");
            await _azureStorageHelper.PushToNotificationsQueueEncodedAsync($"Price check was completed. {result.ItemsChecked} items checked, {result.DealsFound} deals found.");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Error checking prices: {ex.Message}");
            result.Messages.Add($"Exception {ex.Message} was cought during execution of {nameof(CheckPricesAsync)}");
            result.Success = false;
            return result;
        }
    }

    private async Task<bool> FetchAndStoreWeaponsAsync(WeaponType weaponType)
    {
        try
        {
            var itemCode = weaponType.ToString().ToLower();
            var weaponListRequest = PrepareRequest(itemCode);
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Making initial fetch POST request to get {Constants.EquipmentLookup.NameMapping[itemCode]} weapon.");
            var initialResponse = await _httpClient.SendAsync(weaponListRequest);
            
            if (!initialResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"{nameof(CheckThePricesService)}: Initial {Constants.EquipmentLookup.NameMapping[itemCode]} weapon fetch request failed with status code: {initialResponse.StatusCode} and reason: {initialResponse.ReasonPhrase}");
                await _azureStorageHelper.PushToNotificationsQueueEncodedAsync($"Application encountered {initialResponse.StatusCode}-{initialResponse.ReasonPhrase} response.");
                return false;
            }

            var initialData = await ParseResponseContent(initialResponse.Content);
            await ProcessPossibleWeaponTradeDealsAsync(initialData.ItemsModel);

            FillWeaponCollection(initialData.ItemsModel);
            var nextCursor = initialData.NextCursor;

            while(nextCursor != null) 
            {
                var batchWeaponRequest = PrepareBatchRequest(itemCode, nextCursor);
                _logger.LogDebug($"{nameof(CheckThePricesService)}: Making {Constants.EquipmentLookup.NameMapping[itemCode]} weapon batch fetch POST request with cursor: {nextCursor}");
                
                var nextBatchResponse = await _httpClient.SendAsync(batchWeaponRequest);
                var batchData = await ParseResponseContent(nextBatchResponse.Content);
                await ProcessPossibleWeaponTradeDealsAsync(initialData.ItemsModel);

                FillWeaponCollection(batchData.ItemsModel);
                nextCursor = batchData.NextCursor;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Fetch of weapons resulted in error: {ex.Message}");
        }

        return true;
    }

    private async Task<bool> FetchAndStoreArmorsAsync(ArmorType armorType)
    {
        try
        {
            var itemCode = armorType.ToString().ToLower();
            var armorListRequest = PrepareRequest(itemCode);
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Making initial fetch POST request to get {Constants.EquipmentLookup.NameMapping[itemCode]} armor items.");
            var initialResponse = await _httpClient.SendAsync(armorListRequest);
            
            if (!initialResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"{nameof(CheckThePricesService)}: Initial {Constants.EquipmentLookup.NameMapping[itemCode]} armor fetch request failed with status code: {initialResponse.StatusCode} and reason: {initialResponse.ReasonPhrase}");
                await _azureStorageHelper.PushToNotificationsQueueEncodedAsync($"Application encountered {initialResponse.StatusCode}-{initialResponse.ReasonPhrase} response.");
                return false;
            }

            var initialData = await ParseResponseContent(initialResponse.Content);
            await ProcessPossibleArmorTradeDealsAsync(initialData.ItemsModel);
 
            FillArmorCollection(initialData.ItemsModel);
            var nextCursor = initialData.NextCursor;

            while(nextCursor != null)
            {
                var batchArmorRequest = PrepareBatchRequest(itemCode, nextCursor);
                _logger.LogDebug($"{nameof(CheckThePricesService)}: Making armor batch fetch POST request to get {Constants.EquipmentLookup.NameMapping[itemCode]} with cursor: {nextCursor}");

                var nextBatchResponse = await _httpClient.SendAsync(batchArmorRequest);
                var batchData = await ParseResponseContent(nextBatchResponse.Content);
                await ProcessPossibleArmorTradeDealsAsync(batchData.ItemsModel);
                FillArmorCollection(batchData.ItemsModel);
                nextCursor = batchData.NextCursor;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Fetch of armors resulted in error: {ex.Message}");
        }

        return true;
    }

    private HttpRequestMessage PrepareRequest(string itemCode)
    {
        var equipmentListRequest = GetRequestContent(itemCode);

        foreach (var header in _headers)
        {
            if(!String.IsNullOrEmpty(header.Value))  equipmentListRequest.Headers.Add(header.Key, header.Value);
        }
        string? cookieHeader;
        if(!_headers.TryGetValue("Cookie", out cookieHeader))
        {
            cookieHeader = EnvironmentConfiguration.GetCookieHeader();
        }
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Found cookie, starts with: {cookieHeader.Substring(0, 10)}...");
        }
        else 
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: No cookie found in secrets!");
            throw new MissingMemberException(nameof(cookieHeader));
        }
        
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Initial request prepared.");
        return equipmentListRequest;
    }

    private HttpRequestMessage GetRequestContent(string itemCode)
    {
        if(itemCode.Equals("rifle",StringComparison.OrdinalIgnoreCase))
        {
            return new HttpRequestMessage(HttpMethod.Post, _batchRequestUriBuilder.ToString())
            {
                Content = new StringContent("{\"0\":{\"itemCode\":"
                +$"\"{itemCode}\",\"limit\":12,\"direction\":\"forward\","
                +"\"minSkills\": {"
                +$"\"criticalChance\": {Constants.RareItemsBrowseLimits.MinWeaponCrit},\"attack\": {Constants.RareItemsBrowseLimits.MinWeaponDmg}"
                +"}}}",
                System.Text.Encoding.UTF8, "application/json")
            };
        }
        if(itemCode[^1..].Equals("3"))
        {
            return new HttpRequestMessage(HttpMethod.Post, _batchRequestUriBuilder.ToString())
            {
                Content = new StringContent("{\"0\":{\"itemCode\":"
                +$"\"{itemCode}\",\"limit\":12,\"direction\":\"forward\","
                +"\"minSkills\": {"
                +$"\"{Constants.EquipmentLookup.StatMapping[itemCode[..^1]]}\": {(itemCode.Equals("helmet3") ? Constants.RareItemsBrowseLimits.MinHelmetStat : Constants.RareItemsBrowseLimits.MinArmorStat)}"
                +"}}}",
                System.Text.Encoding.UTF8, "application/json")
            };
        }
        return  new HttpRequestMessage(HttpMethod.Post, _initialTransactionRequestUriBuilder.ToString())
        {
            Content = new StringContent("{\"0\":{\"itemCode\":"
            +$"\"{itemCode}\",\"limit\":12,\"direction\":\"forward\""
            +"},\"1\":{\"itemCode\":"
            +$"\"{itemCode}\",\"limit\":10,\"transactionType\":\"itemMarket\","
            +"\"direction\":\"forward\"}}",
            System.Text.Encoding.UTF8, "application/json")
        };        
    }

    private HttpRequestMessage PrepareBatchRequest(string itemCode, string nextCursor)
    {
        var batchWeaponRequest = GetBatchRequestContent(itemCode, nextCursor);
        
        foreach (var header in _headers)
        {
            if(header.Key.Equals(Constants.AlternativeHeaders.BatchRequestPathDictionaryKey, StringComparison.OrdinalIgnoreCase))
            {
                batchWeaponRequest.Headers.Add(Constants.AlternativeHeaders.BatchRequestPathDictionaryKey, Constants.AlternativeHeaders.BatchRequestPathDictionaryValue);
            }
            else if(!String.IsNullOrEmpty(header.Value))  
            {
                batchWeaponRequest.Headers.Add(header.Key, header.Value);
            }
        }
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Batch request prepared.");
        return batchWeaponRequest;
    }

        private HttpRequestMessage GetBatchRequestContent(string itemCode, string nextCursor)
    {
        if(itemCode.Equals("rifle",StringComparison.OrdinalIgnoreCase))
        {
            return new HttpRequestMessage(HttpMethod.Post, _batchRequestUriBuilder.ToString())
            {
                Content = new StringContent("{\"0\":{\"itemCode\":"
                +$"\"{itemCode}\",\"limit\":12,\"cursor\":\"{nextCursor}\",\"direction\":\"forward\","
                +"\"minSkills\": {"
                +$"\"criticalChance\": {Constants.RareItemsBrowseLimits.MinWeaponCrit},\"attack\": {Constants.RareItemsBrowseLimits.MinWeaponDmg}"
                +"}}}",
                System.Text.Encoding.UTF8, "application/json")
            };
        }
        if(itemCode[^1..].Equals("3"))
        {
            return new HttpRequestMessage(HttpMethod.Post, _batchRequestUriBuilder.ToString())
            {
                Content = new StringContent("{\"0\":{\"itemCode\":"
                +$"\"{itemCode}\",\"limit\":12,\"cursor\":\"{nextCursor}\",\"direction\":\"forward\","
                +"\"minSkills\": {"
                +$"\"{Constants.EquipmentLookup.StatMapping[itemCode[..^1]]}\": {(itemCode.Equals("helmet3") ? Constants.RareItemsBrowseLimits.MinHelmetStat : Constants.RareItemsBrowseLimits.MinArmorStat)}"
                +"}}}",
                System.Text.Encoding.UTF8, "application/json")
            };
        }
        return  new HttpRequestMessage(HttpMethod.Post, _batchRequestUriBuilder.ToString())
        {
            Content = new StringContent("{\"0\":{\"itemCode\":"
            +$"\"{itemCode}\",\"limit\":12,\"cursor\":\"{nextCursor}\","
            +"\"direction\":\"forward\"}}",
            System.Text.Encoding.UTF8, "application/json")
        };        
    }

    private void PrepareQuerryStringParameters()
    {
        var query = HttpUtility.ParseQueryString(_initialTransactionRequestUriBuilder.Query);
        query["batch"] = "1";
        _initialTransactionRequestUriBuilder.Query = query.ToString();
        _batchRequestUriBuilder.Query = query.ToString();
        _initialTransactionRequestUriBuilder.Port = -1; // Ensure default port is being added based on the schema (http/https)
        _batchRequestUriBuilder.Port = -1; 
    }

    private void FillWeaponCollection(List<EquipmentResponseModel> weaponList)
    {
        foreach(var item in weaponList)
        {
            if (item.Price > 2000m) 
            {
                continue;
            }
            _weapons.Add(new Weapon
            {
                Type = Enum.Parse<WeaponType>(item.ItemCode, ignoreCase: true),
                Price = item.Price,
                Attack = item.Item.Skills[Constants.EquipmentLookup.AttackStatName],
                Crit = item.Item.Skills[Constants.EquipmentLookup.CritStatName]
            });
        }
    }

    private void FillArmorCollection(List<EquipmentResponseModel> armorList)    
    {
        armorList.ForEach(item => _armors.Add(new Armor
        {
            Type = Enum.Parse<ArmorType>(item.ItemCode, ignoreCase: true),
            Price = item.Price,
            Stat = item.Item.Skills.First().Value          
        }));
    }

    private async Task<ItemMarketDataContainerModel> ParseResponseContent(HttpContent content)
    {
        var parsedContent = await content.ReadFromJsonAsync<List<ItemMarketResponseModel>>();
        if(parsedContent == null)
        {
            throw new Exception($"{nameof(CheckThePricesService)}: Failed to deserialize response content");
        }
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Equipment response deserialized successfully.");
        return parsedContent[0].Result.Data;
    }

    private async Task InsertWeaponsAsync(List<Weapon> weapons)
    {
        try
        {
            await _dbContext.Weapons.AddRangeAsync(weapons);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Successfully inserted {weapons.Count} weapons into database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Error inserting weapons into database: {ex.Message}");
        }
    }

    private async Task InsertArmorsAsync(List<Armor> armors)
    {
        try
        {
            await _dbContext.Armors.AddRangeAsync(armors);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Successfully inserted {armors.Count} armors into database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Error inserting armors into database: {ex.Message}");
        }
    }

    private async Task ClearEquipmentTables()
    {
        try
        {
            await _dbContext.Armors.ExecuteDeleteAsync();
            await _dbContext.Weapons.ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Error clearing equipment tables: {ex.Message}");
        }
    }

    private async Task ProcessPossibleArmorTradeDealsAsync(List<EquipmentResponseModel> equipment)
    {
        foreach(var position in equipment)
        {
            var armorType = Enum.Parse<ArmorType>(position.Item.ItemCode, ignoreCase: true);
            var averagePrice = _dbContext.ArmorPrices.SingleOrDefault(
                w=> w.Type == armorType &&
                w.Stat == position.Item.Skills.First().Value)?.Price;
            
            if(averagePrice is not null && position.Price < averagePrice*0.9m && position.CreatedAt > DateTime.UtcNow.AddHours(-2))
            {
                try
                {
                    await _azureStorageHelper.PushToTradeDealsQueueEncodedAsync(new EquipmentQueueMessageModel()
                    {
                        Item = position.Item,
                        Price = position.Price,
                        CreatedAt = position.CreatedAt,
                        Margin = Math.Round( (decimal) (0.99m*averagePrice-1.01m*position.Price),3)
                    });
                    _dealsFound++;
                }
                catch(Exception ex)
                {
                    _logger.LogError($"{nameof(CheckThePricesService)}: Armor deal message was not pushed in queue, exception: {ex.Message}");
                }
            }
        }
    }
    private async Task ProcessPossibleWeaponTradeDealsAsync(List<EquipmentResponseModel> equipment)
    {
        foreach(var position in equipment)
        {
            var weaponType = Enum.Parse<WeaponType>(position.Item.ItemCode, ignoreCase: true);
            var attack = position.Item.Skills[Constants.EquipmentLookup.AttackStatName];
            var crit = position.Item.Skills[Constants.EquipmentLookup.CritStatName];
            var averagePrice = _dbContext.WeaponPrices.SingleOrDefault(w=>
                w.Type == weaponType &&
                w.Attack == attack &&
                w.Crit == crit)?.Price;
            
            if (averagePrice is not null && position.Price < averagePrice*0.9m && position.CreatedAt > DateTime.UtcNow.AddHours(-2))
            {
                try
                {
                    await _azureStorageHelper.PushToTradeDealsQueueEncodedAsync(new EquipmentQueueMessageModel()
                    {
                        Item = position.Item,
                        Price = position.Price,
                        CreatedAt = position.CreatedAt,
                        Margin = Math.Round( (decimal) (0.99m*averagePrice - 1.01m*position.Price),3)
                    });
                    _dealsFound++;
                }
                catch(Exception ex)
                {
                    _logger.LogError($"{nameof(CheckThePricesService)}: Weapon deal message was not pushed in queue, exception: {ex.Message}");
                }
            }
        }
    }
}

