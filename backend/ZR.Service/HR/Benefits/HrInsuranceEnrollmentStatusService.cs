using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Hr.Dto;
using ZR.Model.Hr;
using ZR.Repository;
using ZR.Service.Hr.IHrService;

namespace ZR.Service.Hr
{
    /// <summary>
    /// hrinsuranceenrollmentstatusService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrInsuranceEnrollmentStatusService), ServiceLifetime = LifeTime.Transient)]
    public class HrInsuranceEnrollmentStatusService : BaseService<HrInsuranceEnrollmentStatus>, IHrInsuranceEnrollmentStatusService
    {
        /// <summary>
        /// 查询hrinsuranceenrollmentstatus列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrInsuranceEnrollmentStatusDto> GetList(HrInsuranceEnrollmentStatusQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<HrInsuranceEnrollmentStatus, HrInsuranceEnrollmentStatusDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public HrInsuranceEnrollmentStatus GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加hrinsuranceenrollmentstatus
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrInsuranceEnrollmentStatus AddHrInsuranceEnrollmentStatus(HrInsuranceEnrollmentStatus model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改hrinsuranceenrollmentstatus
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrInsuranceEnrollmentStatus(HrInsuranceEnrollmentStatus model)
        {
            return Update(model, true, "修改hrinsuranceenrollmentstatus");
        }

        /// <summary>
        /// 清空hrinsuranceenrollmentstatus
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrInsuranceEnrollmentStatus()
        {
            var newTableName = $"hr_insurance_enrollment_status_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_insurance_enrollment_status", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入hrinsuranceenrollmentstatus
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrInsuranceEnrollmentStatus(List<HrInsuranceEnrollmentStatus> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "Name不能为空")
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
        /// 导出hrinsuranceenrollmentstatus
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrInsuranceEnrollmentStatusDto> ExportList(HrInsuranceEnrollmentStatusQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrInsuranceEnrollmentStatusDto()
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
        private static Expressionable<HrInsuranceEnrollmentStatus> QueryExp(HrInsuranceEnrollmentStatusQueryDto parm)
        {
            var predicate = Expressionable.Create<HrInsuranceEnrollmentStatus>();

            predicate = predicate.AndIF(parm.Id != null, it => it.Id == parm.Id);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time  >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
