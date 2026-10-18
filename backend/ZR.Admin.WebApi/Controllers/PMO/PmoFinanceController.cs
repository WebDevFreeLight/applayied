using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Financial management — budgets/revisions, cost transactions, forecasts, summary and drill-down.
    /// Finance is a Project-Manager/Administrator domain; Contributors are refused at the route (403).
    /// </summary>
    [Route("pmo/finance")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoFinanceController : PmoBaseController
    {
        private readonly IPmoFinanceService _finance;
        public PmoFinanceController(IPmoFinanceService finance) => _finance = finance;

        /// <summary>Derived financial summary for a project at an optional reporting cutoff.</summary>
        [HttpGet("summary/{projectId}")]
        [ActionPermissionFilter(Permission = "pmo:finance:list")]
        public IActionResult Summary(long projectId, [FromQuery] DateTime? cutoff)
            => SUCCESS(_finance.GetProjectSummary(projectId, cutoff, CurrentActor));

        // ---- Cost transactions ----
        [HttpGet("transactions")]
        [ActionPermissionFilter(Permission = "pmo:finance:list")]
        public IActionResult Transactions([FromQuery] PmoCostTransactionQueryDto parm)
            => SUCCESS(_finance.GetTransactions(parm, CurrentActor));

        [HttpPost("transactions")]
        [ActionPermissionFilter(Permission = "pmo:finance:add")]
        [Log(Title = "PMO Cost Transaction", BusinessType = BusinessType.INSERT)]
        public IActionResult AddTransaction([FromBody] PmoCostTransactionInputDto dto)
            => SUCCESS(_finance.AddTransaction(dto, CurrentActor));

        [HttpPut("transactions")]
        [ActionPermissionFilter(Permission = "pmo:finance:edit")]
        [Log(Title = "PMO Cost Transaction", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateTransaction([FromBody] PmoCostTransactionInputDto dto)
            => ToResponse(_finance.UpdateTransaction(dto, CurrentActor));

        /// <summary>Post a draft cost (atomic with its audit event). Updates spend/remaining/utilization.</summary>
        [HttpPost("transactions/{transactionId}/post")]
        [ActionPermissionFilter(Permission = "pmo:finance:post")]
        [Log(Title = "PMO Cost Transaction Post", BusinessType = BusinessType.UPDATE)]
        public IActionResult PostTransaction(long transactionId)
            => SUCCESS(_finance.PostTransaction(transactionId, CurrentActor));

        // ---- Budget revisions ----
        [HttpGet("revisions")]
        [ActionPermissionFilter(Permission = "pmo:finance:list")]
        public IActionResult Revisions([FromQuery] PmoBudgetRevisionQueryDto parm)
            => SUCCESS(_finance.GetRevisions(parm, CurrentActor));

        [HttpPost("revisions")]
        [ActionPermissionFilter(Permission = "pmo:finance:revision")]
        [Log(Title = "PMO Budget Revision", BusinessType = BusinessType.INSERT)]
        public IActionResult AddRevision([FromBody] PmoBudgetRevisionInputDto dto)
            => SUCCESS(_finance.AddRevision(dto, CurrentActor));

        [HttpPost("revisions/decision")]
        [ActionPermissionFilter(Permission = "pmo:finance:approve")]
        [Log(Title = "PMO Budget Revision Decision", BusinessType = BusinessType.UPDATE)]
        public IActionResult DecideRevision([FromBody] PmoBudgetRevisionDecisionDto dto)
            => SUCCESS(_finance.DecideRevision(dto, CurrentActor));

        // ---- Forecasts ----
        [HttpGet("forecasts/{projectId}")]
        [ActionPermissionFilter(Permission = "pmo:finance:list")]
        public IActionResult Forecasts(long projectId) => SUCCESS(_finance.GetForecasts(projectId));

        [HttpPost("forecasts")]
        [ActionPermissionFilter(Permission = "pmo:finance:forecast")]
        [Log(Title = "PMO Forecast", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpsertForecast([FromBody] PmoForecastInputDto dto)
            => SUCCESS(_finance.UpsertForecast(dto, CurrentActor));

        // ---- Drill-down (finance KPI -> underlying posted transactions) ----
        [HttpGet("drilldown")]
        [ActionPermissionFilter(Permission = "pmo:finance:list")]
        public IActionResult Drilldown([FromQuery] long? projectId, [FromQuery] long? categoryId,
            [FromQuery] string period = null, [FromQuery] DateTime? cutoff = null)
            => SUCCESS(_finance.GetTransactionDrilldown(projectId, categoryId, period, cutoff));
    }
}
