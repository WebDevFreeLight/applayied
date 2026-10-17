using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    #region Event Permit Controller

    /// <summary>
    /// Event Permit - ????????
    /// API Controller
    /// </summary>
    [Route("event/EventPermit")]
    public class EventPermitController : BaseController
    {
        private readonly IEventPermitService _EventPermitService;

        public EventPermitController(IEventPermitService eventPermitService)
        {
            _EventPermitService = eventPermitService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventpermit:list")]
        public IActionResult QueryEventPermits([FromQuery] EventPermitQueryDto parm)
        {
            return SUCCESS(_EventPermitService.GetList(parm));
        }

        [HttpGet("{permitId}")]
        [ActionPermissionFilter(Permission = "eventpermit:query")]
        public IActionResult GetEventPermit(long permitId)
        {
            var response = _EventPermitService.GetInfo(permitId);
            return SUCCESS(response.Adapt<EventPermitDto>());
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "eventpermit:add")]
        [Log(Title = "Event Permit", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventPermit([FromBody] EventPermitDto parm)
        {
            var modal = parm.Adapt<EventPermit>().ToCreate(HttpContext);
            return SUCCESS(_EventPermitService.AddEventPermit(modal));
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "eventpermit:edit")]
        [Log(Title = "Event Permit", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventPermit([FromBody] EventPermitDto parm)
        {
            var modal = parm.Adapt<EventPermit>().ToUpdate(HttpContext);
            int data = _EventPermitService.UpdateEventPermit(modal);
            return ToResponse(data);
        }

        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "eventpermit:delete")]
        [Log(Title = "Event Permit", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventPermit([FromRoute] string ids)
        {
            return ToResponse(_EventPermitService.Delete(Tools.SplitAndConvert<long>(ids), "Delete Event Permit"));
        }


        [Log(Title = "event permit Tenant", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "eventpermit:export")]
        public IActionResult Export([FromQuery] EventPermitQueryDto parm)
        {
            var list = _EventPermitService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "eventpermit Tenant", "eventpermit Tenant");


            return ExportExcel(result.Item2, result.Item1);
        }


        /// <summary>
        /// Get permits by event ID
        /// </summary>
        [HttpGet("byEvent/{eventId}")]
        [ActionPermissionFilter(Permission = "eventpermit:list")]
        public IActionResult GetPermitsByEvent(long eventId)
        {
            return SUCCESS(_EventPermitService.GetPermitsByEventId(eventId));
        }

        /// <summary>
        /// Approve permit
        /// </summary>
        [HttpPost("approve")]
        [ActionPermissionFilter(Permission = "eventpermit:approve")]
        [Log(Title = "Approve Permit", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApprovePermit([FromBody] ApproveEventPermitRequest request)
        {
            if (request == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Request payload is required.");
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return ToResponse(ResultCode.PARAM_ERROR, string.IsNullOrWhiteSpace(errors) ? "Invalid request payload." : errors);
            }

            if (request.PermitId <= 0 || request.ApprovedBy <= 0)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Permit ID and Approved By must be greater than zero.");
            }

            var permit = _EventPermitService.GetInfo(request.PermitId);
            if (permit == null)
            {
                return ToResponse(ResultCode.NO_DATA, "Permit not found.");
            }

            if (!string.Equals(permit.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                var message = string.Equals(permit.Status, "approved", StringComparison.OrdinalIgnoreCase)
                    ? "Permit is already approved."
                    : "Only permits with pending status can be approved.";
                return ToResponse(ResultCode.CUSTOM_ERROR, message);
            }

            var rows = _EventPermitService.ApprovePermit(request.PermitId, request.ApprovedBy);
            if (rows <= 0)
            {
                return ToResponse(ResultCode.FAIL, "Failed to approve permit.");
            }

            return ToResponse(ResultCode.SUCCESS, "Permit approved successfully.");
        }

        /// <summary>
        /// Reject permit
        /// </summary>
        [HttpPost("reject")]
        [ActionPermissionFilter(Permission = "eventpermit:reject")]
        [Log(Title = "Reject Permit", BusinessType = BusinessType.UPDATE)]
        public IActionResult RejectPermit([FromBody] RejectEventPermitRequest request)
        {
            if (request == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Request payload is required.");
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return ToResponse(ResultCode.PARAM_ERROR, string.IsNullOrWhiteSpace(errors) ? "Invalid request payload." : errors);
            }

            if (request.PermitId <= 0 || request.RejectedBy <= 0)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Permit ID and Rejected By must be greater than zero.");
            }

            var reason = request.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Rejection reason is required.");
            }

            var permit = _EventPermitService.GetInfo(request.PermitId);
            if (permit == null)
            {
                return ToResponse(ResultCode.NO_DATA, "Permit not found.");
            }

            if (!string.Equals(permit.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                var message = string.Equals(permit.Status, "rejected", StringComparison.OrdinalIgnoreCase)
                    ? "Permit is already rejected."
                    : "Only permits with pending status can be rejected.";
                return ToResponse(ResultCode.CUSTOM_ERROR, message);
            }

            var rows = _EventPermitService.RejectPermit(request.PermitId, request.RejectedBy, reason);
            if (rows <= 0)
            {
                return ToResponse(ResultCode.FAIL, "Failed to reject permit.");
            }

            return ToResponse(ResultCode.SUCCESS, "Permit rejected successfully.");
        }
    }

    #endregion

}

