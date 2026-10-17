using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Repository;
using ZR.Service.BusinessPartners.IService;

namespace ZR.Service.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerkindService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBpBusinesspartnerkindService), ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnerkindService : BaseService<BpBusinesspartnerkind>, IBpBusinesspartnerkindService
    {
        /// <summary>
        /// 查询BusinessPartnerkind列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnerkindDto> GetList(BpBusinesspartnerkindQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Kindid asc")
                .Where(predicate.ToExpression())
                .ToPage<BpBusinesspartnerkind, BpBusinesspartnerkindDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Kindid"></param>
        /// <returns></returns>
        public BpBusinesspartnerkind GetInfo(long Kindid)
        {
            var response = Queryable()
                .Where(x => x.Kindid == Kindid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加BusinessPartnerkind
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BpBusinesspartnerkind AddBpBusinesspartnerkind(BpBusinesspartnerkind model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改BusinessPartnerkind
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBpBusinesspartnerkind(BpBusinesspartnerkind model)
        {
            return Update(model, true, "修改BusinessPartnerkind");
        }

        /// <summary>
        /// 清空BusinessPartnerkind
        /// </summary>
        /// <returns></returns>
        public bool TruncateBpBusinesspartnerkind()
        {
            var newTableName = $"bp_businesspartnerkind_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("bp_businesspartnerkind", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入BusinessPartnerkind
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBpBusinesspartnerkind(List<BpBusinesspartnerkind> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "kind name不能为空")
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
        /// 导出BusinessPartnerkind
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnerkindDto> ExportList(BpBusinesspartnerkindQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BpBusinesspartnerkindDto()
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
        private static Expressionable<BpBusinesspartnerkind> QueryExp(BpBusinesspartnerkindQueryDto parm)
        {
            var predicate = Expressionable.Create<BpBusinesspartnerkind>();

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
