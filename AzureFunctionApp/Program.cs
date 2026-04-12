using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureFunctionApp.Services;
using AzureFunctionApp.Interfaces;
using TradeBot.Data.Contexts;
using TradeBot.Base;
using TradeBot.Data.Configuration;
using System;

// // Load environment variables from .env file
var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
if(environment == "Development")
{
    EnvironmentConfiguration.LoadEnvironment();
}

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Register data access services
        var connectionString = EnvironmentConfiguration.GetTradingDatabaseConnectionString();
        // var connectionString = System.Environment.GetEnvironmentVariable(Constants.AzureSqlConnectionStringEnvironmentVariableName);
        
        services.AddTradingDatabase(connectionString);

        // Register application services
        services.AddScoped<ICheckTheAvPricesService, CheckTheAvPricesService>();
    })
    .Build();

await host.RunAsync();
