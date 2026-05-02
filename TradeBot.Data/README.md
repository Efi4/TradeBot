# TradeBot.Data

A .NET 8 class library providing data access and persistence for the TradeBot application using Entity Framework Core and Azure SQL Database.

## Overview

`TradeBot.Data` implements the data layer for TradeBot using Entity Framework Core (EF Core). It handles:
- Database context and ORM configuration
- Entity models for weapons, armor, and prices
- Database initialization and connection management
- Azure Storage integration helpers
- Migration support through `TradeBot.Data.Migrations`

## Project Structure

```
TradeBot.Data/
├── Contexts/
│   └── TradingDbContext.cs              # Entity Framework DbContext
├── Models/
│   ├── Weapon.cs                        # Weapon entity
│   ├── Armor.cs                         # Armor entity
│   ├── WeaponPrice.cs                   # Average weapon prices
│   └── ArmorPrice.cs                    # Average armor prices
├── Configuration/
│   ├── ServiceCollectionExtensions.cs   # DI registration
│   └── DatabaseConfiguration.cs         # Database initialization
├── Helpers/
│   └── AzureStorageHelper.cs            # Azure Storage operations
└── TradeBot.Data.csproj
```

## Database Context

### TradingDbContext

The main Entity Framework Core `DbContext` class that represents the database.

**Location**: [Contexts/TradingDbContext.cs](Contexts/TradingDbContext.cs)

**DbSets:**
```csharp
public DbSet<Weapon> Weapons { get; set; }
public DbSet<Armor> Armors { get; set; }
public DbSet<WeaponPrice> WeaponPrices { get; set; }
public DbSet<ArmorPrice> ArmorPrices { get; set; }
```

**Configuration:**
- Uses SQL Server as database provider
- Supports both Azure SQL Database and LocalDB for development
- Command timeout: 30 seconds
- Uses migrations assembly: `TradeBot.Data.Migrations`

## Entity Models

### Weapon Entity

Represents a weapon item in the marketplace.

**Properties:**
```csharp
public int Id { get; set; }
public string Name { get; set; }              // Weapon name
public string Type { get; set; }              // Weapon type (rifle, sniper, etc.)
public string Tier { get; set; }              // Item tier (Common, Rare, Epic, Legendary)
public int AttackDamage { get; set; }         // Damage value
public int CriticalChance { get; set; }       // Crit chance percentage
public decimal CurrentPrice { get; set; }     // Current marketplace price
public decimal AveragePrice { get; set; }     // Historical average price
public DateTime UpdatedAt { get; set; }       // Last price update timestamp
```

### Armor Entity

Represents armor equipment in the marketplace.

**Properties:**
```csharp
public int Id { get; set; }
public string Name { get; set; }              // Armor name
public string Type { get; set; }              // Armor type (helmet, chest, legs, etc.)
public string Tier { get; set; }              // Item tier
public int DefenseValue { get; set; }         // Defense rating
public int ElementalResistance { get; set; }  // Resistance percentage
public decimal CurrentPrice { get; set; }     // Current marketplace price
public decimal AveragePrice { get; set; }     // Historical average price
public DateTime UpdatedAt { get; set; }       // Last price update timestamp
```

### WeaponPrice Entity

Aggregated average price data for weapons by stat ranges.

**Properties:**
```csharp
public int Id { get; set; }
public string Type { get; set; }              // Weapon type
public string Tier { get; set; }              // Item tier
public int MinAttackDamage { get; set; }      // Damage range start
public int MaxAttackDamage { get; set; }      // Damage range end
public int MinCriticalChance { get; set; }    // Crit range start
public int MaxCriticalChance { get; set; }    // Crit range end
public decimal AveragePrice { get; set; }     // Calculated average price
public int SampleSize { get; set; }           // Number of items in average
public DateTime CalculatedAt { get; set; }    // Calculation timestamp
```

### ArmorPrice Entity

Aggregated average price data for armor by stat ranges.

