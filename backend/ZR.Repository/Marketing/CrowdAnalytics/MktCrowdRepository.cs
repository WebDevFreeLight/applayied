using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.CrowdAnalytics;
using ZR.Repository;

namespace ZR.Repository.Marketing.CrowdAnalytics
{
    /// <summary>
    /// Crowd Analytics Repository - مستودع تحليلات الحشود
    /// Real-time crowd behavior data access
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktCrowdAnalyticsRepository : BaseRepository<MktCrowdAnalytics>
    {
        #region Analytics Queries

        /// <summary>
        /// Get latest analytics for event and optional zone - الحصول على أحدث التحليلات
        /// </summary>
        public List<MktCrowdAnalytics> GetLatestAnalytics(long eventId, long? zoneId)
        {
            var query = Context.Queryable<MktCrowdAnalytics>()
                .Where(a => a.EventId == eventId);

            if (zoneId.HasValue)
            {
                query = query.Where(a => a.ZoneId == zoneId.Value);
            }

            return query
                .OrderByDescending(a => a.RecordedAt)
                .Take(50)
                .ToList();
        }

        /// <summary>
        /// Get analytics by date range - الحصول على التحليلات حسب الفترة
        /// </summary>
        public List<MktCrowdAnalytics> GetByDateRange(long eventId, DateTime from, DateTime to)
        {
            return Context.Queryable<MktCrowdAnalytics>()
                .Where(a => a.EventId == eventId)
                .Where(a => a.RecordedAt >= from && a.RecordedAt <= to)
                .OrderBy(a => a.RecordedAt)
                .ToList();
        }

        /// <summary>
        /// Get high density alerts - الحصول على تنبيهات الكثافة العالية
        /// </summary>
        public List<MktCrowdAnalytics> GetHighDensityAlerts(long eventId)
        {
            return Context.Queryable<MktCrowdAnalytics>()
                .Where(a => a.EventId == eventId)
                .Where(a => a.DensityLevel == DensityLevels.HIGH || a.DensityLevel == DensityLevels.CRITICAL)
                .OrderByDescending(a => a.RecordedAt)
                .ToList();
        }

        /// <summary>
        /// Get zone occupancy summary - ملخص إشغال المناطق
        /// </summary>
        public List<MktCrowdAnalytics> GetCurrentZoneOccupancy(long eventId)
        {
            return Context.Queryable<MktCrowdAnalytics>()
                .Where(a => a.EventId == eventId)
                .Where(a => a.ZoneId != null)
                .GroupBy(a => a.ZoneId)
                .Select(a => new MktCrowdAnalytics
                {
                    ZoneId = a.ZoneId,
                    ZoneName = SqlFunc.AggregateMax(a.ZoneName),
                    CurrentOccupancy = SqlFunc.AggregateMax(a.CurrentOccupancy),
                    MaxCapacity = SqlFunc.AggregateMax(a.MaxCapacity),
                    DensityLevel = SqlFunc.AggregateMax(a.DensityLevel),
                    RecordedAt = SqlFunc.AggregateMax(a.RecordedAt)
                })
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// Crowd Heatmap Repository - مستودع خريطة الحرارة للحشود
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktCrowdHeatmapRepository : BaseRepository<MktCrowdHeatmap>
    {
        /// <summary>
        /// Get heatmap snapshots by date range - الحصول على لقطات الخريطة الحرارية
        /// </summary>
        public List<MktCrowdHeatmap> GetHeatmapSnapshots(long eventId, DateTime from, DateTime to)
        {
            return Context.Queryable<MktCrowdHeatmap>()
                .Where(h => h.EventId == eventId)
                .Where(h => h.SnapshotAt >= from && h.SnapshotAt <= to)
                .OrderBy(h => h.SnapshotAt)
                .ToList();
        }

        /// <summary>
        /// Get latest heatmap for event - الحصول على أحدث خريطة حرارية
        /// </summary>
        public MktCrowdHeatmap GetLatestHeatmap(long eventId, string floorLevel = null)
        {
            var query = Context.Queryable<MktCrowdHeatmap>()
                .Where(h => h.EventId == eventId);

            if (!string.IsNullOrEmpty(floorLevel))
            {
                query = query.Where(h => h.FloorLevel == floorLevel);
            }

            return query
                .OrderByDescending(h => h.SnapshotAt)
                .First();
        }

        /// <summary>
        /// Get heatmaps by resolution - الحصول على الخرائط الحرارية حسب الدقة
        /// </summary>
        public List<MktCrowdHeatmap> GetByResolution(long eventId, string resolution)
        {
            return Context.Queryable<MktCrowdHeatmap>()
                .Where(h => h.EventId == eventId)
                .Where(h => h.Resolution == resolution)
                .OrderByDescending(h => h.SnapshotAt)
                .ToList();
        }
    }

    /// <summary>
    /// Exhibitor Report Repository - مستودع تقارير العارضين
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktExhibitorReportRepository : BaseRepository<MktExhibitorReport>
    {
        /// <summary>
        /// Get reports by exhibitor - الحصول على تقارير العارض
        /// </summary>
        public List<MktExhibitorReport> GetExhibitorReports(long exhibitorId)
        {
            return Context.Queryable<MktExhibitorReport>()
                .Where(r => r.ExhibitorId == exhibitorId)
                .OrderByDescending(r => r.PeriodEnd)
                .ToList();
        }

        /// <summary>
        /// Get reports by event and exhibitor - الحصول على تقارير العارض في الحدث
        /// </summary>
        public List<MktExhibitorReport> GetByEventAndExhibitor(long eventId, long exhibitorId)
        {
            return Context.Queryable<MktExhibitorReport>()
                .Where(r => r.EventId == eventId && r.ExhibitorId == exhibitorId)
                .OrderByDescending(r => r.PeriodEnd)
                .ToList();
        }

        /// <summary>
        /// Get premium reports - الحصول على التقارير المميزة
        /// </summary>
        public List<MktExhibitorReport> GetPremiumReports(long eventId)
        {
            return Context.Queryable<MktExhibitorReport>()
                .Where(r => r.EventId == eventId)
                .Where(r => r.IsPremium)
                .Where(r => r.Status == "ready")
                .OrderByDescending(r => r.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get reports pending delivery - الحصول على التقارير بانتظار التسليم
        /// </summary>
        public List<MktExhibitorReport> GetPendingDelivery()
        {
            return Context.Queryable<MktExhibitorReport>()
                .Where(r => r.Status == "ready" && r.DeliveredAt == null)
                .OrderBy(r => r.Create_time)
                .ToList();
        }
    }

    /// <summary>
    /// Realtime Insight Repository - مستودع الرؤى اللحظية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktRealtimeInsightRepository : BaseRepository<MktRealtimeInsight>
    {
        /// <summary>
        /// Get active insights for an event - الحصول على الرؤى النشطة للحدث
        /// </summary>
        public List<MktRealtimeInsight> GetActiveInsights(long eventId)
        {
            return Context.Queryable<MktRealtimeInsight>()
                .Where(i => i.EventId == eventId)
                .Where(i => i.ExpiresAt == null || i.ExpiresAt > DateTime.Now)
                .OrderByDescending(i => i.GeneratedAt)
                .ToList();
        }

        /// <summary>
        /// Get insights by type - الحصول على الرؤى حسب النوع
        /// </summary>
        public List<MktRealtimeInsight> GetByInsightType(long eventId, string insightType)
        {
            return Context.Queryable<MktRealtimeInsight>()
                .Where(i => i.EventId == eventId)
                .Where(i => i.InsightType == insightType)
                .OrderByDescending(i => i.GeneratedAt)
                .ToList();
        }

        /// <summary>
        /// Get critical insights - الحصول على الرؤى الحرجة
        /// </summary>
        public List<MktRealtimeInsight> GetCriticalInsights(long eventId)
        {
            return Context.Queryable<MktRealtimeInsight>()
                .Where(i => i.EventId == eventId)
                .Where(i => i.Severity == "critical" || i.Severity == "warning")
                .Where(i => !i.IsActioned)
                .Where(i => i.ExpiresAt == null || i.ExpiresAt > DateTime.Now)
                .OrderByDescending(i => i.Confidence)
                .ToList();
        }

        /// <summary>
        /// Get unactioned insights - الحصول على الرؤى غير المعالجة
        /// </summary>
        public List<MktRealtimeInsight> GetUnactionedInsights(long eventId)
        {
            return Context.Queryable<MktRealtimeInsight>()
                .Where(i => i.EventId == eventId)
                .Where(i => !i.IsActioned)
                .Where(i => i.ExpiresAt == null || i.ExpiresAt > DateTime.Now)
                .OrderByDescending(i => i.GeneratedAt)
                .ToList();
        }
    }
}
