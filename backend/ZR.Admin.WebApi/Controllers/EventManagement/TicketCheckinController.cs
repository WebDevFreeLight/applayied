using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    /// <summary>
    /// Ticket Check-in/Checkout Controller - *3,JD /.HD H.1H, 'D*0'C1
    /// API Controller
    /// </summary>
    [Route("api/ticket-checkin")]
    public class TicketCheckinController : BaseController
    {
        private readonly ITicketCheckinService _ticketCheckinService;
        private readonly IEventTicketService _eventTicketService;

        public TicketCheckinController(
            ITicketCheckinService ticketCheckinService,
            IEventTicketService eventTicketService)
        {
            _ticketCheckinService = ticketCheckinService;
            _eventTicketService = eventTicketService;
        }

        /// <summary>
        /// Get check-in/checkout history with pagination
        /// </summary>
        [HttpGet("history")]
        [ActionPermissionFilter(Permission = "ticketcheckin:list")]
        public IActionResult GetHistory([FromQuery] TicketCheckinQueryDto parm)
        {
            var response = _ticketCheckinService.GetHistory(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get ticket history by ticket ID
        /// </summary>
        [HttpGet("history/{ticketId}")]
        [ActionPermissionFilter(Permission = "ticketcheckin:query")]
        public IActionResult GetTicketHistory(long ticketId, [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 100)
        {
            var history = _ticketCheckinService.GetTicketHistory(ticketId, pageNum, pageSize);
            return SUCCESS(history);
        }

        /// <summary>
        /// Get ticket current status
        /// </summary>
        [HttpGet("status/{ticketId}")]
        [ActionPermissionFilter(Permission = "ticketcheckin:query")]
        public IActionResult GetTicketStatus(long ticketId)
        {
            var status = _ticketCheckinService.GetTicketStatus(ticketId);
            if (status == null)
            {
                return ToResponse(ResultCode.FAIL, "Ticket not found");
            }
            return SUCCESS(status);
        }

        /// <summary>
        /// Get all tickets for a visitor
        /// </summary>
        [HttpGet("visitor/{visitorId}/tickets")]
        [ActionPermissionFilter(Permission = "ticketcheckin:query")]
        public IActionResult GetVisitorTickets(long visitorId)
        {
            var tickets = _ticketCheckinService.GetVisitorTickets(visitorId);
            return SUCCESS(tickets);
        }

        /// <summary>
        /// Check-in ticket
        /// </summary>
        [HttpPost("checkin")]
        [ActionPermissionFilter(Permission = "ticketcheckin:checkin")]
        [Log(Title = "Ticket Check-In", BusinessType = BusinessType.INSERT)]
        public IActionResult CheckIn([FromBody] CreateTicketCheckinDto dto)
        {
            try
            {
                // Get operator ID from current user
                var operatorId = HttpContext.GetUId();

                // Get IP address
                var ipAddress = HttpContext.GetClientUserIp();

                var result = _ticketCheckinService.CheckInTicket(dto, operatorId, ipAddress);
                
                // Get updated ticket status to return to frontend
                var updatedTicket = _eventTicketService.GetInfo(dto.TicketId);
                if (updatedTicket != null)
                {
                    var ticketDto = updatedTicket.Adapt<EventTicketDto>();
                    return SUCCESS(new { 
                        Checkin = result, 
                        Ticket = ticketDto 
                    }, "Ticket checked in successfully");
                }
                
                return SUCCESS(result, "Ticket checked in successfully");
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Check-in failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Check-out ticket
        /// </summary>
        [HttpPost("checkout")]
        [ActionPermissionFilter(Permission = "ticketcheckin:checkout")]
        [Log(Title = "Ticket Check-Out", BusinessType = BusinessType.INSERT)]
        public IActionResult CheckOut([FromBody] CreateTicketCheckoutDto dto)
        {
            try
            {
                // Get operator ID from current user
                var operatorId = HttpContext.GetUId();

                // Get IP address
                var ipAddress = HttpContext.GetClientUserIp();

                var result = _ticketCheckinService.CheckOutTicket(dto, operatorId, ipAddress);
                
                // Get updated ticket status to return to frontend
                var updatedTicket = _eventTicketService.GetInfo(dto.TicketId);
                if (updatedTicket != null)
                {
                    var ticketDto = updatedTicket.Adapt<EventTicketDto>();
                    return SUCCESS(new { 
                        Checkout = result, 
                        Ticket = ticketDto 
                    }, "Ticket checked out successfully");
                }
                
                return SUCCESS(result, "Ticket checked out successfully");
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Check-out failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get check-in details
        /// </summary>
        [HttpGet("{checkinId}")]
        [ActionPermissionFilter(Permission = "ticketcheckin:query")]
        public IActionResult GetCheckinDetail(long checkinId)
        {
            var response = _ticketCheckinService.GetInfo(checkinId);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Check-in record not found");
            }
            return SUCCESS(response.Adapt<TicketCheckinHistoryDto>());
        }
    }
}

