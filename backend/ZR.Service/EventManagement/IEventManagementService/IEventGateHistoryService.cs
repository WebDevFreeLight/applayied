using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{

    // ==================== Event Gate History ====================
    public interface IEventGateHistoryService : IBaseService<EventGateHistory>
    {
        PagedInfo<EventGateHistoryDto> GetList(EventGateHistoryQueryDto parm);
        List<EventGateHistoryDto> GetHistoryByGateId(long gateId);
        List<EventGateHistoryDto> GetHistoryByEventId(long eventId);
        EventGateHistory LogHistory(EventGateHistory history);
    }
}

