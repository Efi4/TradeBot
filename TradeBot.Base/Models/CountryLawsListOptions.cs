using System.Collections.Generic;
using TradeBot.Base.Objects;

namespace TradeBot.Base.Models;

/// <summary>
/// Configuration options for information about the countries which laws are worth checking.
/// </summary>
public class CountryLawsListOptions
{
    /// <summary>
    /// Gets or sets the collection of country ids.
    /// </summary>
    public required List<string> CountriesList { get; set; }

    /// <summary>
    /// Gets or sets the request path dictionary value.
    /// </summary>
    public required string RequestPathHeader { get; set; }
}