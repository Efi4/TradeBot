using System;
using System.Collections.Generic;

namespace TradeBot.Base.Models;

/// <summary>
/// Result object returned after checking market prices for trading opportunities.
/// Contains summary statistics and messages about the price check operation.
/// </summary>
public class CheckPricesResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the price checking operation completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the collection of messages generated during the price check operation.
    /// </summary>
    public List<string> Messages { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the total number of items checked during the operation.
    /// </summary>
    public int ItemsChecked { get; set; }

    /// <summary>
    /// Gets or sets the number of profitable trading deals identified.
    /// </summary>
    public int DealsFound { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the price check was performed.
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Returns a formatted string representation of the check prices result.
    /// </summary>
    /// <returns>A string containing success status, items checked, deals found, timestamp, and messages.</returns>
    public override string ToString()
    {
        return $"Success: {Success},\n" +
               $"ItemsChecked: {ItemsChecked},\n" +
               $"DealsFound: {DealsFound},\n" +
               $"CheckedAt: {CheckedAt:g},\n" +
               $"Messages: [{string.Join(",\n", Messages)}]";
    }
}