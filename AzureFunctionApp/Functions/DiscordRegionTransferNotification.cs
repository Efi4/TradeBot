using TradeBot.Base.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using System.Threading.Tasks;
using TradeBot.Core.Interfaces;

namespace AzureFunctionApp.Functions;

/// <summary>
/// Azure Function that processes notifications from the queue and sends them to Discord.
/// Triggered when a message appears in the notifications queue.
/// </summary>
public class DiscordRegionTransferNotification(ILogger<DiscordRegionTransferNotification> logger, IDiscordIntegrationService discordIntegrationService)
{
    private readonly ILogger<DiscordRegionTransferNotification> _logger = logger;
    private readonly IDiscordIntegrationService _discordIntegrationService = discordIntegrationService;

    /// <summary>
    /// Executes when a message is added to the region transfer notification queue.
    /// Sends the message to the dedicated Discord channel.
    /// </summary>
    /// <param name="message">The notification message from the queue to post to Discord.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Function(nameof(DiscordRegionTransferNotification))]
    public async Task Run(
        [QueueTrigger("region-transfer-notifications", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogDebug($"C# Queue trigger {nameof(DiscordNotification)} function processed message: {message}");
        await _discordIntegrationService.PostRegionTransferNotificationMessageInDedicatedChannelAsync(message);
    }
}