using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Crowd Analytics DTOs

    public class CrowdAnalyticsQueryDto : PagerInfo
    {
        public long? EventId { get; set; }
        public long? ZoneId { get; set; }
        public string Granularity { get; set; }
        public string DensityLevel { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class CrowdAnalyticsDto
    {
        public long AnalyticsId { get; set; }
        public long? EventId { get; set; }
        public long? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public DateTime RecordedAt { get; set; }
        public string Granularity { get; set; }
        public int CurrentOccupancy { get; set; }
        public int MaxCapacity { get; set; }
        public int EntryCount { get; set; }
        public int ExitCount { get; set; }
        public decimal? AvgDwellTimeMinutes { get; set; }
        public int PeakOccupancy { get; set; }
        public string DensityLevel { get; set; }
        public string FlowDirection { get; set; }
        public decimal? AvgMovementSpeed { get; set; }
        public decimal? EngagementScore { get; set; }
        public string DemographicsJson { get; set; }
        public string InteractionsJson { get; set; }
        public decimal OccupancyRate { get; set; }
    }

    public class CrowdAnalyticsSummaryDto
    {
        public long? EventId { get; set; }
        public string EventName { get; set; }
        public int TotalZones { get; set; }
        public int TotalCurrentOccupancy { get; set; }
        public int TotalCapacity { get; set; }
        public decimal OverallOccupancyRate { get; set; }
        public int PeakAttendance { get; set; }
        public decimal AvgDwellTime { get; set; }
        public decimal AvgEngagementScore { get; set; }
        public List<ZoneSummaryDto> ZoneSummaries { get; set; }
        public List<HourlyTrendDto> HourlyTrend { get; set; }
        public List<DensityAlertDto> ActiveAlerts { get; set; }
    }

    public class ZoneSummaryDto
    {
        public long ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int CurrentOccupancy { get; set; }
        public int MaxCapacity { get; set; }
        public decimal OccupancyRate { get; set; }
        public string DensityLevel { get; set; }
        public decimal? AvgDwellTime { get; set; }
        public decimal? EngagementScore { get; set; }
    }

    public class HourlyTrendDto
    {
        public int Hour { get; set; }
        public int AvgOccupancy { get; set; }
        public int PeakOccupancy { get; set; }
        public int TotalEntries { get; set; }
        public int TotalExits { get; set; }
    }

    public class DensityAlertDto
    {
        public long ZoneId { get; set; }
        public string ZoneName { get; set; }
        public string DensityLevel { get; set; }
        public int CurrentOccupancy { get; set; }
        public int MaxCapacity { get; set; }
        public decimal OccupancyRate { get; set; }
        public DateTime DetectedAt { get; set; }
    }

    #endregion

    #region Heatmap DTOs

    public class HeatmapQueryDto
    {
        public long? EventId { get; set; }
        public string FloorLevel { get; set; }
        public DateTime? SnapshotFrom { get; set; }
        public DateTime? SnapshotTo { get; set; }
    }

    public class HeatmapDto
    {
        public long HeatmapId { get; set; }
        public long? EventId { get; set; }
        public string FloorLevel { get; set; }
        public DateTime SnapshotAt { get; set; }
        public string GridDataJson { get; set; }
        public string HotZonesJson { get; set; }
        public string ColdZonesJson { get; set; }
        public string FlowPathsJson { get; set; }
        public string BottlenecksJson { get; set; }
        public int TotalPeopleTracked { get; set; }
        public string Resolution { get; set; }
    }

    #endregion

    #region Exhibitor Report DTOs

    public class ExhibitorReportQueryDto : PagerInfo
    {
        public long? EventId { get; set; }
        public long? ExhibitorId { get; set; }
        public string ReportType { get; set; }
        public string Status { get; set; }
        public bool? IsPremium { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
    }

    public class ExhibitorReportDto
    {
        public long ReportId { get; set; }
        public long? EventId { get; set; }
        public string EventName { get; set; }
        public long ExhibitorId { get; set; }
        public string ExhibitorName { get; set; }
        public string ReportTitle { get; set; }
        public string ReportTitleAr { get; set; }
        public string ReportType { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public long FootTraffic { get; set; }
        public int UniqueVisitors { get; set; }
        public decimal? AvgDwellTime { get; set; }
        public decimal? ConversionRate { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? VsVenueAvgPercent { get; set; }
        public int? ExhibitorRank { get; set; }
        public int? TotalExhibitors { get; set; }
        public string PeakHoursJson { get; set; }
        public string VisitorDemographicsJson { get; set; }
        public string VisitorBehaviorJson { get; set; }
        public string AiRecommendationsJson { get; set; }
        public string CompetitorBenchmarkJson { get; set; }
        public string OpportunityAlertsJson { get; set; }
        public decimal ReportPrice { get; set; }
        public bool IsPremium { get; set; }
        public string Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ViewedAt { get; set; }
        public decimal? SatisfactionRating { get; set; }
    }

    public class GenerateExhibitorReportRequest
    {
        public long EventId { get; set; }
        public long ExhibitorId { get; set; }
        public string ReportType { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public bool IsPremium { get; set; }
    }

    #endregion

    #region Real-Time Insight DTOs

    public class RealtimeInsightQueryDto : PagerInfo
    {
        public long? EventId { get; set; }
        public string InsightType { get; set; }
        public string Severity { get; set; }
        public string TargetAudience { get; set; }
        public bool? IsActioned { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class RealtimeInsightDto
    {
        public long InsightId { get; set; }
        public long? EventId { get; set; }
        public string EventName { get; set; }
        public string InsightType { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public string DataPointsJson { get; set; }
        public string ActionsJson { get; set; }
        public decimal Confidence { get; set; }
        public string TargetAudience { get; set; }
        public long? TargetEntityId { get; set; }
        public long? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActioned { get; set; }
        public DateTime? ActionedAt { get; set; }
        public string ActionTakenNote { get; set; }
    }

    public class ActionInsightDto
    {
        public long InsightId { get; set; }
        public string ActionTakenNote { get; set; }
    }

    #endregion
}
