using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Marketing.Investors;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Investor Nexus Service Interface - خدمة ربط المستثمرين
    /// </summary>
    public interface IInvestorNexusService : IBaseService<MktInvestmentOpportunity>
    {
        #region Investment Opportunities

        /// <summary>
        /// Get investment opportunities with filtering
        /// </summary>
        PagedInfo<InvestmentOpportunityOutputDto> GetOpportunities(InvestmentOpportunityQueryDto query);

        /// <summary>
        /// Get opportunity detail by ID
        /// </summary>
        InvestmentOpportunityOutputDto GetOpportunityDetail(long opportunityId);

        /// <summary>
        /// Add a new investment opportunity
        /// </summary>
        MktInvestmentOpportunity AddOpportunity(InvestmentOpportunityDto dto);

        /// <summary>
        /// Update an existing investment opportunity
        /// </summary>
        int UpdateOpportunity(InvestmentOpportunityDto dto);

        /// <summary>
        /// Delete opportunities by IDs
        /// </summary>
        int DeleteOpportunities(long[] ids);

        /// <summary>
        /// Update opportunity status
        /// </summary>
        int UpdateOpportunityStatus(long opportunityId, string status);

        #endregion

        #region Sponsor Packages

        /// <summary>
        /// Get sponsor packages with filtering
        /// </summary>
        PagedInfo<SponsorPackageDto> GetPackages(SponsorPackageQueryDto query);

        /// <summary>
        /// Get package detail by ID
        /// </summary>
        SponsorPackageDto GetPackageDetail(long packageId);

        /// <summary>
        /// Add a new sponsor package
        /// </summary>
        MktSponsorPackage AddPackage(SponsorPackageDto dto);

        /// <summary>
        /// Update an existing sponsor package
        /// </summary>
        int UpdatePackage(SponsorPackageDto dto);

        /// <summary>
        /// Delete packages by IDs
        /// </summary>
        int DeletePackages(long[] ids);

        #endregion

        #region Sponsor Applications

        /// <summary>
        /// Get sponsor applications with filtering
        /// </summary>
        PagedInfo<SponsorApplicationOutputDto> GetApplications(SponsorApplicationQueryDto query);

        /// <summary>
        /// Get application detail by ID
        /// </summary>
        SponsorApplicationOutputDto GetApplicationDetail(long applicationId);

        /// <summary>
        /// Submit a sponsor application
        /// </summary>
        MktSponsorApplication SubmitApplication(SponsorApplicationDto dto);

        /// <summary>
        /// Review a sponsor application
        /// </summary>
        int ReviewApplication(ReviewApplicationDto dto, long reviewerId);

        /// <summary>
        /// Calculate AI match score for an application
        /// </summary>
        decimal CalculateAiMatchScore(long applicationId);

        #endregion

        #region KPI Reports

        /// <summary>
        /// Get investor KPI reports
        /// </summary>
        PagedInfo<InvestorKpiReportDto> GetInvestorKpis(InvestorKpiQueryDto query);

        /// <summary>
        /// Get KPI report detail by ID
        /// </summary>
        InvestorKpiReportDto GetKpiReportDetail(long reportId);

        /// <summary>
        /// Generate a KPI report for a sponsor
        /// </summary>
        MktInvestorKpiReport GenerateKpiReport(long applicationId, string reportPeriod, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Send a KPI report to the investor
        /// </summary>
        int SendKpiReport(long reportId);

        #endregion

        #region Analytics

        /// <summary>
        /// Get investor analytics summary
        /// </summary>
        InvestorAnalyticsDto GetInvestorAnalytics();

        #endregion
    }
}
