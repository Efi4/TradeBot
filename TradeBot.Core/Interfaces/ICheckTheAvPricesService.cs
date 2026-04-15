using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Core.Models;

namespace TradeBot.Core.Interfaces
{
    public interface ICheckTheAvPricesService
    {
        Task<CheckPricesResult> CheckPricesAsync();
    }
}
