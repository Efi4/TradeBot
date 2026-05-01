using System;
using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

public class EquipmentResponseModel
{
    [JsonPropertyName("itemCode")]
    public required string ItemCode { get; set; }
    [JsonPropertyName("item")]
    public required TradeBot.Base.Models.ItemModel Item { get; set; }
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    [JsonPropertyName("CreatedAt")]
    public required DateTime CreatedAt { get; set; }
}