using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Event Type - أنواع الفعالياتservice接口
    /// </summary>
    public interface IEventTypeService : IBaseService<EventType>
    {
        PagedInfo<EventTypeDto> GetList(EventTypeQueryDto parm);

        EventType GetInfo(long Typeid);


        EventType AddEventType(EventType parm);
        int UpdateEventType(EventType parm);
        
        bool TruncateEventType();

        (string, object, object) ImportEventType(List<EventType> list);

        PagedInfo<EventTypeDto> ExportList(EventTypeQueryDto parm);
    }
}

