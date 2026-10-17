using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Account Repository - مستودع الحسابات
    /// </summary>
    [AppService(ServiceType = typeof(IGLAccountRepository), ServiceLifetime = LifeTime.Transient)]
    public class GLAccountRepository : BaseRepository<GLAccount>, IGLAccountRepository
    {
        #region Basic CRUD

        /// <summary>
        /// Get paginated list of accounts
        /// </summary>
        public PagedInfo<GLAccount> GetList(GLAccount parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AccountNumber), it => it.AccountNumber.Contains(parm.AccountNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AccountNameAr), it => it.AccountNameAr.Contains(parm.AccountNameAr));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AccountNameEn), it => it.AccountNameEn.Contains(parm.AccountNameEn));
            predicate = predicate.AndIF(parm.AccountType != default, it => it.AccountType == parm.AccountType);
            predicate = predicate.AndIF(parm.ParentAccountId.HasValue, it => it.ParentAccountId == parm.ParentAccountId);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.AccountNumber)
                .ToPage(pager);
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        public GLAccount GetById(Guid id)
        {
            return GetFirst(x => x.AccountId == id);
        }

        /// <summary>
        /// Get account by ID async
        /// </summary>
        public async Task<GLAccount> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.AccountId == id);
        }

        /// <summary>
        /// Get account by code
        /// </summary>
        public GLAccount GetByAccountNumber(string accountNumber)
        {
            return GetFirst(x => x.AccountNumber == accountNumber && x.IsActive);
        }

        /// <summary>
        /// Get account by code async
        /// </summary>
        public async Task<GLAccount> GetByAccountNumberAsync(string accountNumber)
        {
            return await GetFirstAsync(x => x.AccountNumber == accountNumber && x.IsActive);
        }

        /// <summary>
        /// Insert account
        /// </summary>
        public int InsertAccount(GLAccount model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Insert account async
        /// </summary>
        public async Task<int> InsertAccountAsync(GLAccount model)
        {
            return await InsertAsync(model) ? 1 : 0;
        }

        /// <summary>
        /// Update account
        /// </summary>
        public int UpdateAccount(GLAccount model)
        {
            return Update(model, it => new
            {
                it.AccountNameAr,
                it.AccountNameEn,
                it.AccountNumber,
                it.FullAccountNumber,
                it.AccountType,
                it.NormalBalance,
                it.Level,
                it.ParentAccountId,
                it.IsActive,
                it.IsLeaf,
                it.AccountPath,
                it.DisplayOrder,
                it.AllowManualEntries,
                it.AllowPostingToParent,
                it.DimensionRequirements,
                it.AccountNature,
                it.IsControlAccount,
                it.ReconciliationMode,
                it.Currency,
                it.IsForeignCurrency,
                it.DescriptionAr,
                it.DescriptionEn,
                it.ExternalCode,
                it.TaxCode,
                it.RowVersion,
                it.Update_by,
                it.Update_time
            }, f => f.AccountId == model.AccountId);
        }

        /// <summary>
        /// Update account async
        /// </summary>
        public async Task<int> UpdateAccountAsync(GLAccount model)
        {
            return await Context.Updateable(model)
                .UpdateColumns(it => new
                {
                    it.AccountNameAr,
                    it.AccountNameEn,
                    it.AccountNumber,
                    it.FullAccountNumber,
                    it.AccountType,
                    it.NormalBalance,
                    it.Level,
                    it.ParentAccountId,
                    it.IsActive,
                    it.IsLeaf,
                    it.AccountPath,
                    it.DisplayOrder,
                    it.AllowManualEntries,
                    it.AllowPostingToParent,
                    it.DimensionRequirements,
                    it.AccountNature,
                    it.IsControlAccount,
                    it.ReconciliationMode,
                    it.Currency,
                    it.IsForeignCurrency,
                    it.DescriptionAr,
                    it.DescriptionEn,
                    it.ExternalCode,
                    it.TaxCode,
                    it.RowVersion,
                    it.Update_by,
                    it.Update_time
                })
                .Where(f => f.AccountId == model.AccountId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete accounts by IDs (soft delete)
        /// </summary>
        public int DeleteByIds(Guid[] ids)
        {
            return Context.Updateable<GLAccount>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.AccountId))
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete accounts by IDs async
        /// </summary>
        public async Task<int> DeleteByIdsAsync(Guid[] ids)
        {
            return await Context.Updateable<GLAccount>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.AccountId))
                .ExecuteCommandAsync();
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Get all active accounts
        /// </summary>
        public List<GLAccount> GetActiveAccounts()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get all active accounts async
        /// </summary>
        public async Task<List<GLAccount>> GetActiveAccountsAsync()
        {
            return await Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Get accounts by type
        /// </summary>
        public List<GLAccount> GetByType(AccountType accountType)
        {
            return Queryable()
                .Where(x => x.AccountType == accountType && x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get accounts by type async
        /// </summary>
        public async Task<List<GLAccount>> GetByTypeAsync(AccountType accountType)
        {
            return await Queryable()
                .Where(x => x.AccountType == accountType && x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Get child accounts
        /// </summary>
        public List<GLAccount> GetChildAccounts(Guid parentAccountId)
        {
            return Queryable()
                .Where(x => x.ParentAccountId == parentAccountId && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get child accounts async
        /// </summary>
        public async Task<List<GLAccount>> GetChildAccountsAsync(Guid parentAccountId)
        {
            return await Queryable()
                .Where(x => x.ParentAccountId == parentAccountId && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .OrderBy(x => x.AccountNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Get all descendants of an account
        /// </summary>
        public List<GLAccount> GetAllDescendants(Guid accountId)
        {
            var account = GetById(accountId);
            if (account == null || string.IsNullOrEmpty(account.AccountPath))
            {
                return GetChildAccountsRecursive(accountId);
            }

            // Use materialized path for efficient lookup
            var pathPrefix = account.AccountPath + "/";
            return Queryable()
                .Where(x => x.AccountPath.StartsWith(pathPrefix) && x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get all descendants async
        /// </summary>
        public async Task<List<GLAccount>> GetAllDescendantsAsync(Guid accountId)
        {
            var account = await GetByIdAsync(accountId);
            if (account == null || string.IsNullOrEmpty(account.AccountPath))
            {
                return await GetChildAccountsRecursiveAsync(accountId);
            }

            var pathPrefix = account.AccountPath + "/";
            return await Queryable()
                .Where(x => x.AccountPath.StartsWith(pathPrefix) && x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToListAsync();
        }

        private List<GLAccount> GetChildAccountsRecursive(Guid parentAccountId)
        {
            var result = new List<GLAccount>();
            var children = GetChildAccounts(parentAccountId);
            foreach (var child in children)
            {
                result.Add(child);
                result.AddRange(GetChildAccountsRecursive(child.AccountId));
            }
            return result;
        }

        private async Task<List<GLAccount>> GetChildAccountsRecursiveAsync(Guid parentAccountId)
        {
            var result = new List<GLAccount>();
            var children = await GetChildAccountsAsync(parentAccountId);
            foreach (var child in children)
            {
                result.Add(child);
                result.AddRange(await GetChildAccountsRecursiveAsync(child.AccountId));
            }
            return result;
        }

        /// <summary>
        /// Get root accounts (no parent)
        /// </summary>
        public List<GLAccount> GetRootAccounts()
        {
            return Queryable()
                .Where(x => x.ParentAccountId == null && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get root accounts async
        /// </summary>
        public async Task<List<GLAccount>> GetRootAccountsAsync()
        {
            return await Queryable()
                .Where(x => x.ParentAccountId == null && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .OrderBy(x => x.AccountNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Get leaf accounts only
        /// </summary>
        public List<GLAccount> GetLeafAccounts()
        {
            return Queryable()
                .Where(x => x.IsLeaf && x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get leaf accounts async
        /// </summary>
        public async Task<List<GLAccount>> GetLeafAccountsAsync()
        {
            return await Queryable()
                .Where(x => x.IsLeaf && x.IsActive)
                .OrderBy(x => x.AccountNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Get cash accounts
        /// Filters by AccountNature == Cash OR by account number pattern (1211x for Cash Box, 1111x for 10-digit codes)
        /// </summary>
        public List<GLAccount> GetCashAccounts()
        {
            return Queryable()
                .Where(x => x.IsActive &&
                    (x.AccountNature == AccountNature.Cash ||
                     x.AccountNumber.StartsWith("1211") ||  // Cash Box (short code)
                     x.AccountNumber.StartsWith("1111")))   // Cash Boxes (10-digit)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get bank accounts
        /// Filters by AccountNature == Bank OR by account number pattern (1212x for Banks, 1112x for 10-digit codes)
        /// </summary>
        public List<GLAccount> GetBankAccounts()
        {
            return Queryable()
                .Where(x => x.IsActive &&
                    (x.AccountNature == AccountNature.Bank ||
                     x.AccountNumber.StartsWith("1212") ||  // Banks (short code)
                     x.AccountNumber.StartsWith("1112")))   // Bank Accounts (10-digit)
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Check if account code exists
        /// </summary>
        public bool CodeExists(string accountNumber, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.AccountNumber == accountNumber);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if account code exists async
        /// </summary>
        public async Task<bool> CodeExistsAsync(string accountNumber, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.AccountNumber == accountNumber);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return await Queryable().AnyAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Check if account number exists under the same parent (sibling uniqueness)
        /// </summary>
        public bool AccountNumberExistsUnderParent(string accountNumber, Guid? parentAccountId, long? companyId, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.AccountNumber == accountNumber);

            if (parentAccountId.HasValue)
                predicate = predicate.And(it => it.ParentAccountId == parentAccountId.Value);
            else
                predicate = predicate.And(it => it.ParentAccountId == null);

            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId.Value);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if account number exists under the same parent async
        /// </summary>
        public async Task<bool> AccountNumberExistsUnderParentAsync(string accountNumber, Guid? parentAccountId, long? companyId, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.AccountNumber == accountNumber);

            if (parentAccountId.HasValue)
                predicate = predicate.And(it => it.ParentAccountId == parentAccountId.Value);
            else
                predicate = predicate.And(it => it.ParentAccountId == null);

            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId.Value);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return await Queryable().AnyAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Check if full account number exists
        /// </summary>
        public bool FullAccountNumberExists(string fullAccountNumber, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.FullAccountNumber == fullAccountNumber);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if full account number exists async
        /// </summary>
        public async Task<bool> FullAccountNumberExistsAsync(string fullAccountNumber, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.FullAccountNumber == fullAccountNumber);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return await Queryable().AnyAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Check if full account number exists within a company
        /// </summary>
        public bool FullAccountNumberExistsInCompany(string fullAccountNumber, long? companyId, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.FullAccountNumber == fullAccountNumber);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId.Value);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if full account number exists within a company async
        /// </summary>
        public async Task<bool> FullAccountNumberExistsInCompanyAsync(string fullAccountNumber, long? companyId, Guid? excludeAccountId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.And(it => it.FullAccountNumber == fullAccountNumber);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId.Value);
            predicate = predicate.AndIF(excludeAccountId.HasValue, it => it.AccountId != excludeAccountId.Value);

            return await Queryable().AnyAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Check if account has children
        /// </summary>
        public bool HasChildren(Guid accountId)
        {
            return Any(x => x.ParentAccountId == accountId && x.IsActive);
        }

        /// <summary>
        /// Check if account has children async
        /// </summary>
        public async Task<bool> HasChildrenAsync(Guid accountId)
        {
            return await Queryable().AnyAsync(x => x.ParentAccountId == accountId && x.IsActive);
        }

        /// <summary>
        /// Check if account has transactions
        /// </summary>
        public bool HasTransactions(Guid accountId)
        {
            // Check journal entries
            return Context.Queryable<GLJournalEntry>()
                .Any(x => x.AccountId == accountId);
        }

        /// <summary>
        /// Check if account has transactions async
        /// </summary>
        public async Task<bool> HasTransactionsAsync(Guid accountId)
        {
            return await Context.Queryable<GLJournalEntry>()
                .AnyAsync(x => x.AccountId == accountId);
        }

        /// <summary>
        /// Validate circular reference
        /// </summary>
        public bool WouldCreateCircularReference(Guid accountId, Guid? newParentId)
        {
            if (!newParentId.HasValue) return false;
            if (accountId == newParentId.Value) return true;

            // Check if the new parent is a descendant of the account
            var descendants = GetAllDescendants(accountId);
            return descendants.Any(d => d.AccountId == newParentId.Value);
        }

        /// <summary>
        /// Validate circular reference async
        /// </summary>
        public async Task<bool> WouldCreateCircularReferenceAsync(Guid accountId, Guid? newParentId)
        {
            if (!newParentId.HasValue) return false;
            if (accountId == newParentId.Value) return true;

            var descendants = await GetAllDescendantsAsync(accountId);
            return descendants.Any(d => d.AccountId == newParentId.Value);
        }

        #endregion

        #region Hierarchy Operations

        /// <summary>
        /// Update IsLeaf flag for parent
        /// </summary>
        public int UpdateParentIsLeaf(Guid parentAccountId, bool isLeaf)
        {
            return Context.Updateable<GLAccount>()
                .SetColumns(it => it.IsLeaf == isLeaf)
                .Where(it => it.AccountId == parentAccountId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Update IsLeaf flag for parent async
        /// </summary>
        public async Task<int> UpdateParentIsLeafAsync(Guid parentAccountId, bool isLeaf)
        {
            return await Context.Updateable<GLAccount>()
                .SetColumns(it => it.IsLeaf == isLeaf)
                .Where(it => it.AccountId == parentAccountId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Update account path
        /// </summary>
        public int UpdateAccountPath(Guid accountId, string path)
        {
            return Context.Updateable<GLAccount>()
                .SetColumns(it => it.AccountPath == path)
                .Where(it => it.AccountId == accountId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Update account path async
        /// </summary>
        public async Task<int> UpdateAccountPathAsync(Guid accountId, string path)
        {
            return await Context.Updateable<GLAccount>()
                .SetColumns(it => it.AccountPath == path)
                .Where(it => it.AccountId == accountId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Recalculate all account paths
        /// </summary>
        public async Task RecalculateAllPathsAsync()
        {
            var rootAccounts = await GetRootAccountsAsync();
            foreach (var root in rootAccounts)
            {
                await UpdatePathRecursiveAsync(root, root.AccountId.ToString());
            }
        }

        private async Task UpdatePathRecursiveAsync(GLAccount account, string currentPath)
        {
            await UpdateAccountPathAsync(account.AccountId, currentPath);

            var children = await GetChildAccountsAsync(account.AccountId);
            foreach (var child in children)
            {
                var childPath = currentPath + "/" + child.AccountId;
                await UpdatePathRecursiveAsync(child, childPath);
            }
        }

        /// <summary>
        /// Get account hierarchy path
        /// </summary>
        public List<GLAccount> GetHierarchyPath(Guid accountId)
        {
            var path = new List<GLAccount>();
            var current = GetById(accountId);

            while (current != null)
            {
                path.Insert(0, current);
                if (current.ParentAccountId.HasValue)
                {
                    current = GetById(current.ParentAccountId.Value);
                }
                else
                {
                    break;
                }
            }

            return path;
        }

        #endregion

        #region List Operations

        /// <summary>
        /// Get all accounts with optional company filter
        /// </summary>
        public List<GLAccount> ListAll(long? companyId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.AccountNumber)
                .ToList();
        }

        /// <summary>
        /// Get all accounts with optional company filter async
        /// </summary>
        public async Task<List<GLAccount>> ListAllAsync(long? companyId = null)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.AccountNumber)
                .ToListAsync();
        }

        #endregion

        #region Transaction Date Updates

        /// <summary>
        /// Update last transaction date
        /// </summary>
        public int UpdateLastTransactionDate(Guid accountId, DateTime transactionDate)
        {
            return Context.Updateable<GLAccount>()
                .SetColumns(it => it.LastTransactionDate == transactionDate)
                .Where(it => it.AccountId == accountId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Update last transaction date async
        /// </summary>
        public async Task<int> UpdateLastTransactionDateAsync(Guid accountId, DateTime transactionDate)
        {
            return await Context.Updateable<GLAccount>()
                .SetColumns(it => it.LastTransactionDate == transactionDate)
                .Where(it => it.AccountId == accountId)
                .ExecuteCommandAsync();
        }

        #endregion

        #region Count Operations

        /// <summary>
        /// Increment child count for parent account
        /// </summary>
        public int IncrementChildCount(Guid parentAccountId)
        {
            return Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { ChildCount = it.ChildCount + 1 })
                .Where(it => it.AccountId == parentAccountId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Increment child count for parent account async
        /// </summary>
        public async Task<int> IncrementChildCountAsync(Guid parentAccountId)
        {
            return await Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { ChildCount = it.ChildCount + 1 })
                .Where(it => it.AccountId == parentAccountId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Decrement child count for parent account
        /// </summary>
        public int DecrementChildCount(Guid parentAccountId)
        {
            return Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { ChildCount = it.ChildCount - 1 })
                .Where(it => it.AccountId == parentAccountId && it.ChildCount > 0)
                .ExecuteCommand();
        }

        /// <summary>
        /// Decrement child count for parent account async
        /// </summary>
        public async Task<int> DecrementChildCountAsync(Guid parentAccountId)
        {
            return await Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { ChildCount = it.ChildCount - 1 })
                .Where(it => it.AccountId == parentAccountId && it.ChildCount > 0)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Increment descendant count for all ancestors
        /// </summary>
        public int IncrementAncestorDescendantCounts(Guid accountId)
        {
            var account = GetById(accountId);
            if (account == null || !account.ParentAccountId.HasValue)
                return 0;

            var ancestorIds = GetAncestorIds(account.ParentAccountId.Value);
            if (!ancestorIds.Any())
                return 0;

            return Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { DescendantCount = it.DescendantCount + 1 })
                .Where(it => ancestorIds.Contains(it.AccountId))
                .ExecuteCommand();
        }

        /// <summary>
        /// Increment descendant count for all ancestors async
        /// </summary>
        public async Task<int> IncrementAncestorDescendantCountsAsync(Guid accountId)
        {
            var account = await GetByIdAsync(accountId);
            if (account == null || !account.ParentAccountId.HasValue)
                return 0;

            var ancestorIds = await GetAncestorIdsAsync(account.ParentAccountId.Value);
            if (!ancestorIds.Any())
                return 0;

            return await Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { DescendantCount = it.DescendantCount + 1 })
                .Where(it => ancestorIds.Contains(it.AccountId))
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Decrement descendant count for all ancestors
        /// </summary>
        public int DecrementAncestorDescendantCounts(Guid accountId, int descendantCount)
        {
            var account = GetById(accountId);
            if (account == null || !account.ParentAccountId.HasValue)
                return 0;

            var ancestorIds = GetAncestorIds(account.ParentAccountId.Value);
            if (!ancestorIds.Any())
                return 0;

            // Decrement by (1 + descendantCount) to account for the account itself plus its descendants
            int decrementBy = 1 + descendantCount;

            return Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { DescendantCount = it.DescendantCount - decrementBy })
                .Where(it => ancestorIds.Contains(it.AccountId) && it.DescendantCount >= decrementBy)
                .ExecuteCommand();
        }

        /// <summary>
        /// Decrement descendant count for all ancestors async
        /// </summary>
        public async Task<int> DecrementAncestorDescendantCountsAsync(Guid accountId, int descendantCount)
        {
            var account = await GetByIdAsync(accountId);
            if (account == null || !account.ParentAccountId.HasValue)
                return 0;

            var ancestorIds = await GetAncestorIdsAsync(account.ParentAccountId.Value);
            if (!ancestorIds.Any())
                return 0;

            // Decrement by (1 + descendantCount) to account for the account itself plus its descendants
            int decrementBy = 1 + descendantCount;

            return await Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { DescendantCount = it.DescendantCount - decrementBy })
                .Where(it => ancestorIds.Contains(it.AccountId) && it.DescendantCount >= decrementBy)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Get all ancestor account IDs (including the starting account)
        /// </summary>
        private List<Guid> GetAncestorIds(Guid accountId)
        {
            var ids = new List<Guid>();
            var currentId = accountId;

            while (true)
            {
                ids.Add(currentId);
                var account = GetFirst(x => x.AccountId == currentId);
                if (account == null || !account.ParentAccountId.HasValue)
                    break;
                currentId = account.ParentAccountId.Value;
            }

            return ids;
        }

        /// <summary>
        /// Get all ancestor account IDs async
        /// </summary>
        private async Task<List<Guid>> GetAncestorIdsAsync(Guid accountId)
        {
            var ids = new List<Guid>();
            var currentId = accountId;

            while (true)
            {
                ids.Add(currentId);
                var account = await GetFirstAsync(x => x.AccountId == currentId);
                if (account == null || !account.ParentAccountId.HasValue)
                    break;
                currentId = account.ParentAccountId.Value;
            }

            return ids;
        }

        /// <summary>
        /// Recalculate all child and descendant counts, and fix IsLeaf property
        /// </summary>
        public async Task RecalculateAllCountsAsync()
        {
            // First, reset all counts to 0 and set all accounts as leaf
            await Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { ChildCount = 0, DescendantCount = 0, IsLeaf = true })
                .ExecuteCommandAsync();

            // Get all accounts
            var allAccounts = await Queryable().ToListAsync();

            // Calculate child counts
            var childCounts = allAccounts
                .Where(a => a.ParentAccountId.HasValue)
                .GroupBy(a => a.ParentAccountId.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Update child counts and IsLeaf=false for parent accounts
            foreach (var kvp in childCounts)
            {
                await Context.Updateable<GLAccount>()
                    .SetColumns(it => new GLAccount { ChildCount = kvp.Value, IsLeaf = false, AllowManualEntries = false })
                    .Where(it => it.AccountId == kvp.Key)
                    .ExecuteCommandAsync();
            }

            // Calculate descendant counts using recursive logic
            var rootAccounts = allAccounts.Where(a => !a.ParentAccountId.HasValue).ToList();
            var accountDict = allAccounts.ToDictionary(a => a.AccountId);

            foreach (var root in rootAccounts)
            {
                await CalculateDescendantCountRecursiveAsync(root.AccountId, accountDict);
            }
        }

        private async Task<int> CalculateDescendantCountRecursiveAsync(Guid accountId, Dictionary<Guid, GLAccount> accountDict)
        {
            var children = accountDict.Values
                .Where(a => a.ParentAccountId == accountId)
                .ToList();

            int descendantCount = children.Count;

            foreach (var child in children)
            {
                descendantCount += await CalculateDescendantCountRecursiveAsync(child.AccountId, accountDict);
            }

            await Context.Updateable<GLAccount>()
                .SetColumns(it => new GLAccount { DescendantCount = descendantCount })
                .Where(it => it.AccountId == accountId)
                .ExecuteCommandAsync();

            return descendantCount;
        }

        #endregion
    }
}
