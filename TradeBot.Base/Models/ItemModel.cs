using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

public class ItemModel
{
    [JsonPropertyName("_id")]
    public required string Id { get; set; }

    [JsonPropertyName("code")]
    public required string ItemCode { get; set; }
    
    [JsonPropertyName("Skills")]
    public required Dictionary<string, int> Skills { get; set; }
}