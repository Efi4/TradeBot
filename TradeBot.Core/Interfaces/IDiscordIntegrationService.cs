using System.Threading.Tasks;
using TradeBot.Base.Models;

namespace TradeBot.Core.Interfaces
{
    /// <summary>
    /// Service for sending messages to Discord channels via webhooks.
    /// Handles both trade deal notifications and general notification messages.
    /// </summary>
    public interface IDiscordIntegrationService
    {
        /// <summary>
        /// Posts a formatted trade deal notification message to the dedicated equipment channel on Discord.
        /// </summary>
        /// <param name="equipmentData">
        /// The equipment queue message model containing item details, price, margin, and timestamp.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// The message format includes the item name, stats, price, time posted, and profit margin.
        /// Logs a warning if the HTTP request fails but does not throw an exception.
        /// </remarks>
        Task PostMessageInDedicatedChannelAsync(EquipmentQueueMessageModel equipmentData);

        /// <summary>
        /// Posts a general notification message to the notifications Discord channel.
        /// </summary>
        /// <param name="message">
        /// The plain text message to post to the notifications channel.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method is used for system notifications and alerts.
        /// Logs a warning if the HTTP request fails but does not throw an exception.
        /// </remarks>
        Task PostNotificationMessageInDedicatedChannelAsync(string message);
    }
}
