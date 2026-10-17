using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model;
using ZR.Model.Marketing.CrowdAnalytics;
using ZR.Model.Marketing.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Crowd Marketing Service Implementation - خدمة تسويق الحشود
    /// Crowd data aggregation, heatmap management, exhibitor report generation, real-time insight processing
    /// </summary>
    [AppService(ServiceType = typeof(ICrowdMarketingService), ServiceLifetime = LifeTime.Transient)]
    public class CrowdMarketingService : BaseService<MktCrowdAnalytics>, ICrowdMarketingService
    {
        #region Crowd Analytics

        /// <summary>
        /// Get crowd analytics data with filtering
        /// </summary>
        public PagedInfo<CrowdAnalyticsDto> GetCrowdAnalytics(CrowdAnalyticsQueryDto query)
        {
            var predicate = Expressionable.Create<MktCrowdAnalytics>();

            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.ZoneId != null, it => it.ZoneId == query.ZoneId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Granularity), it => it.Granularity == query.Granularity);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.DensityLevel), it => it.DensityLevel == query.DensityLevel);
            predicate = predicate.AndIF(query.DateFrom != null, it => it.RecordedAt >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo != null, it => it.RecordedAt <= query.DateTo);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(c => c.RecordedAt)
                .Select(c => new CrowdAnalyticsDto
                {
                    AnalyticsId = c.AnalyticsId,
                    EventId = c.EventId,
                    ZoneId = c.ZoneId,
                    ZoneName = c.ZoneName,
                    RecordedAt = c.RecordedAt,
                    Granularity = c.Granularity,
                    CurrentOccupancy = c.CurrentOccupancy,
                    MaxCapacity = c.MaxCapacity,
                    EntryCount = c.EntryCount,
                    ExitCount = c.ExitCount,
                    AvgDwellTimeMinutes = c.AvgDwellTimeMinutes,
                    PeakOccupancy = c.PeakOccupancy,
                    DensityLevel = c.DensityLevel,
                    FlowDirection = c.FlowDirection,
                    AvgMovementSpeed = c.AvgMovementSpeed,
                    EngagementScore = c.EngagementScore,
                    DemographicsJson = c.DemographicsJson,
                    InteractionsJson = c.InteractionsJson,
                    OccupancyRate = c.OccupancyRate
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Get crowd summary for an event
        /// </summary>
        public CrowdAnalyticsSummaryDto GetCrowdSummary(long? eventId)
        {
            var query = Queryable();
            if (eventId.HasValue)
            {
                query = query.Where(c => c.EventId == eventId);
            }

            var data = query.ToList();
            if (data.Count == 0)
            {
                return new CrowdAnalyticsSummaryDto
                {
                    EventId = eventId,
                    ZoneSummaries = new List<ZoneSummaryDto>(),
                    HourlyTrend = new List<HourlyTrendDto>(),
                    ActiveAlerts = new List<DensityAlertDto>()
                };
            }

            // Get latest record per zone for current state
            var latestByZone = data
                .Where(d => d.ZoneId.HasValue)
                .GroupBy(d => d.ZoneId)
                .Select(g => g.OrderByDescending(x => x.RecordedAt).First())
                .ToList();

            var summary = new CrowdAnalyticsSummaryDto
            {
                EventId = eventId,
                TotalZones = latestByZone.Count,
                TotalCurrentOccupancy = latestByZone.Sum(z => z.CurrentOccupancy),
                TotalCapacity = latestByZone.Sum(z => z.MaxCapacity),
                OverallOccupancyRate = latestByZone.Sum(z => z.MaxCapacity) > 0
                    ? (decimal)latestByZone.Sum(z => z.CurrentOccupancy) / latestByZone.Sum(z => z.MaxCapacity) * 100
                    : 0,
                PeakAttendance = data.Max(d => d.PeakOccupancy),
                AvgDwellTime = data
                    .Where(d => d.AvgDwellTimeMinutes.HasValue)
                    .Select(d => d.AvgDwellTimeMinutes.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AvgEngagementScore = data
                    .Where(d => d.EngagementScore.HasValue)
                    .Select(d => d.EngagementScore.Value)
                    .DefaultIfEmpty(0)
                    .Average(),

                ZoneSummaries = latestByZone.Select(z => new ZoneSummaryDto
                {
                    ZoneId = z.ZoneId.Value,
                    ZoneName = z.ZoneName,
                    CurrentOccupancy = z.CurrentOccupancy,
                    MaxCapacity = z.MaxCapacity,
                    OccupancyRate = z.OccupancyRate,
                    DensityLevel = z.DensityLevel,
                    AvgDwellTime = z.AvgDwellTimeMinutes,
                    EngagementScore = z.EngagementScore
                }).ToList(),

                HourlyTrend = data
                    .GroupBy(d => d.RecordedAt.Hour)
                    .OrderBy(g => g.Key)
                    .Select(g => new HourlyTrendDto
                    {
                        Hour = g.Key,
                        AvgOccupancy = (int)g.Average(x => x.CurrentOccupancy),
                        PeakOccupancy = g.Max(x => x.PeakOccupancy),
                        TotalEntries = g.Sum(x => x.EntryCount),
                        TotalExits = g.Sum(x => x.ExitCount)
                    }).ToList(),

                ActiveAlerts = latestByZone
                    .Where(z => z.DensityLevel == DensityLevels.HIGH || z.DensityLevel == DensityLevels.CRITICAL)
                    .Select(z => new DensityAlertDto
                    {
                        ZoneId = z.ZoneId.Value,
                        ZoneName = z.ZoneName,
                        DensityLevel = z.DensityLevel,
                        CurrentOccupancy = z.CurrentOccupancy,
                        MaxCapacity = z.MaxCapacity,
                        OccupancyRate = z.OccupancyRate,
                        DetectedAt = z.RecordedAt
                    }).ToList()
            };

            return summary;
        }

        /// <summary>
        /// Record crowd analytics data (from IoT/sensors)
        /// </summary>
        public MktCrowdAnalytics RecordCrowdData(CrowdAnalyticsDto dto)
        {
            var entity = new MktCrowdAnalytics
            {
                EventId = dto.EventId,
                ZoneId = dto.ZoneId,
                ZoneName = dto.ZoneName,
                RecordedAt = dto.RecordedAt != default ? dto.RecordedAt : DateTime.Now,
                Granularity = dto.Granularity,
                CurrentOccupancy = dto.CurrentOccupancy,
                MaxCapacity = dto.MaxCapacity,
                EntryCount = dto.EntryCount,
                ExitCount = dto.ExitCount,
                AvgDwellTimeMinutes = dto.AvgDwellTimeMinutes,
                PeakOccupancy = dto.PeakOccupancy,
                DensityLevel = dto.DensityLevel,
                FlowDirection = dto.FlowDirection,
                AvgMovementSpeed = dto.AvgMovementSpeed,
                EngagementScore = dto.EngagementScore,
                DemographicsJson = dto.DemographicsJson,
                InteractionsJson = dto.InteractionsJson
            };

            Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        /// <summary>
        /// Get zone-level analytics for an event
        /// </summary>
        public List<ZoneSummaryDto> GetZoneAnalytics(long eventId)
        {
            var data = Queryable()
                .Where(c => c.EventId == eventId && c.ZoneId.HasValue)
                .ToList();

            return data
                .GroupBy(d => d.ZoneId)
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.RecordedAt).First();
                    return new ZoneSummaryDto
                    {
                        ZoneId = latest.ZoneId.Value,
                        ZoneName = latest.ZoneName,
                        CurrentOccupancy = latest.CurrentOccupancy,
                        MaxCapacity = latest.MaxCapacity,
                        OccupancyRate = latest.OccupancyRate,
                        DensityLevel = latest.DensityLevel,
                        AvgDwellTime = latest.AvgDwellTimeMinutes,
                        EngagementScore = latest.EngagementScore
                    };
                })
                .ToList();
        }

        #endregion

        #region Heatmaps

        /// <summary>
        /// Get heatmap data
        /// </summary>
        public List<HeatmapDto> GetHeatmaps(HeatmapQueryDto query)
        {
            var predicate = Expressionable.Create<MktCrowdHeatmap>();

            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.FloorLevel), it => it.FloorLevel == query.FloorLevel);
            predicate = predicate.AndIF(query.SnapshotFrom != null, it => it.SnapshotAt >= query.SnapshotFrom);
            predicate = predicate.AndIF(query.SnapshotTo != null, it => it.SnapshotAt <= query.SnapshotTo);

            return Context.Queryable<MktCrowdHeatmap>()
                .Where(predicate.ToExpression())
                .OrderByDescending(h => h.SnapshotAt)
                .Select(h => new HeatmapDto
                {
                    HeatmapId = h.HeatmapId,
                    EventId = h.EventId,
                    FloorLevel = h.FloorLevel,
                    SnapshotAt = h.SnapshotAt,
                    GridDataJson = h.GridDataJson,
                    HotZonesJson = h.HotZonesJson,
                    ColdZonesJson = h.ColdZonesJson,
                    FlowPathsJson = h.FlowPathsJson,
                    BottlenecksJson = h.BottlenecksJson,
                    TotalPeopleTracked = h.TotalPeopleTracked,
                    Resolution = h.Resolution
                })
                .ToList();
        }

        /// <summary>
        /// Get latest heatmap for an event and optional floor level
        /// </summary>
        public HeatmapDto GetLatestHeatmap(long eventId, string floorLevel)
        {
            var query = Context.Queryable<MktCrowdHeatmap>()
                .Where(h => h.EventId == eventId);

            if (!string.IsNullOrEmpty(floorLevel))
            {
                query = query.Where(h => h.FloorLevel == floorLevel);
            }

            var heatmap = query.OrderByDescending(h => h.SnapshotAt).First();
            if (heatmap == null) return null;

            return new HeatmapDto
            {
                HeatmapId = heatmap.HeatmapId,
                EventId = heatmap.EventId,
                FloorLevel = heatmap.FloorLevel,
                SnapshotAt = heatmap.SnapshotAt,
                GridDataJson = heatmap.GridDataJson,
                HotZonesJson = heatmap.HotZonesJson,
                ColdZonesJson = heatmap.ColdZonesJson,
                FlowPathsJson = heatmap.FlowPathsJson,
                BottlenecksJson = heatmap.BottlenecksJson,
                TotalPeopleTracked = heatmap.TotalPeopleTracked,
                Resolution = heatmap.Resolution
            };
        }

        /// <summary>
        /// Record a heatmap snapshot
        /// </summary>
        public MktCrowdHeatmap RecordHeatmap(HeatmapDto dto)
        {
            var heatmap = new MktCrowdHeatmap
            {
                EventId = dto.EventId,
                FloorLevel = dto.FloorLevel,
                SnapshotAt = dto.SnapshotAt != default ? dto.SnapshotAt : DateTime.Now,
                GridDataJson = dto.GridDataJson,
                HotZonesJson = dto.HotZonesJson,
                ColdZonesJson = dto.ColdZonesJson,
                FlowPathsJson = dto.FlowPathsJson,
                BottlenecksJson = dto.BottlenecksJson,
                TotalPeopleTracked = dto.TotalPeopleTracked,
                Resolution = dto.Resolution
            };

            Context.Insertable(heatmap).ExecuteReturnSnowflakeId();
            return heatmap;
        }

        #endregion

        #region Exhibitor Reports

        /// <summary>
        /// Get exhibitor reports with filtering
        /// </summary>
        public PagedInfo<ExhibitorReportDto> GetExhibitorReports(ExhibitorReportQueryDto query)
        {
            var predicate = Expressionable.Create<MktExhibitorReport>();

            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.ExhibitorId != null, it => it.ExhibitorId == query.ExhibitorId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ReportType), it => it.ReportType == query.ReportType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.IsPremium != null, it => it.IsPremium == query.IsPremium);
            predicate = predicate.AndIF(query.PeriodFrom != null, it => it.PeriodStart >= query.PeriodFrom);
            predicate = predicate.AndIF(query.PeriodTo != null, it => it.PeriodEnd <= query.PeriodTo);

            var response = Context.Queryable<MktExhibitorReport>()
                .Where(predicate.ToExpression())
                .OrderByDescending(r => r.Create_time)
                .Select(r => new ExhibitorReportDto
                {
                    ReportId = r.ReportId,
                    EventId = r.EventId,
                    ExhibitorId = r.ExhibitorId,
                    ReportTitle = r.ReportTitle,
                    ReportTitleAr = r.ReportTitleAr,
                    ReportType = r.ReportType,
                    PeriodStart = r.PeriodStart,
                    PeriodEnd = r.PeriodEnd,
                    FootTraffic = r.FootTraffic,
                    UniqueVisitors = r.UniqueVisitors,
                    AvgDwellTime = r.AvgDwellTime,
                    ConversionRate = r.ConversionRate,
                    Revenue = r.Revenue,
                    VsVenueAvgPercent = r.VsVenueAvgPercent,
                    ExhibitorRank = r.ExhibitorRank,
                    TotalExhibitors = r.TotalExhibitors,
                    PeakHoursJson = r.PeakHoursJson,
                    VisitorDemographicsJson = r.VisitorDemographicsJson,
                    VisitorBehaviorJson = r.VisitorBehaviorJson,
                    AiRecommendationsJson = r.AiRecommendationsJson,
                    CompetitorBenchmarkJson = r.CompetitorBenchmarkJson,
                    OpportunityAlertsJson = r.OpportunityAlertsJson,
                    ReportPrice = r.ReportPrice,
                    IsPremium = r.IsPremium,
                    Status = r.Status,
                    DeliveredAt = r.DeliveredAt,
                    ViewedAt = r.ViewedAt,
                    SatisfactionRating = r.SatisfactionRating
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Generate an exhibitor report
        /// </summary>
        public MktExhibitorReport GenerateExhibitorReport(GenerateExhibitorReportRequest request)
        {
            var report = new MktExhibitorReport
            {
                EventId = request.EventId,
                ExhibitorId = request.ExhibitorId,
                ReportTitle = $"Exhibitor Report - {request.ReportType}",
                ReportType = request.ReportType,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                IsPremium = request.IsPremium,
                Status = "generating"
            };

            Context.Insertable(report).ExecuteReturnSnowflakeId();

            // Aggregate crowd data for this exhibitor's zone
            // In real implementation, this would query IoT/sensor data
            Context.Updateable<MktExhibitorReport>()
                .SetColumns(r => r.Status == "ready")
                .Where(r => r.ReportId == report.ReportId)
                .ExecuteCommand();

            return report;
        }

        /// <summary>
        /// Get a specific exhibitor report by ID
        /// </summary>
        public ExhibitorReportDto GetExhibitorReportDetail(long reportId)
        {
            var r = Context.Queryable<MktExhibitorReport>()
                .Where(x => x.ReportId == reportId)
                .First();

            if (r == null) return null;

            return new ExhibitorReportDto
            {
                ReportId = r.ReportId,
                EventId = r.EventId,
                ExhibitorId = r.ExhibitorId,
                ReportTitle = r.ReportTitle,
                ReportTitleAr = r.ReportTitleAr,
                ReportType = r.ReportType,
                PeriodStart = r.PeriodStart,
                PeriodEnd = r.PeriodEnd,
                FootTraffic = r.FootTraffic,
                UniqueVisitors = r.UniqueVisitors,
                AvgDwellTime = r.AvgDwellTime,
                ConversionRate = r.ConversionRate,
                Revenue = r.Revenue,
                VsVenueAvgPercent = r.VsVenueAvgPercent,
                ExhibitorRank = r.ExhibitorRank,
                TotalExhibitors = r.TotalExhibitors,
                PeakHoursJson = r.PeakHoursJson,
                VisitorDemographicsJson = r.VisitorDemographicsJson,
                VisitorBehaviorJson = r.VisitorBehaviorJson,
                AiRecommendationsJson = r.AiRecommendationsJson,
                CompetitorBenchmarkJson = r.CompetitorBenchmarkJson,
                OpportunityAlertsJson = r.OpportunityAlertsJson,
                ReportPrice = r.ReportPrice,
                IsPremium = r.IsPremium,
                Status = r.Status,
                DeliveredAt = r.DeliveredAt,
                ViewedAt = r.ViewedAt,
                SatisfactionRating = r.SatisfactionRating
            };
        }

        /// <summary>
        /// Mark an exhibitor report as delivered
        /// </summary>
        public int DeliverExhibitorReport(long reportId)
        {
            return Context.Updateable<MktExhibitorReport>()
                .SetColumns(r => r.Status == "delivered")
                .SetColumns(r => r.DeliveredAt == DateTime.Now)
                .Where(r => r.ReportId == reportId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Rate an exhibitor report
        /// </summary>
        public int RateExhibitorReport(long reportId, decimal rating)
        {
            return Context.Updateable<MktExhibitorReport>()
                .SetColumns(r => r.SatisfactionRating == rating)
                .Where(r => r.ReportId == reportId)
                .ExecuteCommand();
        }

        #endregion

        #region Real-Time Insights

        /// <summary>
        /// Get real-time insights with filtering
        /// </summary>
        public PagedInfo<RealtimeInsightDto> GetRealtimeInsights(RealtimeInsightQueryDto query)
        {
            var predicate = Expressionable.Create<MktRealtimeInsight>();

            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.InsightType), it => it.InsightType == query.InsightType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Severity), it => it.Severity == query.Severity);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.TargetAudience), it => it.TargetAudience == query.TargetAudience);
            predicate = predicate.AndIF(query.IsActioned != null, it => it.IsActioned == query.IsActioned);
            predicate = predicate.AndIF(query.DateFrom != null, it => it.GeneratedAt >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo != null, it => it.GeneratedAt <= query.DateTo);

            var response = Context.Queryable<MktRealtimeInsight>()
                .Where(predicate.ToExpression())
                .OrderByDescending(i => i.GeneratedAt)
                .Select(i => new RealtimeInsightDto
                {
                    InsightId = i.InsightId,
                    EventId = i.EventId,
                    InsightType = i.InsightType,
                    Severity = i.Severity,
                    Title = i.Title,
                    TitleAr = i.TitleAr,
                    Message = i.Message,
                    MessageAr = i.MessageAr,
                    DataPointsJson = i.DataPointsJson,
                    ActionsJson = i.ActionsJson,
                    Confidence = i.Confidence,
                    TargetAudience = i.TargetAudience,
                    TargetEntityId = i.TargetEntityId,
                    ZoneId = i.ZoneId,
                    GeneratedAt = i.GeneratedAt,
                    ExpiresAt = i.ExpiresAt,
                    IsActioned = i.IsActioned,
                    ActionedAt = i.ActionedAt,
                    ActionTakenNote = i.ActionTakenNote
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Action a real-time insight
        /// </summary>
        public int ActionInsight(ActionInsightDto dto)
        {
            return Context.Updateable<MktRealtimeInsight>()
                .SetColumns(i => i.IsActioned == true)
                .SetColumns(i => i.ActionedAt == DateTime.Now)
                .SetColumns(i => i.ActionTakenNote == dto.ActionTakenNote)
                .Where(i => i.InsightId == dto.InsightId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get active (non-expired, non-actioned) insights for an event
        /// </summary>
        public List<RealtimeInsightDto> GetActiveInsights(long eventId)
        {
            return Context.Queryable<MktRealtimeInsight>()
                .Where(i => i.EventId == eventId)
                .Where(i => i.IsActioned == false)
                .Where(i => i.ExpiresAt == null || i.ExpiresAt > DateTime.Now)
                .OrderByDescending(i => i.GeneratedAt)
                .Select(i => new RealtimeInsightDto
                {
                    InsightId = i.InsightId,
                    EventId = i.EventId,
                    InsightType = i.InsightType,
                    Severity = i.Severity,
                    Title = i.Title,
                    TitleAr = i.TitleAr,
                    Message = i.Message,
                    MessageAr = i.MessageAr,
                    DataPointsJson = i.DataPointsJson,
                    ActionsJson = i.ActionsJson,
                    Confidence = i.Confidence,
                    TargetAudience = i.TargetAudience,
                    TargetEntityId = i.TargetEntityId,
                    ZoneId = i.ZoneId,
                    GeneratedAt = i.GeneratedAt,
                    ExpiresAt = i.ExpiresAt,
                    IsActioned = i.IsActioned,
                    ActionedAt = i.ActionedAt,
                    ActionTakenNote = i.ActionTakenNote
                })
                .ToList();
        }

        #endregion
    }
}
