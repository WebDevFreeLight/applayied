using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.CrowdAnalytics
{
    /// <summary>
    /// Crowd Analytics - تحليلات الحشود
    /// Real-time crowd behavior data collection and analysis for marketing insights
    /// </summary>
    [SugarTable("mkt_crowd_analytics")]
    public class MktCrowdAnalytics
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AnalyticsId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Zone/area within the event venue
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ZoneId { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ZoneName { get; set; }

        /// <summary>
        /// Recording timestamp
        /// </summary>
        public DateTime RecordedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Granularity: minute, five_minutes, fifteen_minutes, hourly, daily
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Granularity { get; set; } = "fifteen_minutes";

        /// <summary>
        /// Number of people in zone at this time
        /// </summary>
        public int CurrentOccupancy { get; set; } = 0;

        /// <summary>
        /// Maximum capacity for the zone
        /// </summary>
        public int MaxCapacity { get; set; } = 0;

        /// <summary>
        /// People who entered the zone in this period
        /// </summary>
        public int EntryCount { get; set; } = 0;

        /// <summary>
        /// People who exited the zone in this period
        /// </summary>
        public int ExitCount { get; set; } = 0;

        /// <summary>
        /// Average dwell time in minutes
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AvgDwellTimeMinutes { get; set; }

        /// <summary>
        /// Peak occupancy during this period
        /// </summary>
        public int PeakOccupancy { get; set; } = 0;

        /// <summary>
        /// Density: low, medium, high, critical
        /// </summary>
        [SugarColumn(Length = 20)]
        public string DensityLevel { get; set; } = "low";

        /// <summary>
        /// Crowd flow direction: north, south, east, west, scattered
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string FlowDirection { get; set; }

        /// <summary>
        /// Average movement speed (meters/minute)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AvgMovementSpeed { get; set; }

        /// <summary>
        /// Engagement score for this zone (0-100) based on dwell time and interactions
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EngagementScore { get; set; }

        /// <summary>
        /// JSON: demographic breakdown if available
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DemographicsJson { get; set; }

        /// <summary>
        /// JSON: interaction data (QR scans, NFC taps, app check-ins)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string InteractionsJson { get; set; }

        /// <summary>
        /// Temperature in the zone (if available from IoT sensors)
        /// </summary>
        [SugarColumn(DecimalDigits = 1, IsNullable = true)]
        public decimal? Temperature { get; set; }

        /// <summary>
        /// Noise level in decibels (if available)
        /// </summary>
        [SugarColumn(DecimalDigits = 1, IsNullable = true)]
        public decimal? NoiseLevel { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }

        // Computed
        [SugarColumn(IsIgnore = true)]
        public decimal OccupancyRate => MaxCapacity > 0 ? ((decimal)CurrentOccupancy / MaxCapacity) * 100 : 0;
    }

    /// <summary>
    /// Crowd Heatmap - خريطة حرارية للحشود
    /// Spatial heatmap data for venue visualization
    /// </summary>
    [SugarTable("mkt_crowd_heatmap")]
    public class MktCrowdHeatmap
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HeatmapId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Venue floor/level
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string FloorLevel { get; set; }

        /// <summary>
        /// Snapshot timestamp
        /// </summary>
        public DateTime SnapshotAt { get; set; } = DateTime.Now;

        /// <summary>
        /// JSON: grid data for heatmap rendering
        /// [[x, y, intensity], [x, y, intensity], ...]
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string GridDataJson { get; set; }

        /// <summary>
        /// JSON: hot zones (areas with highest concentration)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string HotZonesJson { get; set; }

        /// <summary>
        /// JSON: cold zones (underutilized areas)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ColdZonesJson { get; set; }

        /// <summary>
        /// JSON: flow paths (main movement corridors)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string FlowPathsJson { get; set; }

        /// <summary>
        /// JSON: bottleneck areas
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BottlenecksJson { get; set; }

        /// <summary>
        /// Total people tracked in this snapshot
        /// </summary>
        public int TotalPeopleTracked { get; set; } = 0;

        /// <summary>
        /// Resolution: low, medium, high
        /// </summary>
        [SugarColumn(Length = 10)]
        public string Resolution { get; set; } = "medium";

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Exhibitor Report - تقارير العارضين
    /// Real-time marketing intelligence reports sold/shared with exhibitors
    /// </summary>
    [SugarTable("mkt_exhibitor_report")]
    public class MktExhibitorReport : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReportId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Exhibitor/Merchant ID (business partner)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ExhibitorId { get; set; }

        [SugarColumn(Length = 200)]
        public string ReportTitle { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ReportTitleAr { get; set; }

        /// <summary>
        /// Type: traffic_report, engagement_report, competitor_benchmark, opportunity_alert,
        ///       daily_summary, realtime_dashboard, custom
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ReportType { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Foot traffic near exhibitor booth/area
        /// </summary>
        public long FootTraffic { get; set; } = 0;

        /// <summary>
        /// Unique visitors
        /// </summary>
        public int UniqueVisitors { get; set; } = 0;

        /// <summary>
        /// Average dwell time at booth (minutes)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AvgDwellTime { get; set; }

        /// <summary>
        /// Conversion rate (visitors who made purchase/inquiry)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ConversionRate { get; set; }

        /// <summary>
        /// Revenue generated during period
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? Revenue { get; set; }

        /// <summary>
        /// Comparison to venue average
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? VsVenueAvgPercent { get; set; }

        /// <summary>
        /// Rank among all exhibitors
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? ExhibitorRank { get; set; }

        /// <summary>
        /// Total exhibitors for rank context
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? TotalExhibitors { get; set; }

        /// <summary>
        /// JSON: peak hours with traffic data
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string PeakHoursJson { get; set; }

        /// <summary>
        /// JSON: visitor demographics (age, gender, nationality if available)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string VisitorDemographicsJson { get; set; }

        /// <summary>
        /// JSON: visitor interests and behavior patterns
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string VisitorBehaviorJson { get; set; }

        /// <summary>
        /// JSON: recommended actions to increase traffic/sales
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiRecommendationsJson { get; set; }

        /// <summary>
        /// JSON: nearby competitor exhibitor performance (anonymized)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string CompetitorBenchmarkJson { get; set; }

        /// <summary>
        /// JSON: opportunity alerts (e.g., "traffic surge expected at 2pm")
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string OpportunityAlertsJson { get; set; }

        /// <summary>
        /// Report pricing (0 = free, included in package)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal ReportPrice { get; set; } = 0;

        /// <summary>
        /// Is this a paid premium report?
        /// </summary>
        public bool IsPremium { get; set; } = false;

        /// <summary>
        /// Status: generating, ready, delivered, viewed
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "generating";

        [SugarColumn(IsNullable = true)]
        public DateTime? DeliveredAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ViewedAt { get; set; }

        [SugarColumn(DecimalDigits = 1, IsNullable = true)]
        public decimal? SatisfactionRating { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Real-Time Insight - الرؤى اللحظية
    /// AI-generated real-time marketing insights from crowd data
    /// </summary>
    [SugarTable("mkt_realtime_insight")]
    public class MktRealtimeInsight
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InsightId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Insight type: traffic_spike, crowd_alert, opportunity, trend, anomaly, prediction, recommendation
        /// </summary>
        [SugarColumn(Length = 50)]
        public string InsightType { get; set; }

        /// <summary>
        /// Severity: info, warning, critical, opportunity
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Severity { get; set; } = "info";

        [SugarColumn(Length = 300)]
        public string Title { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string TitleAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT")]
        public string Message { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string MessageAr { get; set; }

        /// <summary>
        /// JSON: data points that generated this insight
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DataPointsJson { get; set; }

        /// <summary>
        /// JSON: recommended actions
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ActionsJson { get; set; }

        /// <summary>
        /// AI confidence (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal Confidence { get; set; }

        /// <summary>
        /// Target: all, specific_exhibitor, specific_zone, operations
        /// </summary>
        [SugarColumn(Length = 30)]
        public string TargetAudience { get; set; } = "all";

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? TargetEntityId { get; set; }

        /// <summary>
        /// Zone where insight originated
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ZoneId { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Expiry time for time-sensitive insights
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Was this insight acted upon?
        /// </summary>
        public bool IsActioned { get; set; } = false;

        [SugarColumn(IsNullable = true)]
        public DateTime? ActionedAt { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ActionTakenNote { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    public static class DensityLevels
    {
        public const string LOW = "low";
        public const string MEDIUM = "medium";
        public const string HIGH = "high";
        public const string CRITICAL = "critical";
    }

    public static class InsightTypes
    {
        public const string TRAFFIC_SPIKE = "traffic_spike";
        public const string CROWD_ALERT = "crowd_alert";
        public const string OPPORTUNITY = "opportunity";
        public const string TREND = "trend";
        public const string ANOMALY = "anomaly";
        public const string PREDICTION = "prediction";
        public const string RECOMMENDATION = "recommendation";
    }
}
