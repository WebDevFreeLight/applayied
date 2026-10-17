using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model.Finance.Tax.Dto;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Tax;
using ZR.Repository.Finance.Tax;
using ZR.Service.IService.Finance.Tax;

namespace ZR.Service.Finance.Tax
{
    /// <summary>
    /// Tax Posting Service Implementation
    /// Handles creating and posting tax journal lines
    /// تنفيذ خدمة ترحيل الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxPostingService), ServiceLifetime = LifeTime.Transient)]
    public class TaxPostingService : ITaxPostingService
    {
        private readonly ITaxJournalLineRepository _taxJournalLineRepository;
        private readonly ITaxCodeRepository _taxCodeRepository;

        public TaxPostingService(
            ITaxJournalLineRepository taxJournalLineRepository,
            ITaxCodeRepository taxCodeRepository)
        {
            _taxJournalLineRepository = taxJournalLineRepository;
            _taxCodeRepository = taxCodeRepository;
        }

        #region Tax Journal Line Operations

        /// <summary>
        /// Create tax journal lines from calculation result
        /// </summary>
        public List<TaxJournalLine> CreateTaxJournalLines(TaxCalculationResultDto calculationResult, TaxPostingContext context)
        {
            var lines = new List<TaxJournalLine>();

            if (calculationResult == null || !calculationResult.Success || calculationResult.LineResults == null)
            {
                return lines;
            }

            foreach (var lineResult in calculationResult.LineResults)
            {
                // Skip lines with no tax
                if (lineResult.TaxAmountForeign == 0 && lineResult.TaxAmountBase == 0)
                {
                    continue;
                }

                var taxLine = new TaxJournalLine
                {
                    Id = Guid.NewGuid(),
                    CompanyID = context.CompanyId,
                    JournalId = context.JournalId,
                    InvoiceId = context.InvoiceId,
                    SourceModule = context.SourceModule,
                    SourceDocumentId = context.SourceDocumentId,
                    SourceDocumentNumber = context.SourceDocumentNumber,
                    TaxCodeId = lineResult.TaxCodeId,
                    TaxRate = lineResult.TaxRate,
                    IsTaxInclusive = lineResult.IsTaxInclusive,
                    TaxableAmountBase = lineResult.TaxableAmountBase,
                    TaxableAmountForeign = lineResult.TaxableAmountForeign,
                    TaxAmountBase = lineResult.TaxAmountBase,
                    TaxAmountForeign = lineResult.TaxAmountForeign,
                    Currency = context.Currency,
                    ExchangeRate = context.ExchangeRate,
                    TaxDate = context.TaxDate,
                    TaxAccountId = lineResult.TaxAccountId,
                    IsRecoverable = lineResult.IsRecoverable,
                    IsReverseCharge = lineResult.IsReverseCharge,
                    RoundingDelta = lineResult.RoundingDelta,
                    RoundingMode = calculationResult.RoundingModeUsed.ToString(),
                    IsPosted = false,
                    Notes = context.Notes,
                    Create_by = context.CreatedBy,
                    Create_time = DateTime.Now
                };

                lines.Add(taxLine);
            }

            return lines;
        }

        /// <summary>
        /// Create tax journal lines from calculation result (async)
        /// </summary>
        public async Task<List<TaxJournalLine>> CreateTaxJournalLinesAsync(TaxCalculationResultDto calculationResult, TaxPostingContext context)
        {
            return await Task.Run(() => CreateTaxJournalLines(calculationResult, context));
        }

        /// <summary>
        /// Save tax journal lines to database
        /// </summary>
        public int SaveTaxJournalLines(List<TaxJournalLine> lines)
        {
            if (lines == null || !lines.Any())
            {
                return 0;
            }

            return _taxJournalLineRepository.InsertTaxJournalLines(lines);
        }

        /// <summary>
        /// Save tax journal lines to database (async)
        /// </summary>
        public async Task<int> SaveTaxJournalLinesAsync(List<TaxJournalLine> lines)
        {
            if (lines == null || !lines.Any())
            {
                return 0;
            }

            return await _taxJournalLineRepository.InsertTaxJournalLinesAsync(lines);
        }

        /// <summary>
        /// Post tax journal lines (mark as immutable)
        /// </summary>
        public int PostTaxJournalLines(Guid[] ids, string postedBy)
        {
            return _taxJournalLineRepository.MarkAsPosted(ids, postedBy);
        }

        /// <summary>
        /// Post tax journal lines (async)
        /// </summary>
        public async Task<int> PostTaxJournalLinesAsync(Guid[] ids, string postedBy)
        {
            return await _taxJournalLineRepository.MarkAsPostedAsync(ids, postedBy);
        }

        /// <summary>
        /// Post all tax journal lines for a journal
        /// </summary>
        public int PostTaxJournalLinesByJournal(Guid journalId, string postedBy)
        {
            return _taxJournalLineRepository.MarkJournalLinesAsPosted(journalId, postedBy);
        }

