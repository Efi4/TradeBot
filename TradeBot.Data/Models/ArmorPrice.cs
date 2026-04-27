using System;
using System.ComponentModel.DataAnnotations;
using TradeBot.Base.Objects;

namespace TradeBot.Data.Models;

/// <summary>
/// Armor prices entity representing user armor holdings
/// </summary>
public class ArmorPrice
{
    [Key]
    public ArmorType Type { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Key]
    public int Stat { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
