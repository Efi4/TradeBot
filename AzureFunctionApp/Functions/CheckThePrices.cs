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

        [Function(nameof(CheckThePrices))]
        public async Task Run(
            [TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            if (DateTime.UtcNow.Hour > 1 && DateTime.UtcNow.Hour < 7)
            {
                _logger.LogDebug($"{nameof(CheckThePrices)}: Skipping execution during night hours: {DateTime.Now}");
                return;
            }

            try
            {
                 var result = await _priceService.CheckPricesAsync();
                _logger.LogInformation($"{nameof(CheckThePrices)}: Items checked: {result.ItemsChecked}, Deals found: {result.DealsFound}");
                _logger.LogInformation($"{nameof(CheckThePrices)}: Price check result: {string.Join(", ", result.Messages)}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CheckThePrices)}: Error {ex.Message}");
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogDebug($"{nameof(CheckThePrices)}: Next timer schedule: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
