using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
  
    #region Event Incident Controller

    /// <summary>
    /// Event Incident - الحوادث والبلاغات
    /// API Controller
    /// </summary>
    [Route("event/EventIncident")]
    public class EventIncidentController : BaseController
    {
        private readonly IEventIncidentService _EventIncidentService;

        public EventIncidentController(IEventIncidentService eventIncidentService)
        {
            _EventIncidentService = eventIncidentService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventincident:list")]
        public IActionResult QueryEventIncidents([FromQuery] EventIncidentQueryDto parm)
        {
            return SUCCESS(_EventIncidentService.GetList(parm));
        }

        [HttpGet("{incidentId}")]
        [ActionPermissionFilter(Permission = "eventincident:query")]
        public IActionResult GetEventIncident(long incidentId)
        {
            var response = _EventIncidentService.GetInfo(incidentId);
            return SUCCESS(response.Adapt<EventIncidentDto>());
        }

        /// <summary>
        /// Create a new incident - Only allows creation fields
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "eventincident:add")]
        [Log(Title = "Event Incident", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventIncident([FromBody] EventIncidentCreateDto parm)
        {
            var modal = parm.Adapt<EventIncident>();
            // Set default values for creation
            modal.Status = "New";
            modal.IsActive = true;
            modal = modal.ToCreate(HttpContext);
            return SUCCESS(_EventIncidentService.AddEventIncident(modal));
        }

        /// <summary>
        /// Update an existing incident - Allows workflow-based field updates
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "eventincident:edit")]
        [Log(Title = "Event Incident", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventIncident([FromBody] EventIncidentUpdateDto parm)
        {
            // Get existing incident to preserve immutable fields
            var existing = _EventIncidentService.GetInfo(parm.IncidentId);
            if (existing == null)
            {
                return ToResponse(ApiResult.Error("Incident not found"));
            }

            var modal = parm.Adapt<EventIncident>();
            // Preserve immutable fields from existing record
            modal.EventId = existing.EventId;
            modal.ReportedDate = existing.ReportedDate;
            modal.ReportedBy = existing.ReportedBy;
            modal.ReporterType = existing.ReporterType;
            modal.ReporterContact = existing.ReporterContact;
            modal.IncidentTime = existing.IncidentTime;
            modal.IncidentNumber = existing.IncidentNumber;
            // Preserve workflow DateTime fields to prevent overwriting with default values
            modal.ResolvedTime = existing.ResolvedTime;
            modal.ClosedTime = existing.ClosedTime;

            modal = modal.ToUpdate(HttpContext);
            return ToResponse(_EventIncidentService.UpdateEventIncident(modal));
        }

        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "eventincident:delete")]
        [Log(Title = "Event Incident", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventIncident([FromRoute] string ids)
        {
            return ToResponse(_EventIncidentService.Delete(Tools.SplitAndConvert<long>(ids), "Delete Event Incident"));
        }

        /// <summary>
        /// Get incidents by event ID
        /// </summary>
        [HttpGet("byEvent/{eventId}")]
        [ActionPermissionFilter(Permission = "eventincident:list")]
        public IActionResult GetIncidentsByEvent(long eventId)
        {
            return SUCCESS(_EventIncidentService.GetIncidentsByEventId(eventId));
        }

        /// <summary>
        /// Get incidents by assignee
        /// </summary>
        [HttpGet("byAssignee/{assignedToId}")]
        [ActionPermissionFilter(Permission = "eventincident:list")]
        public IActionResult GetIncidentsByAssignee(long assignedToId)
        {
            return SUCCESS(_EventIncidentService.GetIncidentsByAssignee(assignedToId));
        }

        /// <summary>
        /// Assign incident
        /// </summary>
        [HttpPost("assign")]
        [ActionPermissionFilter(Permission = "eventincident:assign")]
        [Log(Title = "Assign Incident", BusinessType = BusinessType.UPDATE)]
        public IActionResult AssignIncident([FromBody] AssignIncidentDto parm)
        {
            return ToResponse(_EventIncidentService.AssignIncident(parm.IncidentId, parm.AssignedToId));
        }

        /// <summary>
        /// Resolve incident
        /// </summary>
        [HttpPost("resolve")]
        [ActionPermissionFilter(Permission = "eventincident:resolve")]
        [Log(Title = "Resolve Incident", BusinessType = BusinessType.UPDATE)]
        public IActionResult ResolveIncident([FromBody] ResolveIncidentDto parm)
        {
            return ToResponse(_EventIncidentService.ResolveIncident(parm.IncidentId, parm.Resolution));
        }

        /// <summary>
        /// Close incident
        /// </summary>
        [HttpPost("close")]
        [ActionPermissionFilter(Permission = "eventincident:close")]
        [Log(Title = "Close Incident", BusinessType = BusinessType.UPDATE)]
        public IActionResult CloseIncident([FromBody] CloseIncidentDto parm)
        {
            return ToResponse(_EventIncidentService.CloseIncident(parm.IncidentId, parm.SatisfactionRating));
        }
    }

    #endregion
}

