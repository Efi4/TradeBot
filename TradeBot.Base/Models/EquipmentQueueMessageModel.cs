using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

/// <summary>
/// Represents a trade deal message containing equipment details, price, and profit margin.
/// Used for queueing notifications about identified trading opportunities.
/// </summary>
public class EquipmentQueueMessageModel
{
    /// <summary>
    /// Gets or sets the profit margin percentage for this trade deal.
    /// </summary>
    [JsonPropertyName("margin")]
    public decimal Margin {get; set;}

    /// <summary>
    /// Gets or sets the item details including ID, code, and stats.
    /// </summary>
    [JsonPropertyName("item")]
    public required ItemModel Item { get; set; }

    /// <summary>
    /// Gets or sets the price of the equipment at the time of the deal.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the deal was identified.
    /// </summary>
    [JsonPropertyName("CreatedAt")]
    public required DateTime CreatedAt { get; set; }
}