**Properties:**
```csharp
public int Id { get; set; }
public string Type { get; set; }              // Armor type
public string Tier { get; set; }              // Item tier
public int MinDefenseValue { get; set; }      // Defense range start
public int MaxDefenseValue { get; set; }      // Defense range end
public int MinElementalResistance { get; set; }    // Resistance range start
public int MaxElementalResistance { get; set; }    // Resistance range end
public decimal AveragePrice { get; set; }     // Calculated average price
public int SampleSize { get; set; }           // Number of items in average
public DateTime CalculatedAt { get; set; }    // Calculation timestamp
```

## Configuration

### Service Registration

Register the database in your dependency injection container:

```csharp
using TradeBot.Data.Configuration;
using Microsoft.Extensions.DependencyInjection;

var connectionString = "Server=(localdb)\\mssqllocaldb;Database=TradeBot;Trusted_Connection=true;";
var services = new ServiceCollection();

// Register the database context
services.AddTradingDatabase(connectionString);

var serviceProvider = services.BuildServiceProvider();

// Initialize database and apply migrations
await DatabaseConfiguration.InitializeDatabaseAsync(serviceProvider);
```

**Location of extensions:** [Configuration/ServiceCollectionExtensions.cs](Configuration/ServiceCollectionExtensions.cs)

### Connection Strings

#### Development (LocalDB)
```
Server=(localdb)\mssqllocaldb;Database=TradeBot;Trusted_Connection=true;
```

#### Azure SQL Database
```
Server=tcp:<server>.database.windows.net,1433;Initial Catalog=TradeBot;
Persist Security Info=False;User ID=<username>;Password=<password>;
MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;
Connection Timeout=30;
```

#### Environment Variables
Set these in `.env` or Azure configuration:
```
SQLAZURECONNSTR_tradingDatabase=<your-connection-string>
```

### Database Initialization

The `DatabaseConfiguration.InitializeDatabaseAsync()` method:
1. Creates the database if it doesn't exist
2. Applies all pending migrations
3. Seeds initial data (if configured)

```csharp
await DatabaseConfiguration.InitializeDatabaseAsync(serviceProvider);
```

**Location**: [Configuration/DatabaseConfiguration.cs](Configuration/DatabaseConfiguration.cs)

## Usage Examples

### Querying Data

```csharp
using TradeBot.Data.Contexts;

public class WeaponService
{
    private readonly TradingDbContext _context;

    public WeaponService(TradingDbContext context)
    {
        _context = context;
    }

    // Get all weapons of a specific type
    public async Task<List<Weapon>> GetWeaponsByType(string weaponType)
    {
        return await _context.Weapons
            .Where(w => w.Type == weaponType)
            .OrderByDescending(w => w.CurrentPrice)
            .ToListAsync();
    }

    // Get weapons under average price
    public async Task<List<Weapon>> GetUnderpricedWeapons(decimal discountThreshold = 0.9m)
    {
        return await _context.Weapons
            .Where(w => w.CurrentPrice < w.AveragePrice * discountThreshold)
            .OrderBy(w => (w.CurrentPrice / w.AveragePrice))
            .ToListAsync();
    }

    // Get average prices for a weapon type and tier
    public async Task<WeaponPrice?> GetAveragePriceByTypeAndTier(string type, string tier)
    {
        return await _context.WeaponPrices
            .Where(p => p.Type == type && p.Tier == tier)
            .FirstOrDefaultAsync();
    }
}
```

### Adding/Updating Data

```csharp
public class WeaponRepository
{
    private readonly TradingDbContext _context;

    public WeaponRepository(TradingDbContext context)
    {
        _context = context;
    }

    // Add a new weapon
    public async Task AddWeaponAsync(Weapon weapon)
    {
        _context.Weapons.Add(weapon);
        await _context.SaveChangesAsync();
    }

    // Update weapon prices
    public async Task UpdateWeaponPriceAsync(int weaponId, decimal newPrice)
    {
        var weapon = await _context.Weapons.FindAsync(weaponId);
        if (weapon != null)
        {
            weapon.CurrentPrice = newPrice;
            weapon.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    // Bulk insert weapons
    public async Task BulkInsertWeaponsAsync(List<Weapon> weapons)
    {
        _context.Weapons.AddRange(weapons);
        await _context.SaveChangesAsync();
    }
}
```

