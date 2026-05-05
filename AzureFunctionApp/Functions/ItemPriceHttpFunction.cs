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
    public class ItemPriceHttpFunction
    {
        private readonly ILogger<ItemPriceHttpFunction> _logger;
        private readonly ICheckThePricesService _priceService;

        public ItemPriceHttpFunction(
            ILogger<ItemPriceHttpFunction> logger, 
            ICheckThePricesService priceService)
        {
            _logger = logger;
            _priceService = priceService;
        }

        [Function(nameof(ItemPriceHttpFunction))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "prices")] HttpRequestData req, [FromBody] ItemPriceRequestModel itemTypePriceRequestModel)
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
