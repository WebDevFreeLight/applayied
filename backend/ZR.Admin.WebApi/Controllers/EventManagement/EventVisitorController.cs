using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    #region Event Visitor Controller

    /// <summary>
    /// Event Visitor - الزوار
    /// API Controller
    /// </summary>
    [Route("event/EventVisitor")]
    public class EventVisitorController : BaseController
    {
        private readonly IEventVisitorService _EventVisitorService;

        public EventVisitorController(IEventVisitorService eventVisitorService)
        {
            _EventVisitorService = eventVisitorService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventvisitor:list")]
        public IActionResult QueryEventVisitors([FromQuery] EventVisitorQueryDto parm)
        {
            return SUCCESS(_EventVisitorService.GetList(parm));
        }

        [HttpGet("{visitorId}")]
        [ActionPermissionFilter(Permission = "eventvisitor:query")]
        public IActionResult GetEventVisitor(long visitorId)
        {
            var response = _EventVisitorService.GetInfo(visitorId);
            return SUCCESS(response.Adapt<EventVisitorDto>());
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "eventvisitor:add")]
        [Log(Title = "Event Visitor", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventVisitor([FromBody] EventVisitorDto parm)
        {
            var modal = parm.Adapt<EventVisitor>().ToCreate(HttpContext);
            return SUCCESS(_EventVisitorService.AddEventVisitor(modal));
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "eventvisitor:edit")]
        [Log(Title = "Event Visitor", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventVisitor([FromBody] EventVisitorDto parm)
        {
            var modal = parm.Adapt<EventVisitor>().ToUpdate(HttpContext);
            return ToResponse(_EventVisitorService.UpdateEventVisitor(modal));
        }

        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "eventvisitor:delete")]
        [Log(Title = "Event Visitor", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventVisitor([FromRoute] string ids)
        {
            return ToResponse(_EventVisitorService.Delete(Tools.SplitAndConvert<long>(ids), "Delete Event Visitor"));
        }

        /// <summary>
        /// Get visitors by event ID
        /// </summary>
        [HttpGet("byEvent/{eventId}")]
        [ActionPermissionFilter(Permission = "eventvisitor:list")]
        public IActionResult GetVisitorsByEvent(long eventId)
        {
            return SUCCESS(_EventVisitorService.GetVisitorsByEventId(eventId));
        }

        /// <summary>
        /// Check in visitor
        /// </summary>
        [HttpPost("checkIn")]
        [ActionPermissionFilter(Permission = "eventvisitor:edit")]
        [Log(Title = "Visitor Check-In", BusinessType = BusinessType.UPDATE)]
        public IActionResult CheckInVisitor([FromBody] CheckInVisitorDto dto)
        {
            return ToResponse(_EventVisitorService.CheckInVisitor(dto.VisitorId, dto.GateId));
        }

        /// <summary>
        /// Check out visitor
        /// </summary>
        [HttpPost("checkOut")]
        [ActionPermissionFilter(Permission = "eventvisitor:edit")]
        [Log(Title = "Visitor Check-Out", BusinessType = BusinessType.UPDATE)]
        public IActionResult CheckOutVisitor([FromBody] CheckOutVisitorDto dto)
        {
            return ToResponse(_EventVisitorService.CheckOutVisitor(dto.VisitorId, dto.GateId));
        }

        /// <summary>
        /// Get visitor by QR code
        /// </summary>
        [HttpGet("byQRCode/{qrCode}")]
        [ActionPermissionFilter(Permission = "eventvisitor:query")]
        public IActionResult GetVisitorByQRCode(string qrCode)
        {
            return SUCCESS(_EventVisitorService.GetVisitorByQRCode(qrCode));
        }
    }

    #endregion




}

