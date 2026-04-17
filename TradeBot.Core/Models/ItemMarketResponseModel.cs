using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace TradeBot.Core.Models;

public class ItemMarketResponseModel
{
    [JsonPropertyName("result")]
    public required ItemMarketResultModel Result { get; set; }
}

public class ItemMarketResultModel
{
    [JsonPropertyName("data")]
    public required ItemMarketDataContainerModel Data { get; set; }
}

public class ItemMarketDataContainerModel
{
    [JsonPropertyName("items")]
    public required List<TradeBot.Core.Models.ResponseWeaponItemModel> ItemsModel { get; set; }
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}