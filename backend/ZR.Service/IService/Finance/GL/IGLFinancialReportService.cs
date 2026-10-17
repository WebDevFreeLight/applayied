using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// GL Financial Report Service Interface - واجهة خدمة التقارير المالية
    /// </summary>
    public interface IGLFinancialReportService
    {
        /// <summary>
        /// Get paginated list of reports - الحصول على قائمة التقارير بالترقيم
        /// </summary>
        PagedInfo<FinancialReportResponseDto> GetList(GLFinancialReportQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get report by ID - الحصول على التقرير بالمعرف
        /// </summary>
        FinancialReportResponseDto GetById(Guid id);

        /// <summary>
        /// Get report by ID async - الحصول على التقرير بالمعرف (غير متزامن)
        /// </summary>
        Task<FinancialReportResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Get reports by type - الحصول على التقارير بالنوع
        /// </summary>
        List<FinancialReportResponseDto> GetByType(ReportType reportType, long? companyId = null);

        /// <summary>
        /// Get reports by period - الحصول على التقارير بالفترة
        /// </summary>
        List<FinancialReportResponseDto> GetByPeriod(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Generate Trial Balance - توليد ميزان المراجعة
        /// </summary>
        FinancialReportResponseDto GenerateTrialBalance(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate Trial Balance async - توليد ميزان المراجعة (غير متزامن)
        /// </summary>
        Task<FinancialReportResponseDto> GenerateTrialBalanceAsync(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate Balance Sheet - توليد الميزانية العمومية
        /// </summary>
        FinancialReportResponseDto GenerateBalanceSheet(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate Balance Sheet async - توليد الميزانية العمومية (غير متزامن)
        /// </summary>
        Task<FinancialReportResponseDto> GenerateBalanceSheetAsync(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate Income Statement - توليد قائمة الدخل
        /// </summary>
        FinancialReportResponseDto GenerateIncomeStatement(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate Income Statement async - توليد قائمة الدخل (غير متزامن)
        /// </summary>
        Task<FinancialReportResponseDto> GenerateIncomeStatementAsync(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate Cash Flow - توليد قائمة التدفقات النقدية
        /// </summary>
        FinancialReportResponseDto GenerateCashFlow(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate Cash Flow async - توليد قائمة التدفقات النقدية (غير متزامن)
        /// </summary>
        Task<FinancialReportResponseDto> GenerateCashFlowAsync(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate IAS 7 Compliant Cash Flow Statement (Indirect Method)
        /// توليد قائمة التدفقات النقدية وفق معيار المحاسبة الدولي 7 (الطريقة غير المباشرة)
        /// </summary>
        /// <param name="dto">Report generation parameters</param>
        /// <param name="generatedBy">User who generated the report</param>
        /// <returns>IAS 7 compliant cash flow statement</returns>
        FinancialReportResponseDto GenerateCashFlowIAS7(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Generate IAS 7 Compliant Cash Flow Statement async (Indirect Method)
        /// توليد قائمة التدفقات النقدية وفق معيار المحاسبة الدولي 7 (الطريقة غير المباشرة) - غير متزامن
        /// </summary>
        Task<FinancialReportResponseDto> GenerateCashFlowIAS7Async(GenerateReportDto dto, string generatedBy);

        /// <summary>
        /// Get IAS 7 Cash Flow Statement Data (without saving to database)
        /// الحصول على بيانات قائمة التدفقات النقدية وفق IAS 7 (بدون الحفظ في قاعدة البيانات)
        /// </summary>
        CashFlowStatementDto GetCashFlowDataIAS7(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get IAS 7 Cash Flow Statement Data async
        /// </summary>
        Task<CashFlowStatementDto> GetCashFlowDataIAS7Async(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Delete report - حذف التقرير
        /// </summary>
        int Delete(Guid id);

        /// <summary>
        /// Delete report async - حذف التقرير (غير متزامن)
        /// </summary>
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// Get Trial Balance data - الحصول على بيانات ميزان المراجعة
        /// </summary>
        TrialBalanceReportDto GetTrialBalanceData(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get Trial Balance data async - الحصول على بيانات ميزان المراجعة (غير متزامن)
        /// </summary>
        Task<TrialBalanceReportDto> GetTrialBalanceDataAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get Enhanced Trial Balance with IFRS-compliant structure
        /// الحصول على ميزان المراجعة المحسن مع هيكل متوافق مع IFRS
        /// Includes: Opening Balance, Period Movement, Closing Balance, Hierarchical Structure
        /// </summary>
        /// <param name="query">Query parameters with filters</param>
        /// <returns>Enhanced trial balance report with hierarchical structure</returns>
        TrialBalanceReportDto GetEnhancedTrialBalance(TrialBalanceQueryDto query);

        /// <summary>
        /// Get Enhanced Trial Balance async
        /// </summary>
        Task<TrialBalanceReportDto> GetEnhancedTrialBalanceAsync(TrialBalanceQueryDto query);

        /// <summary>
        /// Get Balance Sheet data (without saving) - الحصول على بيانات الميزانية العمومية
        /// </summary>
        BalanceSheetReportDto GetBalanceSheetData(DateTime asOfDate, long? companyId = null);

        /// <summary>
        /// Get Balance Sheet data async - الحصول على بيانات الميزانية العمومية (غير متزامن)
        /// </summary>
        Task<BalanceSheetReportDto> GetBalanceSheetDataAsync(DateTime asOfDate, long? companyId = null);

        /// <summary>
        /// Get IFRS-compliant Balance Sheet data
        /// الحصول على بيانات الميزانية العمومية وفق المعايير الدولية (IAS 1)
        /// Includes: Current/Non-Current classification, hierarchical structure, optional comparative period
        /// </summary>
        /// <param name="query">Query parameters including asOfDate, viewMode, etc.</param>
        /// <returns>IFRS-compliant balance sheet report</returns>
        Task<IFRSBalanceSheetReportDto> GetIFRSBalanceSheetAsync(BalanceSheetQueryDto query);

        /// <summary>
        /// Get Income Statement data (without saving) - الحصول على بيانات قائمة الدخل
        /// </summary>
        IncomeStatementReportDto GetIncomeStatementData(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get Income Statement data async - الحصول على بيانات قائمة الدخل (غير متزامن)
        /// </summary>
        Task<IncomeStatementReportDto> GetIncomeStatementDataAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get IFRS-compliant Income Statement data
        /// الحصول على بيانات قائمة الدخل وفق المعايير الدولية (IAS 1)
        /// Includes: Revenue, COGS, Operating Expenses, Other Income/Expense, hierarchical structure
        /// </summary>
        /// <param name="query">Query parameters including period, viewMode, etc.</param>
        /// <returns>IFRS-compliant income statement report</returns>
        Task<IFRSIncomeStatementReportDto> GetIFRSIncomeStatementAsync(IncomeStatementQueryDto query);

        #region Cost Center & Project P&L Reports - تقارير الربح والخسارة حسب مركز التكلفة والمشروع

        /// <summary>
        /// Get Cost Center P&L Report - تقرير الربح والخسارة حسب مركز التكلفة
        /// </summary>
        /// <param name="query">Query parameters including period and optional cost center filter</param>
        /// <returns>P&L report grouped by cost center</returns>
        CostCenterPLReportDto GetCostCenterPLReport(CostCenterPLQueryDto query);

        /// <summary>
        /// Get Cost Center P&L Report async - تقرير الربح والخسارة حسب مركز التكلفة (غير متزامن)
        /// </summary>
        Task<CostCenterPLReportDto> GetCostCenterPLReportAsync(CostCenterPLQueryDto query);

        /// <summary>
        /// Get Project P&L Report - تقرير الربح والخسارة حسب المشروع
        /// </summary>
        /// <param name="query">Query parameters including period and optional project filter</param>
        /// <returns>P&L report grouped by project</returns>
        ProjectPLReportDto GetProjectPLReport(ProjectPLQueryDto query);

        /// <summary>
        /// Get Project P&L Report async - تقرير الربح والخسارة حسب المشروع (غير متزامن)
        /// </summary>
        Task<ProjectPLReportDto> GetProjectPLReportAsync(ProjectPLQueryDto query);

        /// <summary>
        /// Get Cost Center P&L Summary - ملخص الربح والخسارة حسب مراكز التكلفة
        /// For dashboard widgets with top N cost centers
        /// </summary>
        List<DimensionPLSummaryDto> GetCostCenterPLSummary(DateTime periodStart, DateTime periodEnd, long? companyId = null, int topN = 10);

        /// <summary>
        /// Get Project P&L Summary - ملخص الربح والخسارة حسب المشاريع
        /// For dashboard widgets with top N projects
        /// </summary>
        List<DimensionPLSummaryDto> GetProjectPLSummary(DateTime periodStart, DateTime periodEnd, long? companyId = null, int topN = 10);

        #endregion
    }
}


