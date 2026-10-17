using Infrastructure;
using Infrastructure.Attribute;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ZR.Model.Finance.Dashboard;
using ZR.Service.IService.Finance.Dashboard;

namespace ZR.Admin.WebApi.Controllers.Finance.Dashboard
{
    /// <summary>
    /// Finance Dashboard Controller
    /// Enterprise-grade finance dashboard similar to SAP/Odoo
    /// Provides comprehensive view of financial health and operations
    /// </summary>
    [Route("api/erp/finance/dashboard")]
    [ApiController]
    public class FinanceDashboardController : BaseController
    {
        private readonly IFinanceDashboardService _dashboardService;

        public FinanceDashboardController(IFinanceDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get comprehensive finance dashboard
        /// </summary>
        /// <param name="asOfDate">Date to calculate metrics (optional, defaults to today)</param>
        /// <param name="fiscalYear">Fiscal year filter (optional)</param>
        /// <param name="period">Period code filter (optional)</param>
        /// <returns>Complete dashboard with KPIs, summaries, charts, alerts, and recent activities</returns>
        /// <remarks>
        /// This endpoint provides a comprehensive view of the entire finance module including:
        /// - Key financial KPIs (Assets, Liabilities, Revenue, Expenses, Cash Flow)
        /// - Module summaries (GL, AR, AP, Treasury, Assets, Budget, Tax)
        /// - Trend charts (Revenue, Expense, Profit, Cash Flow)
        /// - Real-time alerts and notifications
        /// - Recent financial activities
        /// - Quick action shortcuts
        /// - Current period information
        ///
        /// Example usage:
        /// - GET /api/erp/finance/dashboard
        /// - GET /api/erp/finance/dashboard?asOfDate=2025-12-31
        /// - GET /api/erp/finance/dashboard?fiscalYear=2025&amp;period=12
        /// </remarks>
        [HttpGet]
        [ActionPermissionFilter(Permission = "finance:dashboard:view")]
        [Log(Title = "Finance Dashboard", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime? asOfDate, [FromQuery] string fiscalYear, [FromQuery] string period)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(asOfDate, fiscalYear, period);
                return SUCCESS(dashboard, TIME_FORMAT_FULL);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to load finance dashboard: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get only financial KPIs (lightweight endpoint)
        /// </summary>
        /// <param name="asOfDate">Date to calculate KPIs</param>
        /// <param name="fiscalYear">Fiscal year filter</param>
        /// <param name="period">Period code filter</param>
        /// <returns>Financial KPIs only</returns>
        [HttpGet("kpis")]
        [ActionPermissionFilter(Permission = "finance:dashboard:view")]
        public async Task<IActionResult> GetKPIsOnly([FromQuery] DateTime? asOfDate, [FromQuery] string fiscalYear, [FromQuery] string period)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(asOfDate, fiscalYear, period);
                return SUCCESS(dashboard.Kpis, TIME_FORMAT_FULL);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to load KPIs: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get only module summaries
        /// </summary>
        /// <param name="asOfDate">Date to calculate summaries</param>
        /// <param name="fiscalYear">Fiscal year filter</param>
        /// <param name="period">Period code filter</param>
        /// <returns>Module summaries only</returns>
        [HttpGet("modules")]
        [ActionPermissionFilter(Permission = "finance:dashboard:view")]
        public async Task<IActionResult> GetModuleSummaries([FromQuery] DateTime? asOfDate, [FromQuery] string fiscalYear, [FromQuery] string period)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(asOfDate, fiscalYear, period);
                return SUCCESS(dashboard.Modules, TIME_FORMAT_FULL);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to load module summaries: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get only chart data for visualizations
        /// </summary>
        /// <param name="asOfDate">Date to calculate chart data</param>
        /// <returns>Chart data for trends and distributions</returns>
        [HttpGet("charts")]
        [ActionPermissionFilter(Permission = "finance:dashboard:view")]
        public async Task<IActionResult> GetChartData([FromQuery] DateTime? asOfDate)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(asOfDate, null, null);
                return SUCCESS(dashboard.Charts, TIME_FORMAT_FULL);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to load chart data: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get only alerts and notifications
        /// </summary>
        /// <param name="asOfDate">Date to check alerts</param>
        /// <returns>List of active alerts</returns>
        [HttpGet("alerts")]
        [ActionPermissionFilter(Permission = "finance:dashboard:view")]
        public async Task<IActionResult> GetAlerts([FromQuery] DateTime? asOfDate)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(asOfDate, null, null);
                return SUCCESS(dashboard.Alerts, TIME_FORMAT_FULL);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to load alerts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get only recent activities
        /// </summary>
        /// <returns>List of recent financial activities</returns>
        [HttpGet("activities")]
        [ActionPermissionFilter(Permission = "finance:dashboard:view")]
        public async Task<IActionResult> GetRecentActivities()
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(null, null, null);
                return SUCCESS(dashboard.RecentActivities, TIME_FORMAT_FULL);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to load recent activities: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get quick action shortcuts
        /// </summary>
        /// <returns>List of available quick actions</returns>
        [HttpGet("quick-actions")]
        [ActionPermissionFilter(Permission = "finance:dashboard:view")]
        public async Task<IActionResult> GetQuickActions()
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(null, null, null);
                return SUCCESS(dashboard.QuickActions, TIME_FORMAT_FULL);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to load quick actions: {ex.Message}"));
            }
        }
    }
}
