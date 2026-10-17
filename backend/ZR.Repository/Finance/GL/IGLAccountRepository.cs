using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Account Repository Interface - واجهة مستودع الحسابات
    /// </summary>
    public interface IGLAccountRepository : IBaseRepository<GLAccount>
    {
        #region Basic CRUD

        /// <summary>
        /// Get paginated list of accounts - الحصول على قائمة الحسابات بالترقيم
        /// </summary>
        PagedInfo<GLAccount> GetList(GLAccount parm, PagerInfo pager);

        /// <summary>
        /// Get account by ID - الحصول على الحساب بالمعرف
        /// </summary>
        GLAccount GetById(Guid id);

        /// <summary>
        /// Get account by ID async
        /// </summary>
        Task<GLAccount> GetByIdAsync(Guid id);

        /// <summary>
        /// Get account by code - الحصول على الحساب بالرمز
        /// </summary>
        GLAccount GetByAccountNumber(string accountNumber);

        /// <summary>
        /// Get account by code async
        /// </summary>
        Task<GLAccount> GetByAccountNumberAsync(string accountNumber);

        /// <summary>
        /// Insert account - إضافة حساب
        /// </summary>
        int InsertAccount(GLAccount model);

        /// <summary>
        /// Insert account async
        /// </summary>
        Task<int> InsertAccountAsync(GLAccount model);

        /// <summary>
        /// Update account - تحديث حساب
        /// </summary>
        int UpdateAccount(GLAccount model);

        /// <summary>
        /// Update account async
        /// </summary>
        Task<int> UpdateAccountAsync(GLAccount model);

        /// <summary>
        /// Delete accounts by IDs - حذف حسابات بالمعرفات (soft delete)
        /// </summary>
        int DeleteByIds(Guid[] ids);

        /// <summary>
        /// Delete accounts by IDs async
        /// </summary>
        Task<int> DeleteByIdsAsync(Guid[] ids);

        #endregion

        #region Query Methods

        /// <summary>
        /// Get all active accounts - الحصول على جميع الحسابات النشطة
        /// </summary>
        List<GLAccount> GetActiveAccounts();

        /// <summary>
        /// Get all active accounts async
        /// </summary>
        Task<List<GLAccount>> GetActiveAccountsAsync();

        /// <summary>
        /// Get accounts by type - الحصول على الحسابات بالنوع
        /// </summary>
        List<GLAccount> GetByType(AccountType accountType);

        /// <summary>
        /// Get accounts by type async
        /// </summary>
        Task<List<GLAccount>> GetByTypeAsync(AccountType accountType);

        /// <summary>
        /// Get child accounts - الحصول على الحسابات الفرعية
        /// </summary>
        List<GLAccount> GetChildAccounts(Guid parentAccountId);

        /// <summary>
        /// Get child accounts async
        /// </summary>
        Task<List<GLAccount>> GetChildAccountsAsync(Guid parentAccountId);

        /// <summary>
        /// Get all descendants of an account - الحصول على جميع الحسابات التابعة
        /// </summary>
        List<GLAccount> GetAllDescendants(Guid accountId);

        /// <summary>
        /// Get all descendants async
        /// </summary>
        Task<List<GLAccount>> GetAllDescendantsAsync(Guid accountId);

        /// <summary>
        /// Get root accounts (no parent) - الحصول على الحسابات الرئيسية
        /// </summary>
        List<GLAccount> GetRootAccounts();

        /// <summary>
        /// Get root accounts async
        /// </summary>
        Task<List<GLAccount>> GetRootAccountsAsync();

        /// <summary>
        /// Get leaf accounts only - الحصول على الحسابات النهائية فقط
        /// </summary>
        List<GLAccount> GetLeafAccounts();

        /// <summary>
        /// Get leaf accounts async
        /// </summary>
        Task<List<GLAccount>> GetLeafAccountsAsync();

        /// <summary>
        /// Get cash accounts - الحصول على الحسابات النقدية
        /// </summary>
        List<GLAccount> GetCashAccounts();

        /// <summary>
        /// Get bank accounts - الحصول على الحسابات البنكية
        /// </summary>
        List<GLAccount> GetBankAccounts();

        #endregion

        #region Validation Methods

        /// <summary>
        /// Check if account code exists - التحقق من وجود رقم الحساب
        /// </summary>
        bool CodeExists(string accountNumber, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if account code exists async
        /// </summary>
        Task<bool> CodeExistsAsync(string accountNumber, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if account number exists under the same parent (sibling uniqueness)
        /// </summary>
        bool AccountNumberExistsUnderParent(string accountNumber, Guid? parentAccountId, long? companyId, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if account number exists under the same parent async
        /// </summary>
        Task<bool> AccountNumberExistsUnderParentAsync(string accountNumber, Guid? parentAccountId, long? companyId, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if full account number exists - التحقق من وجود رقم الحساب الكامل
        /// </summary>
        bool FullAccountNumberExists(string fullAccountNumber, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if full account number exists async
        /// </summary>
        Task<bool> FullAccountNumberExistsAsync(string fullAccountNumber, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if full account number exists within a company
        /// </summary>
        bool FullAccountNumberExistsInCompany(string fullAccountNumber, long? companyId, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if full account number exists within a company async
        /// </summary>
        Task<bool> FullAccountNumberExistsInCompanyAsync(string fullAccountNumber, long? companyId, Guid? excludeAccountId = null);

        /// <summary>
        /// Check if account has children - التحقق من وجود حسابات فرعية
        /// </summary>
        bool HasChildren(Guid accountId);

        /// <summary>
        /// Check if account has children async
        /// </summary>
        Task<bool> HasChildrenAsync(Guid accountId);

        /// <summary>
        /// Check if account has transactions - التحقق من وجود حركات
        /// </summary>
        bool HasTransactions(Guid accountId);

        /// <summary>
        /// Check if account has transactions async
        /// </summary>
        Task<bool> HasTransactionsAsync(Guid accountId);

        /// <summary>
        /// Validate circular reference - التحقق من المرجع الدائري
        /// </summary>
        bool WouldCreateCircularReference(Guid accountId, Guid? newParentId);

        /// <summary>
        /// Validate circular reference async
        /// </summary>
        Task<bool> WouldCreateCircularReferenceAsync(Guid accountId, Guid? newParentId);

        #endregion

        #region Hierarchy Operations

        /// <summary>
        /// Update IsLeaf flag for parent - تحديث علامة الحساب النهائي للأب
        /// </summary>
        int UpdateParentIsLeaf(Guid parentAccountId, bool isLeaf);

        /// <summary>
        /// Update IsLeaf flag for parent async
        /// </summary>
        Task<int> UpdateParentIsLeafAsync(Guid parentAccountId, bool isLeaf);

        /// <summary>
        /// Update account path - تحديث مسار الحساب
        /// </summary>
        int UpdateAccountPath(Guid accountId, string path);

        /// <summary>
        /// Update account path async
        /// </summary>
        Task<int> UpdateAccountPathAsync(Guid accountId, string path);

        /// <summary>
        /// Recalculate all account paths - إعادة حساب جميع مسارات الحسابات
        /// </summary>
        Task RecalculateAllPathsAsync();

        /// <summary>
        /// Get account hierarchy path - الحصول على مسار التسلسل الهرمي
        /// </summary>
        List<GLAccount> GetHierarchyPath(Guid accountId);

        #endregion

        #region List Operations

        /// <summary>
        /// Get all accounts with optional company filter - الحصول على جميع الحسابات
        /// </summary>
        List<GLAccount> ListAll(long? companyId = null);

        /// <summary>
        /// Get all accounts with optional company filter async - الحصول على جميع الحسابات (غير متزامن)
        /// </summary>
        Task<List<GLAccount>> ListAllAsync(long? companyId = null);

        #endregion

        #region Transaction Date Updates

        /// <summary>
        /// Update last transaction date - تحديث تاريخ آخر عملية
        /// </summary>
        int UpdateLastTransactionDate(Guid accountId, DateTime transactionDate);

        /// <summary>
        /// Update last transaction date async
        /// </summary>
        Task<int> UpdateLastTransactionDateAsync(Guid accountId, DateTime transactionDate);

        #endregion

        #region Count Operations

        /// <summary>
        /// Increment child count for parent account
        /// </summary>
        int IncrementChildCount(Guid parentAccountId);

        /// <summary>
        /// Increment child count for parent account async
        /// </summary>
        Task<int> IncrementChildCountAsync(Guid parentAccountId);

        /// <summary>
        /// Decrement child count for parent account
        /// </summary>
        int DecrementChildCount(Guid parentAccountId);

        /// <summary>
        /// Decrement child count for parent account async
        /// </summary>
        Task<int> DecrementChildCountAsync(Guid parentAccountId);

        /// <summary>
        /// Increment descendant count for all ancestors
        /// </summary>
        int IncrementAncestorDescendantCounts(Guid accountId);

        /// <summary>
        /// Increment descendant count for all ancestors async
        /// </summary>
        Task<int> IncrementAncestorDescendantCountsAsync(Guid accountId);

        /// <summary>
        /// Decrement descendant count for all ancestors
        /// </summary>
        int DecrementAncestorDescendantCounts(Guid accountId, int descendantCount);

        /// <summary>
        /// Decrement descendant count for all ancestors async
        /// </summary>
        Task<int> DecrementAncestorDescendantCountsAsync(Guid accountId, int descendantCount);

        /// <summary>
        /// Recalculate all child and descendant counts
        /// </summary>
        Task RecalculateAllCountsAsync();

        #endregion
    }
}
