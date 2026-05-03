using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Data.Models;

namespace TradeBot.Core.Interfaces
{
    public interface ICalculateAveragePriceService
    {
        Task<List<WeaponPrice>> CalculateAverageWeaponPricesAsync();
        Task<List<ArmorPrice>> CalculateAverageArmorPricesAsync();
    }
}
