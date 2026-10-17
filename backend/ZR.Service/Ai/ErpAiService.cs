using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.AI;
using ZR.Repository;
using ZR.Service.AI.IService;

namespace ZR.Service.AI
{
    [AppService(ServiceType = typeof(IErpAiAgentService), ServiceLifetime = LifeTime.Transient)]
    public class ErpAiAgentService : BaseService<ErpAiAgent>, IErpAiAgentService
    {


        public PagedInfo<ErpAiAgent> GetList(ErpAiAgent parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpAiAgent>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AgentName), it => it.AgentName.Contains(parm.AgentName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AgentType), it => it.AgentType == parm.AgentType);
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.AgentName).ToPage(pager);
        }

        public ErpAiAgent GetInfo(long agentId) => GetFirst(x => x.AgentId == agentId);

        public ErpAiAgent CreateAgent(ErpAiAgent model)
        {
            model.AgentId = SnowFlakeSingle.Instance.NextId();
            model.IsActive = true;
            Insert(model);
            return model;
        }

        public int UpdateAgent(ErpAiAgent model) => Update(model, it => new { it.AgentName, it.AgentType, it.Description, it.Configuration, it.Update_by, it.Update_time }, f => f.AgentId == model.AgentId);
        public int Delete(long[] ids) => Context.Updateable<ErpAiAgent>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.AgentId)).ExecuteCommand();
        public List<ErpAiAgent> GetActiveAgents() => Queryable().Where(x => x.IsActive == true).ToList();
        public List<ErpAiAgent> GetAgentsByType(string agentType) => Queryable().Where(x => x.AgentType == agentType && x.IsActive == true).ToList();

        public async Task<object> ExecuteAgentAsync(long agentId, object input)
        {
            var agent = GetInfo(agentId);
            if (agent == null || !agent.IsActive) return null;

            // Log task creation
            var task = new ErpAiAgentTask
            {
                TaskId = SnowFlakeSingle.Instance.NextId(),
                AgentId = agentId,
                TaskType = "execution",
                InputData = System.Text.Json.JsonSerializer.Serialize(input),
                Status = "processing",
                StartedAt = DateTime.Now,
                Create_time = DateTime.Now
            };
            Context.Insertable(task).ExecuteCommand();

            // Execute agent logic based on type
            object result = agent.AgentType switch
            {
                "thinking" => await ExecuteThinkingAgentAsync(agent, input),
                "planning" => await ExecutePlanningAgentAsync(agent, input),
                "decision" => await ExecuteDecisionAgentAsync(agent, input),
                "learning" => await ExecuteLearningAgentAsync(agent, input),
                _ => new { error = "Unknown agent type" }
            };

            // Update task with result
            var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
            Context.Updateable<ErpAiAgentTask>()
                .SetColumns(it => new ErpAiAgentTask { Status = "completed", OutputData = resultJson, CompletedAt = DateTime.Now, Update_time = DateTime.Now })
                .Where(it => it.TaskId == task.TaskId).ExecuteCommand();

            return result;
        }

        private async Task<object> ExecuteThinkingAgentAsync(ErpAiAgent agent, object input) { await Task.CompletedTask; return new { type = "thinking", analysis = "Analyzing data..." }; }
        private async Task<object> ExecutePlanningAgentAsync(ErpAiAgent agent, object input) { await Task.CompletedTask; return new { type = "planning", plan = new List<string> { "Step 1", "Step 2" } }; }
        private async Task<object> ExecuteDecisionAgentAsync(ErpAiAgent agent, object input) { await Task.CompletedTask; return new { type = "decision", recommendation = "Based on analysis..." }; }
        private async Task<object> ExecuteLearningAgentAsync(ErpAiAgent agent, object input) { await Task.CompletedTask; return new { type = "learning", insights = "Learned patterns..." }; }

        public int ActivateAgent(long agentId) => Context.Updateable<ErpAiAgent>().SetColumns(it => new ErpAiAgent { IsActive = true, Update_time = DateTime.Now }).Where(it => it.AgentId == agentId).ExecuteCommand();
        public int DeactivateAgent(long agentId) => Context.Updateable<ErpAiAgent>().SetColumns(it => new ErpAiAgent { IsActive = false, Update_time = DateTime.Now }).Where(it => it.AgentId == agentId).ExecuteCommand();
    }



}
