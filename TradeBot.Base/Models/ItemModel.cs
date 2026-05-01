using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

public class ItemModel
{
    [JsonPropertyName("code")]
    public required string ItemCode { get; set; }
    [JsonPropertyName("Skills")]
    public required Dictionary<string, int> Skills { get; set; }
}