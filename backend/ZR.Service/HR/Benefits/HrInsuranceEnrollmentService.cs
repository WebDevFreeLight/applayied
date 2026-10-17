using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Hr.Dto;
using ZR.Model.Hr;
using ZR.Repository;
using ZR.Service.Hr.IHrService;

namespace ZR.Service.Hr
{
    /// <summary>
    /// insuranceenrollmentService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrInsuranceEnrollmentService), ServiceLifetime = LifeTime.Transient)]
    public class HrInsuranceEnrollmentService : BaseService<HrInsuranceEnrollment>, IHrInsuranceEnrollmentService
    {
        /// <summary>
        /// 查询insuranceenrollment列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrInsuranceEnrollmentDto> GetList(HrInsuranceEnrollmentQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Enrollmentid asc")
                .Where(predicate.ToExpression())
                .ToPage<HrInsuranceEnrollment, HrInsuranceEnrollmentDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Enrollmentid"></param>
        /// <returns></returns>
        public HrInsuranceEnrollment GetInfo(long Enrollmentid)
        {
            var response = Queryable()
                .Where(x => x.Enrollmentid == Enrollmentid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加insuranceenrollment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrInsuranceEnrollment AddHrInsuranceEnrollment(HrInsuranceEnrollment model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改insuranceenrollment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrInsuranceEnrollment(HrInsuranceEnrollment model)
        {
            return Update(model, true, "修改insuranceenrollment");
        }

        /// <summary>
        /// 清空insuranceenrollment
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrInsuranceEnrollment()
        {
            var newTableName = $"hr_insurance_enrollment_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_insurance_enrollment", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入insuranceenrollment
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrInsuranceEnrollment(List<HrInsuranceEnrollment> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Employeeid.IsEmpty(), "معرف الموظف不能为空")
                .SplitError(x => x.Item.Planid.IsEmpty(), "معرف الخطة不能为空")
                .SplitError(x => x.Item.Enrollmentdate.IsEmpty(), "تاريخ التسجيل不能为空")
                .SplitError(x => x.Item.Coverageamount.IsEmpty(), "مبلغ التغطية不能为空")
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
        /// 导出insuranceenrollment
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrInsuranceEnrollmentDto> ExportList(HrInsuranceEnrollmentQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrInsuranceEnrollmentDto()
                {
                    EmployeeidLabel = it.Employeeid.GetConfigValue<Model.System.SysDictData>("sql_hremployee"),
                    PlanidLabel = it.Planid.GetConfigValue<Model.System.SysDictData>("sql_hr_insurance_plan"),
                    StatusLabel = it.Status.GetConfigValue<Model.System.SysDictData>("sql_hr_insurance_enrollment_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<HrInsuranceEnrollment> QueryExp(HrInsuranceEnrollmentQueryDto parm)
        {
            var predicate = Expressionable.Create<HrInsuranceEnrollment>();

            predicate = predicate.AndIF(parm.Employeeid != null, it => it.Employeeid == parm.Employeeid);
            predicate = predicate.AndIF(parm.Planid != null, it => it.Planid == parm.Planid);
            predicate = predicate.AndIF(parm.BeginEnrollmentdate != null, it => it.Enrollmentdate >= parm.BeginEnrollmentdate);
            predicate = predicate.AndIF(parm.EndEnrollmentdate != null, it => it.Enrollmentdate <= parm.EndEnrollmentdate);
            predicate = predicate.AndIF(parm.BeginTerminationdate != null, it => it.Terminationdate >= parm.BeginTerminationdate);
            predicate = predicate.AndIF(parm.EndTerminationdate != null, it => it.Terminationdate <= parm.EndTerminationdate);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);



            return predicate;
        }
    }
}
