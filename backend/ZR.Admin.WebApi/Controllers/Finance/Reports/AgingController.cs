using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Service.IService.Finance.ARAP;

namespace ZR.Admin.WebApi.Controllers.Finance.ARAP
{
    /// <summary>
    /// Aging Controller - متحكم التقادم
    /// </summary>
    [Verify]
    [Route("finance/arap/aging")]
    [ApiController]
    public class AgingController : BaseController
    {
        private readonly IAgingService _agingService;

        public AgingController(IAgingService agingService)
        {
            _agingService = agingService;
        }

        /// <summary>
        /// Get customer aging report - الحصول على تقرير تقادم العملاء
        /// </summary>
        [HttpGet("customers/report")]
        [ActionPermissionFilter(Permission = "finance:arap:aging:report")]
        public IActionResult GetCustomerAgingReport([FromQuery] AgingQueryDto query)
        {
            var result = _agingService.GetCustomerAgingReport(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get vendor aging report - الحصول على تقرير تقادم الموردين
        /// </summary>
        [HttpGet("vendors/report")]
        [ActionPermissionFilter(Permission = "finance:arap:aging:report")]
        public IActionResult GetVendorAgingReport([FromQuery] AgingQueryDto query)
        {
            var result = _agingService.GetVendorAgingReport(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get customer aging summary - الحصول على ملخص تقادم العملاء
        /// </summary>
        [HttpGet("customers/summary")]
        [ActionPermissionFilter(Permission = "finance:arap:aging:report")]
        public IActionResult GetCustomerAgingSummary([FromQuery] AgingQueryDto query)
        {
            var result = _agingService.GetCustomerAgingSummary(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get vendor aging summary - الحصول على ملخص تقادم الموردين
        /// </summary>
        [HttpGet("vendors/summary")]
        [ActionPermissionFilter(Permission = "finance:arap:aging:report")]
        public IActionResult GetVendorAgingSummary([FromQuery] AgingQueryDto query)
        {
            var result = _agingService.GetVendorAgingSummary(query);
            return SUCCESS(result);
        }
    }
}


