using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Journal Entry Repository Interface - واجهة مستودع قيود اليومية
    /// </summary>
    public interface IGLJournalEntryRepository : IBaseRepository<GLJournalEntry>
    {
        /// <summary>
        /// Get paginated list of entries - الحصول على قائمة القيود بالترقيم
        /// </summary>
        PagedInfo<GLJournalEntry> GetList(GLJournalEntry parm, PagerInfo pager);

        /// <summary>
        /// Get entry by ID - الحصول على القيد بالمعرف
        /// </summary>
        GLJournalEntry GetById(Guid id);

        /// <summary>
        /// Insert entry - إضافة قيد
        /// </summary>
        int InsertEntry(GLJournalEntry model);

        /// <summary>
        /// Insert multiple entries - إضافة قيود متعددة
        /// </summary>
        int InsertEntries(List<GLJournalEntry> models);

        /// <summary>
        /// Update entry - تحديث قيد
        /// </summary>
        int UpdateEntry(GLJournalEntry model);

        /// <summary>
        /// Delete entries by IDs - حذف قيود بالمعرفات
        /// </summary>
        int DeleteByIds(Guid[] ids);

        /// <summary>
        /// Delete entries by journal ID - حذف قيود بمعرف دفتر اليومية
        /// </summary>
        int DeleteByJournalId(Guid journalId);

        /// <summary>
        /// Get entries by journal ID - الحصول على القيود بمعرف دفتر اليومية
        /// </summary>
        List<GLJournalEntry> GetByJournalId(Guid journalId);

        /// <summary>
        /// Get entries by account ID - الحصول على القيود بمعرف الحساب
        /// </summary>
        List<GLJournalEntry> GetByAccountId(Guid accountId);

        /// <summary>
        /// Get total debit by journal ID - الحصول على إجمالي المدين بمعرف دفتر اليومية
        /// </summary>
        decimal GetTotalDebitByJournalId(Guid journalId);

        /// <summary>
        /// Get total credit by journal ID - الحصول على إجمالي الدائن بمعرف دفتر اليومية
        /// </summary>
        decimal GetTotalCreditByJournalId(Guid journalId);

        /// <summary>
        /// Check if journal is balanced - التحقق من توازن دفتر اليومية
        /// </summary>
        bool IsJournalBalanced(Guid journalId);

        /// <summary>
        /// Get account balance for a period - الحصول على رصيد الحساب للفترة
        /// </summary>
        AccountBalanceResult GetAccountBalance(Guid accountId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Get account balance for a period async - الحصول على رصيد الحساب للفترة
        /// </summary>
        Task<AccountBalanceResult> GetAccountBalanceAsync(Guid accountId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Get entries for P&L report by dimension (Cost Center / Project) - الحصول على القيود لتقرير الربح والخسارة
        /// </summary>
        /// <param name="periodStart">Period start date</param>
        /// <param name="periodEnd">Period end date</param>
        /// <param name="accountIds">List of account IDs to include (Revenue/Expense)</param>
        /// <param name="companyId">Optional company ID filter</param>
        /// <param name="costCenterId">Optional specific cost center filter</param>
        /// <param name="projectId">Optional specific project filter</param>
        /// <returns>List of journal entries matching criteria</returns>
        List<GLJournalEntry> GetEntriesForDimensionPL(
            DateTime periodStart,
            DateTime periodEnd,
            List<Guid> accountIds,
            long? companyId = null,
            Guid? costCenterId = null,
            Guid? projectId = null);

        #region Trial Balance Methods - طرق ميزان المراجعة

        /// <summary>
        /// Get opening balance for an account (transactions before period start)
        /// الحصول على الرصيد الافتتاحي للحساب (المعاملات قبل بداية الفترة)
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="beforeDate">Date before which to calculate balance</param>
        /// <param name="companyId">Optional company ID filter</param>
        /// <returns>Opening balance result</returns>
        AccountBalanceResult GetOpeningBalance(Guid accountId, DateTime beforeDate, long? companyId = null);

        /// <summary>
        /// Get opening balance for an account async
        /// </summary>
        Task<AccountBalanceResult> GetOpeningBalanceAsync(Guid accountId, DateTime beforeDate, long? companyId = null);

        /// <summary>
        /// Get period movement for an account (transactions within period)
        /// الحصول على حركة الفترة للحساب (المعاملات خلال الفترة)
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="periodStart">Period start date</param>
        /// <param name="periodEnd">Period end date</param>
        /// <param name="companyId">Optional company ID filter</param>
        /// <returns>Period movement result</returns>
        AccountBalanceResult GetPeriodMovement(Guid accountId, DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get period movement for an account async
        /// </summary>
        Task<AccountBalanceResult> GetPeriodMovementAsync(Guid accountId, DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get trial balance data for all accounts efficiently
        /// الحصول على بيانات ميزان المراجعة لجميع الحسابات بكفاءة
        /// </summary>
        /// <param name="periodStart">Period start date</param>
        /// <param name="periodEnd">Period end date</param>
        /// <param name="companyId">Optional company ID filter</param>
        /// <returns>Dictionary of account ID to trial balance data</returns>
        Dictionary<Guid, TrialBalanceAccountData> GetTrialBalanceData(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get trial balance data for all accounts efficiently async
        /// </summary>
        Task<Dictionary<Guid, TrialBalanceAccountData>> GetTrialBalanceDataAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        #endregion
    }

    /// <summary>
    /// Account Balance Result - نتيجة رصيد الحساب
    /// </summary>
    public class AccountBalanceResult
    {
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Balance => TotalDebit - TotalCredit;
    }

    /// <summary>
    /// Trial Balance Account Data - بيانات ميزان المراجعة للحساب
    /// Contains opening balance and period movement for efficient trial balance generation
    /// </summary>
    public class TrialBalanceAccountData
    {
        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Opening Debit Balance (before period start) - رصيد المدين الافتتاحي
        /// </summary>
        public decimal OpeningDebit { get; set; }

        /// <summary>
        /// Opening Credit Balance (before period start) - رصيد الدائن الافتتاحي
        /// </summary>
        public decimal OpeningCredit { get; set; }

        /// <summary>
        /// Period Debit Movement - حركة المدين للفترة
        /// </summary>
        public decimal PeriodDebit { get; set; }

        /// <summary>
        /// Period Credit Movement - حركة الدائن للفترة
        /// </summary>
        public decimal PeriodCredit { get; set; }

        /// <summary>
        /// Opening Balance (Net) - الرصيد الافتتاحي الصافي
        /// </summary>
        public decimal OpeningBalance => OpeningDebit - OpeningCredit;

        /// <summary>
        /// Period Movement (Net) - صافي حركة الفترة
        /// </summary>
        public decimal PeriodMovement => PeriodDebit - PeriodCredit;

        /// <summary>
        /// Closing Balance (Net) - الرصيد الختامي الصافي
        /// </summary>
        public decimal ClosingBalance => OpeningBalance + PeriodMovement;

        /// <summary>
        /// Closing Debit - رصيد المدين الختامي
        /// </summary>
        public decimal ClosingDebit => ClosingBalance >= 0 ? ClosingBalance : 0;

        /// <summary>
        /// Closing Credit - رصيد الدائن الختامي
        /// </summary>
        public decimal ClosingCredit => ClosingBalance < 0 ? Math.Abs(ClosingBalance) : 0;

        /// <summary>
        /// Has Activity - لديه حركة
        /// </summary>
        public bool HasActivity => OpeningDebit != 0 || OpeningCredit != 0 || PeriodDebit != 0 || PeriodCredit != 0;
    }
}

