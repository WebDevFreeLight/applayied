using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Repository;
using ZR.Service.BusinessPartners.IService;

namespace ZR.Service.BusinessPartners
{
    /// <summary>
    /// compliancestatusService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBpCompliancestatusService), ServiceLifetime = LifeTime.Transient)]
    public class BpCompliancestatusService : BaseService<BpCompliancestatus>, IBpCompliancestatusService
    {
        /// <summary>
        /// 查询compliancestatus列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpCompliancestatusDto> GetList(BpCompliancestatusQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<BpCompliancestatus, BpCompliancestatusDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public BpCompliancestatus GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加compliancestatus
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BpCompliancestatus AddBpCompliancestatus(BpCompliancestatus model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改compliancestatus
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBpCompliancestatus(BpCompliancestatus model)
        {
            return Update(model, true, "修改compliancestatus");
        }

        /// <summary>
        /// 清空compliancestatus
        /// </summary>
        /// <returns></returns>
        public bool TruncateBpCompliancestatus()
        {
            var newTableName = $"bp_compliancestatus_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("bp_compliancestatus", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入compliancestatus
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBpCompliancestatus(List<BpCompliancestatus> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "Type name不能为空")
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
        /// 导出compliancestatus
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpCompliancestatusDto> ExportList(BpCompliancestatusQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BpCompliancestatusDto()
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
        private static Expressionable<BpCompliancestatus> QueryExp(BpCompliancestatusQueryDto parm)
        {
            var predicate = Expressionable.Create<BpCompliancestatus>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
