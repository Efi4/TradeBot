using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradeBot.Base;
using TradeBot.Base.Models;
using TradeBot.Core.Interfaces;

namespace TradeBot.Core.Services;

/// <summary>
/// Service for sending messages to Discord channels via webhooks.
/// Handles both trade deal notifications and general system notifications.
/// </summary>
public class DiscordIntegrationService : IDiscordIntegrationService
{
    private readonly ILogger<DiscordIntegrationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<DiscordIntegrationOptions> _discordIntegrationOptions;

    /// <summary>
    /// Initializes a new instance of the DiscordIntegrationService class.
    /// </summary>
    /// <param name="logger">Logger instance for writing diagnostic messages.</param>
    /// <param name="httpClient">HTTP client for sending webhook requests to Discord.</param>
    /// <param name="discordIntegrationOptions">Configuration options containing Discord webhook URLs.</param>
    public DiscordIntegrationService(ILogger<DiscordIntegrationService> logger, HttpClient httpClient, IOptions<DiscordIntegrationOptions> discordIntegrationOptions)
    {
        _logger = logger;
        _httpClient = httpClient;
        _discordIntegrationOptions = discordIntegrationOptions;
    }
       
    /// <summary>
    /// Posts a formatted trade deal notification message to the dedicated equipment channel on Discord.
    /// </summary>
    /// <param name="equipmentData">The equipment queue message model containing item details, price, margin, and timestamp.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The message format includes the item name, stats, price, relative time posted, and profit margin.
    /// Logs a warning if the HTTP request fails but does not throw an exception.
    /// </remarks>
    public async Task PostMessageInDedicatedChannelAsync(EquipmentQueueMessageModel equipmentData)
    {
        var discordChannelPostRequest = PrepareRequest(equipmentData);
        var result = await _httpClient.SendAsync(discordChannelPostRequest);
        if(!result.IsSuccessStatusCode)
        {
            _logger.LogWarning($"{nameof(DiscordIntegrationService)}: Unable to send a message for {Constants.EquipmentLookup.NameMapping[equipmentData.Item.ItemCode]} dedicated discord channel. Reason:{result.ReasonPhrase}");
        }
        _logger.LogDebug($"{nameof(DiscordIntegrationService)}: Message was succesfully sent in dedicated discord channel.");
    }

    /// <summary>
    /// Posts a general notification message to the notifications Discord channel.
    /// </summary>
    /// <param name="message">The plain text message to post to the notifications channel.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method is used for system notifications and alerts.
    /// Logs a warning if the HTTP request fails but does not throw an exception.
    /// </remarks>
    public async Task PostNotificationMessageInDedicatedChannelAsync(string message)
    {
        var discordChannelPostRequest = PrepareNotificationRequest(message, _discordIntegrationOptions.Value.NotificationsWebHookUrl);
        var result = await _httpClient.SendAsync(discordChannelPostRequest);
        if(!result.IsSuccessStatusCode)
        {
            _logger.LogWarning($"{nameof(DiscordIntegrationService)}: Unable to send a message. Reason:{result.ReasonPhrase}");
        }
        _logger.LogDebug($"{nameof(DiscordIntegrationService)}: Message was succesfully sent in dedicated discord channel.");
    }

    /// <summary>
    /// Posts a region transfer notification message to the dedicated Discord channel.
    /// </summary>
    /// <param name="message">The plain text message to post in dedicated channel.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method is used for system notifications and alerts.
    /// Logs a warning if the HTTP request fails but does not throw an exception.
    /// </remarks>
    public async Task PostRegionTransferNotificationMessageInDedicatedChannelAsync(string message)
    {
        var discordChannelPostRequest = PrepareNotificationRequest(message, _discordIntegrationOptions.Value.RegionTransferNotificationWebHookUrl);
        var result = await _httpClient.SendAsync(discordChannelPostRequest);
        if(!result.IsSuccessStatusCode)
        {
            _logger.LogWarning($"{nameof(DiscordIntegrationService)}: Unable to send a message. Reason:{result.ReasonPhrase}");
        }
        _logger.LogDebug($"{nameof(DiscordIntegrationService)}: Message was succesfully sent in dedicated discord channel.");
    }

    private HttpRequestMessage PrepareRequest(EquipmentQueueMessageModel equipmentData)
    {
        var discordChannelPostRequest = new HttpRequestMessage(HttpMethod.Post, _discordIntegrationOptions.Value.WebHookUrl)
        {
            Content = new StringContent("{\"content\": \"" +
            $"{Constants.EquipmentLookup.NameMapping[equipmentData.Item.ItemCode]}"+
            $"({string.Join("-",equipmentData.Item.Skills.Values)}),{1.01m*equipmentData.Price} gold, "+
            $"approx. margin {equipmentData.Margin}"+
            $"<t:{new DateTimeOffset(equipmentData.CreatedAt).ToUnixTimeSeconds()}:R>, "+           
            "\"}",
            System.Text.Encoding.UTF8, "application/json")
        };
        _logger.LogDebug($"{nameof(DiscordIntegrationService)}: Request has been prepared.");

        return discordChannelPostRequest;
    }
    private HttpRequestMessage PrepareNotificationRequest(string message, string notificationsWebHookUrl)
    {
        var discordChannelPostRequest = new HttpRequestMessage(HttpMethod.Post, notificationsWebHookUrl)
        {
            Content = new StringContent("{\"content\": \""+
            $"{message}"+
            "\"}",
            System.Text.Encoding.UTF8, "application/json")
        };
        _logger.LogDebug($"{nameof(DiscordIntegrationService)}: Request has been prepared.");

        return discordChannelPostRequest;
    }

}