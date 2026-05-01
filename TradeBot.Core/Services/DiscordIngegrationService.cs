using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TradeBot.Base.Models;
using TradeBot.Core.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Humanizer;
using TradeBot.Base;

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
       
    public async Task<bool> PostMessageInDedicatedChannelAsync(EquipmentResponseModel equipmentData)
    {
        var discordChannelPostRequest = PrepareRequest(equipmentData);
        var result = await _httpClient.SendAsync(discordChannelPostRequest);
        if(result.IsSuccessStatusCode)
        {
            _logger.LogDebug("Message was succesfully sent in dedicated discord channel.");
            return true;
        }
        return false;
    }

    private HttpRequestMessage PrepareRequest(EquipmentResponseModel equipmentData)
    {
        var discordChannelPostRequest = new HttpRequestMessage(HttpMethod.Post, _discordIntegrationOptions.Value.WebHookUrl)
        {
            Content = new StringContent("{\"content\": \"Trade deal is available: " +
            $"{Constants.EquipmentLookup.Mapping[equipmentData.ItemCode]}, price is {equipmentData.Price} gold, <t:{new DateTimeOffset(equipmentData.CreatedAt).ToUnixTimeSeconds()}:R>"+
            "\"}",
            System.Text.Encoding.UTF8, "application/json")
        };
        return discordChannelPostRequest;
    }

}