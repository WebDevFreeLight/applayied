using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZR.Service.IService.Customers.DataMine
{
    public interface IDmPhoneRevealService
    {
        Task<int> GetTodayRevealCount(long userId);
        Task<List<long>> GetTodayRevealedContactIds(long userId);
        Task<string> RevealPhone(long userId, long contactId);
        Task<int> GetRemainingReveals(long userId);
    }
}
