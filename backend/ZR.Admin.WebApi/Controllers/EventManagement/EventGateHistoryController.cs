using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.   IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    /// <summary>
    /// Event Gate History - سجل تاريخ البوابات
    /// API Controller
    /// </summary>
    [Route("event/EventGateHistory")]
    public class EventGateHistoryController : BaseController
    {
        private readonly IEventGateHistoryService _eventGateHistoryService;

        public EventGateHistoryController(IEventGateHistoryService eventGateHistoryService)
        {
            _eventGateHistoryService = eventGateHistoryService;
        }

        /// <summary>
        /// Get gate history list with pagination and filters
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventgate:history")]
        public IActionResult QueryEventGateHistory([FromQuery] EventGateHistoryQueryDto parm)
        {
            return SUCCESS(_eventGateHistoryService.GetList(parm));
        }

        /// <summary>
        /// Get history by gate ID
        /// </summary>
        [HttpGet("gate/{gateId}")]
        [ActionPermissionFilter(Permission = "eventgate:history")]
        public IActionResult GetHistoryByGateId(long gateId)
        {
            return SUCCESS(_eventGateHistoryService.GetHistoryByGateId(gateId));
        }

        /// <summary>
        /// Get history by event ID
        /// </summary>
        [HttpGet("event/{eventId}")]
        [ActionPermissionFilter(Permission = "eventgate:history")]
        public IActionResult GetHistoryByEventId(long eventId)
        {
            return SUCCESS(_eventGateHistoryService.GetHistoryByEventId(eventId));
        }
    }
}

