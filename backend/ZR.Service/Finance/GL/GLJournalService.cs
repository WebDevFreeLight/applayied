using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Repository;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.GL;
using ZR.Service.Finance.Validation;
using ZR.Service.Finance.Configuration;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// GL Journal Service - خدمة دفتر اليومية
    /// Enhanced with comprehensive validation
    /// </summary>
    [AppService(ServiceType = typeof(IGLJournalService), ServiceLifetime = LifeTime.Transient)]
    public class GLJournalService : BaseService<GLJournal>, IGLJournalService
    {
        private readonly IGLJournalRepository _journalRepository;
        private readonly IGLJournalEntryRepository _entryRepository;
        private readonly IGLJournalEntryService _entryService;
        private readonly IJournalValidationService _validationService;
        private readonly IGLPeriodClosingRepository _periodRepository;
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLPeriodClosingService _periodClosingService;
        private readonly IFiscalYearHelper _fiscalYearHelper;

        // Lock object for thread-safe journal number generation
        private static readonly object _journalNumberLock = new object();

        public GLJournalService(
            IGLJournalRepository journalRepository,
            IGLJournalEntryRepository entryRepository,
            IGLJournalEntryService entryService,
            IJournalValidationService validationService,
            IGLPeriodClosingRepository periodRepository,
            IGLAccountRepository accountRepository,
            IGLPeriodClosingService periodClosingService,
            IFiscalYearHelper fiscalYearHelper)
        {
            _journalRepository = journalRepository;
            _entryRepository = entryRepository;
            _entryService = entryService;
            _validationService = validationService;
            _periodRepository = periodRepository;
            _accountRepository = accountRepository;
            _periodClosingService = periodClosingService;
            _fiscalYearHelper = fiscalYearHelper;
        }

        /// <summary>
        /// Get paginated list of journals
        /// </summary>
        public PagedInfo<JournalResponseDto> GetList(GLJournalQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLJournal>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.JournalNumber), it => it.JournalNumber.Contains(query.JournalNumber));
            predicate = predicate.AndIF(query.Status.HasValue, it => it.Status == query.Status.Value);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Source), it => it.Source == query.Source);
            predicate = predicate.AndIF(query.BeginJournalDate.HasValue, it => it.JournalDate >= query.BeginJournalDate.Value);
            predicate = predicate.AndIF(query.EndJournalDate.HasValue, it => it.JournalDate <= query.EndJournalDate.Value);
            predicate = predicate.AndIF(query.BeginCreateTime.HasValue, it => it.Create_time >= query.BeginCreateTime.Value);
            predicate = predicate.AndIF(query.EndCreateTime.HasValue, it => it.Create_time <= query.EndCreateTime.Value);
            predicate = predicate.AndIF(query.FiscalYear.HasValue, it => it.FiscalYear == query.FiscalYear.Value);
            predicate = predicate.AndIF(query.FiscalPeriod.HasValue, it => it.FiscalPeriod == query.FiscalPeriod.Value);

            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.JournalDate, OrderByType.Desc)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .ToPage(pager);

            var dtos = result.Result.Select(j =>
            {
                var dto = j.Adapt<JournalResponseDto>();
                var (totalDebit, totalCredit) = _entryService.GetJournalTotals(j.Id);
                dto.TotalDebit = totalDebit;
                dto.TotalCredit = totalCredit;
                return dto;
            }).ToList();

            return new PagedInfo<JournalResponseDto>
            {
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalNum = result.TotalNum,
                Result = dtos
            };
        }

        /// <summary>
        /// Get journal by ID
        /// </summary>
        public JournalResponseDto GetById(Guid id)
        {
            var journal = GetFirst(x => x.Id == id);
            if (journal == null) return null;

            var dto = journal.Adapt<JournalResponseDto>();
            var (totalDebit, totalCredit) = _entryService.GetJournalTotals(id);
            dto.TotalDebit = totalDebit;
            dto.TotalCredit = totalCredit;
            return dto;
        }

        /// <summary>
        /// Get journal with entries
        /// </summary>
        public JournalResponseDto GetWithEntries(Guid id)
        {
            var journal = GetFirst(x => x.Id == id);
            if (journal == null) return null;

            var dto = journal.Adapt<JournalResponseDto>();
            dto.Entries = _entryService.GetByJournalId(id);
            dto.TotalDebit = dto.Entries.Sum(e => e.Debit);
            dto.TotalCredit = dto.Entries.Sum(e => e.Credit);
            return dto;
        }

        /// <summary>
        /// Create journal with comprehensive validation
        /// </summary>
        public GLJournal Create(CreateJournalDto dto, string userName)
        {
            // Step 0: CRITICAL - Check if journal date is in closed/locked period
            // This MUST be the first validation - prevents ANY operation in closed periods
            long? companyId = dto.CompanyId;
            _periodClosingService.CheckIfDateInClosedOrLockedPeriod(dto.JournalDate, companyId, allowDrafts: false);

            // Step 1: Validate before creation
            var validation = _validationService.ValidateForCreate(dto);
            if (!validation.IsValid)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "Errors", string.Join("; ", validation.Errors) } });
            }

            var model = dto.Adapt<GLJournal>();
            model.Id = Guid.NewGuid();
            model.Create_by = userName;
            model.Create_time = DateTime.Now;
            model.Status = JournalStatus.Draft;

            // Step 2: Generate thread-safe journal number
            if (string.IsNullOrEmpty(model.JournalNumber))
            {
                model.JournalNumber = GenerateNextJournalNumber();
            }

            // Step 3: Auto-calculate fiscal year and period if not provided
            // Use FiscalYearHelper to correctly calculate fiscal year and period based on ERP Settings
            if (!model.FiscalYear.HasValue)
            {
                model.FiscalYear = _fiscalYearHelper.GetFiscalYearFromDate(model.JournalDate, model.CompanyID);
            }
            if (!model.FiscalPeriod.HasValue)
            {
                model.FiscalPeriod = _fiscalYearHelper.GetFiscalPeriodFromDate(model.JournalDate, model.CompanyID);
            }

            // Step 4: Set source if not provided
            if (string.IsNullOrEmpty(model.Source))
            {
                model.Source = "Manual";
            }

            // Check if already in an ambient TransactionScope - if so, don't start our own transaction
            // This prevents nested transaction conflicts when called from Asset operations
            if (System.Transactions.Transaction.Current != null)
            {
                // Already in a TransactionScope, execute without starting a new transaction
                ExecuteJournalCreation(model, dto, userName);
                return model;
            }

            // No ambient transaction, use our own transaction
            var result = UseTran(() =>
            {
                ExecuteJournalCreation(model, dto, userName);
            });

            return result.IsSuccess ? model : null;
        }

        /// <summary>
        /// Execute the actual journal creation logic (called by Create method)
        /// </summary>
        private void ExecuteJournalCreation(GLJournal model, CreateJournalDto dto, string userName)
        {
            Insert(model);

            // Insert entries with validation
            if (dto.Entries != null && dto.Entries.Any())
            {
                var lineNumber = 1;
                foreach (var entryDto in dto.Entries)
                {
                    entryDto.LineNumber = lineNumber++;
                }
                _entryService.CreateMultiple(dto.Entries, model.Id, userName);
            }

            // Update totals on the journal header
            var (totalDebit, totalCredit) = _entryService.GetJournalTotals(model.Id);
            model.TotalDebit = totalDebit;
            model.TotalCredit = totalCredit;
            Update(model, it => new { it.TotalDebit, it.TotalCredit }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Update journal with validation
        /// </summary>
        public int Update(UpdateJournalDto dto, string userName)
        {
            var existing = GetFirst(x => x.Id == dto.Id);
            if (existing == null)
                return 0;

            // Step 0: CRITICAL - Check if journal date is in closed/locked period
            // This prevents editing journals with dates in closed periods
            long? companyId = dto.CompanyId ?? existing.CompanyID;
            _periodClosingService.CheckIfDateInClosedOrLockedPeriod(dto.JournalDate, companyId, allowDrafts: false);

            // Step 1: Validate before update
            var validation = _validationService.ValidateForUpdate(dto);
            if (!validation.IsValid)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "Errors", string.Join("; ", validation.Errors) } });
            }

            // Step 2: Status check (redundant with validation but kept for safety)
            if (existing.Status == JournalStatus.Posted || existing.Status == JournalStatus.Cancelled)
                return 0;

            var model = dto.Adapt<GLJournal>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            var result = UseTran(() =>
            {
                Update(model, it => new
                {
                    it.JournalNumber,
                    it.DescriptionAr,
                    it.DescriptionEn,
                    it.JournalDate,
                    it.Source,
                    it.ReferenceNumber,
                    it.Currency,
                    it.FiscalYear,
                    it.FiscalPeriod,
                    it.Update_by,
                    it.Update_time
                }, f => f.Id == model.Id);

                // Update entries if provided
                if (dto.Entries != null)
                {
                    // Delete existing entries
                    _entryService.DeleteByJournalId(dto.Id);

                    // Insert new entries with line numbers
                    var createDtos = dto.Entries.Select((e, index) => new CreateJournalEntryDto
                    {
                        JournalId = dto.Id,
                        AccountId = e.AccountId,
                        LineNumber = index + 1,
                        Debit = e.Debit,
                        Credit = e.Credit,
                        DescriptionAr = e.DescriptionAr,
                        DescriptionEn = e.DescriptionEn,
                        CostCenterId = e.CostCenterId,
                        DepartmentId = e.DepartmentId,
                        ProjectId = e.ProjectId,
                        ReferenceNumber = e.ReferenceNumber,
                        Currency = e.Currency,
                        ExchangeRate = e.ExchangeRate,
                        TenantId = e.TenantId
                    }).ToList();

                    _entryService.CreateMultiple(createDtos, dto.Id, userName);
                }

                // Update totals on the journal header
                var (totalDebit, totalCredit) = _entryService.GetJournalTotals(dto.Id);
                Update(new GLJournal { Id = dto.Id, TotalDebit = totalDebit, TotalCredit = totalCredit },
                    it => new { it.TotalDebit, it.TotalCredit }, f => f.Id == dto.Id);
            });

            return result.IsSuccess ? 1 : 0;
        }

        /// <summary>
        /// Delete journals (soft delete by setting status to cancelled)
        /// Only draft journals can be deleted
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Validate all journals are draft before deletion
            foreach (var id in ids)
            {
                var journal = GetFirst(x => x.Id == id);
                if (journal == null)
                    throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_NOT_FOUND, new Dictionary<string, object> { { "JournalId", id } });
                if (journal.Status != JournalStatus.Draft)
                    throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_ONLY_DRAFT_DELETE, new Dictionary<string, object> { { "JournalId", id }, { "JournalNumber", journal.JournalNumber }, { "Status", journal.Status.ToString() } });

                // Step 0: CRITICAL - Check if journal date is in closed/locked period
                // This prevents deleting journals with dates in closed periods
                long? companyId = journal.CompanyID;
                _periodClosingService.CheckIfDateInClosedOrLockedPeriod(journal.JournalDate, companyId, allowDrafts: false);
            }

            return Context.Updateable<GLJournal>()
                .SetColumns(it => it.Status == JournalStatus.Cancelled)
                .SetColumns(it => it.Update_time == DateTime.Now)
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Post journal with comprehensive validation
        /// </summary>
        public int PostJournal(Guid id, string userName)
        {
            var journal = GetFirst(x => x.Id == id);
            if (journal == null)
                return 0;

            // Step 0: CRITICAL - Check if journal date is in closed/locked period
            // This is the MOST IMPORTANT validation - MUST NOT post to closed periods
            long? companyId = journal.CompanyID;
            _periodClosingService.CheckIfDateInClosedOrLockedPeriod(journal.JournalDate, companyId, allowDrafts: false);

            // Step 1: Comprehensive validation for posting
            var validation = _validationService.ValidateForPost(id);
            if (!validation.IsValid)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_POSTING_FAILED, new Dictionary<string, object> { { "Errors", string.Join("; ", validation.Errors) } });
            }

            // Step 2: Update totals before posting
            var (totalDebit, totalCredit) = _entryService.GetJournalTotals(id);

            // Step 3: Parse user ID if possible
            Guid? postedByGuid = null;
            if (Guid.TryParse(userName, out var parsedGuid))
            {
                postedByGuid = parsedGuid;
            }

            return Context.Updateable<GLJournal>()
                .SetColumns(it => new GLJournal
                {
                    Status = JournalStatus.Posted,
                    PostedDate = DateTime.Now,
                    PostedBy = postedByGuid,
                    TotalDebit = totalDebit,
                    TotalCredit = totalCredit,
                    Update_by = userName,
                    Update_time = DateTime.Now
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Cancel journal - only non-posted journals
        /// </summary>
        public int CancelJournal(Guid id, string userName)
        {
            var journal = GetFirst(x => x.Id == id);
            if (journal == null)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_NOT_FOUND, new Dictionary<string, object> { { "JournalId", id } });

            if (journal.Status == JournalStatus.Cancelled)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_ALREADY_CANCELLED, new Dictionary<string, object> { { "JournalId", id } });

            if (journal.Status == JournalStatus.Posted)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_CANNOT_CANCEL_POSTED, new Dictionary<string, object> { { "JournalId", id } });

            return Context.Updateable<GLJournal>()
                .SetColumns(it => new GLJournal
                {
                    Status = JournalStatus.Cancelled,
                    Update_by = userName,
                    Update_time = DateTime.Now
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Reverse a posted journal - creates a new reversal journal
        /// </summary>
        public GLJournal ReverseJournal(ReverseJournalDto dto, string userName)
        {
            var originalJournal = GetFirst(x => x.Id == dto.Id);
            if (originalJournal == null)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_NOT_FOUND, new Dictionary<string, object> { { "JournalId", dto.Id } });

            var reversalDate = dto.ReversalDate ?? DateTime.Now.Date;

            // Step 0: CRITICAL - Check if reversal date is in closed/locked period
            // This prevents creating reversals in closed periods
            long? companyId = originalJournal.CompanyID;
            _periodClosingService.CheckIfDateInClosedOrLockedPeriod(reversalDate, companyId, allowDrafts: false);

            // Step 1: Validate for reversal
            var validation = _validationService.ValidateForReversal(dto.Id, dto.ReversalDate);
            if (!validation.IsValid)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "Errors", string.Join("; ", validation.Errors) } });
            }

            var originalEntries = _entryService.GetByJournalId(dto.Id);
            if (originalEntries == null || !originalEntries.Any())
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_NO_ENTRIES, new Dictionary<string, object> { { "JournalId", dto.Id } });

            var reversalReason = dto.ReversalReason ?? "Reversal of " + originalJournal.JournalNumber;

            // Step 2: Create reversal journal
            var reversalJournal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = GenerateNextJournalNumber(),
                JournalDate = reversalDate,
                DescriptionAr = $"عكس القيد {originalJournal.JournalNumber}: {reversalReason}",
                DescriptionEn = $"Reversal of {originalJournal.JournalNumber}: {reversalReason}",
                Status = JournalStatus.Draft,
                Source = "Reversal",
                SourceDocumentType = "Reversal",
                SourceDocumentId = originalJournal.Id,
                Currency = originalJournal.Currency,
                FiscalYear = reversalDate.Year,
                FiscalPeriod = reversalDate.Month,
                OriginalJournalId = originalJournal.Id,
                Create_by = userName,
                Create_time = DateTime.Now
            };

            var result = UseTran(() =>
            {
                Insert(reversalJournal);

                // Step 3: Create reversed entries (swap debit and credit)
                var lineNumber = 1;
                foreach (var originalEntry in originalEntries)
                {
                    var reversalEntry = new GLJournalEntry
                    {
                        Id = Guid.NewGuid(),
                        JournalId = reversalJournal.Id,
                        AccountId = originalEntry.AccountId,
                        LineNumber = lineNumber++,
                        Debit = originalEntry.Credit,  // Swap: credit becomes debit
                        Credit = originalEntry.Debit,  // Swap: debit becomes credit
                        DescriptionAr = $"عكس: {originalEntry.DescriptionAr}",
                        DescriptionEn = $"Reversal: {originalEntry.DescriptionEn}",
                        CostCenterId = originalEntry.CostCenterId,
                        DepartmentId = originalEntry.DepartmentId,
                        ProjectId = originalEntry.ProjectId,
                        Currency = originalEntry.Currency,
                        ExchangeRate = originalEntry.ExchangeRate,
                        BaseDebit = originalEntry.BaseCredit,
                        BaseCredit = originalEntry.BaseDebit,
                        Create_by = userName,
                        Create_time = DateTime.Now,
                        CompanyID = originalEntry.CompanyID
                    };
                    Context.Insertable(reversalEntry).ExecuteCommand();
                }

                // Step 4: Update totals on reversal journal
                var (totalDebit, totalCredit) = _entryService.GetJournalTotals(reversalJournal.Id);
                reversalJournal.TotalDebit = totalDebit;
                reversalJournal.TotalCredit = totalCredit;
                Update(reversalJournal, it => new { it.TotalDebit, it.TotalCredit }, f => f.Id == reversalJournal.Id);

                // Step 5: Mark original journal as reversed
                originalJournal.Status = JournalStatus.Reversed;
                originalJournal.ReversalJournalId = reversalJournal.Id;
                originalJournal.Update_by = userName;
                originalJournal.Update_time = DateTime.Now;
                Update(originalJournal, it => new { it.Status, it.ReversalJournalId, it.Update_by, it.Update_time },
                    f => f.Id == originalJournal.Id);
            });

            return result.IsSuccess ? reversalJournal : null;
        }

        /// <summary>
        /// Submit journal for approval
        /// </summary>
        public int SubmitJournal(Guid id, string userName)
        {
            var journal = GetFirst(x => x.Id == id);
            if (journal == null)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_NOT_FOUND, new Dictionary<string, object> { { "JournalId", id } });

            if (journal.Status != JournalStatus.Draft)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_ONLY_DRAFT_SUBMIT, new Dictionary<string, object> { { "JournalId", id }, { "Status", journal.Status.ToString() } });

            // Basic validation before submission
            if (!IsJournalBalanced(id))
                throw new AppException(ExceptionCode.FINANCE_GL_UNBALANCED_ENTRY, new Dictionary<string, object> { { "JournalId", id } });

            return Context.Updateable<GLJournal>()
                .SetColumns(it => new GLJournal
                {
                    Status = JournalStatus.Submitted,
                    Update_by = userName,
                    Update_time = DateTime.Now
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Approve journal
        /// </summary>
        public int ApproveJournal(Guid id, string userName)
        {
            var journal = GetFirst(x => x.Id == id);
            if (journal == null)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_NOT_FOUND, new Dictionary<string, object> { { "JournalId", id } });

            if (journal.Status != JournalStatus.Submitted)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_ONLY_SUBMITTED_APPROVE, new Dictionary<string, object> { { "JournalId", id }, { "Status", journal.Status.ToString() } });

            Guid? approvedByGuid = null;
            if (Guid.TryParse(userName, out var parsedGuid))
            {
                approvedByGuid = parsedGuid;
            }

            return Context.Updateable<GLJournal>()
                .SetColumns(it => new GLJournal
                {
                    Status = JournalStatus.Approved,
                    ApprovedDate = DateTime.Now,
                    ApprovedBy = approvedByGuid,
                    Update_by = userName,
                    Update_time = DateTime.Now
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Reject journal
        /// </summary>
        public int RejectJournal(Guid id, string reason, string userName)
        {
            var journal = GetFirst(x => x.Id == id);
            if (journal == null)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_NOT_FOUND, new Dictionary<string, object> { { "JournalId", id } });

            if (journal.Status != JournalStatus.Submitted)
                throw new AppException(ExceptionCode.FINANCE_GL_JOURNAL_ONLY_SUBMITTED_REJECT, new Dictionary<string, object> { { "JournalId", id }, { "Status", journal.Status.ToString() } });

            return Context.Updateable<GLJournal>()
                .SetColumns(it => new GLJournal
                {
                    Status = JournalStatus.Rejected,
                    Update_by = userName,
                    Update_time = DateTime.Now
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Generate next journal number - THREAD-SAFE
        /// </summary>
        public string GenerateNextJournalNumber()
        {
            lock (_journalNumberLock)
            {
                var year = DateTime.Now.Year;
                var prefix = $"JV-{year}-";

                var lastJournal = Queryable()
                    .Where(x => x.JournalNumber.StartsWith(prefix))
                    .OrderByDescending(x => x.JournalNumber)
                    .First();

                if (lastJournal == null)
                {
                    return $"{prefix}00001";
                }

                var lastNumber = lastJournal.JournalNumber.Replace(prefix, "");
                if (int.TryParse(lastNumber, out int num))
                {
                    return $"{prefix}{(num + 1):D5}";
                }

                return $"{prefix}00001";
            }
        }

        /// <summary>
        /// Check if journal is balanced
        /// </summary>
        public bool IsJournalBalanced(Guid journalId)
        {
            var (totalDebit, totalCredit) = _entryService.GetJournalTotals(journalId);
            return Math.Abs(totalDebit - totalCredit) < 0.0001m && totalDebit > 0;
        }

        /// <summary>
        /// Validate journal - returns detailed validation result
        /// </summary>
        public JournalValidationResultDto ValidateJournal(Guid journalId)
        {
            return _validationService.ValidateForPost(journalId);
        }

        /// <summary>
        /// Get journals by date range
        /// </summary>
        public List<GLJournal> GetByDateRange(DateTime startDate, DateTime endDate, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLJournal>();
            predicate = predicate.And(it => it.JournalDate >= startDate && it.JournalDate <= endDate);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId.Value);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.JournalDate)
                .ToList();
        }

        /// <summary>
        /// Get journals by status
        /// </summary>
        public List<GLJournal> GetByStatus(JournalStatus status, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLJournal>();
            predicate = predicate.And(it => it.Status == status);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId.Value);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get Year-End Closing Journal for a fiscal year
        /// البحث عن قيد إقفال نهاية السنة للسنة المالية
        /// Searches by Source="YearEndClosing" or SourceDocumentType="YearEndClosing"
        /// Also searches Arabic/English descriptions as fallback
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to search for</param>
        /// <param name="companyId">Optional company ID for multi-tenant support</param>
        /// <returns>The Year-End Closing Journal if found (most recent if multiple), null otherwise</returns>
        public GLJournal GetYearEndClosingJournal(int fiscalYear, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLJournal>();

            // Filter by fiscal year
            predicate = predicate.And(x => x.FiscalYear == fiscalYear);

            // Filter by company if provided
            predicate = predicate.AndIF(companyId.HasValue, x => x.CompanyID == companyId.Value);

            // Must be Posted (only reverse posted journals)
            predicate = predicate.And(x => x.Status == JournalStatus.Posted);

            // Search for Year-End Closing Journal by Source or SourceDocumentType
            // Also search by description keywords as fallback
            predicate = predicate.And(x =>
                x.Source == "YearEndClosing" ||
                x.SourceDocumentType == "YearEndClosing" ||
                (x.DescriptionAr != null && x.DescriptionAr.Contains("إقفال السنة")) ||
                (x.DescriptionEn != null && x.DescriptionEn.Contains("Year-End Clos")));

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.JournalDate, OrderByType.Desc)  // Get most recent if multiple exist
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .First();
        }
    }
}
