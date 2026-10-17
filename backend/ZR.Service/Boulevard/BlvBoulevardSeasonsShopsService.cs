using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Repository;
using ZR.Service.Boulevard.IBoulevardService;
using Mapster;

namespace ZR.Service.Boulevard
{
    /// <summary>
    /// boulevard_seasons_ShopsService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBlvBoulevardSeasonsShopsService), ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardSeasonsShopsService : BaseService<BlvBoulevardSeasonsShops>, IBlvBoulevardSeasonsShopsService
    {
        /// <summary>
        /// 查询boulevard_seasons_Shops列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvBoulevardSeasonsShopsDto> GetList(BlvBoulevardSeasonsShopsQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<BlvBoulevardSeasonsShops, BlvBoulevardSeasonsShopsDto>(parm);

            return response;
        }




        /// <summary>
        /// 查询boulevard_seasons_Shops列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public List<BlvBoulevardSeasonsShopsDto> GetListAll(BlvBoulevardSeasonsShopsQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToList( );

            return response.Adapt<List<BlvBoulevardSeasonsShopsDto>>();
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public BlvBoulevardSeasonsShops GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加boulevard_seasons_Shops
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BlvBoulevardSeasonsShops AddBlvBoulevardSeasonsShops(BlvBoulevardSeasonsShops model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改boulevard_seasons_Shops
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBlvBoulevardSeasonsShops(BlvBoulevardSeasonsShops model)
        {
            return Update(model, true, "修改boulevard_seasons_Shops");
        }

        /// <summary>
        /// 清空boulevard_seasons_Shops
        /// </summary>
        /// <returns></returns>
        public bool TruncateBlvBoulevardSeasonsShops()
        {
            var newTableName = $"blv_boulevard_seasons_shops_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("blv_boulevard_seasons_shops", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入boulevard_seasons_Shops
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBlvBoulevardSeasonsShops(List<BlvBoulevardSeasonsShops> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.SeasonId.IsEmpty(), "SeasonId不能为空")
                .SplitError(x => x.Item.ShopId.IsEmpty(), "ShopId不能为空")
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
        /// 导出boulevard_seasons_Shops
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvBoulevardSeasonsShopsDto> ExportList(BlvBoulevardSeasonsShopsQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BlvBoulevardSeasonsShopsDto()
                {
                    SeasonIdLabel = it.SeasonId.GetConfigValue<Model.System.SysDictData>("sql_blv_boulevard_seasons"),
                    ShopIdLabel = it.ShopId.GetConfigValue<Model.System.SysDictData>("sql_blv_boulevard_shops"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<BlvBoulevardSeasonsShops> QueryExp(BlvBoulevardSeasonsShopsQueryDto parm)
        {
            var predicate = Expressionable.Create<BlvBoulevardSeasonsShops>();

            predicate = predicate.AndIF(parm.SeasonId != null, it => it.SeasonId == parm.SeasonId);
            predicate = predicate.AndIF(parm.ShopId != null, it => it.ShopId == parm.ShopId);
            return predicate;
        }
    }
}
