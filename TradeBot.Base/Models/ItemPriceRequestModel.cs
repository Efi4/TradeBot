using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace TradeBot.Base.Models;

/// <summary>
/// Request model for retrieving the current price of an item based on its code and stats.
/// </summary>
public class ItemPriceRequestModel
{
    /// <summary>
    /// Gets or sets the item code identifying the type of equipment to get the price for.
    /// </summary>
    [JsonPropertyName("itemType")]
    public required string ItemCode { get; set; }

    /// <summary>
    /// Gets or sets the item stats used to identify the specific price entry.
    /// For weapons: contains attack and crit stats. For armor: contains defense stat.
    /// </summary>
    [JsonPropertyName("skills")]
    public required Dictionary<string, int> Skills { get; set; }
}