using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TradeBot.Core.Interfaces
{
    public interface ICalculateAveragePriceService
    {
        Task<bool> CalculateAverageWeaponPricesAsync();
        Task<bool> CalculateAverageArmorPricesAsync();
    }
}
