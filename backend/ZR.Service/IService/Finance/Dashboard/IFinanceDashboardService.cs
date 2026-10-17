using System;
using System.Threading.Tasks;
using ZR.Model.Finance.Dashboard;

namespace ZR.Service.IService.Finance.Dashboard
{
    /// <summary>
    /// Finance Dashboard Service Interface
    /// </summary>
    public interface IFinanceDashboardService
    {
        /// <summary>
        /// Get comprehensive finance dashboard data
        /// </summary>
        /// <param name="asOfDate">Date to calculate dashboard metrics (default: today)</param>
        /// <param name="fiscalYear">Fiscal year (optional)</param>
        /// <param name="period">Period code (optional)</param>
        /// <returns>Complete dashboard data with KPIs, summaries, charts, and alerts</returns>
        Task<FinanceDashboardDto> GetDashboardAsync(DateTime? asOfDate = null, string fiscalYear = null, string period = null);
    }
}
