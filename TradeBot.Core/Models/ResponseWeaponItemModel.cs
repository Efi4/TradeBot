namespace TradeBot.Core.Models;

public class ResponseWeaponItemModel
{
    public string ItemCode { get; set; }
    public TradeBot.Core.Models.ItemModel Item { get; set; }
    public decimal Price { get; set; }
}