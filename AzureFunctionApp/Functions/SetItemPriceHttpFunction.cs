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
    public class SetItemPriceHttpFunction
    {
        private readonly ILogger<SetItemPriceHttpFunction> _logger;
        private readonly ICheckThePricesService _priceService;

        public SetItemPriceHttpFunction(
            ILogger<SetItemPriceHttpFunction> logger, 
            ICheckThePricesService priceService)
        {
            _logger = logger;
            _priceService = priceService;
        }

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
