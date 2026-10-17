using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    /// <summary>
    /// Event Booth History - سجل الأكشاك
    /// API Controller
    /// </summary>
    [Route("event/EventBoothHistory")]
    public class EventBoothHistoryController : BaseController
    {
        private readonly IEventBoothHistoryService _eventBoothHistoryService;

        public EventBoothHistoryController(IEventBoothHistoryService eventBoothHistoryService)
        {
            _eventBoothHistoryService = eventBoothHistoryService;
        }

        /// <summary>
        /// Get booth history list with pagination and filters
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventbooth:history")]
        public IActionResult QueryEventBoothHistory([FromQuery] EventBoothHistoryQueryDto parm)
        {
            return SUCCESS(_eventBoothHistoryService.GetList(parm));
        }

        /// <summary>
        /// Get history by booth ID
        /// </summary>
        [HttpGet("booth/{boothId}")]
        [ActionPermissionFilter(Permission = "eventbooth:history")]
        public IActionResult GetHistoryByBoothId(long boothId)
        {
            return SUCCESS(_eventBoothHistoryService.GetHistoryByBoothId(boothId));
        }

        /// <summary>
        /// Get history by tenant ID
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        [ActionPermissionFilter(Permission = "eventbooth:history")]
        public IActionResult GetHistoryByTenantId(long tenantId)
        {
            return SUCCESS(_eventBoothHistoryService.GetHistoryByTenantId(tenantId));
        }

        /// <summary>
        /// Get history by event ID
        /// </summary>
        [HttpGet("event/{eventId}")]
        [ActionPermissionFilter(Permission = "eventbooth:history")]
        public IActionResult GetHistoryByEventId(long eventId)
        {
            return SUCCESS(_eventBoothHistoryService.GetHistoryByEventId(eventId));
        }
    }
}

