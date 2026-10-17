using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;
using ZR.Repository;
using ZR.Service.HR.Business.IBusinessService;

namespace ZR.Service.HR.Business
{
    /// <summary>
    /// Insurance Plans TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrInsurancePlanService), ServiceLifetime = LifeTime.Transient)]
    public class HrInsurancePlanService : BaseService<HrInsurancePlan>, IHrInsurancePlanService
    {
        /// <summary>
        /// 查询Insurance Plans Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrInsurancePlanDto> GetList(HrInsurancePlanQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Planid asc")
                .Where(predicate.ToExpression())
                .ToPage<HrInsurancePlan, HrInsurancePlanDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Planid"></param>
        /// <returns></returns>
        public HrInsurancePlan GetInfo(long Planid)
        {
            var response = Queryable()
                .Where(x => x.Planid == Planid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Insurance Plans Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrInsurancePlan AddHrInsurancePlan(HrInsurancePlan model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Insurance Plans Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrInsurancePlan(HrInsurancePlan model)
        {
            return Update(model, true, "修改Insurance Plans Table");
        }

        /// <summary>
        /// 清空Insurance Plans Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrInsurancePlan()
        {
            var newTableName = $"hr_insurance_plan_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_insurance_plan", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Insurance Plans Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrInsurancePlan(List<HrInsurancePlan> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Planname.IsEmpty(), "اسم الخطة不能为空")
                .SplitError(x => x.Item.Provider.IsEmpty(), "مزود不能为空")
                .SplitError(x => x.Item.Plantype.IsEmpty(), "نوع الخطة不能为空")
                .SplitError(x => x.Item.Coveragelevel.IsEmpty(), "مستوى التغطية不能为空")
                //.WhereColumns(it => it.UserName)//如果不是主键可以这样实现（多字段it=>new{it.x1,it.x2}）
                .ToStorage();
            var result = x.AsInsertable.ExecuteCommand();//插入可插入部分;

            string msg = $"插入{x.InsertList.Count} 更新{x.UpdateList.Count} 错误数据{x.ErrorList.Count} 不计算数据{x.IgnoreList.Count} 删除数据{x.DeleteList.Count} 总共{x.TotalList.Count}";                    
            Console.WriteLine(msg);

            //输出错误信息               
            foreach (var item in x.ErrorList)
            {
                Console.WriteLine("错误" + item.StorageMessage);
            }
            foreach (var item in x.IgnoreList)
            {
                Console.WriteLine("忽略" + item.StorageMessage);
            }

            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// 导出Insurance Plans Table
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrInsurancePlanDto> ExportList(HrInsurancePlanQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrInsurancePlanDto()
                {
                    PlantypeLabel = it.Plantype.GetConfigValue<Model.System.SysDictData>("sql_hr_insurance_plan_ype"),
                    CoveragelevelLabel = it.Coveragelevel.GetConfigValue<Model.System.SysDictData>("sql_hr_insurance_coveragelevel"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<HrInsurancePlan> QueryExp(HrInsurancePlanQueryDto parm)
        {
            var predicate = Expressionable.Create<HrInsurancePlan>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Planname), it => it.Planname.Contains(parm.Planname));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Provider), it => it.Provider.Contains(parm.Provider));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Plantype), it => it.Plantype == parm.Plantype);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Coveragelevel), it => it.Coveragelevel == parm.Coveragelevel);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Coveragedetails), it => it.Coveragedetails.Contains(parm.Coveragedetails));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            return predicate;
        }
    }
}
