using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace AzureFunctionApp.Functions
{
    /// <summary>
    /// Azure Function that periodically checks country laws and identifies region transfer threats.
    /// Runs every 5 minutes during active  hours to detect and prevent.
    /// Skips execution during night hours (between 1 AM and 7 AM UTC) for efficiency.
    /// </summary>
    public class CheckTheLaws
    {
        private readonly ILogger<CheckTheLaws> _logger;
        private readonly ICheckTheLawsService _lawService;

        /// <summary>
        /// Initializes a new instance of the CheckTheLaws function class.
        /// </summary>
        /// <param name="logger">Logger instance for writing diagnostic messages.</param>
        /// <param name="lawService">Service for checking prices and identifying deals.</param>
        public CheckTheLaws(ILogger<CheckTheLaws> logger, ICheckTheLawsService lawService)
        {
            _logger = logger;
            _lawService = lawService;
        }

        /// <summary>
        /// Executes the price checking function triggered by a timer.
        /// Checks country laws and identifies region transfer threats for dedicated countries.
        /// </summary>
        /// <param name="myTimer">Timer information for the scheduled trigger (runs every 5 minutes).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Function(nameof(CheckTheLaws))]
        public async Task Run(
            [TimerTrigger("30 */5 * * * *")] TimerInfo myTimer)
        {
            if (DateTime.UtcNow.Hour > 1 && DateTime.UtcNow.Hour < 7)
            {
                _logger.LogDebug($"{nameof(CheckTheLaws)}: Skipping execution during night hours: {DateTime.Now}");
                return;
            }

            try
            {
                await _lawService.CheckTheLawsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CheckTheLaws)}: Error {ex.Message}");
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogDebug($"{nameof(CheckTheLaws)}: Next timer schedule: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
