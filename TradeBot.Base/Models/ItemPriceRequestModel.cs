using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace TradeBot.Base.Models;

public class ItemPriceRequestModel
{
    [JsonPropertyName("itemType")]
    public required string ItemCode { get; set; }

    [JsonPropertyName("skills")]
    public required Dictionary<string, int> Skills { get; set; }
}