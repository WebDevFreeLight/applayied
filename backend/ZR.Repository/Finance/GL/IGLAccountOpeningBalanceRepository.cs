using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Account Opening Balance Repository Interface - واجهة مستودع الأرصدة الافتتاحية
    /// </summary>
    public interface IGLAccountOpeningBalanceRepository : IBaseRepository<GLAccountOpeningBalance>
    {
        #region Basic CRUD

        /// <summary>
        /// Get paginated list of opening balances
        /// </summary>
        PagedInfo<GLAccountOpeningBalance> GetList(GLAccountOpeningBalance parm, PagerInfo pager);

        /// <summary>
        /// Get opening balance by ID
        /// </summary>
        GLAccountOpeningBalance GetById(Guid id);

        /// <summary>
        /// Get opening balance by ID async
        /// </summary>
        Task<GLAccountOpeningBalance> GetByIdAsync(Guid id);

        /// <summary>
        /// Get opening balance for account and fiscal year
        /// </summary>
        GLAccountOpeningBalance GetByAccountAndYear(Guid accountId, int fiscalYear);

        /// <summary>
        /// Get opening balance for account and fiscal year async
        /// </summary>
        Task<GLAccountOpeningBalance> GetByAccountAndYearAsync(Guid accountId, int fiscalYear);

        /// <summary>
        /// Insert opening balance
        /// </summary>
        int InsertOpeningBalance(GLAccountOpeningBalance model);

        /// <summary>
        /// Insert opening balance async
        /// </summary>
        Task<int> InsertOpeningBalanceAsync(GLAccountOpeningBalance model);

        /// <summary>
        /// Insert multiple opening balances
        /// </summary>
        int InsertBulk(List<GLAccountOpeningBalance> models);

        /// <summary>
        /// Insert multiple opening balances async
        /// </summary>
        Task<int> InsertBulkAsync(List<GLAccountOpeningBalance> models);

        /// <summary>
        /// Update opening balance
        /// </summary>
        int UpdateOpeningBalance(GLAccountOpeningBalance model);

        /// <summary>
        /// Update opening balance async
        /// </summary>
        Task<int> UpdateOpeningBalanceAsync(GLAccountOpeningBalance model);

        /// <summary>
        /// Delete opening balance
        /// </summary>
        int DeleteById(Guid id);

        /// <summary>
        /// Delete opening balance async
        /// </summary>
        Task<int> DeleteByIdAsync(Guid id);

        #endregion

        #region Query Methods

        /// <summary>
        /// Get opening balances by fiscal year
        /// </summary>
        List<GLAccountOpeningBalance> GetByFiscalYear(int fiscalYear);

        /// <summary>
        /// Get opening balances by fiscal year async
        /// </summary>
        Task<List<GLAccountOpeningBalance>> GetByFiscalYearAsync(int fiscalYear);

        /// <summary>
        /// Get opening balances by account ID
        /// </summary>
        List<GLAccountOpeningBalance> GetByAccountId(Guid accountId);

        /// <summary>
        /// Get opening balances by account ID async
        /// </summary>
        Task<List<GLAccountOpeningBalance>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Get all opening balances with account details for fiscal year
        /// </summary>
        List<GLAccountOpeningBalance> GetWithAccountDetails(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get all opening balances with account details async
        /// </summary>
        Task<List<GLAccountOpeningBalance>> GetWithAccountDetailsAsync(int fiscalYear, long? companyId = null);

        #endregion

        #region Validation Methods

        /// <summary>
        /// Check if opening balance exists for account and year
        /// </summary>
        bool Exists(Guid accountId, int fiscalYear);

        /// <summary>
        /// Check if opening balance exists async
        /// </summary>
        Task<bool> ExistsAsync(Guid accountId, int fiscalYear);

        /// <summary>
        /// Check if any opening balances are locked for fiscal year
        /// </summary>
        bool HasLockedBalances(int fiscalYear);

        /// <summary>
        /// Check if any opening balances are locked async
        /// </summary>
        Task<bool> HasLockedBalancesAsync(int fiscalYear);

        #endregion

        #region Lock Operations

        /// <summary>
        /// Lock opening balance
        /// </summary>
        int Lock(Guid id, Guid lockedBy);

        /// <summary>
        /// Lock opening balance async
        /// </summary>
        Task<int> LockAsync(Guid id, Guid lockedBy);

        /// <summary>
        /// Lock all opening balances for fiscal year
        /// </summary>
        int LockByFiscalYear(int fiscalYear, Guid lockedBy);

        /// <summary>
        /// Lock all opening balances for fiscal year async
        /// </summary>
        Task<int> LockByFiscalYearAsync(int fiscalYear, Guid lockedBy);

        /// <summary>
        /// Unlock opening balance (admin only)
        /// </summary>
        int Unlock(Guid id);

        /// <summary>
        /// Unlock opening balance async
        /// </summary>
        Task<int> UnlockAsync(Guid id);

        #endregion

        #region Year-End Operations

        /// <summary>
        /// Create opening balances from previous year closing
        /// </summary>
        Task<int> CreateFromPreviousYearClosingAsync(int newFiscalYear, int previousFiscalYear, long? companyId, string createdBy);

        /// <summary>
        /// Get total debit and credit for fiscal year
        /// </summary>
        (decimal TotalDebit, decimal TotalCredit) GetTotalsByFiscalYear(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get total debit and credit for fiscal year async
        /// </summary>
        Task<(decimal TotalDebit, decimal TotalCredit)> GetTotalsByFiscalYearAsync(int fiscalYear, long? companyId = null);

        #endregion
    }
}
