using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.AI;
using ZR.Repository;
using ZR.Service.AI.IService;

namespace ZR.Service.AI
{

    [AppService(ServiceType = typeof(IErpAiThinkingLogService), ServiceLifetime = LifeTime.Transient)]
    public class ErpAiThinkingLogService : BaseService<ErpAiThinkingLog>, IErpAiThinkingLogService
    {


        public PagedInfo<ErpAiThinkingLog> GetList(ErpAiThinkingLog parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpAiThinkingLog>();
            predicate = predicate.AndIF(parm.AgentId > 0, it => it.AgentId == parm.AgentId);
            predicate = predicate.AndIF(parm.TaskId > 0, it => it.TaskId == parm.TaskId);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.Create_time).ToPage(pager);
        }

        public ErpAiThinkingLog LogThinking(ErpAiThinkingLog model)
        {
            model.LogId = SnowFlakeSingle.Instance.NextId();
            Insert(model);
            return model;
        }

        public List<ErpAiThinkingLog> GetAgentThinkingLogs(long agentId, DateTime? fromDate, DateTime? toDate) => Queryable().Where(x => x.AgentId == agentId).WhereIF(fromDate.HasValue, x => x.Create_time >= fromDate.Value).WhereIF(toDate.HasValue, x => x.Create_time <= toDate.Value).ToList();
        public List<ErpAiThinkingLog> GetTaskThinkingLogs(long taskId) => Queryable().Where(x => x.TaskId == taskId).OrderBy(x => x.Create_time).ToList();
    }


}
