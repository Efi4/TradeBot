using System.Threading.Tasks;
using TradeBot.Base.Models;

namespace TradeBot.Core.Interfaces
{
    public interface IDiscordIntegrationService
    {
        Task PostMessageInDedicatedChannelAsync(EquipmentResponseModel equipmentData);
    }
}
