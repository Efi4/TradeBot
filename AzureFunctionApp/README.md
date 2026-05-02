# Azure Function App

The core serverless application for TradeBot, providing timer-triggered and HTTP-triggered Azure Functions for continuous market monitoring.

## Overview

This .NET 8 Azure Functions project implements the runtime for TradeBot's price monitoring and deal detection system. It includes:
- Timer-triggered functions for scheduled price checks
- HTTP-triggered functions for manual operations
- Dependency injection with custom services
- Integration with database and Azure storage
- Structured logging and error handling
- Local development support

## Quick Start

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4+
- Visual Studio Code or Visual Studio 2022

### Running Locally

1. **Build the project:**
```bash
dotnet build
```

2. **Configure local settings:**
   - Copy `local.settings.json.template` to `local.settings.json` (if needed)
   - Update connection strings for your environment

3. **Start the function app:**
```bash
func start
```

The functions will be available at `http://localhost:7071`

## Project Structure

```
AzureFunctionApp/
├── Functions/
│   ├── CheckThePrices.cs              # Timer trigger for price checking
│   ├── CalculateAveragePrices.cs      # Timer trigger for price averaging
│   ├── CheckThePricesService.cs       # Business logic for price checking
│   ├── HttpTriggerFunction.cs         # HTTP endpoint for manual triggers
│   └── TradeDealNotification.cs       # Deal notification handler
├── Program.cs                          # Startup and dependency injection
├── host.json                           # Function runtime configuration
├── appsettings.json                    # Application settings
├── appsettings.Development.json        # Development environment overrides
├── local.settings.json                 # Local development secrets
└── Properties/
    └── launchSettings.json             # Launch configuration
```

## Functions

### CheckThePrices (Timer Trigger)

Monitors marketplace prices at regular intervals to detect deals.

**Schedule:** Configurable via `host.json` CRON expression
- Development: Every 5 minutes: `0 */5 * * * *`
- Production: Every hour: `0 0 * * * *` (customizable)

**Triggers:** 
- `[TimerTrigger("0 */5 * * * *")]` - Executes on schedule
- Supported for `TimerInfo myTimer` parameter

**Service:** `ICheckThePricesService`

**Logs:**
- Information: Price check started/completed
- Warning: Deals found above threshold
- Error: API call failures, database errors

**Example Workflow:**
1. Function triggers on schedule
2. Fetches latest weapon and armor prices from marketplace API
3. Compares against historical averages
4. Identifies items with price reductions
5. Queues detected deals to Azure Service Bus
6. Sends Discord notifications for significant deals

**Location:** [Functions/CheckThePrices.cs](Functions/CheckThePrices.cs)

### CalculateAveragePrices (Timer Trigger)

Computes average prices for weapons and armor based on collected data.

**Schedule:** Daily calculation (e.g., `0 0 1 * * *` - 1 AM UTC)

**Service:** `ICalculateAveragePriceService`

**Responsibility:**
1. Query historical weapon prices from database
2. Group by type and stat ranges
3. Calculate average prices
4. Store aggregated prices for deal detection
5. Log calculation completion

**Location:** [Functions/CalculateAveragePrices.cs](Functions/CalculateAveragePrices.cs)

### HttpTriggerFunction (HTTP Trigger)

Manual HTTP endpoint for testing and on-demand operations.

**Endpoint:** `POST/GET /api/hello`

**Authorization:** Anonymous (can be restricted)

**Request:** Any HTTP GET or POST to the endpoint

**Response:** JSON object with:
- `message`: Response message
- `timestamp`: UTC timestamp of response
- `method`: HTTP method (GET, POST, etc.)
- `path`: Request path

**Example Request:**
```bash
curl -X GET http://localhost:7071/api/hello
```

**Example Response:**
```json
{
  "message": "Hello from TradeBot",
  "timestamp": "2026-05-02T14:30:00Z",
  "method": "GET",
  "path": "/api/hello"
}
```

**Location:** [Functions/HttpTriggerFunction.cs](Functions/HttpTriggerFunction.cs)

### TradeDealNotification (Queue Trigger)

Processes deal notifications from Azure Queue Storage and sends to Discord.

**Trigger:** Message in Azure Queue Storage

**Service:** `IDiscordIntegrationService`

**Flow:**
1. Listen for messages on `trade-deals` queue
2. Parse equipment deal data
3. Format as Discord embed
4. Send to configured Discord webhook
5. Log notification status

**Location:** [Functions/TradeDealNotification.cs](Functions/TradeDealNotification.cs)

## Services

### Built-in Services

All services are registered in [Program.cs](Program.cs) and injected into functions.

#### ICheckThePricesService
**Location:** `TradeBot.Core.Services`
- Monitors marketplace prices
- Detects deals vs. average prices
- Queues notifications
- Returns CheckPricesResult

