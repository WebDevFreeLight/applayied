using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Localization;
using ZR.Model.Finance;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance;
using ZR.Repository.Finance.GL;

namespace ZR.Service.Finance.Validation
{
    /// <summary>
    /// Journal Validation Service - خدمة التحقق من صحة القيود
    /// Comprehensive validation for all accounting rules
    /// </summary>
    [AppService(ServiceType = typeof(IJournalValidationService), ServiceLifetime = LifeTime.Transient)]
    public class JournalValidationService : IJournalValidationService
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLPeriodClosingRepository _periodRepository;
        private readonly IGLJournalRepository _journalRepository;
        private readonly BilingualMessageHelper _messageHelper;

        // Configuration constants - should be moved to system settings
        private const decimal BALANCE_TOLERANCE = 0.0001m; // Tolerance for floating point comparison
        private const int MINIMUM_ENTRY_LINES = 2;

        public JournalValidationService(
            IGLAccountRepository accountRepository,
            IGLPeriodClosingRepository periodRepository,
            IGLJournalRepository journalRepository,
            BilingualMessageHelper messageHelper)
        {
            _accountRepository = accountRepository;
            _periodRepository = periodRepository;
            _journalRepository = journalRepository;
            _messageHelper = messageHelper;
        }

        #region Main Validation Methods

        /// <summary>
        /// Validate journal for creation - التحقق من القيد للإنشاء
        /// </summary>
        public JournalValidationResultDto ValidateForCreate(CreateJournalDto dto, long? companyId = null)
        {
            var result = new JournalValidationResultDto(_messageHelper) { IsValid = true, Errors = new List<string>() };

            // Determine if this is a system-generated journal (ARAP, Asset, etc.)
            // System journals are allowed to post to control accounts and accounts that block manual entries
            bool isSystemGenerated = !string.IsNullOrEmpty(dto.Source) &&
                                     dto.Source != "Manual";

            // 1. Validate Header Rules
            ValidateHeaderRules(dto.JournalDate, dto.DescriptionAr, dto.DescriptionEn, dto.Currency, result);

            // 2. Validate Entries exist
            if (dto.Entries == null || dto.Entries.Count < MINIMUM_ENTRY_LINES)
            {
                result.AddError($"يجب أن يحتوي القيد على {MINIMUM_ENTRY_LINES} أسطر على الأقل - Journal must have at least {MINIMUM_ENTRY_LINES} lines");
            }

            if (dto.Entries != null && dto.Entries.Any())
            {
                // 3. Validate Debit/Credit Rules
                ValidateDebitCreditRules(dto.Entries.Cast<object>().ToList(), result);

                // 4. Validate Each Entry Line
                foreach (var entry in dto.Entries)
                {
                    ValidateEntryLine(entry.AccountId, entry.Debit, entry.Credit,
                        entry.CostCenterId, entry.DepartmentId, entry.ProjectId,
                        entry.Currency, companyId, result, isSystemGenerated);
                }
            }

            // 5. Validate Period Rules (for journal date)
            ValidatePeriodRules(dto.JournalDate, companyId, result, isPosting: false);

            return result;
        }

        /// <summary>
        /// Validate journal for update - التحقق من القيد للتحديث
        /// </summary>
        public JournalValidationResultDto ValidateForUpdate(UpdateJournalDto dto, long? companyId = null)
        {
            var result = new JournalValidationResultDto(_messageHelper) { IsValid = true, Errors = new List<string>() };

            // Check if journal exists and is not posted
            var existingJournal = _journalRepository.GetById(dto.Id);
            if (existingJournal == null)
            {
                result.AddError("القيد غير موجود - Journal not found");
                return result;
            }

            if (existingJournal.Status == JournalStatus.Posted)
            {
                result.AddError("لا يمكن تعديل قيد مرحّل - Cannot update a posted journal");
                return result;
            }

            if (existingJournal.Status == JournalStatus.Cancelled)
            {
                result.AddError("لا يمكن تعديل قيد ملغى - Cannot update a cancelled journal");
                return result;
            }

            // 1. Validate Header Rules
            ValidateHeaderRules(dto.JournalDate, dto.DescriptionAr, dto.DescriptionEn, dto.Currency, result);

            if (dto.Entries != null && dto.Entries.Any())
            {
                // 2. Validate Entries count
                if (dto.Entries.Count < MINIMUM_ENTRY_LINES)
                {
                    result.AddError($"يجب أن يحتوي القيد على {MINIMUM_ENTRY_LINES} أسطر على الأقل - Journal must have at least {MINIMUM_ENTRY_LINES} lines");
                }

                // 3. Validate Debit/Credit Rules
                ValidateDebitCreditRules(dto.Entries.Cast<object>().ToList(), result);

                // 4. Validate Each Entry Line
                foreach (var entry in dto.Entries)
                {
                    ValidateEntryLine(entry.AccountId, entry.Debit, entry.Credit,
                        entry.CostCenterId, entry.DepartmentId, entry.ProjectId,
                        entry.Currency, companyId, result);
                }
            }

            // 5. Validate Period Rules
            ValidatePeriodRules(dto.JournalDate, companyId, result, isPosting: false);

            return result;
        }

