using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Event Information - معلومات الفعاليةservice接口
    /// </summary>
    public interface IEventService : IBaseService<Event>
    {
        PagedInfo<EventDto> GetList(EventQueryDto parm);

        Event GetInfo(long Eventid);


        Event AddEvent(Event parm);
        int UpdateEvent(Event parm);
        
        bool TruncateEvent();

        (string, object, object) ImportEvent(List<Event> list);

        PagedInfo<EventDto> ExportList(EventQueryDto parm);
    }
}

