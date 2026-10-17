using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;

namespace ZR.Service.Tests.ERP.Finance.TestHelpers
{
    /// <summary>
    /// Test-specific GL Account Service - خدمة الحسابات للاختبارات
    /// Updated for IFRS-compliant model with FullAccountNumber, AccountPath, enums
    /// Bypasses repository layer to work directly with ISqlSugarClient
    /// </summary>
    public class TestGLAccountService
    {
        private readonly ISqlSugarClient _db;

        public TestGLAccountService(ISqlSugarClient db)
        {
            _db = db;
        }

        public GLAccount Create(CreateAccountDto dto, string userId)
        {
            // Use enum directly from DTO
            var accountType = dto.AccountType;

            // Determine normal balance based on account type
            var normalBalance = (accountType == AccountType.Asset || accountType == AccountType.Expense)
                ? NormalBalance.Debit
                : NormalBalance.Credit;

            // Override with DTO value
            normalBalance = dto.NormalBalance;

            // Build FullAccountNumber based on parent
            string fullAccountNumber = dto.AccountNumber;
            string accountPath = "";
            int level = dto.Level;

            if (dto.ParentAccountId.HasValue)
            {
                var parent = _db.Queryable<GLAccount>().First(a => a.AccountId == dto.ParentAccountId.Value);
                if (parent != null)
                {
                    fullAccountNumber = $"{parent.FullAccountNumber}.{dto.AccountNumber}";
                    accountPath = string.IsNullOrEmpty(parent.AccountPath)
                        ? parent.AccountId.ToString()
                        : parent.AccountPath;
                    level = parent.Level + 1;

                    // Update parent's IsLeaf and ChildCount
                    parent.IsLeaf = false;
                    parent.ChildCount++;
                    _db.Updateable(parent).ExecuteCommand();

                    // Update ancestor DescendantCount
                    UpdateAncestorDescendantCounts(parent.AccountId, 1);
                }
            }

            var account = new GLAccount
            {
                AccountId = Guid.NewGuid(),
                AccountNumber = dto.AccountNumber,
                FullAccountNumber = fullAccountNumber,
                AccountNameAr = dto.AccountNameAr,
                AccountNameEn = dto.AccountNameEn,
                AccountType = accountType,
                NormalBalance = normalBalance,
                Level = level,
                ParentAccountId = dto.ParentAccountId,
                IsActive = dto.IsActive,
                IsLeaf = dto.IsLeaf,
                IsSystemAccount = false,
                AllowManualEntries = dto.AllowManualEntries,
                AllowPostingToParent = false,
                RequiresCostCenter = dto.RequiresCostCenter,
                RequiresDepartment = false,
                RequiresProject = false,
                IsCashAccount = false,
                IsBankAccount = false,
                IsControlAccount = false,
                IsReconciliationAccount = false,
                Currency = dto.Currency ?? "SAR",
                IsForeignCurrency = false,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                DisplayOrder = 0,
                ChildCount = 0,
                DescendantCount = 0,
                RowVersion = 1,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            // Set AccountPath after AccountId is generated
            account.AccountPath = string.IsNullOrEmpty(accountPath)
                ? account.AccountId.ToString()
                : $"{accountPath}/{account.AccountId}";

            _db.Insertable(account).ExecuteCommand();
            return account;
        }

        private void UpdateAncestorDescendantCounts(Guid accountId, int delta)
        {
            var currentId = accountId;
            while (true)
            {
                var account = _db.Queryable<GLAccount>().First(a => a.AccountId == currentId);
                if (account == null) break;

                account.DescendantCount += delta;
                _db.Updateable(account).UpdateColumns(a => a.DescendantCount).ExecuteCommand();

                if (!account.ParentAccountId.HasValue) break;
                currentId = account.ParentAccountId.Value;
            }
        }

        public GLAccount GetById(Guid id)
        {
            return _db.Queryable<GLAccount>().First(a => a.AccountId == id);
        }

        public int Update(UpdateAccountDto dto, string userId)
        {
            var account = _db.Queryable<GLAccount>().First(a => a.AccountId == dto.AccountId);
            if (account == null) return 0;

            // Check for system account protection
            if (account.IsSystemAccount)
            {
                throw new Exception("Cannot modify system account - لا يمكن تعديل حساب النظام");
            }

            // Use enum values directly from DTO
            account.AccountNameAr = dto.AccountNameAr;
            account.AccountNameEn = dto.AccountNameEn;
            account.AccountNumber = dto.AccountNumber;
            account.AccountType = dto.AccountType;
            account.NormalBalance = dto.NormalBalance;
            account.IsActive = dto.IsActive;
            account.IsLeaf = dto.IsLeaf;
            account.AllowManualEntries = dto.AllowManualEntries;
            account.RequiresCostCenter = dto.RequiresCostCenter;
            account.ParentAccountId = dto.ParentAccountId;
            account.RowVersion++;
            account.Update_by = userId;
            account.Update_time = DateTime.Now;

            // Rebuild FullAccountNumber if AccountNumber or Parent changed
            if (dto.ParentAccountId.HasValue)
            {
                var parent = _db.Queryable<GLAccount>().First(a => a.AccountId == dto.ParentAccountId.Value);
                if (parent != null)
                {
                    account.FullAccountNumber = $"{parent.FullAccountNumber}.{dto.AccountNumber}";
                }
            }
            else
            {
                account.FullAccountNumber = dto.AccountNumber;
            }

            return _db.Updateable(account).ExecuteCommand();
        }

        public int Delete(Guid[] ids)
        {
            // Check if any account has children
            foreach (var id in ids)
            {
                var hasChildren = _db.Queryable<GLAccount>().Any(a => a.ParentAccountId == id);
                if (hasChildren)
                    return 0;
            }

            // Soft delete
            return _db.Updateable<GLAccount>()
                .SetColumns(a => a.IsActive == false)
                .Where(a => ids.Contains(a.AccountId))
                .ExecuteCommand();
        }

        public decimal GetAccountBalance(Guid accountId)
        {
            var account = _db.Queryable<GLAccount>().First(a => a.AccountId == accountId);
            if (account == null) return 0;

            // Sum of posted entries - use base amounts for multi-currency support
            var debitSum = _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .Where((e, v) => e.AccountId == accountId && v.Status == JournalStatus.Posted)
                .Sum((e, v) => e.BaseDebit);

            var creditSum = _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .Where((e, v) => e.AccountId == accountId && v.Status == JournalStatus.Posted)
                .Sum((e, v) => e.BaseCredit);

            // Use the account's CalculateBalance method for proper normal balance handling
            return account.CalculateBalance(debitSum, creditSum);
        }

        public bool CodeExists(string accountNumber, Guid? excludeAccountId = null)
        {
            var query = _db.Queryable<GLAccount>().Where(a => a.AccountNumber == accountNumber);
            if (excludeAccountId.HasValue)
                query = query.Where(a => a.AccountId != excludeAccountId.Value);
            return query.Any();
        }

        public bool FullAccountNumberExists(string fullAccountNumber, long? companyId = null, Guid? excludeAccountId = null)
        {
            var query = _db.Queryable<GLAccount>().Where(a => a.FullAccountNumber == fullAccountNumber);
            if (companyId.HasValue)
                query = query.Where(a => a.CompanyID == companyId.Value);
            if (excludeAccountId.HasValue)
                query = query.Where(a => a.AccountId != excludeAccountId.Value);
            return query.Any();
        }

        public bool AccountNumberExistsUnderParent(string accountNumber, Guid? parentAccountId, long? companyId = null, Guid? excludeAccountId = null)
        {
            var query = _db.Queryable<GLAccount>().Where(a => a.AccountNumber == accountNumber);
            if (parentAccountId.HasValue)
                query = query.Where(a => a.ParentAccountId == parentAccountId.Value);
            else
                query = query.Where(a => a.ParentAccountId == null);
            if (companyId.HasValue)
                query = query.Where(a => a.CompanyID == companyId.Value);
            if (excludeAccountId.HasValue)
                query = query.Where(a => a.AccountId != excludeAccountId.Value);
            return query.Any();
        }

        public bool HasTransactions(Guid accountId)
        {
            return _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .Where((e, v) => e.AccountId == accountId && v.Status == JournalStatus.Posted)
                .Any();
        }

        public bool HasChildren(Guid accountId)
        {
            return _db.Queryable<GLAccount>().Any(a => a.ParentAccountId == accountId);
        }

        public List<GLAccount> GetAccountTree()
        {
            return _db.Queryable<GLAccount>().Where(a => a.IsActive).ToList();
        }

        public List<GLAccount> GetByType(AccountType accountType)
        {
            return _db.Queryable<GLAccount>()
                .Where(a => a.AccountType == accountType && a.IsActive)
                .ToList();
        }

        /// <summary>
        /// Get accounts by type using string (for backward compatibility with older tests)
        /// </summary>
        public List<GLAccount> GetByType(string accountTypeString)
        {
            if (Enum.TryParse<AccountType>(accountTypeString, true, out var accountType))
            {
                return GetByType(accountType);
            }
            return new List<GLAccount>();
        }

        // Async Methods
        public async Task<GLAccount> CreateAsync(CreateAccountDto dto, string userId)
        {
            // Build FullAccountNumber based on parent
            string fullAccountNumber = dto.AccountNumber;
            string accountPath = "";
            int level = dto.Level;

            if (dto.ParentAccountId.HasValue)
            {
                var parent = await _db.Queryable<GLAccount>().FirstAsync(a => a.AccountId == dto.ParentAccountId.Value);
                if (parent != null)
                {
                    fullAccountNumber = $"{parent.FullAccountNumber}.{dto.AccountNumber}";
                    accountPath = string.IsNullOrEmpty(parent.AccountPath)
                        ? parent.AccountId.ToString()
                        : parent.AccountPath;
                    level = parent.Level + 1;

                    // Update parent's IsLeaf and ChildCount
                    parent.IsLeaf = false;
                    parent.ChildCount++;
                    await _db.Updateable(parent).ExecuteCommandAsync();
                }
            }

            var account = new GLAccount
            {
                AccountId = Guid.NewGuid(),
                AccountNumber = dto.AccountNumber,
                FullAccountNumber = fullAccountNumber,
                AccountNameAr = dto.AccountNameAr,
                AccountNameEn = dto.AccountNameEn,
                AccountType = dto.AccountType,
                NormalBalance = dto.NormalBalance,
                Level = level,
                ParentAccountId = dto.ParentAccountId,
                IsActive = dto.IsActive,
                IsLeaf = dto.IsLeaf,
                IsSystemAccount = false,
                AllowManualEntries = dto.AllowManualEntries,
                AllowPostingToParent = dto.AllowPostingToParent,
                RequiresCostCenter = dto.RequiresCostCenter,
                RequiresDepartment = dto.RequiresDepartment,
                RequiresProject = dto.RequiresProject,
                IsCashAccount = dto.IsCashAccount,
                IsBankAccount = dto.IsBankAccount,
                IsControlAccount = dto.IsControlAccount,
                IsReconciliationAccount = dto.IsReconciliationAccount,
                Currency = dto.Currency ?? "SAR",
                IsForeignCurrency = dto.IsForeignCurrency,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                DisplayOrder = dto.DisplayOrder,
                ChildCount = 0,
                DescendantCount = 0,
                RowVersion = 1,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            // Set AccountPath after AccountId is generated
            account.AccountPath = string.IsNullOrEmpty(accountPath)
                ? account.AccountId.ToString()
                : $"{accountPath}/{account.AccountId}";

            await _db.Insertable(account).ExecuteCommandAsync();
            return account;
        }

        public async Task<GLAccount> GetByIdAsync(Guid id)
        {
            return await _db.Queryable<GLAccount>().FirstAsync(a => a.AccountId == id);
        }

        public async Task<int> UpdateAsync(UpdateAccountDto dto, string userId)
        {
            var account = await _db.Queryable<GLAccount>().FirstAsync(a => a.AccountId == dto.AccountId);
            if (account == null) return 0;

            // Check for system account protection
            if (account.IsSystemAccount)
            {
                throw new Exception("Cannot modify system account - لا يمكن تعديل حساب النظام");
            }

            account.AccountNameAr = dto.AccountNameAr;
            account.AccountNameEn = dto.AccountNameEn;
            account.AccountNumber = dto.AccountNumber;
            account.AccountType = dto.AccountType;
            account.NormalBalance = dto.NormalBalance;
            account.IsActive = dto.IsActive;
            account.IsLeaf = dto.IsLeaf;
            account.AllowManualEntries = dto.AllowManualEntries;
            account.AllowPostingToParent = dto.AllowPostingToParent;
            account.RequiresCostCenter = dto.RequiresCostCenter;
            account.RequiresDepartment = dto.RequiresDepartment;
            account.RequiresProject = dto.RequiresProject;
            account.ParentAccountId = dto.ParentAccountId;
            account.RowVersion++;
            account.Update_by = userId;
            account.Update_time = DateTime.Now;

            // Rebuild FullAccountNumber if AccountNumber or Parent changed
            if (dto.ParentAccountId.HasValue)
            {
                var parent = await _db.Queryable<GLAccount>().FirstAsync(a => a.AccountId == dto.ParentAccountId.Value);
                if (parent != null)
                {
                    account.FullAccountNumber = $"{parent.FullAccountNumber}.{dto.AccountNumber}";
                }
            }
            else
            {
                account.FullAccountNumber = dto.AccountNumber;
            }

            return await _db.Updateable(account).ExecuteCommandAsync();
        }

        public async Task<decimal> GetAccountBalanceAsync(Guid accountId)
        {
            var account = await _db.Queryable<GLAccount>().FirstAsync(a => a.AccountId == accountId);
            if (account == null) return 0;

            var debitSum = await _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .Where((e, v) => e.AccountId == accountId && v.Status == JournalStatus.Posted)
                .SumAsync((e, v) => e.BaseDebit);

            var creditSum = await _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .Where((e, v) => e.AccountId == accountId && v.Status == JournalStatus.Posted)
                .SumAsync((e, v) => e.BaseCredit);

            // Use the account's CalculateBalance method for proper normal balance handling
            return account.CalculateBalance(debitSum, creditSum);
        }
    }

    /// <summary>
    /// Test-specific GL Journal Service - خدمة القيود للاختبارات
    /// Renamed from TestGLVoucherService to TestGLJournalService
    /// </summary>
    public class TestGLJournalService
    {
        private readonly ISqlSugarClient _db;

        public TestGLJournalService(ISqlSugarClient db)
        {
            _db = db;
        }

        public GLJournal Create(CreateJournalDto dto, string userId)
        {
            // Convert TenantId (Guid) to CompanyID (long) - use CompanyId if provided, otherwise derive from TenantId
            var companyId = dto.CompanyId ?? (dto.TenantId != Guid.Empty ? (long?)dto.TenantId.GetHashCode() : null);

            var journal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = GenerateJournalNumber(),
                JournalDate = dto.JournalDate,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                Status = JournalStatus.Draft,
                Currency = dto.Currency ?? "SAR",
                FiscalYear = dto.FiscalYear,
                FiscalPeriod = dto.FiscalPeriod,
                Remark = $"Test journal: {dto.DescriptionEn}",
                CompanyID = companyId,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            _db.Insertable(journal).ExecuteCommand();

            // Create entries
            decimal totalDebit = 0, totalCredit = 0;
            int lineNumber = 1;
            foreach (var entryDto in dto.Entries ?? new List<CreateJournalEntryDto>())
            {
                var exchangeRate = entryDto.ExchangeRate > 0 ? entryDto.ExchangeRate : 1.0m;
                var baseDebit = entryDto.Debit * exchangeRate;
                var baseCredit = entryDto.Credit * exchangeRate;

                var entry = new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = entryDto.AccountId,
                    LineNumber = lineNumber++,
                    Debit = entryDto.Debit,
                    Credit = entryDto.Credit,
                    DescriptionAr = entryDto.DescriptionAr,
                    DescriptionEn = entryDto.DescriptionEn,
                    Currency = dto.Currency ?? "SAR",
                    ExchangeRate = exchangeRate,
                    BaseDebit = baseDebit,
                    BaseCredit = baseCredit,
                    Remark = $"Test entry: {entryDto.DescriptionEn}",
                    CompanyID = companyId,
                    Create_by = userId,
                    Create_time = DateTime.Now
                };

                _db.Insertable(entry).ExecuteCommand();
                totalDebit += baseDebit;  // Sum base amounts, not original amounts
                totalCredit += baseCredit; // Sum base amounts, not original amounts
            }

            journal.TotalDebit = totalDebit;
            journal.TotalCredit = totalCredit;
            _db.Updateable(journal).ExecuteCommand();

            return journal;
        }

        public GLJournal GetById(Guid id)
        {
            return _db.Queryable<GLJournal>().First(v => v.Id == id);
        }

        public int Update(UpdateJournalDto dto, string userId)
        {
            var journal = _db.Queryable<GLJournal>().First(v => v.Id == dto.Id);
            if (journal == null || journal.Status != JournalStatus.Draft) return 0;

            // Delete existing entries
            _db.Deleteable<GLJournalEntry>().Where(e => e.JournalId == dto.Id).ExecuteCommand();

            // Create new entries
            decimal totalDebit = 0, totalCredit = 0;
            int lineNumber = 1;
            foreach (var entryDto in dto.Entries ?? new List<UpdateJournalEntryDto>())
            {
                var exchangeRate = entryDto.ExchangeRate > 0 ? entryDto.ExchangeRate : 1.0m;
                var baseDebit = entryDto.Debit * exchangeRate;
                var baseCredit = entryDto.Credit * exchangeRate;

                var entry = new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = entryDto.AccountId,
                    LineNumber = lineNumber++,
                    Debit = entryDto.Debit,
                    Credit = entryDto.Credit,
                    Currency = journal.Currency,
                    ExchangeRate = exchangeRate,
                    BaseDebit = baseDebit,
                    BaseCredit = baseCredit,
                    CompanyID = journal.CompanyID,
                    Create_by = userId,
                    Create_time = DateTime.Now
                };

                _db.Insertable(entry).ExecuteCommand();
                totalDebit += baseDebit;  // Sum base amounts
                totalCredit += baseCredit; // Sum base amounts
            }

            journal.TotalDebit = totalDebit;
            journal.TotalCredit = totalCredit;
            journal.Update_by = userId;
            journal.Update_time = DateTime.Now;
            return _db.Updateable(journal).ExecuteCommand();
        }

