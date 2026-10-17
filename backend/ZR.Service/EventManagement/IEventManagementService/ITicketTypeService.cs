using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Ticket Type - أنواع التذاكرservice接口
    /// </summary>
    public interface ITicketTypeService : IBaseService<TicketType>
    {
        PagedInfo<TicketTypeDto> GetList(TicketTypeQueryDto parm);

        TicketType GetInfo(long Typeid);


        TicketType AddTicketType(TicketType parm);
        int UpdateTicketType(TicketType parm);
        
        bool TruncateTicketType();

        (string, object, object) ImportTicketType(List<TicketType> list);

        PagedInfo<TicketTypeDto> ExportList(TicketTypeQueryDto parm);
    }
}

