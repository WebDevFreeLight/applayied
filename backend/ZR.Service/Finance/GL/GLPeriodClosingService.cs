using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Exceptions;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.GL;
using ZR.Service.Finance.Configuration;

namespace ZR.Service.Finance
{
    /// <summary>
    /// GL Period Closing Service - خدمة إقفال الفترات
    /// Enhanced with auto-reversal of Year-End Closing Journals when reopening fiscal year periods
    /// Uses Lazy<IGLJournalService> to break circular dependency with GLJournalService
    /// </summary>
    [AppService(ServiceType = typeof(IGLPeriodClosingService), ServiceLifetime = LifeTime.Transient)]
    public class GLPeriodClosingService : BaseService<GLPeriodClosing>, IGLPeriodClosingService
    {
        private readonly IGLPeriodClosingRepository _periodRepository;
        private readonly IGLJournalRepository _journalRepository;
        private readonly IGLJournalEntryRepository _entryRepository;
        private readonly Lazy<IGLJournalService> _journalServiceLazy;
        private readonly IFiscalYearHelper _fiscalYearHelper;

        // Property to access the journal service - resolved only when needed
        private IGLJournalService JournalService => _journalServiceLazy.Value;

        public GLPeriodClosingService(
            IGLPeriodClosingRepository periodRepository,
            IGLJournalRepository journalRepository,
            IGLJournalEntryRepository entryRepository,
            Lazy<IGLJournalService> journalService,
            IFiscalYearHelper fiscalYearHelper)
        {
            _periodRepository = periodRepository;
            _journalRepository = journalRepository;
            _entryRepository = entryRepository;
            _journalServiceLazy = journalService;
            _fiscalYearHelper = fiscalYearHelper;
        }

        public PagedInfo<PeriodClosingResponseDto> GetList(GLPeriodClosingQueryDto query, PagerInfo pager)
        {
            var searchModel = new GLPeriodClosing
            {
                PeriodType = query.PeriodType ?? PeriodType.Month,
                Status = query.Status ?? PeriodStatus.Open,
                FiscalYear = query.FiscalYear ?? 0,
                FiscalPeriod = query.FiscalPeriod ?? 0,
                CompanyID = query.TenantId
            };

            var result = _periodRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(MapToDto).ToList();

            return new PagedInfo<PeriodClosingResponseDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        public PeriodClosingResponseDto GetById(Guid id)
        {
            var entity = _periodRepository.GetById(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<PeriodClosingResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _periodRepository.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public List<PeriodClosingResponseDto> GetByFiscalYear(int fiscalYear, long? tenantId = null)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var entities = _periodRepository.GetByFiscalYear(fiscalYear, null);
            return entities.Select(MapToDto).ToList();
        }

        public List<PeriodClosingResponseDto> GetOpenPeriods(long? tenantId = null)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var entities = _periodRepository.GetOpenPeriods(null);
            return entities.Select(MapToDto).ToList();
        }

        public PeriodClosingResponseDto Create(CreatePeriodClosingDto dto, string createdBy)
        {
            // Check if period already exists
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            if (_periodRepository.PeriodExists(dto.PeriodStartDate, dto.PeriodEndDate, dto.PeriodType, null))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_EXISTS);
            }

            Guid? createdByGuid = null;
            if (Guid.TryParse(createdBy, out Guid parsedGuid))
                createdByGuid = parsedGuid;

            var period = new GLPeriodClosing
            {
                Id = Guid.NewGuid(),
                PeriodType = dto.PeriodType,
                PeriodStartDate = dto.PeriodStartDate.Date,
                PeriodEndDate = dto.PeriodEndDate.Date,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                FiscalPeriod = dto.FiscalPeriod ?? dto.PeriodStartDate.Month,
                Status = PeriodStatus.Open,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                CompanyID = dto.TenantId,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            // Calculate voucher statistics for this period
            UpdatePeriodStatistics(period);

            _periodRepository.CreatePeriod(period);
            return MapToDto(period);
        }

