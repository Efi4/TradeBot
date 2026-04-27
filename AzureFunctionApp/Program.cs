using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeBot.Core.Services;
using TradeBot.Core.Interfaces;
using TradeBot.Base.Configuration;
using TradeBot.Data.Configuration;
using System;
using TradeBot.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;
using TradeBot.Core.Models;

// Load environment variables from .env file
var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
if(environment == "Development")
{
    EnvironmentConfiguration.LoadEnvironment();
}

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Register data access services
        var connectionString = EnvironmentConfiguration.GetTradingDatabaseConnectionString(environment == "Development");
        
        services.AddTradingDatabase(connectionString);

        // Register HTTP client with CheckThePricesService
        services.AddHttpClient<ICheckThePricesService, CheckThePricesService>();
        services.AddSingleton<ICalculateAveragePriceService, CalculateAveragePriceService>();
        // Bind HttpHeaders configuration
        services.Configure<RequestDataOptions>(context.Configuration.GetSection("RequestDataOptions"));
        services.Configure<StatRangeOptions>(context.Configuration.GetSection("StatRangeOptions"));

    })
    .ConfigureAppConfiguration((context, builder) =>
    {
        // Add custom configuration sources (JSON, Env Vars, etc.)
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     .AddUserSecrets<Program>(optional: true)
                     .AddEnvironmentVariables();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
