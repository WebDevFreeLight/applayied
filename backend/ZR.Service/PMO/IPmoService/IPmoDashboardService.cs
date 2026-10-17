using System.Collections.Generic;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoDashboardService
    {
        PmoDashboardKpis GetKpis(PmoDashboardFilter filter, PmoActor actor);
        PmoDashboardAnalytics GetAnalytics(PmoDashboardFilter filter, PmoActor actor);
        /// <summary>Delivery drill-down: tasks matching the filter (optionally only overdue/blocked).</summary>
        List<PmoTask> DrilldownTasks(PmoDashboardFilter filter, string mode, PmoActor actor);
        List<PmoRisk> DrilldownRisks(PmoDashboardFilter filter, bool highExposureOnly, PmoActor actor);
    }
}
