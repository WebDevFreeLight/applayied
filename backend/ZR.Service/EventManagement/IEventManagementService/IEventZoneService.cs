using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement
{
    // ==================== Event Zone ====================
    public interface IEventZoneService : IBaseService<EventZone>
    {
        PagedInfo<EventZoneDto> GetList(EventZoneQueryDto parm);
        EventZone GetInfo(long zoneId);
        EventZone AddEventZone(EventZone parm);
        int UpdateEventZone(EventZone parm);
        List<EventZone> GetZonesByEventId(long eventId);
        int UpdateOccupancy(long zoneId, int currentOccupancy);
    }
}

