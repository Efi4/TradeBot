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

namespace TradeBot.Core.Services;

public class CheckTheAvPricesService : ICheckTheAvPricesService
{
    private readonly ILogger<CheckTheAvPricesService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<RequestData> _requestData;
    private readonly TradingDbContext _dbContext;
    private List<WeaponObject> _weaponObjects;


    public CheckTheAvPricesService(ILogger<CheckTheAvPricesService> logger, HttpClient httpClient, IOptions<RequestData> requestData, TradingDbContext dbContext)
    {
        var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate};
        _logger = logger;
        _httpClient = new HttpClient(handler);
        _requestData = requestData;
        _dbContext = dbContext;
        _weaponObjects = new List<WeaponObject>();
    }

    public async Task<CheckPricesResult> CheckPricesAsync()
    {
        _logger.LogInformation("Starting to check prices...");

        try
        {
            // Make HTTP POST request to fetch prices
            var weapons = await FetchWeaponsAsync();
            _logger.LogInformation($"Fetched {weapons.Count} weapons. Example:{weapons.FirstOrDefault().ToString()}");
            if (weapons.Count == 0)
            {
                return new CheckPricesResult
                {
                    Success = false,
                    Messages = new List<string> { $"HTTP request failed" },
                    CheckedAt = DateTime.Now
                };
            }

            // Insert weapons into database
            await InsertWeaponsAsync(weapons);
            
            var result = new CheckPricesResult
                {
                    Success = true,
                    Messages = new List<string> { $"HTTP request succeeded" },
                    CheckedAt = DateTime.Now
                };
            _logger.LogInformation($"Price check completed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking prices: {ex.Message}");
            return new CheckPricesResult
            {
                Success = false,
                Messages = new List<string> { $"Error: {ex.Message}" },
                CheckedAt = DateTime.Now
            };
        }
    }

    private async Task<List<WeaponObject>> FetchWeaponsAsync()
    {
        try
        {
            // var url = Environment.GetEnvironmentVariable("PRICECHECK_API_URL") ?? "https://api.example.com/prices";
            var initialTransactionRequestUriBuilder = new UriBuilder(_requestData.Value.BaseUrl);
            var batchRequestUriBuilder = new UriBuilder(_requestData.Value.BaseBatchUrl);
            var query = HttpUtility.ParseQueryString(initialTransactionRequestUriBuilder.Query);
            query["batch"] = "1";
            initialTransactionRequestUriBuilder.Query = query.ToString();
            initialTransactionRequestUriBuilder.Port = -1; // Ensure default port is being added based on the schema (http/https)
            batchRequestUriBuilder.Query = query.ToString();
            batchRequestUriBuilder.Port = -1; 
            // Create request with headers including cookie
            var itemCode = "sniper";
            var weaponListRequest = new HttpRequestMessage(HttpMethod.Post, initialTransactionRequestUriBuilder.ToString())
            {
                Content = new StringContent("{\"0\":{\"itemCode\":"
                +$"\"{itemCode}\",\"limit\":12,\"direction\":\"forward\""
                +"},\"1\":{\"itemCode\":"
                +$"\"{itemCode}\",\"limit\":10,\"transactionType\":\"itemMarket\","
                +"\"direction\":\"forward\"}}",
                System.Text.Encoding.UTF8, "application/json")
            };
 
            // Add headers
            var headers = _requestData.Value.HttpHeadersDictionary;
            foreach (var header in headers)
            {
                if(!String.IsNullOrEmpty(header.Value))  weaponListRequest.Headers.Add(header.Key, header.Value);
            }
            headers.TryGetValue("Cookie", out var cookieHeader);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                _logger.LogInformation($"Found cookie in secrets, starts with: {cookieHeader.Substring(0, 10)}...");
            }
            else _logger.LogWarning($"Error: No cookie found in secrets");

            _logger.LogInformation($"Making initial fetch POST request to {initialTransactionRequestUriBuilder.ToString()}");
            var response = await _httpClient.SendAsync(weaponListRequest);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Initial weapon request successful: {response.StatusCode}");
                var nextCursor = await ParseResponseContentFillWeaponCollectionAsync(response.Content);
                while(nextCursor != null)
                {
                    var batchWeaponRequest = new HttpRequestMessage(HttpMethod.Post, batchRequestUriBuilder.ToString())
                    {
                        Content = new StringContent("{\"0\":{\"itemCode\":"
                        +$"\"{itemCode}\",\"limit\":12,\"cursor\":\"{nextCursor}\","
                        +"\"direction\":\"forward\"}}",
                        System.Text.Encoding.UTF8, "application/json")
                    };
                    Console.WriteLine($"Content: {batchWeaponRequest.Content.ReadAsStringAsync().Result}");
                    foreach (var header in headers)
                    {
                        if(header.Key.Equals("%3Apath", StringComparison.OrdinalIgnoreCase))
                        {
                            batchWeaponRequest.Headers.Add(header.Key, "/trpc/itemOffer.getItemOffers?batch=1");
                        }
                        else if(!String.IsNullOrEmpty(header.Value))  batchWeaponRequest.Headers.Add(header.Key, header.Value);

                    }
                    _logger.LogInformation($"Making batch fetch POST request to {batchRequestUriBuilder.ToString()} with cursor: {nextCursor}");
                    var nextBatchResponse = await _httpClient.SendAsync(batchWeaponRequest);
                    nextCursor = await ParseResponseContentFillWeaponCollectionAsync(nextBatchResponse.Content);
                    break; // Adding hard stop to minimize request numbers during testing.                
                }
                return _weaponObjects;
            }
            else
            {
                _logger.LogWarning($"Initial HTTP request failed with status code: {response.StatusCode}");
                return _weaponObjects;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"HTTP request error: {ex.Message}");
            return _weaponObjects;
        }
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
                weaponList.ForEach(item => _weaponObjects.Add(new WeaponObject
                {
                    Type = Enum.Parse<WeaponType>(item.ItemCode, ignoreCase: true),
                    Price = item.Price,
                    Attack = item.Item.Skills["attack"],
                    Crit = item.Item.Skills["criticalChance"]
                }));
        return data.NextCursor;
    }

    private async Task InsertWeaponsAsync(List<WeaponObject> weapons)
    {
        try
        {
            foreach (var weapon in weapons)
            {
                var weaponEntity = new Weapon
                {
                    Type = weapon.Type,
                    Price = weapon.Price,
                    Attack = weapon.Attack,
                    Crit = weapon.Crit,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Weapons.Add(weaponEntity);
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Successfully inserted {weapons.Count} weapons into database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inserting weapons into database: {ex.Message}");
        }
    }
}

