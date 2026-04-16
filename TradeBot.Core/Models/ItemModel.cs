using System.Collections.Generic;

namespace TradeBot.Core.Models;

public class ItemModel
{
    public string ItemCode { get; set; }
    public Dictionary<string, string> Skills { get; set; }
}