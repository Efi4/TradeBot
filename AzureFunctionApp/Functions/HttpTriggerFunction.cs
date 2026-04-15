using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using TradeBot.Core.Interfaces;

namespace AzureFunctionApp.Functions
{
    public class HttpTriggerFunction
    {
        private readonly ILogger<HttpTriggerFunction> _logger;
        private readonly ICheckTheAvPricesService _avpriceService;
        public HttpTriggerFunction(
            ILogger<HttpTriggerFunction> logger, 
            ICheckTheAvPricesService avpriceService)
        {
            _logger = logger;
            _avpriceService = avpriceService;
        }

        [Function("HttpTriggerFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "hello")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP trigger function processed a request.");

            var result = await _avpriceService.CheckPricesAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}
