using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    #region Event Tenant Controller

    /// <summary>
    /// Event Tenant - ?????????? 0
    /// API Controller
    /// </summary>
    [Route("event/EventTenant")]
    public class EventTenantController : BaseController
    {
        private readonly IEventTenantService _EventTenantService;

        public EventTenantController(IEventTenantService eventTenantService)
        {
            _EventTenantService = eventTenantService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "eventtenant:list")]
        public IActionResult QueryEventTenants([FromQuery] EventTenantQueryDto parm)
        {
            return SUCCESS(_EventTenantService.GetList(parm));
        }

        [HttpGet("{tenantId}")]
        [ActionPermissionFilter(Permission = "eventtenant:query")]
        public IActionResult GetEventTenant(long tenantId)
        {
            var response = _EventTenantService.GetInfo(tenantId);
            return SUCCESS(response.Adapt<EventTenantDto>());
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "eventtenant:add")]
        [Log(Title = "Event Tenant", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventTenant([FromBody] EventTenantDto parm)
        {
            var modal = parm.Adapt<EventTenant>().ToCreate(HttpContext);
            return SUCCESS(_EventTenantService.AddEventTenant(modal));
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "eventtenant:edit")]
        [Log(Title = "Event Tenant", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventTenant([FromBody] EventTenantDto parm)
        {
            var modal = parm.Adapt<EventTenant>().ToUpdate(HttpContext);
            return ToResponse(_EventTenantService.UpdateEventTenant(modal));
        }

        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "eventtenant:delete")]
        [Log(Title = "Event Tenant", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventTenant([FromRoute] string ids)
        {
            return ToResponse(_EventTenantService.Delete(Tools.SplitAndConvert<long>(ids), "Delete Event Tenant"));
        }

        [Log(Title = "Event Tenant", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "eventtenant:export")]
        public IActionResult Export([FromQuery] EventTenantQueryDto parm)
        {
            var list = _EventTenantService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Event Tenant", "Event Tenant");
            return ExportExcel(result.Item2, result.Item1);
        }

        [HttpPost("importData")]
        [Log(Title = "Event Tenant Import", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "eventtenant:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventTenantDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventTenantDto>(startCell: "A1").ToList();
            }
            return SUCCESS(_EventTenantService.ImportEventTenant(list.Adapt<List<EventTenant>>()));
        }
    }

    #endregion

}