**Injection:**
```csharp
public CheckThePrices(ICheckThePricesService priceService, ILogger<CheckThePrices> logger)
{
    _priceService = priceService;
    _logger = logger;
}
```

#### ICalculateAveragePriceService
**Location:** `TradeBot.Core.Services`
- Calculates average weapon prices
- Calculates average armor prices
- Uses stat ranges for filtering and optimizing price calculation
- Updates database aggregations

#### IDiscordIntegrationService
**Location:** `TradeBot.Core.Services`
- Formats deal notifications
- Posts to Discord webhooks
- Handles Discord API errors

#### IAzureStorageHelper
**Location:** `TradeBot.Data.Helpers`
- Queue message operations
- Blob storage operations
- Handles Azure Storage authentication

#### TradingDbContext
**Location:** `TradeBot.Data.Contexts`
- Entity Framework Core context
- Database access
- Registered as scoped service

## Configuration

### Program.cs (Startup)

The `Program.cs` file configures:

```csharp
// Load environment variables
EnvironmentConfiguration.LoadEnvironment();

// Register services
services.AddScoped<ICheckThePricesService, CheckThePricesService>();
services.AddHttpClient<ICheckThePricesService, CheckThePricesService>();
services.AddScoped<ICalculateAveragePriceService, CalculateAveragePriceService>();
services.AddScoped<IDiscordIntegrationService, DiscordIntegrationService>();
services.AddHttpClient<IDiscordIntegrationService, DiscordIntegrationService>();
services.AddSingleton<IAzureStorageHelper, AzureStorageHelper>();

// Configure options
services.Configure<RequestDataOptions>(configuration.GetSection("RequestDataOptions"));
services.Configure<StatRangeOptions>(configuration.GetSection("StatRangeOptions"));
services.Configure<DiscordIntegrationOptions>(configuration.GetSection("DiscordIntegrationOptions"));
```

**Location:** [Program.cs](Program.cs)

### appsettings.json

Application settings for all environments:

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
  },
  "StatRangeOptions": {
    "MinWeaponDmg": 50,
    "MaxWeaponDmg": 250,
    "MinWeaponCrit": 5,
    "MaxWeaponCrit": 50,
    "MinArmorDef": 10,
    "MaxArmorDef": 200
  },
  "DiscordIntegrationOptions": {
    "WebHookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
  }
}
```

**Location:** [appsettings.json](appsettings.json)

### appsettings.Development.json

Development-specific overrides (e.g., different API endpoints or Discord testing webhook).

### local.settings.json

Local development secrets and connection strings (NOT committed to git):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SQLAZURECONNSTR_tradingDatabase": "Server=(localdb)\\mssqllocaldb;Database=TradeBot;Trusted_Connection=true;",
    "CUSTOMCONNSTR_tradingStorageAccount": "UseDevelopmentStorage=true"
  }
}
```

### host.json

Azure Functions runtime configuration:

- **extensionBundle**: Functions runtime extensions
- **logging**: Log level configuration
- **functionTimeout**: Max execution time
- **tracing**: Application Insights configuration

**Location:** [host.json](host.json)

## Dependency Injection

All services use constructor injection:

```csharp
public class MyFunction
{
    private readonly ICheckThePricesService _priceService;
    private readonly ILogger<MyFunction> _logger;

    // Services injected through constructor
    public MyFunction(ICheckThePricesService priceService, ILogger<MyFunction> logger)
    {
        _priceService = priceService;
        _logger = logger;
    }

    [Function("MyFunction")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
    {
        try
        {
            var result = await _priceService.CheckPricesAsync();
            _logger.LogInformation($"Check complete: {result.ItemsChecked} items");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in function");
        }
    }
}
```

## Logging

Structured logging with multiple log levels:

```csharp
// Information - Normal operation flow
_logger.LogInformation("Price check started");

// Warning - Unexpected but recoverable
_logger.LogWarning("Deal threshold exceeded for item: {ItemName}", itemName);

// Error - Failures that need attention
_logger.LogError(ex, "Failed to fetch prices from API");

// Debug - Development diagnostics
_logger.LogDebug("Processing weapon: {WeaponType}", weaponType);
```

View logs locally:
```bash
func start  # Logs appear in console output
```

## Deployment

### To Azure

1. **Create Azure resources**
   - Storage account
   - Function App
   - SQL Database
   - Key Vault

2. **Deploy with Azure CLI:**
```bash
# Login to Azure
az login

# Deploy function app
func azure functionapp publish <FunctionAppName>
```
Or use Github workflow.

3. **Or use Visual Studio:**
   - Right-click project > Publish
   - Select Azure Function App
   - Complete the publish wizard

### Configuration in Azure

