using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Business.Dto;
using ZR.Model.Business;
using ZR.Repository;
using ZR.Service.Business.IBusinessService;

namespace ZR.Service.Business
{
    /// <summary>
    /// Organizations Information TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IOrganizationtypeService), ServiceLifetime = LifeTime.Transient)]
    public class OrganizationtypeService : BaseService<Organizationtype>, IOrganizationtypeService
    {
        /// <summary>
        /// 查询Organizations Information Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<OrganizationtypeDto> GetList(OrganizationtypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Organizationtypeid asc")
                .Where(predicate.ToExpression())
                .ToPage<Organizationtype, OrganizationtypeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Organizationtypeid"></param>
        /// <returns></returns>
        public Organizationtype GetInfo(long Organizationtypeid)
        {
            var response = Queryable()
                .Where(x => x.Organizationtypeid == Organizationtypeid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Organizations Information Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Organizationtype AddOrganizationtype(Organizationtype model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Organizations Information Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateOrganizationtype(Organizationtype model)
        {
            return Update(model, true, "修改Organizations Information Table");
        }

        /// <summary>
        /// 清空Organizations Information Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateOrganizationtype()
        {
            var newTableName = $"organizationtype_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("organizationtype", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Organizations Information Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportOrganizationtype(List<Organizationtype> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Organizationtypename.IsEmpty(), "OrganizationType name不能为空")
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
        /// 导出Organizations Information Table
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<OrganizationtypeDto> ExportList(OrganizationtypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new OrganizationtypeDto()
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
        private static Expressionable<Organizationtype> QueryExp(OrganizationtypeQueryDto parm)
        {
            var predicate = Expressionable.Create<Organizationtype>();

            predicate = predicate.AndIF(parm.Organizationtypeid != null, it => it.Organizationtypeid == parm.Organizationtypeid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Organizationtypename), it => it.Organizationtypename.Contains(parm.Organizationtypename));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CreateBy), it => it.Create_by == parm.CreateBy);
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.UpdateBy), it => it.Update_by == parm.UpdateBy);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
