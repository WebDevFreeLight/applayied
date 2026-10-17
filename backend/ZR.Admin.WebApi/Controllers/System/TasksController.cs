using Microsoft.AspNetCore.Mvc;
using Quartz;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.System.Dto;

using ZR.Tasks;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Scheduled Tasks
    /// </summary>
    [Route("system/Tasks")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class TasksController : BaseController
    {
        private ISysTasksQzService _tasksQzService;
        private ITaskSchedulerServer _schedulerServer;

        public TasksController(
            ISysTasksQzService sysTasksQzService,
            ITaskSchedulerServer taskScheduler)
        {
            _tasksQzService = sysTasksQzService;
            _schedulerServer = taskScheduler;
        }

        /// <summary>
        /// Query scheduled task list
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "monitor:job:list")]
        public IActionResult ListTask([FromQuery] TasksQueryDto parm)
        {
            var response = _tasksQzService.SelectTaskList(parm);
            return SUCCESS(response, TIME_FORMAT_FULL);
        }

        /// <summary>
        /// Query single scheduled task
        /// </summary>
        /// <param name="id">Code</param>
        /// <returns></returns>
        [HttpGet("get")]
        public IActionResult Get(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return SUCCESS(_tasksQzService.GetId(id));
            }
            return SUCCESS(0);
        }

        /// <summary>
        /// Add task
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        [ActionPermissionFilter(Permission = "monitor:job:add")]
        [Log(Title = "Add Task", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] TasksCreateDto parm)
        {
            //Check if it already exists
            if (_tasksQzService.Any(m => m.Name == parm.Name))
            {
                throw new CustomException($"Add {parm.Name} failed, this task exists and cannot be duplicated!");
            }
            if (!string.IsNullOrEmpty(parm.Cron) && !CronExpression.IsValidExpression(parm.Cron))
            {
                throw new CustomException($"Cron expression is incorrect");
            }
            if (string.IsNullOrEmpty(parm.ApiUrl) && parm.TaskType == 2)
            {
                throw new CustomException($"Address cannot be empty");
            }
            if (string.IsNullOrEmpty(parm.SqlText) && parm.TaskType == 3)
            {
                throw new CustomException($"SQL statement cannot be empty");
            }
            if (parm.TaskType == 1 && (parm.AssemblyName.IsEmpty() || parm.ClassName.IsEmpty()))
            {
                throw new CustomException($"Assembly or class name cannot be empty");
            }
            //Map from Dto to entity
            var tasksQz = parm.Adapt<SysTasks>().ToCreate(HttpContext);
            tasksQz.Create_by = HttpContext.GetName();
            tasksQz.ID = SnowFlakeSingle.Instance.NextId().ToString();

            return SUCCESS(_tasksQzService.AddTasks(tasksQz));
        }

        /// <summary>
        /// Update task
        /// </summary>
        /// <returns></returns>
        [HttpPost("update")]
        [ActionPermissionFilter(Permission = "monitor:job:edit")]
        [Log(Title = "Modify Task", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Update([FromBody] TasksCreateDto parm)
        {
            //Check if it already exists
            if (_tasksQzService.Any(m => m.Name == parm.Name && m.ID != parm.ID))
            {
                throw new CustomException($"Update {parm.Name} failed, this task exists and cannot be duplicated!");
            }
            if (string.IsNullOrEmpty(parm.Cron) && parm.TriggerType == 1)
            {
                throw new CustomException($"In trigger Cron mode, runtime expression must be filled");
            }
            if (!string.IsNullOrEmpty(parm.Cron) && !CronExpression.IsValidExpression(parm.Cron))
            {
                throw new CustomException($"Cron expression is incorrect");
            }
            var tasksQz = _tasksQzService.GetFirst(m => m.ID == parm.ID);
            if (string.IsNullOrEmpty(parm.ApiUrl) && parm.TaskType == 2)
            {
                throw new CustomException($"API address cannot be empty");
            }

            if (tasksQz.IsStart == 1)
            {
                throw new CustomException($"This task is running, please stop it first before updating");
            }
            var model = parm.Adapt<SysTasks>();
            model.Update_by = HttpContextExtension.GetName(HttpContext);
            int response = _tasksQzService.UpdateTasks(model);
            if (response > 0)
            {
                var respon = await _schedulerServer.UpdateTaskScheduleAsync(tasksQz);
            }

            return SUCCESS(response);
        }

        /// <summary>
        /// Delete task
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete")]
        [ActionPermissionFilter(Permission = "monitor:job:delete")]
        [Log(Title = "Delete Task", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new CustomException("Delete task Id cannot be empty");
            }

            if (!_tasksQzService.Any(m => m.ID == id))
            {
                throw new CustomException("Task does not exist");
            }

            var tasksQz = _tasksQzService.GetFirst(m => m.ID == id);
            var taskResult = await _schedulerServer.DeleteTaskScheduleAsync(tasksQz);

            if (taskResult.IsSuccess())
            {
                _tasksQzService.Delete(id);
            }
            return ToResponse(taskResult);
        }

        /// <summary>
        /// Start task
        /// </summary>
        /// <returns></returns>
        [HttpGet("start")]
        [ActionPermissionFilter(Permission = "monitor:job:start")]
        [Log(Title = "Start Task", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> Start(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new CustomException("Start task Id cannot be empty");
            }

            if (!_tasksQzService.Any(m => m.ID == id))
            {
                throw new CustomException("Task does not exist");
            }

            var tasksQz = _tasksQzService.GetFirst(m => m.ID == id);
            var taskResult = await _schedulerServer.AddTaskScheduleAsync(tasksQz);

            if (taskResult.IsSuccess())
            {
                tasksQz.IsStart = 1;
                _tasksQzService.Update(tasksQz);
            }

            return ToResponse(taskResult);
        }

        /// <summary>
        /// Stop task
        /// </summary>
        /// <returns></returns>
        [HttpGet("stop")]
        [ActionPermissionFilter(Permission = "monitor:job:stop")]
        [Log(Title = "Stop Task", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> Stop(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new CustomException("Stop task Id cannot be empty");
            }

            if (!_tasksQzService.Any(m => m.ID == id))
            {
                throw new CustomException("Task does not exist");
            }

            var tasksQz = _tasksQzService.GetFirst(m => m.ID == id);
            var taskResult = await _schedulerServer.DeleteTaskScheduleAsync(tasksQz);//await _schedulerServer.PauseTaskScheduleAsync(tasksQz);

            if (taskResult.IsSuccess())
            {
                tasksQz.IsStart = 0;
                _tasksQzService.Update(tasksQz);
            }

            return ToResponse(taskResult);
        }

        /// <summary>
        /// Execute scheduled task immediately once
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("run")]
        [ActionPermissionFilter(Permission = "monitor:job:run")]
        [Log(Title = "Execute Task", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> Run(string id)
        {
            var result = await _tasksQzService.IsAnyAsync(m => m.ID == id);
            if (!result)
            {
                throw new CustomException("Task does not exist");
            }
            var tasksQz = await _tasksQzService.GetFirstAsync(m => m.ID == id);
            var taskResult = await _schedulerServer.RunTaskScheduleAsync(tasksQz);

            return ToResponse(taskResult);
        }

        /// <summary>
        /// Scheduled task export
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "Scheduled Task Export")]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "monitor:job:export")]
        public IActionResult Export()
        {
            var list = _tasksQzService.GetAll();

            string sFileName = ExportExcel(list, "monitorjob", "Scheduled Tasks");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }
    }
}

