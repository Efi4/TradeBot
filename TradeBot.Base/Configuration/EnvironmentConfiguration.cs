using System;
using System.IO;
using DotNetEnv;

namespace TradeBot.Base;

/// <summary>
/// Environment configuration loader
/// </summary>
public static class EnvironmentConfiguration
{
    /// <summary>
    /// Loads environment variables from .env file
    /// </summary>
    public static void LoadEnvironment(string? envFilePath = null)
    {
        if (envFilePath == null)
        {
            // Look for .env in the application root
            envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
        }

        if (File.Exists(envFilePath))
        {
            DotNetEnv.Env.Load(envFilePath);
        }
    }

    /// <summary>
    /// Gets the trading database connection string from environment
    /// </summary>
    public static string GetTradingDatabaseConnectionString()
    {
        return Environment.GetEnvironmentVariable(Constants.AzureSqlConnectionStringEnvironmentVariableName)
            ?? throw new ArgumentNullException(nameof(Constants.AzureSqlConnectionStringEnvironmentVariableName));
    }

    /// <summary>
    /// Gets the Azure Storage connection string from environment
    /// </summary>
    public static string GetAzureStorageConnectionString()
    {
        return Environment.GetEnvironmentVariable(Constants.AzureStorageConnectionStringEnvironmentVariableName)
            ?? throw new ArgumentNullException(nameof(Constants.AzureStorageConnectionStringEnvironmentVariableName));
    }
}
