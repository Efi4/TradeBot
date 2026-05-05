using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace TradeBot.Base.Models;

public class ItemSetPriceRequestModel
{
    [JsonPropertyName("itemType")]
    public required string ItemCode { get; set; }

    [JsonPropertyName("skills")]
    public required Dictionary<string, int> Skills { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}