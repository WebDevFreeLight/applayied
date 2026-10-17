using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Customers.Dto;

using ZR.Service.Customers.ICustomersService;

namespace ZR.Admin.WebApi.Controllers.Customers
{
    [Verify]
    [Route("api/customers/assignments")]
    public class LeadAssignmentController : BaseController
    {
        private readonly IBpLeadAssignmentService _assignmentService;

        public LeadAssignmentController(IBpLeadAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpGet("history")]
        [ActionPermissionFilter("marketing:leads:list")]
        public IActionResult GetAssignmentHistory([FromQuery] LeadAssignmentQueryDto queryDto)
        {
            return SUCCESS(_assignmentService.GetAssignmentHistory(queryDto));
        }

        [HttpGet("lead/{leadId}/history")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetLeadAssignmentHistory(long leadId)
        {
            return SUCCESS(_assignmentService.GetLeadAssignmentHistory(leadId));
        }

        [HttpPost("assign")]
        [ActionPermissionFilter("marketing:leads:edit")]
        public IActionResult AssignLead([FromBody] AssignLeadDto assignDto)
        {
            try
            {
                return SUCCESS(_assignmentService.AssignLead(assignDto), "Lead assigned successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error assigning lead: {ex.Message}");
            }
        }

        [HttpPost("bulk-assign")]
        [ActionPermissionFilter("marketing:leads:edit")]
        public IActionResult BulkAssignLeads([FromBody] BulkAssignLeadsDto bulkAssignDto)
        {
            try
            {
                var count = _assignmentService.BulkAssignLeads(bulkAssignDto);
                return SUCCESS(new { assignedCount = count }, $"Successfully assigned {count} leads");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error in bulk assignment: {ex.Message}");
            }
        }

        [HttpPost("auto-assign/{leadId}")]
        [ActionPermissionFilter("marketing:leads:edit")]
        public IActionResult AutoAssignLead(long leadId, [FromBody] string reason = "Auto-Assignment")
        {
            try
            {
                return SUCCESS(_assignmentService.AutoAssignLead(leadId, reason), "Lead auto-assigned successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error in auto-assignment: {ex.Message}");
            }
        }

        [HttpGet("workload-distribution")]
        [ActionPermissionFilter("marketing:leads:list")]
        public IActionResult GetWorkloadDistribution([FromQuery] string leadStatus = null)
        {
            return SUCCESS(_assignmentService.GetWorkloadDistribution(leadStatus));
        }

        [HttpGet("employee/{employeeId}/lead-count")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetEmployeeLeadCount(long employeeId, [FromQuery] string leadStatus = null)
        {
            return SUCCESS(new { employeeId, leadCount = _assignmentService.GetEmployeeLeadCount(employeeId, leadStatus), leadStatus });
        }

        [HttpGet("recent-reassignments")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetRecentReassignments([FromQuery] int days = 7)
        {
            return SUCCESS(_assignmentService.GetRecentReassignments(days));
        }

        [HttpGet("statistics")]
        [ActionPermissionFilter("marketing:leads:list")]
        public IActionResult GetAssignmentStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return SUCCESS(_assignmentService.GetAssignmentStatistics(startDate, endDate));
        }
    }
}
