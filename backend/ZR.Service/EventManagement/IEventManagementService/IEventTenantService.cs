using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{

    // ==================== Event Tenant ====================
    public interface IEventTenantService : IBaseService<EventTenant>
    {
        PagedInfo<EventTenantDto> GetList(EventTenantQueryDto parm);
        EventTenant GetInfo(long tenantId);
        EventTenant AddEventTenant(EventTenant parm);
        int UpdateEventTenant(EventTenant parm);
        bool TruncateEventTenant();
        (string, object, object) ImportEventTenant(List<EventTenant> list);
        PagedInfo<EventTenantDto> ExportList(EventTenantQueryDto parm);
    }

}