        public int Delete(Guid id)
        {
            var journal = _db.Queryable<GLJournal>().First(v => v.Id == id);
            if (journal == null || journal.Status != JournalStatus.Draft) return 0;

            _db.Deleteable<GLJournalEntry>().Where(e => e.JournalId == id).ExecuteCommand();
            return _db.Deleteable<GLJournal>().Where(v => v.Id == id).ExecuteCommand();
        }

        public int Post(Guid id, string userId)
        {
            var journal = _db.Queryable<GLJournal>().First(v => v.Id == id);
            if (journal == null || journal.Status != JournalStatus.Draft) return 0;

            // Validate double entry - use base amounts for multi-currency support
            var entries = _db.Queryable<GLJournalEntry>().Where(e => e.JournalId == id).ToList();
            var totalDebit = entries.Sum(e => e.BaseDebit);
            var totalCredit = entries.Sum(e => e.BaseCredit);
            if (totalDebit != totalCredit)
                throw new Exception("Journal is not balanced");

            journal.Status = JournalStatus.Posted;
            journal.PostedDate = DateTime.Now;
            journal.PostedBy = Guid.Parse(userId);
            return _db.Updateable(journal).ExecuteCommand();
        }

        public int Reverse(Guid id, string userId)
        {
            var journal = _db.Queryable<GLJournal>().First(v => v.Id == id);
            if (journal == null || journal.Status != JournalStatus.Posted) return 0;

            // Create reversal journal
            var reversalJournal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = GenerateJournalNumber(),
                JournalDate = DateTime.Now,
                DescriptionAr = $"عكس قيد {journal.JournalNumber}",
                DescriptionEn = $"Reversal of {journal.JournalNumber}",
                Status = JournalStatus.Posted,
                Currency = journal.Currency,
                TotalDebit = journal.TotalCredit,
                TotalCredit = journal.TotalDebit,
                FiscalYear = DateTime.Now.Year,
                FiscalPeriod = DateTime.Now.Month,
                CompanyID = journal.CompanyID,
                ReferenceNumber = journal.Id.ToString(), // Store original journal ID as reference
                Remark = $"Reversal journal for {journal.JournalNumber}",
                OriginalJournalId = journal.Id,
                Create_by = userId,
                Create_time = DateTime.Now,
                PostedDate = DateTime.Now,
                PostedBy = Guid.Parse(userId)
            };

