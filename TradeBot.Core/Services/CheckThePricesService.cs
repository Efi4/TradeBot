using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Models;
using TradeBot.Base;
using TradeBot.Base.Objects;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using TradeBot.Core.Objects;
using System.Linq;
using TradeBot.Data.Contexts;
using TradeBot.Data.Models;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

namespace TradeBot.Core.Services;

public class CheckThePricesService : ICheckThePricesService
{
    private readonly ILogger<CheckThePricesService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<RequestDataOptions> _requestData;
    private readonly TradingDbContext _dbContext;
    private List<Weapon> _weapons;
    private List<Armor> _armors;
    private int _dealsFound;
    private UriBuilder _initialTransactionRequestUriBuilder;
    private UriBuilder _batchRequestUriBuilder;
    private Dictionary<string,string> _headers;

    public CheckThePricesService(ILogger<CheckThePricesService> logger, HttpClient httpClient, IOptions<RequestDataOptions> requestData, TradingDbContext dbContext)
    {
        var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate};
        _logger = logger;
        _httpClient = new HttpClient(handler) {Timeout = TimeSpan.FromSeconds(30)};
        _requestData = requestData;
        _dbContext = dbContext;
        _weapons = new List<Weapon>();
        _armors = new List<Armor>();
        _initialTransactionRequestUriBuilder = new UriBuilder(requestData.Value.BaseUrl);
        _batchRequestUriBuilder = new UriBuilder(requestData.Value.BaseBatchUrl);
        _headers = requestData.Value.HttpHeadersDictionary;
    }

    public async Task<CheckPricesResult> CheckPricesAsync()
    {
        _logger.LogInformation("Clearing equipment tables.");
        await ClearEquipmentTables();

        PrepareQuerryStringParameters();

        _logger.LogInformation("Starting to check prices...");
        var result = new CheckPricesResult();
        try
        {            
            foreach(var weaponType in Enum.GetValues<WeaponType>())
            {
                if (weaponType is not WeaponType.Tank) {continue;} //Add for testing purposes
                var isSuccessful = await FetchAndStoreWeaponsAsync(weaponType);
                if(isSuccessful) 
                {
                    result.Messages.Add($"{weaponType} weapons were checked.");
                }
            }
            // Insert weapons found into database
            await InsertWeaponsAsync(_weapons);
            result.Messages.Add($"{_weapons.Count} weapons were added in database.");
            foreach(var armorType in Enum.GetValues<ArmorType>())
            {
                if (armorType is not ArmorType.Helmet4) {continue;} //Add for testing purposes
                var isSuccessful = await FetchAndStoreArmorsAsync(armorType);
                if(isSuccessful) 
                {
                    result.Messages.Add($"{armorType} armor were checked.");
                }
            }
            // Insert armor found into database
            await InsertArmorsAsync(_armors);
            result.Messages.Add($"{_armors.Count} armor items were added in database.");

            result.DealsFound = _dealsFound;
            _logger.LogInformation($"Found deals:{_dealsFound}");            

            result.Success = true;
            _logger.LogInformation($"Price check completed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking prices: {ex.Message}");
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
            _logger.LogInformation($"Making initial fetch POST request to {weaponListRequest.RequestUri} to get {itemCode} weapon.");
            var initialResponse = await _httpClient.SendAsync(weaponListRequest);
            
            if (!initialResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Initial weapon fetch request failed with status code: {initialResponse.StatusCode}");
                return false;
            }

            _logger.LogInformation($"Initial weapon request successful: {initialResponse.StatusCode}");
            var initialData = await ParseResponseContent(initialResponse.Content);
            
            FillWeaponCollectionAsync(initialData.ItemsModel);
            var nextCursor = initialData.NextCursor;

            while(nextCursor != null)
            {
                var batchWeaponRequest = PrepareBatchRequest(itemCode, nextCursor);
                _logger.LogInformation($"Making weapon batch fetch POST request to {batchWeaponRequest.RequestUri} with cursor: {nextCursor}");
                
                var nextBatchResponse = await _httpClient.SendAsync(batchWeaponRequest);
                
                var batchData = await ParseResponseContent(nextBatchResponse.Content);
                
                FillWeaponCollectionAsync(batchData.ItemsModel);
                nextCursor = batchData.NextCursor;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fetch of weapons resulted in error: {ex.Message}");
        }

        return true;
    }

    private async Task<bool> FetchAndStoreArmorsAsync(ArmorType armorType)
    {
        try
        {
            var itemCode = armorType.ToString().ToLower();
            var armorListRequest = PrepareRequest(itemCode);
            _logger.LogInformation($"Making initial fetch POST request to {armorListRequest.RequestUri} to get {itemCode} armor items.");
            var initialResponse = await _httpClient.SendAsync(armorListRequest);
            
            if (!initialResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Initial armor fetch request failed with status code: {initialResponse.StatusCode}");
                return false;
            }

            _logger.LogInformation($"Initial armor request successful: {initialResponse.StatusCode}");
            var initialData = await ParseResponseContent(initialResponse.Content);
            
            FillArmorCollectionAsync(initialData.ItemsModel);
            var nextCursor = initialData.NextCursor;

            while(nextCursor != null)
            {
                var batchArmorRequest = PrepareBatchRequest(itemCode, nextCursor);
                _logger.LogInformation($"Making armor batch fetch POST request to {batchArmorRequest.RequestUri} with cursor: {nextCursor}");
                
                var nextBatchResponse = await _httpClient.SendAsync(batchArmorRequest);
                
                var batchData = await ParseResponseContent(nextBatchResponse.Content);
                
                FillArmorCollectionAsync(batchData.ItemsModel);
                nextCursor = batchData.NextCursor;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fetch of armors resulted in error: {ex.Message}");
        }

        return true;
    }

    private HttpRequestMessage PrepareRequest(string itemCode)
    {
        var equipmentListRequest = new HttpRequestMessage(HttpMethod.Post, _initialTransactionRequestUriBuilder.ToString())
        {
            Content = new StringContent("{\"0\":{\"itemCode\":"
            +$"\"{itemCode}\",\"limit\":12,\"direction\":\"forward\""
            +"},\"1\":{\"itemCode\":"
            +$"\"{itemCode}\",\"limit\":10,\"transactionType\":\"itemMarket\","
            +"\"direction\":\"forward\"}}",
            System.Text.Encoding.UTF8, "application/json")
        };

        foreach (var header in _headers)
        {
            if(!String.IsNullOrEmpty(header.Value))  equipmentListRequest.Headers.Add(header.Key, header.Value);
        }
        _headers.TryGetValue("Cookie", out var cookieHeader);
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            _logger.LogInformation($"Found cookie in secrets, starts with: {cookieHeader.Substring(0, 10)}...");
        }
        else _logger.LogWarning($"Error: No cookie found in secrets");

        return equipmentListRequest;
    }

    private HttpRequestMessage PrepareBatchRequest(string itemCode, string nextCursor)
    {
        var batchWeaponRequest = new HttpRequestMessage(HttpMethod.Post, _batchRequestUriBuilder.ToString())
        {
            Content = new StringContent("{\"0\":{\"itemCode\":"
            +$"\"{itemCode}\",\"limit\":12,\"cursor\":\"{nextCursor}\","
            +"\"direction\":\"forward\"}}",
            System.Text.Encoding.UTF8, "application/json")
        };
        
        foreach (var header in _headers)
        {
            if(header.Key.Equals("%3Apath", StringComparison.OrdinalIgnoreCase))
            {
                batchWeaponRequest.Headers.Add(header.Key, "/trpc/itemOffer.getItemOffers?batch=1");
            }
            else if(!String.IsNullOrEmpty(header.Value))  batchWeaponRequest.Headers.Add(header.Key, header.Value);
        }
        return batchWeaponRequest;
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

    private void FillWeaponCollectionAsync(List<EquipmentResponseModel> weaponList)
    {
        weaponList.ForEach(item => _weapons.Add(new Weapon
        {
            Type = Enum.Parse<WeaponType>(item.ItemCode, ignoreCase: true),
            Price = item.Price,
            Attack = item.Item.Skills["attack"],
            Crit = item.Item.Skills["criticalChance"]
        }));
    }

    private void FillArmorCollectionAsync(List<EquipmentResponseModel> armorList)    
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
            _logger.LogError("Failed to deserialize response content");
            throw new Exception("Failed to deserialize response content");
        }

        return parsedContent[0].Result.Data;
    }

    private async Task InsertWeaponsAsync(List<Weapon> weapons)
    {
        try
        {
            await _dbContext.Weapons.AddRangeAsync(weapons);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Successfully inserted {weapons.Count} weapons into database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inserting weapons into database: {ex.Message}");
        }
    }

    private async Task InsertArmorsAsync(List<Armor> armors)
    {
        try
        {
            await _dbContext.Armors.AddRangeAsync(armors);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Successfully inserted {armors.Count} armors into database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inserting armors into database: {ex.Message}");
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
            _logger.LogError($"Error clearing equipment tables: {ex.Message}");
        }
    }
}

