using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Model.System;
using ZR.Repository;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>
    /// Change-request approval workflow. Enforces permitted transitions, prevents self-approval, and
    /// writes the status change + exactly one history row inside ONE transaction (atomic audit trail).
    /// </summary>
    [AppService(ServiceType = typeof(IPmoChangeRequestService), ServiceLifetime = LifeTime.Transient)]
    public class PmoChangeRequestService : BaseService<PmoChangeRequest>, IPmoChangeRequestService
    {
        private const string Draft = "Draft";
        private const string Submitted = "Submitted";
        private const string UnderReview = "UnderReview";
        private const string Approved = "Approved";
        private const string Rejected = "Rejected";
        private const string Returned = "Returned";

        /// <summary>Permitted state transitions. Any (from,to) not listed is rejected server-side.</summary>
        private static readonly HashSet<(string, string)> Allowed = new()
        {
            (Draft, Submitted),
            (Returned, Submitted),
            (Submitted, UnderReview),
            (Submitted, Approved), (Submitted, Rejected), (Submitted, Returned),
            (UnderReview, Approved), (UnderReview, Rejected), (UnderReview, Returned),
        };

        public PagedInfo<PmoChangeRequest> GetList(PmoChangeRequestQueryDto parm, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoChangeRequest>();
            exp.AndIF(parm.ProjectId != null, c => c.ProjectId == parm.ProjectId);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), c => c.Status == parm.Status);
            if (!string.IsNullOrWhiteSpace(parm.Keyword))
                exp.And(c => c.Title.Contains(parm.Keyword));

            // Scope: PMs/Admins see all; contributors see their own requests or those on projects
            // they own/are a member of.
            if (!actor.IsProjectManager)
            {
                long uid = actor.UserId;
                exp.And(c => c.RequestedByUserId == uid
                    || SqlFunc.Subqueryable<PmoProject>().Where(p => p.ProjectId == c.ProjectId && p.OwnerUserId == uid).Any()
                    || SqlFunc.Subqueryable<PmoProjectMember>().Where(m => m.ProjectId == c.ProjectId && m.UserId == uid).Any());
            }

            var page = Queryable().Where(exp.ToExpression())
                .OrderBy(c => c.ChangeRequestId, OrderByType.Desc).ToPage(parm);
            Enrich(page.Result);
            return page;
        }

        public PmoChangeRequest GetInfo(long changeRequestId, PmoActor actor)
        {
            var cr = Queryable().First(c => c.ChangeRequestId == changeRequestId)
                ?? throw new CustomException(ResultCode.FAIL, "Change request not found");
            Enrich(new List<PmoChangeRequest> { cr });
            return cr;
        }

        public List<PmoChangeHistory> GetHistory(long changeRequestId)
        {
            var list = Context.Queryable<PmoChangeHistory>()
                .Where(h => h.ChangeRequestId == changeRequestId)
                .OrderBy(h => h.ActedAt).ToList();
            var actorIds = list.Select(h => h.ActorUserId).Distinct().ToList();
            var names = Context.Queryable<SysUser>().Where(u => actorIds.Contains(u.UserId))
                .ToList().ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);
            foreach (var h in list) h.ActorName = names.TryGetValue(h.ActorUserId, out var n) ? n : null;
            return list;
        }

        public PmoChangeRequest Create(PmoChangeRequestInputDto dto, PmoActor actor)
        {
            if (!Context.Queryable<PmoProject>().Any(p => p.ProjectId == dto.ProjectId))
                throw new CustomException(ResultCode.FAIL, "Project not found");

            var cr = new PmoChangeRequest
            {
                ProjectId = dto.ProjectId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                EstimatedCostImpact = dto.EstimatedCostImpact,
                RequestedByUserId = actor.UserId,
                Status = Draft,
                Version = 1,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            cr.ChangeRequestId = Insertable(cr).ExecuteReturnSnowflakeId();
            WriteHistoryStandalone(cr.ChangeRequestId, null, Draft, actor, "Created");
            return cr;
        }

        public int Update(PmoChangeRequestInputDto dto, PmoActor actor)
        {
            var cr = Queryable().First(c => c.ChangeRequestId == dto.ChangeRequestId)
                ?? throw new CustomException(ResultCode.FAIL, "Change request not found");
            if (cr.Status != Draft && cr.Status != Returned)
                throw new CustomException(ResultCode.FAIL, $"A change request in '{cr.Status}' cannot be edited");
            if (!actor.IsAdmin && cr.RequestedByUserId != actor.UserId)
                throw new CustomException(ResultCode.FORBIDDEN, "Only the requester may edit this change request");

            cr.Title = dto.Title.Trim();
            cr.Description = dto.Description;
            cr.EstimatedCostImpact = dto.EstimatedCostImpact;
            cr.Update_by = actor.UserName;
            cr.Update_time = DateTime.Now;
            return Update(cr, true, "Updated change request");
        }

        public PmoChangeRequest Submit(long changeRequestId, PmoActor actor)
        {
            var cr = Queryable().First(c => c.ChangeRequestId == changeRequestId)
                ?? throw new CustomException(ResultCode.FAIL, "Change request not found");
            if (!actor.IsAdmin && cr.RequestedByUserId != actor.UserId)
                throw new CustomException(ResultCode.FORBIDDEN, "Only the requester may submit this change request");

            return Transition(cr, Submitted, actor, null);
        }

        public PmoChangeRequest Decide(PmoChangeRequestDecisionDto dto, PmoActor actor)
        {
            var cr = Queryable().First(c => c.ChangeRequestId == dto.ChangeRequestId)
                ?? throw new CustomException(ResultCode.FAIL, "Change request not found");

            // Self-approval prevention — a user may never decide their own request.
            if (cr.RequestedByUserId == actor.UserId)
                throw new CustomException(ResultCode.FORBIDDEN, "You cannot review or decide your own change request");

            var target = dto.Action switch
            {
                "StartReview" => UnderReview,
                "Approve" => Approved,
                "Reject" => Rejected,
                "Return" => Returned,
                _ => throw new CustomException(ResultCode.FAIL, $"Unknown action '{dto.Action}'")
            };

            if ((target == Rejected || target == Returned) && string.IsNullOrWhiteSpace(dto.Reason))
                throw new CustomException(ResultCode.FAIL, "A decision reason is required to reject or return");

            return Transition(cr, target, actor, dto.Reason);
        }

        /// <summary>
        /// Validate the transition, then commit the status change AND one history row atomically.
        /// </summary>
        private PmoChangeRequest Transition(PmoChangeRequest cr, string target, PmoActor actor, string reason)
        {
            var from = cr.Status;
            if (!Allowed.Contains((from, target)))
                throw new CustomException(ResultCode.FAIL, $"Transition {from} → {target} is not permitted");

            var now = DateTime.Now;
            try
            {
                Context.Ado.BeginTran();

                cr.Status = target;
                if (target is Approved or Rejected or Returned)
                {
                    cr.DecisionByUserId = actor.UserId;
                    cr.DecisionReason = reason;
                    cr.DecisionAt = now;
                }
                cr.Version = (cr.Version ?? 0) + 1;
                cr.Update_by = actor.UserName;
                cr.Update_time = now;
                Context.Updateable(cr).ExecuteCommand();

                var history = new PmoChangeHistory
                {
                    ChangeRequestId = cr.ChangeRequestId,
                    FromStatus = from,
                    ToStatus = target,
                    ActorUserId = actor.UserId,
                    Reason = reason,
                    ActedAt = now,
                    Create_by = actor.UserName,
                    Create_time = now
                };
                history.HistoryId = Context.Insertable(history).ExecuteReturnSnowflakeId();

                Context.Ado.CommitTran();
            }
            catch
            {
                Context.Ado.RollbackTran();   // status change and its history row both roll back
                throw;
            }
            return cr;
        }

        private void WriteHistoryStandalone(long crId, string from, string to, PmoActor actor, string reason)
        {
            var history = new PmoChangeHistory
            {
                ChangeRequestId = crId,
                FromStatus = from,
                ToStatus = to,
                ActorUserId = actor.UserId,
                Reason = reason,
                ActedAt = DateTime.Now,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            history.HistoryId = Context.Insertable(history).ExecuteReturnSnowflakeId();
        }

        private void Enrich(List<PmoChangeRequest> list)
        {
            if (list == null || list.Count == 0) return;
            var projIds = list.Select(c => c.ProjectId).Distinct().ToList();
            var userIds = list.Select(c => c.RequestedByUserId)
                .Concat(list.Where(c => c.DecisionByUserId != null).Select(c => c.DecisionByUserId.Value))
                .Distinct().ToList();
            var projNames = Context.Queryable<PmoProject>().Where(p => projIds.Contains(p.ProjectId))
                .ToList().ToDictionary(p => p.ProjectId, p => p.Name);
            var userNames = Context.Queryable<SysUser>().Where(u => userIds.Contains(u.UserId))
                .ToList().ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);
            foreach (var c in list)
            {
                c.ProjectName = projNames.TryGetValue(c.ProjectId, out var pn) ? pn : null;
                c.RequestedByName = userNames.TryGetValue(c.RequestedByUserId, out var rn) ? rn : null;
                if (c.DecisionByUserId != null && userNames.TryGetValue(c.DecisionByUserId.Value, out var dn))
                    c.DecisionByName = dn;
            }
        }
    }
}
