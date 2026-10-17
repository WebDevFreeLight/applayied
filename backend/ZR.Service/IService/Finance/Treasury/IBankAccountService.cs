using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Bank Account Service Interface - واجهة خدمة الحساب البنكي
    /// </summary>
    public interface IBankAccountService : IBaseService<BankAccount>
    {
        /// <summary>
        /// Get bank account list with paging - الحصول على قائمة الحسابات البنكية مع ترقيم الصفحات
        /// </summary>
        PagedInfo<BankAccountListDto> GetList(BankAccountQueryDto parm);

        /// <summary>
        /// Get bank account by ID - الحصول على حساب بنكي بالمعرف
        /// </summary>
        BankAccountDetailDto GetDetail(Guid id);

        /// <summary>
        /// Create bank account - إنشاء حساب بنكي
        /// </summary>
        BankAccount Create(CreateBankAccountDto dto, string userId);

        /// <summary>
        /// Update bank account - تحديث حساب بنكي
        /// </summary>
        int Update(UpdateBankAccountDto dto, string userId);

        /// <summary>
        /// Delete bank accounts - حذف حسابات بنكية
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Soft delete bank account with transaction check - حذف ناعم للحساب البنكي مع التحقق من المعاملات
        /// Blocks deletion if account has related payments or cheques
        /// </summary>
        int SoftDelete(Guid id, string userId);

        /// <summary>
        /// Soft delete multiple bank accounts with transaction check
        /// </summary>
        int SoftDelete(Guid[] ids, string userId);

        /// <summary>
        /// Check if account has related transactions - التحقق من وجود معاملات مرتبطة
        /// </summary>
        bool HasRelatedTransactions(Guid accountId);

        /// <summary>
        /// Check if account number exists - التحقق من وجود رقم الحساب
        /// </summary>
        bool AccountNumberExists(string accountNumber, Guid? excludeId = null, long? companyId = null);

        /// <summary>
        /// Get balance - الحصول على الرصيد
        /// </summary>
        BankAccountBalanceDto GetBalance(Guid accountId);

        /// <summary>
        /// Get pending reconciliations - الحصول على التسويات المعلقة
        /// </summary>
        List<BankReconciliation> GetPendingReconciliations(Guid accountId);

        /// <summary>
        /// Get active bank accounts for dropdowns - الحصول على الحسابات البنكية النشطة للقوائم
        /// </summary>
        List<BankAccountListDto> GetActiveAccounts(long? companyId = null);

        /// <summary>
        /// Create bank account with automatic GL account creation - إنشاء حساب بنكي مع إنشاء تلقائي لحساب دفتر الأستاذ
        /// If GLAccountId is not provided and AutoCreateGLAccount is true, creates a new GL account automatically.
        /// </summary>
        Task<BankAccount> CreateWithGLAccountAsync(CreateBankAccountDto dto, string userId);

        /// <summary>
        /// Get available parent GL accounts for bank account creation - الحصول على حسابات الأصل المتاحة
        /// Returns GL accounts suitable for placing new bank GL accounts under.
        /// </summary>
        Task<List<GLAccount>> GetBankParentAccountOptionsAsync(long? companyId = null);
    }
}






























