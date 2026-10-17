using Infrastructure.Model;
using NLog;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ZR.Model.System;

namespace ZR.Tasks
{
    /// <summary>
    /// Scheduled task center
    /// </summary>
    //[AppService]
    public class TaskSchedulerServer : ITaskSchedulerServer
    {
        private Task<IScheduler> _scheduler;
        private readonly IJobFactory _jobFactory;
        /// <summary>
        /// Logging interface
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TaskSchedulerServer(IJobFactory jobFactory)
        {
            _scheduler = GetTaskSchedulerAsync();
            _jobFactory = jobFactory;
        }

        /// <summary>
        /// Start scheduled task
        /// Reference article: https://www.quartz-scheduler.net/documentation/quartz-3.x/configuration/reference.html#datasources-ado-net-jobstores
        /// </summary>
        /// <returns></returns>
        private Task<IScheduler> GetTaskSchedulerAsync()
        {
            if (_scheduler != null)
            {
                return _scheduler;
            }

            NameValueCollection collection = new NameValueCollection
            {
                { "quartz.serializer.type","binary" },
                //quartz parameters
                //{ "quartz.scheduler.instanceId", "AUTO" },
                //{ "quartz.serializer.type","json" },
                ////Configuration for specifying quartz persistent database
                //{ "quartz.jobStore.type","Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                //{ "quartz.jobStore.tablePrefix","QRTZ_"},
                //{ "quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.MySQLDelegate, Quartz"},
                //{ "quartz.jobStore.useProperties", "true"},
                //{ "quartz.jobStore.dataSource", "myDS" },
                //{ "quartz.dataSource.myDS.connectionString", @"server=xxx.xxx.xxx.xxx;port=3306;database=Admin;uid=zrry;pwd=********;Charset=utf8;"},
                //{ "quartz.dataSource.myDS.provider", "MySql" },
            };

            StdSchedulerFactory factory = new StdSchedulerFactory(collection);

            return _scheduler = factory.GetScheduler();
        }

