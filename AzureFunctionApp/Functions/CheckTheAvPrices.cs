using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using AzureFunctionApp.Services;
using AzureFunctionApp.Interfaces;

namespace AzureFunctionApp.Functions
{
    public class CheckTheAvPrices
    {
        private readonly ILogger<CheckTheAvPrices> _logger;
        private readonly ICheckTheAvPricesService _avpriceService;

        public CheckTheAvPrices(ILogger<CheckTheAvPrices> logger, ICheckTheAvPricesService avpriceService)
        {
            _logger = logger;
            _avpriceService = avpriceService;
        }

        [Function("CheckTheAvPrices")]
        public async Task Run(
            [TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"{nameof(CheckTheAvPrices)}: Timer trigger function executed at: {DateTime.Now}");
            
            try
            {
                var result = await _avpriceService.CheckPricesAsync();
                
                _logger.LogInformation($"Price check result: {string.Join(", ", result.Messages)}");
                _logger.LogInformation($"Items checked: {result.ItemsChecked}, Deals found: {result.DealsFound}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CheckTheAvPrices: {ex.Message}");
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
