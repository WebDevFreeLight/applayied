using System.Collections.Generic;
using System.Linq;
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
    /// Activity &amp; logging reads (source-of-truth rows, never derived): historical progress
    /// snapshots per project, automation-rule run log, notification outbox and the durable audit
    /// trail. All read-only and paged; actor names are resolved via single IN queries (no fan-out).
    /// </summary>
    [AppService(ServiceType = typeof(IPmoActivityService), ServiceLifetime = LifeTime.Transient)]
    public class PmoActivityService : BaseService<PmoProgressSnapshot>, IPmoActivityService
    {
        public PagedInfo<PmoProgressSnapshot> GetSnapshots(PmoSnapshotQueryDto parm, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoProgressSnapshot>();
            exp.AndIF(parm.ProjectId != null, s => s.ProjectId == parm.ProjectId);
            return Queryable().Where(exp.ToExpression())
                .OrderBy(s => s.SnapshotDate, OrderByType.Desc)
                .OrderBy(s => s.SnapshotId, OrderByType.Desc).ToPage(parm);
        }

        public PagedInfo<PmoAutomationRun> GetAutomationRuns(PmoAutomationQueryDto parm)
        {
            var exp = Expressionable.Create<PmoAutomationRun>();
            exp.AndIF(!string.IsNullOrEmpty(parm.RuleKey), r => r.RuleKey == parm.RuleKey);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), r => r.Status == parm.Status);
            return Context.Queryable<PmoAutomationRun>().Where(exp.ToExpression())
                .OrderBy(r => r.TriggeredAt, OrderByType.Desc).ToPage(parm);
        }

        public PagedInfo<PmoOutboxNotification> GetOutbox(PmoOutboxQueryDto parm)
        {
            var exp = Expressionable.Create<PmoOutboxNotification>();
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), n => n.Status == parm.Status);
            exp.AndIF(parm.TargetUserId != null, n => n.TargetUserId == parm.TargetUserId);
            var page = Context.Queryable<PmoOutboxNotification>().Where(exp.ToExpression())
                .OrderBy(n => n.CreatedAt, OrderByType.Desc).ToPage(parm);
            return page;
        }

        public PagedInfo<PmoAuditEvent> GetAudit(PmoAuditQueryDto parm)
        {
            var exp = Expressionable.Create<PmoAuditEvent>();
            exp.AndIF(!string.IsNullOrEmpty(parm.EntityType), a => a.EntityType == parm.EntityType);
            exp.AndIF(parm.EntityId != null, a => a.EntityId == parm.EntityId);
            exp.AndIF(parm.ActorUserId != null, a => a.ActorUserId == parm.ActorUserId);

            var page = Context.Queryable<PmoAuditEvent>().Where(exp.ToExpression())
                .OrderBy(a => a.ActedAt, OrderByType.Desc).ToPage(parm);

            // enrich actor display names (single IN query)
            var rows = page.Result;
            if (rows != null && rows.Count > 0)
            {
                var ids = rows.Select(a => a.ActorUserId).Distinct().ToList();
                var users = Context.Queryable<SysUser>().Where(u => ids.Contains(u.UserId))
                    .Select(u => new { u.UserId, u.NickName, u.UserName }).ToList()
                    .ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);
                foreach (var a in rows)
                    a.ActorName = users.TryGetValue(a.ActorUserId, out var n) ? n : null;
            }
            return page;
        }
    }
}
