# TradeBot Data Layer Documentation

## Overview

The data layer provides storage functionality for the TradeBot application, including:
- SQL Azure Database integration using Entity Framework Core
- Azure Storage Account support
- Database initialization and migration management

## Architecture

### Projects

#### TradeBot.Data
**Class Library** - Core data access layer
- **DbContext**: `TradingDbContext` - Entity Framework Core context
- **Models**:
  - `Trade`: Represents trading transactions
  - `Portfolio`: Represents portfolio holdings
- **Configuration**:
  - `DatabaseConfiguration`: DI registration and database initialization
  - `EnvironmentConfiguration`: Environment variable loading from .env

#### TradeBot.Data.Migrations  
**Class Library** - Database migration management
- `TradingDbContextFactory`: Design-time DbContext factory for EF Core tools
- Contains auto-generated migration files

### Models

#### Trade Entity
Represents individual trading transactions.

```csharp
public class Trade
{
    public int Id { get; set; }
    public string Symbol { get; set; }          // Stock symbol (e.g., "AAPL")
    public decimal Price { get; set; }          // Trade price
    public int Quantity { get; set; }           // Number of shares
    public DateTime TradeDate { get; set; }     // When the trade occurred
    public string TradeType { get; set; }       // "Buy" or "Sell"
    public decimal TotalAmount { get; set; }    // Calculated: Price * Quantity
    public DateTime CreatedAt { get; set; }     // Record creation timestamp
    public DateTime? UpdatedAt { get; set; }    // Last update timestamp
}
```

#### Portfolio Entity
Represents current holdings in a portfolio.

```csharp
public class Portfolio
{
    public int Id { get; set; }
    public string PortfolioName { get; set; }   // Portfolio identifier
    public string Symbol { get; set; }          // Stock symbol
    public int Quantity { get; set; }           // Shares owned
    public decimal AverageCost { get; set; }    // Average purchase price
    public decimal CurrentPrice { get; set; }   // Current market price
    public decimal TotalValue { get; set; }     // Calculated: Quantity * CurrentPrice
    public decimal UnrealizedGain { get; set; } // Calculated: TotalValue - Cost
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

## Configuration

### Environment Variables (.env)

Create a `.env` file in the solution root:

```env
# Required: Azure SQL Database Connection String
TRADING_DB_CONNECTION_STRING=Server=(localdb)\mssqllocaldb;Database=TradeBot;Trusted_Connection=true;

# Required: Azure Storage Connection String
AZURE_STORAGE_CONNECTION_STRING=UseDevelopmentStorage=true

# Optional
AZURE_STORAGE_ACCOUNT_NAME=
AZURE_STORAGE_ACCOUNT_KEY=
ENVIRONMENT=Development
LOG_LEVEL=Information
```

#### Connection String Examples

**Local Development (SQLLocalDB)**:
```
Server=(localdb)\mssqllocaldb;Database=TradeBot;Trusted_Connection=true;
```

**Azure SQL Database**:
```
Server=tcp:<server>.database.windows.net,1433;Initial Catalog=TradeBot;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**Azure Storage Emulator**:
```
UseDevelopmentStorage=true
```

**Azure Storage Account**:
```
DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net
```

### Dependency Injection

Register the database in your `Program.cs`:

```csharp
using TradeBot.Data.Configuration;

// Load environment variables
EnvironmentConfiguration.LoadEnvironment();

var services = new ServiceCollection();

// Register database
var connectionString = EnvironmentConfiguration.GetTradingDatabaseConnectionString();
services.AddTradingDatabase(connectionString);

// Initialize database (applies pending migrations)
var serviceProvider = services.BuildServiceProvider();
await DatabaseConfiguration.InitializeDatabaseAsync(serviceProvider);
```

## Usage Examples

### Querying Data

```csharp
using TradeBot.Data.Contexts;

public class TradeService
{
    private readonly TradingDbContext _context;

    public TradeService(TradingDbContext context)
    {
        _context = context;
    }

    // Get all trades for a symbol
    public async Task<List<Trade>> GetTradesBySymbol(string symbol)
    {
        return await _context.Trades
            .Where(t => t.Symbol == symbol)
            .OrderByDescending(t => t.TradeDate)
            .ToListAsync();
    }

    // Get portfolio holdings
    public async Task<List<Portfolio>> GetPortfolioHoldings(string portfolioName)
    {
        return await _context.Portfolios
            .Where(p => p.PortfolioName == portfolioName)
            .ToListAsync();
    }
}
```

### Creating Records

```csharp
// Add a new trade
var trade = new Trade
{
    Symbol = "AAPL",
    Price = 150.00m,
    Quantity = 100,
    TradeDate = DateTime.UtcNow,
    TradeType = "Buy"
};

_context.Trades.Add(trade);
await _context.SaveChangesAsync();
```

### Updating Records

```csharp
var portfolio = await _context.Portfolios
    .FirstOrDefaultAsync(p => p.Symbol == "AAPL");

if (portfolio != null)
{
    portfolio.CurrentPrice = 155.00m;
    portfolio.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
}
```

## Database Features

- **Automatic Timestamps**: All entities have `CreatedAt` and `UpdatedAt` fields
- **Calculated Properties**: `Trade.TotalAmount`, `Portfolio.TotalValue`, `Portfolio.UnrealizedGain`
- **Indexes**: Optimized queries for symbol and date filters
- **Connection Resilience**: Automatic retry with exponential backoff for SQL Server
- **Precision**: Decimal fields use 18,2 precision for accurate financial calculations

## Migrations

See [MIGRATIONS_GUIDE.md](./MIGRATIONS_GUIDE.md) for detailed migration instructions.

Common migration commands:

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project TradeBot.Data.Migrations

# Apply pending migrations
dotnet ef database update --project TradeBot.Data.Migrations

# View migration history
dotnet ef migrations list --project TradeBot.Data.Migrations
```

## Azure Storage Integration

To use Azure Storage Account for blob operations:

```csharp
using Azure.Storage.Blobs;
using TradeBot.Data.Configuration;

var connectionString = EnvironmentConfiguration.GetAzureStorageConnectionString();
var blobClient = new BlobContainerClient(
    new Uri("https://<account>.blob.core.windows.net/<container>"), 
    new DefaultAzureCredential());
```

## Best Practices

1. **Always use async methods**: Use `ToListAsync()`, `FirstOrDefaultAsync()`, etc.
2. **Dispose context properly**: Use dependency injection for automatic disposal
3. **Validate connection strings**: Check `.env` is configured before running
4. **Use transactions for multiple operations**: Ensure data consistency
5. **Apply migrations at startup**: Let `InitializeDatabaseAsync()` handle initialization
6. **Index frequently queried columns**: Keep indexes updated in migrations

## Troubleshooting

### Database Connection Issues
- Verify `.env` file exists in project root
- Ensure connection string is correctly formatted
- For Azure SQL, verify firewall rules allow your IP
- Check user credentials and permissions

### Migration Errors
- Run `dotnet clean && dotnet build` before migrations
- Ensure both projects are up to date: `dotnet restore`
- For conflicts, remove and recreate migrations

### EF Core Issues
- Update NuGet packages: `dotnet add package Microsoft.EntityFrameworkCore` 
- Verify DbContext is registered in DI container
- Check that design-time factory implements `IDesignTimeDbContextFactory<TContext>`

## Related Documentation
- [MIGRATIONS_GUIDE.md](./MIGRATIONS_GUIDE.md) - Detailed migration instructions
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [Azure SQL Database](https://learn.microsoft.com/en-us/azure/azure-sql/)
