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

public class DiscordIntegrationService : IDiscordIntegrationService
{
    private readonly ILogger<DiscordIntegrationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<DiscordIntegrationOptions> _discordIntegrationOptions;
    public DiscordIntegrationService(ILogger<DiscordIntegrationService> logger, HttpClient httpClient, IOptions<DiscordIntegrationOptions> discordIntegrationOptions)
    {
        _logger = logger;
        _httpClient = httpClient;
        _discordIntegrationOptions = discordIntegrationOptions;
    }
       
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

    private HttpRequestMessage PrepareRequest(EquipmentQueueMessageModel equipmentData)
    {
        var discordChannelPostRequest = new HttpRequestMessage(HttpMethod.Post, _discordIntegrationOptions.Value.WebHookUrl)
        {
            Content = new StringContent("{\"content\": \"Trade deal is available: " +
            $"{Constants.EquipmentLookup.NameMapping[equipmentData.Item.ItemCode]}"+
            $"({string.Join("-",equipmentData.Item.Skills.Values)}),{equipmentData.Price} gold, "+
            $"<t:{new DateTimeOffset(equipmentData.CreatedAt).ToUnixTimeSeconds()}:R>,"+
            $"possible margin {equipmentData.Margin}"+
            "\"}",
            System.Text.Encoding.UTF8, "application/json")
        };
        _logger.LogDebug($"{nameof(DiscordIntegrationService)}: Request has been prepared.");

        return discordChannelPostRequest;
    }

}