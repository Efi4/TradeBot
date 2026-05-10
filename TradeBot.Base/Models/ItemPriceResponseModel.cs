using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

/// <summary>
/// Response model containing the price information for a requested item.
/// </summary>
public class ItemPriceResponseModel
{
    /// <summary>
    /// Gets or sets the display name of the item.
    /// </summary>
    [JsonPropertyName("item")]
    public required string ItemName { get; set; }

    /// <summary>
    /// Gets or sets the average price for the item with the specified stats.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price {get; set;}

    /// <summary>
    /// Gets or sets the formatted stats description for the item (e.g., "(100-50)" for weapon with attack 100, crit 50).
    /// </summary>
    [JsonPropertyName("stats")]
    public required string Stats {get; set;}
}