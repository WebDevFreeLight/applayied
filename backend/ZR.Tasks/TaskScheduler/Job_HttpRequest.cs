using Infrastructure;
using Infrastructure.Attribute;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using SqlSugar.IOC;
using System;
using System.Threading.Tasks;
using ZR.Model.System;

namespace ZR.Tasks.TaskScheduler
{
    /// <summary>
    /// Scheduled task HTTP request
    /// </summary>
    [AppService(ServiceType = typeof(Job_HttpRequest), ServiceLifetime = LifeTime.Scoped)]
    internal class Job_HttpRequest : JobBase, IJob
    {
        //private readonly ISysTasksQzService tasksQzService;
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //public Job_HttpRequest(ISysTasksQzService tasksQzService)
        //{
        //    this.tasksQzService = tasksQzService;
        //}
        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteJob(context, async () => await Run(context));
        }
        public async Task Run(IJobExecutionContext context)
        {
            AbstractTrigger trigger = (context as JobExecutionContextImpl).Trigger as AbstractTrigger;
            //var info = await tasksQzService.CopyNew().GetByIdAsync(trigger.JobName);
            var info = await DbScoped.SugarScope.CopyNew()
                .Queryable<SysTasks>()
                .FirstAsync(f => f.ID == trigger.JobName) ?? throw new CustomException($"Task {trigger?.JobName} network request execution failed, task does not exist");
            string result;
            if (info.RequestMethod != null && info.RequestMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                result = await HttpHelper.HttpPostAsync(info.ApiUrl, info.JobParams);
            }
            else
            {
                var url = info.ApiUrl;
                if (url.IndexOf("?") > -1)
                {
                    url += "&" + info.JobParams;
                }
                else
                {
                    url += "?" + info.JobParams;
                }
                result = await HttpHelper.HttpGetAsync(url);
            }

            logger.Info($"Task [{info.Name}] network request execution result=" + result);
        }
    }
}

