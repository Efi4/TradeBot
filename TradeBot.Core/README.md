# TradeBot.Core

A .NET 8 class library containing the core business logic and service implementations for market monitoring, price analysis, and Discord notifications.

## Overview

`TradeBot.Core` implements the main business services that power TradeBot's functionality:
- Market price monitoring and analysis
- Deal detection and comparison
- Average price calculations
- Discord webhook integration for notifications
- Azure Storage integration for data persistence

## Project Structure

```
TradeBot.Core/
├── Interfaces/
│   ├── ICalculateAveragePriceService.cs    # Average price calculation contract
│   ├── ICheckThePricesService.cs           # Price checking contract
│   └── IDiscordIntegrationService.cs       # Discord notifications contract
└── Services/
    ├── CalculateAveragePriceService.cs     # Average price calculation implementation
    ├── CheckThePricesService.cs            # Price checking implementation
    └── DiscordIntegrationService.cs        # Discord webhook implementation
```

## Key Services

### ICheckThePricesService / CheckThePricesService

Primary service for monitoring marketplace prices and detecting deals.

**Features:**
- Fetches real-time weapon and armor prices from marketplace APIs
- Compares prices against historical averages
- Identifies items with significant price reductions
- Queues detected deals for Discord notifications
- Stores price data in the trading database

**Key Methods:**
- `CheckPricesAsync()` - Main entry point; monitors all equipment and returns results

**Returns:**
```csharp
public class CheckPricesResult
{
    public int ItemsChecked { get; set; }    // Total items scanned
    public int DealsFound { get; set; }      // Number of deals detected
    public bool Success { get; set; }        // Operation success status
    public DateTime CheckedAt { get; set; }  // Check timestamp
}
```

**Dependencies:**
- `HttpClient`: Market API communication
- `TradingDbContext`: Database access
- `IAzureStorageHelper`: Queue operations
- `IOptions<RequestDataOptions>`: API configuration
- `ILogger<CheckThePricesService>`: Structured logging

**Location**: [Services/CheckThePricesService.cs](Services/CheckThePricesService.cs)

**Usage Example:**
```csharp
public class MyFunction
{
    private readonly ICheckThePricesService _priceService;

    public MyFunction(ICheckThePricesService priceService)
    {
        _priceService = priceService;
    }

    public async Task RunAsync()
    {
        var result = await _priceService.CheckPricesAsync();
        Console.WriteLine($"Checked {result.ItemsChecked} items, found {result.DealsFound} deals");
    }
}
```

### ICalculateAveragePriceService / CalculateAveragePriceService

Computes and stores average prices for weapons and armor based on database records.

**Features:**
- Calculates weapon average prices by stat ranges
- Calculates armor average prices by stat ranges
- Uses configurable stat thresholds
- Updates historical averages for deal detection

**Key Methods:**
- `CalculateAverageWeaponPricesAsync()` - Calculate and store weapon averages
- `CalculateAverageArmorPricesAsync()` - Calculate and store armor averages

**Returns:** `Task<bool>` - Success status

**Dependencies:**
- `TradingDbContext`: Database access
- `IOptions<StatRangeOptions>`: Stat configuration
- `ILogger<CalculateAveragePriceService>`: Structured logging

**Location**: [Services/CalculateAveragePriceService.cs](Services/CalculateAveragePriceService.cs)

**Usage Example:**
```csharp
public class PriceCalculationFunction
{
    private readonly ICalculateAveragePriceService _priceCalc;

    public PriceCalculationFunction(ICalculateAveragePriceService priceCalc)
    {
        _priceCalc = priceCalc;
    }

    public async Task RunAsync()
    {
        var weaponSuccess = await _priceCalc.CalculateAverageWeaponPricesAsync();
        var armorSuccess = await _priceCalc.CalculateAverageArmorPricesAsync();
        
        if (weaponSuccess && armorSuccess)
            Console.WriteLine("Price calculations completed successfully");
    }
}
```

### IDiscordIntegrationService / DiscordIntegrationService

Sends formatted deal notifications to Discord channels via webhooks.

**Features:**
- Formats equipment deals as Discord embeds
- Sends messages to configured Discord webhook
- Includes deal details: name, tier, price, discount percentage
- Error handling and logging

**Key Methods:**
- `PostMessageInDedicatedChannelAsync(EquipmentQueueMessageModel equipmentData)` - Send deal notification

**Dependencies:**
- `HttpClient`: Discord API communication
- `IOptions<DiscordIntegrationOptions>`: Webhook configuration
- `ILogger<DiscordIntegrationService>`: Structured logging

**Location**: [Services/DiscordIntegrationService.cs](Services/DiscordIntegrationService.cs)

