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
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace TradeBot.Core.Services;

/// <summary>
/// Service for checking market prices and retrieving/updating item price data.
/// Manages price comparisons against average prices and identifies profitable trading opportunities.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the CheckThePricesService class.
    /// </summary>
    /// <param name="logger">Logger instance for writing diagnostic messages.</param>
    /// <param name="httpClient">HTTP client for making requests to the market API.</param>
    /// <param name="requestData">Configuration options containing API endpoints and headers.</param>
    /// <param name="dbContext">Database context for accessing price and item data.</param>
    /// <param name="azureStorageHelper">Helper for queue and blob storage operations.</param>
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

    /// <summary>
    /// Retrieves the current average price for a specific item based on its code and stats.
    /// </summary>
    /// <param name="itemPriceRequest">The request model containing the item code and its stats.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an ItemPriceResponseModel
    /// with the item name, stats description, and current average price from the database.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no price data exists for the specified item and stats combination.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when the item code cannot be parsed as either an armor or weapon type.
    /// </exception>
    public async Task<ItemPriceResponseModel> GetItemPriceAsync(ItemPriceRequestModel itemPriceRequest)
    {
        ItemPriceResponseModel itemPriceResponse = new ()
        {
            ItemName = Constants.EquipmentLookup.NameMapping[itemPriceRequest.ItemCode],
            Stats = String.Empty
        };
        if (Enum.TryParse<ArmorType>(itemPriceRequest.ItemCode,ignoreCase: true, out ArmorType armor))
        {
            var stat = itemPriceRequest.Skills.First().Value;
            var armorPrice = await _dbContext.ArmorPrices.Where(ar => ar.Type == armor && 
            ar.Stat == stat).Select(ar => ar.Price).SingleAsync();
            itemPriceResponse.Stats = $"({stat})";
            itemPriceResponse.Price = armorPrice;
            
            return itemPriceResponse;
        }

        if (Enum.TryParse<WeaponType>(itemPriceRequest.ItemCode,ignoreCase: true, out WeaponType weapon))
        {
            var attack = itemPriceRequest.Skills[Constants.EquipmentLookup.AttackStatName];
            var crit = itemPriceRequest.Skills[Constants.EquipmentLookup.CritStatName];
            var weaponPrice = await _dbContext.WeaponPrices.Where(w => 
            w.Type == weapon && 
            w.Crit == crit &&
            w.Attack == attack).Select(w => w.Price).SingleAsync();
            itemPriceResponse.Stats = $"({attack}-{crit})";
            itemPriceResponse.Price = weaponPrice;

            return itemPriceResponse;
        }
        throw new Exception($"{nameof(CheckThePricesService)}:{nameof(GetItemPriceAsync)}: Failed to parse item code!");
    }
    
    /// <summary>
    /// Updates the average price for a specific item in the database.
    /// Creates a new price entry if it does not already exist.
    /// </summary>
    /// <param name="itemPriceRequest">The request model containing the item code, stats, and new price.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a boolean indicating
    /// whether the price update was successful (true) or failed (false).
    /// </returns>
    /// <remarks>
    /// If the price entry does not exist for the given item and stats combination, this method returns false.
    /// Exceptions during database save operations are caught and logged as warnings.
    /// </remarks>
    public async Task<bool> SetItemPriceAsync(ItemSetPriceRequestModel itemPriceRequest)
    {        
        if (Enum.TryParse<ArmorType>(itemPriceRequest.ItemCode,ignoreCase: true, out ArmorType armor))
        {
            var stat = itemPriceRequest.Skills.First().Value;
            var armorPrice = await _dbContext.ArmorPrices.Where(ar => ar.Type == armor && 
            ar.Stat == stat).FirstOrDefaultAsync();
              
            if (armorPrice != null)
            {
                _dbContext.ArmorPrices.Update(armorPrice);                
                armorPrice.Price = itemPriceRequest.Price;
                try
                {  
                    await _dbContext.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogWarning($"{nameof(CheckThePricesService)}: Exception was caught during {nameof(SetItemPriceAsync)} of armor item. {ex.Message}");
                    return false;
                }
                return true;
            }
        }

        if (Enum.TryParse<WeaponType>(itemPriceRequest.ItemCode,ignoreCase: true, out WeaponType weapon))
        {
            var attack = itemPriceRequest.Skills[Constants.EquipmentLookup.AttackStatName];
            var crit = itemPriceRequest.Skills[Constants.EquipmentLookup.CritStatName];
            var weaponPrice = await _dbContext.WeaponPrices.Where(w => 
            w.Type == weapon && 
            w.Crit == crit &&
            w.Attack == attack).FirstOrDefaultAsync();
           if (weaponPrice != null)
            {
                _dbContext.WeaponPrices.Update(weaponPrice);                
                weaponPrice.Price = itemPriceRequest.Price;
                try
                {  
                    await _dbContext.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogWarning($"{nameof(CheckThePricesService)}: Exception was caught during {nameof(SetItemPriceAsync)} of weapon item. {ex.Message}");
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks current market prices and identifies profitable trading deals.
    /// Compares market prices against stored average prices to find arbitrage opportunities.
    /// </summary>
    /// <param name="skipProcessing">
    /// Skip price comparison and discord notifications during the check.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a CheckPricesResult object
    /// with information about items checked, deals found, success status, and any messages from the operation.
    /// </returns>
    /// <remarks>
    /// This method:
    /// 1. Fetches current weapon and armor listings from the market API
    /// 2. Compares prices against stored average prices
    /// 3. Publishes identified trade opportunities to the queue for notification
    /// 4. Updates the local weapon and armor lists
    /// Exceptions during processing are caught and logged, allowing the operation to continue.
    /// </remarks>
    public async Task<CheckPricesResult> CheckPricesAsync(bool skipProcessing)
    {
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Starting to check prices...");
        var result = new CheckPricesResult();

        PrepareQuerryStringParameters();
        try
        {
            foreach(var weaponType in Enum.GetValues<WeaponType>())
            {
                var isSuccessful = await FetchAndStoreWeaponsAsync(weaponType, skipProcessing);
                if(isSuccessful) 
                {
                    result.Messages.Add($"{weaponType} weapons were checked.");
                }
            }

            await MergeWeaponsAsync(_weapons);
            result.Messages.Add($"{_weapons.Count} weapons were upserted in database.");

            foreach(var armorType in Enum.GetValues<ArmorType>())
            {
                var isSuccessful = await FetchAndStoreArmorsAsync(armorType, skipProcessing);
                if(isSuccessful) 
                {
                    result.Messages.Add($"{armorType} armor were checked.");
                }
            }

            await MergeArmorsAsync(_armors);
            result.Messages.Add($"{_armors.Count} armor items were upserted in database.");

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

    private async Task<bool> FetchAndStoreWeaponsAsync(WeaponType weaponType, bool skipProcessing)
    {
        var itemCode = weaponType.ToString().ToLower();
        var weaponListRequest = PrepareRequest(itemCode);
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Making initial fetch POST request to get {Constants.EquipmentLookup.NameMapping[itemCode]} weapon.");
        var initialResponse = await _httpClient.SendAsync(weaponListRequest);
        
        if (!initialResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning($"{nameof(CheckThePricesService)}: Initial {Constants.EquipmentLookup.NameMapping[itemCode]} weapon fetch request failed with status code: {initialResponse.StatusCode} and reason: {initialResponse.ReasonPhrase}");
            await _azureStorageHelper.PushToNotificationsQueueEncodedAsync($"{nameof(CheckThePricesService)}: Application encountered {initialResponse.StatusCode}-{initialResponse.ReasonPhrase} response.");
            throw new Exception($"{nameof(CheckThePricesService)}: Request to host indicates no success.");
        }

        var initialData = await ParseResponseContent(initialResponse.Content);
        if(!skipProcessing) 
        {
            await ProcessPossibleWeaponTradeDealsAsync(initialData.ItemsModel);
        }

        FillWeaponCollection(initialData.ItemsModel);
        var nextCursor = initialData.NextCursor;

        while(nextCursor != null) 
        {
            var batchWeaponRequest = PrepareBatchRequest(itemCode, nextCursor);
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Making {Constants.EquipmentLookup.NameMapping[itemCode]} weapon batch fetch POST request with cursor: {nextCursor}");
            try
            {
                var nextBatchResponse = await _httpClient.SendAsync(batchWeaponRequest);
                var batchData = await ParseResponseContent(nextBatchResponse.Content);
                await ProcessPossibleWeaponTradeDealsAsync(batchData.ItemsModel);

                FillWeaponCollection(batchData.ItemsModel);
                nextCursor = batchData.NextCursor;
            }
            catch(Exception ex)
            {
                if(ex is System.Text.Json.JsonException)
                {
                    _logger.LogInformation($"{nameof(CheckThePricesService)}: Decompression related error. AGAIN.");
                }
                else {
                _logger.LogWarning($"{nameof(CheckThePricesService)}: Exception {ex.Message}.");
                }
                break;
            }
        }
        return true;
    }

    private async Task<bool> FetchAndStoreArmorsAsync(ArmorType armorType, bool skipProcessing)
    {
        var itemCode = armorType.ToString().ToLower();
        var armorListRequest = PrepareRequest(itemCode);
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Making initial fetch POST request to get {Constants.EquipmentLookup.NameMapping[itemCode]} armor items.");
        var initialResponse = await _httpClient.SendAsync(armorListRequest);
        
        if (!initialResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning($"{nameof(CheckThePricesService)}: Initial {Constants.EquipmentLookup.NameMapping[itemCode]} armor fetch request failed with status code: {initialResponse.StatusCode} and reason: {initialResponse.ReasonPhrase}");
            await _azureStorageHelper.PushToNotificationsQueueEncodedAsync($"{nameof(CheckThePricesService)}: Application encountered {initialResponse.StatusCode}-{initialResponse.ReasonPhrase} response.");
            throw new Exception($"{nameof(CheckThePricesService)}: Request to host indicates no success.");
        }

        var initialData = await ParseResponseContent(initialResponse.Content);
        if(!skipProcessing) 
        {
            await ProcessPossibleArmorTradeDealsAsync(initialData.ItemsModel);
        }

        FillArmorCollection(initialData.ItemsModel);
        var nextCursor = initialData.NextCursor;

        while(nextCursor != null)
        {
            var batchArmorRequest = PrepareBatchRequest(itemCode, nextCursor);
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Making armor batch fetch POST request to get {Constants.EquipmentLookup.NameMapping[itemCode]} with cursor: {nextCursor}");
            try
            {
                var nextBatchResponse = await _httpClient.SendAsync(batchArmorRequest);
                var batchData = await ParseResponseContent(nextBatchResponse.Content);
                await ProcessPossibleArmorTradeDealsAsync(batchData.ItemsModel);
                FillArmorCollection(batchData.ItemsModel);
                nextCursor = batchData.NextCursor;
            }
            catch(Exception ex)
            {
                if(ex is System.Text.Json.JsonException)
                {
                    _logger.LogInformation($"{nameof(CheckThePricesService)}: Decompression related error. AGAIN.");
                }
                else {
                _logger.LogWarning($"{nameof(CheckThePricesService)}: Exception {ex.Message}.");
                }
                break;
            }
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
            if(header.Key.Equals(Constants.AlternativeHeaders.RequestPathDictionaryKey, StringComparison.OrdinalIgnoreCase))
            {
                batchWeaponRequest.Headers.Add(Constants.AlternativeHeaders.RequestPathDictionaryKey, Constants.AlternativeHeaders.BatchRequestPathDictionaryValue);
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

    private void FillWeaponCollection(List<ItemResponseModel> weaponList)
    {
        foreach(var equipment in weaponList)
        {
            if (equipment.Price > 2000m) 
            {
                continue;
            }
            _weapons.Add(new Weapon
            {
                Id = equipment.Item.Id,
                CreatedAt = equipment.CreatedAt,
                Type = Enum.Parse<WeaponType>(equipment.ItemCode, ignoreCase: true),
                Price = equipment.Price,
                Attack = equipment.Item.Skills[Constants.EquipmentLookup.AttackStatName],
                Crit = equipment.Item.Skills[Constants.EquipmentLookup.CritStatName]
            });
        }
    }

    private void FillArmorCollection(List<ItemResponseModel> armorList)    
    {
        foreach(var equipment in armorList)
        {
            if (equipment.Price > 2000m) 
            {
                continue;
            }
            _armors.Add(new Armor
            {
                Id = equipment.Item.Id,
                CreatedAt = equipment.CreatedAt,
                Type = Enum.Parse<ArmorType>(equipment.ItemCode, ignoreCase: true),
                Price = equipment.Price,
                Stat = equipment.Item.Skills.First().Value    
            });
        }
    }

    private async Task<ItemMarketDataContainerModel> ParseResponseContent(HttpContent content)
    {
        var parsedContent = await content.ReadFromJsonAsync<List<ItemMarketResponseModel>>() ?? throw new Exception($"{nameof(CheckThePricesService)}: Failed to deserialize response content");
        _logger.LogDebug($"{nameof(CheckThePricesService)}: Equipment response deserialized successfully.");
        return parsedContent[0].Result.Data;
    }

    private async Task MergeWeaponsAsync(List<Weapon> weapons)
    {
        try
        {
            await _dbContext.BulkInsertOrUpdateOrDeleteAsync(weapons);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Successfully upserted {weapons.Count} weapons into database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Error during bulk update weapon items: {ex.Message}");
        }
    }

    private async Task MergeArmorsAsync(List<Armor> armors)
    {
        try
        {
            await _dbContext.BulkInsertOrUpdateOrDeleteAsync(armors);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug($"{nameof(CheckThePricesService)}: Successfully upserted {armors.Count} armors into database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckThePricesService)}: Error during bulk update armors: {ex.Message}");
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

    private async Task ProcessPossibleArmorTradeDealsAsync(List<ItemResponseModel> equipment)
    {
        foreach(var position in equipment)
        {
            var armorType = Enum.Parse<ArmorType>(position.Item.ItemCode, ignoreCase: true);
            var averagePrice = _dbContext.ArmorPrices.SingleOrDefault(
                w=> w.Type == armorType &&
                w.Stat == position.Item.Skills.First().Value)?.Price;
            
            if(averagePrice is not null && position.Price < averagePrice*0.9m && position.CreatedAt > DateTime.UtcNow.AddHours(-1))
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
    private async Task ProcessPossibleWeaponTradeDealsAsync(List<ItemResponseModel> equipment)
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
            
            if (averagePrice is not null && position.Price < averagePrice*0.9m && position.CreatedAt > DateTime.UtcNow.AddHours(-1))
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

