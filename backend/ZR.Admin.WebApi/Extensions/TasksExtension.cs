using Quartz.Spi;
using SqlSugar;
using SqlSugar.IOC;
using ZR.Model.System;
using ZR.Tasks;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// ????????
    /// </summary>
    public static class TasksExtension
    {
        /// <summary>
        /// ????
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddTaskSchedulers(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            //??Quartz??
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddTransient<ITaskSchedulerServer, TaskSchedulerServer>();
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAddTaskSchedulers(this IApplicationBuilder app)
        {
            ITaskSchedulerServer _schedulerServer = app.ApplicationServices.GetRequiredService<ITaskSchedulerServer>();


            try
            {


                var tasks = DbScoped.SugarScope.Queryable<SysTasks>()
                            .Where(m => m.IsStart == 1).ToListAsync();

                //?????????????
                foreach (var task in tasks.Result)
                {
                    var result = _schedulerServer.AddTaskScheduleAsync(task);
                    if (result.Result.IsSuccess())
                    {
                        Console.WriteLine($"????[{task.Name}]ID:{task.ID}??");
                    }
                }


            }
            catch (Exception ex)
            {




            }
       



            return app;
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInit(this IApplicationBuilder app)
        {
            var mainDb = App.Configuration["mainDb"];




            var tenantId = App.GetCurrentTenantId();
            Console.WriteLine("???????...");
            //var db = DbScoped.SugarScope.GetConnection(mainDb);

            try
            {

                var db = DbScoped.SugarScope;

                var types = db.Queryable<SysDictType>()
                    .Where(it => it.Status == "0")
                    .Select(it => it.DictType)
                    .ToList();

                //?????????Any??,????????????
                if (!db.ConfigQuery.Any())
                {
                    foreach (var type in types)
                    {



                        db.ConfigQuery.SetTable<SysDictData>(it => SqlFunc.ToString(it.DictValue), it => it.DictLabel, type, it => it.DictType == type);
                    }
                }


            }
            catch (Exception ex)
            {
                 

                Console.WriteLine("errors :" + ex.Message);
            }
            
            



            return app;
        }
    }
}

