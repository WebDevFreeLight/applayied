using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Executive dashboard analytics endpoints — live SQL-backed KPIs, visuals and drill-downs.
    /// Management view: requires Project Manager / Administrator.
    /// </summary>
    [Route("pmo/dashboard")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoDashboardController : PmoBaseController
    {
        private readonly IPmoDashboardService _dashboard;
        public PmoDashboardController(IPmoDashboardService dashboard) => _dashboard = dashboard;

        /// <summary>KPI cards (delivery, risk/governance, finance) for the current filter context.</summary>
        [HttpGet("kpis")]
        [ActionPermissionFilter(Permission = "pmo:dashboard:view")]
        public IActionResult Kpis([FromQuery] PmoDashboardFilter filter) => SUCCESS(_dashboard.GetKpis(filter, CurrentActor));

        /// <summary>Data for the interactive visuals (health, budget-vs-actual-vs-EAC, trends, risk matrix…).</summary>
        [HttpGet("analytics")]
        [ActionPermissionFilter(Permission = "pmo:dashboard:view")]
        public IActionResult Analytics([FromQuery] PmoDashboardFilter filter) => SUCCESS(_dashboard.GetAnalytics(filter, CurrentActor));

        /// <summary>Delivery drill-down: tasks (mode = all | overdue | blocked).</summary>
        [HttpGet("drilldown/tasks")]
        [ActionPermissionFilter(Permission = "pmo:dashboard:view")]
        public IActionResult DrilldownTasks([FromQuery] PmoDashboardFilter filter, [FromQuery] string mode)
            => SUCCESS(_dashboard.DrilldownTasks(filter, mode, CurrentActor));

        /// <summary>Delivery drill-down: risks (optionally only open high-exposure).</summary>
        [HttpGet("drilldown/risks")]
        [ActionPermissionFilter(Permission = "pmo:dashboard:view")]
        public IActionResult DrilldownRisks([FromQuery] PmoDashboardFilter filter, [FromQuery] bool highExposureOnly)
            => SUCCESS(_dashboard.DrilldownRisks(filter, highExposureOnly, CurrentActor));
    }
}