        /// <summary>
        /// Validate journal for posting - التحقق من القيد للترحيل
        /// </summary>
        public JournalValidationResultDto ValidateForPost(Guid journalId, long? companyId = null)
        {
            var result = new JournalValidationResultDto(_messageHelper) { IsValid = true, Errors = new List<string>() };

            // Get journal with entries
            var journal = _journalRepository.GetById(journalId);
            if (journal == null)
            {
                result.AddError("القيد غير موجود - Journal not found");
                return result;
            }

            // Determine if this is a system-generated journal
            bool isSystemGenerated = !string.IsNullOrEmpty(journal.Source) &&
                                     journal.Source != "Manual";

            // 1. Check journal status
            if (journal.Status != JournalStatus.Draft && journal.Status != JournalStatus.Approved)
            {
                result.AddError($"لا يمكن ترحيل قيد بحالة {journal.Status} - Cannot post journal with status {journal.Status}");
                return result;
            }

            // 2. Validate Header Rules
            ValidateHeaderRules(journal.JournalDate, journal.DescriptionAr, journal.DescriptionEn, journal.Currency, result);

            // 3. Get entries and validate
            var entries = _journalRepository.GetJournalEntries(journalId);
            if (entries == null || entries.Count < MINIMUM_ENTRY_LINES)
            {
                result.AddError($"يجب أن يحتوي القيد على {MINIMUM_ENTRY_LINES} أسطر على الأقل - Journal must have at least {MINIMUM_ENTRY_LINES} lines");
            }
            else
            {
                // Calculate totals
                var totalDebit = entries.Sum(e => e.Debit);
                var totalCredit = entries.Sum(e => e.Credit);
                result.TotalDebit = totalDebit;
                result.TotalCredit = totalCredit;
                result.Difference = Math.Abs(totalDebit - totalCredit);

                // 4. Validate balance
                if (Math.Abs(totalDebit - totalCredit) > BALANCE_TOLERANCE)
                {
                    result.AddError($"القيد غير متوازن - Journal is not balanced. Debit: {totalDebit}, Credit: {totalCredit}");
                }

                // 5. Validate at least one debit and one credit
                if (!entries.Any(e => e.Debit > 0))
                {
                    result.AddError("يجب أن يحتوي القيد على سطر مدين واحد على الأقل - Journal must have at least one debit line");
                }
                if (!entries.Any(e => e.Credit > 0))
                {
                    result.AddError("يجب أن يحتوي القيد على سطر دائن واحد على الأقل - Journal must have at least one credit line");
                }

                // 6. Validate each line for posting
                foreach (var entry in entries)
                {
                    ValidateEntryLineForPosting(entry, companyId, result, isSystemGenerated);
                }
            }

            // 7. Validate Period Rules (strict for posting)
            ValidatePeriodRules(journal.JournalDate, companyId, result, isPosting: true);

            // 8. Validate journal number exists
            if (string.IsNullOrEmpty(journal.JournalNumber))
            {
                result.AddError("رقم القيد مطلوب للترحيل - Journal number is required for posting");
            }

            return result;
        }

