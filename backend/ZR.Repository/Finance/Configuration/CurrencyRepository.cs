using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.GL;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Currency Repository Implementation
    /// مستودع العملات
    /// </summary>
    [AppService(ServiceType = typeof(ICurrencyRepository), ServiceLifetime = LifeTime.Transient)]
    public class CurrencyRepository : BaseRepository<Currency>, ICurrencyRepository
    {
        /// <summary>
        /// Get paginated list of currencies
        /// </summary>
        public PagedInfo<Currency> GetList(Currency searchModel, PagerInfo pager)
        {
            var predicate = Expressionable.Create<Currency>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(searchModel.Code), it => it.Code.Contains(searchModel.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(searchModel.Name), it => it.Name.Contains(searchModel.Name));

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Code)
                .ToPage(pager);
        }

        /// <summary>
        /// Get currency by ID
        /// </summary>
        public Currency GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get currency by ID (async)
        /// </summary>
        public async Task<Currency> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Get currency by code
        /// </summary>
        public Currency GetByCode(string code)
        {
            return GetFirst(x => x.Code == code);
        }

        /// <summary>
        /// Get currency by code (async)
        /// </summary>
        public async Task<Currency> GetByCodeAsync(string code)
        {
            return await GetFirstAsync(x => x.Code == code);
        }

        /// <summary>
        /// Get base currency
        /// </summary>
        public Currency GetBaseCurrency()
        {
            return GetFirst(x => x.IsBaseCurrency && x.IsActive);
        }

        /// <summary>
        /// Get base currency (async)
        /// </summary>
        public async Task<Currency> GetBaseCurrencyAsync()
        {
            return await GetFirstAsync(x => x.IsBaseCurrency && x.IsActive);
        }

        /// <summary>
        /// Get all active currencies
        /// </summary>
        public List<Currency> GetActiveCurrencies()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToList();
        }

        /// <summary>
        /// Get all active currencies (async)
        /// </summary>
        public async Task<List<Currency>> GetActiveCurrenciesAsync()
        {
            return await Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToListAsync();
        }

        /// <summary>
        /// Check if currency code exists
        /// </summary>
        public bool CurrencyCodeExists(string code, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<Currency>();
            predicate = predicate.And(it => it.Code == code);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if currency code exists (async)
        /// </summary>
        public async Task<bool> CurrencyCodeExistsAsync(string code, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<Currency>();
            predicate = predicate.And(it => it.Code == code);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return await Queryable().Where(predicate.ToExpression()).AnyAsync();
        }

        /// <summary>
        /// Check if base currency exists
        /// </summary>
        public bool BaseCurrencyExists()
        {
            return Any(x => x.IsBaseCurrency);
        }

        /// <summary>
        /// Check if base currency exists (async)
        /// </summary>
        public async Task<bool> BaseCurrencyExistsAsync()
        {
            return await Queryable().Where(x => x.IsBaseCurrency).AnyAsync();
        }

        /// <summary>
        /// Create currency
        /// </summary>
        public int CreateCurrency(Currency currency)
        {
            return Insert(currency);
        }

        /// <summary>
        /// Create currency (async)
        /// </summary>
        public async Task<int> CreateCurrencyAsync(Currency currency)
        {
            return await InsertAsync(currency) ? 1 : 0;
        }

        /// <summary>
        /// Update currency
        /// </summary>
        public int UpdateCurrency(Currency currency)
        {
            return Update(currency);
        }

        /// <summary>
        /// Update currency (async)
        /// </summary>
        public async Task<int> UpdateCurrencyAsync(Currency currency)
        {
            return await UpdateAsync(currency) ? 1 : 0;
        }

        /// <summary>
        /// Set base currency
        /// </summary>
        public int SetBaseCurrency(Guid currencyId)
        {
            // First unset all base currencies
            Context.Updateable<Currency>()
                .SetColumns(c => c.IsBaseCurrency == false)
                .Where(c => c.IsBaseCurrency)
                .ExecuteCommand();

            // Set new base currency
            return Context.Updateable<Currency>()
                .SetColumns(c => c.IsBaseCurrency == true)
                .Where(c => c.Id == currencyId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Set base currency (async)
        /// </summary>
        public async Task<int> SetBaseCurrencyAsync(Guid currencyId)
        {
            // First unset all base currencies
            await Context.Updateable<Currency>()
                .SetColumns(c => c.IsBaseCurrency == false)
                .Where(c => c.IsBaseCurrency)
                .ExecuteCommandAsync();

            // Set new base currency
            return await Context.Updateable<Currency>()
                .SetColumns(c => c.IsBaseCurrency == true)
                .Where(c => c.Id == currencyId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Activate currency
        /// </summary>
        public int ActivateCurrency(Guid id)
        {
            return Context.Updateable<Currency>()
                .SetColumns(c => c.IsActive == true)
                .Where(c => c.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Activate currency (async)
        /// </summary>
        public async Task<int> ActivateCurrencyAsync(Guid id)
        {
            return await Context.Updateable<Currency>()
                .SetColumns(c => c.IsActive == true)
                .Where(c => c.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Deactivate currency
        /// </summary>
        public int DeactivateCurrency(Guid id)
        {
            // Cannot deactivate base currency
            var currency = GetById(id);
            if (currency?.IsBaseCurrency == true)
            {
                throw new InvalidOperationException("Cannot deactivate base currency");
            }

            return Context.Updateable<Currency>()
                .SetColumns(c => c.IsActive == false)
                .Where(c => c.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Deactivate currency (async)
        /// </summary>
        public async Task<int> DeactivateCurrencyAsync(Guid id)
        {
            var currency = await GetByIdAsync(id);
            if (currency?.IsBaseCurrency == true)
            {
                throw new InvalidOperationException("Cannot deactivate base currency");
            }

            return await Context.Updateable<Currency>()
                .SetColumns(c => c.IsActive == false)
                .Where(c => c.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete currency
        /// </summary>
        public int DeleteCurrency(Guid id)
        {
            var currency = GetById(id);
            if (currency?.IsBaseCurrency == true)
            {
                throw new InvalidOperationException("Cannot delete base currency");
            }

            if (IsUsedInJournals(id))
            {
                throw new InvalidOperationException("Cannot delete currency that is used in journals");
            }

            return Delete(id);
        }

        /// <summary>
        /// Delete currency (async)
        /// </summary>
        public async Task<int> DeleteCurrencyAsync(Guid id)
        {
            var currency = await GetByIdAsync(id);
            if (currency?.IsBaseCurrency == true)
            {
                throw new InvalidOperationException("Cannot delete base currency");
            }

            if (await IsUsedInJournalsAsync(id))
            {
                throw new InvalidOperationException("Cannot delete currency that is used in journals");
            }

            return await DeleteAsync(x => x.Id == id) ? 1 : 0;
        }

        /// <summary>
        /// Check if currency is used in journals
        /// </summary>
        public bool IsUsedInJournals(Guid currencyId)
        {
            // Check if currency is used in any journal entries
            return Context.Queryable<GLJournal>()
                .Any(j => j.CurrencyId.HasValue && j.CurrencyId.Value == currencyId);
        }

        /// <summary>
        /// Check if currency is used in journals (async)
        /// </summary>
        public async Task<bool> IsUsedInJournalsAsync(Guid currencyId)
        {
            return await Context.Queryable<GLJournal>()
                .AnyAsync(j => j.CurrencyId.HasValue && j.CurrencyId.Value == currencyId);
        }
    }
}
