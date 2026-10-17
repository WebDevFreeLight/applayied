using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// GL Account Service Interface - واجهة خدمة الحسابات
    /// </summary>
    public interface IGLAccountService : IBaseService<GLAccount>
    {
        #region Account CRUD Operations

        /// <summary>
        /// Get paginated list of accounts
        /// </summary>
        PagedInfo<AccountResponseDto> GetList(GLAccountQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get account by ID
        /// </summary>
        AccountResponseDto GetById(Guid id);

        /// <summary>
        /// Get account by ID async
        /// </summary>
        Task<AccountResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Get account by code
        /// </summary>
        AccountResponseDto GetByAccountNumber(string accountNumber);

        /// <summary>
        /// Get account by code async
        /// </summary>
        Task<AccountResponseDto> GetByAccountNumberAsync(string accountNumber);

        /// <summary>
        /// Create account
        /// </summary>
        GLAccount Create(CreateAccountDto dto, string userName);

        /// <summary>
        /// Create account async
        /// </summary>
        Task<GLAccount> CreateAsync(CreateAccountDto dto, string userName);

        /// <summary>
        /// Update account
        /// </summary>
        int Update(UpdateAccountDto dto, string userName);

        /// <summary>
        /// Update account async
        /// </summary>
        Task<int> UpdateAsync(UpdateAccountDto dto, string userName);

        /// <summary>
        /// Delete accounts (soft delete)
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Delete accounts async
        /// </summary>
        Task<int> DeleteAsync(Guid[] ids);

        #endregion

        #region Account Query Operations

        /// <summary>
        /// Get account tree structure
        /// </summary>
        List<AccountTreeNodeDto> GetAccountTree();

        /// <summary>
        /// Get account tree async
        /// </summary>
        Task<List<AccountTreeNodeDto>> GetAccountTreeAsync();

        /// <summary>
        /// Get accounts by type
        /// </summary>
        List<AccountResponseDto> GetByType(AccountType accountType);

        /// <summary>
        /// Get accounts by type async
        /// </summary>
        Task<List<AccountResponseDto>> GetByTypeAsync(AccountType accountType);

        /// <summary>
        /// Get leaf accounts (postable accounts)
        /// </summary>
        List<AccountResponseDto> GetLeafAccounts();

        /// <summary>
        /// Get leaf accounts async
        /// </summary>
        Task<List<AccountResponseDto>> GetLeafAccountsAsync();

        /// <summary>
        /// Get child accounts
        /// </summary>
        List<AccountResponseDto> GetChildAccounts(Guid parentAccountId);

        /// <summary>
        /// Get child accounts async
        /// </summary>
        Task<List<AccountResponseDto>> GetChildAccountsAsync(Guid parentAccountId);

        /// <summary>
        /// Get cash accounts
        /// </summary>
        List<AccountResponseDto> GetCashAccounts();

        /// <summary>
        /// Get bank accounts
        /// </summary>
        List<AccountResponseDto> GetBankAccounts();

        /// <summary>
        /// Search accounts
        /// </summary>
        List<AccountResponseDto> Search(string searchTerm, int maxResults = 20);

        /// <summary>
        /// Search accounts async
        /// </summary>
        Task<List<AccountResponseDto>> SearchAsync(string searchTerm, int maxResults = 20);

        #endregion

        #region Account Balance Operations

        /// <summary>
        /// Get account balance for fiscal year
        /// </summary>
        AccountBalanceDto GetAccountBalance(Guid accountId, int fiscalYear, DateTime? asOfDate = null);

        /// <summary>
        /// Get account balance async
        /// </summary>
        Task<AccountBalanceDto> GetAccountBalanceAsync(Guid accountId, int fiscalYear, DateTime? asOfDate = null);

        /// <summary>
        /// Get account balance including sub-accounts
        /// </summary>
        AccountBalanceDto GetAggregatedBalance(Guid accountId, int fiscalYear, DateTime? asOfDate = null);

        /// <summary>
        /// Get account balance including sub-accounts async
        /// </summary>
        Task<AccountBalanceDto> GetAggregatedBalanceAsync(Guid accountId, int fiscalYear, DateTime? asOfDate = null);

        /// <summary>
        /// Get balance summary for an account
        /// </summary>
        BalanceSummaryResponseDto GetBalanceSummary(BalanceSummaryQueryDto query);

        /// <summary>
        /// Get balance summary async
        /// </summary>
        Task<BalanceSummaryResponseDto> GetBalanceSummaryAsync(BalanceSummaryQueryDto query);

        /// <summary>
        /// Recalculate balance for account and period
        /// </summary>
        Task RecalculateBalanceAsync(Guid accountId, int fiscalYear, int fiscalPeriod);

        #endregion

        #region Opening Balance Operations

        /// <summary>
        /// Get opening balances for fiscal year
        /// </summary>
        PagedInfo<OpeningBalanceResponseDto> GetOpeningBalances(OpeningBalanceQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get opening balance by ID
        /// </summary>
        OpeningBalanceResponseDto GetOpeningBalanceById(Guid id);

        /// <summary>
        /// Get opening balance by ID async
        /// </summary>
        Task<OpeningBalanceResponseDto> GetOpeningBalanceByIdAsync(Guid id);

        /// <summary>
        /// Create opening balance
        /// </summary>
        GLAccountOpeningBalance CreateOpeningBalance(CreateOpeningBalanceDto dto, string userName);

        /// <summary>
        /// Create opening balance async
        /// </summary>
        Task<GLAccountOpeningBalance> CreateOpeningBalanceAsync(CreateOpeningBalanceDto dto, string userName);

        /// <summary>
        /// Update opening balance
        /// </summary>
        int UpdateOpeningBalance(UpdateOpeningBalanceDto dto, string userName);

        /// <summary>
        /// Update opening balance async
        /// </summary>
        Task<int> UpdateOpeningBalanceAsync(UpdateOpeningBalanceDto dto, string userName);

        /// <summary>
        /// Delete opening balance
        /// </summary>
        int DeleteOpeningBalance(Guid id);

        /// <summary>
        /// Delete opening balance async
        /// </summary>
        Task<int> DeleteOpeningBalanceAsync(Guid id);

        /// <summary>
        /// Import bulk opening balances
        /// </summary>
        Task<int> ImportOpeningBalancesAsync(BulkOpeningBalanceDto dto, string userName);

        /// <summary>
        /// Lock opening balances for fiscal year
        /// </summary>
        Task<int> LockOpeningBalancesAsync(int fiscalYear, Guid lockedBy);

        /// <summary>
        /// Carryforward opening balances to new fiscal year
        /// </summary>
        Task<int> CarryforwardOpeningBalancesAsync(int newFiscalYear, int previousFiscalYear, string createdBy);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validate account for posting
        /// </summary>
        AccountValidationResultDto ValidateForPosting(Guid accountId, bool isManualEntry = true);

        /// <summary>
        /// Validate account for posting async
        /// </summary>
        Task<AccountValidationResultDto> ValidateForPostingAsync(Guid accountId, bool isManualEntry = true);

        /// <summary>
        /// Validate account number uniqueness
        /// </summary>
        bool IsAccountNumberUnique(string accountNumber, Guid? excludeAccountId = null);

        /// <summary>
        /// Validate account number uniqueness async
        /// </summary>
        Task<bool> IsAccountNumberUniqueAsync(string accountNumber, Guid? excludeAccountId = null);

        /// <summary>
        /// Validate account hierarchy (no circular references)
        /// </summary>
        bool ValidateHierarchy(Guid accountId, Guid? newParentId);

        /// <summary>
        /// Validate account hierarchy async
        /// </summary>
        Task<bool> ValidateHierarchyAsync(Guid accountId, Guid? newParentId);

        /// <summary>
        /// Validate account can be deleted
        /// </summary>
        AccountValidationResultDto ValidateForDeletion(Guid accountId);

        /// <summary>
        /// Validate account can be deleted async
        /// </summary>
        Task<AccountValidationResultDto> ValidateForDeletionAsync(Guid accountId);

        /// <summary>
        /// Validate opening balance totals (debits = credits)
        /// </summary>
        bool ValidateOpeningBalanceTotals(int fiscalYear);

        /// <summary>
        /// Validate opening balance totals async
        /// </summary>
        Task<bool> ValidateOpeningBalanceTotalsAsync(int fiscalYear);

        #endregion

        #region Hierarchy Operations

        /// <summary>
        /// Get account hierarchy path
        /// </summary>
        List<AccountResponseDto> GetHierarchyPath(Guid accountId);

        /// <summary>
        /// Recalculate all account paths
        /// </summary>
        Task RecalculateAllPathsAsync();

        /// <summary>
        /// Get normal balance for account type
        /// </summary>
        NormalBalance GetDefaultNormalBalance(AccountType accountType);

        #endregion

        #region Legacy Compatibility

        /// <summary>
        /// Check if account code exists (legacy compatibility)
        /// </summary>
        bool CodeExists(string accountNumber, Guid? excludeAccountId = null);

        #endregion

        #region Excel Import Operations

        /// <summary>
        /// Import accounts from Excel file stream
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="updateExisting">Whether to update existing accounts</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Import result with success/failure counts</returns>
        Task<ExcelImportResultDto> ImportFromExcelAsync(System.IO.Stream fileStream, bool updateExisting, string userName);

        /// <summary>
        /// Rebuild account hierarchy - fixes ParentAccountId and IsLeaf based on Level structure
        /// </summary>
        Task RebuildAccountHierarchyAsync();

        /// <summary>
        /// Fix NormalBalance for all accounts based on their AccountType
        /// This corrects accounts that were imported with incorrect NormalBalance values
        /// إصلاح الرصيد الطبيعي لجميع الحسابات بناءً على نوع الحساب
        /// </summary>
        /// <returns>Number of accounts fixed</returns>
        Task<int> FixAccountNormalBalancesAsync();

        #endregion
    }
}
