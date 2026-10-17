using Infrastructure;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Ticket Check-in/Checkout Service Interface
    /// </summary>
    public interface ITicketCheckinService : IBaseService<TicketCheckin>
    {
        /// <summary>
        /// Get paginated check-in history
        /// </summary>
        PagedInfo<TicketCheckinHistoryDto> GetHistory(TicketCheckinQueryDto parm);

        /// <summary>
        /// Check-in ticket
        /// </summary>
        TicketCheckin CheckInTicket(CreateTicketCheckinDto dto, long operatorId, string ipAddress);

        /// <summary>
        /// Check-out ticket
        /// </summary>
        TicketCheckin CheckOutTicket(CreateTicketCheckoutDto dto, long operatorId, string ipAddress);

        /// <summary>
        /// Get ticket current status
        /// </summary>
        TicketStatusDto GetTicketStatus(long ticketId);

        /// <summary>
        /// Get ticket history
        /// </summary>
        List<TicketCheckinHistoryDto> GetTicketHistory(long ticketId, int pageNum = 1, int pageSize = 100);

        /// <summary>
        /// Get all tickets for a visitor
        /// </summary>
        List<TicketStatusDto> GetVisitorTickets(long visitorId);

        /// <summary>
        /// Get check-in record by ID
        /// </summary>
        TicketCheckin GetInfo(long checkinId);
    }
}

