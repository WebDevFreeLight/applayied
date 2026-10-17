using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{


    // ==================== Event Booth ====================
    public interface IEventBoothService : IBaseService<EventBooth>
    {
        PagedInfo<EventBoothDto> GetList(EventBoothQueryDto parm);
        EventBooth GetInfo(long boothId);
        EventBooth AddEventBooth(EventBooth parm);
        int UpdateEventBooth(EventBooth parm);
        List<EventBooth> GetBoothsByEventId(long eventId);
        List<EventBooth> GetAvailableBooths(long eventId);
        int AssignBoothToTenant(long boothId, long tenantId);
        int AssignBoothToTenantWithContract(AssignBoothToTenantRequest request);
        int ReleaseBoothFromTenant(long boothId);
        int UnlockBooth(long boothId);
    }
}

