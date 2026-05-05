using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Base.Models;

namespace TradeBot.Core.Interfaces
{
    public interface ICheckThePricesService
    {
        Task<CheckPricesResult> CheckPricesAsync();
        Task<ItemPriceResponseModel> GetItemPriceAsync(ItemPriceRequestModel itemTypePriceRequestModel);
        Task<bool> SetItemPriceAsync(ItemSetPriceRequestModel itemPriceRequest);
    }
}
