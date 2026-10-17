using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model;
using ZR.Model.Marketing.Investors;
using ZR.Model.Marketing.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Investor Nexus Service Implementation - خدمة ربط المستثمرين
    /// Investment opportunity management, sponsor package management, application review workflow, KPI reports, analytics
    /// </summary>
    [AppService(ServiceType = typeof(IInvestorNexusService), ServiceLifetime = LifeTime.Transient)]
    public class InvestorNexusService : BaseService<MktInvestmentOpportunity>, IInvestorNexusService
    {
        #region Investment Opportunities

        /// <summary>
        /// Get investment opportunities with filtering
        /// </summary>
        public PagedInfo<InvestmentOpportunityOutputDto> GetOpportunities(InvestmentOpportunityQueryDto query)
        {
            var predicate = Expressionable.Create<MktInvestmentOpportunity>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Title), it => it.Title.Contains(query.Title));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.OpportunityType), it => it.OpportunityType == query.OpportunityType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Visibility), it => it.Visibility == query.Visibility);
            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.MinInvestmentFrom != null, it => it.MinInvestment >= query.MinInvestmentFrom);
            predicate = predicate.AndIF(query.MinInvestmentTo != null, it => it.MinInvestment <= query.MinInvestmentTo);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(o => o.Create_time)
                .Select(o => new InvestmentOpportunityOutputDto
                {
                    OpportunityId = o.OpportunityId,
                    OpportunityCode = o.OpportunityCode,
                    Title = o.Title,
                    TitleAr = o.TitleAr,
                    Description = o.Description,
                    DescriptionAr = o.DescriptionAr,
                    OpportunityType = o.OpportunityType,
                    EventId = o.EventId,
                    MinInvestment = o.MinInvestment,
                    MaxInvestment = o.MaxInvestment,
                    Currency = o.Currency,
                    ExpectedRoi = o.ExpectedRoi,
                    BenefitsJson = o.BenefitsJson,
                    DeliverablesJson = o.DeliverablesJson,
                    KpiMetricsJson = o.KpiMetricsJson,
                    MaxSponsors = o.MaxSponsors,
                    CurrentSponsors = o.CurrentSponsors,
                    Status = o.Status,
                    Visibility = o.Visibility,
                    TargetIndustriesJson = o.TargetIndustriesJson,
                    ApplicationDeadline = o.ApplicationDeadline,
                    AiMatchThreshold = o.AiMatchThreshold,
                    CoverImageUrl = o.CoverImageUrl,
                    BrochureUrl = o.BrochureUrl,
                    TotalRevenue = o.TotalRevenue,
                    ViewCount = o.ViewCount,
                    InquiryCount = o.InquiryCount,
                    IsAvailable = o.IsAvailable,
                    FillRate = o.FillRate
                })
                .ToPage(query);

            // Load application counts
            foreach (var item in response.Result)
            {
                item.ApplicationCount = Context.Queryable<MktSponsorApplication>()
                    .Where(a => a.OpportunityId == item.OpportunityId)
                    .Count();
            }

            return response;
        }

        /// <summary>
        /// Add a new investment opportunity
        /// </summary>
        public MktInvestmentOpportunity AddOpportunity(InvestmentOpportunityDto dto)
        {
            var opportunity = new MktInvestmentOpportunity
            {
                OpportunityCode = dto.OpportunityCode,
                Title = dto.Title,
                TitleAr = dto.TitleAr,
                Description = dto.Description,
                DescriptionAr = dto.DescriptionAr,
                OpportunityType = dto.OpportunityType,
                EventId = dto.EventId,
                MinInvestment = dto.MinInvestment,
                MaxInvestment = dto.MaxInvestment,
                Currency = dto.Currency ?? "SAR",
                ExpectedRoi = dto.ExpectedRoi,
                BenefitsJson = dto.BenefitsJson,
                DeliverablesJson = dto.DeliverablesJson,
                KpiMetricsJson = dto.KpiMetricsJson,
                MaxSponsors = dto.MaxSponsors,
                Status = dto.Status ?? "draft",
                Visibility = dto.Visibility ?? "public",
                TargetIndustriesJson = dto.TargetIndustriesJson,
                ApplicationDeadline = dto.ApplicationDeadline,
                AiMatchThreshold = dto.AiMatchThreshold,
                CoverImageUrl = dto.CoverImageUrl,
                BrochureUrl = dto.BrochureUrl
            };

            Insertable(opportunity).ExecuteReturnSnowflakeId();
            return opportunity;
        }

        /// <summary>
        /// Get opportunity detail by ID
        /// </summary>
        public InvestmentOpportunityOutputDto GetOpportunityDetail(long opportunityId)
        {
            var o = Queryable()
                .Where(x => x.OpportunityId == opportunityId)
                .First();

            if (o == null) return null;

            var dto = new InvestmentOpportunityOutputDto
            {
                OpportunityId = o.OpportunityId,
                OpportunityCode = o.OpportunityCode,
                Title = o.Title,
                TitleAr = o.TitleAr,
                Description = o.Description,
                DescriptionAr = o.DescriptionAr,
                OpportunityType = o.OpportunityType,
                EventId = o.EventId,
                MinInvestment = o.MinInvestment,
                MaxInvestment = o.MaxInvestment,
                Currency = o.Currency,
                ExpectedRoi = o.ExpectedRoi,
                BenefitsJson = o.BenefitsJson,
                DeliverablesJson = o.DeliverablesJson,
                KpiMetricsJson = o.KpiMetricsJson,
                MaxSponsors = o.MaxSponsors,
                CurrentSponsors = o.CurrentSponsors,
                Status = o.Status,
                Visibility = o.Visibility,
                TargetIndustriesJson = o.TargetIndustriesJson,
                ApplicationDeadline = o.ApplicationDeadline,
                AiMatchThreshold = o.AiMatchThreshold,
                CoverImageUrl = o.CoverImageUrl,
                BrochureUrl = o.BrochureUrl,
                TotalRevenue = o.TotalRevenue,
                ViewCount = o.ViewCount,
                InquiryCount = o.InquiryCount,
                IsAvailable = o.IsAvailable,
                FillRate = o.FillRate
            };

            dto.ApplicationCount = Context.Queryable<MktSponsorApplication>()
                .Where(a => a.OpportunityId == opportunityId)
                .Count();

            return dto;
        }

        /// <summary>
        /// Update an existing investment opportunity
        /// </summary>
        public int UpdateOpportunity(InvestmentOpportunityDto dto)
        {
            var opportunity = Queryable()
                .Where(x => x.OpportunityId == dto.OpportunityId)
                .First();

            if (opportunity == null) return 0;

            opportunity.OpportunityCode = dto.OpportunityCode;
            opportunity.Title = dto.Title;
            opportunity.TitleAr = dto.TitleAr;
            opportunity.Description = dto.Description;
            opportunity.DescriptionAr = dto.DescriptionAr;
            opportunity.OpportunityType = dto.OpportunityType;
            opportunity.EventId = dto.EventId;
            opportunity.MinInvestment = dto.MinInvestment;
            opportunity.MaxInvestment = dto.MaxInvestment;
            opportunity.Currency = dto.Currency;
            opportunity.ExpectedRoi = dto.ExpectedRoi;
            opportunity.BenefitsJson = dto.BenefitsJson;
            opportunity.DeliverablesJson = dto.DeliverablesJson;
            opportunity.KpiMetricsJson = dto.KpiMetricsJson;
            opportunity.MaxSponsors = dto.MaxSponsors;
            opportunity.Visibility = dto.Visibility;
            opportunity.TargetIndustriesJson = dto.TargetIndustriesJson;
            opportunity.ApplicationDeadline = dto.ApplicationDeadline;
            opportunity.AiMatchThreshold = dto.AiMatchThreshold;
            opportunity.CoverImageUrl = dto.CoverImageUrl;
            opportunity.BrochureUrl = dto.BrochureUrl;

            return Update(opportunity, true, "Update opportunity");
        }

        /// <summary>
        /// Delete opportunities by IDs
        /// </summary>
        public int DeleteOpportunities(long[] ids)
        {
            return Delete(ids);
        }

        /// <summary>
        /// Update opportunity status
        /// </summary>
        public int UpdateOpportunityStatus(long opportunityId, string status)
        {
            return Context.Updateable<MktInvestmentOpportunity>()
                .SetColumns(o => o.Status == status)
                .Where(o => o.OpportunityId == opportunityId)
                .ExecuteCommand();
        }

        #endregion

        #region Sponsor Packages

        /// <summary>
        /// Get sponsor packages with filtering
        /// </summary>
        public PagedInfo<SponsorPackageDto> GetPackages(SponsorPackageQueryDto query)
        {
            var predicate = Expressionable.Create<MktSponsorPackage>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.PackageName), it => it.PackageName.Contains(query.PackageName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Tier), it => it.Tier == query.Tier);
            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.IsActive != null, it => it.IsActive == query.IsActive);

            var response = Context.Queryable<MktSponsorPackage>()
                .Where(predicate.ToExpression())
                .OrderBy(p => p.SortOrder)
                .Select(p => new SponsorPackageDto
                {
                    PackageId = p.PackageId,
                    PackageCode = p.PackageCode,
                    PackageName = p.PackageName,
                    PackageNameAr = p.PackageNameAr,
                    Description = p.Description,
                    DescriptionAr = p.DescriptionAr,
                    Tier = p.Tier,
                    Price = p.Price,
                    Currency = p.Currency,
                    BenefitsJson = p.BenefitsJson,
                    BrandingJson = p.BrandingJson,
                    ComplimentaryTickets = p.ComplimentaryTickets,
                    BoothSpaceSqm = p.BoothSpaceSqm,
                    SpeakingSlots = p.SpeakingSlots,
                    LogoPlacements = p.LogoPlacements,
                    SocialMediaMentions = p.SocialMediaMentions,
                    EmailBlasts = p.EmailBlasts,
                    AttendeeDataAccess = p.AttendeeDataAccess,
                    CrowdAnalyticsAccess = p.CrowdAnalyticsAccess,
                    DedicatedManager = p.DedicatedManager,
                    SortOrder = p.SortOrder,
                    IsActive = p.IsActive,
                    CoverImageUrl = p.CoverImageUrl,
                    BadgeColor = p.BadgeColor,
                    BadgeIcon = p.BadgeIcon,
                    EventId = p.EventId
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Add a new sponsor package
        /// </summary>
        public MktSponsorPackage AddPackage(SponsorPackageDto dto)
        {
            var package = new MktSponsorPackage
            {
                PackageCode = dto.PackageCode,
                PackageName = dto.PackageName,
                PackageNameAr = dto.PackageNameAr,
                Description = dto.Description,
                DescriptionAr = dto.DescriptionAr,
                Tier = dto.Tier,
                Price = dto.Price,
                Currency = dto.Currency ?? "SAR",
                BenefitsJson = dto.BenefitsJson,
                BrandingJson = dto.BrandingJson,
                ComplimentaryTickets = dto.ComplimentaryTickets,
                BoothSpaceSqm = dto.BoothSpaceSqm,
                SpeakingSlots = dto.SpeakingSlots,
                LogoPlacements = dto.LogoPlacements,
                SocialMediaMentions = dto.SocialMediaMentions,
                EmailBlasts = dto.EmailBlasts,
                AttendeeDataAccess = dto.AttendeeDataAccess,
                CrowdAnalyticsAccess = dto.CrowdAnalyticsAccess,
                DedicatedManager = dto.DedicatedManager,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CoverImageUrl = dto.CoverImageUrl,
                BadgeColor = dto.BadgeColor,
                BadgeIcon = dto.BadgeIcon,
                EventId = dto.EventId
            };

            Context.Insertable(package).ExecuteReturnSnowflakeId();
            return package;
        }

        /// <summary>
        /// Get package detail by ID
        /// </summary>
        public SponsorPackageDto GetPackageDetail(long packageId)
        {
            var p = Context.Queryable<MktSponsorPackage>()
                .Where(x => x.PackageId == packageId)
                .First();

            if (p == null) return null;

            return new SponsorPackageDto
            {
                PackageId = p.PackageId,
                PackageCode = p.PackageCode,
                PackageName = p.PackageName,
                PackageNameAr = p.PackageNameAr,
                Description = p.Description,
                DescriptionAr = p.DescriptionAr,
                Tier = p.Tier,
                Price = p.Price,
                Currency = p.Currency,
                BenefitsJson = p.BenefitsJson,
                BrandingJson = p.BrandingJson,
                ComplimentaryTickets = p.ComplimentaryTickets,
                BoothSpaceSqm = p.BoothSpaceSqm,
                SpeakingSlots = p.SpeakingSlots,
                LogoPlacements = p.LogoPlacements,
                SocialMediaMentions = p.SocialMediaMentions,
                EmailBlasts = p.EmailBlasts,
                AttendeeDataAccess = p.AttendeeDataAccess,
                CrowdAnalyticsAccess = p.CrowdAnalyticsAccess,
                DedicatedManager = p.DedicatedManager,
                SortOrder = p.SortOrder,
                IsActive = p.IsActive,
                CoverImageUrl = p.CoverImageUrl,
                BadgeColor = p.BadgeColor,
                BadgeIcon = p.BadgeIcon,
                EventId = p.EventId
            };
        }

        /// <summary>
        /// Update an existing sponsor package
        /// </summary>
        public int UpdatePackage(SponsorPackageDto dto)
        {
            var package = Context.Queryable<MktSponsorPackage>()
                .Where(x => x.PackageId == dto.PackageId)
                .First();

            if (package == null) return 0;

            package.PackageCode = dto.PackageCode;
            package.PackageName = dto.PackageName;
            package.PackageNameAr = dto.PackageNameAr;
            package.Description = dto.Description;
            package.DescriptionAr = dto.DescriptionAr;
            package.Tier = dto.Tier;
            package.Price = dto.Price;
            package.Currency = dto.Currency;
            package.BenefitsJson = dto.BenefitsJson;
            package.BrandingJson = dto.BrandingJson;
            package.ComplimentaryTickets = dto.ComplimentaryTickets;
            package.BoothSpaceSqm = dto.BoothSpaceSqm;
            package.SpeakingSlots = dto.SpeakingSlots;
            package.LogoPlacements = dto.LogoPlacements;
            package.SocialMediaMentions = dto.SocialMediaMentions;
            package.EmailBlasts = dto.EmailBlasts;
            package.AttendeeDataAccess = dto.AttendeeDataAccess;
            package.CrowdAnalyticsAccess = dto.CrowdAnalyticsAccess;
            package.DedicatedManager = dto.DedicatedManager;
            package.SortOrder = dto.SortOrder;
            package.IsActive = dto.IsActive;
            package.CoverImageUrl = dto.CoverImageUrl;
            package.BadgeColor = dto.BadgeColor;
            package.BadgeIcon = dto.BadgeIcon;
            package.EventId = dto.EventId;

            return Context.Updateable(package).ExecuteCommand();
        }

        /// <summary>
        /// Delete packages by IDs
        /// </summary>
        public int DeletePackages(long[] ids)
        {
            return Context.Deleteable<MktSponsorPackage>()
                .In(ids)
                .ExecuteCommand();
        }

        #endregion

        #region Sponsor Applications

        /// <summary>
        /// Get sponsor applications with filtering
        /// </summary>
        public PagedInfo<SponsorApplicationOutputDto> GetApplications(SponsorApplicationQueryDto query)
        {
            var predicate = Expressionable.Create<MktSponsorApplication>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CompanyName), it => it.CompanyName.Contains(query.CompanyName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.OpportunityId != null, it => it.OpportunityId == query.OpportunityId);
            predicate = predicate.AndIF(query.PackageId != null, it => it.PackageId == query.PackageId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Industry), it => it.Industry == query.Industry);
            predicate = predicate.AndIF(query.SubmittedFrom != null, it => it.Create_time >= query.SubmittedFrom);
            predicate = predicate.AndIF(query.SubmittedTo != null, it => it.Create_time <= query.SubmittedTo);

            var response = Context.Queryable<MktSponsorApplication>()
                .Where(predicate.ToExpression())
                .OrderByDescending(a => a.Create_time)
                .Select(a => new SponsorApplicationOutputDto
                {
                    ApplicationId = a.ApplicationId,
                    ApplicationCode = a.ApplicationCode,
                    OpportunityId = a.OpportunityId,
                    PackageId = a.PackageId,
                    ApplicantType = a.ApplicantType,
                    CompanyName = a.CompanyName,
                    CompanyNameAr = a.CompanyNameAr,
                    ContactPerson = a.ContactPerson,
                    ContactEmail = a.ContactEmail,
                    ContactPhone = a.ContactPhone,
                    ContactTitle = a.ContactTitle,
                    Website = a.Website,
                    Industry = a.Industry,
                    CompanySize = a.CompanySize,
                    ProposedAmount = a.ProposedAmount,
                    Currency = a.Currency,
                    RequirementsJson = a.RequirementsJson,
                    MotivationNote = a.MotivationNote,
                    DocumentsJson = a.DocumentsJson,
                    Status = a.Status,
                    ReviewNotes = a.ReviewNotes,
                    ReviewedBy = a.ReviewedBy,
                    ReviewedAt = a.ReviewedAt,
                    AiMatchScore = a.AiMatchScore,
                    AiMatchReasonJson = a.AiMatchReasonJson,
                    ContractedAmount = a.ContractedAmount,
                    ContractStartDate = a.ContractStartDate,
                    ContractEndDate = a.ContractEndDate
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Submit a sponsor application
        /// </summary>
        public MktSponsorApplication SubmitApplication(SponsorApplicationDto dto)
        {
            var application = new MktSponsorApplication
            {
                ApplicationCode = dto.ApplicationCode,
                OpportunityId = dto.OpportunityId,
                PackageId = dto.PackageId,
                ApplicantType = dto.ApplicantType,
                CompanyName = dto.CompanyName,
                CompanyNameAr = dto.CompanyNameAr,
                ContactPerson = dto.ContactPerson,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                ContactTitle = dto.ContactTitle,
                Website = dto.Website,
                Industry = dto.Industry,
                CompanySize = dto.CompanySize,
                ProposedAmount = dto.ProposedAmount,
                Currency = dto.Currency ?? "SAR",
                RequirementsJson = dto.RequirementsJson,
                MotivationNote = dto.MotivationNote,
                DocumentsJson = dto.DocumentsJson,
                Status = ApplicationStatus.SUBMITTED
            };

            Context.Insertable(application).ExecuteReturnSnowflakeId();

            // Update opportunity inquiry count
            if (dto.OpportunityId.HasValue)
            {
                Context.Updateable<MktInvestmentOpportunity>()
                    .SetColumns(o => o.InquiryCount == o.InquiryCount + 1)
                    .Where(o => o.OpportunityId == dto.OpportunityId)
                    .ExecuteCommand();
            }

            return application;
        }

        /// <summary>
        /// Review a sponsor application
        /// </summary>
        public int ReviewApplication(ReviewApplicationDto dto, long reviewerId)
        {
            var application = Context.Queryable<MktSponsorApplication>()
                .Where(a => a.ApplicationId == dto.ApplicationId)
                .First();

            if (application == null) return 0;

            application.Status = dto.Status;
            application.ReviewNotes = dto.ReviewNotes;
            application.ReviewedBy = reviewerId;
            application.ReviewedAt = DateTime.Now;

            if (dto.ContractedAmount.HasValue)
                application.ContractedAmount = dto.ContractedAmount;
            if (dto.ContractStartDate.HasValue)
                application.ContractStartDate = dto.ContractStartDate;
            if (dto.ContractEndDate.HasValue)
                application.ContractEndDate = dto.ContractEndDate;

            var result = Context.Updateable(application).ExecuteCommand();

            // If approved, update opportunity sponsor count and revenue
            if (dto.Status == ApplicationStatus.APPROVED || dto.Status == ApplicationStatus.CONTRACTED)
            {
                if (application.OpportunityId.HasValue)
                {
                    var amount = dto.ContractedAmount ?? application.ProposedAmount;
                    Context.Updateable<MktInvestmentOpportunity>()
                        .SetColumns(o => o.CurrentSponsors == o.CurrentSponsors + 1)
                        .SetColumns(o => o.TotalRevenue == o.TotalRevenue + amount)
                        .Where(o => o.OpportunityId == application.OpportunityId)
                        .ExecuteCommand();
                }
            }

            return result;
        }

        /// <summary>
        /// Get application detail by ID
        /// </summary>
        public SponsorApplicationOutputDto GetApplicationDetail(long applicationId)
        {
            var a = Context.Queryable<MktSponsorApplication>()
                .Where(x => x.ApplicationId == applicationId)
                .First();

            if (a == null) return null;

            return new SponsorApplicationOutputDto
            {
                ApplicationId = a.ApplicationId,
                ApplicationCode = a.ApplicationCode,
                OpportunityId = a.OpportunityId,
                PackageId = a.PackageId,
                ApplicantType = a.ApplicantType,
                CompanyName = a.CompanyName,
                CompanyNameAr = a.CompanyNameAr,
                ContactPerson = a.ContactPerson,
                ContactEmail = a.ContactEmail,
                ContactPhone = a.ContactPhone,
                ContactTitle = a.ContactTitle,
                Website = a.Website,
                Industry = a.Industry,
                CompanySize = a.CompanySize,
                ProposedAmount = a.ProposedAmount,
                Currency = a.Currency,
                RequirementsJson = a.RequirementsJson,
                MotivationNote = a.MotivationNote,
                DocumentsJson = a.DocumentsJson,
                Status = a.Status,
                ReviewNotes = a.ReviewNotes,
                ReviewedBy = a.ReviewedBy,
                ReviewedAt = a.ReviewedAt,
                AiMatchScore = a.AiMatchScore,
                AiMatchReasonJson = a.AiMatchReasonJson,
                ContractedAmount = a.ContractedAmount,
                ContractStartDate = a.ContractStartDate,
                ContractEndDate = a.ContractEndDate
            };
        }

        /// <summary>
        /// Calculate AI match score for an application
        /// </summary>
        public decimal CalculateAiMatchScore(long applicationId)
        {
            var application = Context.Queryable<MktSponsorApplication>()
                .Where(a => a.ApplicationId == applicationId)
                .First();

            if (application == null) return 0;

            // Simple scoring logic based on available data
            decimal score = 50; // Base score

            if (!string.IsNullOrEmpty(application.Industry)) score += 10;
            if (application.ProposedAmount > 0) score += 10;
            if (!string.IsNullOrEmpty(application.Website)) score += 5;
            if (!string.IsNullOrEmpty(application.MotivationNote)) score += 10;
            if (!string.IsNullOrEmpty(application.DocumentsJson)) score += 15;

            score = Math.Min(score, 100);

            // Persist the score
            Context.Updateable<MktSponsorApplication>()
                .SetColumns(a => a.AiMatchScore == score)
                .Where(a => a.ApplicationId == applicationId)
                .ExecuteCommand();

            return score;
        }

        #endregion

        #region KPI Reports

        /// <summary>
        /// Get investor KPI reports
        /// </summary>
        public PagedInfo<InvestorKpiReportDto> GetInvestorKpis(InvestorKpiQueryDto query)
        {
            var predicate = Expressionable.Create<MktInvestorKpiReport>();

            predicate = predicate.AndIF(query.ApplicationId != null, it => it.ApplicationId == query.ApplicationId);
            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ReportPeriod), it => it.ReportPeriod == query.ReportPeriod);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.DeliveryStatus), it => it.DeliveryStatus == query.DeliveryStatus);
            predicate = predicate.AndIF(query.PeriodFrom != null, it => it.PeriodStart >= query.PeriodFrom);
            predicate = predicate.AndIF(query.PeriodTo != null, it => it.PeriodEnd <= query.PeriodTo);

            var response = Context.Queryable<MktInvestorKpiReport>()
                .Where(predicate.ToExpression())
                .OrderByDescending(r => r.Create_time)
                .Select(r => new InvestorKpiReportDto
                {
                    ReportId = r.ReportId,
                    ApplicationId = r.ApplicationId,
                    EventId = r.EventId,
                    ReportTitle = r.ReportTitle,
                    ReportPeriod = r.ReportPeriod,
                    PeriodStart = r.PeriodStart,
                    PeriodEnd = r.PeriodEnd,
                    BrandImpressions = r.BrandImpressions,
                    FootTraffic = r.FootTraffic,
                    LeadsGenerated = r.LeadsGenerated,
                    EngagementCount = r.EngagementCount,
                    SocialMentions = r.SocialMentions,
                    MediaCoverage = r.MediaCoverage,
                    EstimatedMediaValue = r.EstimatedMediaValue,
                    CalculatedRoi = r.CalculatedRoi,
                    SatisfactionScore = r.SatisfactionScore,
                    DetailedKpisJson = r.DetailedKpisJson,
                    CrowdDataJson = r.CrowdDataJson,
                    AiInsightsJson = r.AiInsightsJson,
                    DeliveryStatus = r.DeliveryStatus,
                    SentAt = r.SentAt,
                    ViewedAt = r.ViewedAt
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Generate a KPI report for a sponsor
        /// </summary>
        public MktInvestorKpiReport GenerateKpiReport(long applicationId, string reportPeriod, DateTime periodStart, DateTime periodEnd)
        {
            var application = Context.Queryable<MktSponsorApplication>()
                .Where(a => a.ApplicationId == applicationId)
                .First();

            var report = new MktInvestorKpiReport
            {
                ApplicationId = applicationId,
                ReportTitle = $"KPI Report - {application?.CompanyName ?? "Sponsor"} - {reportPeriod}",
                ReportPeriod = reportPeriod,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                DeliveryStatus = "generating"
            };

            Context.Insertable(report).ExecuteReturnSnowflakeId();

            // Mark as ready (in real implementation, this would be async after data aggregation)
            Context.Updateable<MktInvestorKpiReport>()
                .SetColumns(r => r.DeliveryStatus == "ready")
                .Where(r => r.ReportId == report.ReportId)
                .ExecuteCommand();

            return report;
        }

        /// <summary>
        /// Get KPI report detail by ID
        /// </summary>
        public InvestorKpiReportDto GetKpiReportDetail(long reportId)
        {
            var r = Context.Queryable<MktInvestorKpiReport>()
                .Where(x => x.ReportId == reportId)
                .First();

            if (r == null) return null;

            return new InvestorKpiReportDto
            {
                ReportId = r.ReportId,
                ApplicationId = r.ApplicationId,
                EventId = r.EventId,
                ReportTitle = r.ReportTitle,
                ReportPeriod = r.ReportPeriod,
                PeriodStart = r.PeriodStart,
                PeriodEnd = r.PeriodEnd,
                BrandImpressions = r.BrandImpressions,
                FootTraffic = r.FootTraffic,
                LeadsGenerated = r.LeadsGenerated,
                EngagementCount = r.EngagementCount,
                SocialMentions = r.SocialMentions,
                MediaCoverage = r.MediaCoverage,
                EstimatedMediaValue = r.EstimatedMediaValue,
                CalculatedRoi = r.CalculatedRoi,
                SatisfactionScore = r.SatisfactionScore,
                DetailedKpisJson = r.DetailedKpisJson,
                CrowdDataJson = r.CrowdDataJson,
                AiInsightsJson = r.AiInsightsJson,
                DeliveryStatus = r.DeliveryStatus,
                SentAt = r.SentAt,
                ViewedAt = r.ViewedAt
            };
        }

        /// <summary>
        /// Send a KPI report to the investor
        /// </summary>
        public int SendKpiReport(long reportId)
        {
            return Context.Updateable<MktInvestorKpiReport>()
                .SetColumns(r => r.DeliveryStatus == "sent")
                .SetColumns(r => r.SentAt == DateTime.Now)
                .Where(r => r.ReportId == reportId)
                .ExecuteCommand();
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get investor analytics summary
        /// </summary>
        public InvestorAnalyticsDto GetInvestorAnalytics()
        {
            var opportunities = Queryable().ToList();
            var applications = Context.Queryable<MktSponsorApplication>().ToList();
            var packages = Context.Queryable<MktSponsorPackage>().ToList();

            var approvedApps = applications.Where(a =>
                a.Status == ApplicationStatus.APPROVED || a.Status == ApplicationStatus.CONTRACTED).ToList();

            var totalContracted = approvedApps
                .Where(a => a.ContractedAmount.HasValue)
                .Sum(a => a.ContractedAmount.Value);

            var analytics = new InvestorAnalyticsDto
            {
                TotalOpportunities = opportunities.Count,
                OpenOpportunities = opportunities.Count(o => o.Status == "published"),
                FilledOpportunities = opportunities.Count(o => o.Status == "filled"),
                TotalApplications = applications.Count,
                ApprovedApplications = approvedApps.Count,
                TotalContractedRevenue = totalContracted,
                AvgContractValue = approvedApps.Count > 0 ? totalContracted / approvedApps.Count : 0,
                ConversionRate = applications.Count > 0 ? (decimal)approvedApps.Count / applications.Count * 100 : 0,

                TierDistribution = packages
                    .GroupBy(p => p.Tier)
                    .Select(g => new InvestorTierDistributionDto
                    {
                        Tier = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(p => p.Price)
                    })
                    .ToList(),

                IndustryDistribution = applications
                    .Where(a => !string.IsNullOrEmpty(a.Industry))
                    .GroupBy(a => a.Industry)
                    .Select(g => new IndustryDistributionDto
                    {
                        Industry = g.Key,
                        Count = g.Count(),
                        AvgInvestment = g.Average(a => a.ProposedAmount)
                    })
                    .ToList(),

                MonthlyRevenue = approvedApps
                    .Where(a => a.ContractedAmount.HasValue && a.ReviewedAt.HasValue)
                    .GroupBy(a => new { a.ReviewedAt.Value.Year, a.ReviewedAt.Value.Month })
                    .Select(g => new MonthlyRevenueDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(a => a.ContractedAmount.Value),
                        Applications = g.Count()
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList()
            };

            return analytics;
        }

        #endregion
    }
}
