using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    #region Event Zone Controller

    /// <summary>
    /// Event Zone - ??????? ???? ????????
    /// API Controller
    /// </summary>
    [Route("event/EventZone")]
    public class EventZoneController : BaseController
    {
        private readonly IEventZoneService _EventZoneService;

        public EventZoneController(IEventZoneService eventZoneService)
        {
            _EventZoneService = eventZoneService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventzone:list")]
        public IActionResult QueryEventZones([FromQuery] EventZoneQueryDto parm)
        {
            return SUCCESS(_EventZoneService.GetList(parm));
        }

        [HttpGet("{zoneId}")]
        [ActionPermissionFilter(Permission = "eventzone:query")]
        public IActionResult GetEventZone(long zoneId)
        {
            var response = _EventZoneService.GetInfo(zoneId);
            return SUCCESS(response.Adapt<EventZoneDto>());
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "eventzone:add")]
        [Log(Title = "Event Zone", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventZone([FromBody] EventZoneDto parm)
        {
            var modal = parm.Adapt<EventZone>().ToCreate(HttpContext);
            return SUCCESS(_EventZoneService.AddEventZone(modal));
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "eventzone:edit")]
        [Log(Title = "Event Zone", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventZone([FromBody] EventZoneDto parm)
        {
            var modal = parm.Adapt<EventZone>().ToUpdate(HttpContext);
            return ToResponse(_EventZoneService.UpdateEventZone(modal));
        }

        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "eventzone:delete")]
        [Log(Title = "Event Zone", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventZone([FromRoute] string ids)
        {
            return ToResponse(_EventZoneService.Delete(Tools.SplitAndConvert<long>(ids), "Delete Event Zone"));
        }

        /// <summary>
        /// Get zones by event ID
        /// </summary>
        [HttpGet("byEvent/{eventId}")]
        [ActionPermissionFilter(Permission = "eventzone:list")]
        public IActionResult GetZonesByEvent(long eventId)
        {
            return SUCCESS(_EventZoneService.GetZonesByEventId(eventId));
        }

        /// <summary>
        /// Update zone occupancy
        /// </summary>
        [HttpPost("updateOccupancy")]
        [ActionPermissionFilter(Permission = "eventzone:edit")]
        [Log(Title = "Event Zone Occupancy", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateOccupancy([FromBody] UpdateOccupancyDto request)
        {
            return ToResponse(_EventZoneService.UpdateOccupancy(request.ZoneId, request.Occupancy));
        }
    }

    #endregion

}

