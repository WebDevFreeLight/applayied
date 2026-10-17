using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;

namespace ZR.Service.EventManagement.IEventManagementService
{


    // ==================== Event Visitor ====================
    public interface IEventVisitorService : IBaseService<EventVisitor>
    {
        PagedInfo<EventVisitorDto> GetList(EventVisitorQueryDto parm);
        EventVisitor GetInfo(long visitorId);
        EventVisitor AddEventVisitor(EventVisitor parm);
        int UpdateEventVisitor(EventVisitor parm);
        
        /// <summary>
        /// DEPRECATED: Visitors are no longer tied to events. Use ticket_checkins table to find visitors by event through their tickets instead.
        /// </summary>
        [Obsolete("Visitors are no longer tied to events. Use ticket_checkins table to find visitors by event through their tickets.")]
        List<EventVisitor> GetVisitorsByEventId(long eventId);
        
        /// <summary>
        /// DEPRECATED: Check-ins are now only linked to tickets. Use TicketCheckinService to check in tickets instead.
        /// </summary>
        [Obsolete("Check-ins are now only linked to tickets. Use TicketCheckinService to check in tickets instead.")]
        int CheckInVisitor(long visitorId, long gateId);
        
        /// <summary>
        /// DEPRECATED: Check-ins are now only linked to tickets. Use TicketCheckinService to check out tickets instead.
        /// </summary>
        [Obsolete("Check-ins are now only linked to tickets. Use TicketCheckinService to check out tickets instead.")]
        int CheckOutVisitor(long visitorId, long gateId);
        
        /// <summary>
        /// DEPRECATED: QRCode field removed from visitor table.
        /// </summary>
        [Obsolete("QRCode field removed from visitor table. QR codes should be generated dynamically or stored in a separate table.")]
        EventVisitor GetVisitorByQRCode(string qrCode);
    }
}

