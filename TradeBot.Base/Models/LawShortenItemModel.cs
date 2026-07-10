using System.Text.Json.Serialization;

namespace TradeBot.Base.Models;

/// <summary>
/// Represents equipment data returned from the market API.
/// Contains item details, price, and posting information.
/// </summary>
public class LawShortenItemModel
{
    /// <summary>
    /// Gets or sets the text description of the law.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the status of the law.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the status of the law.
    /// </summary>
    [JsonPropertyName("votes")]
    public Votes Votes { get; set; }

    /// <summary>
    /// Gets or sets the full item details including ID, code, and stats.
    /// </summary>
    [JsonPropertyName("data")]
    public ShortLawModel Law { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the law was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents law data returned from the country laws API.
/// Contains law details and metadata.
/// </summary>
public class ShortLawModel
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("targetCountry")]
    public string TargetCountry { get; set; }
}

/// <summary>
/// Represents law data details returned from the country laws API.
/// Contains law details and metadata.
/// </summary>
public class Votes
{
    [JsonPropertyName("accept")]
    public List<string> Accepted { get; set; } = [];
    [JsonPropertyName("abstain")]
    public List<string> Abstained { get; set; } = [];
    [JsonPropertyName("reject")]
    public List<string> Rejected { get; set; } = [];
}