        public async Task<PeriodClosingResponseDto> CreateAsync(CreatePeriodClosingDto dto, string createdBy)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            if (await _periodRepository.PeriodExistsAsync(dto.PeriodStartDate, dto.PeriodEndDate, dto.PeriodType, null))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_EXISTS);
            }

            Guid? createdByGuid = null;
            if (Guid.TryParse(createdBy, out Guid parsedGuid))
                createdByGuid = parsedGuid;

            var period = new GLPeriodClosing
            {
                Id = Guid.NewGuid(),
                PeriodType = dto.PeriodType,
                PeriodStartDate = dto.PeriodStartDate.Date,
                PeriodEndDate = dto.PeriodEndDate.Date,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                FiscalPeriod = dto.FiscalPeriod ?? dto.PeriodStartDate.Month,
                Status = PeriodStatus.Open,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                CompanyID = dto.TenantId,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            await UpdatePeriodStatisticsAsync(period);
            await _periodRepository.CreatePeriodAsync(period);
            return MapToDto(period);
        }

        public int ClosePeriod(ClosePeriodDto dto, string closedBy)
        {
            var period = _periodRepository.GetById(dto.Id);
            if (period == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_FOUND, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            if (period.Status == PeriodStatus.Closed)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_CLOSED, new Dictionary<string, object> { { "PeriodId", dto.Id }, { "Reason", "AlreadyClosed" } });

            if (period.Status == PeriodStatus.Locked)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_LOCKED);

            // CRITICAL: Always get fresh statistics regardless of ForceClose flag
            // This ensures the unposted count is ALWAYS accurate for audit trail
            // and the force close confirmation dialog shows real-time data
            var periodStatus = GetPeriodStatus(dto.Id);
            if (periodStatus == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_STATUS_FAILED, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            // Now validate based on fresh statistics
            if (!dto.ForceClose)
            {
                // Normal close: block if validation fails
                if (periodStatus.Status != PeriodStatus.Open)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_ALREADY_OPEN, new Dictionary<string, object> { { "PeriodId", dto.Id }, { "Status", periodStatus.StatusName } });

                if (!periodStatus.AllVouchersPosted)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_UNPOSTED_VOUCHERS, new Dictionary<string, object> { { "UnpostedCount", periodStatus.UnpostedVouchers } });

                if (!periodStatus.IsBalanced)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_BALANCED, new Dictionary<string, object> { { "TotalDebit", periodStatus.TotalDebit }, { "TotalCredit", periodStatus.TotalCredit } });
            }
            else
            {
                // Force close: only block on hard constraints (balance must still match)
                // Unposted vouchers are allowed, but log them for audit
                if (!periodStatus.IsBalanced)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_BALANCED, new Dictionary<string, object> { { "TotalDebit", periodStatus.TotalDebit }, { "TotalCredit", periodStatus.TotalCredit } });

                // Record the force close decision with accurate statistics
                if (periodStatus.UnpostedVouchers > 0)
                {
                    var forceCloseNote = $"[FORCE CLOSED] Unposted vouchers at time of closing: {periodStatus.UnpostedVouchers}";
                    dto.ClosingNotes = string.IsNullOrEmpty(dto.ClosingNotes)
                        ? forceCloseNote
                        : $"{dto.ClosingNotes}\n{forceCloseNote}";
                }
            }

            // Update statistics before closing
            UpdatePeriodStatistics(period);

            Guid? closedByGuid = null;
            if (Guid.TryParse(closedBy, out Guid parsedGuid))
                closedByGuid = parsedGuid;

            period.ClosingNotes = dto.ClosingNotes;
            period.Update_by = closedBy;
            _periodRepository.UpdatePeriod(period);

            return _periodRepository.UpdateStatus(dto.Id, PeriodStatus.Closed, closedByGuid);
        }

        public async Task<int> ClosePeriodAsync(ClosePeriodDto dto, string closedBy)
        {
            var period = await _periodRepository.GetByIdAsync(dto.Id);
            if (period == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_FOUND, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            if (period.Status == PeriodStatus.Closed)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_CLOSED, new Dictionary<string, object> { { "PeriodId", dto.Id }, { "Reason", "AlreadyClosed" } });

            if (period.Status == PeriodStatus.Locked)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_LOCKED);

            // CRITICAL: Always get fresh statistics regardless of ForceClose flag
            // This ensures the unposted count is ALWAYS accurate for audit trail
            // and the force close confirmation dialog shows real-time data
            var periodStatus = await GetPeriodStatusAsync(dto.Id);
            if (periodStatus == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_STATUS_FAILED, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            // Now validate based on fresh statistics
            if (!dto.ForceClose)
            {
                // Normal close: block if validation fails
                if (periodStatus.Status != PeriodStatus.Open)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_ALREADY_OPEN, new Dictionary<string, object> { { "PeriodId", dto.Id }, { "Status", periodStatus.StatusName } });

                if (!periodStatus.AllVouchersPosted)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_UNPOSTED_VOUCHERS, new Dictionary<string, object> { { "UnpostedCount", periodStatus.UnpostedVouchers } });

                if (!periodStatus.IsBalanced)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_BALANCED, new Dictionary<string, object> { { "TotalDebit", periodStatus.TotalDebit }, { "TotalCredit", periodStatus.TotalCredit } });
            }
            else
            {
                // Force close: only block on hard constraints (balance must still match)
                // Unposted vouchers are allowed, but log them for audit
                if (!periodStatus.IsBalanced)
                    throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_BALANCED, new Dictionary<string, object> { { "TotalDebit", periodStatus.TotalDebit }, { "TotalCredit", periodStatus.TotalCredit } });

                // Record the force close decision with accurate statistics
                if (periodStatus.UnpostedVouchers > 0)
                {
                    var forceCloseNote = $"[FORCE CLOSED] Unposted vouchers at time of closing: {periodStatus.UnpostedVouchers}";
                    dto.ClosingNotes = string.IsNullOrEmpty(dto.ClosingNotes)
                        ? forceCloseNote
                        : $"{dto.ClosingNotes}\n{forceCloseNote}";
                }
            }

            await UpdatePeriodStatisticsAsync(period);

            Guid? closedByGuid = null;
            if (Guid.TryParse(closedBy, out Guid parsedGuid))
                closedByGuid = parsedGuid;

            period.ClosingNotes = dto.ClosingNotes;
            period.Update_by = closedBy;
            await _periodRepository.UpdatePeriodAsync(period);

            return await _periodRepository.UpdateStatusAsync(dto.Id, PeriodStatus.Closed, closedByGuid);
        }

        public int ReopenPeriod(ReopenPeriodDto dto, string reopenedBy)
        {
            var period = _periodRepository.GetById(dto.Id);
            if (period == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_FOUND, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            if (period.Status == PeriodStatus.Open)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_ALREADY_OPEN, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            if (period.Status == PeriodStatus.Locked)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_LOCKED);

            Guid? reopenedByGuid = null;
            if (Guid.TryParse(reopenedBy, out Guid parsedGuid))
                reopenedByGuid = parsedGuid;

            // CRITICAL FIX: Auto-reverse Year-End Closing Journal when reopening a fiscal year period
            // This prevents Retained Earnings from being permanently inflated (IFRS/GAAP compliance)
            GLJournal reversalJournal = null;
            if (period.PeriodType == PeriodType.Year)
            {
                // Find the Year-End Closing Journal for this fiscal year
                var yearEndJournal = JournalService.GetYearEndClosingJournal(period.FiscalYear, period.CompanyID);

                if (yearEndJournal != null && yearEndJournal.Status == JournalStatus.Posted)
                {
                    // Automatically reverse the Year-End closing journal
                    var reversalDto = new ReverseJournalDto
                    {
                        Id = yearEndJournal.Id,
                        ReversalDate = DateTime.Now.Date,
                        ReversalReason = $"Auto-reversal due to fiscal year {period.FiscalYear} reopening - عكس تلقائي بسبب إعادة فتح السنة المالية {period.FiscalYear}"
                    };

                    reversalJournal = JournalService.ReverseJournal(reversalDto, reopenedBy);

                    // Auto-post the reversal journal to ensure balances are corrected
                    if (reversalJournal != null)
                    {
                        JournalService.PostJournal(reversalJournal.Id, reopenedBy);
                    }

                    // Update period notes with reversal information
                    period.ClosingNotes = $"Year-End Closing Journal {yearEndJournal.JournalNumber} auto-reversed.\n" +
                                          $"Reversal Journal: {reversalJournal?.JournalNumber}\n" +
                                          $"Reopened: {dto.Reason}\n{period.ClosingNotes}";
                }
                else
                {
                    period.ClosingNotes = $"Reopened: {dto.Reason}\n{period.ClosingNotes}";
                }
            }
            else
            {
                period.ClosingNotes = $"Reopened: {dto.Reason}\n{period.ClosingNotes}";
            }

            period.Update_by = reopenedBy;
            _periodRepository.UpdatePeriod(period);

            return _periodRepository.UpdateStatus(dto.Id, PeriodStatus.Open, reopenedByGuid);
        }

        public async Task<int> ReopenPeriodAsync(ReopenPeriodDto dto, string reopenedBy)
        {
            var period = await _periodRepository.GetByIdAsync(dto.Id);
            if (period == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_FOUND, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            if (period.Status == PeriodStatus.Open)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_ALREADY_OPEN, new Dictionary<string, object> { { "PeriodId", dto.Id } });

            if (period.Status == PeriodStatus.Locked)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_LOCKED);

            Guid? reopenedByGuid = null;
            if (Guid.TryParse(reopenedBy, out Guid parsedGuid))
                reopenedByGuid = parsedGuid;

            // CRITICAL FIX: Auto-reverse Year-End Closing Journal when reopening a fiscal year period
            // This prevents Retained Earnings from being permanently inflated (IFRS/GAAP compliance)
            GLJournal reversalJournal = null;
            if (period.PeriodType == PeriodType.Year)
            {
                // Find the Year-End Closing Journal for this fiscal year
                // Note: GetYearEndClosingJournal is synchronous, but we're in async context
                var yearEndJournal = await Task.Run(() => JournalService.GetYearEndClosingJournal(period.FiscalYear, period.CompanyID));

                if (yearEndJournal != null && yearEndJournal.Status == JournalStatus.Posted)
                {
                    // Automatically reverse the Year-End closing journal
                    var reversalDto = new ReverseJournalDto
                    {
                        Id = yearEndJournal.Id,
                        ReversalDate = DateTime.Now.Date,
                        ReversalReason = $"Auto-reversal due to fiscal year {period.FiscalYear} reopening - عكس تلقائي بسبب إعادة فتح السنة المالية {period.FiscalYear}"
                    };

                    // ReverseJournal and PostJournal are synchronous, wrap in Task.Run
                    reversalJournal = await Task.Run(() => JournalService.ReverseJournal(reversalDto, reopenedBy));

                    // Auto-post the reversal journal to ensure balances are corrected
                    if (reversalJournal != null)
                    {
                        await Task.Run(() => JournalService.PostJournal(reversalJournal.Id, reopenedBy));
                    }

                    // Update period notes with reversal information
                    period.ClosingNotes = $"Year-End Closing Journal {yearEndJournal.JournalNumber} auto-reversed.\n" +
                                          $"Reversal Journal: {reversalJournal?.JournalNumber}\n" +
                                          $"Reopened: {dto.Reason}\n{period.ClosingNotes}";
                }
                else
                {
                    period.ClosingNotes = $"Reopened: {dto.Reason}\n{period.ClosingNotes}";
                }
            }
            else
            {
                period.ClosingNotes = $"Reopened: {dto.Reason}\n{period.ClosingNotes}";
            }

            period.Update_by = reopenedBy;
            await _periodRepository.UpdatePeriodAsync(period);

            return await _periodRepository.UpdateStatusAsync(dto.Id, PeriodStatus.Open, reopenedByGuid);
        }

        public int LockPeriod(Guid id, string lockedBy)
        {
            var period = _periodRepository.GetById(id);
            if (period == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_FOUND, new Dictionary<string, object> { { "PeriodId", id } });

            if (period.Status != PeriodStatus.Closed)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_CLOSED, new Dictionary<string, object> { { "PeriodId", id }, { "Reason", "MustBeClosedToLock" } });

            Guid? lockedByGuid = null;
            if (Guid.TryParse(lockedBy, out Guid parsedGuid))
                lockedByGuid = parsedGuid;

            return _periodRepository.UpdateStatus(id, PeriodStatus.Locked, lockedByGuid);
        }

        public async Task<int> LockPeriodAsync(Guid id, string lockedBy)
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_FOUND, new Dictionary<string, object> { { "PeriodId", id } });

            if (period.Status != PeriodStatus.Closed)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_NOT_CLOSED, new Dictionary<string, object> { { "PeriodId", id }, { "Reason", "MustBeClosedToLock" } });

            Guid? lockedByGuid = null;
            if (Guid.TryParse(lockedBy, out Guid parsedGuid))
                lockedByGuid = parsedGuid;

            return await _periodRepository.UpdateStatusAsync(id, PeriodStatus.Locked, lockedByGuid);
        }

        public PeriodStatusResponseDto GetPeriodStatus(Guid id)
        {
            var period = _periodRepository.GetById(id);
            if (period == null)
                return null;

            // Get journal statistics for the period
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var journals = _journalRepository.GetByDateRange(period.PeriodStartDate, period.PeriodEndDate, null);
            var totalJournals = journals.Count;
            var postedJournals = journals.Count(v => v.Status == JournalStatus.Posted);
            var totalDebit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalDebit);
            var totalCredit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalCredit);

            var canClose = period.Status == PeriodStatus.Open &&
                           totalJournals == postedJournals &&
                           totalDebit == totalCredit;

            string message = "";
            if (period.Status != PeriodStatus.Open)
                message = $"Period status is {period.Status}";
            else if (totalJournals != postedJournals)
                message = $"Unposted journals: {totalJournals - postedJournals}";
            else if (totalDebit != totalCredit)
                message = $"Unbalanced: Debit={totalDebit}, Credit={totalCredit}";
            else
                message = "Ready to close";

            return new PeriodStatusResponseDto
            {
                PeriodId = period.Id,
                Status = period.Status,
                StatusName = GetStatusName(period.Status),
                TotalVouchers = totalJournals,
                PostedVouchers = postedJournals,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Message = message
            };
        }

        public async Task<PeriodStatusResponseDto> GetPeriodStatusAsync(Guid id)
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null)
                return null;

            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var journals = _journalRepository.GetByDateRange(period.PeriodStartDate, period.PeriodEndDate, null);
            var totalJournals = journals.Count;
            var postedJournals = journals.Count(v => v.Status == JournalStatus.Posted);
            var totalDebit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalDebit);
            var totalCredit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalCredit);

            var canClose = period.Status == PeriodStatus.Open &&
                           totalJournals == postedJournals &&
                           totalDebit == totalCredit;

            string message = "";
            if (period.Status != PeriodStatus.Open)
                message = $"Period status is {period.Status}";
            else if (totalJournals != postedJournals)
                message = $"Unposted journals: {totalJournals - postedJournals}";
            else if (totalDebit != totalCredit)
                message = $"Unbalanced: Debit={totalDebit}, Credit={totalCredit}";
            else
                message = "Ready to close";

            return new PeriodStatusResponseDto
            {
                PeriodId = period.Id,
                Status = period.Status,
                StatusName = GetStatusName(period.Status),
                TotalVouchers = totalJournals,
                PostedVouchers = postedJournals,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Message = message
            };
        }

        public (bool CanClose, string Message) ValidateForClosing(Guid id)
        {
            var status = GetPeriodStatus(id);
            if (status == null)
                return (false, "Period not found - الفترة غير موجودة");

            if (status.Status != PeriodStatus.Open)
                return (false, $"Period is not open. Current status: {status.StatusName}");

            if (!status.AllVouchersPosted)
                return (false, $"Cannot close period with unposted vouchers. Unposted: {status.UnpostedVouchers} - لا يمكن إقفال الفترة مع وجود سندات غير مرحّلة");

            if (!status.IsBalanced)
                return (false, $"Period is not balanced. Debit: {status.TotalDebit}, Credit: {status.TotalCredit} - الفترة غير متوازنة");

            return (true, "Ready to close - جاهزة للإقفال");
        }

        public async Task<(bool CanClose, string Message)> ValidateForClosingAsync(Guid id)
        {
            var status = await GetPeriodStatusAsync(id);
            if (status == null)
                return (false, "Period not found - الفترة غير موجودة");

            if (status.Status != PeriodStatus.Open)
                return (false, $"Period is not open. Current status: {status.StatusName}");

            if (!status.AllVouchersPosted)
                return (false, $"Cannot close period with unposted vouchers. Unposted: {status.UnpostedVouchers} - لا يمكن إقفال الفترة مع وجود سندات غير مرحّلة");

            if (!status.IsBalanced)
                return (false, $"Period is not balanced. Debit: {status.TotalDebit}, Credit: {status.TotalCredit} - الفترة غير متوازنة");

            return (true, "Ready to close - جاهزة للإقفال");
        }

        public bool IsDateInClosedPeriod(DateTime date, long? companyId = null)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            return _periodRepository.IsDateInClosedPeriod(date, null);
        }

        /// <summary>
        /// Check if date is in closed or locked period and throw exception if true
        /// This method MUST be called before ANY GL transaction (Journal, AP, AR, etc.)
        /// التحقق من التاريخ في فترة مغلقة أو مقفلة وإلقاء استثناء إذا كان كذلك
        /// </summary>
        /// <param name="date">Transaction date</param>
        /// <param name="companyId">Company ID (optional)</param>
        /// <param name="allowDrafts">Allow saving drafts in closed periods (default: false)</param>
        /// <exception cref="PeriodNotFoundException">When no period exists for the date</exception>
        /// <exception cref="PeriodClosedException">When period is closed</exception>
        /// <exception cref="PeriodLockedException">When period is permanently locked</exception>
        public void CheckIfDateInClosedOrLockedPeriod(DateTime date, long? companyId = null, bool allowDrafts = false)
        {
            // Get the period for the specified date
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var period = _periodRepository.GetPeriodByDate(date, null);

            // Rule 1: Period must exist
            if (period == null)
            {
                throw new PeriodNotFoundException(
                    date,
                    $"لا توجد فترة مالية للتاريخ {date:yyyy-MM-dd}. يرجى إنشاء الفترة أولاً",
                    $"No fiscal period found for date {date:yyyy-MM-dd}. Please create the period first"
                );
            }

            // Rule 2: CANNOT perform ANY operation in Locked period
            if (period.Status == PeriodStatus.Locked)
            {
                throw new PeriodLockedException(
                    date,
                    $"الفترة المالية مقفلة نهائياً للتاريخ {date:yyyy-MM-dd}. لا يمكن تنفيذ أي عمليات",
                    $"Fiscal period is permanently locked for date {date:yyyy-MM-dd}. No operations allowed"
                );
            }

            // Rule 3: CANNOT perform ANY operation in Closed period (even drafts by default)
            if (period.Status == PeriodStatus.Closed)
            {
                if (!allowDrafts)
                {
                    throw new PeriodClosedException(
                        date,
                        $"الفترة المالية مغلقة للتاريخ {date:yyyy-MM-dd}. لا يمكن إنشاء أو تعديل أو حذف القيود",
                        $"Fiscal period is closed for date {date:yyyy-MM-dd}. Cannot create, edit, or delete transactions"
                    );
                }
                else
                {
                    // Even if allowDrafts is true, still validate the period is closed
                    // This is for future use cases where we might allow draft saving in closed periods
                    throw new PeriodClosedException(
                        date,
                        $"الفترة المالية مغلقة للتاريخ {date:yyyy-MM-dd}. لا يمكن تنفيذ العملية",
                        $"Fiscal period is closed for date {date:yyyy-MM-dd}. Cannot perform operation"
                    );
                }
            }

            // Rule 4: Adjusted periods show warning but don't block (for now)
            // This can be made stricter based on business requirements
            if (period.Status == PeriodStatus.Adjusted)
            {
                // Log warning but don't throw - adjusted periods allow operations with caution
                // In future, this could throw a custom AdjustedPeriodWarning exception
            }

            // Period is Open - operation allowed
        }

        /// <summary>
        /// Async version of CheckIfDateInClosedOrLockedPeriod
        /// </summary>
        public async Task CheckIfDateInClosedOrLockedPeriodAsync(DateTime date, long? companyId = null, bool allowDrafts = false)
        {
            // Since repository methods are synchronous, we wrap in Task
            await Task.Run(() => CheckIfDateInClosedOrLockedPeriod(date, companyId, allowDrafts));
        }

        public int Delete(Guid id)
        {
            var period = _periodRepository.GetById(id);
            if (period == null) return 0;

            if (period.Status != PeriodStatus.Open)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_DELETE_ONLY_OPEN, new Dictionary<string, object> { { "PeriodId", id } });

            return _periodRepository.DeletePeriod(id);
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null) return 0;

            if (period.Status != PeriodStatus.Open)
                throw new AppException(ExceptionCode.FINANCE_GL_PERIOD_DELETE_ONLY_OPEN, new Dictionary<string, object> { { "PeriodId", id } });

            return await _periodRepository.DeletePeriodAsync(id);
        }

        public List<PeriodClosingResponseDto> GenerateFiscalYearPeriods(int fiscalYear, PeriodType periodType, string createdBy, long? tenantId = null)
        {
            var periods = new List<PeriodClosingResponseDto>();

            // Use fiscal year helper to get correct start date based on ERP Settings
            var startDate = _fiscalYearHelper.GetFiscalYearStartDate(fiscalYear, tenantId);

            int periodsCount = periodType switch
            {
                PeriodType.Month => 12,
                PeriodType.Quarter => 4,
                PeriodType.Year => 1,
                _ => 12
            };

            for (int i = 0; i < periodsCount; i++)
            {
                DateTime periodStart, periodEnd;

                switch (periodType)
                {
                    case PeriodType.Month:
                        periodStart = startDate.AddMonths(i);
                        periodEnd = periodStart.AddMonths(1).AddDays(-1);
                        break;
                    case PeriodType.Quarter:
                        periodStart = startDate.AddMonths(i * 3);
                        periodEnd = periodStart.AddMonths(3).AddDays(-1);
                        break;
                    case PeriodType.Year:
                        periodStart = startDate;
                        // Use fiscal year helper to get correct end date
                        periodEnd = _fiscalYearHelper.GetFiscalYearEndDate(fiscalYear, tenantId);
                        break;
                    default:
                        periodStart = startDate.AddMonths(i);
                        periodEnd = periodStart.AddMonths(1).AddDays(-1);
                        break;
                }

                var dto = new CreatePeriodClosingDto
                {
                    PeriodType = periodType,
                    PeriodStartDate = periodStart,
                    PeriodEndDate = periodEnd,
                    FiscalYear = fiscalYear,
                    FiscalPeriod = i + 1,
                    DescriptionAr = GetPeriodDescriptionAr(periodType, fiscalYear, i + 1),
                    DescriptionEn = GetPeriodDescriptionEn(periodType, fiscalYear, i + 1),
                    TenantId = tenantId
                };

                try
                {
                    var period = Create(dto, createdBy);
                    periods.Add(period);
                }
                catch
                {
                    // Skip if period already exists
                }
            }

            return periods;
        }

        public async Task<List<PeriodClosingResponseDto>> GenerateFiscalYearPeriodsAsync(int fiscalYear, PeriodType periodType, string createdBy, long? tenantId = null)
        {
            var periods = new List<PeriodClosingResponseDto>();

            // Use fiscal year helper to get correct start date based on ERP Settings
            var startDate = _fiscalYearHelper.GetFiscalYearStartDate(fiscalYear, tenantId);

            int periodsCount = periodType switch
            {
                PeriodType.Month => 12,
                PeriodType.Quarter => 4,
                PeriodType.Year => 1,
                _ => 12
            };

            for (int i = 0; i < periodsCount; i++)
            {
                DateTime periodStart, periodEnd;

                switch (periodType)
                {
                    case PeriodType.Month:
                        periodStart = startDate.AddMonths(i);
                        periodEnd = periodStart.AddMonths(1).AddDays(-1);
                        break;
                    case PeriodType.Quarter:
                        periodStart = startDate.AddMonths(i * 3);
                        periodEnd = periodStart.AddMonths(3).AddDays(-1);
                        break;
                    case PeriodType.Year:
                        periodStart = startDate;
                        // Use fiscal year helper to get correct end date
                        periodEnd = _fiscalYearHelper.GetFiscalYearEndDate(fiscalYear, tenantId);
                        break;
                    default:
                        periodStart = startDate.AddMonths(i);
                        periodEnd = periodStart.AddMonths(1).AddDays(-1);
                        break;
                }

                var dto = new CreatePeriodClosingDto
                {
                    PeriodType = periodType,
                    PeriodStartDate = periodStart,
                    PeriodEndDate = periodEnd,
                    FiscalYear = fiscalYear,
                    FiscalPeriod = i + 1,
                    DescriptionAr = GetPeriodDescriptionAr(periodType, fiscalYear, i + 1),
                    DescriptionEn = GetPeriodDescriptionEn(periodType, fiscalYear, i + 1),
                    TenantId = tenantId
                };

                try
                {
                    var period = await CreateAsync(dto, createdBy);
                    periods.Add(period);
                }
                catch
                {
                    // Skip if period already exists
                }
            }

            return periods;
        }

        #region Private Methods

        private void UpdatePeriodStatistics(GLPeriodClosing period)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var journals = _journalRepository.GetByDateRange(period.PeriodStartDate, period.PeriodEndDate, null);
            period.TotalVouchers = journals.Count;
            period.PostedVouchers = journals.Count(v => v.Status == JournalStatus.Posted);
            period.TotalDebit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalDebit);
            period.TotalCredit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalCredit);
        }

        private async Task UpdatePeriodStatisticsAsync(GLPeriodClosing period)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var journals = _journalRepository.GetByDateRange(period.PeriodStartDate, period.PeriodEndDate, null);
            period.TotalVouchers = journals.Count;
            period.PostedVouchers = journals.Count(v => v.Status == JournalStatus.Posted);
            period.TotalDebit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalDebit);
            period.TotalCredit = journals.Where(v => v.Status == JournalStatus.Posted).Sum(v => v.TotalCredit);
            await Task.CompletedTask;
        }

        private PeriodClosingResponseDto MapToDto(GLPeriodClosing entity)
        {
            return new PeriodClosingResponseDto
            {
                Id = entity.Id,
                PeriodType = entity.PeriodType,
                PeriodStartDate = entity.PeriodStartDate,
                PeriodEndDate = entity.PeriodEndDate,
                FiscalYear = entity.FiscalYear,
                FiscalPeriod = entity.FiscalPeriod,
                Status = entity.Status,
                DescriptionAr = entity.DescriptionAr,
                DescriptionEn = entity.DescriptionEn,
                TotalVouchers = entity.TotalVouchers,
                PostedVouchers = entity.PostedVouchers,
                TotalDebit = entity.TotalDebit,
                TotalCredit = entity.TotalCredit,
                ClosingNotes = entity.ClosingNotes,
                ClosedAt = entity.ClosedAt,
                ClosedBy = entity.ClosedBy,
                ReopenedAt = entity.ReopenedAt,
                ReopenedBy = entity.ReopenedBy,
                TenantId = entity.CompanyID,
                CreatedBy = Guid.TryParse(entity.Create_by, out Guid createdByGuid) ? createdByGuid : (Guid?)null,
                CreatedAt = entity.Create_time
            };
        }

        private static string GetStatusName(PeriodStatus status) => status switch
        {
            PeriodStatus.Open => "Open - مفتوحة",
            PeriodStatus.Closed => "Closed - مغلقة",
            PeriodStatus.Adjusted => "Adjusted - معدّلة",
            PeriodStatus.Locked => "Locked - مقفلة نهائياً",
            _ => status.ToString()
        };

        private static string GetPeriodDescriptionAr(PeriodType type, int year, int period) => type switch
        {
            PeriodType.Month => $"الشهر {period} لسنة {year}",
            PeriodType.Quarter => $"الربع {period} لسنة {year}",
            PeriodType.Year => $"السنة المالية {year}",
            _ => $"الفترة {period} لسنة {year}"
        };

        private static string GetPeriodDescriptionEn(PeriodType type, int year, int period) => type switch
        {
            PeriodType.Month => $"Month {period} of {year}",
            PeriodType.Quarter => $"Quarter {period} of {year}",
            PeriodType.Year => $"Fiscal Year {year}",
            _ => $"Period {period} of {year}"
        };

        #endregion
    }
}

