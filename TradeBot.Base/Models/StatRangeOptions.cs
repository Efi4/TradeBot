using System.Collections.Generic;
using TradeBot.Base.Objects;

namespace TradeBot.Base.Models;

/// <summary>
/// Configuration options for armor stat ranges used in price calculations.
/// </summary>
public class StatRangeOptions
{
    /// <summary>
    /// Gets or sets the collection of stat ranges for each armor type.
    /// </summary>
    public required List<ArmorStatRange> ArmorStatRanges { get; set; }
}

/// <summary>
/// Represents the valid stat range for a specific armor type.
/// </summary>
public class ArmorStatRange
{
    /// <summary>
    /// Gets or sets the armor type this stat range applies to.
    /// </summary>
    public required ArmorType Name { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of stat names and their maximum values for this armor type.
    /// </summary>
    public required Dictionary<string, int> Stats { get; set; }
}