        /// <summary>
        /// Validate for reversal - التحقق من القيد للعكس
        /// </summary>
        public JournalValidationResultDto ValidateForReversal(Guid journalId, DateTime? reversalDate, long? companyId = null)
        {
            var result = new JournalValidationResultDto(_messageHelper) { IsValid = true, Errors = new List<string>() };

            var journal = _journalRepository.GetById(journalId);
            if (journal == null)
            {
                result.AddError("القيد غير موجود - Journal not found");
                return result;
            }

            // 1. Only posted journals can be reversed
            if (journal.Status != JournalStatus.Posted)
            {
                result.AddError("يمكن عكس القيود المرحّلة فقط - Only posted journals can be reversed");
                return result;
            }

            // 2. Check if already reversed
            if (journal.ReversalJournalId.HasValue)
            {
                result.AddError("القيد معكوس بالفعل - Journal is already reversed");
                return result;
            }

            // 3. Validate reversal date
            var effectiveReversalDate = reversalDate ?? DateTime.Now.Date;

            // Reversal date cannot be before original journal date
            if (effectiveReversalDate < journal.JournalDate)
            {
                result.AddError("تاريخ العكس لا يمكن أن يكون قبل تاريخ القيد الأصلي - Reversal date cannot be before original journal date");
            }

            // 4. Validate period for reversal date
            ValidatePeriodRules(effectiveReversalDate, companyId, result, isPosting: true);

            return result;
        }

        #endregion

        #region Private Validation Methods

        /// <summary>
        /// Validate header rules - التحقق من قواعد الرأس
        /// </summary>
        private void ValidateHeaderRules(DateTime journalDate, string descriptionAr, string descriptionEn, string currency, JournalValidationResultDto result)
        {
            // Date not in future
            if (journalDate.Date > DateTime.Now.Date)
            {
                result.AddError("تاريخ القيد لا يمكن أن يكون في المستقبل - Journal date cannot be in the future");
            }

            // Currency required
            if (string.IsNullOrWhiteSpace(currency))
            {
                result.AddError("العملة مطلوبة - Currency is required");
            }

            // At least one description (configurable - can be made stricter)
            // Note: In strict mode, both AR and EN descriptions should be required
        }

        /// <summary>
        /// Validate debit/credit rules - التحقق من قواعد المدين/الدائن
        /// </summary>
        private void ValidateDebitCreditRules(List<object> entries, JournalValidationResultDto result)
        {
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            bool hasDebit = false;
            bool hasCredit = false;
            int lineNumber = 0;

            foreach (var entry in entries)
            {
                lineNumber++;
                decimal debit, credit;

                // Handle both CreateJournalEntryDto and UpdateJournalEntryDto
                if (entry is CreateJournalEntryDto createDto)
                {
                    debit = createDto.Debit;
                    credit = createDto.Credit;
                }
                else if (entry is UpdateJournalEntryDto updateDto)
                {
                    debit = updateDto.Debit;
                    credit = updateDto.Credit;
                }
                else
                {
                    continue;
                }

                // Rule: No negative amounts
                if (debit < 0)
                {
                    result.AddError($"السطر {lineNumber}: المبلغ المدين لا يمكن أن يكون سالب - Line {lineNumber}: Debit cannot be negative");
                }
                if (credit < 0)
                {
                    result.AddError($"السطر {lineNumber}: المبلغ الدائن لا يمكن أن يكون سالب - Line {lineNumber}: Credit cannot be negative");
                }

                // Rule: Cannot have both debit and credit on same line
                if (debit > 0 && credit > 0)
                {
                    result.AddError($"السطر {lineNumber}: لا يمكن أن يحتوي السطر على مدين ودائن في نفس الوقت - Line {lineNumber}: Cannot have both debit and credit");
                }

                // Rule: Line must have either debit or credit
                if (debit == 0 && credit == 0)
                {
                    result.AddError($"السطر {lineNumber}: يجب أن يحتوي السطر على مبلغ مدين أو دائن - Line {lineNumber}: Must have either debit or credit amount");
                }

                totalDebit += debit;
                totalCredit += credit;
                if (debit > 0) hasDebit = true;
                if (credit > 0) hasCredit = true;
            }

            // Rule: Must have at least one debit line
            if (!hasDebit)
            {
                result.AddError("يجب أن يحتوي القيد على سطر مدين واحد على الأقل - Journal must have at least one debit line");
            }

            // Rule: Must have at least one credit line
            if (!hasCredit)
            {
                result.AddError("يجب أن يحتوي القيد على سطر دائن واحد على الأقل - Journal must have at least one credit line");
            }

            // Rule: Debit must equal Credit (with tolerance)
            if (Math.Abs(totalDebit - totalCredit) > BALANCE_TOLERANCE)
            {
                result.AddError($"القيد غير متوازن. إجمالي المدين: {totalDebit}, إجمالي الدائن: {totalCredit} - Journal is not balanced. Total Debit: {totalDebit}, Total Credit: {totalCredit}");
            }

            result.TotalDebit = totalDebit;
            result.TotalCredit = totalCredit;
            result.Difference = Math.Abs(totalDebit - totalCredit);
        }

