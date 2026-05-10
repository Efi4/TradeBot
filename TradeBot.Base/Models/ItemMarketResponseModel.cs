using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace TradeBot.Base.Models;

/// <summary>
/// Root response model containing market data from the API.
/// </summary>
public class ItemMarketResponseModel
{
    /// <summary>
    /// Gets or sets the result container with market data.
    /// </summary>
    [JsonPropertyName("result")]
    public required ItemMarketResultModel Result { get; set; }
}

/// <summary>
/// Container for the data result from the market API response.
/// </summary>
public class ItemMarketResultModel
{
    /// <summary>
    /// Gets or sets the data container with items and pagination information.
    /// </summary>
    [JsonPropertyName("data")]
    public required ItemMarketDataContainerModel Data { get; set; }
}

/// <summary>
/// Container for market data including equipment items and pagination cursor.
/// </summary>
public class ItemMarketDataContainerModel
{
    /// <summary>
    /// Gets or sets the list of equipment items from the market.
    /// </summary>
    [JsonPropertyName("items")]
    public required List<Models.EquipmentResponseModel> ItemsModel { get; set; }

    /// <summary>
    /// Gets or sets the cursor for pagination to retrieve the next batch of items.
    /// </summary>
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}