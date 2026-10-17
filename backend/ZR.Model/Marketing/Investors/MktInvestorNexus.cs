using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Investors
{
    /// <summary>
    /// Investment Opportunity - الفرص الاستثمارية
    /// Manages investment opportunities within events for sponsor/investor attraction
    /// </summary>
    [SugarTable("mkt_investment_opportunity")]
    public class MktInvestmentOpportunity : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OpportunityId { get; set; }

        [SugarColumn(Length = 100)]
        public string OpportunityCode { get; set; }

        [SugarColumn(Length = 300)]
        public string Title { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string TitleAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Type: title_sponsorship, platinum_sponsorship, gold_sponsorship, silver_sponsorship,
        ///       booth_premium, booth_standard, media_sponsorship, tech_sponsorship,
        ///       naming_rights, exclusive_category, custom
        /// </summary>
        [SugarColumn(Length = 50)]
        public string OpportunityType { get; set; }

        /// <summary>
        /// Related event ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Investment range minimum
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal MinInvestment { get; set; }

        /// <summary>
        /// Investment range maximum
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal MaxInvestment { get; set; }

        [SugarColumn(Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Expected ROI percentage
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? ExpectedRoi { get; set; }

        /// <summary>
        /// JSON: benefits included in this opportunity
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string BenefitsJson { get; set; }

        /// <summary>
        /// JSON: deliverables and timeline
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DeliverablesJson { get; set; }

        /// <summary>
        /// JSON: KPI metrics that will be shared with investor
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string KpiMetricsJson { get; set; }

        /// <summary>
        /// Maximum number of sponsors for this opportunity
        /// </summary>
        public int MaxSponsors { get; set; } = 1;

        public int CurrentSponsors { get; set; } = 0;

        /// <summary>
        /// Status: draft, published, in_negotiation, filled, closed, expired
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Status { get; set; } = "draft";

        /// <summary>
        /// Visibility: public, invited_only, vip_only
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Visibility { get; set; } = "public";

        /// <summary>
        /// JSON: target industries for this opportunity
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TargetIndustriesJson { get; set; }

        /// <summary>
        /// Deadline for applications
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ApplicationDeadline { get; set; }

        /// <summary>
        /// AI match score threshold (only show to investors above this score)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AiMatchThreshold { get; set; }

        /// <summary>
        /// Cover image URL
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Brochure/PDF URL
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string BrochureUrl { get; set; }

        /// <summary>
        /// Total revenue generated from this opportunity
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal TotalRevenue { get; set; } = 0;

        public int ViewCount { get; set; } = 0;
        public int InquiryCount { get; set; } = 0;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? BranchId { get; set; }

        // Computed
        [SugarColumn(IsIgnore = true)]
        public bool IsAvailable => CurrentSponsors < MaxSponsors && Status == "published";

        [SugarColumn(IsIgnore = true)]
        public decimal FillRate => MaxSponsors > 0 ? ((decimal)CurrentSponsors / MaxSponsors) * 100 : 0;
    }

    /// <summary>
    /// Sponsor Package - باقات الرعاية
    /// Pre-defined sponsorship packages with tiered benefits
    /// </summary>
    [SugarTable("mkt_sponsor_package")]
    public class MktSponsorPackage : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PackageId { get; set; }

        [SugarColumn(Length = 100)]
        public string PackageCode { get; set; }

        [SugarColumn(Length = 200)]
        public string PackageName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string PackageNameAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Tier: title, diamond, platinum, gold, silver, bronze, custom
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Tier { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal Price { get; set; }

        [SugarColumn(Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// JSON: comprehensive benefit list with categories
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string BenefitsJson { get; set; }

        /// <summary>
        /// JSON: branding placements and visibility
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BrandingJson { get; set; }

        /// <summary>
        /// Number of complimentary tickets included
        /// </summary>
        public int ComplimentaryTickets { get; set; } = 0;

        /// <summary>
        /// Booth space in square meters
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? BoothSpaceSqm { get; set; }

        /// <summary>
        /// Speaking opportunity slots
        /// </summary>
        public int SpeakingSlots { get; set; } = 0;

        /// <summary>
        /// Logo placement positions count
        /// </summary>
        public int LogoPlacements { get; set; } = 0;

        /// <summary>
        /// Social media mentions included
        /// </summary>
        public int SocialMediaMentions { get; set; } = 0;

        /// <summary>
        /// Email blast inclusions
        /// </summary>
        public int EmailBlasts { get; set; } = 0;

        /// <summary>
        /// Access to attendee data/list
        /// </summary>
        public bool AttendeeDataAccess { get; set; } = false;

        /// <summary>
        /// Access to crowd analytics reports
        /// </summary>
        public bool CrowdAnalyticsAccess { get; set; } = false;

        /// <summary>
        /// Dedicated account manager
        /// </summary>
        public bool DedicatedManager { get; set; } = false;

        /// <summary>
        /// Priority display order (lower = higher priority)
        /// </summary>
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string CoverImageUrl { get; set; }

        [SugarColumn(Length = 30, IsNullable = true)]
        public string BadgeColor { get; set; }

        [SugarColumn(Length = 50, IsNullable = true)]
        public string BadgeIcon { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Sponsor Application - طلبات الرعاية
    /// Applications from sponsors/investors for opportunities or packages
    /// </summary>
    [SugarTable("mkt_sponsor_application")]
    public class MktSponsorApplication : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ApplicationId { get; set; }

        [SugarColumn(Length = 100)]
        public string ApplicationCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? OpportunityId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? PackageId { get; set; }

        /// <summary>
        /// Applicant type: company, individual
        /// </summary>
        [SugarColumn(Length = 20)]
        public string ApplicantType { get; set; }

        [SugarColumn(Length = 300)]
        public string CompanyName { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string CompanyNameAr { get; set; }

        [SugarColumn(Length = 200)]
        public string ContactPerson { get; set; }

        [SugarColumn(Length = 200)]
        public string ContactEmail { get; set; }

        [SugarColumn(Length = 50)]
        public string ContactPhone { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ContactTitle { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string Website { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string Industry { get; set; }

        /// <summary>
        /// Company size: startup, small, medium, large, enterprise
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string CompanySize { get; set; }

        /// <summary>
        /// Proposed investment amount
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal ProposedAmount { get; set; }

        [SugarColumn(Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// JSON: specific requirements or customizations requested
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string RequirementsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string MotivationNote { get; set; }

        /// <summary>
        /// Status: submitted, under_review, shortlisted, approved, rejected, withdrawn, contracted
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Status { get; set; } = "submitted";

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ReviewNotes { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ReviewedBy { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// AI match score (0-100) - how well this sponsor matches the opportunity
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AiMatchScore { get; set; }

        /// <summary>
        /// JSON: AI reasoning for match score
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiMatchReasonJson { get; set; }

        /// <summary>
        /// JSON: uploaded documents (logo, company profile, etc.)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DocumentsJson { get; set; }

        /// <summary>
        /// Contracted amount after negotiation
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? ContractedAmount { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ContractStartDate { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ContractEndDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Investor KPI Report - تقارير مؤشرات الأداء للمستثمرين
    /// Real-time KPI reports generated for sponsors and investors
    /// </summary>
    [SugarTable("mkt_investor_kpi_report")]
    public class MktInvestorKpiReport : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReportId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ApplicationId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        [SugarColumn(Length = 200)]
        public string ReportTitle { get; set; }

        /// <summary>
        /// Report period: daily, weekly, monthly, event_summary, custom
        /// </summary>
        [SugarColumn(Length = 20)]
        public string ReportPeriod { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Brand impressions within event
        /// </summary>
        public long BrandImpressions { get; set; } = 0;

        /// <summary>
        /// Booth/stand foot traffic
        /// </summary>
        public long FootTraffic { get; set; } = 0;

        /// <summary>
        /// Leads generated for sponsor
        /// </summary>
        public int LeadsGenerated { get; set; } = 0;

        /// <summary>
        /// Engagement interactions
        /// </summary>
        public int EngagementCount { get; set; } = 0;

        /// <summary>
        /// Social media mentions of sponsor
        /// </summary>
        public int SocialMentions { get; set; } = 0;

        /// <summary>
        /// Media coverage pieces
        /// </summary>
        public int MediaCoverage { get; set; } = 0;

        /// <summary>
        /// Estimated media value in SAR
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EstimatedMediaValue { get; set; }

        /// <summary>
        /// Calculated ROI for sponsor
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? CalculatedRoi { get; set; }

        /// <summary>
        /// Audience satisfaction score (1-5)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? SatisfactionScore { get; set; }

        /// <summary>
        /// JSON: detailed KPI breakdown
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DetailedKpisJson { get; set; }

        /// <summary>
        /// JSON: crowd behavior data near sponsor area
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string CrowdDataJson { get; set; }

        /// <summary>
        /// JSON: AI-generated insights and recommendations
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiInsightsJson { get; set; }

        /// <summary>
        /// Report delivery status: generating, ready, sent, viewed
        /// </summary>
        [SugarColumn(Length = 20)]
        public string DeliveryStatus { get; set; } = "generating";

        [SugarColumn(IsNullable = true)]
        public DateTime? SentAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ViewedAt { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    public static class SponsorTiers
    {
        public const string TITLE = "title";
        public const string DIAMOND = "diamond";
        public const string PLATINUM = "platinum";
        public const string GOLD = "gold";
        public const string SILVER = "silver";
        public const string BRONZE = "bronze";
        public const string CUSTOM = "custom";
    }

    public static class ApplicationStatus
    {
        public const string SUBMITTED = "submitted";
        public const string UNDER_REVIEW = "under_review";
        public const string SHORTLISTED = "shortlisted";
        public const string APPROVED = "approved";
        public const string REJECTED = "rejected";
        public const string WITHDRAWN = "withdrawn";
        public const string CONTRACTED = "contracted";
    }
}
