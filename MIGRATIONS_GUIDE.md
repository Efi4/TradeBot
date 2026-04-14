# Database Migration Guide

This document explains how to create and apply database migrations for the TradeBot project.

## Prerequisites

- .NET 8 SDK installed
- SQL Server or LocalDB (for development)
- Entity Framework Core CLI tools installed

### Install EF Core Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

## Environment Configuration

Before running migrations, ensure your `.env` file is configured with the correct database connection string:

```env
TRADING_DB_CONNECTION_STRING=Server=(localdb)\mssqllocaldb;Database=TradeBot;Trusted_Connection=true;
```

### For Azure SQL Database:
```env
TRADING_DB_CONNECTION_STRING=Server=tcp:<server>.database.windows.net,1433;Initial Catalog=TradeBot;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Creating Migrations

To create a new migration after modifying any models in `TradeBot.Data/Models`:

1. Navigate to the dedicated migration project root directory (to avoid using parameters) or root repo directory:
```bash
cd /mnt/c/Work/TradeBot.Data.Migrations
```

2. Create a migration with a descriptive name:
```bash
dotnet ef migrations add InitialCreate --project TradeBot.Data.Migrations --startup-project TradeBot.Data.Migrations
```

Other examples:
```bash
dotnet ef migrations add AddTradeTable --project TradeBot.Data.Migrations --startup-project TradeBot.Data.Migrations
dotnet ef migrations add AddPortfolioIndexes --project TradeBot.Data.Migrations --startup-project TradeBot.Data.Migrations
```

## Applying Migrations

### Automatically (during application startup)

Migrations are automatically applied when the application starts through the `DatabaseConfiguration.InitializeDatabaseAsync()` method called in `Program.cs`.

### Manually via CLI

To apply migrations manually:

```bash
dotnet ef database update --project TradeBot.Data.Migrations --startup-project TradeBot.Data.Migrations
```

To update to a specific migration:
```bash
dotnet ef database update <MigrationName> --project TradeBot.Data.Migrations --startup-project TradeBot.Data.Migrations
```

## Viewing Migrations

To see the migration history:
```bash
dotnet ef migrations list --project TradeBot.Data.Migrations --startup-project TradeBot.Data.Migrations
```

## Removing Migrations

To remove the last migration (not yet applied):
```bash
dotnet ef migrations remove --project TradeBot.Data.Migrations --startup-project TradeBot.Data.Migrations
```

## Project Structure

- **TradeBot.Data**: Contains DbContext, models, and database configuration
  - `Contexts/TradingDbContext.cs`: EF Core DbContext
  - `Models/Trade.cs`: Trade entity
  - `Models/Portfolio.cs`: Portfolio entity
  - `Configuration/DatabaseConfiguration.cs`: DI registration and initialization

- **TradeBot.Data.Migrations**: Handles database schema migrations
  - `TradingDbContextFactory.cs`: Design-time context factory for migrations
  - `Migrations/`: Auto-generated migration files

## Troubleshooting

### Connection String Issues
- Ensure `.env` file is in the solution root
- Verify connection string format is correct
- For LocalDB, ensure SQL Server Express or SQL Server is installed

### Migration Conflicts
If you encounter migration conflicts:
1. Check for duplicate pending migrations
2. Use `dotnet ef migrations remove` to remove unsynced migrations
3. Recreate migrations with proper naming

### Build Errors
- Ensure all NuGet packages are restored: `dotnet restore`
- Verify .NET 8 SDK is installed: `dotnet --version`
- Clean build: `dotnet clean && dotnet build`

## Database Initialization Script

If needed, you can manually initialize the database using the `DatabaseConfiguration.InitializeDatabaseAsync()` method:

```csharp
var services = new ServiceCollection();
var connectionString = EnvironmentConfiguration.GetTradingDatabaseConnectionString();
services.AddTradingDatabase(connectionString);
var serviceProvider = services.BuildServiceProvider();
await DatabaseConfiguration.InitializeDatabaseAsync(serviceProvider);
```

## References

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
