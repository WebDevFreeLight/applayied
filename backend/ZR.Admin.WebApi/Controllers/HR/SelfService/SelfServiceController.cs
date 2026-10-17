using System.Text.Json.Serialization;
using Infrastructure.Converter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR;
using ZR.Service.HR.IHrService;
using Microsoft.AspNetCore.Authorization;
using ZR.Admin.WebApi.Extensions;
using ZR.Common;
using SqlSugar;

namespace ZR.Admin.WebApi.Controllers.HR.SelfService
{
    /// <summary>
    /// Employee Self-Service Portal Controller
    /// </summary>
    [Verify]
    [Route("api/hr/self")]
    [ApiExplorerSettings(GroupName = "hr")]
    [ApiController]
    [Authorize]
    public class SelfServiceController : BaseController
    {
        private readonly IEmployeeSelfServiceService _selfServiceService;

        public SelfServiceController(IEmployeeSelfServiceService selfServiceService)
        {
            _selfServiceService = selfServiceService;
        }

        /// <summary>
        /// Get my profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var employeeId = HttpContext.GetUId();
            var profile = await _selfServiceService.GetMyProfileAsync(employeeId);
            return SUCCESS(profile);
        }

        /// <summary>
        /// Update my profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] HrEmployeeProfile profile)
        {
            var employeeId = HttpContext.GetUId();
            var result = await _selfServiceService.UpdateMyProfileAsync(employeeId, profile);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get my attendance
        /// </summary>
        [HttpGet("attendance")]
        public async Task<IActionResult> GetMyAttendance([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var employeeId = HttpContext.GetUId();
            var attendance = await _selfServiceService.GetMyAttendanceAsync(employeeId, startDate, endDate);
            return SUCCESS(attendance);
        }

        /// <summary>
        /// Get my leave requests
        /// </summary>
        [HttpGet("leave")]
        public async Task<IActionResult> GetMyLeaveRequests()
        {
            var employeeId = HttpContext.GetUId();
            var requests = await _selfServiceService.GetMyLeaveRequestsAsync(employeeId);
            return SUCCESS(requests);
        }

        /// <summary>
        /// Get my payslips
        /// </summary>
        [HttpGet("payslips")]
        public async Task<IActionResult> GetMyPayslips([FromQuery] int year)
        {
            var employeeId = HttpContext.GetUId();
            var payslips = await _selfServiceService.GetMyPayslipsAsync(employeeId, year);
            return SUCCESS(payslips);
        }

        /// <summary>
        /// Get my notifications
        /// </summary>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
        {
            var employeeId = HttpContext.GetUId();
            var notifications = await _selfServiceService.GetMyNotificationsAsync(employeeId, unreadOnly);
            return SUCCESS(notifications);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(long id)
        {
            var employeeId = HttpContext.GetUId();
            var result = await _selfServiceService.MarkNotificationAsReadAsync(employeeId, id);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get my documents
        /// </summary>
        [HttpGet("documents")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var employeeId = HttpContext.GetUId();
            var documents = await _selfServiceService.GetMyDocumentsAsync(employeeId);
            return SUCCESS(documents);
        }

        /// <summary>
        /// Upload my document
        /// </summary>
        [HttpPost("documents")]
        public async Task<IActionResult> UploadMyDocument([FromForm] DocumentUploadRequest request)
        {
            var employeeId = HttpContext.GetUId();
            var result = await _selfServiceService.UploadMyDocumentAsync(employeeId, request);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get my benefits
        /// </summary>
        [HttpGet("benefits")]
        public async Task<IActionResult> GetMyBenefits()
        {
            var employeeId = HttpContext.GetUId();
            var benefits = await _selfServiceService.GetMyBenefitsAsync(employeeId);
            return SUCCESS(benefits);
        }

        /// <summary>
        /// Get my training history
        /// </summary>
        [HttpGet("training")]
        public async Task<IActionResult> GetMyTrainingHistory()
        {
            var employeeId = HttpContext.GetUId();
            var training = await _selfServiceService.GetMyTrainingHistoryAsync(employeeId);
            return SUCCESS(training);
        }

        /// <summary>
        /// Get my performance reviews
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> GetMyPerformanceReviews()
        {
            var employeeId = HttpContext.GetUId();
            var reviews = await _selfServiceService.GetMyPerformanceReviewsAsync(employeeId);
            return SUCCESS(reviews);
        }

        /// <summary>
        /// Submit time-off request
        /// </summary>
        [HttpPost("time-off")]
        public async Task<IActionResult> SubmitTimeOffRequest([FromBody] TimeOffRequest request)
        {
            var employeeId = HttpContext.GetUId();
            request.EmployeeId = employeeId;
            request.Create_by = HttpContext.GetName();
            request.Create_time = DateTime.Now;
            
            var result = await _selfServiceService.SubmitTimeOffRequestAsync(request);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Update emergency contact
        /// </summary>
        [HttpPut("emergency-contact")]
        public async Task<IActionResult> UpdateEmergencyContact([FromBody] HrEmergencyContact contact)
        {
            var employeeId = HttpContext.GetUId();
            var result = await _selfServiceService.UpdateEmergencyContactAsync(employeeId, contact);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get my team (if manager)
        /// </summary>
        [HttpGet("team")]
        public async Task<IActionResult> GetMyTeam()
        {
            var employeeId = HttpContext.GetUId();
            var team = await _selfServiceService.GetMyTeamAsync(employeeId);
            return SUCCESS(team);
        }
    }

    public class DocumentUploadRequest
    {
        public string DocumentType { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }

    public class TimeOffRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
    }

}

