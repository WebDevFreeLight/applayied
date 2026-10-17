using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Customers.Dto;
using ZR.Model.Customers.Leads;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Customers;
using ZR.Model.Customers.Dto;
using ZR.Service.Customers.ICustomersService;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    /// <summary>
    /// Sales lead access - shows Qualified, In Negotiation, Confirmed leads
    /// </summary>
    [Verify]
    [Route("api/sales/leads")]
    public class SalesLeadController : BaseController
    {
        private static readonly string[] SalesAllowedStatuses = { "Qualified", "Under Negotiation", "Confirmed", "Converted" };
        private static readonly string[] SalesEditableStatuses = { "Qualified", "Under Negotiation", "Confirmed", "Converted", "Lost" };

        private readonly IBpLeadService _bpLeadService;
        private readonly ICustomerService _customerService;

        public SalesLeadController(IBpLeadService bpLeadService, ICustomerService customerService)
        {
            _bpLeadService = bpLeadService;
            _customerService = customerService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get leads list for Sales (Qualified, In Negotiation, Confirmed)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("sales:leads:list")]
        public async Task<IActionResult> GetLeadsList([FromQuery] LeadQueryDto dto)
        {
            var currentUser = GetCurrentUser();
            dto.CurrentUserId = currentUser?.UserId;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
            dto.IsCurrentUserAdmin = isAdmin || hasAllDataScope;

            // Sync: ensure active customers without leads get a Confirmed lead record
            await _bpLeadService.SyncCustomersToLeadsAsync(currentUser?.UserName ?? "System");

            // Sales only sees Qualified, In Negotiation, Confirmed leads
            dto.AllowedStatuses = SalesAllowedStatuses;

            // Filter by SalesAssignedTo instead of AssignedTo for non-admin sales users
            dto.FilterBySalesAssignment = true;

            var result = await _bpLeadService.GetLeadsListAsync(dto);
            return SUCCESS(result);
        }

        [HttpGet("{leadId}")]
        [ActionPermissionFilter("sales:leads:view")]
        public async Task<IActionResult> GetLead(long leadId)
        {
            return SUCCESS(await _bpLeadService.GetLeadByIdAsync(leadId));
        }

        /// <summary>
        /// Reassign salesperson on a lead (admin only)
        /// </summary>
        [HttpPost("update-sales-assignment")]
        [ActionPermissionFilter("sales:leads:edit")]
        public async Task<IActionResult> UpdateSalesAssignment([FromBody] UpdateSalesAssignmentDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                bool isAdmin = currentUser?.IsAdmin() ?? false;
                bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;

                if (!isAdmin && !hasAllDataScope)
                {
                    return ToResponse(ResultCode.FAIL, "ليس لديك صلاحية لتغيير موظف المبيعات المسؤول");
                }

                var lead = await _bpLeadService.GetLeadByIdAsync(dto.LeadId);
                if (lead == null)
                {
                    return ToResponse(ResultCode.FAIL, "Lead not found");
                }

                lead.SalesAssignedTo = dto.SalesAssignedTo;
                var result = await _bpLeadService.UpdateLeadAsync(lead);
                return SUCCESS(result, "تم تحديث موظف المبيعات المسؤول بنجاح");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to update sales assignment: {ex.Message}");
            }
        }

        /// <summary>
        /// Update lead status from Sales
        /// Sales can set: Qualified, In Negotiation, Confirmed, Lost
        /// </summary>
        [HttpPost("update-status")]
        [ActionPermissionFilter("sales:leads:edit")]
        public async Task<IActionResult> UpdateLeadStatus([FromBody] UpdateLeadStatusDto dto)
        {
            try
            {
                // Validate Sales can only set these statuses
                if (!SalesEditableStatuses.Contains(dto.NewStatus))
                {
                    return ToResponse(ResultCode.FAIL, $"Sales cannot set status to '{dto.NewStatus}'. Allowed: {string.Join(", ", SalesEditableStatuses)}");
                }

                var currentUser = GetCurrentUser();

                // Transactional status update with side effects (Lost cleanup, Confirmed auto-set, audit log)
                var result = await _bpLeadService.UpdateLeadStatusWithSideEffectsAsync(
                    dto.LeadId, dto.NewStatus, dto.Notes, currentUser?.UserName ?? "System");

                // Auto-create customer record when lead is Confirmed
                if (dto.NewStatus == "Confirmed")
                {
                    try
                    {
                        var lead = await _bpLeadService.GetLeadByIdAsync(dto.LeadId);

                        // Check if customer already exists for this lead
                        var existingCustomer = _customerService.Queryable()
                            .Where(c => c.LeadId == dto.LeadId)
                            .First();

                        if (existingCustomer == null && lead != null)
                        {
                            var customerDto = new CreateCustomerDto
                            {
                                CustomerCode = _customerService.GenerateNextCustomerCode(),
                                NameAr = lead.BrandName ?? lead.OwnerName,
                                NameEn = lead.BrandName ?? lead.OwnerName,
                                ContactPerson = lead.OwnerName,
                                Phone = lead.OwnerPhone,
                                Email = lead.OwnerEmail,
                                IsActive = true,
                                CreditDays = 30,
                                LeadId = lead.LeadId,
                                BrandName = lead.BrandName,
                                BrandImage = lead.BrandImage,
                            };

                            _customerService.Create(customerDto, currentUser?.UserName ?? "System");
                        }
                    }
                    catch (Exception customerEx)
                    {
                        // Log but don't fail the status update
                        Console.WriteLine($"Warning: Failed to auto-create customer for lead {dto.LeadId}: {customerEx.Message}");
                    }
                }

                return SUCCESS(result, "Lead status updated successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to update lead status: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle readyForOperations flag on a lead
        /// </summary>
        [HttpPost("toggle-ready-for-operations")]
        [ActionPermissionFilter("sales:leads:edit")]
        public async Task<IActionResult> ToggleReadyForOperations([FromBody] ToggleReadyForOperationsDto dto)
        {
            try
            {
                var lead = await _bpLeadService.GetLeadByIdAsync(dto.LeadId);
                if (lead == null)
                {
                    return ToResponse(ResultCode.FAIL, "Lead not found");
                }

                lead.ReadyForOperations = dto.ReadyForOperations;
                var result = await _bpLeadService.UpdateLeadAsync(lead);
                return SUCCESS(result, lead.ReadyForOperations
                    ? "Lead is now visible to Operations"
                    : "Lead is no longer visible to Operations");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to toggle ready for operations: {ex.Message}");
            }
        }
    }

    public class ToggleReadyForOperationsDto
    {
        public long LeadId { get; set; }
        public bool ReadyForOperations { get; set; }
    }
}
