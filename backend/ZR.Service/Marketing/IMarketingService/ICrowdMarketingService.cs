using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Marketing.CrowdAnalytics;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Crowd Marketing Service Interface - خدمة تسويق الحشود
    /// </summary>
    public interface ICrowdMarketingService : IBaseService<MktCrowdAnalytics>
    {
        /// <summary>
        /// Get crowd analytics data with filtering
        /// </summary>
        PagedInfo<CrowdAnalyticsDto> GetCrowdAnalytics(CrowdAnalyticsQueryDto query);

        /// <summary>
        /// Get crowd summary for an event
        /// </summary>
        CrowdAnalyticsSummaryDto GetCrowdSummary(long? eventId);

        /// <summary>
        /// Record crowd analytics data (from IoT/sensors)
        /// </summary>
        MktCrowdAnalytics RecordCrowdData(CrowdAnalyticsDto dto);

        /// <summary>
        /// Get zone-level analytics for an event
        /// </summary>
        List<ZoneSummaryDto> GetZoneAnalytics(long eventId);

        /// <summary>
        /// Get heatmap data
        /// </summary>
        List<HeatmapDto> GetHeatmaps(HeatmapQueryDto query);

        /// <summary>
        /// Get latest heatmap for an event and optional floor level
        /// </summary>
        HeatmapDto GetLatestHeatmap(long eventId, string floorLevel);

        /// <summary>
        /// Record a heatmap snapshot
        /// </summary>
        MktCrowdHeatmap RecordHeatmap(HeatmapDto dto);

        /// <summary>
        /// Get exhibitor reports with filtering
        /// </summary>
        PagedInfo<ExhibitorReportDto> GetExhibitorReports(ExhibitorReportQueryDto query);

        /// <summary>
        /// Get a specific exhibitor report by ID
        /// </summary>
        ExhibitorReportDto GetExhibitorReportDetail(long reportId);

        /// <summary>
        /// Generate an exhibitor report
        /// </summary>
        MktExhibitorReport GenerateExhibitorReport(GenerateExhibitorReportRequest request);

        /// <summary>
        /// Mark an exhibitor report as delivered
        /// </summary>
        int DeliverExhibitorReport(long reportId);

        /// <summary>
        /// Rate an exhibitor report
        /// </summary>
        int RateExhibitorReport(long reportId, decimal rating);

        /// <summary>
        /// Get real-time insights with filtering
        /// </summary>
        PagedInfo<RealtimeInsightDto> GetRealtimeInsights(RealtimeInsightQueryDto query);

        /// <summary>
        /// Get active (non-expired, non-actioned) insights for an event
        /// </summary>
        List<RealtimeInsightDto> GetActiveInsights(long eventId);

        /// <summary>
        /// Action a real-time insight
        /// </summary>
        int ActionInsight(ActionInsightDto dto);
    }
}
