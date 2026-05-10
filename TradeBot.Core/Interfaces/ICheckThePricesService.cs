using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Base.Models;

namespace TradeBot.Core.Interfaces
{
    /// <summary>
    /// Service for managing item price checking and updates in the trading system.
    /// </summary>
    public interface ICheckThePricesService
    {
        /// <summary>
        /// Checks current market prices and identifies profitable trading deals.
        /// Compares market prices against stored average prices to find arbitrage opportunities.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a CheckPricesResult object
        /// with information about items checked, deals found, and any messages from the operation.
        /// </returns>
        /// <remarks>
        /// This method fetches current market data, filters items based on average prices,
        /// and publishes identified trade opportunities to the appropriate queue for notification.
        /// </remarks>
        Task<CheckPricesResult> CheckPricesAsync();

        /// <summary>
        /// Retrieves the current average price for a specific item based on its code and stats.
        /// </summary>
        /// <param name="itemTypePriceRequestModel">
        /// The request model containing the item code and its stats (e.g., attack/crit for weapons, stat for armor).
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an ItemPriceResponseModel
        /// with the item name, stats description, and current average price.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the item code cannot be parsed or no price data exists for the specified item and stats.
        /// </exception>
        Task<ItemPriceResponseModel> GetItemPriceAsync(ItemPriceRequestModel itemTypePriceRequestModel);

        /// <summary>
        /// Updates the average price for a specific item in the database.
        /// </summary>
        /// <param name="itemPriceRequest">
        /// The request model containing the item code, stats, and new price to set.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a boolean indicating
        /// whether the price update was successful (true) or failed (false).
        /// </returns>
        /// <remarks>
        /// This method updates or creates a price entry in the WeaponPrices or ArmorPrices table based on the item type.
        /// </remarks>
        Task<bool> SetItemPriceAsync(ItemSetPriceRequestModel itemPriceRequest);
    }
}
