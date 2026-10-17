using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.HR.Hrinsurancetype.Dto;
using ZR.Model.HR.Hrinsurancetype;
using ZR.Repository;
using ZR.Service.HR.Hrinsurancetype.IHrinsurancetypeService;

namespace ZR.Service.HR.Hrinsurancetype
{
    /// <summary>
    /// employeeependentrelationshipService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrEmployeeDependentRelationshipService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeDependentRelationshipService : BaseService<HrEmployeeDependentRelationship>, IHrEmployeeDependentRelationshipService
    {
        /// <summary>
        /// 查询employeeependentrelationship列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrEmployeeDependentRelationshipDto> GetList(HrEmployeeDependentRelationshipQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Dependentid asc")
                .Where(predicate.ToExpression())
                .ToPage<HrEmployeeDependentRelationship, HrEmployeeDependentRelationshipDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Dependentid"></param>
        /// <returns></returns>
        public HrEmployeeDependentRelationship GetInfo(long Dependentid)
        {
            var response = Queryable()
                .Where(x => x.Dependentid == Dependentid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加employeeependentrelationship
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrEmployeeDependentRelationship AddHrEmployeeDependentRelationship(HrEmployeeDependentRelationship model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改employeeependentrelationship
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrEmployeeDependentRelationship(HrEmployeeDependentRelationship model)
        {
            return Update(model, true, "修改employeeependentrelationship");
        }

        /// <summary>
        /// 清空employeeependentrelationship
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrEmployeeDependentRelationship()
        {
            var newTableName = $"hr_employee_dependent_relationship_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_employee_dependent_relationship", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入employeeependentrelationship
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrEmployeeDependentRelationship(List<HrEmployeeDependentRelationship> list)
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
        /// 导出employeeependentrelationship
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrEmployeeDependentRelationshipDto> ExportList(HrEmployeeDependentRelationshipQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrEmployeeDependentRelationshipDto()
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
        private static Expressionable<HrEmployeeDependentRelationship> QueryExp(HrEmployeeDependentRelationshipQueryDto parm)
        {
            var predicate = Expressionable.Create<HrEmployeeDependentRelationship>();

            predicate = predicate.AndIF(parm.Dependentid != null, it => it.Dependentid == parm.Dependentid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));


            return predicate;
        }
    }
}
