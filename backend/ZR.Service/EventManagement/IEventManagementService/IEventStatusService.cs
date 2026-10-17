using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Event Status - أنواع حالة الفعالياتservice接口
    /// </summary>
    public interface IEventStatusService : IBaseService<EventStatus>
    {
        PagedInfo<EventStatusDto> GetList(EventStatusQueryDto parm);

        EventStatus GetInfo(long Typeid);


        EventStatus AddEventStatus(EventStatus parm);
        int UpdateEventStatus(EventStatus parm);
        
        bool TruncateEventStatus();

        (string, object, object) ImportEventStatus(List<EventStatus> list);

        PagedInfo<EventStatusDto> ExportList(EventStatusQueryDto parm);
    }
}

