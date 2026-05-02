using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeBot.Core.Services;
using TradeBot.Core.Interfaces;
using TradeBot.Base.Configuration;
using TradeBot.Data.Configuration;
using TradeBot.Data.Helpers;
using System;
using TradeBot.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TradeBot.Base.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http.Diagnostics;
using TradeBot.Base;

var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

if (environment == "Development")
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
        
        services.AddRedaction();
        services.AddExtendedHttpClientLogging(options=>
        {
            options.RequestPathParameterRedactionMode = HttpRouteParameterRedactionMode.Strict;
        });
        // Register HTTP client with CheckThePricesService
        services.AddScoped<ICheckThePricesService, CheckThePricesService>();
        services.AddHttpClient<ICheckThePricesService, CheckThePricesService>();
        services.AddScoped<ICalculateAveragePriceService, CalculateAveragePriceService>();
        services.AddHttpClient<IDiscordIntegrationService, DiscordIntegrationService>();
        services.AddScoped<IDiscordIntegrationService, DiscordIntegrationService>();
        
        // Register AzureStorageHelper as Singleton
        services.AddSingleton<IAzureStorageHelper, AzureStorageHelper>();
        
        // Bind configuration
        services.Configure<RequestDataOptions>(context.Configuration.GetSection(Constants.Appsettings.RequestDataOptionsSectionName));
        services.Configure<StatRangeOptions>(context.Configuration.GetSection(Constants.Appsettings.StatRangeOptionsSectionName));
        services.Configure<DiscordIntegrationOptions>(context.Configuration.GetSection(Constants.Appsettings.DiscordIntegrationOptionsSectionName));

    })
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
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
