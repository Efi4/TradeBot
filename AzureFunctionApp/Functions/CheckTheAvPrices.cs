using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace AzureFunctionApp.Functions
{
    public class CheckTheAvPrices
    {
        private readonly ILogger<CheckTheAvPrices> _logger;
        private readonly ICheckThePricesService _priceService;

        public CheckTheAvPrices(ILogger<CheckTheAvPrices> logger, ICheckThePricesService avpriceService)
        {
            _logger = logger;
            _priceService = avpriceService;
        }

        [Function("CheckTheAvPrices")]
        public async Task Run(
            [TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"{nameof(CheckTheAvPrices)}: Timer trigger function executed at: {DateTime.Now}");
            
            try
            {
                var result = await _priceService.CheckWeaponPricesAsync();
                
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
