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
public class CheckTheLawsService : ICheckTheLawsService
{
    private readonly ILogger<CheckTheLawsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<RequestDataOptions> _requestData;
    private readonly IOptions<CountryLawsListOptions> _countryLaws;
    private readonly TradingDbContext _dbContext;
    private readonly IAzureStorageHelper _azureStorageHelper;
    private UriBuilder _initialTransactionRequestUriBuilder;
    private UriBuilder _batchRequestUriBuilder;
    private Dictionary<string,string> _headers;

    /// <summary>
    /// Initializes a new instance of the CheckTheLawsService class.
    /// </summary>
    /// <param name="logger">Logger instance for writing diagnostic messages.</param>
    /// <param name="requestData">Configuration options containing API endpoints and headers.</param>
    /// <param name="countryLaws">Configuration options containing country laws data.</param>
    /// <param name="dbContext">Database context for data.</param>
    /// <param name="azureStorageHelper">Helper for queue and blob storage operations.</param>
    public CheckTheLawsService(ILogger<CheckTheLawsService> logger, IOptions<RequestDataOptions> requestData, IOptions<CountryLawsListOptions> countryLaws, TradingDbContext dbContext, IAzureStorageHelper azureStorageHelper)
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
        _countryLaws = countryLaws;
        _dbContext = dbContext;
        _azureStorageHelper = azureStorageHelper;
        _initialTransactionRequestUriBuilder = new UriBuilder(requestData.Value.BaseLawsUrl);
        _batchRequestUriBuilder = new UriBuilder(requestData.Value.BaseBatchUrl);
        _headers = requestData.Value.HttpHeadersDictionary;
    }

    /// <summary>
    /// Checks current country laws and identifies region transfer threats.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// This method fetches current country laws data, filters based on configuration,
    /// and publishes region transfer threats to the appropriate queue for notification.
    /// </remarks>
    public async Task CheckTheLawsAsync()
    {
        _logger.LogDebug($"{nameof(CheckTheLawsService)}: Starting to check laws...");
        int lawsChecked = 0;
        var countryList = _countryLaws.Value.CountriesList;

        PrepareQuerryStringParameters();
        try
        {
            foreach(var countryId in countryList)
            {
                var isSuccessful = await FetchAndCheckLawsAsync(countryId);
                if(isSuccessful) 
                {
                    _logger.LogDebug($"{nameof(CheckTheLawsService)}: Country {countryId} laws were checked.");
                    lawsChecked+=10;
                }
            }            
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CheckTheLawsService)}: Error checking laws: {ex.Message}");
        }

        await _azureStorageHelper.PushToNotificationsQueueEncodedAsync($"Law check was completed. {lawsChecked} laws checked, for countries: {string.Join(", ", countryList)}.");
    }

    private async Task<bool> FetchAndCheckLawsAsync(string countryId)
    {
        var lawListRequest = PrepareRequest(countryId);
        _logger.LogDebug($"{nameof(CheckTheLawsService)}: Making initial fetch POST request to get {Constants.CountryLookup.CountryMapping[countryId]} laws.");
        
        try
        {
            var initialResponse = await _httpClient.SendAsync(lawListRequest);

            if (!initialResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"{nameof(CheckTheLawsService)}: Initial {Constants.CountryLookup.CountryMapping[countryId]} law fetch request failed with status code: {initialResponse.StatusCode} and reason: {initialResponse.ReasonPhrase}");
                await _azureStorageHelper.PushToNotificationsQueueEncodedAsync($"{nameof(CheckTheLawsService)}: Application encountered {initialResponse.StatusCode}-{initialResponse.ReasonPhrase} response.");
                throw new Exception($"{nameof(CheckTheLawsService)}: Request to host indicates no success.");
            }

            CountryLawsResponseModel? parsedContent = await initialResponse.Content.ReadFromJsonAsync<CountryLawsResponseModel>();
            var initialData = parsedContent!.LawsResult.Data;
            // var initialData = await ParseResponseContent(initialResponse.Content);

            await ProcessPossibleLawsAsync(initialData.ItemsModel!, countryId);
        }
        catch(Exception ex)
        {
            if(ex is System.Text.Json.JsonException)
            {
                _logger.LogInformation($"{nameof(CheckTheLawsService)}: Decompression related error. AGAIN.");
                Console.WriteLine($"{nameof(CheckTheLawsService)}: {ex.Message}");
            }
            else {
            _logger.LogWarning($"{nameof(CheckTheLawsService)}: Exception {ex.Message}.");
            }
            return false;
        }
        return true;
    }

    private HttpRequestMessage PrepareRequest(string itemCode)
    {
        var lawsListRequest = GetRequestContent(itemCode);

        foreach (var header in _headers)
        {
            if(header.Key.Equals(Constants.AlternativeHeaders.RequestPathDictionaryKey, StringComparison.OrdinalIgnoreCase))
            {
                lawsListRequest.Headers.Add(Constants.AlternativeHeaders.RequestPathDictionaryKey, _countryLaws.Value.RequestPathHeader);
            }
            else if(!String.IsNullOrEmpty(header.Value))  
            {
                lawsListRequest.Headers.Add(header.Key, header.Value);
            }
        }

        string? cookieHeader;
        if(!_headers.TryGetValue("Cookie", out cookieHeader))
        {
            cookieHeader = EnvironmentConfiguration.GetCookieHeader();
        }
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            _logger.LogDebug($"{nameof(CheckTheLawsService)}: Found cookie, starts with: {cookieHeader.Substring(0, 10)}...");
        }
        else 
        {
            _logger.LogError($"{nameof(CheckTheLawsService)}: No cookie found in secrets!");
            throw new MissingMemberException(nameof(cookieHeader));
        }
        
        _logger.LogDebug($"{nameof(CheckTheLawsService)}: Initial request prepared.");
        return lawsListRequest;
    }

    private HttpRequestMessage GetRequestContent(string countryId)
    {
        return  new HttpRequestMessage(HttpMethod.Post, _initialTransactionRequestUriBuilder.ToString())
        {
            Content = new StringContent($"{{\"0\":{{\"countryId\":\"{countryId}\"}}," +
                $"\"1\":{{\"countryId\":\"{countryId}\"}}," +
                $"\"2\":{{\"countryId\":\"{countryId}\",\"limit\":10,"+
                "\"direction\":\"forward\"}}",
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

    // private async Task<CountryLawsDataContainerModel> ParseResponseContent(HttpContent content)
    // {
    //     var parsedContent = await content.ReadFromJsonAsync<List<CountryLawsResponseModel>>() ?? throw new Exception($"{nameof(CheckTheLawsService)}: Failed to deserialize response content");
    //     _logger.LogDebug($"{nameof(CheckTheLawsService)}: Laws response deserialized successfully.");
    //     return parsedContent.LastOrDefault()!.Result.Data;
    // }

    private async Task ProcessPossibleLawsAsync(List<LawItem> lawItems, string countryId)
    {
        foreach(LawItem law in lawItems)
        {
            Enum.TryParse<LawTypes>(law.Data!.Type, ignoreCase: true, out var lawType);
            
            if((lawType is LawTypes.liberate_region or LawTypes.transfer_region) && law.CreatedAt > DateTime.UtcNow.AddMinutes(1500))
            {
                try
                {
                    var regionTransferNotificationMessage = $"Region transfer threat detected: {law.Data!.Type} in {ExtractCountryName(countryId)} laws to {ExtractCountryName(law.Data!.TargetCountry)} at <t:{new DateTimeOffset((DateTime)law.CreatedAt).ToUnixTimeSeconds()}:R>.";
                    await _azureStorageHelper.PushToRegionTransferNotificationsQueueEncodedAsync(regionTransferNotificationMessage);
                }
                catch(Exception ex)
                {
                    _logger.LogError($"{nameof(CheckTheLawsService)}: Region transfer notification message was not pushed in queue, exception: {ex.Message}");
                }
            }
        }
    }
    private static string ExtractCountryName(string countryId)
    {
        string countryName;
        try
        {
            countryName = Constants.CountryLookup.CountryMapping[countryId];
        }
        catch(Exception)
        {
            countryName = "Unknown";
        }
        return countryName;
    }
}

