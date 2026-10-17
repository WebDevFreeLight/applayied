using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.AI.Dto;
using ZR.Model.AI;
using ZR.Repository;
using ZR.Service.AI.IAIService;

namespace ZR.Service.AI
{
    /// <summary>
    /// Team Mode TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IAiTeamModeService), ServiceLifetime = LifeTime.Transient)]
    public class AiTeamModeService : BaseService<AiTeamMode>, IAiTeamModeService
    {
        /// <summary>
        /// 查询Team Mode Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<AiTeamModeDto> GetList(AiTeamModeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<AiTeamMode, AiTeamModeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AiTeamMode GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Team Mode Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public AiTeamMode AddAiTeamMode(AiTeamMode model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Team Mode Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateAiTeamMode(AiTeamMode model)
        {
            return Update(model, true, "修改Team Mode Table");
        }

        /// <summary>
        /// 清空Team Mode Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateAiTeamMode()
        {
            var newTableName = $"ai_team_mode_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("ai_team_mode", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Team Mode Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportAiTeamMode(List<AiTeamMode> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Id.IsEmpty(), "Id不能为空")
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
        /// 导出Team Mode Table
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<AiTeamModeDto> ExportList(AiTeamModeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new AiTeamModeDto()
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
        private static Expressionable<AiTeamMode> QueryExp(AiTeamModeQueryDto parm)
        {
            var predicate = Expressionable.Create<AiTeamMode>();

            predicate = predicate.AndIF(parm.Id != null, it => it.Id == parm.Id);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
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