        /// <summary>
        /// Validate single entry line - التحقق من سطر القيد
        /// </summary>
        private void ValidateEntryLine(Guid accountId, decimal debit, decimal credit,
            Guid? costCenterId, Guid? departmentId, Guid? projectId,
            string currency, long? companyId, JournalValidationResultDto result, bool isSystemGenerated = false)
        {
            // Get account
            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                result.AddError($"الحساب غير موجود: {accountId} - Account not found: {accountId}");
                return;
            }

            // Rule: Account must be active
            if (!account.IsActive)
            {
                result.AddError($"الحساب {account.FullAccountNumber} غير نشط - Account {account.FullAccountNumber} is not active");
            }

            // Rule: Must be a leaf account (unless AllowPostingToParent is true)
            if (!account.IsLeaf && !account.AllowPostingToParent)
            {
                result.AddError($"لا يمكن الترحيل للحساب الرئيسي {account.FullAccountNumber}. يجب استخدام حساب فرعي - Cannot post to parent account {account.FullAccountNumber}. Use a leaf account");
            }

            // Skip manual-entry and control-account checks for system-generated journals (ARAP, Asset, etc.)
            // These modules ARE the designated posting sources for control accounts
            if (!isSystemGenerated)
            {
                // Rule: Account must allow manual entries (for manual journals)
                if (!account.AllowManualEntries)
                {
                    result.AddError($"الحساب {account.FullAccountNumber} لا يسمح بإدخال قيود يدوية - Account {account.FullAccountNumber} does not allow manual entries");
                }

                // Rule: Control accounts MUST NOT accept manual postings
                if (account.IsControlAccount)
                {
                    result.AddError(
                        $"الحساب {account.FullAccountNumber} هو حساب مراقبة ولا يقبل قيود يدوية. " +
                        $"يجب استخدام الوحدات المخصصة (الذمم المدينة/الدائنة/المخزون) للترحيل لهذا الحساب. - " +
                        $"Account {account.FullAccountNumber} is a control account and does not accept manual entries. " +
                        $"Use designated modules (AR/AP/Inventory) to post to this account.");
                }

                // Rule: System accounts MUST NOT accept manual postings
                if (account.IsSystemAccount)
                {
                    result.AddError(
                        $"الحساب {account.FullAccountNumber} هو حساب نظام محجوز ولا يقبل قيود يدوية. " +
                        $"حسابات النظام مخصصة لعمليات النظام الداخلية فقط (مثل إقفال السنة المالية). - " +
                        $"Account {account.FullAccountNumber} is a reserved system account and does not accept manual entries. " +
                        $"System accounts are designated for internal system operations only (e.g., year-end closing).");
                }
            }

            // Rule: Validate tenant/company
            if (companyId.HasValue && account.CompanyID.HasValue && account.CompanyID != companyId)
            {
                result.AddError($"الحساب {account.FullAccountNumber} لا ينتمي للشركة الحالية - Account {account.FullAccountNumber} does not belong to current company");
            }

            // Rule: Currency compatibility
            if (!string.IsNullOrEmpty(currency) && account.IsForeignCurrency && account.Currency != currency)
            {
                result.AddWarning($"عملة القيد ({currency}) تختلف عن عملة الحساب ({account.Currency}) - Entry currency ({currency}) differs from account currency ({account.Currency})");
            }

            // Rule: Required dimensions
            if (account.RequiresCostCenter && !costCenterId.HasValue)
            {
                result.AddError($"الحساب {account.FullAccountNumber} يتطلب مركز تكلفة - Account {account.FullAccountNumber} requires cost center");
            }

            if (account.RequiresDepartment && !departmentId.HasValue)
            {
                result.AddError($"الحساب {account.FullAccountNumber} يتطلب قسم - Account {account.FullAccountNumber} requires department");
            }

