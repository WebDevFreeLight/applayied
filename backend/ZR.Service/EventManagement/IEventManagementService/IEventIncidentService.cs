using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{


    // ==================== Event Incident ====================
    public interface IEventIncidentService : IBaseService<EventIncident>
    {
        PagedInfo<EventIncidentDto> GetList(EventIncidentQueryDto parm);
        EventIncident GetInfo(long incidentId);
        EventIncident AddEventIncident(EventIncident parm);
        int UpdateEventIncident(EventIncident parm);
        List<EventIncident> GetIncidentsByEventId(long eventId);
        List<EventIncident> GetIncidentsByAssignee(long assignedToId);
        int AssignIncident(long incidentId, long assignedToId);
        int ResolveIncident(long incidentId, string resolution);
        int CloseIncident(long incidentId, int satisfactionRating);
    }
}

