using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{
    // ==================== Event Ticket ====================
    public interface IEventTicketService : IBaseService<EventTicket>
    {
        PagedInfo<EventTicketDto> GetList(EventTicketQueryDto parm);
        EventTicket GetInfo(long ticketId);
        EventTicket AddEventTicket(EventTicket parm);
        int UpdateEventTicket(EventTicket parm);
        List<EventTicket> GetTicketsByEventId(long eventId);
        List<EventTicket> GetTicketsByVisitorId(long visitorId);
        int UseTicket(long ticketId);
        int TransferTicket(long ticketId, long newVisitorId);
        public int CancelTicket(long ticketId);
    }
}

