using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

/// <summary>
/// Represents equipment data returned from the market API.
/// Contains item details, price, and posting information.
/// </summary>
public class ItemResponseModel
{
    /// <summary>
    /// Gets or sets the item code identifying the type of equipment.
    /// </summary>
    [JsonPropertyName("itemCode")]
    public required string ItemCode { get; set; }

    /// <summary>
    /// Gets or sets the full item details including ID, code, and stats.
    /// </summary>
    [JsonPropertyName("item")]
    public required ItemModel Item { get; set; }

    /// <summary>
    /// Gets or sets the asking price for this equipment listing.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the equipment listing was created.
    /// </summary>
    [JsonPropertyName("CreatedAt")]
    public required DateTime CreatedAt { get; set; }
}