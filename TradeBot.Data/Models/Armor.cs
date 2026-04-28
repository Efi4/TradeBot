using System;
using System.ComponentModel.DataAnnotations;
using TradeBot.Base.Objects;

namespace TradeBot.Data.Models;

/// <summary>
/// Weapon entity representing individual armor records with a unique identifier
/// </summary>
public class Armor
{
    [Key]
    public int Id { get; set; }

    [Required]
    public ArmorType Type { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Stat { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
