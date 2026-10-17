using System;
using System.Threading.Tasks;
using ZR.Model.Finance.GL;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// GL Account Auto Creator Interface
    /// Used by Treasury module to auto-create GL accounts when creating bank/cash accounts
    /// واجهة إنشاء حسابات دفتر الأستاذ تلقائياً
    /// </summary>
    public interface IGLAccountAutoCreator
    {
        /// <summary>
        /// Creates a GL Account for a new bank account
        /// إنشاء حساب دفتر أستاذ للحساب البنكي الجديد
        /// </summary>
        /// <param name="bankNameAr">Bank name in Arabic</param>
        /// <param name="bankNameEn">Bank name in English</param>
        /// <param name="currency">Currency code (e.g., SAR)</param>
        /// <param name="companyId">Company/Tenant ID</param>
        /// <param name="createdBy">User creating the account</param>
        /// <param name="parentAccountId">Optional: Parent GL account ID (uses default if not provided)</param>
        /// <param name="customAccountCode">Optional: Custom account code (auto-generated if not provided)</param>
        /// <returns>The created GL Account</returns>
        Task<GLAccount> CreateBankGLAccountAsync(
            string bankNameAr,
            string bankNameEn,
            string currency,
            long? companyId,
            string createdBy,
            Guid? parentAccountId = null,
            string customAccountCode = null);

        /// <summary>
        /// Creates a GL Account for a new cash account
        /// إنشاء حساب دفتر أستاذ للحساب النقدي الجديد
        /// </summary>
        Task<GLAccount> CreateCashGLAccountAsync(
            string cashAccountNameAr,
            string cashAccountNameEn,
            string currency,
            long? companyId,
            string createdBy,
            Guid? parentAccountId = null,
            string customAccountCode = null);

        /// <summary>
        /// Generates the next available account code under a parent
        /// توليد رقم الحساب التالي المتاح تحت الحساب الأب
        /// </summary>
        /// <param name="parentAccountId">Parent account ID</param>
        /// <param name="companyId">Company/Tenant ID</param>
        /// <returns>Generated account code</returns>
        Task<string> GenerateNextAccountCodeAsync(Guid parentAccountId, long? companyId);

        /// <summary>
        /// Gets or creates the default parent account for bank GL accounts
        /// Uses account number "1112000000" as the standard Bank Accounts parent
        /// الحصول على أو إنشاء حساب الأصل الافتراضي للحسابات البنكية
        /// </summary>
        Task<GLAccount> GetOrCreateBankParentAccountAsync(long? companyId, string createdBy);

        /// <summary>
        /// Gets or creates the default parent account for cash GL accounts
        /// Uses account number "1111000000" as the standard Cash Boxes parent
        /// الحصول على أو إنشاء حساب الأصل الافتراضي للحسابات النقدية
        /// </summary>
        Task<GLAccount> GetOrCreateCashParentAccountAsync(long? companyId, string createdBy);

        /// <summary>
        /// Gets parent GL accounts suitable for bank account placement
        /// Returns non-leaf Asset accounts under Cash & Equivalents
        /// الحصول على حسابات الأصل المناسبة لوضع الحسابات البنكية
        /// </summary>
        Task<System.Collections.Generic.List<GLAccount>> GetBankParentAccountOptionsAsync(long? companyId);

        /// <summary>
        /// Gets parent GL accounts suitable for cash account placement
        /// Returns non-leaf Asset accounts under Cash & Equivalents (Cash Box hierarchy)
        /// الحصول على حسابات الأصل المناسبة لوضع الحسابات النقدية
        /// </summary>
        Task<System.Collections.Generic.List<GLAccount>> GetCashParentAccountOptionsAsync(long? companyId);

        /// <summary>
        /// Validates if a GL account can be used as a parent for bank accounts
        /// التحقق من إمكانية استخدام حساب كأصل للحسابات البنكية
        /// </summary>
        Task<bool> ValidateBankParentAccountAsync(Guid parentAccountId);
    }
}
