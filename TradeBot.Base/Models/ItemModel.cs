using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

/// <summary>
/// Represents an equipment item with its identifier, item code, and stat information.
/// </summary>
public class ItemModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the item.
    /// </summary>
    [JsonPropertyName("_id")]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the item code that identifies the type of equipment (e.g., weapon name or armor type).
    /// </summary>
    [JsonPropertyName("code")]
    public required string ItemCode { get; set; }
    
    /// <summary>
    /// Gets or sets the dictionary of item stats (e.g., attack/crit for weapons, defense for armor).
    /// Key represents stat name, value represents stat value.
    /// </summary>
    [JsonPropertyName("Skills")]
    public required Dictionary<string, int> Skills { get; set; }
}