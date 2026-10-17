using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Hr.Dto;
using ZR.Repository;
using ZR.Service.Hr.IHrService;
using ZR.Model.HR.Payroll;

namespace ZR.Service.Hr
{
    /// <summary>
    /// Salary Compliance - امتثال الرواتبService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrSalaryComplianceService), ServiceLifetime = LifeTime.Transient)]
    public class HrSalaryComplianceService : BaseService<HrSalaryCompliance>, IHrSalaryComplianceService
    {
        /// <summary>
        /// 查询Salary Compliance - امتثال الرواتب列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrSalaryComplianceDto> GetList(HrSalaryComplianceQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Complianceid asc")
                .Where(predicate.ToExpression())
                .ToPage<HrSalaryCompliance, HrSalaryComplianceDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Complianceid"></param>
        /// <returns></returns>
        public HrSalaryCompliance GetInfo(long Complianceid)
        {
            var response = Queryable()
                .Where(x => x.Complianceid == Complianceid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrSalaryCompliance AddHrSalaryCompliance(HrSalaryCompliance model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrSalaryCompliance(HrSalaryCompliance model)
        {
            return Update(model, true, "修改Salary Compliance - امتثال الرواتب");
        }

        /// <summary>
        /// 清空Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrSalaryCompliance()
        {
            var newTableName = $"hr_salary_compliance_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_salary_compliance", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrSalaryCompliance(List<HrSalaryCompliance> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Country.IsEmpty(), "Country - البلد不能为空")
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
        /// 导出Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrSalaryComplianceDto> ExportList(HrSalaryComplianceQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrSalaryComplianceDto()
                {
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<HrSalaryCompliance> QueryExp(HrSalaryComplianceQueryDto parm)
        {
            var predicate = Expressionable.Create<HrSalaryCompliance>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Country), it => it.Country.Contains(parm.Country));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Endofserviceformula), it => it.Endofserviceformula.Contains(parm.Endofserviceformula));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Vatrequirements), it => it.Vatrequirements.Contains(parm.Vatrequirements));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Procedures), it => it.Procedures.Contains(parm.Procedures));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Currencycode), it => it.Currencycode.Contains(parm.Currencycode));
            predicate = predicate.AndIF(parm.Maxworkinghoursperweek != null, it => it.Maxworkinghoursperweek == parm.Maxworkinghoursperweek);
            predicate = predicate.AndIF(parm.Annualleavedays != null, it => it.Annualleavedays == parm.Annualleavedays);
            predicate = predicate.AndIF(parm.Sickleavedays != null, it => it.Sickleavedays == parm.Sickleavedays);
            predicate = predicate.AndIF(parm.Maternityleavedays != null, it => it.Maternityleavedays == parm.Maternityleavedays);
            predicate = predicate.AndIF(parm.Paternityleavedays != null, it => it.Paternityleavedays == parm.Paternityleavedays);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Taxbrackets), it => it.Taxbrackets.Contains(parm.Taxbrackets));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Notes), it => it.Notes == parm.Notes);
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            return predicate;
        }
    }
}
