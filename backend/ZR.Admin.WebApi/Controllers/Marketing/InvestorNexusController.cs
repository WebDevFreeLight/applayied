using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.Marketing.Dto;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Investor Nexus Controller - منظومة ربط التاجر والمستثمر
    /// Investment opportunities, sponsor packages, applications, KPI reports
    /// </summary>
    [Verify]
    [Route("api/marketing/investors")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class InvestorNexusController : BaseController
    {
        private readonly IInvestorNexusService _investorService;

        public InvestorNexusController(IInvestorNexusService investorService)
        {
            _investorService = investorService;
        }

        #region Investment Opportunities

        [HttpGet("opportunities")]
        [ActionPermissionFilter("marketing:investor:opportunities")]
        public IActionResult GetOpportunities([FromQuery] InvestmentOpportunityQueryDto dto)
        {
            var result = _investorService.GetOpportunities(dto);
            return SUCCESS(result);
        }

        [HttpGet("opportunities/{opportunityId}")]
        [ActionPermissionFilter("marketing:investor:opportunities")]
        public IActionResult GetOpportunityDetail(long opportunityId)
        {
            var result = _investorService.GetOpportunityDetail(opportunityId);
            if (result == null) return ToResponse(ResultCode.FAIL, "Opportunity not found");
            return SUCCESS(result);
        }

        [HttpPost("opportunities")]
        [ActionPermissionFilter("marketing:investor:opportunities:add")]
        [Log(Title = "Investment Opportunity", BusinessType = BusinessType.INSERT)]
        public IActionResult AddOpportunity([FromBody] InvestmentOpportunityDto dto)
        {
            var result = _investorService.AddOpportunity(dto);
            return SUCCESS(result);
        }

        [HttpPut("opportunities")]
        [ActionPermissionFilter("marketing:investor:opportunities:edit")]
        [Log(Title = "Investment Opportunity", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateOpportunity([FromBody] InvestmentOpportunityDto dto)
        {
            var result = _investorService.UpdateOpportunity(dto);
            return ToResponse(result);
        }

        [HttpDelete("opportunities/{ids}")]
        [ActionPermissionFilter("marketing:investor:opportunities:delete")]
        [Log(Title = "Investment Opportunity", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteOpportunities(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _investorService.DeleteOpportunities(idArray);
            return ToResponse(result);
        }

        [HttpPut("opportunities/{id}/status")]
        [ActionPermissionFilter("marketing:investor:opportunities:edit")]
        [Log(Title = "Opportunity Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateOpportunityStatus(long id, [FromBody] string status)
        {
            var result = _investorService.UpdateOpportunityStatus(id, status);
            return ToResponse(result);
        }

        #endregion

        #region Sponsor Packages

        [HttpGet("packages")]
        [ActionPermissionFilter("marketing:investor:packages")]
        public IActionResult GetPackages([FromQuery] SponsorPackageQueryDto dto)
        {
            var result = _investorService.GetPackages(dto);
            return SUCCESS(result);
        }

        [HttpGet("packages/{packageId}")]
        [ActionPermissionFilter("marketing:investor:packages")]
        public IActionResult GetPackageDetail(long packageId)
        {
            var result = _investorService.GetPackageDetail(packageId);
            return SUCCESS(result);
        }

        [HttpPost("packages")]
        [ActionPermissionFilter("marketing:investor:packages:add")]
        [Log(Title = "Sponsor Package", BusinessType = BusinessType.INSERT)]
        public IActionResult AddPackage([FromBody] SponsorPackageDto dto)
        {
            var result = _investorService.AddPackage(dto);
            return SUCCESS(result);
        }

        [HttpPut("packages")]
        [ActionPermissionFilter("marketing:investor:packages:edit")]
        [Log(Title = "Sponsor Package", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdatePackage([FromBody] SponsorPackageDto dto)
        {
            var result = _investorService.UpdatePackage(dto);
            return ToResponse(result);
        }

        [HttpDelete("packages/{ids}")]
        [ActionPermissionFilter("marketing:investor:packages:delete")]
        [Log(Title = "Sponsor Package", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePackages(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _investorService.DeletePackages(idArray);
            return ToResponse(result);
        }

        #endregion

        #region Sponsor Applications

        [HttpGet("applications")]
        [ActionPermissionFilter("marketing:investor:applications")]
        public IActionResult GetApplications([FromQuery] SponsorApplicationQueryDto dto)
        {
            var result = _investorService.GetApplications(dto);
            return SUCCESS(result);
        }

        [HttpGet("applications/{applicationId}")]
        [ActionPermissionFilter("marketing:investor:applications")]
        public IActionResult GetApplicationDetail(long applicationId)
        {
            var result = _investorService.GetApplicationDetail(applicationId);
            return SUCCESS(result);
        }

        [HttpPost("applications")]
        [ActionPermissionFilter("marketing:investor:applications:add")]
        [Log(Title = "Sponsor Application", BusinessType = BusinessType.INSERT)]
        public IActionResult SubmitApplication([FromBody] SponsorApplicationDto dto)
        {
            var result = _investorService.SubmitApplication(dto);
            return SUCCESS(result);
        }

        [HttpPost("applications/review")]
        [ActionPermissionFilter("marketing:investor:applications:review")]
        [Log(Title = "Application Review", BusinessType = BusinessType.UPDATE)]
        public IActionResult ReviewApplication([FromBody] ReviewApplicationDto dto)
        {
            var result = _investorService.ReviewApplication(dto, HttpContext.GetUId());
            return ToResponse(result);
        }

        [HttpGet("applications/{applicationId}/ai-match")]
        [ActionPermissionFilter("marketing:investor:applications")]
        public IActionResult GetAiMatchScore(long applicationId)
        {
            var result = _investorService.CalculateAiMatchScore(applicationId);
            return SUCCESS(result);
        }

        #endregion

        #region Investor KPI Reports

        [HttpGet("kpi-reports")]
        [ActionPermissionFilter("marketing:investor:kpi")]
        public IActionResult GetKpiReports([FromQuery] InvestorKpiQueryDto dto)
        {
            var result = _investorService.GetInvestorKpis(dto);
            return SUCCESS(result);
        }

        [HttpGet("kpi-reports/{reportId}")]
        [ActionPermissionFilter("marketing:investor:kpi")]
        public IActionResult GetKpiReportDetail(long reportId)
        {
            var result = _investorService.GetKpiReportDetail(reportId);
            return SUCCESS(result);
        }

        [HttpPost("kpi-reports/generate")]
        [ActionPermissionFilter("marketing:investor:kpi:generate")]
        [Log(Title = "KPI Report Generation", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateKpiReport([FromQuery] long applicationId, [FromQuery] string period,
            [FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
        {
            var result = _investorService.GenerateKpiReport(applicationId, period, periodStart, periodEnd);
            return SUCCESS(result);
        }

        [HttpPost("kpi-reports/{reportId}/send")]
        [ActionPermissionFilter("marketing:investor:kpi:send")]
        [Log(Title = "KPI Report Delivery", BusinessType = BusinessType.OTHER)]
        public IActionResult SendKpiReport(long reportId)
        {
            var result = _investorService.SendKpiReport(reportId);
            return ToResponse(result);
        }

        #endregion

        #region Analytics

        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:investor:analytics")]
        public IActionResult GetInvestorAnalytics()
        {
            var result = _investorService.GetInvestorAnalytics();
            return SUCCESS(result);
        }

        [HttpGet("export")]
        [ActionPermissionFilter("marketing:investor:export")]
        [Log(Title = "Investor Export", BusinessType = BusinessType.EXPORT)]
        public IActionResult ExportApplications([FromQuery] SponsorApplicationQueryDto dto)
        {
            var list = _investorService.GetApplications(dto);
            var result = ExportExcelMini(list.Result, "SponsorApplications", "SponsorApplications");
            return ExportExcel(result.Item2, result.Item1);
        }

        #endregion
    }
}
