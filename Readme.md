# TradeBot

TradeBot is an automated market scanning application designed to continuously monitor various marketplaces for price fluctuations. It alerts users in real-time whenever an item is detected at a significantly reduced price, helping users capitalize on deals and save money.

## Examples

### WarEra Game Examples

TradeBot can be configured to monitor specific games like WarEra. Here are some example alerts:

- **Rare Item Alert**: A Rare item in WarEra is now priced at 500 gold, down from 1500 gold (67% discount).
- **Common Item Alert**: A Common item available for 20 gold, compared to usual 50 gold.
- **Epic Item Alert**: An Epic item has been listed at a bargain price of 300 gold.
- **Legendary Item Alert**: A Legendary item price dropped from 800 gold to 400 gold in the last hour.
- **Rare Item Alert**: A Rare item has been undercut by a competitor selling for 10% less.

## Solution Architecture

TradeBot is organized into several projects, each with specific responsibilities:

```
TradeBot/
├── TradeBot.Base/                   # Shared models, constants, and configuration
├── TradeBot.Core/                   # Business logic services and interfaces
├── TradeBot.Data/                   # Entity Framework Core data access layer
├── TradeBot.Data.Migrations/        # Database migration management
├── AzureFunctionApp/                # Azure Functions for market monitoring
├── TradeBot.UnitTests/              # Unit tests for services
├── TradeBot.IntegrationTests/       # Integration tests for Azure Functions
└── TradeBot.sln                     # Solution file
```

## Projects Overview

### [TradeBot.Base](TradeBot.Base/README.md)
Foundational library containing shared models, constants, and configuration.

**Includes:**
- Application-wide constants
- Shared data models (`CheckPricesResult`, `EquipmentQueueMessageModel`, etc.)
- Environment configuration loader
- Equipment type enumerations

### [TradeBot.Core](TradeBot.Core/README.md)
Business logic services for market monitoring and notifications.

**Key Services:**
- `ICheckThePricesService` - Monitors marketplace prices and detects deals
- `ICalculateAveragePriceService` - Calculates average prices for comparison
- `IDiscordIntegrationService` - Sends deal notifications to Discord

### [TradeBot.Data](TradeBot.Data/README.md)
Data access layer using Entity Framework Core.

**Includes:**
- `TradingDbContext` - EF Core database context
- Entity models (`Weapon`, `Armor`, `WeaponPrice`, `ArmorPrice`)
- Database configuration and initialization
- Azure Storage integration helpers

### [TradeBot.Data.Migrations](TradeBot.Data.Migrations/README.md)
Database migration management for schema updates.

**See:** [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md) for detailed migration instructions.

### [AzureFunctionApp](AzureFunctionApp/README.md)
.NET 8 Azure Function App with timer and HTTP triggers for market monitoring.

**Key Features:**
- Timer-triggered price checking (configurable schedule)
- HTTP-triggered endpoints for manual operations
- Dependency injection for services
- Integrated logging and error handling
- Local development support with Azure Functions Core Tools

### [TradeBot.UnitTests](TradeBot.UnitTests/README.md)
Comprehensive unit tests for services and business logic.

**Coverage:**
- `CheckThePricesService` tests
- `CalculateAveragePriceService` tests
- `DiscordIntegrationService` tests
- Service mocking and assertions examples

### [TradeBot.IntegrationTests](TradeBot.IntegrationTests/README.md)
Integration tests for Azure Functions and end-to-end workflows.

**Includes:**
- HTTP trigger function tests
- Database integration tests
- External API mock testing

## Getting Started

### Prerequisites
- **.NET 8 SDK** or later
- **Visual Studio Code** or **Visual Studio 2022**
- **Azure Functions Core Tools** (v4.0.6127 or later)
- **Docker** (optional, for local Azure Emulator)

### Setup Instructions

1. **Clone and open the solution:**
```bash
git clone <repository-url>
cd TradeBot
code .  # or open with Visual Studio
```

2. **Restore dependencies:**
```bash
dotnet restore
```

