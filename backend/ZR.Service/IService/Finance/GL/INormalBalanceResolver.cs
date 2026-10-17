using ZR.Model.Finance.Enums;
using ZR.Model.Finance.GL;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// Centralized resolver for normalBalance-aware balance calculations.
    /// All financial calculations MUST use this interface.
    /// جميع الحسابات المالية يجب أن تستخدم هذه الواجهة
    /// </summary>
    public interface INormalBalanceResolver
    {
        /// <summary>
        /// Calculate account balance respecting normalBalance direction.
        /// حساب رصيد الحساب مع احترام اتجاه الرصيد الطبيعي
        /// </summary>
        /// <param name="totalDebit">Sum of all debits - مجموع المدين</param>
        /// <param name="totalCredit">Sum of all credits - مجموع الدائن</param>
        /// <param name="normalBalance">Account's normal balance direction - اتجاه الرصيد الطبيعي</param>
        /// <returns>Calculated balance (positive = normal direction)</returns>
        decimal CalculateBalance(decimal totalDebit, decimal totalCredit, NormalBalance normalBalance);

        /// <summary>
        /// Calculate account balance using account entity.
        /// </summary>
        decimal CalculateBalance(decimal totalDebit, decimal totalCredit, GLAccount account);

        /// <summary>
        /// Determine if a balance is in the normal direction (positive).
        /// </summary>
        bool IsBalanceNormal(decimal balance, NormalBalance normalBalance);

        /// <summary>
        /// Get the debit/credit amounts needed to zero out an account.
        /// الحصول على مبالغ المدين/الدائن اللازمة لإقفال الحساب
        /// </summary>
        (decimal Debit, decimal Credit) GetClosingEntry(decimal currentBalance, NormalBalance normalBalance);

        /// <summary>
        /// Get default normalBalance for an account type.
        /// </summary>
        NormalBalance GetDefaultNormalBalance(AccountType accountType);

        /// <summary>
        /// Validate that normalBalance matches expected for AccountType.
        /// Returns false for non-standard configurations (e.g., ContraAsset with Debit normal).
        /// </summary>
        bool ValidateNormalBalance(AccountType accountType, NormalBalance normalBalance);
    }
}
