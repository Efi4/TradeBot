using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradeBot.Core.Models;

public class ItemMarketResponseModel
{
    [Required]
    public List<TradeBot.Core.Models.ResponseItemModel> ItemModel { get; set; }
    [Required]
    public string NextCursor { get; set; }
}