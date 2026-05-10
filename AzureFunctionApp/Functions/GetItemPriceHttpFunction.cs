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
    public class GetItemPriceHttpFunction
    {
        private readonly ILogger<GetItemPriceHttpFunction> _logger;
        private readonly ICheckThePricesService _priceService;

        /// <summary>
        /// Initializes a new instance of the GetItemPriceHttpFunction class.
        /// </summary>
        /// <param name="logger">Logger instance for writing diagnostic messages.</param>
        /// <param name="priceService">Service for retrieving item prices.</param>
        public GetItemPriceHttpFunction(
            ILogger<GetItemPriceHttpFunction> logger, 
            ICheckThePricesService priceService)
        {
            _logger = logger;
            _priceService = priceService;
        }

        /// <summary>
        /// Processes an HTTP GET request to retrieve the average price of an item.
        /// </summary>
        /// <param name="req">The HTTP request data.</param>
        /// <param name="itemTypePriceRequestModel">The item price request with item code and stats.</param>
        /// <returns>An HTTP response containing the item price information or error details.</returns>
        [Function(nameof(GetItemPriceHttpFunction))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "prices")] HttpRequestData req, [FromBody] ItemPriceRequestModel itemTypePriceRequestModel)
        {
            _logger.LogDebug("HTTP trigger function processed a request.");
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
               ItemPriceResponseModel itemPriceResponse = await _priceService.GetItemPriceAsync(itemTypePriceRequestModel);
               await response.WriteAsJsonAsync(itemPriceResponse);
            }
            catch(Exception ex)
            {
                await response.WriteStringAsync($"Failed to process: {ex.Message}");   
            }
            return response;
        }
    }
}
