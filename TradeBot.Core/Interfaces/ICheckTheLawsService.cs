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
        /// Checks current country laws and identifies region transfer threats.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method fetches current country laws data, filters based on configuration,
        /// and publishes region transfer threats to the appropriate queue for notification.
        /// </remarks>
        Task CheckTheLawsAsync();
    }
}
