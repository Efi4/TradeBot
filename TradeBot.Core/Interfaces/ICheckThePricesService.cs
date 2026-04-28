using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Core.Models;

namespace TradeBot.Core.Interfaces
{
    public interface ICheckThePricesService
    {
        Task<CheckPricesResult> CheckWeaponPricesAsync();
    }
}
