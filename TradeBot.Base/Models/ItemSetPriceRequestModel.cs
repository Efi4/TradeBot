using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace TradeBot.Base.Models;

/// <summary>
/// Request model for updating the average price of an item in the database.
/// </summary>
public class ItemSetPriceRequestModel
{
    /// <summary>
    /// Gets or sets the item code identifying the type of equipment to update the price for.
    /// </summary>
    [JsonPropertyName("itemType")]
    public required string ItemCode { get; set; }

    /// <summary>
    /// Gets or sets the item stats used to identify the specific price entry to update.
    /// For weapons: contains attack and crit stats. For armor: contains defense stat.
    /// </summary>
    [JsonPropertyName("skills")]
    public required Dictionary<string, int> Skills { get; set; }

    /// <summary>
    /// Gets or sets the new average price to set for the item.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}