            if (account.RequiresProject && !projectId.HasValue)
            {
                result.AddError($"الحساب {account.FullAccountNumber} يتطلب مشروع - Account {account.FullAccountNumber} requires project");
            }
        }

        /// <summary>
        /// Validate entry line for posting - التحقق من سطر القيد للترحيل
        /// </summary>
        private void ValidateEntryLineForPosting(GLJournalEntry entry, long? companyId, JournalValidationResultDto result, bool isSystemGenerated = false)
        {
            ValidateEntryLine(entry.AccountId, entry.Debit, entry.Credit,
                entry.CostCenterId, entry.DepartmentId, entry.ProjectId,
                entry.Currency, companyId, result, isSystemGenerated);
        }

        /// <summary>
        /// Validate period rules - التحقق من قواعد الفترة
        /// </summary>
        private void ValidatePeriodRules(DateTime journalDate, long? companyId, JournalValidationResultDto result, bool isPosting)
        {
            // Rule: Date not in future
            if (journalDate.Date > DateTime.Now.Date)
            {
                result.AddError("تاريخ القيد لا يمكن أن يكون في المستقبل - Journal date cannot be in the future");
                return;
            }

            // Check if date falls in a period
            var period = _periodRepository.GetPeriodByDate(journalDate, companyId);

            if (period == null)
            {
                if (isPosting)
                {
                    result.AddError($"لا توجد فترة مالية مفتوحة للتاريخ {journalDate:yyyy-MM-dd} - No fiscal period found for date {journalDate:yyyy-MM-dd}");
                }
                else
                {
                    result.AddWarning($"لا توجد فترة مالية للتاريخ {journalDate:yyyy-MM-dd} - No fiscal period found for date {journalDate:yyyy-MM-dd}");
                }
                return;
            }

            // Rule: Period must be Open for posting
            if (isPosting)
            {
                if (period.Status == PeriodStatus.Closed)
                {
                    result.AddError($"الفترة المالية مغلقة ولا يمكن الترحيل إليها - Fiscal period is closed. Cannot post to closed period");
                }
                else if (period.Status == PeriodStatus.Locked)
                {
                    result.AddError($"الفترة المالية مقفلة نهائياً - Fiscal period is permanently locked");
                }
                else if (period.Status == PeriodStatus.Adjusted)
                {
                    result.AddWarning($"الفترة المالية في وضع التعديل - Fiscal period is in adjustment mode");
                }
            }
            else
            {
                // For create/update, warn about closed periods
                if (period.Status != PeriodStatus.Open)
                {
                    result.AddWarning($"الفترة المالية بحالة {period.Status}. قد لا يمكن ترحيل هذا القيد - Fiscal period status is {period.Status}. This journal may not be postable");
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Journal Validation Service Interface
    /// </summary>
    public interface IJournalValidationService
    {
        JournalValidationResultDto ValidateForCreate(CreateJournalDto dto, long? companyId = null);
        JournalValidationResultDto ValidateForUpdate(UpdateJournalDto dto, long? companyId = null);
        JournalValidationResultDto ValidateForPost(Guid journalId, long? companyId = null);
        JournalValidationResultDto ValidateForReversal(Guid journalId, DateTime? reversalDate, long? companyId = null);
    }

    /// <summary>
    /// Enhanced Journal Validation Result DTO
    /// </summary>
    public class JournalValidationResultDto
    {
        private readonly BilingualMessageHelper _messageHelper;

        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Difference { get; set; }
        public bool IsBalanced => Math.Abs(TotalDebit - TotalCredit) < 0.0001m;

        public JournalValidationResultDto(BilingualMessageHelper messageHelper)
        {
            _messageHelper = messageHelper;
        }

        public void AddError(string bilingualError)
        {
            // Extract language-specific message
            var localizedError = _messageHelper.GetLocalizedMessage(bilingualError);
            Errors.Add(localizedError);
            IsValid = false;
        }

        public void AddWarning(string bilingualWarning)
        {
            // Extract language-specific message
            var localizedWarning = _messageHelper.GetLocalizedMessage(bilingualWarning);
            Warnings.Add(localizedWarning);
        }

        public string GetSummary()
        {
            if (IsValid)
                return Warnings.Any() ? $"Valid with {Warnings.Count} warning(s)" : "Valid";
            return $"Invalid: {string.Join("; ", Errors)}";
        }
    }
}
