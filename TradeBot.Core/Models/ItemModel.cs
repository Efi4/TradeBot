using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TradeBot.Core.Models;

public class ItemModel
{
    [JsonPropertyName("code")]
    public required string ItemCode { get; set; }
    public required Dictionary<string, int> Skills { get; set; }
}