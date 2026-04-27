using System;
using System.ComponentModel.DataAnnotations;
using TradeBot.Base.Objects;

namespace TradeBot.Data.Models;

/// <summary>
/// Weapon prices entity representing user weapon holdings
/// </summary>
public class WeaponPrice
{
    [Key]
    public WeaponType Type { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Key]
    public int Attack { get; set; }

    [Key]
    public int Crit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
