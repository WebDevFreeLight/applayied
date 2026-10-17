using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    #region Event Booth Controller

    /// <summary>
    /// Event Booth - ???????
    /// API Controller
    /// </summary>
    [Route("event/EventBooth")]
    public class EventBoothController : BaseController
    {
        private readonly IEventBoothService _EventBoothService;

        public EventBoothController(IEventBoothService eventBoothService)
        {
            _EventBoothService = eventBoothService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventbooth:list")]
        public IActionResult QueryEventBooths([FromQuery] EventBoothQueryDto parm)
        {
            return SUCCESS(_EventBoothService.GetList(parm));
        }

        [HttpGet("{boothId}")]
        [ActionPermissionFilter(Permission = "eventbooth:query")]
        public IActionResult GetEventBooth(long boothId)
        {
            var response = _EventBoothService.GetInfo(boothId);
            return SUCCESS(response.Adapt<EventBoothDto>());
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "eventbooth:add")]
        [Log(Title = "Event Booth", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventBooth([FromBody] EventBoothDto parm)
        {
            var modal = parm.Adapt<EventBooth>().ToCreate(HttpContext);
            return SUCCESS(_EventBoothService.AddEventBooth(modal));
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "eventbooth:edit")]
        [Log(Title = "Event Booth", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventBooth([FromBody] EventBoothDto parm)
        {
            var modal = parm.Adapt<EventBooth>().ToUpdate(HttpContext);
            return ToResponse(_EventBoothService.UpdateEventBooth(modal));
        }

        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "eventbooth:delete")]
        [Log(Title = "Event Booth", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventBooth([FromRoute] string ids)
        {
            return ToResponse(_EventBoothService.Delete(Tools.SplitAndConvert<long>(ids), "Delete Event Booth"));
        }

        /// <summary>
        /// Get booths by event ID
        /// </summary>
        [HttpGet("byEvent/{eventId}")]
        [ActionPermissionFilter(Permission = "eventbooth:list")]
        public IActionResult GetBoothsByEvent(long eventId)
        {
            return SUCCESS(_EventBoothService.GetBoothsByEventId(eventId));
        }

        /// <summary>
        /// Get available booths
        /// </summary>
        [HttpGet("available/{eventId}")]
        [ActionPermissionFilter(Permission = "eventbooth:list")]
        public IActionResult GetAvailableBooths(long eventId)
        {
            return SUCCESS(_EventBoothService.GetAvailableBooths(eventId));
        }

        /// <summary>
        /// Assign booth to tenant
        /// </summary>
        [HttpPost("assign")]
        [ActionPermissionFilter(Permission = "eventbooth:assign")]
        [Log(Title = "Assign Booth", BusinessType = BusinessType.UPDATE)]
        public IActionResult AssignBoothToTenant([FromBody] AssignBoothToTenantRequest data)
        {
            var result = _EventBoothService.AssignBoothToTenantWithContract(data);

            if (result > 0)
            {
                // Return the updated booth info
                var updatedBooth = _EventBoothService.GetInfo(data.BoothId);
                var message = data.CreateContract
                    ? "Booth assigned and contract created successfully"
                    : "Booth assigned successfully";
                return SUCCESS(updatedBooth.Adapt<EventBoothDto>(), message);
            }

            return ToResponse(result);
        }

        /// <summary>
        /// Release booth from tenant
        /// </summary>
        [HttpPost("release/{boothId}")]
        [ActionPermissionFilter(Permission = "eventbooth:release")]
        [Log(Title = "Release Booth", BusinessType = BusinessType.UPDATE)]
        public IActionResult ReleaseBoothFromTenant(long boothId)
        {
            var result = _EventBoothService.ReleaseBoothFromTenant(boothId);

            if (result > 0)
            {
                // Return the updated booth info
                var updatedBooth = _EventBoothService.GetInfo(boothId);
                return SUCCESS(updatedBooth.Adapt<EventBoothDto>(), "Booth released successfully");
            }

            return ToResponse(result);
        }

        /// <summary>
        /// Unlock booth - force release and make available
        /// </summary>
        [HttpPost("unlock/{boothId}")]
        [ActionPermissionFilter(Permission = "eventbooth:unlock")]
        [Log(Title = "Unlock Booth", BusinessType = BusinessType.UPDATE)]
        public IActionResult UnlockBooth(long boothId)
        {
            var result = _EventBoothService.UnlockBooth(boothId);

            if (result > 0)
            {
                // Return the updated booth info
                var updatedBooth = _EventBoothService.GetInfo(boothId);
                return SUCCESS(updatedBooth.Adapt<EventBoothDto>(), "Booth unlocked successfully");
            }

            return ToResponse(result);
        }
    }

    #endregion

}

