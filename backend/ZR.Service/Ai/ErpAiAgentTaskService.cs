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

    [AppService(ServiceType = typeof(IErpAiAgentTaskService), ServiceLifetime = LifeTime.Transient)]
    public class ErpAiAgentTaskService : BaseService<ErpAiAgentTask>, IErpAiAgentTaskService
    {


        public PagedInfo<ErpAiAgentTask> GetList(ErpAiAgentTask parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpAiAgentTask>();
            predicate = predicate.AndIF(parm.AgentId > 0, it => it.AgentId == parm.AgentId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.Create_time).ToPage(pager);
        }

        public ErpAiAgentTask GetInfo(long taskId) => GetFirst(x => x.TaskId == taskId);

        public ErpAiAgentTask CreateTask(ErpAiAgentTask model)
        {
            model.TaskId = SnowFlakeSingle.Instance.NextId();
            model.Status = "pending";
            Insert(model);
            return model;
        }

        public int UpdateTaskStatus(long taskId, string status, string result) => Context.Updateable<ErpAiAgentTask>().SetColumns(it => new ErpAiAgentTask { Status = status, OutputData = result, CompletedAt = status == "completed" || status == "failed" ? DateTime.Now : null, Update_time = DateTime.Now }).Where(it => it.TaskId == taskId).ExecuteCommand();
        public List<ErpAiAgentTask> GetPendingTasks(long? agentId = null) => Queryable().Where(x => x.Status == "pending").WhereIF(agentId.HasValue, x => x.AgentId == agentId).ToList();
        public List<ErpAiAgentTask> GetAgentTasks(long agentId, DateTime? fromDate, DateTime? toDate) => Queryable().Where(x => x.AgentId == agentId).WhereIF(fromDate.HasValue, x => x.Create_time >= fromDate.Value).WhereIF(toDate.HasValue, x => x.Create_time <= toDate.Value).ToList();
        public async Task<object> ProcessTaskAsync(long taskId) { await Task.CompletedTask; return new { processed = true }; }
    }


}
