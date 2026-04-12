using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeBot.Data.Contexts;

namespace TradeBot.Data.Configuration;

/// <summary>
/// Extension methods for database service registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Trading database context with dependency injection
    /// </summary>
    public static IServiceCollection AddTradingDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<TradingDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("TradeBot.Data.Migrations");
                sqlOptions.CommandTimeout(30);
            }));

        return services;
    }
}