3. **Build the solution:**
```bash
dotnet build
```

4. **Setup local database (if using LocalDB):**
   - Ensure SQL Server LocalDB is installed
   - Configure connection string in `local.settings.json`
   - Run migrations: See [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md)

5. **Setup local Azure services (optional):**
```bash
docker compose up -d  # Starts Azurite emulator for storage/queues
```

6. **Configure environment variables:**
   - Copy `.env.template` to `.env`
   - Update values for your environment

7. **Run Azure Functions locally:**
```bash
cd AzureFunctionApp
func start
```

The function app will be available at `http://localhost:7071`

## Development Workflow

### Running Tests
```bash
# Unit tests only
dotnet test TradeBot.UnitTests

# Integration tests (requires Azure Functions running)
dotnet test TradeBot.IntegrationTests

# All tests
dotnet test
```

### Building for Production
```bash
# Build Release configuration
dotnet build --configuration Release

# Publish Azure Functions
dotnet publish AzureFunctionApp --configuration Release
```

### Database Migrations
See [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md) for:
- Creating new migrations
- Applying migrations
- Troubleshooting migration issues

## Configuration

### Local Development (local.settings.json)
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

### Application Settings (appsettings.json)
Configure in `AzureFunctionApp/appsettings.json`:
- `RequestDataOptions` - API endpoints and headers
- `StatRangeOptions` - Equipment stat thresholds
- `DiscordIntegrationOptions` - Discord webhook URL

See individual project READMEs for detailed configuration.

## Infrastructure

### Deployment
All resources are hosted in **Azure Cloud**:
- **Function App** - Azure Functions for scheduled price monitoring
- **SQL Database** - Azure SQL for data persistence
- **Storage Account** - Blob storage and message queues
- **Key Vault** - Secrets management

### CI/CD
GitHub Actions workflow handles:
- Automated builds and tests
- Deployment to Azure environments
- Secret management through Key Vault

**Note:** Infrastructure was provisioned manually (No Terraform free tier anymore).

## Monitoring and Logging

TradeBot implements structured logging with:
- **Console logging** for local development
- **Application Insights** for production monitoring
- **Log levels**: Information, Warning, Error
- **Correlation IDs** for request tracing

View logs:
```bash
# Local development
func start  # Logs appear in console

# Production (Azure Portal)
# Navigate to Function App > Log Stream
```

## Documentation Index

| Component | Documentation |
|-----------|---------------|
| **TradeBot.Base** | [README.md](TradeBot.Base/README.md) |
| **TradeBot.Core** | [README.md](TradeBot.Core/README.md) |
| **TradeBot.Data** | [README.md](TradeBot.Data/README.md) |
| **TradeBot.Data.Migrations** | [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md) |
| **AzureFunctionApp** | [README.md](AzureFunctionApp/README.md) |
| **TradeBot.UnitTests** | [README.md](TradeBot.UnitTests/README.md) |
| **TradeBot.IntegrationTests** | [README.md](TradeBot.IntegrationTests/README.md) |
| **Data Layer** | [DATA_LAYER_README.md](DATA_LAYER_README.md) |

## Troubleshooting

### Azure Functions Won't Start
- Ensure `local.settings.json` exists and is configured
- Check connection strings are valid
- Verify Azure Functions Core Tools is installed: `func --version`

### Database Connection Errors
- Verify SQL Server or LocalDB is running
- Check connection string in `local.settings.json`
- Ensure migrations have been applied: `dotnet ef database update`

### Docker/Azurite Issues
- Ensure Docker is running: `docker ps`
- Check container status: `docker compose ps`
- View logs: `docker compose logs`

## Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make changes and run tests: `dotnet test`
3. Commit with clear messages: `git commit -m "Add feature description"`
4. Push to branch: `git push origin feature/your-feature`
5. Create a Pull Request

## Support

For issues or questions:
1. Check the relevant project README
2. Review existing GitHub issues
3. See [DATA_LAYER_README.md](DATA_LAYER_README.md) for database questions
4. See [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md) for migration questions
