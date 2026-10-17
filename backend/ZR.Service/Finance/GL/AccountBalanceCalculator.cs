using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.GL;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// High-level account balance calculator that applies normalBalance rules.
    /// حاسبة أرصدة الحسابات عالية المستوى التي تطبق قواعد الرصيد الطبيعي
    /// </summary>
    [AppService(ServiceType = typeof(IAccountBalanceCalculator), ServiceLifetime = LifeTime.Transient)]
    public class AccountBalanceCalculator : IAccountBalanceCalculator
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLJournalEntryRepository _entryRepository;
        private readonly INormalBalanceResolver _normalBalanceResolver;

        public AccountBalanceCalculator(
            IGLAccountRepository accountRepository,
            IGLJournalEntryRepository entryRepository,
            INormalBalanceResolver normalBalanceResolver)
        {
            _accountRepository = accountRepository;
            _entryRepository = entryRepository;
            _normalBalanceResolver = normalBalanceResolver;
        }

        /// <inheritdoc />
        public async Task<AccountBalanceResultDto> GetAccountBalanceAsync(
            Guid accountId,
            DateTime startDate,
            DateTime endDate)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                return null;

            var rawBalance = await _entryRepository.GetAccountBalanceAsync(accountId, startDate, endDate);

            var normalizedBalance = _normalBalanceResolver.CalculateBalance(
                rawBalance.TotalDebit,
                rawBalance.TotalCredit,
                account.NormalBalance);

            return new AccountBalanceResultDto
            {
                AccountId = account.AccountId,
                AccountCode = account.FullAccountNumber,
                AccountNameEn = account.AccountNameEn,
                AccountNameAr = account.AccountNameAr,
                AccountType = account.AccountType,
                NormalBalance = account.NormalBalance,
                TotalDebit = rawBalance.TotalDebit,
                TotalCredit = rawBalance.TotalCredit,
                Balance = normalizedBalance,
                IsNormalDirection = _normalBalanceResolver.IsBalanceNormal(normalizedBalance, account.NormalBalance)
            };
        }

        /// <inheritdoc />
        public async Task<List<AccountBalanceResultDto>> GetAccountBalancesAsync(
            IEnumerable<Guid> accountIds,
            DateTime startDate,
            DateTime endDate)
        {
            var results = new List<AccountBalanceResultDto>();

            foreach (var accountId in accountIds)
            {
                var balance = await GetAccountBalanceAsync(accountId, startDate, endDate);
                if (balance != null)
                {
                    results.Add(balance);
                }
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<decimal> GetTotalBalanceByTypeAsync(
            AccountType accountType,
            DateTime startDate,
            DateTime endDate,
            long? companyId = null)
        {
            var accounts = await _accountRepository.GetByTypeAsync(accountType);

            if (companyId.HasValue)
            {
                accounts = accounts.Where(a => a.CompanyID == companyId.Value).ToList();
            }

            decimal totalBalance = 0;

            foreach (var account in accounts.Where(a => a.IsActive && a.IsLeaf))
            {
                var rawBalance = await _entryRepository.GetAccountBalanceAsync(account.AccountId, startDate, endDate);
                var normalizedBalance = _normalBalanceResolver.CalculateBalance(
                    rawBalance.TotalDebit,
                    rawBalance.TotalCredit,
                    account.NormalBalance);
                totalBalance += normalizedBalance;
            }

            return totalBalance;
        }

        /// <inheritdoc />
        public async Task<AccountBalanceResultDto> GetBalanceSummaryAsync(
            Guid accountId,
            DateTime asOfDate,
            long? companyId = null)
        {
            // Use a very early start date to get cumulative balance
            var startDate = new DateTime(2000, 1, 1);
            return await GetAccountBalanceAsync(accountId, startDate, asOfDate);
        }
    }
}
