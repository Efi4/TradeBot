using System;
using System.ComponentModel.DataAnnotations;
using TradeBot.Base.Objects;

namespace TradeBot.Data.Models;

/// <summary>
/// Weapon entity representing individual weapon records with a unique identifier
/// </summary>
public class Weapon
{
    [Key]
    public int Id { get; set; }

    [Required]
    public WeaponType Type { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Attack { get; set; }

    [Required]
    public int Crit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
