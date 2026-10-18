using Microsoft.AspNetCore.Mvc;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Activity &amp; logging reads: progress-snapshot history, automation run log, notification
    /// outbox and the durable audit trail. All read-only (source-of-truth rows).
    /// </summary>
    [Route("pmo/activity")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoActivityController : PmoBaseController
    {
        private readonly IPmoActivityService _service;
        public PmoActivityController(IPmoActivityService service) => _service = service;

        /// <summary>Historical progress snapshots (trend) for a project.</summary>
        [HttpGet("snapshots")]
        [ActionPermissionFilter(Permission = "pmo:snapshot:list")]
        public IActionResult Snapshots([FromQuery] PmoSnapshotQueryDto parm) => SUCCESS(_service.GetSnapshots(parm, CurrentActor));

        /// <summary>Automation-rule run log (idempotent runs with dedupe keys).</summary>
        [HttpGet("automation")]
        [ActionPermissionFilter(Permission = "pmo:automation:list")]
        public IActionResult Automation([FromQuery] PmoAutomationQueryDto parm) => SUCCESS(_service.GetAutomationRuns(parm));

        /// <summary>Notification outbox (pending/sent/failed, deduped).</summary>
        [HttpGet("outbox")]
        [ActionPermissionFilter(Permission = "pmo:automation:list")]
        public IActionResult Outbox([FromQuery] PmoOutboxQueryDto parm) => SUCCESS(_service.GetOutbox(parm));

        /// <summary>Durable audit trail of decisions and postings.</summary>
        [HttpGet("audit")]
        [ActionPermissionFilter(Permission = "pmo:audit:list")]
        public IActionResult Audit([FromQuery] PmoAuditQueryDto parm) => SUCCESS(_service.GetAudit(parm));
    }
}
