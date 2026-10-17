using Infrastructure;
using NLog;
using Quartz;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZR.Common;
using ZR.Model.System;
using ZR.ServiceCore.Services;

namespace ZR.Tasks
{
    public class JobBase
    {
        /// <summary>
        /// Log interface
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Execute specified task
        /// </summary>
        /// <param name="context">Job context</param>
        /// <param name="job">Business logic method</param>
        public async Task<SysTasksLog> ExecuteJob(IJobExecutionContext context, Func<Task> job)
        {
            double elapsed = 0;
            int status = 0;
            string logMsg;
            try
            {
                //var s = context.Trigger.Key.Name;
                //Record Job time
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                //Execute task
                await job();
                stopwatch.Stop();
                elapsed = stopwatch.Elapsed.TotalMilliseconds;
                logMsg = "success";
            }
            catch (Exception ex)
            {
                JobExecutionException e2 = new(ex)
                {
                    //true means immediately re-execute task 
                    RefireImmediately = true
                };
                status = 1;
                logMsg = $"Job Run Fail，Exception：{ex.Message}";
                WxNoticeHelper.SendMsg("Task execution error", logMsg);
            }

            var logModel = new SysTasksLog()
            {
                Elapsed = elapsed,
                Status = status.ToString(),
                JobMessage = logMsg
            };

            await RecordTaskLog(context, logModel);
            return logModel;
        }

        /// <summary>
        /// Execute specified task (receive return result)
        /// </summary>
        /// <param name="context">Job context</param>
        /// <param name="job">Business logic method</param>
        public async Task<SysTasksLog> ExecuteJob(IJobExecutionContext context, Func<Task<string>> job)
        {
            double elapsed = 0;
            int status = 0;
            string logMsg;
            try
            {
                //var s = context.Trigger.Key.Name;
                //Record Job time
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                //Execute task and return result
                string result = await job();
                stopwatch.Stop();
                elapsed = stopwatch.Elapsed.TotalMilliseconds;
                logMsg = result.Length <= 2000 ? result : result.Substring(0, 2000);
            }
            catch (Exception ex)
            {
                JobExecutionException e2 = new(ex)
                {
                    //true means immediately re-execute task 
                    RefireImmediately = true
                };
                status = 1;
                logMsg = $"Job Run Fail，Exception：{ex.Message}";
                WxNoticeHelper.SendMsg("Task execution error", logMsg);
            }

            var logModel = new SysTasksLog()
            {
                Elapsed = elapsed,
                Status = status.ToString(),
                JobMessage = logMsg
            };

            await RecordTaskLog(context, logModel);
            return logModel;
        }

        /// <summary>
        /// Record to log
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logModel"></param>
        protected async Task RecordTaskLog(IJobExecutionContext context, SysTasksLog logModel)
        {
            var tasksLogService = (ISysTasksLogService)App.GetRequiredService(typeof(ISysTasksLogService));
            var taskQzService = (ISysTasksQzService)App.GetRequiredService(typeof(ISysTasksQzService));

            // 可以直接获取 JobDetail 的值
            IJobDetail job = context.JobDetail;

            logModel.InvokeTarget = job.JobType.FullName;
            logModel = await tasksLogService.AddTaskLog(job.Key.Name, logModel);
            //Increment execution count after success
            if (logModel.Status == "0")
            {
                await taskQzService.UpdateAsync(f => new SysTasks()
                {
                    RunTimes = f.RunTimes + 1,
                    LastRunTime = DateTime.Now
                }, f => f.ID == job.Key.Name);
            }
            logger.Info($"Execute task [{job.Key.Name}|{logModel.JobName}] result={logModel.JobMessage}");
        }
    }
}

