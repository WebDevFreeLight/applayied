using Infrastructure.Attribute;
using Quartz;
using SqlSugar.IOC;
using System.Threading.Tasks;
using ZR.Model.System;

namespace ZR.Tasks.TaskScheduler
{
    /// <summary>
    /// Scheduled task test
    /// After using the following registration, no need to register again in TaskExtensions
    /// </summary>
    [AppService(ServiceType = typeof(Job_SyncTest), ServiceLifetime = LifeTime.Scoped)]
    public class Job_SyncTest : JobBase, IJob
    {
        //private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteJob(context, Run);            
        }

        /// <summary>
        /// Note when using tasks: All methods need to use async, and cannot miss await
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            await Task.Delay(1);
            //TODO Business logic
            var db = DbScoped.SugarScope;
            var info = await db.Queryable<SysDept>().FirstAsync();

            //Other database operations
            //var db2 = DbScoped.SugarScope.GetConnectionScope(2);
            System.Console.WriteLine("job test");
        }
    }
}

