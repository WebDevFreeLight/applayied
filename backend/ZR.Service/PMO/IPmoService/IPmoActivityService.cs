using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    /// <summary>Read-only activity &amp; logging views: progress snapshots, automation runs, outbox, audit.</summary>
    public interface IPmoActivityService
    {
        PagedInfo<PmoProgressSnapshot> GetSnapshots(PmoSnapshotQueryDto parm, PmoActor actor);
        PagedInfo<PmoAutomationRun> GetAutomationRuns(PmoAutomationQueryDto parm);
        PagedInfo<PmoOutboxNotification> GetOutbox(PmoOutboxQueryDto parm);
        PagedInfo<PmoAuditEvent> GetAudit(PmoAuditQueryDto parm);
    }
}
