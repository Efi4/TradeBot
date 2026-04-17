namespace TradeBot.Core.Objects;

public class WeaponObject
{
    public WeaponType Type { get; set; }
    public decimal Price { get; set; }
    public int Attack { get; set; }
    public int Crit { get; set; }
    public override string ToString() 
    {
        return $"Weapon: {Type}, Price: {Price}, Attack: {Attack}, Crit: {Crit}";
    }
}