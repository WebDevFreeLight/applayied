using System;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{

    #region Event Ticket Controller

    /// <summary>
    /// Event Ticket - ???????
    /// API Controller
    /// </summary>
    [Route("event/EventTicket")]
    public class EventTicketController : BaseController
    {
        private readonly IEventTicketService _EventTicketService;
        private readonly ITicketCheckinService _TicketCheckinService;

        public EventTicketController(
            IEventTicketService eventTicketService,
            ITicketCheckinService ticketCheckinService)
        {
            _EventTicketService = eventTicketService;
            _TicketCheckinService = ticketCheckinService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventticket:list")]
        public IActionResult QueryEventTickets([FromQuery] EventTicketQueryDto parm)
        {
            return SUCCESS(_EventTicketService.GetList(parm));
        }

        [HttpGet("{ticketId}")]
        [ActionPermissionFilter(Permission = "eventticket:query")]
        public IActionResult GetEventTicket(long ticketId)
        {
            var response = _EventTicketService.GetInfo(ticketId);
            return SUCCESS(response.Adapt<EventTicketDto>());
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "eventticket:add")]
        [Log(Title = "Event Ticket", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventTicket([FromBody] EventTicketDto parm)
        {
            var modal = parm.Adapt<EventTicket>().ToCreate(HttpContext);
            return SUCCESS(_EventTicketService.AddEventTicket(modal));
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "eventticket:edit")]
        [Log(Title = "Event Ticket", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventTicket([FromBody] EventTicketDto parm)
        {
            var modal = parm.Adapt<EventTicket>().ToUpdate(HttpContext);
            return ToResponse(_EventTicketService.UpdateEventTicket(modal));
        }

        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "eventticket:delete")]
        [Log(Title = "Event Ticket", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventTicket([FromRoute] string ids)
        {
            return ToResponse(_EventTicketService.Delete(Tools.SplitAndConvert<long>(ids), "Delete Event Ticket"));
        }

        /// <summary>
        /// Get tickets by event ID
        /// </summary>
        [HttpGet("byEvent/{eventId}")]
        [ActionPermissionFilter(Permission = "eventticket:list")]
        public IActionResult GetTicketsByEvent(long eventId)
        {
            return SUCCESS(_EventTicketService.GetTicketsByEventId(eventId));
        }

        /// <summary>
        /// Get tickets by visitor ID
        /// </summary>
        [HttpGet("byVisitor/{visitorId}")]
        [ActionPermissionFilter(Permission = "eventticket:list")]
        public IActionResult GetTicketsByVisitor(long visitorId)
        {
            return SUCCESS(_EventTicketService.GetTicketsByVisitorId(visitorId));
        }

        /// <summary>
        /// Use ticket
        /// </summary>
        [HttpPost("use/{ticketId}")]
        [ActionPermissionFilter(Permission = "eventticket:edit")]
        [Log(Title = "Use Ticket", BusinessType = BusinessType.UPDATE)]
        public IActionResult UseTicket(long ticketId)
        {
            return ToResponse(_EventTicketService.UseTicket(ticketId));
        }

        /// <summary>
        /// Transfer ticket
        /// </summary>
        [HttpPost("transfer")]
        [ActionPermissionFilter(Permission = "eventticket:edit")]
        [Log(Title = "Transfer Ticket", BusinessType = BusinessType.UPDATE)]
        public IActionResult TransferTicket([FromBody] TransferTicketDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return ToResponse(ResultCode.FAIL, "Transfer request data is required");
                }

                if (dto.TicketId <= 0)
                {
                    return ToResponse(ResultCode.FAIL, "Invalid ticket ID");
                }

                if (dto.NewVisitorId <= 0)
                {
                    return ToResponse(ResultCode.FAIL, "Invalid visitor ID");
                }

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TransferTicket API - TicketId: {dto.TicketId}, NewVisitorId: {dto.NewVisitorId}");
                
                var result = _EventTicketService.TransferTicket(dto.TicketId, dto.NewVisitorId);
                return SUCCESS(result, "Ticket transferred successfully");
            }
            catch (CustomException ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TransferTicket API - CustomException: {ex.Message}");
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TransferTicket API - Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                return ToResponse(ResultCode.FAIL, $"Transfer failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel/Return ticket
        /// </summary>
        [HttpPost("cancel/{ticketId}")]
        [ActionPermissionFilter(Permission = "eventticket:edit")]
        [Log(Title = "Cancel Ticket", BusinessType = BusinessType.UPDATE)]
        public IActionResult CancelTicket(long ticketId)
        {
            return ToResponse(_EventTicketService.CancelTicket(ticketId));
        }

        /// <summary>
        /// Check-in ticket at gate
        /// </summary>
        [HttpPost("checkin")]
        [ActionPermissionFilter(Permission = "eventticket:edit")]
        [Log(Title = "Ticket Check-in", BusinessType = BusinessType.INSERT)]
        public IActionResult CheckInTicket([FromBody] CreateTicketCheckinDto dto)
        {
            var operatorId = HttpContext.GetUId();
            var ipAddress = HttpContext.GetClientUserIp();
            var checkin = _TicketCheckinService.CheckInTicket(dto, operatorId, ipAddress);
            
            // Get updated ticket status to return to frontend
            var updatedTicket = _EventTicketService.GetInfo(dto.TicketId);
            if (updatedTicket != null)
            {
                var ticketDto = updatedTicket.Adapt<EventTicketDto>();
                return SUCCESS(new { 
                    Checkin = checkin, 
                    Ticket = ticketDto 
                }, "Ticket checked in successfully");
            }
            
            return SUCCESS(checkin, "Ticket checked in successfully");
        }

        /// <summary>
        /// Check-out ticket at gate
        /// </summary>
        [HttpPost("checkout")]
        [ActionPermissionFilter(Permission = "eventticket:edit")]
        [Log(Title = "Ticket Check-out", BusinessType = BusinessType.INSERT)]
        public IActionResult CheckOutTicket([FromBody] CreateTicketCheckoutDto dto)
        {
            var operatorId = HttpContext.GetUId();
            var ipAddress = HttpContext.GetClientUserIp();
            var checkout = _TicketCheckinService.CheckOutTicket(dto, operatorId, ipAddress);
            
            // Get updated ticket status to return to frontend
            var updatedTicket = _EventTicketService.GetInfo(dto.TicketId);
            if (updatedTicket != null)
            {
                var ticketDto = updatedTicket.Adapt<EventTicketDto>();
                return SUCCESS(new { 
                    Checkout = checkout, 
                    Ticket = ticketDto 
                }, "Ticket checked out successfully");
            }
            
            return SUCCESS(checkout, "Ticket checked out successfully");
        }

        /// <summary>
        /// Get ticket check-in history
        /// </summary>
        [HttpGet("history/{ticketId}")]
        [ActionPermissionFilter(Permission = "eventticket:query")]
        public IActionResult GetTicketHistory(long ticketId, [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 100)
        {
            return SUCCESS(_TicketCheckinService.GetTicketHistory(ticketId, pageNum, pageSize));
        }

        /// <summary>
        /// Get ticket status (check-in info)
        /// </summary>
        [HttpGet("status/{ticketId}")]
        [ActionPermissionFilter(Permission = "eventticket:query")]
        public IActionResult GetTicketStatus(long ticketId)
        {
            return SUCCESS(_TicketCheckinService.GetTicketStatus(ticketId));
        }

        /// <summary>
        /// Get all check-in history with filters
        /// </summary>
        [HttpGet("checkin-history")]
        [ActionPermissionFilter(Permission = "eventticket:list")]
        public IActionResult GetCheckinHistory([FromQuery] TicketCheckinQueryDto parm)
        {
            return SUCCESS(_TicketCheckinService.GetHistory(parm));
        }
    }

    #endregion
}

