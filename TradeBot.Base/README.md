# TradeBot.Base

A foundational .NET 8 class library that provides shared models, constants, configuration, and utilities used across the TradeBot solution.

## Overview

`TradeBot.Base` serves as the core foundation for the entire TradeBot application. It contains:
- Shared data models used by all projects
- Application constants and configuration
- Environment variable management
- Equipment type definitions and enumerations

## Project Structure

```
TradeBot.Base/
├── Constants.cs                 # Application-wide constants
├── Configuration/
│   └── EnvironmentConfiguration.cs      # Environment variable loading and management
├── Models/
│   ├── CheckPricesResult.cs             # Price checking operation result
│   ├── DiscordIntegrationOptions.cs     # Discord webhook configuration
│   ├── EquipmentQueueMessageModel.cs    # Queue message for equipment deals
│   ├── EquipmentResponseModel.cs        # Equipment API response model
│   ├── ItemMarketResponseModel.cs       # Market item response model
│   ├── ItemModel.cs                     # Individual item data
│   ├── RequestDataOptions.cs            # HTTP request configuration
│   └── StatRangeOptions.cs              # Equipment stat range thresholds
└── Objects/
    └── EquipmentTypes.cs                # Equipment type enumerations
```

## Key Components

### Constants

The `Constants` static class provides application-wide constants including:

- **Environment Variables**: Names for Azure Storage and SQL Database connection strings
- **AppSettings**: Configuration section names for dependency injection
- **Azure Storage**: Queue and blob container names
- **Weapon/Armor Stats**: Min/max stat ranges for different equipment tiers
- **API Headers**: HTTP request header alternatives

**Location**: [Constants.cs](Constants.cs)

### Models

#### CheckPricesResult
Represents the result of a price checking operation.

- `ItemsChecked`: Number of items scanned
- `DealsFound`: Number of deals discovered
- `Success`: Operation success status
- `CheckedAt`: Timestamp of check

#### EquipmentQueueMessageModel
Represents a deal notification queued for processing.

- Equipment details (name, type, tier)
- Price information
- Notification metadata

#### RequestDataOptions
HTTP request configuration loaded from `appsettings.json`:

- `BaseUrl`: Primary API endpoint
- `BaseBatchUrl`: Batch API endpoint
- `HttpHeadersDictionary`: HTTP headers for requests

#### StatRangeOptions
Equipment stat thresholds for deal detection:

- Min/max values for weapons: attack, critical chance
- Min/max values for armor: defense, elemental resistance

#### DiscordIntegrationOptions
Discord webhook configuration:

- `WebHookUrl`: Discord channel webhook URL for notifications

### Environment Configuration

The `EnvironmentConfiguration` static class handles loading and accessing environment variables:

```csharp
// Load environment variables from .env file
EnvironmentConfiguration.LoadEnvironment();

// Get connection strings
var sqlConnection = EnvironmentConfiguration.GetTradingDatabaseConnectionString();
var storageConnection = EnvironmentConfiguration.GetAzureStorageConnectionString();
```

**Location**: [Configuration/EnvironmentConfiguration.cs](Configuration/EnvironmentConfiguration.cs)

#### Supported Environment Variables

- `SQLAZURECONNSTR_tradingDatabase`: Azure SQL Database connection string
- `CUSTOMCONNSTR_tradingStorageAccount`: Azure Storage connection string
- `ConnectionStrings__tradingDatabase`: Local SQL connection string
- Any custom variables loaded from `.env` file

### Equipment Types

Enumeration of equipment types and tiers used throughout the application:

- **Weapon Types**: Rifles, Snipers, Pistols, Shotguns, etc.
- **Armor Types**: Helmets, Chest, Legs, Gloves, etc.
- **Item Tiers**: Common, Uncommon, Rare, Epic, Legendary

**Location**: [Objects/EquipmentTypes.cs](Objects/EquipmentTypes.cs)

## Usage

### Adding to Your Project

Add `TradeBot.Base` as a project reference:

```bash
dotnet add reference ../TradeBot.Base/TradeBot.Base.csproj
```

Or in the `.csproj` file:

```xml
<ItemGroup>
    <ProjectReference Include="..\TradeBot.Base\TradeBot.Base.csproj" />
</ItemGroup>
```

### Using Models

```csharp
using TradeBot.Base.Models;

// Use configuration models
var requestConfig = options.Value;
var baseUrl = requestConfig.BaseUrl;

// Use result models
CheckPricesResult result = new()
{
    ItemsChecked = 100,
    DealsFound = 5,
    Success = true,
    CheckedAt = DateTime.UtcNow
};
```

### Loading Environment Variables

```csharp
using TradeBot.Base.Configuration;

// In Program.cs or startup code
EnvironmentConfiguration.LoadEnvironment();
var connectionString = EnvironmentConfiguration.GetTradingDatabaseConnectionString();
```

## Dependencies

- **.NET 8**: Target framework
- **DotNetEnv** (v2.5.0): For loading `.env` files

## Configuration in appsettings.json

Reference the models by their section names:

```json
{
  "RequestDataOptions": {
    "BaseUrl": "https://api.example.com",
    "BaseBatchUrl": "https://api.example.com/batch",
    "HttpHeadersDictionary": {
      "User-Agent": "TradeBot/1.0"
    }
  },
  "StatRangeOptions": {
    "MinWeaponDmg": 50,
    "MaxWeaponDmg": 200,
    "MinArmorDef": 10,
    "MaxArmorDef": 100
  },
  "DiscordIntegrationOptions": {
    "WebHookUrl": "https://discord.com/api/webhooks/..."
  }
}
```

## Related Projects

- [TradeBot.Core](../TradeBot.Core/README.md) - Services that use these models
- [TradeBot.Data](../TradeBot.Data/README.md) - Database models built on top of these definitions
- [AzureFunctionApp](../AzureFunctionApp/README.md) - Azure Functions that consume these models

## See Also

- [Root README](../Readme.md) - Project overview
- [Data Layer Documentation](../DATA_LAYER_README.md) - Database setup
