using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.IService.Finance.GL;
using ZR.Service.Finance.Configuration;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// Year-End Closing Service - خدمة إغلاق نهاية السنة
    /// Handles fiscal year closing with P/L transfer to Retained Earnings
    /// يعالج إغلاق السنة المالية مع تحويل الأرباح والخسائر إلى الأرباح المحتجزة
    /// </summary>
    [AppService(ServiceType = typeof(IYearEndClosingService), ServiceLifetime = LifeTime.Transient)]
    public class YearEndClosingService : BaseService<GLJournal>, IYearEndClosingService
    {
        private readonly IGLJournalService _journalService;
        private readonly IGLPeriodClosingService _periodClosingService;
        private readonly IFiscalYearHelper _fiscalYearHelper;
        private readonly INormalBalanceResolver _normalBalanceResolver;

        /// <summary>
        /// Source constant for Year-End Closing journals
        /// </summary>
        public const string YearEndClosingSource = "YearEndClosing";

        public YearEndClosingService(
            IGLJournalService journalService,
            IGLPeriodClosingService periodClosingService,
            IFiscalYearHelper fiscalYearHelper,
            INormalBalanceResolver normalBalanceResolver)
        {
            _journalService = journalService;
            _periodClosingService = periodClosingService;
            _fiscalYearHelper = fiscalYearHelper;
            _normalBalanceResolver = normalBalanceResolver;
        }

        public async Task<YearEndClosingResultDto> CloseFiscalYearAndTransferProfitLossAsync(
            CloseFiscalYearDto dto, string userName)
        {
            var result = new YearEndClosingResultDto
            {
                FiscalYear = dto.FiscalYear,
                Success = false
            };

            try
            {
                // Step 1: Validate
                var validation = await ValidateForClosingAsync(dto.FiscalYear, dto.TenantId);
                if (!validation.IsValid)
                {
                    result.Errors = validation.Errors;
                    result.Warnings = validation.Warnings;
                    result.Message = "Validation failed - فشل التحقق";
                    return result;
                }

                // Step 2: Get retained earnings account
                var retainedEarningsAccountId = dto.RetainedEarningsAccountId ?? GetRetainedEarningsAccountId(dto.TenantId);
                if (!retainedEarningsAccountId.HasValue)
                {
                    result.Errors.Add("No retained earnings account found - لم يتم العثور على حساب الأرباح المحتجزة");
                    result.Message = "No retained earnings account configured";
                    return result;
                }

                // Step 3: Calculate net income
                var (totalRevenue, totalExpenses, netIncome) = GetNetIncome(dto.FiscalYear, dto.TenantId);
                result.TotalRevenue = totalRevenue;
                result.TotalExpenses = totalExpenses;
                result.NetIncome = netIncome;

                if (netIncome == 0)
                {
                    result.Warnings.Add("Net income is zero - no closing entry needed");
                }

                // Step 4: Get all revenue and expense accounts with balances
                var revenueAccounts = GetAccountsWithBalances(dto.FiscalYear, AccountType.Revenue, dto.TenantId);
                var expenseAccounts = GetAccountsWithBalances(dto.FiscalYear, AccountType.Expense, dto.TenantId);

                // Step 5: Validate closing date
                // Year-end closing can ONLY be performed AFTER the fiscal year has ended
                // The closing journal is dated on the last day of the fiscal year
                var fiscalYearStartDate = _fiscalYearHelper.GetFiscalYearStartDate(dto.FiscalYear, dto.TenantId);
                var fiscalYearEndDate = _fiscalYearHelper.GetFiscalYearEndDate(dto.FiscalYear, dto.TenantId);

                if (DateTime.Today <= fiscalYearEndDate)
                {
                    result.Errors.Add($"Cannot close fiscal year {dto.FiscalYear} before it ends. Year-end closing can only be performed after {fiscalYearEndDate:yyyy-MM-dd} - لا يمكن إغلاق السنة المالية {dto.FiscalYear} قبل انتهائها. يمكن إجراء إغلاق نهاية السنة فقط بعد {fiscalYearEndDate:yyyy-MM-dd}");
                    result.Message = $"Fiscal year {dto.FiscalYear} has not ended yet. Please wait until after {fiscalYearEndDate:yyyy-MM-dd}";
                    return result;
                }

                // Closing journal is always dated on the last day of the fiscal year
                var closingDate = dto.ClosingDate ?? fiscalYearEndDate;

                // Validate that closing date is within the fiscal year being closed
                if (closingDate < fiscalYearStartDate || closingDate > fiscalYearEndDate)
                {
                    result.Errors.Add($"Closing date must be within fiscal year {dto.FiscalYear} (between {fiscalYearStartDate:yyyy-MM-dd} and {fiscalYearEndDate:yyyy-MM-dd})");
                    result.Message = "Invalid closing date";
                    return result;
                }

                var journalEntries = new List<CreateJournalEntryDto>();
                int lineNumber = 1;

                // Close Revenue accounts using normalBalance-aware closing entry calculation
                foreach (var account in revenueAccounts.Where(a => a.Balance != 0))
                {
                    var (debit, credit) = _normalBalanceResolver.GetClosingEntry(account.Balance, account.NormalBalance);
                    journalEntries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = account.AccountId,
                        Debit = debit,
                        Credit = credit,
                        DescriptionEn = $"Year-End Closing - Close Revenue {account.AccountCode}",
                        DescriptionAr = $"قيد إقفال نهاية السنة - إقفال إيراد {account.AccountCode}"
                    });
                }

                // Close Expense accounts using normalBalance-aware closing entry calculation
                foreach (var account in expenseAccounts.Where(a => a.Balance != 0))
                {
                    var (debit, credit) = _normalBalanceResolver.GetClosingEntry(account.Balance, account.NormalBalance);
                    journalEntries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = account.AccountId,
                        Debit = debit,
                        Credit = credit,
                        DescriptionEn = $"Year-End Closing - Close Expense {account.AccountCode}",
                        DescriptionAr = $"قيد إقفال نهاية السنة - إقفال مصروف {account.AccountCode}"
                    });
                }

                // Transfer net income to Retained Earnings
                if (netIncome != 0)
                {
                    journalEntries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = retainedEarningsAccountId.Value,
                        Debit = netIncome < 0 ? Math.Abs(netIncome) : 0,  // Loss: debit retained earnings
                        Credit = netIncome > 0 ? netIncome : 0,           // Profit: credit retained earnings
                        DescriptionEn = netIncome > 0
                            ? "Year-End Closing - Transfer Net Profit to Retained Earnings"
                            : "Year-End Closing - Transfer Net Loss to Retained Earnings",
                        DescriptionAr = netIncome > 0
                            ? "قيد إقفال نهاية السنة - تحويل صافي الربح إلى الأرباح المحتجزة"
                            : "قيد إقفال نهاية السنة - تحويل صافي الخسارة إلى الأرباح المحتجزة"
                    });
                }

                // Step 6: Create the closing journal
                var journalDto = new CreateJournalDto
                {
                    JournalDate = closingDate,
                    Source = YearEndClosingSource,
                    SourceDocumentType = "YearEndClosing",
                    DescriptionEn = dto.Description ?? $"Year-End Closing Journal for Fiscal Year {dto.FiscalYear}",
                    DescriptionAr = $"قيد إقفال نهاية السنة المالية {dto.FiscalYear}",
                    ReferenceNumber = $"YEC-{dto.FiscalYear}",
                    Currency = "SAR",
                    FiscalYear = dto.FiscalYear,
                    Entries = journalEntries,
                    CompanyId = dto.TenantId
                };

                var closingJournal = _journalService.Create(journalDto, userName);
                result.ClosingJournalId = closingJournal.Id;
                result.ClosingJournalNumber = closingJournal.JournalNumber;
                result.ClosingDate = closingDate;

                // Step 7: Auto-post if requested
                if (dto.AutoPost)
                {
                    var postResult = _journalService.PostJournal(closingJournal.Id, userName);
                    result.JournalPosted = postResult > 0;
                }

                result.Success = true;
                result.Message = $"Fiscal year {dto.FiscalYear} closed successfully - تم إغلاق السنة المالية {dto.FiscalYear} بنجاح";

                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                result.Errors.Add($"Error closing fiscal year: {errorMessage}");
                result.Message = $"An error occurred during year-end closing: {errorMessage}";
                return result;
            }
        }

        public async Task<YearEndClosingPreviewDto> PreviewYearEndClosingAsync(int fiscalYear, long? companyId = null)
        {
            var preview = new YearEndClosingPreviewDto
            {
                FiscalYear = fiscalYear
            };

            // Get validation status
            var validation = await ValidateForClosingAsync(fiscalYear, companyId);
            preview.CanClose = validation.IsValid;
            preview.Warnings = validation.Warnings;
            preview.BlockingIssues = validation.Errors;

            // Get retained earnings account
            var retainedEarningsId = GetRetainedEarningsAccountId(companyId);
            preview.RetainedEarningsAccountId = retainedEarningsId;
            if (retainedEarningsId.HasValue)
            {
                var retainedEarningsAccount = Context.Queryable<GLAccount>()
                    .Where(a => a.AccountId == retainedEarningsId.Value)
                    .First();

                if (retainedEarningsAccount != null)
                {
                    preview.RetainedEarningsAccountCode = retainedEarningsAccount.FullAccountNumber;
                    preview.RetainedEarningsAccountName = retainedEarningsAccount.AccountNameEn;
                }
            }

            // Calculate net income
            var (totalRevenue, totalExpenses, netIncome) = GetNetIncome(fiscalYear, companyId);
            preview.TotalRevenue = totalRevenue;
            preview.TotalExpenses = totalExpenses;
            preview.NetIncome = netIncome;

            // Get revenue accounts with balances using normalBalance-aware closing entry calculation
            var revenueAccounts = GetAccountsWithBalances(fiscalYear, AccountType.Revenue, companyId);
            foreach (var account in revenueAccounts.Where(a => a.Balance != 0))
            {
                var (debit, credit) = _normalBalanceResolver.GetClosingEntry(account.Balance, account.NormalBalance);
                preview.RevenueAccounts.Add(new AccountClosingPreviewDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    Balance = account.Balance,
                    ClosingDebit = debit,
                    ClosingCredit = credit
                });
            }

            // Get expense accounts with balances using normalBalance-aware closing entry calculation
            var expenseAccounts = GetAccountsWithBalances(fiscalYear, AccountType.Expense, companyId);
            foreach (var account in expenseAccounts.Where(a => a.Balance != 0))
            {
                var (debit, credit) = _normalBalanceResolver.GetClosingEntry(account.Balance, account.NormalBalance);
                preview.ExpenseAccounts.Add(new AccountClosingPreviewDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    Balance = account.Balance,
                    ClosingDebit = debit,
                    ClosingCredit = credit
                });
            }

            return preview;
        }

        public async Task<YearEndClosingValidationDto> ValidateForClosingAsync(int fiscalYear, long? companyId = null)
        {
            var validation = new YearEndClosingValidationDto
            {
                FiscalYear = fiscalYear,
                IsValid = true
            };

            // Check 0: Fiscal year must have ended (cannot close a year that hasn't finished)
            var fiscalYearEndDate = _fiscalYearHelper.GetFiscalYearEndDate(fiscalYear, companyId);
            validation.FiscalYearEnded = DateTime.Today > fiscalYearEndDate;
            if (!validation.FiscalYearEnded)
            {
                validation.Errors.Add($"Fiscal year {fiscalYear} has not ended yet. Year-end closing can only be performed after {fiscalYearEndDate:yyyy-MM-dd} - السنة المالية {fiscalYear} لم تنتهِ بعد. يمكن إجراء إغلاق نهاية السنة فقط بعد {fiscalYearEndDate:yyyy-MM-dd}");
                validation.IsValid = false;
            }

            // Check 1: All periods should be open (not locked)
            var periods = _periodClosingService.GetByFiscalYear(fiscalYear, companyId);
            validation.AllPeriodsOpen = periods.All(p => p.Status != PeriodStatus.Locked);
            if (!validation.AllPeriodsOpen)
            {
                validation.Errors.Add("Some periods are locked and cannot be modified - بعض الفترات مقفلة ولا يمكن تعديلها");
                validation.IsValid = false;
            }

            // Check 2: All journals should be posted (Submitted = pending approval)
            // Use fiscal year date range instead of calendar year
            var fiscalYearStartDate = _fiscalYearHelper.GetFiscalYearStartDate(fiscalYear, companyId);
            var unpostedJournals = Context.Queryable<GLJournal>()
                .Where(j => j.JournalDate >= fiscalYearStartDate && j.JournalDate <= fiscalYearEndDate)
                .Where(j => j.Status == JournalStatus.Draft || j.Status == JournalStatus.Submitted)
                .WhereIF(companyId.HasValue, j => j.CompanyID == companyId)
                .Count();

            validation.UnpostedJournalsCount = unpostedJournals;
            validation.AllJournalsPosted = unpostedJournals == 0;
            if (!validation.AllJournalsPosted)
            {
                validation.Warnings.Add($"There are {unpostedJournals} unposted journals in this fiscal year - يوجد {unpostedJournals} قيود غير مرحّلة في هذه السنة المالية");
            }

            // Check 3: Trial balance should be balanced
            // Use fiscal year date range instead of calendar year
            var entries = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => j.JournalDate >= fiscalYearStartDate && j.JournalDate <= fiscalYearEndDate && j.Status == JournalStatus.Posted)
                .WhereIF(companyId.HasValue, (e, j) => j.CompanyID == companyId)
                .Select((e, j) => new { e.Debit, e.Credit })
                .ToList();

            var totalDebit = entries.Sum(e => e.Debit);
            var totalCredit = entries.Sum(e => e.Credit);
            validation.TrialBalanceDifference = Math.Abs(totalDebit - totalCredit);
            validation.TrialBalanceBalanced = validation.TrialBalanceDifference < 0.01m;
            if (!validation.TrialBalanceBalanced)
            {
                validation.Errors.Add($"Trial balance is not balanced. Difference: {validation.TrialBalanceDifference:N4} - ميزان المراجعة غير متوازن");
                validation.IsValid = false;
            }

            // Check 4: Retained earnings account exists
            validation.RetainedEarningsAccountId = GetRetainedEarningsAccountId(companyId);
            validation.HasRetainedEarningsAccount = validation.RetainedEarningsAccountId.HasValue;
            if (!validation.HasRetainedEarningsAccount)
            {
                validation.Errors.Add("No retained earnings account configured (IsRetainedEarningsAccount = true) - لم يتم تكوين حساب الأرباح المحتجزة");
                validation.IsValid = false;
            }

            // Check 5: Not already closed
            var existingClosingJournal = _journalService.GetYearEndClosingJournal(fiscalYear, companyId);
            validation.ExistingClosingJournalId = existingClosingJournal?.Id;
            validation.NotAlreadyClosed = existingClosingJournal == null;
            if (!validation.NotAlreadyClosed)
            {
                validation.Errors.Add($"Fiscal year {fiscalYear} is already closed. Closing journal: {existingClosingJournal.JournalNumber} - السنة المالية مغلقة مسبقاً");
                validation.IsValid = false;
            }

            // Populate period statuses
            foreach (var period in periods)
            {
                validation.PeriodStatuses.Add(new PeriodStatusDto
                {
                    PeriodId = period.Id,
                    PeriodName = $"Period {period.FiscalPeriod}",
                    PeriodNumber = period.FiscalPeriod,
                    IsOpen = period.Status == PeriodStatus.Open,
                    IsClosed = period.Status == PeriodStatus.Closed,
                    HasUnpostedJournals = false
                });
            }

            return validation;
        }

        public Guid? GetRetainedEarningsAccountId(long? companyId = null)
        {
            var account = Context.Queryable<GLAccount>()
                .Where(a => a.IsRetainedEarningsAccount && a.IsActive)
                .WhereIF(companyId.HasValue, a => a.CompanyID == companyId)
                .First();

            return account?.AccountId;
        }

        public bool IsFiscalYearClosed(int fiscalYear, long? companyId = null)
        {
            var closingJournal = _journalService.GetYearEndClosingJournal(fiscalYear, companyId);
            return closingJournal != null;
        }

        public (decimal TotalRevenue, decimal TotalExpenses, decimal NetIncome) GetNetIncome(int fiscalYear, long? companyId = null)
        {
            // Get all posted entries for the fiscal year
            var revenueAccounts = GetAccountsWithBalances(fiscalYear, AccountType.Revenue, companyId);
            var expenseAccounts = GetAccountsWithBalances(fiscalYear, AccountType.Expense, companyId);

            // Revenue has credit normal balance, so positive balance = revenue
            var totalRevenue = revenueAccounts.Sum(a => a.Balance);

            // Expense has debit normal balance, so positive balance = expense
            var totalExpenses = expenseAccounts.Sum(a => a.Balance);

            // Net Income = Revenue - Expenses
            var netIncome = totalRevenue - totalExpenses;

            return (totalRevenue, totalExpenses, netIncome);
        }

        #region Private Helpers

        private List<AccountBalanceInfo> GetAccountsWithBalances(int fiscalYear, AccountType accountType, long? companyId)
        {
            // Get all accounts of the specified type
            var accounts = Context.Queryable<GLAccount>()
                .Where(a => a.AccountType == accountType && a.IsActive && a.IsLeaf)
                .WhereIF(companyId.HasValue, a => a.CompanyID == companyId)
                .ToList();

            var result = new List<AccountBalanceInfo>();

            // Get fiscal year date range for filtering
            var fiscalYearStartDate = _fiscalYearHelper.GetFiscalYearStartDate(fiscalYear, companyId);
            var fiscalYearEndDate = _fiscalYearHelper.GetFiscalYearEndDate(fiscalYear, companyId);

            foreach (var account in accounts)
            {
                // Calculate balance from posted journals using fiscal year date range
                var entries = Context.Queryable<GLJournalEntry>()
                    .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                    .Where((e, j) => e.AccountId == account.AccountId)
                    .Where((e, j) => j.JournalDate >= fiscalYearStartDate && j.JournalDate <= fiscalYearEndDate)
                    .Where((e, j) => j.Status == JournalStatus.Posted)
                    .Where((e, j) => j.Source != YearEndClosingSource) // Exclude previous closing entries
                    .WhereIF(companyId.HasValue, (e, j) => j.CompanyID == companyId)
                    .Select((e, j) => new { e.Debit, e.Credit })
                    .ToList();

                var totalDebit = entries.Sum(e => e.Debit);
                var totalCredit = entries.Sum(e => e.Credit);

                // Use normalBalance-aware calculation for correct balance direction
                var balance = _normalBalanceResolver.CalculateBalance(
                    totalDebit,
                    totalCredit,
                    account.NormalBalance);

                if (balance != 0)
                {
                    result.Add(new AccountBalanceInfo
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.FullAccountNumber,
                        AccountNameAr = account.AccountNameAr,
                        AccountNameEn = account.AccountNameEn,
                        Balance = balance,
                        NormalBalance = account.NormalBalance
                    });
                }
            }

            return result;
        }

        private class AccountBalanceInfo
        {
            public Guid AccountId { get; set; }
            public string AccountCode { get; set; }
            public string AccountNameAr { get; set; }
            public string AccountNameEn { get; set; }
            public decimal Balance { get; set; }
            public NormalBalance NormalBalance { get; set; }
        }

        #endregion
    }
}
