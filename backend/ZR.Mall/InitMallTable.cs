using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar.IOC;
using ZR.Mall.Model;

namespace ZR.Mall
{
    /// <summary>
    /// ????
    /// </summary>
    public static class InitMallTable
    {



        public static void InitDb(this IServiceCollection services, IWebHostEnvironment environment)
        {
            var db = DbScoped.SugarScope.GetConnection("1");
            var options = App.OptionsSetting;
            
            if (!options.InitDb) return;

            if (environment.IsDevelopment())
            {
                db.CodeFirst.InitTables(typeof(Product));
                db.CodeFirst.InitTables(typeof(ProductSpec));
                db.CodeFirst.InitTables(typeof(Skus));
                db.CodeFirst.InitTables(typeof(Category));
                db.CodeFirst.InitTables(typeof(Brand));
                db.CodeFirst.InitTables(typeof(OMSOrder));
                db.CodeFirst.InitTables(typeof(OMSOrderItem));
                db.CodeFirst.InitTables(typeof(MMSUserAddress));
            }
        }
    }
}

