using System;
using System.Text.Json.Serialization;

namespace TradeBot.Core.Models;

public class EquipmentResponseModel
{
    public required string ItemCode { get; set; }
    public required TradeBot.Core.Models.ItemModel Item { get; set; }
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    public required DateTime CreatedAt { get; set; }
}