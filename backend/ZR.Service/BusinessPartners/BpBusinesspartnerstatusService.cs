using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Repository;
using ZR.Service.BusinessPartners.IService;

namespace ZR.Service.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerStatusService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBpBusinesspartnerstatusService), ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnerstatusService : BaseService<BpBusinesspartnerstatus>, IBpBusinesspartnerstatusService
    {
        /// <summary>
        /// 查询BusinessPartnerStatus列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnerstatusDto> GetList(BpBusinesspartnerstatusQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Statusid asc")
                .Where(predicate.ToExpression())
                .ToPage<BpBusinesspartnerstatus, BpBusinesspartnerstatusDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Statusid"></param>
        /// <returns></returns>
        public BpBusinesspartnerstatus GetInfo(long Statusid)
        {
            var response = Queryable()
                .Where(x => x.Statusid == Statusid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加BusinessPartnerStatus
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BpBusinesspartnerstatus AddBpBusinesspartnerstatus(BpBusinesspartnerstatus model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改BusinessPartnerStatus
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBpBusinesspartnerstatus(BpBusinesspartnerstatus model)
        {
            return Update(model, true, "修改BusinessPartnerStatus");
        }

        /// <summary>
        /// 清空BusinessPartnerStatus
        /// </summary>
        /// <returns></returns>
        public bool TruncateBpBusinesspartnerstatus()
        {
            var newTableName = $"bp_businesspartnerstatus_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("bp_businesspartnerstatus", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入BusinessPartnerStatus
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBpBusinesspartnerstatus(List<BpBusinesspartnerstatus> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "Status name不能为空")
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
        /// 导出BusinessPartnerStatus
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnerstatusDto> ExportList(BpBusinesspartnerstatusQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BpBusinesspartnerstatusDto()
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
        private static Expressionable<BpBusinesspartnerstatus> QueryExp(BpBusinesspartnerstatusQueryDto parm)
        {
            var predicate = Expressionable.Create<BpBusinesspartnerstatus>();

            predicate = predicate.AndIF(parm.Name != null, it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
