using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

public class ItemPriceResponseModel
{
    [JsonPropertyName("item")]
    public required string ItemName { get; set; }

    [JsonPropertyName("price")]
    public decimal Price {get; set;}

    [JsonPropertyName("stats")]
    public required string Stats {get; set;}
}