using TradeBot.Base.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using System.Threading.Tasks;
using TradeBot.Core.Interfaces;

namespace AzureFunctionApp.Functions;

/// <summary>
/// Azure Function that processes identified trade deals from the queue and sends them to Discord.
/// Triggered when a trade opportunity message appears in the trade-deals queue.
/// </summary>
public class TradeDealNotification(ILogger<TradeDealNotification> logger, IDiscordIntegrationService discordIntegrationService)
{
    private readonly ILogger<TradeDealNotification> _logger = logger;
    private readonly IDiscordIntegrationService _discordIntegrationService = discordIntegrationService;

    /// <summary>
    /// Executes when a trade deal message is added to the trade-deals queue.
    /// Sends the formatted deal information to the Discord trade deals channel.
    /// </summary>
    /// <param name="equipmentDetails">The equipment details of the identified trade opportunity.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Function(nameof(TradeDealNotification))]
    public async Task Run(
        [QueueTrigger("trade-deals", Connection = "AzureWebJobsStorage")] EquipmentQueueMessageModel equipmentDetails)
    {
        _logger.LogDebug($"C# Queue trigger function processed: {equipmentDetails.Item.ItemCode}");
        await _discordIntegrationService.PostMessageInDedicatedChannelAsync(equipmentDetails);
        await Task.Delay(1000); // to prevent throttling
    }
}