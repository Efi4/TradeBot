using TradeBot.Base.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using System.Threading.Tasks;
using TradeBot.Core.Interfaces;

namespace AzureFunctionApp.Functions;

public class DiscordNotification(ILogger<DiscordNotification> logger, IDiscordIntegrationService discordIntegrationService)
{
    private readonly ILogger<DiscordNotification> _logger = logger;
    private readonly IDiscordIntegrationService _discordIntegrationService = discordIntegrationService;

    [Function(nameof(DiscordNotification))]
    public async Task Run(
        [QueueTrigger("notifications", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogDebug($"C# Queue trigger {nameof(DiscordNotification)} function processed message: {message}");
        await _discordIntegrationService.PostNotificationMessageInDedicatedChannelAsync(message);
    }
}