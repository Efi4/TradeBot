using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeBot.Data.Contexts;

namespace TradeBot.Data.Helpers;

/// <summary>
/// Extension methods for database initialization and seeding
/// </summary>
public static class DatabaseHelpers
{
    /// <summary>
    /// Verifies database connectivity
    /// </summary>
    public static async Task<bool> VerifyConnectionAsync(TradingDbContext context)
    {
        try
        {
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}
