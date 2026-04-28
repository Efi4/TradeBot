using System.Collections.Generic;
using TradeBot.Base.Objects;

namespace TradeBot.Core.Models;

public class StatRangeOptions
{
   public required List<ArmorStatRange> ArmorStatRanges { get; set; }
}

public class ArmorStatRange
{
    public required ArmorType Name { get; set; }
    public required Dictionary<string, int> Stats { get; set; }
}