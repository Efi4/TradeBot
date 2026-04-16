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

## Projects

### AzureFunctionApp
A .NET 8 Azure Function App with set of functions for continuous market monitoring.

**Key Features:**
- **.NET 8** target framework
- **Timer Trigger**: Executes every n seconds (configurable)
- **Dependency Injection**: Built-in support for logging and custom services
- **Local Development**: Ready to run locally with Azure Functions Core Tools

**See**: [AzureFunctionApp/README.md](AzureFunctionApp/README.md) for setup and deployment instructions.

---

## Getting Started

1. Open `TradeBot.sln` in Visual Studio or VS Code
2. Install .NET 8 SDK if not already installed
3. Restore NuGet packages: `dotnet restore`
4. Build the solution: `dotnet build`

## Development

For the Azure Function App:
- Code: [AzureFunctionApp/](AzureFunctionApp/)
For local database and storage account emulator:
- [docker compose file](docker-compose.yml)
- If docker engine is available, you can run 'docker compose up -d' to ensure application has required infrastructure to function locally. 

## Infrastructure

All resources are hosted in Azure:
 - Github Actions workflow is used for provisioning. 
 - Function app is available by link [functionapp](tradebot-fecxhhevc8hfe7g5.westeurope-01.azurewebsites.net).
 - It uses storage account and azure sql database as data layer.
 - Keyvault is used for secrets management.
