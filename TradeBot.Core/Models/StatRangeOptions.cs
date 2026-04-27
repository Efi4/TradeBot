using System.Collections.Generic;
using TradeBot.Base.Objects;

namespace TradeBot.Core.Models;

public class StatRangeOptions
{
   public List<ArmorStatRange> ArmorStatRanges { get; set; }
}

public class ArmorStatRange
{
    public ArmorType Name { get; set; }
    public Dictionary<string, int> Stats { get; set; }
}