using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// Account Reconciliation Service Interface - واجهة خدمة تسوية الحسابات
    /// Manages reconciliation workflow for accounts with ReconciliationMode = Required
    /// </summary>
    public interface IAccountReconciliationService
    {
        /// <summary>
        /// Get all accounts that require reconciliation
        /// الحصول على جميع الحسابات التي تتطلب تسوية
        /// </summary>
        Task<List<AccountReconciliationStatusDto>> GetAccountsRequiringReconciliationAsync(long? companyId = null);

        /// <summary>
        /// Get accounts with pending reconciliation (not reconciled for current period)
        /// الحصول على الحسابات ذات التسوية المعلقة
        /// </summary>
        Task<List<AccountReconciliationStatusDto>> GetPendingReconciliationsAsync(int fiscalYear, int period, long? companyId = null);

        /// <summary>
        /// Mark an account as reconciled
        /// تحديد حساب كمسوى
        /// </summary>
        Task<bool> MarkAccountReconciledAsync(Guid accountId, DateTime asOfDate, string reconciledBy, string notes = null);

        /// <summary>
        /// Mark an account as pending review
        /// تحديد حساب كمعلق للمراجعة
        /// </summary>
        Task<bool> MarkAccountPendingReviewAsync(Guid accountId, string markedBy, string reason);

        /// <summary>
        /// Reset reconciliation status to unreconciled
        /// إعادة تعيين حالة التسوية إلى غير مسوى
        /// </summary>
        Task<bool> ResetReconciliationStatusAsync(Guid accountId, string resetBy, string reason);

        /// <summary>
        /// Validate all reconciliation accounts are reconciled for period closing
        /// التحقق من أن جميع حسابات التسوية مسواة لإغلاق الفترة
        /// </summary>
        Task<ReconciliationValidationResult> ValidateAllReconciliationsCompleteAsync(int fiscalYear, int period, long? companyId = null);

        /// <summary>
        /// Get reconciliation history for an account
        /// الحصول على سجل التسويات لحساب
        /// </summary>
        Task<List<ReconciliationHistoryDto>> GetReconciliationHistoryAsync(Guid accountId);

        /// <summary>
        /// Get reconciliation summary dashboard data
        /// الحصول على بيانات لوحة ملخص التسويات
        /// </summary>
        Task<ReconciliationDashboardDto> GetReconciliationDashboardAsync(long? companyId = null);
    }

    #region DTOs

    // Note: ReconciliationStatus enum is now defined in ZR.Model.Finance.Enums.GLAccountEnums

    /// <summary>
    /// Account Reconciliation Status DTO - حالة تسوية الحساب
    /// </summary>
    public class AccountReconciliationStatusDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameEn { get; set; }
        public string AccountNameAr { get; set; }
        public bool IsReconciliationAccount { get; set; }
        public ReconciliationStatus Status { get; set; }
        public string StatusName => Status switch
        {
            ReconciliationStatus.NotReconciled => "Not Reconciled - غير مسوى",
            ReconciliationStatus.Reconciled => "Reconciled - مسوى",
            ReconciliationStatus.PendingReview => "Pending Review - معلق للمراجعة",
            _ => "Unknown"
        };
        public DateTime? LastReconciledDate { get; set; }
        public string LastReconciledBy { get; set; }
        public int DaysSinceLastReconciliation => LastReconciledDate.HasValue
            ? (int)(DateTime.Now - LastReconciledDate.Value).TotalDays
            : -1;
        public bool IsOverdue { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    /// <summary>
    /// Reconciliation Validation Result - نتيجة التحقق من التسوية
    /// </summary>
    public class ReconciliationValidationResult
    {
        public bool IsValid { get; set; }
        public int TotalReconciliationAccounts { get; set; }
        public int ReconciledCount { get; set; }
        public int PendingCount { get; set; }
        public int NotReconciledCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<AccountReconciliationStatusDto> UnreconciledAccounts { get; set; } = new List<AccountReconciliationStatusDto>();
    }

    /// <summary>
    /// Reconciliation History DTO - سجل التسوية
    /// </summary>
    public class ReconciliationHistoryDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public string ReconciledBy { get; set; }
        public ReconciliationStatus PreviousStatus { get; set; }
        public ReconciliationStatus NewStatus { get; set; }
        public string Notes { get; set; }
        public decimal BalanceAtReconciliation { get; set; }
    }

    /// <summary>
    /// Reconciliation Dashboard DTO - لوحة ملخص التسويات
    /// </summary>
    public class ReconciliationDashboardDto
    {
        public int TotalReconciliationAccounts { get; set; }
        public int ReconciledThisMonth { get; set; }
        public int PendingReview { get; set; }
        public int OverdueReconciliations { get; set; }
        public decimal TotalUnreconciledBalance { get; set; }
        public List<AccountReconciliationStatusDto> OverdueAccounts { get; set; } = new List<AccountReconciliationStatusDto>();
        public List<AccountReconciliationStatusDto> RecentlyReconciled { get; set; } = new List<AccountReconciliationStatusDto>();
    }

    #endregion
}
