using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Event Gate - بوابات الدخولservice接口
    /// </summary>
    public interface IEventGateService : IBaseService<EventGate>
    {
        PagedInfo<EventGateDto> GetList(EventGateQueryDto parm);

        EventGate GetInfo(long Gateid);


        EventGate AddEventGate(EventGate parm);
        int UpdateEventGate(EventGate parm);
        
        bool TruncateEventGate();

        (string, object, object) ImportEventGate(List<EventGate> list);

        PagedInfo<EventGateDto> ExportList(EventGateQueryDto parm);




        List<EventGate> GetGatesByEventId(long eventId);
        int UpdateGateStatus(long gateId, string status);
        int UpdateQueueCount(long gateId, int queueCount);


    }
}

