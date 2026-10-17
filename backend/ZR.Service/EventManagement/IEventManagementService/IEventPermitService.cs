using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{

    // ==================== Event Permit ====================
    public interface IEventPermitService : IBaseService<EventPermit>
    {
        PagedInfo<EventPermitDto> GetList(EventPermitQueryDto parm);
        EventPermit GetInfo(long permitId);
        EventPermit AddEventPermit(EventPermit parm);
        int UpdateEventPermit(EventPermit parm);

        PagedInfo<EventPermitDto> ExportList(EventPermitQueryDto parm);

        List<EventPermit> GetPermitsByEventId(long eventId);
        int ApprovePermit(long permitId, long approvedBy);
        int RejectPermit(long permitId, long rejectedBy, string reason);
    }

}

