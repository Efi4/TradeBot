using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace AzureFunctionApp.Functions
{
    /// <summary>
    /// Azure Function that periodically checks neighbouring countries' laws and identifies threats.
    /// Runs every 5 minutes during active  hours to detect and prevent.
    /// Skips execution during night hours (between 1 AM and 7 AM UTC) for efficiency.
    /// </summary>
    public class CheckTheNeighboursLaws
    {
        private readonly ILogger<CheckTheNeighboursLaws> _logger;
        private readonly ICheckTheLawsService _lawService;

        /// <summary>
        /// Initializes a new instance of the CheckTheLaws function class.
        /// </summary>
        /// <param name="logger">Logger instance for writing diagnostic messages.</param>
        /// <param name="lawService">Service for checking prices and identifying deals.</param>
        public CheckTheNeighboursLaws(ILogger<CheckTheNeighboursLaws> logger, ICheckTheLawsService lawService)
        {
            _logger = logger;
            _lawService = lawService;
        }

        /// <summary>
        /// Executes the price checking function triggered by a timer.
        /// Checks country laws and identifies threats for dedicated country.
        /// </summary>
        /// <param name="myTimer">Timer information for the scheduled trigger (runs every 5 minutes).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Function(nameof(CheckTheNeighboursLaws))]
        public async Task Run(
            [TimerTrigger("45 */5 * * * *")] TimerInfo myTimer)
        {
            if (DateTime.UtcNow.Hour > 1 && DateTime.UtcNow.Hour < 7)
            {
                _logger.LogDebug($"{nameof(CheckTheNeighboursLaws)}: Skipping execution during night hours: {DateTime.Now}");
                return;
            }

            try
            {
                await _lawService.CheckTheNeighboursLawsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CheckTheNeighboursLaws)}: Error {ex.Message}");
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogDebug($"{nameof(CheckTheNeighboursLaws)}: Next timer schedule: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
