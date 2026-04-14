using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureFunctionApp.Services;
using AzureFunctionApp.Interfaces;
using TradeBot.Base;
using TradeBot.Data.Configuration;
using System;
using TradeBot.Data.Contexts;
using Microsoft.EntityFrameworkCore;

// Load environment variables from .env file
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
        var connectionString = EnvironmentConfiguration.GetTradingDatabaseConnectionString();//config.GetConnectionString("tradingDatabase") 
        
        services.AddTradingDatabase(connectionString);

        // Register application services
        services.AddScoped<ICheckTheAvPricesService, CheckTheAvPricesService>();
    })
    .Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        await db.Database.MigrateAsync();
    }

await host.RunAsync();
