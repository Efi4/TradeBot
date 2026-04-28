using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Models;

namespace AzureFunctionApp.Functions
{
    public class HttpTriggerFunction
    {
        private readonly ILogger<HttpTriggerFunction> _logger;
        private readonly ICheckThePricesService _priceService;
        private readonly ICalculateAveragePriceService _averagePriceService;
        public HttpTriggerFunction(
            ILogger<HttpTriggerFunction> logger, 
            ICheckThePricesService priceService,
            ICalculateAveragePriceService averagePriceService)
        {
            _logger = logger;
            _priceService = priceService;
            _averagePriceService = averagePriceService;
        }

        [Function("HttpTriggerFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "hello")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP trigger function processed a request.");

            // CheckPricesResult result = await _priceService.CheckPricesAsync();
            // var averagePriceResult = await _averagePriceService.CalculateAverageWeaponPricesAsync();
            // var averagePriceResult = await _averagePriceService.CalculateAverageArmorPricesAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            // await response.WriteStringAsync(result.ToString());
            return response;
        }
    }
}
