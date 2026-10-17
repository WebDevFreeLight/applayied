using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Tax;

namespace ZR.Repository.Finance.Tax
{
    /// <summary>
    /// Tax Journal Line Repository Interface
    /// واجهة مستودع سطور دفتر الضرائب
    /// </summary>
    public interface ITaxJournalLineRepository : IBaseRepository<TaxJournalLine>
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax journal lines
        /// </summary>
        PagedInfo<TaxJournalLine> GetList(TaxJournalLine parm, PagerInfo pager);

        /// <summary>
        /// Get tax journal line by ID
        /// </summary>
        TaxJournalLine GetById(Guid id);

        /// <summary>
        /// Get tax journal line by ID (async)
        /// </summary>
        Task<TaxJournalLine> GetByIdAsync(Guid id);

        /// <summary>
        /// Insert tax journal line
        /// </summary>
        int InsertTaxJournalLine(TaxJournalLine model);

        /// <summary>
        /// Insert tax journal line (async)
        /// </summary>
        Task<int> InsertTaxJournalLineAsync(TaxJournalLine model);

        /// <summary>
        /// Insert multiple tax journal lines (batch)
        /// </summary>
        int InsertTaxJournalLines(List<TaxJournalLine> models);

        /// <summary>
        /// Insert multiple tax journal lines (async, batch)
        /// </summary>
        Task<int> InsertTaxJournalLinesAsync(List<TaxJournalLine> models);

        /// <summary>
        /// Update tax journal line (only allowed if not posted)
        /// </summary>
        int UpdateTaxJournalLine(TaxJournalLine model);

        /// <summary>
        /// Update tax journal line (async, only allowed if not posted)
        /// </summary>
        Task<int> UpdateTaxJournalLineAsync(TaxJournalLine model);

        /// <summary>
        /// Delete tax journal lines by IDs (only allowed if not posted)
        /// </summary>
        int DeleteTaxJournalLines(Guid[] ids);

        /// <summary>
        /// Delete tax journal lines by IDs (async, only allowed if not posted)
        /// </summary>
        Task<int> DeleteTaxJournalLinesAsync(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get tax journal lines by journal ID
        /// </summary>
        List<TaxJournalLine> GetByJournalId(Guid journalId);

        /// <summary>
        /// Get tax journal lines by journal ID (async)
        /// </summary>
        Task<List<TaxJournalLine>> GetByJournalIdAsync(Guid journalId);

        /// <summary>
        /// Get tax journal lines by invoice ID
        /// </summary>
        List<TaxJournalLine> GetByInvoiceId(Guid invoiceId);

        /// <summary>
        /// Get tax journal lines by invoice ID (async)
        /// </summary>
        Task<List<TaxJournalLine>> GetByInvoiceIdAsync(Guid invoiceId);

        /// <summary>
        /// Get tax journal lines by source document
        /// </summary>
        List<TaxJournalLine> GetBySourceDocument(string sourceModule, Guid sourceDocumentId);

        /// <summary>
        /// Get tax journal lines by source document (async)
        /// </summary>
        Task<List<TaxJournalLine>> GetBySourceDocumentAsync(string sourceModule, Guid sourceDocumentId);

        /// <summary>
        /// Get tax journal lines by tax code
        /// </summary>
        List<TaxJournalLine> GetByTaxCode(Guid taxCodeId, long? companyId = null);

        /// <summary>
        /// Get tax journal lines by date range
        /// </summary>
        List<TaxJournalLine> GetByDateRange(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get tax journal lines by date range (async)
        /// </summary>
        Task<List<TaxJournalLine>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get unposted tax journal lines
        /// </summary>
        List<TaxJournalLine> GetUnposted(long? companyId = null);

        /// <summary>
        /// Get unposted tax journal lines (async)
        /// </summary>
        Task<List<TaxJournalLine>> GetUnpostedAsync(long? companyId = null);

        #endregion

        #region Posting Operations

        /// <summary>
        /// Mark tax journal lines as posted
        /// </summary>
        int MarkAsPosted(Guid[] ids, string postedBy);

        /// <summary>
        /// Mark tax journal lines as posted (async)
        /// </summary>
        Task<int> MarkAsPostedAsync(Guid[] ids, string postedBy);

        /// <summary>
        /// Mark tax journal lines by journal ID as posted
        /// </summary>
        int MarkJournalLinesAsPosted(Guid journalId, string postedBy);

        /// <summary>
        /// Mark tax journal lines by journal ID as posted (async)
        /// </summary>
        Task<int> MarkJournalLinesAsPostedAsync(Guid journalId, string postedBy);

        #endregion

        #region Summary & Reporting Operations

        /// <summary>
        /// Get tax summary by tax code for date range
        /// </summary>
        List<TaxSummary> GetTaxSummaryByCode(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get tax summary by tax code for date range (async)
        /// </summary>
        Task<List<TaxSummary>> GetTaxSummaryByCodeAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get total tax amounts for a period
        /// </summary>
        TaxPeriodTotals GetPeriodTotals(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get total tax amounts for a period (async)
        /// </summary>
        Task<TaxPeriodTotals> GetPeriodTotalsAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get recoverable vs payable tax totals
        /// </summary>
        TaxRecoverableSummary GetRecoverableSummary(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get recoverable vs payable tax totals (async)
        /// </summary>
        Task<TaxRecoverableSummary> GetRecoverableSummaryAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if any lines exist for a journal
        /// </summary>
        bool HasLinesForJournal(Guid journalId);

        /// <summary>
        /// Check if any lines exist for a journal (async)
        /// </summary>
        Task<bool> HasLinesForJournalAsync(Guid journalId);

        /// <summary>
        /// Check if tax journal line can be modified (not posted)
        /// </summary>
        bool CanModify(Guid id);

        /// <summary>
        /// Check if tax journal line can be modified (async)
        /// </summary>
        Task<bool> CanModifyAsync(Guid id);

        #endregion
    }

    #region Summary Classes

    /// <summary>
    /// Tax Summary by Code
    /// </summary>
    public class TaxSummary
    {
        public Guid TaxCodeId { get; set; }
        public string TaxCode { get; set; }
        public string TaxCodeNameEn { get; set; }
        public string TaxCodeNameAr { get; set; }
        public decimal TaxRate { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalTaxableBase { get; set; }
        public decimal TotalTaxBase { get; set; }
        public decimal TotalTaxableForeign { get; set; }
        public decimal TotalTaxForeign { get; set; }
    }

    /// <summary>
    /// Tax Period Totals
    /// </summary>
    public class TaxPeriodTotals
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalTaxableBase { get; set; }
        public decimal TotalTaxBase { get; set; }
        public decimal TotalRecoverableTax { get; set; }
        public decimal TotalPayableTax { get; set; }
        public decimal NetTaxPosition { get; set; } // Payable - Recoverable
    }

    /// <summary>
    /// Tax Recoverable Summary
    /// </summary>
    public class TaxRecoverableSummary
    {
        public decimal TotalRecoverable { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal NetPosition { get; set; }
        public int RecoverableCount { get; set; }
        public int PayableCount { get; set; }
    }

    #endregion
}