        /// <summary>
        /// Post all tax journal lines for a journal (async)
        /// </summary>
        public async Task<int> PostTaxJournalLinesByJournalAsync(Guid journalId, string postedBy)
        {
            return await _taxJournalLineRepository.MarkJournalLinesAsPostedAsync(journalId, postedBy);
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get tax journal lines for a journal
        /// </summary>
        public List<TaxJournalLineResponseDto> GetTaxJournalLinesByJournal(Guid journalId)
        {
            var lines = _taxJournalLineRepository.GetByJournalId(journalId);
            return lines.Select(l => MapToResponse(l)).ToList();
        }

        /// <summary>
        /// Get tax journal lines for a journal (async)
        /// </summary>
        public async Task<List<TaxJournalLineResponseDto>> GetTaxJournalLinesByJournalAsync(Guid journalId)
        {
            var lines = await _taxJournalLineRepository.GetByJournalIdAsync(journalId);
            var result = new List<TaxJournalLineResponseDto>();
            foreach (var line in lines)
            {
                result.Add(await MapToResponseAsync(line));
            }
            return result;
        }

        /// <summary>
        /// Get tax journal lines for an invoice
        /// </summary>
        public List<TaxJournalLineResponseDto> GetTaxJournalLinesByInvoice(Guid invoiceId)
        {
            var lines = _taxJournalLineRepository.GetByInvoiceId(invoiceId);
            return lines.Select(l => MapToResponse(l)).ToList();
        }

        /// <summary>
        /// Get tax journal lines for an invoice (async)
        /// </summary>
        public async Task<List<TaxJournalLineResponseDto>> GetTaxJournalLinesByInvoiceAsync(Guid invoiceId)
        {
            var lines = await _taxJournalLineRepository.GetByInvoiceIdAsync(invoiceId);
            var result = new List<TaxJournalLineResponseDto>();
            foreach (var line in lines)
            {
                result.Add(await MapToResponseAsync(line));
            }
            return result;
        }

        /// <summary>
        /// Get tax journal lines by date range
        /// </summary>
        public List<TaxJournalLineResponseDto> GetTaxJournalLinesByDateRange(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var lines = _taxJournalLineRepository.GetByDateRange(fromDate, toDate, companyId);
            return lines.Select(l => MapToResponse(l)).ToList();
        }

        /// <summary>
        /// Get tax journal lines by date range (async)
        /// </summary>
        public async Task<List<TaxJournalLineResponseDto>> GetTaxJournalLinesByDateRangeAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var lines = await _taxJournalLineRepository.GetByDateRangeAsync(fromDate, toDate, companyId);
            var result = new List<TaxJournalLineResponseDto>();
            foreach (var line in lines)
            {
                result.Add(await MapToResponseAsync(line));
            }
            return result;
        }

        #endregion

        #region Reporting Operations

        /// <summary>
        /// Get tax summary by tax code for reporting
        /// </summary>
        public List<TaxSummary> GetTaxSummaryByCode(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var summaries = _taxJournalLineRepository.GetTaxSummaryByCode(fromDate, toDate, companyId);

            // Enrich with tax code details
            foreach (var summary in summaries)
            {
                var taxCode = _taxCodeRepository.GetById(summary.TaxCodeId);
                if (taxCode != null)
                {
                    summary.TaxCode = taxCode.Code;
                    summary.TaxCodeNameEn = taxCode.NameEn;
                    summary.TaxCodeNameAr = taxCode.NameAr;
                }
            }

            return summaries;
        }

        /// <summary>
        /// Get tax summary by tax code (async)
        /// </summary>
        public async Task<List<TaxSummary>> GetTaxSummaryByCodeAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var summaries = await _taxJournalLineRepository.GetTaxSummaryByCodeAsync(fromDate, toDate, companyId);

            foreach (var summary in summaries)
            {
                var taxCode = await _taxCodeRepository.GetByIdAsync(summary.TaxCodeId);
                if (taxCode != null)
                {
                    summary.TaxCode = taxCode.Code;
                    summary.TaxCodeNameEn = taxCode.NameEn;
                    summary.TaxCodeNameAr = taxCode.NameAr;
                }
            }

            return summaries;
        }

        /// <summary>
        /// Get period totals for reporting
        /// </summary>
        public TaxPeriodTotals GetPeriodTotals(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            return _taxJournalLineRepository.GetPeriodTotals(fromDate, toDate, companyId);
        }

