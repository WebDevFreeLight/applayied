using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Finance.Enums;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// High-level balance calculation service.
    /// خدمة حساب الأرصدة عالية المستوى
    /// </summary>
    public interface IAccountBalanceCalculator
    {
        /// <summary>
        /// Get account balance for a date range with normalBalance applied.
        /// الحصول على رصيد الحساب لفترة زمنية مع تطبيق الرصيد الطبيعي
        /// </summary>
        Task<AccountBalanceResultDto> GetAccountBalanceAsync(
            Guid accountId,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Get multiple account balances efficiently.
        /// الحصول على أرصدة حسابات متعددة بكفاءة
        /// </summary>
        Task<List<AccountBalanceResultDto>> GetAccountBalancesAsync(
            IEnumerable<Guid> accountIds,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Get balance for accounts of a specific type.
        /// الحصول على رصيد حسابات من نوع محدد
        /// </summary>
        Task<decimal> GetTotalBalanceByTypeAsync(
            AccountType accountType,
            DateTime startDate,
            DateTime endDate,
            long? companyId = null);

        /// <summary>
        /// Get balance summary with raw debits/credits and normalized balance.
        /// الحصول على ملخص الرصيد مع المدين/الدائن الخام والرصيد المعياري
        /// </summary>
        Task<AccountBalanceResultDto> GetBalanceSummaryAsync(
            Guid accountId,
            DateTime asOfDate,
            long? companyId = null);
    }

    /// <summary>
    /// Account balance DTO with normalBalance-aware calculation.
    /// كائن نقل بيانات رصيد الحساب مع حساب الرصيد الطبيعي
    /// </summary>
    public class AccountBalanceResultDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameEn { get; set; }
        public string AccountNameAr { get; set; }
        public AccountType AccountType { get; set; }
        public NormalBalance NormalBalance { get; set; }

        /// <summary>
        /// Raw debit total from journal entries.
        /// إجمالي المدين الخام من قيود اليومية
        /// </summary>
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Raw credit total from journal entries.
        /// إجمالي الدائن الخام من قيود اليومية
        /// </summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Normalized balance (calculated using normalBalance direction).
        /// Positive = normal direction, Negative = abnormal direction.
        /// الرصيد المعياري - موجب = اتجاه طبيعي، سالب = اتجاه غير طبيعي
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// True if balance is in normal direction (positive).
        /// صحيح إذا كان الرصيد في الاتجاه الطبيعي
        /// </summary>
        public bool IsNormalDirection { get; set; }
    }
}
