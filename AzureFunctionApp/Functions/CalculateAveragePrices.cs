using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using System.Threading.Tasks;
using System;
using TradeBot.Data.Models;

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
            [TimerTrigger("0 2 */1 * * *")] TimerInfo myTimer)
        {
            _logger.LogDebug($"{nameof(CalculateAveragePrices)}: Timer trigger function executed at: {DateTime.Now}");
            
            try
            {
                var weaponPricesList = await _priceService.CalculateAverageWeaponPricesAsync();
                var armorPricesList = await _priceService.CalculateAverageArmorPricesAsync();
                if(weaponPricesList.Count>0 && armorPricesList.Count>0)
                {
                    _logger.LogInformation($"Average price calculation succeeded. {weaponPricesList.Count + armorPricesList.Count} prices were added/updated.");
                    
                    _logger.LogDebug($"New batch of weapon prices ({weaponPricesList.Count} items):");
                    foreach (var weapon in weaponPricesList)
                    {
                        _logger.LogDebug($" {weapon.Type}({weapon.Crit}-{weapon.Attack}) costs {weapon.Price} in reasonable average.");
                    }
                    
                    _logger.LogDebug($"New batch of armor items prices ({armorPricesList.Count} items):");
                    foreach (var armor in armorPricesList)
                    {
                        _logger.LogDebug($" {armor.Type}({armor.Stat}) costs {armor.Price} in reasonable average.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(CalculateAveragePrices)}: {ex.Message}");
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogDebug($"Next timer schedule: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
