namespace TradeBot.Base.Models;

/// <summary>
/// Configuration options for Discord webhook integration.
/// Contains webhook URLs for sending trade notifications and general messages.
/// </summary>
public class DiscordIntegrationOptions
{
    /// <summary>
    /// Gets or sets the webhook URL for posting trade deal notifications to the dedicated channel.
    /// </summary>
    public required string WebHookUrl { get; set; }

    /// <summary>
    /// Gets or sets the webhook URL for posting general system notifications to the notifications channel.
    /// </summary>
    public required string NotificationsWebHookUrl { get; set; }
}