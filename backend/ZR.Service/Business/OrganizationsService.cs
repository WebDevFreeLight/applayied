using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Business.Dto;
using ZR.Model.Business;
using ZR.Repository;
using ZR.Service.Business.IBusinessService;
using System.Collections.Generic;

namespace ZR.Service.Business
{
    /// <summary>
    /// Organizations Information TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IOrganizationsService), ServiceLifetime = LifeTime.Transient)]
    public class OrganizationsService : BaseService<Organizations>, IOrganizationsService
    {
        /// <summary>
        /// 查询Organizations Information Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<OrganizationsDto> GetList(OrganizationsQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Organizationsid asc")
                .Where(predicate.ToExpression())
                .ToPage<Organizations, OrganizationsDto>(parm);

            return response;
        }

        /// <summary>
        /// 查询Organizations Information Table树列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public List<Organizations> GetTreeList(OrganizationsQueryDto parm)
        {
            var predicate = Expressionable.Create<Organizations>();

            predicate = predicate.AndIF(parm.Organizationsid != null, it => it.Organizationsid == parm.Organizationsid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name == parm.Name);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Organizationtypeid), it => it.Organizationtypeid == parm.Organizationtypeid);
            predicate = predicate.AndIF(parm.DisplayOrder != null, it => it.DisplayOrder == parm.DisplayOrder);
            predicate = predicate.AndIF(parm.Parentid != null, it => it.Parentid == parm.Parentid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CreateBy), it => it.Create_by == parm.CreateBy);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.UpdateBy), it => it.Update_by == parm.UpdateBy);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark == parm.Remark);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .ToTree(it => it.Children, it => it.Parentid, 0);

            return response;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Organizationsid"></param>
        /// <returns></returns>
        public Organizations GetInfo(long Organizationsid)
        {
            var response = Queryable()
                .Where(x => x.Organizationsid == Organizationsid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Organizations Information Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Organizations AddOrganizations(Organizations model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Organizations Information Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateOrganizations(Organizations model)
        {
            int v = Update(model, true, "修改Organizations Information Table");
            return v;
        }

        /// <summary>
        /// 清空Organizations Information Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateOrganizations()
        {
            var newTableName = $"organizations_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("organizations", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Organizations Information Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportOrganizations(List<Organizations> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "Organization name不能为空")
                .SplitError(x => x.Item.Organizationtypeid.IsEmpty(), "Organizationtypeid不能为空")
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
        public PagedInfo<OrganizationsDto> ExportList(OrganizationsQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new OrganizationsDto()
                {
                    OrganizationtypeidLabel = it.Organizationtypeid.GetConfigValue<Model.System.SysDictData>("sql_type_of_organization"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<Organizations> QueryExp(OrganizationsQueryDto parm)
        {
            var predicate = Expressionable.Create<Organizations>();

            predicate = predicate.AndIF(parm.Organizationsid != null, it => it.Organizationsid == parm.Organizationsid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name == parm.Name);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Organizationtypeid), it => it.Organizationtypeid == parm.Organizationtypeid);
            predicate = predicate.AndIF(parm.DisplayOrder != null, it => it.DisplayOrder == parm.DisplayOrder);
            predicate = predicate.AndIF(parm.Parentid != null, it => it.Parentid == parm.Parentid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CreateBy), it => it.Create_by == parm.CreateBy);
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.UpdateBy), it => it.Update_by == parm.UpdateBy);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark == parm.Remark);
            return predicate;
        }
    }
}
