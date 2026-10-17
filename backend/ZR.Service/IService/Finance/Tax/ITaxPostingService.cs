using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Finance.Tax.Dto;
using ZR.Model.Finance.Tax;
using ZR.Repository.Finance.Tax;

namespace ZR.Service.IService.Finance.Tax
{
    /// <summary>
    /// Tax Posting Service Interface
    /// Handles creating and posting tax journal lines
    /// واجهة خدمة ترحيل الضرائب
    /// </summary>
    public interface ITaxPostingService
    {
        #region Tax Journal Line Operations

        /// <summary>
        /// Create tax journal lines from calculation result
        /// </summary>
        List<TaxJournalLine> CreateTaxJournalLines(TaxCalculationResultDto calculationResult, TaxPostingContext context);

        /// <summary>
        /// Create tax journal lines from calculation result (async)
        /// </summary>
        Task<List<TaxJournalLine>> CreateTaxJournalLinesAsync(TaxCalculationResultDto calculationResult, TaxPostingContext context);

        /// <summary>
        /// Save tax journal lines to database
        /// </summary>
        int SaveTaxJournalLines(List<TaxJournalLine> lines);

        /// <summary>
        /// Save tax journal lines to database (async)
        /// </summary>
        Task<int> SaveTaxJournalLinesAsync(List<TaxJournalLine> lines);

        /// <summary>
        /// Post tax journal lines (mark as immutable)
        /// </summary>
        int PostTaxJournalLines(Guid[] ids, string postedBy);

        /// <summary>
        /// Post tax journal lines (async)
        /// </summary>
        Task<int> PostTaxJournalLinesAsync(Guid[] ids, string postedBy);

        /// <summary>
        /// Post all tax journal lines for a journal
        /// </summary>
        int PostTaxJournalLinesByJournal(Guid journalId, string postedBy);

        /// <summary>
        /// Post all tax journal lines for a journal (async)
        /// </summary>
        Task<int> PostTaxJournalLinesByJournalAsync(Guid journalId, string postedBy);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get tax journal lines for a journal
        /// </summary>
        List<TaxJournalLineResponseDto> GetTaxJournalLinesByJournal(Guid journalId);

        /// <summary>
        /// Get tax journal lines for a journal (async)
        /// </summary>
        Task<List<TaxJournalLineResponseDto>> GetTaxJournalLinesByJournalAsync(Guid journalId);

        /// <summary>
        /// Get tax journal lines for an invoice
        /// </summary>
        List<TaxJournalLineResponseDto> GetTaxJournalLinesByInvoice(Guid invoiceId);

        /// <summary>
        /// Get tax journal lines for an invoice (async)
        /// </summary>
        Task<List<TaxJournalLineResponseDto>> GetTaxJournalLinesByInvoiceAsync(Guid invoiceId);

        /// <summary>
        /// Get tax journal lines by date range
        /// </summary>
        List<TaxJournalLineResponseDto> GetTaxJournalLinesByDateRange(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get tax journal lines by date range (async)
        /// </summary>
        Task<List<TaxJournalLineResponseDto>> GetTaxJournalLinesByDateRangeAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        #endregion

        #region Reporting Operations

        /// <summary>
        /// Get tax summary by tax code for reporting
        /// </summary>
        List<TaxSummary> GetTaxSummaryByCode(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get tax summary by tax code (async)
        /// </summary>
        Task<List<TaxSummary>> GetTaxSummaryByCodeAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get period totals for reporting
        /// </summary>
        TaxPeriodTotals GetPeriodTotals(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get period totals (async)
        /// </summary>
        Task<TaxPeriodTotals> GetPeriodTotalsAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get recoverable summary
        /// </summary>
        TaxRecoverableSummary GetRecoverableSummary(DateTime fromDate, DateTime toDate, long? companyId = null);

        /// <summary>
        /// Get recoverable summary (async)
        /// </summary>
        Task<TaxRecoverableSummary> GetRecoverableSummaryAsync(DateTime fromDate, DateTime toDate, long? companyId = null);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validate tax posting is allowed (period not closed, etc.)
        /// </summary>
        TaxValidationResult ValidatePosting(TaxPostingContext context);

        /// <summary>
        /// Validate tax posting is allowed (async)
        /// </summary>
        Task<TaxValidationResult> ValidatePostingAsync(TaxPostingContext context);

        #endregion

        #region Reversal Operations

        /// <summary>
        /// Create reversal entries for tax journal lines
        /// </summary>
        List<TaxJournalLine> CreateReversalEntries(Guid[] originalLineIds, TaxReversalContext context);

        /// <summary>
        /// Create reversal entries (async)
        /// </summary>
        Task<List<TaxJournalLine>> CreateReversalEntriesAsync(Guid[] originalLineIds, TaxReversalContext context);

        #endregion
    }

    #region Context Classes

    /// <summary>
    /// Context for tax posting
    /// </summary>
    public class TaxPostingContext
    {
        public long CompanyId { get; set; }
        public Guid? JournalId { get; set; }
        public Guid? InvoiceId { get; set; }
        public string SourceModule { get; set; }
        public Guid? SourceDocumentId { get; set; }
        public string SourceDocumentNumber { get; set; }
        public DateTime TaxDate { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CreatedBy { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Context for tax reversal
    /// </summary>
    public class TaxReversalContext
    {
        public long CompanyId { get; set; }
        public Guid? ReversalJournalId { get; set; }
        public DateTime ReversalDate { get; set; }
        public string ReversalReason { get; set; }
        public string CreatedBy { get; set; }
    }

    #endregion
}
