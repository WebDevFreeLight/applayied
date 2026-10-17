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
    /// boulevard_season_zonesService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBlvBoulevardSeasonZonesService), ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardSeasonZonesService : BaseService<BlvBoulevardSeasonZones>, IBlvBoulevardSeasonZonesService
    {
        /// <summary>
        /// 查询boulevard_season_zones列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvBoulevardSeasonZonesDto> GetList(BlvBoulevardSeasonZonesQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<BlvBoulevardSeasonZones, BlvBoulevardSeasonZonesDto>(parm);

            return response;
        }


        /// <summary>
        /// 查询boulevard_season_zones列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public List<BlvBoulevardSeasonZonesDto> GetListAll(BlvBoulevardSeasonZonesQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToList ( );

            return response.Adapt<List<BlvBoulevardSeasonZonesDto>>();
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public BlvBoulevardSeasonZones GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加boulevard_season_zones
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BlvBoulevardSeasonZones AddBlvBoulevardSeasonZones(BlvBoulevardSeasonZones model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改boulevard_season_zones
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBlvBoulevardSeasonZones(BlvBoulevardSeasonZones model)
        {
            return Update(model, true, "修改boulevard_season_zones");
        }

        /// <summary>
        /// 清空boulevard_season_zones
        /// </summary>
        /// <returns></returns>
        public bool TruncateBlvBoulevardSeasonZones()
        {
            var newTableName = $"blv_boulevard_season_zones_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("blv_boulevard_season_zones", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入boulevard_season_zones
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBlvBoulevardSeasonZones(List<BlvBoulevardSeasonZones> list)
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
        /// 导出boulevard_season_zones
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvBoulevardSeasonZonesDto> ExportList(BlvBoulevardSeasonZonesQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BlvBoulevardSeasonZonesDto()
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
        private static Expressionable<BlvBoulevardSeasonZones> QueryExp(BlvBoulevardSeasonZonesQueryDto parm)
        {
            var predicate = Expressionable.Create<BlvBoulevardSeasonZones>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.SeasonId != null, it => it.SeasonId == parm.SeasonId);
          //  predicate = predicate.AndIF(parm.BeginCreateTime == null, it => it.CreateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
          //  predicate = predicate.AndIF(parm.BeginUpdateTime == null, it => it.UpdateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
