using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

public class ItemPriceResponseModel
{
    [JsonPropertyName("itemType")]
    public required string ItemCode { get; set; }

    [JsonPropertyName("price")]
    public decimal Price {get; set;}

    [JsonPropertyName("stats")]
    public string Stats {get; set;}
}