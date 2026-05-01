using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace AzureFunctionApp.Functions
{
    public class CalculateAveragePrices
    {
        private readonly ILogger<CalculateAveragePrices> _logger;
        private readonly ICalculateAveragePriceService _priceService;

        public CalculateAveragePrices(ILogger<CalculateAveragePrices> logger, ICalculateAveragePriceService priceService)
        {
            _logger = logger;
            _priceService = priceService;
        }

        [Function(nameof(CalculateAveragePrices))]
        public async Task Run(
            [TimerTrigger("0 0 */2 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"{nameof(CalculateAveragePrices)}: Timer trigger function executed at: {DateTime.Now}");
            
            try
            {
                bool isWeaponPartSuccess = await _priceService.CalculateAverageWeaponPricesAsync();
                bool isArmorPartSuccess = await _priceService.CalculateAverageArmorPricesAsync();
                if(isArmorPartSuccess && isWeaponPartSuccess)
                {
                    _logger.LogInformation($"Average price calculation succeeded.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(CalculateAveragePrices)}: {ex.Message}");
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
