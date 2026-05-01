using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace AzureFunctionApp.Functions
{
    public class CheckThePrices
    {
        private readonly ILogger<CheckThePrices> _logger;
        private readonly ICheckThePricesService _priceService;

        public CheckThePrices(ILogger<CheckThePrices> logger, ICheckThePricesService avpriceService)
        {
            _logger = logger;
            _priceService = avpriceService;
        }

        [Function("CheckTheAvPrices")]
        public async Task Run(
            [TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"{nameof(CheckThePrices)}: Timer trigger function executed at: {DateTime.Now}");
            
            try
            {
                 var result = await _priceService.CheckPricesAsync();
                
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
