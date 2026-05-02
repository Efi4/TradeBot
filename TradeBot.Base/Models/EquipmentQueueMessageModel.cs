using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

public class EquipmentQueueMessageModel
{
    [JsonPropertyName("margin")]
    public decimal Margin {get; set;}
    [JsonPropertyName("item")]
    public required ItemModel Item { get; set; }
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    [JsonPropertyName("CreatedAt")]
    public required DateTime CreatedAt { get; set; }
}