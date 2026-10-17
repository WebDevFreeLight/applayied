using System;
using System.Collections.Generic;
using ZR.Model.Finance.ARAP.Dto;

namespace ZR.Service.IService.Finance.ARAP
{
    /// <summary>
    /// Aging Service Interface - واجهة خدمة التقادم
    /// </summary>
    public interface IAgingService
    {
        /// <summary>
        /// Get customer aging report - الحصول على تقرير تقادم العملاء
        /// </summary>
        List<AgingReportDto> GetCustomerAgingReport(AgingQueryDto query);

        /// <summary>
        /// Get vendor aging report - الحصول على تقرير تقادم الموردين
        /// </summary>
        List<AgingReportDto> GetVendorAgingReport(AgingQueryDto query);

        /// <summary>
        /// Get customer aging summary - الحصول على ملخص تقادم العملاء
        /// </summary>
        List<AgingSummaryDto> GetCustomerAgingSummary(AgingQueryDto query);

        /// <summary>
        /// Get vendor aging summary - الحصول على ملخص تقادم الموردين
        /// </summary>
        List<AgingSummaryDto> GetVendorAgingSummary(AgingQueryDto query);
    }
}


