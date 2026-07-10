using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace TradeBot.Base.Models;

/// <summary>
/// Root response model containing market data from the API.
/// </summary>
public class CountryResponseModel
{
    /// <summary>
    /// Gets or sets the result container with country laws data.
    /// </summary>
    [JsonPropertyName("result")]
    public required CountryGenericDataResultModel Result { get; set; }
}

/// <summary>
/// Container for the data result from the country laws API response.
/// </summary>
public class CountryGenericDataResultModel
{
    /// <summary>
    /// Gets or sets the data container with items and pagination information.
    /// </summary>
    [JsonPropertyName("data")]
    public required JsonElement Data { get; set; }
}

public class CountryLawsDataModel
{
    [JsonPropertyName("items")]
    public List<LawShortenItemModel> Items { get; set; }

    [JsonPropertyName("nextCursor")]
    public string NextCursor { get; set; }
}