        /// <summary>
        /// Get period totals (async)
        /// </summary>
        public async Task<TaxPeriodTotals> GetPeriodTotalsAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            return await _taxJournalLineRepository.GetPeriodTotalsAsync(fromDate, toDate, companyId);
        }

        /// <summary>
        /// Get recoverable summary
        /// </summary>
        public TaxRecoverableSummary GetRecoverableSummary(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            return _taxJournalLineRepository.GetRecoverableSummary(fromDate, toDate, companyId);
        }

        /// <summary>
        /// Get recoverable summary (async)
        /// </summary>
        public async Task<TaxRecoverableSummary> GetRecoverableSummaryAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            return await _taxJournalLineRepository.GetRecoverableSummaryAsync(fromDate, toDate, companyId);
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validate tax posting is allowed (period not closed, etc.)
        /// </summary>
        public TaxValidationResult ValidatePosting(TaxPostingContext context)
        {
            var result = new TaxValidationResult();

            if (context.CompanyId <= 0)
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "INVALID_COMPANY",
                    MessageEn = "Company ID is required",
                    MessageAr = "معرف الشركة مطلوب"
                });
            }

            if (context.TaxDate == default)
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "INVALID_TAX_DATE",
                    MessageEn = "Tax date is required",
                    MessageAr = "تاريخ الضريبة مطلوب"
                });
            }

            if (string.IsNullOrWhiteSpace(context.Currency))
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "INVALID_CURRENCY",
                    MessageEn = "Currency is required",
                    MessageAr = "العملة مطلوبة"
                });
            }

            // TODO: Check if period is closed
            // TODO: Check if period is locked

            return result;
        }

        /// <summary>
        /// Validate tax posting is allowed (async)
        /// </summary>
        public async Task<TaxValidationResult> ValidatePostingAsync(TaxPostingContext context)
        {
            return await Task.Run(() => ValidatePosting(context));
        }

        #endregion

        #region Reversal Operations

        /// <summary>
        /// Create reversal entries for tax journal lines
        /// </summary>
        public List<TaxJournalLine> CreateReversalEntries(Guid[] originalLineIds, TaxReversalContext context)
        {
            var reversalLines = new List<TaxJournalLine>();

            foreach (var originalId in originalLineIds)
            {
                var original = _taxJournalLineRepository.GetById(originalId);
                if (original == null || !original.IsPosted)
                {
                    continue;
                }

                var reversal = new TaxJournalLine
                {
                    Id = Guid.NewGuid(),
                    CompanyID = context.CompanyId,
                    JournalId = context.ReversalJournalId,
                    InvoiceId = original.InvoiceId,
                    SourceModule = original.SourceModule,
                    SourceDocumentId = original.SourceDocumentId,
                    SourceDocumentNumber = original.SourceDocumentNumber,
                    TaxCodeId = original.TaxCodeId,
                    TaxRate = original.TaxRate,
                    IsTaxInclusive = original.IsTaxInclusive,
                    // Reverse the amounts
                    TaxableAmountBase = -original.TaxableAmountBase,
                    TaxableAmountForeign = -original.TaxableAmountForeign,
                    TaxAmountBase = -original.TaxAmountBase,
                    TaxAmountForeign = -original.TaxAmountForeign,
                    Currency = original.Currency,
                    ExchangeRate = original.ExchangeRate,
                    TaxDate = context.ReversalDate,
                    TaxAccountId = original.TaxAccountId,
                    IsRecoverable = original.IsRecoverable,
                    IsReverseCharge = original.IsReverseCharge,
                    RoundingDelta = -original.RoundingDelta,
                    RoundingMode = original.RoundingMode,
                    IsPosted = false,
                    Notes = $"Reversal of {originalId}: {context.ReversalReason}",
                    Create_by = context.CreatedBy,
                    Create_time = DateTime.Now
                };

                reversalLines.Add(reversal);
            }

            return reversalLines;
        }

        /// <summary>
        /// Create reversal entries (async)
        /// </summary>
        public async Task<List<TaxJournalLine>> CreateReversalEntriesAsync(Guid[] originalLineIds, TaxReversalContext context)
        {
            return await Task.Run(() => CreateReversalEntries(originalLineIds, context));
        }

        #endregion

        #region Mapping Helpers

        private TaxJournalLineResponseDto MapToResponse(TaxJournalLine entity)
        {
            var response = entity.Adapt<TaxJournalLineResponseDto>();

            var taxCode = _taxCodeRepository.GetById(entity.TaxCodeId);
            if (taxCode != null)
            {
                response.TaxCode = taxCode.Code;
                response.TaxCodeNameEn = taxCode.NameEn;
                response.TaxCodeNameAr = taxCode.NameAr;
            }

            return response;
        }

        private async Task<TaxJournalLineResponseDto> MapToResponseAsync(TaxJournalLine entity)
        {
            var response = entity.Adapt<TaxJournalLineResponseDto>();

            var taxCode = await _taxCodeRepository.GetByIdAsync(entity.TaxCodeId);
            if (taxCode != null)
            {
                response.TaxCode = taxCode.Code;
                response.TaxCodeNameEn = taxCode.NameEn;
                response.TaxCodeNameAr = taxCode.NameAr;
            }

            return response;
        }

        #endregion
    }
}
