using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;

namespace AzureFunctionApp
{
    public class TimerTriggerFunction
    {
        private readonly ILogger<TimerTriggerFunction> _logger;

        public TimerTriggerFunction(ILogger<TimerTriggerFunction> logger)
        {
            _logger = logger;
        }

        [Function("TimerTriggerFunction")]
        public void Run(
            [TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
