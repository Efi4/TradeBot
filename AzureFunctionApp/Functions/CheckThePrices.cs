using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace AzureFunctionApp.Functions
{
    /// <summary>
    /// Azure Function that periodically checks market prices and identifies trading opportunities.
    /// Runs every 5 minutes during active trading hours to detect profitable deals.
    /// Skips execution during night hours (between 1 AM and 7 AM UTC) for efficiency.
    /// </summary>
    public class CheckThePrices
    {
        private readonly ILogger<CheckThePrices> _logger;
        private readonly ICheckThePricesService _priceService;

        /// <summary>
        /// Initializes a new instance of the CheckThePrices function class.
        /// </summary>
        /// <param name="logger">Logger instance for writing diagnostic messages.</param>
        /// <param name="avpriceService">Service for checking prices and identifying deals.</param>
        public CheckThePrices(ILogger<CheckThePrices> logger, ICheckThePricesService avpriceService)
        {
            _logger = logger;
            _priceService = avpriceService;
        }

        /// <summary>
        /// Executes the price checking function triggered by a timer.
        /// Checks current market prices against average prices to identify profitable trading opportunities.
        /// </summary>
        /// <param name="myTimer">Timer information for the scheduled trigger (runs every 5 minutes).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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
