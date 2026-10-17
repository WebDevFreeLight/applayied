using Microsoft.AspNetCore.Mvc;
using System;
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
    /// Crowd-Sourced Marketing Controller - التسويق عبر إدارة الحشود
    /// Real-time crowd analytics, heatmaps, exhibitor reports, and AI insights
    /// </summary>
    [Verify]
    [Route("api/marketing/crowd")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class CrowdMarketingController : BaseController
    {
        private readonly ICrowdMarketingService _crowdService;

        public CrowdMarketingController(ICrowdMarketingService crowdService)
        {
            _crowdService = crowdService;
        }

        #region Crowd Analytics

        /// <summary>
        /// Get crowd analytics data points
        /// </summary>
        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:crowd:analytics")]
        public IActionResult GetCrowdAnalytics([FromQuery] CrowdAnalyticsQueryDto dto)
        {
            var result = _crowdService.GetCrowdAnalytics(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get real-time crowd summary for an event
        /// </summary>
        [HttpGet("summary/{eventId}")]
        [ActionPermissionFilter("marketing:crowd:analytics")]
        public IActionResult GetCrowdSummary(long eventId)
        {
            var result = _crowdService.GetCrowdSummary(eventId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Record crowd analytics data (from IoT/sensors)
        /// </summary>
        [HttpPost("analytics")]
        [ActionPermissionFilter("marketing:crowd:analytics:add")]
        public IActionResult RecordCrowdData([FromBody] CrowdAnalyticsDto dto)
        {
            var result = _crowdService.RecordCrowdData(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get zone-level analytics
        /// </summary>
        [HttpGet("analytics/zones/{eventId}")]
        [ActionPermissionFilter("marketing:crowd:analytics")]
        public IActionResult GetZoneAnalytics(long eventId)
        {
            var result = _crowdService.GetZoneAnalytics(eventId);
            return SUCCESS(result);
        }

        #endregion

        #region Heatmaps

        /// <summary>
        /// Get heatmap snapshots
        /// </summary>
        [HttpGet("heatmaps")]
        [ActionPermissionFilter("marketing:crowd:heatmaps")]
        public IActionResult GetHeatmaps([FromQuery] HeatmapQueryDto dto)
        {
            var result = _crowdService.GetHeatmaps(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get latest heatmap for event
        /// </summary>
        [HttpGet("heatmaps/latest/{eventId}")]
        [ActionPermissionFilter("marketing:crowd:heatmaps")]
        public IActionResult GetLatestHeatmap(long eventId, [FromQuery] string floorLevel)
        {
            var result = _crowdService.GetLatestHeatmap(eventId, floorLevel);
            return SUCCESS(result);
        }

        /// <summary>
        /// Record heatmap snapshot
        /// </summary>
        [HttpPost("heatmaps")]
        [ActionPermissionFilter("marketing:crowd:heatmaps:add")]
        public IActionResult RecordHeatmap([FromBody] HeatmapDto dto)
        {
            var result = _crowdService.RecordHeatmap(dto);
            return SUCCESS(result);
        }

        #endregion

        #region Exhibitor Reports

        /// <summary>
        /// Get exhibitor marketing reports
        /// </summary>
        [HttpGet("exhibitor-reports")]
        [ActionPermissionFilter("marketing:crowd:reports")]
        public IActionResult GetExhibitorReports([FromQuery] ExhibitorReportQueryDto dto)
        {
            var result = _crowdService.GetExhibitorReports(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get specific exhibitor report
        /// </summary>
        [HttpGet("exhibitor-reports/{reportId}")]
        [ActionPermissionFilter("marketing:crowd:reports")]
        public IActionResult GetExhibitorReportDetail(long reportId)
        {
            var result = _crowdService.GetExhibitorReportDetail(reportId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate new exhibitor report
        /// </summary>
        [HttpPost("exhibitor-reports/generate")]
        [ActionPermissionFilter("marketing:crowd:reports:generate")]
        [Log(Title = "Exhibitor Report Generation", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateExhibitorReport([FromBody] GenerateExhibitorReportRequest dto)
        {
            var result = _crowdService.GenerateExhibitorReport(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Deliver report to exhibitor
        /// </summary>
        [HttpPost("exhibitor-reports/{reportId}/deliver")]
        [ActionPermissionFilter("marketing:crowd:reports:deliver")]
        [Log(Title = "Report Delivery", BusinessType = BusinessType.OTHER)]
        public IActionResult DeliverReport(long reportId)
        {
            var result = _crowdService.DeliverExhibitorReport(reportId);
            return ToResponse(result);
        }

        /// <summary>
        /// Rate exhibitor report
        /// </summary>
        [HttpPost("exhibitor-reports/{reportId}/rate")]
        [ActionPermissionFilter("marketing:crowd:reports")]
        public IActionResult RateReport(long reportId, [FromBody] decimal rating)
        {
            var result = _crowdService.RateExhibitorReport(reportId, rating);
            return ToResponse(result);
        }

        #endregion

        #region Real-Time Insights

        /// <summary>
        /// Get AI-generated real-time insights
        /// </summary>
        [HttpGet("insights")]
        [ActionPermissionFilter("marketing:crowd:insights")]
        public IActionResult GetInsights([FromQuery] RealtimeInsightQueryDto dto)
        {
            var result = _crowdService.GetRealtimeInsights(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get active insights for an event (non-expired)
        /// </summary>
        [HttpGet("insights/active/{eventId}")]
        [ActionPermissionFilter("marketing:crowd:insights")]
        public IActionResult GetActiveInsights(long eventId)
        {
            var result = _crowdService.GetActiveInsights(eventId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Mark insight as actioned
        /// </summary>
        [HttpPost("insights/action")]
        [ActionPermissionFilter("marketing:crowd:insights:action")]
        [Log(Title = "Insight Action", BusinessType = BusinessType.UPDATE)]
        public IActionResult ActionInsight([FromBody] ActionInsightDto dto)
        {
            var result = _crowdService.ActionInsight(dto);
            return ToResponse(result);
        }

        #endregion
    }
}
