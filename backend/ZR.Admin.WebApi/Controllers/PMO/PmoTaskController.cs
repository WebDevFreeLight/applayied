using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Delivery tasks — list/view (scoped), PM create/edit, and the contributor progress update.
    /// </summary>
    [Route("pmo/task")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoTaskController : PmoBaseController
    {
        private readonly IPmoTaskService _taskService;

        public PmoTaskController(IPmoTaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>List tasks (scoped to the caller), with search/filter/paging.</summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "pmo:task:list")]
        public IActionResult List([FromQuery] PmoTaskQueryDto parm)
        {
            var page = _taskService.GetList(parm, CurrentActor);
            return SUCCESS(page);
        }

        /// <summary>Task detail; enforces read scope for contributors.</summary>
        [HttpGet("{taskId}")]
        [ActionPermissionFilter(Permission = "pmo:task:query")]
        public IActionResult Get(long taskId)
        {
            return SUCCESS(_taskService.GetInfo(taskId, CurrentActor));
        }

        /// <summary>Create a task (Project Manager or Administrator).</summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "pmo:task:add")]
        [Log(Title = "PMO Task", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] PmoTaskInputDto dto)
        {
            return SUCCESS(_taskService.AddTask(dto, CurrentActor));
        }

        /// <summary>Update a task (Project Manager or Administrator).</summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "pmo:task:edit")]
        [Log(Title = "PMO Task", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] PmoTaskInputDto dto)
        {
            return ToResponse(_taskService.UpdateTask(dto, CurrentActor));
        }

        /// <summary>Update progress on an assigned task (contributor path; service enforces own-task rule).</summary>
        [HttpPut("{taskId}/progress")]
        [ActionPermissionFilter(Permission = "pmo:task:progress")]
        [Log(Title = "PMO Task Progress", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateProgress(long taskId, [FromBody] PmoTaskProgressDto dto)
        {
            dto.TaskId = taskId;
            return ToResponse(_taskService.UpdateProgress(dto, CurrentActor));
        }
    }
}