            _db.Insertable(reversalJournal).ExecuteCommand();

            // Create reversed entries
            var entries = _db.Queryable<GLJournalEntry>().Where(e => e.JournalId == id).ToList();
            foreach (var entry in entries)
            {
                var reversalEntry = new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = reversalJournal.Id,
                    AccountId = entry.AccountId,
                    LineNumber = entry.LineNumber,
                    Debit = entry.Credit, // Reversed
                    Credit = entry.Debit, // Reversed
                    Currency = entry.Currency,
                    ExchangeRate = entry.ExchangeRate,
                    BaseDebit = entry.BaseCredit,
                    BaseCredit = entry.BaseDebit,
                    Remark = $"Reversal entry for line {entry.LineNumber}",
                    CompanyID = entry.CompanyID,
                    Create_by = userId,
                    Create_time = DateTime.Now
                };
                _db.Insertable(reversalEntry).ExecuteCommand();
            }

            // Mark original as reversed
            journal.Status = JournalStatus.Reversed;
            journal.ReversalJournalId = reversalJournal.Id;
            return _db.Updateable(journal).ExecuteCommand();
        }

        public JournalValidationResult ValidateDoubleEntry(Guid id)
        {
            var entries = _db.Queryable<GLJournalEntry>().Where(e => e.JournalId == id).ToList();
            // Use base amounts for multi-currency support
            var totalDebit = entries.Sum(e => e.BaseDebit);
            var totalCredit = entries.Sum(e => e.BaseCredit);
            return new JournalValidationResult
            {
                IsValid = totalDebit == totalCredit,
                Message = totalDebit == totalCredit ? "Balanced" : "Not balanced"
            };
        }

        public JournalTotals GetJournalTotals(Guid id)
        {
            var entries = _db.Queryable<GLJournalEntry>().Where(e => e.JournalId == id).ToList();
            // Use base amounts for multi-currency support
            var totalDebit = entries.Sum(e => e.BaseDebit);
            var totalCredit = entries.Sum(e => e.BaseCredit);
            return new JournalTotals
            {
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                IsBalanced = totalDebit == totalCredit
            };
        }

        private string GenerateJournalNumber()
        {
            return $"JRN-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }

        // Async Methods
        public async Task<GLJournal> CreateAsync(CreateJournalDto dto, string userId)
        {
            // Convert TenantId (Guid) to CompanyID (long) - use CompanyId if provided, otherwise derive from TenantId
            var companyId = dto.CompanyId ?? (dto.TenantId != Guid.Empty ? (long?)dto.TenantId.GetHashCode() : null);

            var journal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = GenerateJournalNumber(),
                JournalDate = dto.JournalDate,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                Status = JournalStatus.Draft,
                Currency = dto.Currency ?? "SAR",
                FiscalYear = dto.FiscalYear,
                FiscalPeriod = dto.FiscalPeriod,
                Remark = $"Test journal: {dto.DescriptionEn}",
                CompanyID = companyId,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            await _db.Insertable(journal).ExecuteCommandAsync();

            decimal totalDebit = 0, totalCredit = 0;
            int lineNumber = 1;
            foreach (var entryDto in dto.Entries ?? new List<CreateJournalEntryDto>())
            {
                var exchangeRate = entryDto.ExchangeRate > 0 ? entryDto.ExchangeRate : 1.0m;
                var baseDebit = entryDto.Debit * exchangeRate;
                var baseCredit = entryDto.Credit * exchangeRate;

                var entry = new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = entryDto.AccountId,
                    LineNumber = lineNumber++,
                    Debit = entryDto.Debit,
                    Credit = entryDto.Credit,
                    DescriptionAr = entryDto.DescriptionAr,
                    DescriptionEn = entryDto.DescriptionEn,
                    Currency = dto.Currency ?? "SAR",
                    ExchangeRate = exchangeRate,
                    BaseDebit = baseDebit,
                    BaseCredit = baseCredit,
                    Remark = $"Test entry: {entryDto.DescriptionEn}",
                    CompanyID = companyId,
                    Create_by = userId,
                    Create_time = DateTime.Now
                };

                await _db.Insertable(entry).ExecuteCommandAsync();
                totalDebit += baseDebit;  // Sum base amounts
                totalCredit += baseCredit; // Sum base amounts
            }

            journal.TotalDebit = totalDebit;
            journal.TotalCredit = totalCredit;
            await _db.Updateable(journal).ExecuteCommandAsync();

            return journal;
        }

        public async Task<int> PostAsync(Guid id, string userId)
        {
            var journal = await _db.Queryable<GLJournal>().FirstAsync(v => v.Id == id);
            if (journal == null || journal.Status != JournalStatus.Draft) return 0;

            var entries = await _db.Queryable<GLJournalEntry>().Where(e => e.JournalId == id).ToListAsync();
            // Use base amounts for multi-currency support
            var totalDebit = entries.Sum(e => e.BaseDebit);
            var totalCredit = entries.Sum(e => e.BaseCredit);
            if (totalDebit != totalCredit)
                throw new Exception("Journal is not balanced");

            journal.Status = JournalStatus.Posted;
            journal.PostedDate = DateTime.Now;
            journal.PostedBy = Guid.Parse(userId);
            return await _db.Updateable(journal).ExecuteCommandAsync();
        }

        public async Task<GLJournal> GetByIdAsync(Guid id)
        {
            return await _db.Queryable<GLJournal>().FirstAsync(v => v.Id == id);
        }
    }

    /// <summary>
    /// Backward compatibility alias - use TestGLJournalService instead
    /// </summary>
    public class TestGLVoucherService : TestGLJournalService
    {
        public TestGLVoucherService(ISqlSugarClient db) : base(db) { }
    }

    /// <summary>
    /// Test-specific GL Financial Report Service - خدمة التقارير المالية للاختبارات
    /// </summary>
    public class TestGLFinancialReportService
    {
        private readonly ISqlSugarClient _db;

        public TestGLFinancialReportService(ISqlSugarClient db)
        {
            _db = db;
        }

        public GLFinancialReport GenerateTrialBalance(GenerateReportDto dto, string userId)
        {
            // Convert Guid TenantId to long CompanyID
            var companyId = dto.TenantId.GetHashCode();

            // Get all posted entries in period
            var entries = _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .Where((e, v) => v.Status == JournalStatus.Posted &&
                    v.JournalDate >= dto.PeriodStartDate &&
                    v.JournalDate <= dto.PeriodEndDate &&
                    v.CompanyID == companyId)
                .ToList();

            // Use base amounts for multi-currency support
            var totalDebit = entries.Sum(e => e.BaseDebit);
            var totalCredit = entries.Sum(e => e.BaseCredit);

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.TrialBalance,
                ReportNumber = $"TB-{dto.FiscalYear ?? dto.PeriodEndDate.Year}-{DateTime.Now:yyyyMMddHHmmss}",
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodEndDate.Year,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Remark = $"Trial Balance report for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                CompanyID = companyId,
                GeneratedBy = Guid.Parse(userId),
                GeneratedAt = DateTime.Now,
                Create_time = DateTime.Now
            };

            _db.Insertable(report).ExecuteCommand();
            return report;
        }

        public GLFinancialReport GenerateBalanceSheet(GenerateReportDto dto, string userId)
        {
            // Convert Guid TenantId to long CompanyID
            var companyId = dto.TenantId.GetHashCode();

            var entries = _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .InnerJoin<GLAccount>((e, v, a) => e.AccountId == a.AccountId)
                .Where((e, v, a) => v.Status == JournalStatus.Posted &&
                    v.JournalDate <= dto.PeriodEndDate &&
                    v.CompanyID == companyId &&
                    (a.AccountType == AccountType.Asset || a.AccountType == AccountType.Liability || a.AccountType == AccountType.Equity))
                .ToList();

            // Use base amounts for multi-currency support
            var totalDebit = entries.Sum(e => e.BaseDebit);
            var totalCredit = entries.Sum(e => e.BaseCredit);

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.BalanceSheet,
                ReportNumber = $"BS-{dto.FiscalYear ?? dto.PeriodEndDate.Year}-{DateTime.Now:yyyyMMddHHmmss}",
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodEndDate.Year,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Remark = $"Balance Sheet report for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                CompanyID = companyId,
                GeneratedBy = Guid.Parse(userId),
                GeneratedAt = DateTime.Now,
                Create_time = DateTime.Now
            };

            _db.Insertable(report).ExecuteCommand();
            return report;
        }

        public GLFinancialReport GenerateIncomeStatement(GenerateReportDto dto, string userId)
        {
            // Convert Guid TenantId to long CompanyID
            var companyId = dto.TenantId.GetHashCode();

            var entries = _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .InnerJoin<GLAccount>((e, v, a) => e.AccountId == a.AccountId)
                .Where((e, v, a) => v.Status == JournalStatus.Posted &&
                    v.JournalDate >= dto.PeriodStartDate &&
                    v.JournalDate <= dto.PeriodEndDate &&
                    v.CompanyID == companyId &&
                    (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
                .ToList();

            // Use base amounts for multi-currency support
            var totalDebit = entries.Sum(e => e.BaseDebit);
            var totalCredit = entries.Sum(e => e.BaseCredit);

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.IncomeStatement,
                ReportNumber = $"IS-{dto.FiscalYear ?? dto.PeriodEndDate.Year}-{DateTime.Now:yyyyMMddHHmmss}",
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodEndDate.Year,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Remark = $"Income Statement report for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                CompanyID = companyId,
                GeneratedBy = Guid.Parse(userId),
                GeneratedAt = DateTime.Now,
                Create_time = DateTime.Now
            };

            _db.Insertable(report).ExecuteCommand();
            return report;
        }

        public GLFinancialReport GenerateCashFlow(GenerateReportDto dto, string userId)
        {
            // Convert Guid TenantId to long CompanyID
            var companyId = dto.TenantId.GetHashCode();

            var cashEntries = _db.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, v) => e.JournalId == v.Id)
                .InnerJoin<GLAccount>((e, v, a) => e.AccountId == a.AccountId)
                .Where((e, v, a) => v.Status == JournalStatus.Posted &&
                    v.JournalDate >= dto.PeriodStartDate &&
                    v.JournalDate <= dto.PeriodEndDate &&
                    v.CompanyID == companyId &&
                    a.IsCashAccount)
                .ToList();

            // Use base amounts for multi-currency support
            var totalDebit = cashEntries.Sum(e => e.BaseDebit);
            var totalCredit = cashEntries.Sum(e => e.BaseCredit);

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.CashFlow,
                ReportNumber = $"CF-{dto.FiscalYear ?? dto.PeriodEndDate.Year}-{DateTime.Now:yyyyMMddHHmmss}",
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodEndDate.Year,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                ReportData = $"{{\"cashInflow\": {totalDebit}, \"cashOutflow\": {totalCredit}, \"netCashFlow\": {totalDebit - totalCredit}}}",
                Remark = $"Cash Flow report for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                CompanyID = companyId,
                GeneratedBy = Guid.Parse(userId),
                GeneratedAt = DateTime.Now,
                Create_time = DateTime.Now
            };

            _db.Insertable(report).ExecuteCommand();
            return report;
        }

        public GLFinancialReport GetById(Guid id)
        {
            return _db.Queryable<GLFinancialReport>().First(r => r.Id == id);
        }
    }

    /// <summary>
    /// Test-specific GL Period Closing Service - خدمة إقفال الفترات للاختبارات
    /// </summary>
    public class TestGLPeriodClosingService
    {
        private readonly ISqlSugarClient _db;

        public TestGLPeriodClosingService(ISqlSugarClient db)
        {
            _db = db;
        }

        public GLPeriodClosing Create(CreatePeriodClosingDto dto, string userId)
        {
            // Convert TenantId (Guid) to CompanyID (long) - use CompanyId if provided, otherwise derive from TenantId
            var companyId = dto.CompanyId ?? (dto.TenantId != Guid.Empty ? (long?)dto.TenantId.GetHashCode() : null);

            var period = new GLPeriodClosing
            {
                Id = Guid.NewGuid(),
                PeriodType = dto.PeriodType,
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                FiscalPeriod = dto.FiscalPeriod ?? dto.PeriodStartDate.Month,
                Status = PeriodStatus.Open,
                Remark = $"Period closing for {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                CompanyID = companyId,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            _db.Insertable(period).ExecuteCommand();
            return period;
        }

        public GLPeriodClosing GetById(Guid id)
        {
            return _db.Queryable<GLPeriodClosing>().First(p => p.Id == id);
        }

        public int ClosePeriod(ClosePeriodDto dto, string userId)
        {
            var period = _db.Queryable<GLPeriodClosing>().First(p => p.Id == dto.Id);
            if (period == null || period.Status != PeriodStatus.Open)
                throw new Exception("Period not found or already closed");

            // Check for unposted journals
            var hasUnposted = _db.Queryable<GLJournal>()
                .Where(v => v.JournalDate >= period.PeriodStartDate &&
                    v.JournalDate <= period.PeriodEndDate &&
                    v.CompanyID == period.CompanyID &&
                    v.Status == JournalStatus.Draft)
                .Any();

            if (hasUnposted && !dto.ForceClose)
                throw new Exception("Period has unposted journals");

            // Calculate totals from posted journals
            var postedJournals = _db.Queryable<GLJournal>()
                .Where(v => v.JournalDate >= period.PeriodStartDate &&
                    v.JournalDate <= period.PeriodEndDate &&
                    v.CompanyID == period.CompanyID &&
                    v.Status == JournalStatus.Posted)
                .ToList();

            period.PostedVouchers = postedJournals.Count;
            period.TotalDebit = postedJournals.Sum(v => v.TotalDebit);
            period.TotalCredit = postedJournals.Sum(v => v.TotalCredit);
            period.Status = PeriodStatus.Closed;
            period.ClosedAt = DateTime.Now;
            period.ClosedBy = Guid.Parse(userId);
            period.ClosingNotes = dto.ClosingNotes;

            return _db.Updateable(period).ExecuteCommand();
        }

        public int ReopenPeriod(ReopenPeriodDto dto, string userId)
        {
            var period = _db.Queryable<GLPeriodClosing>().First(p => p.Id == dto.Id);
            if (period == null)
                throw new Exception("Period not found");
            if (period.Status == PeriodStatus.Locked)
                throw new Exception("Cannot reopen locked period");

            period.Status = PeriodStatus.Open;
            period.ReopenedAt = DateTime.Now;
            period.ReopenedBy = Guid.Parse(userId);
            // Note: Reason is stored in ClosingNotes for audit trail
            period.ClosingNotes = $"Reopened: {dto.Reason}";

            return _db.Updateable(period).ExecuteCommand();
        }

        public int LockPeriod(Guid id, string userId)
        {
            var period = _db.Queryable<GLPeriodClosing>().First(p => p.Id == id);
            if (period == null || period.Status != PeriodStatus.Closed)
                throw new Exception("Period not found or not closed");

            period.Status = PeriodStatus.Locked;
            // Note: Using ClosingNotes for audit trail since LockedAt/LockedBy don't exist
            period.ClosingNotes = $"{period.ClosingNotes} | Locked at {DateTime.Now} by {userId}";

            return _db.Updateable(period).ExecuteCommand();
        }

        public bool IsDateInClosedPeriod(DateTime date, Guid tenantId)
        {
            // Convert Guid to long for CompanyID comparison
            var companyId = (long)tenantId.GetHashCode();
            return _db.Queryable<GLPeriodClosing>()
                .Where(p => p.CompanyID == companyId &&
                    date >= p.PeriodStartDate &&
                    date <= p.PeriodEndDate &&
                    (p.Status == PeriodStatus.Closed || p.Status == PeriodStatus.Locked))
                .Any();
        }

        public List<GLPeriodClosing> GenerateFiscalYearPeriods(int fiscalYear, PeriodType periodType, string userId, Guid tenantId)
        {
            var periods = new List<GLPeriodClosing>();
            var companyId = (long)tenantId.GetHashCode();

            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(fiscalYear, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var period = new GLPeriodClosing
                {
                    Id = Guid.NewGuid(),
                    PeriodType = periodType,
                    PeriodStartDate = startDate,
                    PeriodEndDate = endDate,
                    FiscalYear = fiscalYear,
                    FiscalPeriod = month,
                    Status = PeriodStatus.Open,
                    Remark = $"Fiscal period {month}/{fiscalYear}",
                    CompanyID = companyId,
                    Create_by = userId,
                    Create_time = DateTime.Now
                };

                _db.Insertable(period).ExecuteCommand();
                periods.Add(period);
            }

            return periods;
        }
    }

    /// <summary>
    /// Journal validation result - نتيجة تحقق القيد
    /// </summary>
    public class JournalValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Journal totals - مجاميع القيد
    /// </summary>
    public class JournalTotals
    {
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsBalanced { get; set; }
    }

    /// <summary>
    /// Backward compatibility alias for VoucherValidationResult
    /// </summary>
    public class VoucherValidationResult : JournalValidationResult { }

    /// <summary>
    /// Backward compatibility alias for VoucherTotals
    /// </summary>
    public class VoucherTotals : JournalTotals { }
}

