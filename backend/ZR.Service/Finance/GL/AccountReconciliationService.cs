using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Microsoft.Extensions.Logging;
using SqlSugar;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// Account Reconciliation Service - خدمة تسوية الحسابات
    /// Manages reconciliation workflow for accounts with ReconciliationMode = Required
    /// This service addresses the critical gap identified in the GLAccount Field Usage Report
    /// </summary>
    [AppService(ServiceType = typeof(IAccountReconciliationService), ServiceLifetime = LifeTime.Transient)]
    public class AccountReconciliationService : BaseService<GLAccount>, IAccountReconciliationService
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLJournalEntryRepository _entryRepository;
        private readonly ILogger<AccountReconciliationService> _logger;

        // Reconciliation is considered overdue after 30 days (configurable)
        private const int ReconciliationOverdueDays = 30;

        public AccountReconciliationService(
            IGLAccountRepository accountRepository,
            IGLJournalEntryRepository entryRepository,
            ILogger<AccountReconciliationService> logger)
        {
            _accountRepository = accountRepository;
            _entryRepository = entryRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<AccountReconciliationStatusDto>> GetAccountsRequiringReconciliationAsync(long? companyId = null)
        {
            var query = Context.Queryable<GLAccount>()
                .Where(a => a.ReconciliationMode == ReconciliationMode.Required && a.IsActive);

            if (companyId.HasValue)
            {
                query = query.Where(a => a.CompanyID == companyId.Value);
            }

            var accounts = await query.OrderBy(a => a.FullAccountNumber).ToListAsync();

            return await MapToReconciliationStatusDtoListAsync(accounts);
        }

        /// <inheritdoc />
        public async Task<List<AccountReconciliationStatusDto>> GetPendingReconciliationsAsync(
            int fiscalYear, int period, long? companyId = null)
        {
            var periodEndDate = new DateTime(fiscalYear, period, DateTime.DaysInMonth(fiscalYear, period));

            var query = Context.Queryable<GLAccount>()
                .Where(a => a.ReconciliationMode == ReconciliationMode.Required && a.IsActive)
                .Where(a => a.ReconciliationStatus != ReconciliationStatus.Reconciled ||
                           a.LastReconciledDate == null ||
                           a.LastReconciledDate < periodEndDate);

            if (companyId.HasValue)
            {
                query = query.Where(a => a.CompanyID == companyId.Value);
            }

            var accounts = await query.OrderBy(a => a.FullAccountNumber).ToListAsync();

            return await MapToReconciliationStatusDtoListAsync(accounts);
        }

        /// <inheritdoc />
        public async Task<bool> MarkAccountReconciledAsync(
            Guid accountId, DateTime asOfDate, string reconciledBy, string notes = null)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Reconciliation failed: Account {AccountId} not found - فشل التسوية: الحساب غير موجود", accountId);
                return false;
            }

            if (account.ReconciliationMode != ReconciliationMode.Required)
            {
                _logger.LogWarning("Reconciliation failed: Account {AccountCode} is not a reconciliation account - الحساب ليس حساب تسوية",
                    account.FullAccountNumber);
                return false;
            }

            var previousStatus = account.ReconciliationStatus;

            // Update account reconciliation status
            var result = await Context.Updateable<GLAccount>()
                .SetColumns(a => new GLAccount
                {
                    ReconciliationStatus = ReconciliationStatus.Reconciled,
                    LastReconciledDate = asOfDate,
                    LastReconciledBy = reconciledBy,
                    Update_time = DateTime.Now,
                    Update_by = reconciledBy
                })
                .Where(a => a.AccountId == accountId)
                .ExecuteCommandAsync();

            if (result > 0)
            {
                _logger.LogInformation(
                    "Account {AccountCode} reconciled by {User} as of {Date}. Previous status: {PreviousStatus} - تم تسوية الحساب",
                    account.FullAccountNumber, reconciledBy, asOfDate, previousStatus);

                // Log to reconciliation history (if history table exists)
                await LogReconciliationHistoryAsync(accountId, reconciledBy, previousStatus,
                    ReconciliationStatus.Reconciled, notes);
            }

            return result > 0;
        }

        /// <inheritdoc />
        public async Task<bool> MarkAccountPendingReviewAsync(Guid accountId, string markedBy, string reason)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Mark pending failed: Account {AccountId} not found", accountId);
                return false;
            }

            var previousStatus = account.ReconciliationStatus;

            var result = await Context.Updateable<GLAccount>()
                .SetColumns(a => new GLAccount
                {
                    ReconciliationStatus = ReconciliationStatus.PendingReview,
                    Update_time = DateTime.Now,
                    Update_by = markedBy
                })
                .Where(a => a.AccountId == accountId)
                .ExecuteCommandAsync();

            if (result > 0)
            {
                _logger.LogInformation(
                    "Account {AccountCode} marked as pending review by {User}. Reason: {Reason}",
                    account.FullAccountNumber, markedBy, reason);

                await LogReconciliationHistoryAsync(accountId, markedBy, previousStatus,
                    ReconciliationStatus.PendingReview, reason);
            }

            return result > 0;
        }

        /// <inheritdoc />
        public async Task<bool> ResetReconciliationStatusAsync(Guid accountId, string resetBy, string reason)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Reset failed: Account {AccountId} not found", accountId);
                return false;
            }

            var previousStatus = account.ReconciliationStatus;

            var result = await Context.Updateable<GLAccount>()
                .SetColumns(a => new GLAccount
                {
                    ReconciliationStatus = ReconciliationStatus.NotReconciled,
                    Update_time = DateTime.Now,
                    Update_by = resetBy
                })
                .Where(a => a.AccountId == accountId)
                .ExecuteCommandAsync();

            if (result > 0)
            {
                _logger.LogWarning(
                    "Account {AccountCode} reconciliation status reset by {User}. Reason: {Reason}. Previous status: {PreviousStatus}",
                    account.FullAccountNumber, resetBy, reason, previousStatus);

                await LogReconciliationHistoryAsync(accountId, resetBy, previousStatus,
                    ReconciliationStatus.NotReconciled, reason);
            }

            return result > 0;
        }

        /// <inheritdoc />
        public async Task<ReconciliationValidationResult> ValidateAllReconciliationsCompleteAsync(
            int fiscalYear, int period, long? companyId = null)
        {
            var result = new ReconciliationValidationResult { IsValid = true };

            var periodEndDate = new DateTime(fiscalYear, period, DateTime.DaysInMonth(fiscalYear, period));

            var query = Context.Queryable<GLAccount>()
                .Where(a => a.ReconciliationMode == ReconciliationMode.Required && a.IsActive);

            if (companyId.HasValue)
            {
                query = query.Where(a => a.CompanyID == companyId.Value);
            }

            var accounts = await query.ToListAsync();

            result.TotalReconciliationAccounts = accounts.Count;
            result.ReconciledCount = accounts.Count(a =>
                a.ReconciliationStatus == ReconciliationStatus.Reconciled &&
                a.LastReconciledDate.HasValue &&
                a.LastReconciledDate.Value >= periodEndDate.AddDays(-ReconciliationOverdueDays));
            result.PendingCount = accounts.Count(a =>
                a.ReconciliationStatus == ReconciliationStatus.PendingReview);
            result.NotReconciledCount = result.TotalReconciliationAccounts - result.ReconciledCount - result.PendingCount;

            // Get unreconciled accounts
            var unreconciledAccounts = accounts.Where(a =>
                a.ReconciliationStatus != ReconciliationStatus.Reconciled ||
                !a.LastReconciledDate.HasValue ||
                a.LastReconciledDate.Value < periodEndDate.AddDays(-ReconciliationOverdueDays)).ToList();

            result.UnreconciledAccounts = await MapToReconciliationStatusDtoListAsync(unreconciledAccounts);

            // Validate
            if (unreconciledAccounts.Any())
            {
                result.IsValid = false;
                foreach (var account in unreconciledAccounts)
                {
                    var message = $"Account {account.FullAccountNumber} ({account.AccountNameEn}) has not been reconciled for period {fiscalYear}/{period} - " +
                                 $"الحساب {account.FullAccountNumber} ({account.AccountNameAr}) لم يتم تسويته للفترة {fiscalYear}/{period}";
                    result.Errors.Add(message);
                }

                _logger.LogWarning(
                    "Period closing validation failed: {Count} accounts require reconciliation for {Year}/{Period}",
                    unreconciledAccounts.Count, fiscalYear, period);
            }

            // Add warnings for pending review
            var pendingAccounts = accounts.Where(a =>
                a.ReconciliationStatus == ReconciliationStatus.PendingReview).ToList();

            foreach (var account in pendingAccounts)
            {
                result.Warnings.Add(
                    $"Account {account.FullAccountNumber} is pending review - الحساب {account.FullAccountNumber} معلق للمراجعة");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<List<ReconciliationHistoryDto>> GetReconciliationHistoryAsync(Guid accountId)
        {
            // For now, return empty list - history tracking would require a separate table
            // This is a placeholder for future implementation
            _logger.LogInformation("Reconciliation history requested for account {AccountId}", accountId);
            return await Task.FromResult(new List<ReconciliationHistoryDto>());
        }

        /// <inheritdoc />
        public async Task<ReconciliationDashboardDto> GetReconciliationDashboardAsync(long? companyId = null)
        {
            var dashboard = new ReconciliationDashboardDto();

            var query = Context.Queryable<GLAccount>()
                .Where(a => a.ReconciliationMode == ReconciliationMode.Required && a.IsActive);

            if (companyId.HasValue)
            {
                query = query.Where(a => a.CompanyID == companyId.Value);
            }

            var accounts = await query.ToListAsync();

            var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var overdueThreshold = DateTime.Now.AddDays(-ReconciliationOverdueDays);

            dashboard.TotalReconciliationAccounts = accounts.Count;

            dashboard.ReconciledThisMonth = accounts.Count(a =>
                a.ReconciliationStatus == ReconciliationStatus.Reconciled &&
                a.LastReconciledDate.HasValue &&
                a.LastReconciledDate.Value >= currentMonthStart);

            dashboard.PendingReview = accounts.Count(a =>
                a.ReconciliationStatus == ReconciliationStatus.PendingReview);

            var overdueAccounts = accounts.Where(a =>
                a.ReconciliationStatus != ReconciliationStatus.Reconciled ||
                !a.LastReconciledDate.HasValue ||
                a.LastReconciledDate.Value < overdueThreshold).ToList();

            dashboard.OverdueReconciliations = overdueAccounts.Count;
            dashboard.OverdueAccounts = await MapToReconciliationStatusDtoListAsync(overdueAccounts.Take(10).ToList());

            var recentlyReconciled = accounts
                .Where(a => a.LastReconciledDate.HasValue)
                .OrderByDescending(a => a.LastReconciledDate)
                .Take(5)
                .ToList();

            dashboard.RecentlyReconciled = await MapToReconciliationStatusDtoListAsync(recentlyReconciled);

            // Calculate total unreconciled balance
            foreach (var account in overdueAccounts)
            {
                var balance = await _entryRepository.GetAccountBalanceAsync(
                    account.AccountId, new DateTime(2000, 1, 1), DateTime.Now);
                dashboard.TotalUnreconciledBalance += Math.Abs(balance.Balance);
            }

            return dashboard;
        }

        #region Private Helper Methods

        private async Task<List<AccountReconciliationStatusDto>> MapToReconciliationStatusDtoListAsync(List<GLAccount> accounts)
        {
            var result = new List<AccountReconciliationStatusDto>();

            foreach (var account in accounts)
            {
                var balance = await _entryRepository.GetAccountBalanceAsync(
                    account.AccountId, new DateTime(2000, 1, 1), DateTime.Now);

                var overdueThreshold = DateTime.Now.AddDays(-ReconciliationOverdueDays);

                result.Add(new AccountReconciliationStatusDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.FullAccountNumber,
                    AccountNameEn = account.AccountNameEn,
                    AccountNameAr = account.AccountNameAr,
                    IsReconciliationAccount = account.ReconciliationMode == ReconciliationMode.Required,
                    Status = account.ReconciliationStatus,
                    LastReconciledDate = account.LastReconciledDate,
                    LastReconciledBy = account.LastReconciledBy,
                    IsOverdue = !account.LastReconciledDate.HasValue ||
                               account.LastReconciledDate.Value < overdueThreshold,
                    CurrentBalance = balance.Balance
                });
            }

            return result;
        }

        private async Task LogReconciliationHistoryAsync(
            Guid accountId, string userId,
            ReconciliationStatus previousStatus, ReconciliationStatus newStatus,
            string notes)
        {
            // Placeholder for history logging - would require a separate history table
            _logger.LogInformation(
                "Reconciliation status changed for account {AccountId}: {PreviousStatus} -> {NewStatus} by {User}. Notes: {Notes}",
                accountId, previousStatus, newStatus, userId, notes ?? "None");

            await Task.CompletedTask;
        }

        #endregion
    }
}