        public async Task<ApiResult> StartTaskScheduleAsync()
        {
            try
            {
                _scheduler.Result.JobFactory = _jobFactory;
                if (_scheduler.Result.IsStarted)
                {
                    return ApiResult.Error(500, $"Scheduled task has already been started");
                }

                //Wait for task to complete
                await _scheduler.Result.Start();
                return ApiResult.Success("Scheduled task started successfully");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Stop scheduled task
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult> StopTaskScheduleAsync()
        {
            try
            {
                if (_scheduler.Result.IsShutdown)
                {
                    return ApiResult.Error(500, $"Scheduled task has already been stopped");
                }

                await _scheduler.Result.Shutdown();
                return ApiResult.Success("Scheduled task has been stopped");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Add a scheduled task
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<ApiResult> AddTaskScheduleAsync(SysTasks tasksQz)
        {
            try
            {
                JobKey jobKey = new JobKey(tasksQz.ID, tasksQz.JobGroup);
                if (await _scheduler.Result.CheckExists(jobKey))
                {
                    return ApiResult.Error(500, $"This scheduled task is already running: [{tasksQz.Name}], please do not add it again!");
                }
                if (tasksQz?.EndTime <= DateTime.Now)
                {
                    return ApiResult.Error(500, $"End time is earlier than current time, task will not be executed");
                }
                #region Set start time and end time

                tasksQz.BeginTime = tasksQz.BeginTime == null ? DateTime.Now : tasksQz.BeginTime;
                tasksQz.EndTime = tasksQz.EndTime == null ? DateTime.MaxValue.AddDays(-1) : tasksQz.EndTime;

                DateTimeOffset starRunTime = DateBuilder.NextGivenSecondDate(tasksQz.BeginTime, 1);//Set start time
                DateTimeOffset endRunTime = DateBuilder.NextGivenSecondDate(tasksQz.EndTime, 1);//Set pause time

                #endregion

                #region Get assembly type and class through reflection   

                Assembly assembly = Assembly.Load(new AssemblyName(tasksQz.AssemblyName));
                Type jobType = assembly.GetType(tasksQz.AssemblyName + "." + tasksQz.ClassName);

                #endregion
                //2. Start scheduler. Check if task scheduling is enabled
                if (!_scheduler.Result.IsStarted)
                {
                    await StartTaskScheduleAsync();
                }

                //3. Create task. Pass in the execution assembly obtained through reflection
                IJobDetail job = new JobDetailImpl(tasksQz.ID, tasksQz.JobGroup, jobType);
                job.JobDataMap.Add("JobParam", tasksQz.JobParams);
                ITrigger trigger;

                //4. Create a trigger
                if (tasksQz.Cron != null && CronExpression.IsValidExpression(tasksQz.Cron))
                {
                    trigger = CreateCronTrigger(tasksQz);
                    //Solution for the problem of Quartz executing immediately after startup
                    ((CronTriggerImpl)trigger).MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing;
                }
                else
                {
                    trigger = CreateSimpleTrigger(tasksQz);
                    ((SimpleTriggerImpl)trigger).MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing;
                }

                // 5. Bind trigger and task to scheduler
                await _scheduler.Result.ScheduleJob(job, trigger);
                //Task not started, pause task
                //if (!tasksQz.IsStart)
                //{
                //    _scheduler.Result.PauseJob(jobKey).Wait();
                //}
                //Reset job execution with new trigger
                await _scheduler.Result.ResumeTrigger(trigger.Key);
                return ApiResult.Success($"Start scheduled task: [{tasksQz.Name}] successfully!");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to start scheduled task, Group: {tasksQz.JobGroup}, Job: [{tasksQz.Name}], error: {ex.Message}");
                return ApiResult.Error(500, $"Start scheduled task: [{tasksQz.Name}] failed!");
            }
        }

        /// <summary>
        /// Pause specified scheduled task
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<ApiResult> PauseTaskScheduleAsync(SysTasks tasksQz)
        {
            try
            {
                JobKey jobKey = new JobKey(tasksQz.ID, tasksQz.JobGroup);
                if (await _scheduler.Result.CheckExists(jobKey))
                {
                    // Prevent data problems during creation, remove first, then execute creation operation
                    await _scheduler.Result.PauseJob(jobKey);
                }
                return ApiResult.Success($"Pause scheduled task: [{tasksQz.Name}] successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return new ApiResult(500, $"Pause scheduled task: [{tasksQz.Name}] failed, {ex.Message}");
            }
        }

        /// <summary>
        /// Resume specified scheduled task
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<ApiResult> ResumeTaskScheduleAsync(SysTasks tasksQz)
        {
            try
            {
                JobKey jobKey = new JobKey(tasksQz.ID, tasksQz.JobGroup);
                if (!await _scheduler.Result.CheckExists(jobKey))
                {
                    return ApiResult.Error(500, $"Scheduled task not found: [{tasksQz.Name}]");
                }
                await _scheduler.Result.ResumeJob(jobKey);
                return ApiResult.Success($"Resume scheduled task: [{tasksQz.Name}] successfully");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Delete specified scheduled task
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<ApiResult> DeleteTaskScheduleAsync(SysTasks tasksQz)
        {
            try
            {
                JobKey jobKey = new JobKey(tasksQz.ID, tasksQz.JobGroup);

                //await _scheduler.Result.ScheduleJob()

                await _scheduler.Result.DeleteJob(jobKey);
                return ApiResult.Success($"Delete scheduled task: [{tasksQz.Name}] successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return ApiResult.Error($"Delete scheduled task: [{tasksQz.Name}] failed, error={ex.Message}");
            }
        }

        /// <summary>
        /// Run immediately
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<ApiResult> RunTaskScheduleAsync(SysTasks tasksQz)
        {
            try
            {
                JobKey jobKey = new JobKey(tasksQz.ID, tasksQz.JobGroup);
                List<JobKey> jobKeys = _scheduler.Result.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(tasksQz.JobGroup)).Result.ToList();
                if (jobKeys == null || jobKeys.Count == 0)
                {
                    await AddTaskScheduleAsync(tasksQz);
                }

                var triggers = await _scheduler.Result.GetTriggersOfJob(jobKey);
                if (triggers.Count <= 0)
                {
                    return new ApiResult(110, $"Trigger not found [{jobKey.Name}]");
                }
                await _scheduler.Result.TriggerJob(jobKey);

                return ApiResult.Success($"Run scheduled task: [{tasksQz.Name}] successfully");
            }
            catch (Exception ex)
            {
                return new ApiResult(500, $"Execute scheduled task: [{tasksQz.Name}] failed, {ex.Message}");
            }
        }

        /// <summary>
        /// Update scheduled task
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<ApiResult> UpdateTaskScheduleAsync(SysTasks tasksQz)
        {
            try
            {
                JobKey jobKey = new JobKey(tasksQz.ID, tasksQz.JobGroup);
                if (await _scheduler.Result.CheckExists(jobKey))
                {
                    //Prevent data problems during creation, remove first, then execute creation operation
                    await _scheduler.Result.DeleteJob(jobKey);
                }
                //await AddTaskScheduleAsync(tasksQz);
                return ApiResult.Success("Plan modified successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return ApiResult.Error($"Plan modification failed, error={ex.Message}");
            }
        }

        #region Create trigger helper methods

        /// <summary>
        /// Create SimpleTrigger (simple trigger)
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        private ITrigger CreateSimpleTrigger(SysTasks tasksQz)
        {
            if (tasksQz.RunTimes > 0)
            {
                ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(tasksQz.ID, tasksQz.JobGroup)
                .StartAt(tasksQz.BeginTime.Value)
                .EndAt(tasksQz.EndTime.Value)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(tasksQz.IntervalSecond)
                .WithRepeatCount(tasksQz.RunTimes)).ForJob(tasksQz.ID, tasksQz.JobGroup).Build();
                return trigger;
            }
            else
            {
                ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(tasksQz.ID, tasksQz.JobGroup)
                .StartAt(tasksQz.BeginTime.Value)
                .EndAt(tasksQz.EndTime.Value)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(tasksQz.IntervalSecond)
                .RepeatForever()).ForJob(tasksQz.ID, tasksQz.JobGroup).Build();
                return trigger;
            }
            // Trigger job to run immediately, then repeat every 10 seconds, infinite loop
        }

        /// <summary>
        /// Create Cron type trigger
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        private ITrigger CreateCronTrigger(SysTasks tasksQz)
        {
            // Job trigger
            return TriggerBuilder.Create()
                   .WithIdentity(tasksQz.ID, tasksQz.JobGroup)
                   .StartAt(tasksQz.BeginTime.Value)//Start time
                   .EndAt(tasksQz.EndTime.Value)//End data
                   .WithCronSchedule(tasksQz.Cron)//Specify cron expression
                   .ForJob(tasksQz.ID, tasksQz.JobGroup)//Job name
                   .Build();
        }

        #endregion

    }
}

