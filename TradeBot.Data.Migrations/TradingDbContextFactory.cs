using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TradeBot.Data.Contexts;
using System;
using TradeBot.Base;
using TradeBot.Base.Configuration;

namespace TradeBot.Data.Migrations;

/// <summary>
/// Factory for creating DbContext instances during design-time operations (migrations)
/// </summary>
public class TradingDbContextFactory : IDesignTimeDbContextFactory<TradingDbContext>
{
    public TradingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TradingDbContext>();
        var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        if(environment == "Development")
        {
            EnvironmentConfiguration.LoadEnvironment();
        }
        // Load connection string from environment
        var connectionString = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.LocalSqlConnectionStringEnvironmentVariableName);

        // if (string.IsNullOrEmpty(connectionString))
        // {
        //     // Fallback to local development connection string
        //     connectionString = "Server=(localdb)\\mssqllocaldb;Database=TradeBot;Trusted_Connection=true;";
        // }

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("TradeBot.Data.Migrations");
        });

        return new TradingDbContext(optionsBuilder.Options);
    }
}
