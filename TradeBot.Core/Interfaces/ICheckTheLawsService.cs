using System;
using System.Threading.Tasks;

namespace TradeBot.Core.Interfaces
{
    /// <summary>
    /// Service for managing country laws for dedicated countries based on provided configuration.
    /// </summary>
    public interface ICheckTheLawsService
    {
        /// <summary>
        /// Checks country laws and identifies region transfer threats.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method fetches country laws data, filters based on configuration,
        /// and publishes region transfer warnings to the appropriate queue for notification.
        /// </remarks>
        Task CheckTheLawsAsync();

        /// <summary>
        /// Checks neighbouring countries' laws and identifies region transfer threats.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method fetches neighbouring countries' laws data, filters based on configuration,
        /// and publishes warning messages to the appropriate queue for notification.
        /// </remarks>
        Task CheckTheNeighboursLawsAsync();
    }
}
