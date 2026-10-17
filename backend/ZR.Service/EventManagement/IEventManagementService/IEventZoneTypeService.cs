using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Zone Type - أنواع المناطقservice接口
    /// </summary>
    public interface IEventZoneTypeService : IBaseService<EventZoneType>
    {
        PagedInfo<EventZoneTypeDto> GetList(EventZoneTypeQueryDto parm);

        EventZoneType GetInfo(long Typeid);


        EventZoneType AddEventZoneType(EventZoneType parm);
        int UpdateEventZoneType(EventZoneType parm);
        
        bool TruncateEventZoneType();

        (string, object, object) ImportEventZoneType(List<EventZoneType> list);

        PagedInfo<EventZoneTypeDto> ExportList(EventZoneTypeQueryDto parm);
    }
}

