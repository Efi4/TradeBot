namespace TradeBot.Core.Models;

public class ResponseItemModel
{
    public string ItemCode { get; set; }
    public TradeBot.Core.Models.ItemModel Item { get; set; }
    public int Attack { get; set; }
    public int CriticalChance { get; set; }
    public decimal Price { get; set; }
}