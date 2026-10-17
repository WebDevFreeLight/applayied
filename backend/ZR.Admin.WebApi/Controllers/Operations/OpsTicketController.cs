using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Operations Ticket Management
    /// </summary>
    [Verify]
    [Route("api/operations/tickets")]
    public class OpsTicketController : BaseController
    {
        private readonly IOpsTicketService _ticketService;

        public OpsTicketController(IOpsTicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get paginated ticket list
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("operations:tickets:list")]
        public async Task<IActionResult> GetTicketsList([FromQuery] OpsTicketQueryDto query)
        {
            var currentUser = GetCurrentUser();
            query.CurrentUserId = currentUser?.UserId;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
            query.IsCurrentUserAdmin = isAdmin || hasAllDataScope;

            var result = await _ticketService.GetTicketsListAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get ticket by ID
        /// </summary>
        [HttpGet("{ticketId}")]
        [ActionPermissionFilter("operations:tickets:view")]
        public async Task<IActionResult> GetTicket(long ticketId)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                return ToResponse(ResultCode.FAIL, "Ticket not found");

            return SUCCESS(ticket);
        }

        /// <summary>
        /// Create a new ticket
        /// </summary>
        [HttpPost("add")]
        [ActionPermissionFilter("operations:tickets:add")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateOpsTicketDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                long? employeeId = null;
                if (currentUser?.UserId != null)
                    employeeId = await _ticketService.GetEmployeeIdByUserIdAsync(currentUser.UserId);

                var ticket = new OpsTicket
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    LeadId = dto.LeadId,
                    DealId = dto.DealId,
                    Category = dto.Category ?? "Operational",
                    Priority = dto.Priority ?? "Medium",
                    AssignedTo = dto.AssignedTo,
                    ReportedBy = employeeId,
                    Create_by = currentUser?.UserName
                };

                var result = await _ticketService.CreateTicketAsync(ticket);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update ticket details
        /// </summary>
        [HttpPost("update")]
        [ActionPermissionFilter("operations:tickets:edit")]
        public async Task<IActionResult> UpdateTicket([FromBody] UpdateOpsTicketDto dto)
        {
            try
            {
                var existing = await _ticketService.GetTicketByIdAsync(dto.TicketId);
                if (existing == null)
                    return ToResponse(ResultCode.FAIL, "Ticket not found");

                var currentUser = GetCurrentUser();
                var ticket = new OpsTicket
                {
                    TicketId = dto.TicketId,
                    Title = dto.Title,
                    Description = dto.Description,
                    LeadId = dto.LeadId,
                    DealId = dto.DealId,
                    Category = dto.Category,
                    Priority = dto.Priority,
                    AssignedTo = dto.AssignedTo,
                    TicketStatus = existing.TicketStatus,
                    ReportedBy = existing.ReportedBy,
                    ResolutionNotes = existing.ResolutionNotes,
                    ResolvedDate = existing.ResolvedDate,
                    EscalatedToDeal = existing.EscalatedToDeal,
                    Create_by = existing.Create_by,
                    Create_time = existing.Create_time,
                    Update_by = currentUser?.UserName
                };

                var result = await _ticketService.UpdateTicketAsync(ticket);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update ticket status (Open -> InProgress -> Resolved -> Closed)
        /// </summary>
        [HttpPost("update-status")]
        [ActionPermissionFilter("operations:tickets:edit")]
        public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateTicketStatusDto dto)
        {
            try
            {
                var result = await _ticketService.UpdateTicketStatusAsync(
                    dto.TicketId, dto.NewStatus, dto.Notes, dto.EscalateToDeal);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete tickets
        /// </summary>
        [HttpPost("delete")]
        [ActionPermissionFilter("operations:tickets:delete")]
        public async Task<IActionResult> DeleteTickets([FromBody] long[] ticketIds)
        {
            var result = await _ticketService.DeleteTicketsAsync(ticketIds);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get ticket dashboard stats
        /// </summary>
        [HttpGet("dashboard")]
        [ActionPermissionFilter("operations:tickets:list")]
        public async Task<IActionResult> GetDashboard()
        {
            var currentUser = GetCurrentUser();
            long? assignedTo = null;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;

            if (!isAdmin && !hasAllDataScope && currentUser?.UserId != null)
            {
                assignedTo = await _ticketService.GetEmployeeIdByUserIdAsync(currentUser.UserId);
            }

            var dashboard = await _ticketService.GetDashboardAsync(assignedTo);
            return SUCCESS(dashboard);
        }
    }
}
