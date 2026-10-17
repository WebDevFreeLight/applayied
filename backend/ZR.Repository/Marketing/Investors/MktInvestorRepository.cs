using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.Investors;
using ZR.Repository;

namespace ZR.Repository.Marketing.Investors
{
    /// <summary>
    /// Investment Opportunity Repository - مستودع الفرص الاستثمارية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktInvestmentOpportunityRepository : BaseRepository<MktInvestmentOpportunity>
    {
        #region Opportunity Queries

        /// <summary>
        /// Get available opportunities (published and not fully filled) - الحصول على الفرص المتاحة
        /// </summary>
        public List<MktInvestmentOpportunity> GetAvailableOpportunities()
        {
            return Context.Queryable<MktInvestmentOpportunity>()
                .Where(o => o.Status == "published")
                .Where(o => o.CurrentSponsors < o.MaxSponsors)
                .OrderByDescending(o => o.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get opportunities by event - الحصول على فرص الحدث
        /// </summary>
        public List<MktInvestmentOpportunity> GetByEventId(long eventId)
        {
            return Context.Queryable<MktInvestmentOpportunity>()
                .Where(o => o.EventId == eventId)
                .OrderByDescending(o => o.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get opportunities by type - الحصول على الفرص حسب النوع
        /// </summary>
        public List<MktInvestmentOpportunity> GetByType(string opportunityType)
        {
            return Context.Queryable<MktInvestmentOpportunity>()
                .Where(o => o.OpportunityType == opportunityType)
                .Where(o => o.Status == "published")
                .OrderBy(o => o.MinInvestment)
                .ToList();
        }

        /// <summary>
        /// Get opportunities nearing deadline - الحصول على الفرص القريبة من الموعد النهائي
        /// </summary>
        public List<MktInvestmentOpportunity> GetNearingDeadline(int daysAhead = 7)
        {
            var deadline = DateTime.Now.AddDays(daysAhead);
            return Context.Queryable<MktInvestmentOpportunity>()
                .Where(o => o.Status == "published")
                .Where(o => o.ApplicationDeadline != null && o.ApplicationDeadline <= deadline && o.ApplicationDeadline >= DateTime.Now)
                .OrderBy(o => o.ApplicationDeadline)
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// Sponsor Package Repository - مستودع باقات الرعاية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSponsorPackageRepository : BaseRepository<MktSponsorPackage>
    {
        /// <summary>
        /// Get active packages ordered by tier - الحصول على الباقات النشطة حسب المستوى
        /// </summary>
        public List<MktSponsorPackage> GetActivePackages()
        {
            return Context.Queryable<MktSponsorPackage>()
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToList();
        }

        /// <summary>
        /// Get packages by event - الحصول على باقات الحدث
        /// </summary>
        public List<MktSponsorPackage> GetByEventId(long eventId)
        {
            return Context.Queryable<MktSponsorPackage>()
                .Where(p => p.EventId == eventId)
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToList();
        }

        /// <summary>
        /// Get packages by tier - الحصول على الباقات حسب المستوى
        /// </summary>
        public List<MktSponsorPackage> GetByTier(string tier)
        {
            return Context.Queryable<MktSponsorPackage>()
                .Where(p => p.Tier == tier)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToList();
        }
    }

    /// <summary>
    /// Sponsor Application Repository - مستودع طلبات الرعاية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSponsorApplicationRepository : BaseRepository<MktSponsorApplication>
    {
        /// <summary>
        /// Get applications by opportunity - الحصول على طلبات الفرصة
        /// </summary>
        public List<MktSponsorApplication> GetApplicationsByOpportunity(long opportunityId)
        {
            return Context.Queryable<MktSponsorApplication>()
                .Where(a => a.OpportunityId == opportunityId)
                .OrderByDescending(a => a.AiMatchScore)
                .ToList();
        }

        /// <summary>
        /// Get applications by status - الحصول على الطلبات حسب الحالة
        /// </summary>
        public List<MktSponsorApplication> GetByStatus(string status)
        {
            return Context.Queryable<MktSponsorApplication>()
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get applications by package - الحصول على طلبات الباقة
        /// </summary>
        public List<MktSponsorApplication> GetByPackageId(long packageId)
        {
            return Context.Queryable<MktSponsorApplication>()
                .Where(a => a.PackageId == packageId)
                .OrderByDescending(a => a.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get applications pending review - الحصول على الطلبات المعلقة للمراجعة
        /// </summary>
        public List<MktSponsorApplication> GetPendingReview()
        {
            return Context.Queryable<MktSponsorApplication>()
                .Where(a => a.Status == ApplicationStatus.SUBMITTED || a.Status == ApplicationStatus.UNDER_REVIEW)
                .OrderByDescending(a => a.AiMatchScore)
                .ToList();
        }
    }

    /// <summary>
    /// Investor KPI Report Repository - مستودع تقارير مؤشرات الأداء للمستثمرين
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktInvestorKpiReportRepository : BaseRepository<MktInvestorKpiReport>
    {
        /// <summary>
        /// Get reports by sponsor application - الحصول على تقارير طلب الرعاية
        /// </summary>
        public List<MktInvestorKpiReport> GetSponsorReports(long applicationId)
        {
            return Context.Queryable<MktInvestorKpiReport>()
                .Where(r => r.ApplicationId == applicationId)
                .OrderByDescending(r => r.PeriodEnd)
                .ToList();
        }

        /// <summary>
        /// Get reports by event - الحصول على تقارير الحدث
        /// </summary>
        public List<MktInvestorKpiReport> GetByEventId(long eventId)
        {
            return Context.Queryable<MktInvestorKpiReport>()
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.PeriodEnd)
                .ToList();
        }

        /// <summary>
        /// Get reports ready for delivery - الحصول على التقارير الجاهزة للتسليم
        /// </summary>
        public List<MktInvestorKpiReport> GetReadyForDelivery()
        {
            return Context.Queryable<MktInvestorKpiReport>()
                .Where(r => r.DeliveryStatus == "ready")
                .OrderBy(r => r.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get latest report for application - الحصول على أحدث تقرير للطلب
        /// </summary>
        public MktInvestorKpiReport GetLatestReport(long applicationId)
        {
            return Context.Queryable<MktInvestorKpiReport>()
                .Where(r => r.ApplicationId == applicationId)
                .OrderByDescending(r => r.PeriodEnd)
                .First();
        }
    }
}
