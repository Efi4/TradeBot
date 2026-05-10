using System.Collections.Generic;

namespace TradeBot.Base.Models;

/// <summary>
/// Configuration options for API requests to the market data service.
/// Contains endpoint URLs and HTTP headers used in market API calls.
/// </summary>
public class RequestDataOptions
{
    /// <summary>
    /// Gets or sets the base URL for the market API.
    /// </summary>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the base URL for batch market API requests.
    /// </summary>
    public required string BaseBatchUrl { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of HTTP headers to include in API requests.
    /// Typically includes User-Agent, Accept, and authentication headers.
    /// </summary>
    public required Dictionary<string, string> HttpHeadersDictionary { get; set; }
}