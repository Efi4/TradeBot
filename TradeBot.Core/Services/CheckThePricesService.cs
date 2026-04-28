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

        _logger.LogInformation("Starting to check weapon prices...");
        var result = new CheckPricesResult();
        try
        {            
            foreach(var weaponType in Enum.GetValues<WeaponType>())
            {
                if (weaponType is not WeaponType.Tank) {continue;} //Add for testing purposes
                var weapons = await FetchWeaponsAsync(weaponType);
                result.Messages.Add($"{weaponType} weapons were checked.");
                _weapons.AddRange(weapons);
            }
            result.DealsFound = _dealsFound;
            _logger.LogInformation($"Found deals:{_dealsFound}");
            // Insert weapons into database
            await InsertWeaponsAsync(_weapons);
            result.Messages.Add($"{_weapons.Count} weapons were added in database.");
            result.Success = true;

            _logger.LogInformation($"Price check completed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking weapon prices: {ex.Message}");
            result.Messages.Add($"Exception {ex.Message} was cought during execution of {nameof(CheckPricesAsync)}");
            result.Success = false;
            return result;
        }
    }

    private async Task<List<Weapon>> FetchWeaponsAsync(WeaponType weaponType)
    {
        try
        {
            var itemCode = weaponType.ToString().ToLower();
            var weaponListRequest = PrepareRequest(itemCode);
            _logger.LogInformation($"Making initial fetch POST request to {weaponListRequest.RequestUri}");
            var initialResponse = await _httpClient.SendAsync(weaponListRequest);
            
            if (initialResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Initial weapon request successful: {initialResponse.StatusCode}");
                var nextCursor = await ParseResponseContentFillWeaponCollectionAsync(initialResponse.Content);
                while(nextCursor != null)
                {
                    var batchWeaponRequest = PrepareBatchRequest(itemCode, nextCursor);
                    _logger.LogInformation($"Making batch fetch POST request to {batchWeaponRequest.RequestUri} with cursor: {nextCursor}");
                    var nextBatchResponse = await _httpClient.SendAsync(batchWeaponRequest);
                    nextCursor = await ParseResponseContentFillWeaponCollectionAsync(nextBatchResponse.Content);
                }
                return _weapons;
            }
            else
            {
                _logger.LogWarning($"Initial HTTP request failed with status code: {initialResponse.StatusCode}");
                return _weapons;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fetch of weapons resulted in error: {ex.Message}");
            return _weapons;
        }
    }

    private HttpRequestMessage PrepareRequest(string itemCode)
    {
        var weaponListRequest = new HttpRequestMessage(HttpMethod.Post, _initialTransactionRequestUriBuilder.ToString())
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
            if(!String.IsNullOrEmpty(header.Value))  weaponListRequest.Headers.Add(header.Key, header.Value);
        }
        _headers.TryGetValue("Cookie", out var cookieHeader);
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            _logger.LogInformation($"Found cookie in secrets, starts with: {cookieHeader.Substring(0, 10)}...");
        }
        else _logger.LogWarning($"Error: No cookie found in secrets");

        return weaponListRequest;
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
        Console.WriteLine($"Content: {batchWeaponRequest.Content.ReadAsStringAsync().Result}");
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

    private async Task<string?> ParseResponseContentFillWeaponCollectionAsync(HttpContent content)    
    {

                var parsedContent = await content.ReadFromJsonAsync<List<ItemMarketResponseModel>>();
                if(parsedContent == null)
                {
                    _logger.LogError("Failed to deserialize response content");
                    throw new Exception("Failed to deserialize response content");
                }
                var data = parsedContent[0].Result.Data;
                var weaponList = data.ItemsModel;
                weaponList.ForEach(item => _weapons.Add(new Weapon
                {
                    Type = Enum.Parse<WeaponType>(item.ItemCode, ignoreCase: true),
                    Price = item.Price,
                    Attack = item.Item.Skills["attack"],
                    Crit = item.Item.Skills["criticalChance"]
                }));
        return data.NextCursor;
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

