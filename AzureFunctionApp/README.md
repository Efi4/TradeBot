# Azure Function App - Timer & HTTP Triggered Functions

This is a .NET 8 Azure Function App project with timer-triggered and HTTP-triggered functions.

## Project Structure

```
AzureFunctionApp/
├── AzureFunctionApp.csproj      # Project file with .NET 8 target and NuGet packages
├── Program.cs                    # Function host configuration and dependency injection
├── host.json                     # Azure Functions host configuration
├── local.settings.json           # Local development settings
├── Functions/
│   ├── TimerTriggerFunction.cs       # Timer-triggered function (runs every 5 seconds)
│   ├── HttpTriggerFunction.cs        # HTTP-triggered function (responds to GET/POST requests)
│   └── CheckTheAvPrices.cs           # Daily price checking function (9 AM UTC)
├── Services/
│   ├── ICheckTheAvPricesService.cs   # Interface for price checking service
│   └── CheckTheAvPricesService.cs    # Implementation of price checking logic
└── Properties/
    └── launchSettings.json       # Visual Studio launch configuration
```

## Timer Trigger Details

The `TimerTriggerFunction` is configured to run every 5 seconds using a CRON expression: `0 */5 * * * *`

- **Description**: Timer trigger function that executes at regular intervals
- **Location**: [Functions/TimerTriggerFunction.cs](Functions/TimerTriggerFunction.cs)
- **Schedule**: Every 5 seconds (in local development)

## HTTP Trigger Details

The `HttpTriggerFunction` responds to HTTP GET and POST requests.

- **Description**: HTTP trigger function that handles incoming HTTP requests
- **Location**: [Functions/HttpTriggerFunction.cs](Functions/HttpTriggerFunction.cs)
- **Route**: `POST/GET /api/hello`
- **Authorization**: Anonymous
- **Response**: JSON object with message, timestamp, method, and path
- **Example Request**: `curl http://localhost:7071/api/hello`

## Check The Av Prices Function

The `CheckTheAvPrices` function runs daily at midnight (UTC) to monitor market prices.

- **Description**: Timer-triggered function that checks and monitors market prices for deals
- **Location**: [Functions/CheckTheAvPrices.cs](Functions/CheckTheAvPrices.cs)
- **Schedule**: Daily at midnight (`0 0 0 * * *`)
- **Service Dependency**: [Services/CheckTheAvPricesService.cs](Services/CheckTheAvPricesService.cs)
- **Purpose**: Integrates with `ICheckTheAvPricesService` to run price checking logic

## Services

### CheckTheAvPricesService

The `CheckTheAvPricesService` handles the business logic for checking and analyzing market prices.

- **Interface**: [Services/ICheckTheAvPricesService.cs](Services/ICheckTheAvPricesService.cs)
- **Implementation**: [Services/CheckTheAvPricesService.cs](Services/CheckTheAvPricesService.cs)
- **Lifetime**: Scoped (created per function invocation)
- **Features**:
  - Async price checking operation
  - Configurable logging
  - Error handling with result reporting
  - Tracks items checked and deals found

### Dependency Injection

The service is registered in [Program.cs](Program.cs):
```csharp
services.AddScoped<ICheckTheAvPricesService, CheckTheAvPricesService>();
```

Services can be injected into function classes through the constructor.

## Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- **IDE Options:**
  - Visual Studio 2022 with C# extensions
  - Visual Studio Code with C# Dev Kit and Azure Functions extensions

## Building and Running Locally

### Restore dependencies:
```bash
dotnet restore
```

### Build:
```bash
dotnet build
```

### Run locally with Azure Functions runtime:
```bash
func start
```

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
