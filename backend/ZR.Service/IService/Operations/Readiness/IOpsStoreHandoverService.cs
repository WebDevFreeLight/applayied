using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Operations.Readiness;
using ZR.Model.Operations.Readiness.Dto;

namespace ZR.Service.Operations.Readiness
{
    public interface IOpsStoreHandoverService
    {
        Task<PagedInfo<OpsStoreHandover>> GetHandoverListAsync(StoreHandoverQueryDto query);
        Task<OpsStoreHandover> GetHandoverByIdAsync(long handoverId);
        Task<OpsStoreHandover> GetHandoverByShopAsync(long shopId);
        Task<OpsStoreHandover> InitializeHandoverAsync(InitializeHandoverDto dto, string userName);
        Task<OpsStoreHandoverItem> CompleteItemAsync(long itemId, CompleteItemDto dto, string userName);
        Task<OpsStoreHandoverItem> UncompleteItemAsync(long itemId, string userName);
        Task<int> RequestHandoverAsync(long handoverId, RequestHandoverDto dto, string userName);
        Task<int> ApproveHandoverAsync(long handoverId, ApproveHandoverDto dto, string userName);
        Task<int> RejectHandoverAsync(long handoverId, RejectHandoverDto dto, string userName);
        Task<HandoverProgressDto> GetProgressAsync(long handoverId);
        Task<int> SyncRequirementsForActiveHandoversAsync(string userName);
    }
}
