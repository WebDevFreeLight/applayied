using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{



    // ==================== Event Booth History ====================
    public interface IEventBoothHistoryService : IBaseService<EventBoothHistory>
    {
        PagedInfo<EventBoothHistoryDto> GetList(EventBoothHistoryQueryDto parm);
        List<EventBoothHistoryDto> GetHistoryByBoothId(long boothId);
        List<EventBoothHistoryDto> GetHistoryByTenantId(long tenantId);
        List<EventBoothHistoryDto> GetHistoryByEventId(long eventId);
        EventBoothHistory LogHistory(EventBoothHistory history);
    }

}

