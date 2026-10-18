using Microsoft.AspNetCore.Mvc;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// PMO employee directory: list/view/create/update/delete employees and link them to portal users.
    /// Viewing is PM+, mutations are admin-only (employee records are an administration concern).
    /// </summary>
    [Route("pmo/employee")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoEmployeeController : PmoBaseController
    {
        private readonly IPmoEmployeeService _service;
        public PmoEmployeeController(IPmoEmployeeService service) => _service = service;

        /// <summary>Paged employee directory (keyword over code, active filter).</summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "pmo:employee:list")]
        public IActionResult List([FromQuery] PmoEmployeeQueryDto parm) => SUCCESS(_service.GetList(parm));

        /// <summary>Single employee.</summary>
        [HttpGet("{employeeId}")]
        [ActionPermissionFilter(Permission = "pmo:employee:list")]
        public IActionResult Get(long employeeId) => SUCCESS(_service.GetInfo(employeeId));

        /// <summary>Portal users not yet linked to an employee (for the link dropdown).</summary>
        [HttpGet("unlinked-users")]
        [ActionPermissionFilter(Permission = "pmo:employee:list")]
        public IActionResult UnlinkedUsers([FromQuery] string search = null) => SUCCESS(_service.UnlinkedUsers(search));

        /// <summary>Create an employee.</summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "pmo:employee:add")]
        public IActionResult Add([FromBody] PmoEmployeeInputDto dto) => SUCCESS(_service.AddEmployee(dto, CurrentActor.UserName).ToString());

        /// <summary>Update an employee.</summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "pmo:employee:edit")]
        public IActionResult Update([FromBody] PmoEmployeeInputDto dto) => SUCCESS(_service.UpdateEmployee(dto, CurrentActor.UserName));

        /// <summary>Delete an employee.</summary>
        [HttpDelete("{employeeId}")]
        [ActionPermissionFilter(Permission = "pmo:employee:remove")]
        public IActionResult Remove(long employeeId) => SUCCESS(_service.RemoveEmployee(employeeId));
    }
}
