using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace TradeBot.Base.Models;

/// <summary>
/// Root response model containing market data from the API.
/// </summary>
public class CountryLawsResponseModel
{
    /// <summary>
    /// Gets or sets the result container with country laws data.
    /// </summary>
    [JsonPropertyName("result")]
    public required CountryDataResultModel CountryResult { get; set; }

    /// <summary>
    /// Gets or sets the result container with country laws data.
    /// </summary>
    [JsonPropertyName("result")]
    public required CountryCongressResultModel CongressResult { get; set; }

    /// <summary>
    /// Gets or sets the result container with country laws data.
    /// </summary>
    [JsonPropertyName("result")]
    public required CountryLawsResultModel LawsResult { get; set; }
}

/// <summary>
/// Container for the data result from the country laws API response.
/// </summary>
public class CountryLawsResultModel
{
    /// <summary>
    /// Gets or sets the data container with items and pagination information.
    /// </summary>
    [JsonPropertyName("data")]
    public required CountryLawsDataContainerModel Data { get; set; }
}

/// <summary>
/// Container for the data result from the country laws API response.
/// </summary>
public class CountryDataResultModel
{
    /// <summary>
    /// Gets or sets the data container with items and pagination information.
    /// </summary>
    [JsonPropertyName("data")]
    public required CountryDataContainerModel Data { get; set; }
}

/// <summary>
/// Container for the data result from the country laws API response.
/// </summary>
public class CountryCongressResultModel
{
    /// <summary>
    /// Gets or sets the data container with items and pagination information.
    /// </summary>
    [JsonPropertyName("data")]
    public required CongressDataContainerModel Data { get; set; }
}