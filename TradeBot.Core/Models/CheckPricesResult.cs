using System;
using System.Collections.Generic;

namespace TradeBot.Core.Models;
public class CheckPricesResult
{
    public bool Success { get; set; }
    public List<string> Messages { get; set; } = new List<string>();
    public int ItemsChecked { get; set; }
    public int DealsFound { get; set; }
    public DateTime CheckedAt { get; set; }
}