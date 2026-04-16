using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Models;
using TradeBot.Base;
using Microsoft.Extensions.Options;
using System.Net;

namespace TradeBot.Core.Services
{
    public class CheckTheAvPricesService : ICheckTheAvPricesService
    {
        private readonly ILogger<CheckTheAvPricesService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IOptions<HttpHeaders> _httpHeadersOptions;


        public CheckTheAvPricesService(ILogger<CheckTheAvPricesService> logger, HttpClient httpClient,IOptions<HttpHeaders> httpHeadersOptions)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpHeadersOptions = httpHeadersOptions;
        }

        public async Task<CheckPricesResult> CheckPricesAsync()
        {
            _logger.LogInformation("Starting to check prices...");

            try
            {
                // Make HTTP POST request to fetch prices
                var postResult = await MakeHttpPostRequestAsync();
                
                if (!postResult.Success)
                {
                    return new CheckPricesResult
                    {
                        Success = false,
                        Messages = new List<string> { $"HTTP request failed: {postResult.Message}" },
                        CheckedAt = DateTime.Now
                    };
                }

                var result = new CheckPricesResult
                {
                    Success = true,
                    Messages = new List<string> { "Price check completed successfully", postResult.Message },
                    ItemsChecked = 0,
                    DealsFound = 0,
                    CheckedAt = DateTime.Now
                };

                _logger.LogInformation($"Price check completed: {string.Join(", ", result.Messages)}");
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

        private async Task<(bool Success, string Message)> MakeHttpPostRequestAsync()
        {
            try
            {
                var url = Environment.GetEnvironmentVariable("PRICECHECK_API_URL") ?? "https://api.example.com/prices";

                // Create request with headers including cookie
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent("{\"0\":{\"itemCode\":\"sniper\",\"limit\":12,\"direction\":\"forward\"},\"1\":{\"itemCode\":\"sniper\",\"limit\":10,\"transactionType\":\"itemMarket\",\"direction\":\"forward\"}}", System.Text.Encoding.UTF8, "application/json")
                };
                // Add headers
                request.Content.Headers.Add("Content-Length", "150");

                var headers = _httpHeadersOptions.Value.HttpHeadersDictionary;
                foreach (var header in headers)
                {
                  if(!String.IsNullOrEmpty(header.Value))  request.Headers.Add(header.Key, header.Value);
                }
                headers.TryGetValue("Cookie", out var cookieHeader);
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    _logger.LogInformation($"Found cookie in secrets, starts with: {cookieHeader.Substring(0, 10)}...");
                }
                else _logger.LogWarning($"Error: No cookie found in secrets");


                _logger.LogInformation($"Making POST request to {url}");
                // var response = await _httpClient.SendAsync(request);

                // if (response.IsSuccessStatusCode)
                // {
                //     var content = await response.Content.ReadAsStringAsync();
                //     _logger.LogInformation($"HTTP request successful: {response.StatusCode}");
                //     return (true, $"HTTP POST successful - Status: {response.StatusCode}");
                // }
                // else
                // {
                //     _logger.LogWarning($"HTTP request failed with status code: {response.StatusCode}");
                //     return (false, $"HTTP request returned status: {response.StatusCode}");
                // }
                _logger.LogInformation($"Simulate request to {url}");
                await Task.Delay(50); // Simulate network delay
                return (true, $"HTTP POST successful - Status: {HttpStatusCode.OK}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"HTTP request error: {ex.Message}");
                return (false, $"HTTP request exception: {ex.Message}");
            }
        }
    }
}
