using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Event Location Information - معلومات موقع الفعاليةservice接口
    /// </summary>
    public interface IEventLocationService : IBaseService<EventLocation>
    {
        PagedInfo<EventLocationDto> GetList(EventLocationQueryDto parm);

        EventLocation GetInfo(long Locationid);


        EventLocation AddEventLocation(EventLocation parm);
        int UpdateEventLocation(EventLocation parm);
        
        bool TruncateEventLocation();

        (string, object, object) ImportEventLocation(List<EventLocation> list);

        PagedInfo<EventLocationDto> ExportList(EventLocationQueryDto parm);
    }
}

