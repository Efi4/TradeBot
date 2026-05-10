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
    /// HTTP Azure Function for updating the average price of an item.
    /// Accessible via POST /api/prices endpoint with proper authorization.
    /// </summary>
    public class SetItemPriceHttpFunction
    {
        private readonly ILogger<SetItemPriceHttpFunction> _logger;
        private readonly ICheckThePricesService _priceService;

        /// <summary>
        /// Initializes a new instance of the SetItemPriceHttpFunction class.
        /// </summary>
        /// <param name="logger">Logger instance for writing diagnostic messages.</param>
        /// <param name="priceService">Service for updating item prices.</param>
        public SetItemPriceHttpFunction(
            ILogger<SetItemPriceHttpFunction> logger, 
            ICheckThePricesService priceService)
        {
            _logger = logger;
            _priceService = priceService;
        }

        /// <summary>
        /// Processes an HTTP POST request to update the average price of an item.
        /// </summary>
        /// <param name="req">The HTTP request data.</param>
        /// <param name="itemSetPriceRequestModel">The item price update request with item code, stats, and new price.</param>
        /// <returns>An HTTP response indicating success or failure of the price update.</returns>
        [Function(nameof(SetItemPriceHttpFunction))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "prices")] HttpRequestData req, [FromBody] ItemSetPriceRequestModel itemSetPriceRequestModel)
        {
            _logger.LogDebug("HTTP trigger function processed a request.");
            
            var response = req.CreateResponse(HttpStatusCode.OK);

            var isSuccesful = await _priceService.SetItemPriceAsync(itemSetPriceRequestModel);
            if(isSuccesful)
            {  
                await response.WriteStringAsync("Price was set succesfully.");
            }
            else 
            {
                await response.WriteStringAsync("Was not able to set the price!");
            }
            return response;
        }
    }
}
