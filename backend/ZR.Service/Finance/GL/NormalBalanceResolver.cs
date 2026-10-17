using System;
using Infrastructure.Attribute;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.GL;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// Implementation of INormalBalanceResolver.
    /// Singleton lifetime for performance - no state, pure calculations.
    /// تنفيذ محلل الرصيد الطبيعي - خدمة مفردة للأداء
    /// </summary>
    [AppService(ServiceType = typeof(INormalBalanceResolver), ServiceLifetime = LifeTime.Singleton)]
    public class NormalBalanceResolver : INormalBalanceResolver
    {
        /// <inheritdoc />
        public decimal CalculateBalance(decimal totalDebit, decimal totalCredit, NormalBalance normalBalance)
        {
            // Debit-normal accounts: positive when Debit > Credit
            // Credit-normal accounts: positive when Credit > Debit
            // حسابات المدين الطبيعي: موجبة عندما المدين > الدائن
            // حسابات الدائن الطبيعي: موجبة عندما الدائن > المدين
            return normalBalance == NormalBalance.Debit
                ? totalDebit - totalCredit
                : totalCredit - totalDebit;
        }

        /// <inheritdoc />
        public decimal CalculateBalance(decimal totalDebit, decimal totalCredit, GLAccount account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            return CalculateBalance(totalDebit, totalCredit, account.NormalBalance);
        }

        /// <inheritdoc />
        public bool IsBalanceNormal(decimal balance, NormalBalance normalBalance)
        {
            // A positive balance indicates the account is in its normal state
            // A negative balance indicates abnormal state (e.g., overdrawn bank account)
            // الرصيد الموجب يعني أن الحساب في حالته الطبيعية
            return balance >= 0;
        }

        /// <inheritdoc />
        public (decimal Debit, decimal Credit) GetClosingEntry(decimal currentBalance, NormalBalance normalBalance)
        {
            // To close an account, we need to bring its balance to zero.
            // The closing entry is the opposite of the current balance.
            // لإقفال الحساب، نحتاج لجعل رصيده صفراً

            if (currentBalance == 0)
                return (0, 0);

            if (normalBalance == NormalBalance.Debit)
            {
                // Debit-normal account with positive balance: Credit to close
                // Debit-normal account with negative balance: Debit to close
                // حساب مدين طبيعي برصيد موجب: دائن للإقفال
                return currentBalance > 0
                    ? (0, currentBalance)
                    : (Math.Abs(currentBalance), 0);
            }
            else
            {
                // Credit-normal account with positive balance: Debit to close
                // Credit-normal account with negative balance: Credit to close
                // حساب دائن طبيعي برصيد موجب: مدين للإقفال
                return currentBalance > 0
                    ? (currentBalance, 0)
                    : (0, Math.Abs(currentBalance));
            }
        }

        /// <inheritdoc />
        public NormalBalance GetDefaultNormalBalance(AccountType accountType)
        {
            return accountType switch
            {
                // Debit-normal accounts (increase with debits)
                // حسابات المدين الطبيعي (تزداد بالمدين)
                AccountType.Asset => NormalBalance.Debit,
                AccountType.Expense => NormalBalance.Debit,
                AccountType.ContraLiability => NormalBalance.Debit,
                AccountType.ContraEquity => NormalBalance.Debit,
                AccountType.ContraRevenue => NormalBalance.Debit,

                // Credit-normal accounts (increase with credits)
                // حسابات الدائن الطبيعي (تزداد بالدائن)
                AccountType.Liability => NormalBalance.Credit,
                AccountType.Equity => NormalBalance.Credit,
                AccountType.Revenue => NormalBalance.Credit,
                AccountType.ContraAsset => NormalBalance.Credit,
                AccountType.OtherComprehensiveIncome => NormalBalance.Credit,

                // Default to Debit for unknown types
                _ => NormalBalance.Debit
            };
        }

        /// <inheritdoc />
        public bool ValidateNormalBalance(AccountType accountType, NormalBalance normalBalance)
        {
            var expected = GetDefaultNormalBalance(accountType);
            return normalBalance == expected;
        }
    }
}
