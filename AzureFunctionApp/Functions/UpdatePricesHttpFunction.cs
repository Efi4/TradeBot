using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using TradeBot.Core.Interfaces;
using TradeBot.Base.Models;
using System;

namespace AzureFunctionApp.Functions
{
    /// <summary>
    /// HTTP Azure Function for retrieving the current average price of an item.
    /// Accessible via GET /api/prices endpoint with proper authorization.
    /// </summary>
    public class UpdatePricesHttpFunction
    {
        private readonly ILogger<UpdatePricesHttpFunction> _logger;
        private readonly ICheckThePricesService _priceService;
        private readonly ICalculateAveragePriceService _averagePriceService;

        /// <summary>
        /// Initializes a new instance of the UpdatePricesHttpFunction class.
        /// </summary>
        /// <param name="logger">Logger instance for writing diagnostic messages.</param>
        /// <param name="priceService">Service for retrieving item prices.</param>
        public UpdatePricesHttpFunction(
            ILogger<UpdatePricesHttpFunction> logger, 
            ICheckThePricesService priceService,
            ICalculateAveragePriceService averagePriceService)
        {
            _logger = logger;
            _priceService = priceService;
            _averagePriceService = averagePriceService;
        }

        /// <summary>
        /// Processes an HTTP GET request to update the average prices for items.
        /// </summary>
        /// <param name="req">The HTTP request data.</param>
        /// <returns>An HTTP response containing success confirmation or error details.</returns>
        [Function(nameof(UpdatePricesHttpFunction))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bulk-update-prices")] HttpRequestData req)
        {
            _logger.LogDebug("HTTP trigger function processed a request.");
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                await _priceService.CheckPricesAsync(skipProcessing: true);
                var weaponPricesList = await _averagePriceService.CalculateAverageWeaponPricesAsync();
                var armorPricesList = await _averagePriceService.CalculateAverageArmorPricesAsync();
                if(weaponPricesList.Count>0 && armorPricesList.Count>0)
                {
                    _logger.LogInformation($"{nameof(UpdatePricesHttpFunction)}: Average price calculation succeeded. {weaponPricesList.Count + armorPricesList.Count} prices were added/updated.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(UpdatePricesHttpFunction)}: Error {ex.Message}");
                await response.WriteStringAsync($"Updating prices failed with the error {ex.Message}");
            }
            return response;
        }
    }
}
