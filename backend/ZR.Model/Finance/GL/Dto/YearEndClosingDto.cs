using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.Finance.GL.Dto
{
    /// <summary>
    /// Close Fiscal Year DTO - إغلاق السنة المالية
    /// </summary>
    public class CloseFiscalYearDto
    {
        [Required(ErrorMessage = "السنة المالية مطلوبة")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Retained Earnings Account ID - معرف حساب الأرباح المحتجزة
        /// If not specified, system will look for account with IsRetainedEarningsAccount = true
        /// </summary>
        public Guid? RetainedEarningsAccountId { get; set; }

        /// <summary>
        /// Closing Journal Date - تاريخ قيد الإغلاق
        /// Defaults to last day of fiscal year
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Auto Post - ترحيل تلقائي
        /// If true, the closing journal will be automatically posted
        /// </summary>
        public bool AutoPost { get; set; } = true;

        /// <summary>
        /// Company ID - معرف الشركة
        /// For multi-tenant support
        /// </summary>
        public long? TenantId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// Optional description for the closing journal
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
    }

    /// <summary>
    /// Year-End Closing Result DTO - نتيجة إغلاق نهاية السنة
    /// </summary>
    public class YearEndClosingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int FiscalYear { get; set; }
        public Guid? ClosingJournalId { get; set; }
        public string ClosingJournalNumber { get; set; }
        public decimal NetIncome { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public bool JournalPosted { get; set; }
        public DateTime? ClosingDate { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Year-End Closing Preview DTO - معاينة إغلاق نهاية السنة
    /// </summary>
    public class YearEndClosingPreviewDto
    {
        public int FiscalYear { get; set; }
        public bool CanClose { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public bool IsProfit => NetIncome > 0;
        public bool IsLoss => NetIncome < 0;
        public Guid? RetainedEarningsAccountId { get; set; }
        public string RetainedEarningsAccountCode { get; set; }
        public string RetainedEarningsAccountName { get; set; }
        public List<AccountClosingPreviewDto> RevenueAccounts { get; set; } = new List<AccountClosingPreviewDto>();
        public List<AccountClosingPreviewDto> ExpenseAccounts { get; set; } = new List<AccountClosingPreviewDto>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> BlockingIssues { get; set; } = new List<string>();
    }

    /// <summary>
    /// Account Closing Preview DTO - معاينة إغلاق الحساب
    /// </summary>
    public class AccountClosingPreviewDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public decimal Balance { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
    }

    /// <summary>
    /// Year-End Closing Validation DTO - التحقق من صلاحية إغلاق نهاية السنة
    /// </summary>
    public class YearEndClosingValidationDto
    {
        public bool IsValid { get; set; }
        public int FiscalYear { get; set; }

        // Validation checks
        public bool FiscalYearEnded { get; set; }  // True if today > Dec 31 of fiscal year
        public bool AllPeriodsOpen { get; set; }
        public bool AllJournalsPosted { get; set; }
        public bool TrialBalanceBalanced { get; set; }
        public bool HasRetainedEarningsAccount { get; set; }
        public bool NotAlreadyClosed { get; set; }

        // Details
        public int UnpostedJournalsCount { get; set; }
        public decimal TrialBalanceDifference { get; set; }
        public Guid? RetainedEarningsAccountId { get; set; }
        public Guid? ExistingClosingJournalId { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<PeriodStatusDto> PeriodStatuses { get; set; } = new List<PeriodStatusDto>();
    }

    /// <summary>
    /// Period Status DTO - حالة الفترة
    /// </summary>
    public class PeriodStatusDto
    {
        public Guid PeriodId { get; set; }
        public string PeriodName { get; set; }
        public int PeriodNumber { get; set; }
        public bool IsOpen { get; set; }
        public bool IsClosed { get; set; }
        public bool HasUnpostedJournals { get; set; }
        public int UnpostedCount { get; set; }
    }

    /// <summary>
    /// Check-Code Response DTO - استجابة التحقق من الرمز
    /// Standard response format: { "code": 200, "data": { "exists": true|false }, "msg": null }
    /// </summary>
    public class CheckCodeResponseDto
    {
        public bool Exists { get; set; }
    }

    /// <summary>
    /// Next Number Response DTO - استجابة الرقم التالي
    /// </summary>
    public class NextNumberResponseDto
    {
        public string Number { get; set; }
        public string InvoiceNumber { get; set; }
        public string NoteNumber { get; set; }
    }
}