### Transactional Operations

```csharp
public class DealProcessor
{
    private readonly TradingDbContext _context;

    public DealProcessor(TradingDbContext context)
    {
        _context = context;
    }

    // Process a deal within a transaction
    public async Task ProcessDealAsync(int weaponId, decimal dealPrice)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var weapon = await _context.Weapons.FindAsync(weaponId);
                if (weapon != null)
                {
                    weapon.CurrentPrice = dealPrice;
                    weapon.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
```

## Azure Storage Integration

### AzureStorageHelper

Helper class for Azure Queue and Blob storage operations.

**Location**: [Helpers/AzureStorageHelper.cs](Helpers/AzureStorageHelper.cs)

**Key Methods:**
- `PushMessageToQueueAsync()`: Send message to Azure Queue
- `GetBlobAsync()`: Retrieve blob from storage
- `StoreBlobAsync()`: Store blob in storage

**Usage:**
```csharp
public class DealNotificationService
{
    private readonly IAzureStorageHelper _storageHelper;

    public DealNotificationService(IAzureStorageHelper storageHelper)
    {
        _storageHelper = storageHelper;
    }

    public async Task QueueDealNotificationAsync(EquipmentQueueMessageModel deal)
    {
        await _storageHelper.PushMessageToQueueAsync(
            Constants.AzureStorageConfiguration.TradeDealsQueueName,
            deal
        );
    }
}
```

## Migrations

Database schema changes are managed through Entity Framework Core migrations.

See [MIGRATIONS_GUIDE.md](../MIGRATIONS_GUIDE.md) for detailed instructions on:
- Creating new migrations
- Applying migrations
- Removing migrations
- Troubleshooting migration issues

**Related Project:** [TradeBot.Data.Migrations](../TradeBot.Data.Migrations/README.md)

## Dependencies

- **.NET 8**: Target framework
- **Microsoft.EntityFrameworkCore** (8.0.26): ORM framework
- **Microsoft.EntityFrameworkCore.SqlServer** (8.0.26): SQL Server provider
- **Microsoft.EntityFrameworkCore.Tools** (8.0.26): Migration tools
- **EFCore.BulkExtensions** (8.1.3): Bulk operations support
- **Azure.Storage.Blobs** (12.18.0): Blob storage client
- **Azure.Storage.Queues** (12.25.0): Queue storage client
- **TradeBot.Base**: Shared models and constants

## Performance Considerations

### Indexes

Database indexes should be created on frequently queried columns:
- `Weapon.Type` and `Weapon.Tier` for filtering
- `WeaponPrice.Type` and `WeaponPrice.Tier` for aggregations
- `UpdatedAt` columns for recent items

### Bulk Operations

Use `EFCore.BulkExtensions` for large data operations:
```csharp
await _context.Weapons.BulkInsertAsync(largeWeaponList);
await _context.Weapons.BulkUpdateAsync(updatedWeapons);
```

### Query Optimization

- Use `.AsNoTracking()` for read-only queries
- Use `.Select()` to project only needed properties
- Consider pagination for large result sets

## Troubleshooting

### Connection String Issues
- Verify SQL Server is running
- Check credentials for Azure SQL Database
- Ensure `.env` file is in the solution root

### Migration Errors
- Ensure `TradeBot.Data.Migrations` project exists
- Run `dotnet ef migrations list` to check migration status
- Review migration files for conflicts

### Performance Issues
- Check database indexes
- Monitor query execution times
- Consider denormalization for read-heavy scenarios

## Related Projects

- [TradeBot.Base](../TradeBot.Base/README.md) - Shared models
- [TradeBot.Core](../TradeBot.Core/README.md) - Services using this data layer
- [TradeBot.Data.Migrations](../TradeBot.Data.Migrations/README.md) - Migration management
- [AzureFunctionApp](../AzureFunctionApp/README.md) - Azure Functions using this data layer

## See Also

- [Root README](../Readme.md) - Project overview
- [DATA_LAYER_README.md](../DATA_LAYER_README.md) - Additional data layer documentation
- [MIGRATIONS_GUIDE.md](../MIGRATIONS_GUIDE.md) - Database migration guide