**Usage Example:**
```csharp
public class DealNotifier
{
    private readonly IDiscordIntegrationService _discord;

    public DealNotifier(IDiscordIntegrationService discord)
    {
        _discord = discord;
    }

    public async Task NotifyDealAsync(EquipmentQueueMessageModel deal)
    {
        await _discord.PostMessageInDedicatedChannelAsync(deal);
    }
}
```

## Service Registration

Register all services in your dependency injection container (typically in `Program.cs`):

```csharp
using TradeBot.Core.Interfaces;
using TradeBot.Core.Services;

var services = new ServiceCollection();

// Register services with appropriate lifetimes
services.AddScoped<ICheckThePricesService, CheckThePricesService>();
services.AddHttpClient<ICheckThePricesService, CheckThePricesService>();

services.AddScoped<ICalculateAveragePriceService, CalculateAveragePriceService>();

services.AddScoped<IDiscordIntegrationService, DiscordIntegrationService>();
services.AddHttpClient<IDiscordIntegrationService, DiscordIntegrationService>();

// Configure service options
services.Configure<RequestDataOptions>(configuration.GetSection("RequestDataOptions"));
services.Configure<StatRangeOptions>(configuration.GetSection("StatRangeOptions"));
services.Configure<DiscordIntegrationOptions>(configuration.GetSection("DiscordIntegrationOptions"));
```

## Configuration

### RequestDataOptions (appsettings.json)

```json
{
  "RequestDataOptions": {
    "BaseUrl": "https://api.marketplace.example.com",
    "BaseBatchUrl": "https://api.marketplace.example.com/batch",
    "HttpHeadersDictionary": {
      "User-Agent": "TradeBot/1.0",
      "Accept": "application/json",
      "Accept-Encoding": "gzip, deflate"
    }
  }
}
```

### StatRangeOptions (appsettings.json)

```json
{
  "StatRangeOptions": {
    "MinWeaponDmg": 50,
    "MaxWeaponDmg": 250,
    "MinWeaponCrit": 5,
    "MaxWeaponCrit": 50,
    "MinArmorDef": 10,
    "MaxArmorDef": 200,
    "MinArmorResist": 0,
    "MaxArmorResist": 100
  }
}
```

### DiscordIntegrationOptions (appsettings.json)

```json
{
  "DiscordIntegrationOptions": {
    "WebHookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
  }
}
```

## Dependencies

- **.NET 8**: Target framework
- **Microsoft.Extensions.Logging.Abstractions**: Logging interfaces
- **Microsoft.Extensions.Options**: Configuration binding
- **TradeBot.Base**: Shared models and constants
- **TradeBot.Data**: Database context and models

## Data Models

### Core Models (from TradeBot.Base)

- `CheckPricesResult`: Price checking operation result
- `EquipmentQueueMessageModel`: Deal notification message
- `ItemResponseModel`: API response structure
- `ItemMarketResponseModel`: Market item details
- `RequestDataOptions`: HTTP configuration
- `StatRangeOptions`: Equipment stat ranges
- `DiscordIntegrationOptions`: Discord webhook settings

### Database Models (from TradeBot.Data)

- `Weapon`: Weapon price records
- `Armor`: Armor price records
- `WeaponPrice`: Average weapon prices
- `ArmorPrice`: Average armor prices

## Error Handling

All services implement comprehensive error handling:

- **Logging**: Detailed error logging at appropriate levels
- **Graceful Degradation**: Services continue operating even if individual operations fail
- **Result Objects**: Methods return status indicators along with data
- **Exception Safety**: Async operations handle cancellations and timeouts

Example:
```csharp
try
{
    var result = await _priceService.CheckPricesAsync();
    if (result.Success)
    {
        Console.WriteLine($"Successfully checked {result.ItemsChecked} items");
    }
    else
    {
        Console.WriteLine("Price check failed - check logs for details");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Testing

This project is designed for testability:

- Services use dependency injection for mocking
- Interfaces allow for mock implementations
- Logging can be captured in tests

See [TradeBot.UnitTests](../TradeBot.UnitTests/README.md) for unit test examples.

## Related Projects

- [TradeBot.Base](../TradeBot.Base/README.md) - Shared models and constants
- [TradeBot.Data](../TradeBot.Data/README.md) - Database context and models
- [AzureFunctionApp](../AzureFunctionApp/README.md) - Azure Functions using these services
- [TradeBot.UnitTests](../TradeBot.UnitTests/README.md) - Unit tests for these services

## See Also

- [Root README](../Readme.md) - Project overview
- [Data Layer Documentation](../DATA_LAYER_README.md) - Database architecture
