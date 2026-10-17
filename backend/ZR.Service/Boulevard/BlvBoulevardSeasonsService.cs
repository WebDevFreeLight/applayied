using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Repository;
using ZR.Service.Boulevard.IBoulevardService;

namespace ZR.Service.Boulevard
{
    /// <summary>
    /// boulevard_seasonsService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBlvBoulevardSeasonsService), ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardSeasonsService : BaseService<BlvBoulevardSeasons>, IBlvBoulevardSeasonsService
    {
        /// <summary>
        /// 查询boulevard_seasons列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvBoulevardSeasonsDto> GetList(BlvBoulevardSeasonsQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<BlvBoulevardSeasons, BlvBoulevardSeasonsDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public BlvBoulevardSeasons GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加boulevard_seasons
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BlvBoulevardSeasons AddBlvBoulevardSeasons(BlvBoulevardSeasons model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改boulevard_seasons
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBlvBoulevardSeasons(BlvBoulevardSeasons model)
        {
            return Update(model, true, "修改boulevard_seasons");
        }

        /// <summary>
        /// 清空boulevard_seasons
        /// </summary>
        /// <returns></returns>
        public bool TruncateBlvBoulevardSeasons()
        {
            var newTableName = $"blv_boulevard_seasons_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("blv_boulevard_seasons", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入boulevard_seasons
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBlvBoulevardSeasons(List<BlvBoulevardSeasons> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.StartDate.IsEmpty(), "Start Date不能为空")
                .SplitError(x => x.Item.EndDate.IsEmpty(), "End Date不能为空")
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
        /// 导出boulevard_seasons
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvBoulevardSeasonsDto> ExportList(BlvBoulevardSeasonsQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BlvBoulevardSeasonsDto()
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
        private static Expressionable<BlvBoulevardSeasons> QueryExp(BlvBoulevardSeasonsQueryDto parm)
        {
            var predicate = Expressionable.Create<BlvBoulevardSeasons>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
          //  predicate = predicate.AndIF(parm.BeginStartDate == null, it => it.StartDate >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginStartDate != null, it => it.StartDate >= parm.BeginStartDate);
            predicate = predicate.AndIF(parm.EndStartDate != null, it => it.StartDate <= parm.EndStartDate);
          //  predicate = predicate.AndIF(parm.BeginEndDate == null, it => it.EndDate >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginEndDate != null, it => it.EndDate >= parm.BeginEndDate);
            predicate = predicate.AndIF(parm.EndEndDate != null, it => it.EndDate <= parm.EndEndDate);
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
