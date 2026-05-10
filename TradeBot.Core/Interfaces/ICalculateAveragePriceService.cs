using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Data.Models;

namespace TradeBot.Core.Interfaces
{
    /// <summary>
    /// Service for calculating and updating average prices for weapons and armor items.
    /// </summary>
    public interface ICalculateAveragePriceService
    {
        /// <summary>
        /// Calculates and updates average weapon prices based on market data.
        /// The calculation uses the lowest 30% of prices for each weapon stat combination to avoid outliers.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of calculated weapon prices.
        /// Returns an empty list if no weapons are found in the database.
        /// </returns>
        /// <remarks>
        /// This method retrieves all weapons from the database, groups them by attack and crit stats,
        /// calculates the average of the lowest 30% of prices for each group, and updates the WeaponPrices table.
        /// </remarks>
        Task<List<WeaponPrice>> CalculateAverageWeaponPricesAsync();

        /// <summary>
        /// Calculates and updates average armor prices based on market data.
        /// The calculation uses the lowest 30% of prices for each armor stat combination to avoid outliers.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of calculated armor prices.
        /// Returns an empty list if no armor items are found in the database.
        /// </returns>
        /// <remarks>
        /// This method retrieves all armor items from the database, groups them by type and stat,
        /// calculates the average of the lowest 30% of prices for each group, and updates the ArmorPrices table.
        /// </remarks>
        Task<List<ArmorPrice>> CalculateAverageArmorPricesAsync();
    }
}
