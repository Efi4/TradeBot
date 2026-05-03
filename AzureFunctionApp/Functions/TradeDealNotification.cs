using TradeBot.Base.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using System.Threading.Tasks;
using TradeBot.Core.Interfaces;

namespace AzureFunctionApp.Functions;

public class TradeDealNotification(ILogger<TradeDealNotification> logger, IDiscordIntegrationService discordIntegrationService)
{
    private readonly ILogger<TradeDealNotification> _logger = logger;
    private readonly IDiscordIntegrationService _discordIntegrationService = discordIntegrationService;

    [Function(nameof(TradeDealNotification))]
    public async Task Run(
        [QueueTrigger("trade-deals", Connection = "AzureWebJobsStorage")] EquipmentQueueMessageModel equipmentDetails)
    {
        _logger.LogDebug($"C# Queue trigger function processed: {equipmentDetails.Item.ItemCode}");
        await _discordIntegrationService.PostMessageInDedicatedChannelAsync(equipmentDetails);
    }
}