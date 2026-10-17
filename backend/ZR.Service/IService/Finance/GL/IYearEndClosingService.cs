using System;
using System.Threading.Tasks;
using ZR.Model.Finance.GL.Dto;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// Year-End Closing Service Interface - واجهة خدمة إغلاق نهاية السنة
    /// Handles fiscal year closing with P&L transfer to Retained Earnings
    /// يعالج إغلاق السنة المالية مع تحويل الأرباح والخسائر إلى الأرباح المحتجزة
    /// </summary>
    public interface IYearEndClosingService
    {
        /// <summary>
        /// Close fiscal year and transfer Profit/Loss to Retained Earnings
        /// إغلاق السنة المالية وتحويل الربح/الخسارة إلى الأرباح المحتجزة
        /// </summary>
        /// <param name="dto">Closing parameters</param>
        /// <param name="userName">User performing the closing</param>
        /// <returns>Result of the closing operation</returns>
        Task<YearEndClosingResultDto> CloseFiscalYearAndTransferProfitLossAsync(
            CloseFiscalYearDto dto, string userName);

        /// <summary>
        /// Preview year-end closing (dry run)
        /// معاينة إغلاق نهاية السنة (تشغيل تجريبي)
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to preview</param>
        /// <param name="companyId">Optional company ID</param>
        /// <returns>Preview of what would happen on closing</returns>
        Task<YearEndClosingPreviewDto> PreviewYearEndClosingAsync(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Validate fiscal year for closing
        /// التحقق من صلاحية إغلاق السنة المالية
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to validate</param>
        /// <param name="companyId">Optional company ID</param>
        /// <returns>Validation result with details</returns>
        Task<YearEndClosingValidationDto> ValidateForClosingAsync(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get Retained Earnings Account for company
        /// الحصول على حساب الأرباح المحتجزة للشركة
        /// </summary>
        /// <param name="companyId">Optional company ID</param>
        /// <returns>Account ID if found, null otherwise</returns>
        Guid? GetRetainedEarningsAccountId(long? companyId = null);

        /// <summary>
        /// Check if fiscal year is already closed
        /// التحقق إذا كانت السنة المالية مغلقة مسبقاً
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to check</param>
        /// <param name="companyId">Optional company ID</param>
        /// <returns>True if already closed</returns>
        bool IsFiscalYearClosed(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get net income for fiscal year (Revenue - Expenses)
        /// الحصول على صافي الدخل للسنة المالية (الإيرادات - المصروفات)
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="companyId">Optional company ID</param>
        /// <returns>Tuple of (TotalRevenue, TotalExpenses, NetIncome)</returns>
        (decimal TotalRevenue, decimal TotalExpenses, decimal NetIncome) GetNetIncome(int fiscalYear, long? companyId = null);
    }
}
