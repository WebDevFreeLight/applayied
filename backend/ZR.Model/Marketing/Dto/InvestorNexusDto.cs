using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Investment Opportunity DTOs

    public class InvestmentOpportunityQueryDto : PagerInfo
    {
        public string Title { get; set; }
        public string OpportunityType { get; set; }
        public string Status { get; set; }
        public string Visibility { get; set; }
        public long? EventId { get; set; }
        public decimal? MinInvestmentFrom { get; set; }
        public decimal? MinInvestmentTo { get; set; }
    }

    public class InvestmentOpportunityDto
    {
        public long OpportunityId { get; set; }
        public string OpportunityCode { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string OpportunityType { get; set; }
        public long? EventId { get; set; }
        public decimal MinInvestment { get; set; }
        public decimal MaxInvestment { get; set; }
        public string Currency { get; set; }
        public decimal? ExpectedRoi { get; set; }
        public string BenefitsJson { get; set; }
        public string DeliverablesJson { get; set; }
        public string KpiMetricsJson { get; set; }
        public int MaxSponsors { get; set; }
        public int CurrentSponsors { get; set; }
        public string Status { get; set; }
        public string Visibility { get; set; }
        public string TargetIndustriesJson { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public decimal? AiMatchThreshold { get; set; }
        public string CoverImageUrl { get; set; }
        public string BrochureUrl { get; set; }
    }

    public class InvestmentOpportunityOutputDto : InvestmentOpportunityDto
    {
        public decimal TotalRevenue { get; set; }
        public int ViewCount { get; set; }
        public int InquiryCount { get; set; }
        public bool IsAvailable { get; set; }
        public decimal FillRate { get; set; }
        public string EventName { get; set; }
        public int ApplicationCount { get; set; }
    }

    #endregion

    #region Sponsor Package DTOs

    public class SponsorPackageQueryDto : PagerInfo
    {
        public string PackageName { get; set; }
        public string Tier { get; set; }
        public long? EventId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SponsorPackageDto
    {
        public long PackageId { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        public string PackageNameAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Tier { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string BenefitsJson { get; set; }
        public string BrandingJson { get; set; }
        public int ComplimentaryTickets { get; set; }
        public decimal? BoothSpaceSqm { get; set; }
        public int SpeakingSlots { get; set; }
        public int LogoPlacements { get; set; }
        public int SocialMediaMentions { get; set; }
        public int EmailBlasts { get; set; }
        public bool AttendeeDataAccess { get; set; }
        public bool CrowdAnalyticsAccess { get; set; }
        public bool DedicatedManager { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string CoverImageUrl { get; set; }
        public string BadgeColor { get; set; }
        public string BadgeIcon { get; set; }
        public long? EventId { get; set; }
    }

    #endregion

    #region Sponsor Application DTOs

    public class SponsorApplicationQueryDto : PagerInfo
    {
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public long? OpportunityId { get; set; }
        public long? PackageId { get; set; }
        public string Industry { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
    }

    public class SponsorApplicationDto
    {
        public long ApplicationId { get; set; }
        public string ApplicationCode { get; set; }
        public long? OpportunityId { get; set; }
        public long? PackageId { get; set; }
        public string ApplicantType { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNameAr { get; set; }
        public string ContactPerson { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactTitle { get; set; }
        public string Website { get; set; }
        public string Industry { get; set; }
        public string CompanySize { get; set; }
        public decimal ProposedAmount { get; set; }
        public string Currency { get; set; }
        public string RequirementsJson { get; set; }
        public string MotivationNote { get; set; }
        public string DocumentsJson { get; set; }
    }

    public class SponsorApplicationOutputDto : SponsorApplicationDto
    {
        public string Status { get; set; }
        public string ReviewNotes { get; set; }
        public long? ReviewedBy { get; set; }
        public string ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public decimal? AiMatchScore { get; set; }
        public string AiMatchReasonJson { get; set; }
        public decimal? ContractedAmount { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string OpportunityTitle { get; set; }
        public string PackageName { get; set; }
    }

    public class ReviewApplicationDto
    {
        public long ApplicationId { get; set; }
        public string Status { get; set; }
        public string ReviewNotes { get; set; }
        public decimal? ContractedAmount { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
    }

    #endregion

    #region Investor KPI Report DTOs

    public class InvestorKpiQueryDto : PagerInfo
    {
        public long? ApplicationId { get; set; }
        public long? EventId { get; set; }
        public string ReportPeriod { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
    }

    public class InvestorKpiReportDto
    {
        public long ReportId { get; set; }
        public long ApplicationId { get; set; }
        public string SponsorName { get; set; }
        public long? EventId { get; set; }
        public string EventName { get; set; }
        public string ReportTitle { get; set; }
        public string ReportPeriod { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public long BrandImpressions { get; set; }
        public long FootTraffic { get; set; }
        public int LeadsGenerated { get; set; }
        public int EngagementCount { get; set; }
        public int SocialMentions { get; set; }
        public int MediaCoverage { get; set; }
        public decimal? EstimatedMediaValue { get; set; }
        public decimal? CalculatedRoi { get; set; }
        public decimal? SatisfactionScore { get; set; }
        public string DetailedKpisJson { get; set; }
        public string CrowdDataJson { get; set; }
        public string AiInsightsJson { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ViewedAt { get; set; }
    }

    #endregion

    #region Investor Analytics DTOs

    public class InvestorAnalyticsDto
    {
        public int TotalOpportunities { get; set; }
        public int OpenOpportunities { get; set; }
        public int FilledOpportunities { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public decimal TotalContractedRevenue { get; set; }
        public decimal AvgContractValue { get; set; }
        public decimal ConversionRate { get; set; }
        public List<InvestorTierDistributionDto> TierDistribution { get; set; }
        public List<IndustryDistributionDto> IndustryDistribution { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; }
    }

    public class InvestorTierDistributionDto
    {
        public string Tier { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class IndustryDistributionDto
    {
        public string Industry { get; set; }
        public int Count { get; set; }
        public decimal AvgInvestment { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int Applications { get; set; }
    }

    #endregion
}
