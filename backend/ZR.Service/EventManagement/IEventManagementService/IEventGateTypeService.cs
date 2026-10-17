using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Gate Type - أنواع البواباتservice接口
    /// </summary>
    public interface IEventGateTypeService : IBaseService<EventGateType>
    {
        PagedInfo<EventGateTypeDto> GetList(EventGateTypeQueryDto parm);

        EventGateType GetInfo(long Typeid);


        EventGateType AddEventGateType(EventGateType parm);
        int UpdateEventGateType(EventGateType parm);
        
        bool TruncateEventGateType();

        (string, object, object) ImportEventGateType(List<EventGateType> list);

        PagedInfo<EventGateTypeDto> ExportList(EventGateTypeQueryDto parm);
    }
}

