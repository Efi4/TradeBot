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
public class DiscordNotification(ILogger<DiscordNotification> logger, IDiscordIntegrationService discordIntegrationService)
{
    private readonly ILogger<DiscordNotification> _logger = logger;
    private readonly IDiscordIntegrationService _discordIntegrationService = discordIntegrationService;

    /// <summary>
    /// Executes when a message is added to the notifications queue.
    /// Sends the message to the Discord notifications channel.
    /// </summary>
    /// <param name="message">The notification message from the queue to post to Discord.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Function(nameof(DiscordNotification))]
    public async Task Run(
        [QueueTrigger("notifications", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogDebug($"C# Queue trigger {nameof(DiscordNotification)} function processed message: {message}");
        await _discordIntegrationService.PostNotificationMessageInDedicatedChannelAsync(message);
    }
}