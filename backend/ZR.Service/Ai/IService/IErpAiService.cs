using ZR.Model;
using ZR.Model.AI;
using ZR.Service;

namespace ZR.Service.AI.IService
{
    public interface IErpAiAgentService : IBaseService<ErpAiAgent>
    {
        PagedInfo<ErpAiAgent> GetList(ErpAiAgent parm, PagerInfo pager);
        ErpAiAgent GetInfo(long agentId);
        ErpAiAgent CreateAgent(ErpAiAgent model);
        int UpdateAgent(ErpAiAgent model);
        int Delete(long[] ids);
        List<ErpAiAgent> GetActiveAgents();
        List<ErpAiAgent> GetAgentsByType(string agentType);
        Task<object> ExecuteAgentAsync(long agentId, object input);
        int ActivateAgent(long agentId);
        int DeactivateAgent(long agentId);
    }

    public interface IErpAiAgentTaskService : IBaseService<ErpAiAgentTask>
    {
        PagedInfo<ErpAiAgentTask> GetList(ErpAiAgentTask parm, PagerInfo pager);
        ErpAiAgentTask GetInfo(long taskId);
        ErpAiAgentTask CreateTask(ErpAiAgentTask model);
        int UpdateTaskStatus(long taskId, string status, string result);
        List<ErpAiAgentTask> GetPendingTasks(long? agentId = null);
        List<ErpAiAgentTask> GetAgentTasks(long agentId, DateTime? fromDate, DateTime? toDate);
        Task<object> ProcessTaskAsync(long taskId);
    }

    public interface IErpAiDecisionService : IBaseService<ErpAiDecision>
    {
        PagedInfo<ErpAiDecision> GetList(ErpAiDecision parm, PagerInfo pager);
        ErpAiDecision GetInfo(long decisionId);
        ErpAiDecision LogDecision(ErpAiDecision model);
        int ApproveDecision(long decisionId, string userId);
        int RejectDecision(long decisionId, string reason);
        List<ErpAiDecision> GetPendingDecisions();
        List<object> GetDecisionAnalytics(DateTime fromDate, DateTime toDate);
    }

    public interface IErpAiThinkingLogService : IBaseService<ErpAiThinkingLog>
    {
        PagedInfo<ErpAiThinkingLog> GetList(ErpAiThinkingLog parm, PagerInfo pager);
        ErpAiThinkingLog LogThinking(ErpAiThinkingLog model);
        List<ErpAiThinkingLog> GetAgentThinkingLogs(long agentId, DateTime? fromDate, DateTime? toDate);
        List<ErpAiThinkingLog> GetTaskThinkingLogs(long taskId);
    }
}
