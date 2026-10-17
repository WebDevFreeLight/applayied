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


    [AppService(ServiceType = typeof(IErpAiDecisionService), ServiceLifetime = LifeTime.Transient)]
    public class ErpAiDecisionService : BaseService<ErpAiDecision>, IErpAiDecisionService
    {


        public PagedInfo<ErpAiDecision> GetList(ErpAiDecision parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpAiDecision>();
            predicate = predicate.AndIF(parm.AgentId > 0, it => it.AgentId == parm.AgentId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.Create_time).ToPage(pager);
        }

        public ErpAiDecision GetInfo(long decisionId) => GetFirst(x => x.DecisionId == decisionId);

        public ErpAiDecision LogDecision(ErpAiDecision model)
        {
            model.DecisionId = SnowFlakeSingle.Instance.NextId();
            // Check if agent requires approval
            var agent = Context.Queryable<ErpAiAgent>().First(x => x.AgentId == model.AgentId);
            model.Status = agent?.RequiresApproval == true ? "pending" : "approved";
            Insert(model);
            return model;
        }

        public int ApproveDecision(long decisionId, string userId) => Context.Updateable<ErpAiDecision>().SetColumns(it => new ErpAiDecision { Status = "approved", ApprovedBy = userId, ApprovalTime = DateTime.Now, UserApproved = true, Update_time = DateTime.Now }).Where(it => it.DecisionId == decisionId && it.Status == "pending").ExecuteCommand();
        public int RejectDecision(long decisionId, string reason) => Context.Updateable<ErpAiDecision>().SetColumns(it => new ErpAiDecision { Status = "rejected", Notes = reason, UserApproved = false, Update_time = DateTime.Now }).Where(it => it.DecisionId == decisionId && it.Status == "pending").ExecuteCommand();
        public List<ErpAiDecision> GetPendingDecisions() => Queryable().Where(x => x.Status == "pending").ToList();

        public List<object> GetDecisionAnalytics(DateTime fromDate, DateTime toDate)
        {
            return Queryable().Where(x => x.Create_time >= fromDate && x.Create_time <= toDate)
                .GroupBy(x => new { x.DecisionType, x.Status })
                .Select(x => new { x.DecisionType, x.Status, Count = SqlFunc.AggregateCount(x.DecisionId) })
                .ToList().Cast<object>().ToList();
        }
    }

}
