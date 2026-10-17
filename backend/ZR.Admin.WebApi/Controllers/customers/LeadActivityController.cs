using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Customers.Dto;
using ZR.Service.Customers.ICustomersService;

namespace ZR.Admin.WebApi.Controllers.Customers
{
    [Verify]
    [Route("api/customers/activities")]
    public class LeadActivityController : BaseController
    {
        private readonly IBpLeadActivityService _activityService;

        public LeadActivityController(IBpLeadActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter("marketing:leads:list")]
        public IActionResult GetActivityList([FromQuery] LeadActivityQueryDto queryDto)
        {
            return SUCCESS(_activityService.GetActivityList(queryDto));
        }

        [HttpGet("{activityId}")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetActivityById(long activityId)
        {
            var result = _activityService.GetActivityById(activityId);
            if (result == null) return ToResponse(ResultCode.FAIL, "Activity not found");
            return SUCCESS(result);
        }

        [HttpPost("create")]
        [ActionPermissionFilter("marketing:leads:edit")]
        public IActionResult CreateActivity([FromBody] CreateLeadActivityDto createDto)
        {
            try
            {
                return SUCCESS(_activityService.CreateActivity(createDto), "Activity created successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error creating activity: {ex.Message}");
            }
        }

        [HttpGet("lead/{leadId}/timeline")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetLeadActivityTimeline(long leadId)
        {
            return SUCCESS(_activityService.GetLeadActivityTimeline(leadId));
        }

        [HttpGet("employee/{employeeId}/recent")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetRecentActivitiesByEmployee(long employeeId, [FromQuery] int limit = 10)
        {
            return SUCCESS(_activityService.GetRecentActivitiesByEmployee(employeeId, limit));
        }

        [HttpGet("type/{activityType}")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetActivitiesByType(string activityType, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return SUCCESS(_activityService.GetActivitiesByType(activityType, startDate, endDate));
        }

        [HttpGet("follow-ups/upcoming")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetUpcomingFollowUps([FromQuery] long? employeeId = null, [FromQuery] int daysAhead = 7)
        {
            return SUCCESS(_activityService.GetUpcomingFollowUps(employeeId, daysAhead));
        }

        [HttpGet("follow-ups/overdue")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetOverdueFollowUps([FromQuery] long? employeeId = null)
        {
            return SUCCESS(_activityService.GetOverdueFollowUps(employeeId));
        }

        [HttpGet("statistics/employee/{employeeId}")]
        [ActionPermissionFilter("marketing:leads:view")]
        public IActionResult GetEmployeeActivityStatistics(long employeeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return SUCCESS(_activityService.GetEmployeeActivityStatistics(employeeId, startDate, endDate));
        }

        [HttpPost("delete/{activityId}")]
        [ActionPermissionFilter("marketing:leads:delete")]
        public IActionResult DeleteActivity(long activityId)
        {
            try
            {
                var result = _activityService.DeleteActivity(activityId);
                return SUCCESS(new { deletedCount = result }, "Activity deleted successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error deleting activity: {ex.Message}");
            }
        }
    }
}
