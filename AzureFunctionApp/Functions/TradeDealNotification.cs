using TradeBot.Base.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace AzureFunctionApp.Functions;

public class TradeDealNotification
    {
        private readonly ILogger<TradeDealNotification> _logger;

        public TradeDealNotification(ILogger<TradeDealNotification> logger)
        {
            _logger = logger;
        }

        [Function(nameof(TradeDealNotification))]
        public void Run(
            [QueueTrigger("trade-deals", Connection = "AzureWebJobsStorage")] EquipmentResponseModel equipmentDetails)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {equipmentDetails.ItemCode}");
            
        }
    }