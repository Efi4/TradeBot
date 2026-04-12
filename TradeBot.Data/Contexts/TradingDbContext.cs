using Microsoft.EntityFrameworkCore;
using TradeBot.Data.Models;

namespace TradeBot.Data.Contexts;

/// <summary>
/// Entity Framework Core DbContext for Trading database
/// </summary>
public class TradingDbContext : DbContext
{
    public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options)
    {
    }

    public DbSet<WeaponPrices> WeaponPrices { get; set; } = null!;
    public DbSet<ArmorPrices> ArmorPrices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WeaponPrices table configuration
        modelBuilder.Entity<WeaponPrices>(entity =>
        {
            entity.ToTable("WeaponPrices");
            entity.HasKey(e => e.Type);
            entity.Property(e => e.Price).HasPrecision(10, 5);
            entity.Property(e => e.Attack).IsRequired();
            entity.Property(e => e.Crit).IsRequired();
        });

        // ArmorPrices table configuration
        modelBuilder.Entity<ArmorPrices>(entity =>
        {
            entity.ToTable("ArmorPrices");
            entity.HasKey(e => e.Type);
            entity.Property(e => e.Price).HasPrecision(10, 5);
            entity.Property(e => e.Stat).IsRequired();
        });
    }
}