Set environment variables in Function App configuration:
- `SQLAZURECONNSTR_tradingDatabase` - SQL Database connection
- `CUSTOMCONNSTR_tradingStorageAccount` - Storage connection
- Add to Application Settings in Azure Portal

## Testing

### Running Tests

Unit tests for services:
```bash
dotnet test ../TradeBot.UnitTests
```

Integration tests for functions:
```bash
# Start functions first
func start

# In another terminal
dotnet test ../TradeBot.IntegrationTests
```

### Manual Testing

Use HTTP client to test endpoints:

```bash
# Test HTTP trigger
curl -X POST http://localhost:7071/api/hello -H "Content-Type: application/json"

# Or with PowerShell
Invoke-WebRequest -Uri http://localhost:7071/api/hello -Method POST
```

## Troubleshooting

### Function App Won't Start

**Error:** `The specified extension bundle version x.x.x is not available`

**Solution:**
1. Update Azure Functions Core Tools: `npm install -g azure-functions-core-tools@latest`
2. Update `extensionBundle` version in `host.json`

**Error:** `Connection string missing or invalid`

**Solution:**
1. Verify `local.settings.json` exists
2. Check connection string format
3. Ensure SQL Server/LocalDB is running

### Service Not Found

**Error:** `Unable to resolve service for type...`

**Solution:**
1. Check service is registered in `Program.cs`
2. Verify constructor parameter names
3. Review dependency chain for missing registrations

### Database Connection Errors

**Error:** `Cannot open database "TradeBot"`

**Solution:**
1. Apply migrations: `dotnet ef database update`
2. Verify database exists in SQL Server
3. Check connection string in `local.settings.json`

## Performance Optimization

### Scaling Considerations

- **Timer-triggered functions**: Adjust CRON expression for frequency
- **HTTP-triggered functions**: Auto-scale based on HTTP queue depth
- **Database**: Monitor query performance, add indexes if needed

### Best Practices

- Use async/await for I/O operations
- Cache HTTP client instances
- Batch database operations
- Use connection pooling for databases
- Monitor function execution time

## Monitoring

### Application Insights

Configure in Azure Portal:
1. Enable Application Insights for Function App
2. View telemetry in Azure Portal > Function App > Monitor
3. Set up alerts for failures or performance issues

### Logs

View logs in Azure:
```bash
# Stream logs
az webapp log tail --name <FunctionAppName> --resource-group <ResourceGroup>

# Or in Azure Portal
Function App > App Service logs > Log stream
```

## Project Dependencies

The Function App depends on:
- [TradeBot.Base](../TradeBot.Base/README.md) - Shared models
- [TradeBot.Core](../TradeBot.Core/README.md) - Services
- [TradeBot.Data](../TradeBot.Data/README.md) - Database access
- Microsoft.Azure.Functions.Worker - Azure Functions runtime
- Microsoft.EntityFrameworkCore - ORM

## Related Documentation

- [TradeBot Root README](../Readme.md) - Project overview
- [TradeBot.Core](../TradeBot.Core/README.md) - Services documentation
- [TradeBot.Data](../TradeBot.Data/README.md) - Database documentation
- [DATA_LAYER_README.md](../DATA_LAYER_README.md) - Data layer details
- [MIGRATIONS_GUIDE.md](../MIGRATIONS_GUIDE.md) - Database migrations
- [TradeBot.UnitTests](../TradeBot.UnitTests/README.md) - Unit test examples
- [TradeBot.IntegrationTests](../TradeBot.IntegrationTests/README.md) - Integration test examples

## Support

For issues or questions:
1. Check the above sections for common solutions
2. Review related project READMEs
3. Check Azure Functions documentation: https://learn.microsoft.com/en-us/azure/azure-functions/

The function will be accessible at `http://localhost:7071/`

## Deployment to Azure

1. Create a Function App resource in Azure
2. Deploy using Azure Functions Core Tools:
```bash
func azure functionapp publish <FunctionAppName>
```

Or use Visual Studio publish functionality.

## Configuration

Edit the CRON expression in [TimerTriggerFunction.cs](AzureFunctionApp/TimerTriggerFunction.cs) to change the schedule:
- `0 */5 * * * *` - Every 5 seconds (development, not recommended for production)
- `0 0 * * * *` - Every hour
- `0 0 0 * * *` - Daily at midnight
- `0 0 9 * * MON-FRI` - Weekdays at 9 AM

See [CRON expressions](https://github.com/Azure/azure-functions-host/wiki/Supported-schedule-expressions) for more options.

## NuGet Dependencies

- `Microsoft.Azure.Functions.Worker` - Azure Functions worker runtime
- `Microsoft.Azure.Functions.Worker.Extensions.Timer` - Timer trigger support
- `Microsoft.Azure.Functions.Worker.Sdk` - SDK and analyzers
- `Microsoft.Azure.WebJobs.Extensions.Storage` - Storage support
