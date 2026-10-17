using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Tax;

namespace ZR.Repository.Finance.Tax
{
    /// <summary>
    /// Tax Code Repository Implementation
    /// تنفيذ مستودع رموز الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxCodeRepository), ServiceLifetime = LifeTime.Transient)]
    public class TaxCodeRepository : BaseRepository<TaxCode>, ITaxCodeRepository
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax codes with tenant filtering
        /// </summary>
        public PagedInfo<TaxCode> GetList(TaxCode parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<TaxCode>();

            // Tenant filtering - CRITICAL for multi-tenant isolation
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            // Additional filters
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Code),
                it => it.Code.Contains(parm.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NameEn),
                it => it.NameEn.Contains(parm.NameEn) || it.NameAr.Contains(parm.NameEn));
            predicate = predicate.AndIF(parm.IsActive,
                it => it.IsActive == parm.IsActive);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.SortOrder)
                .OrderBy(x => x.Code)
                .ToPage(pager);
        }

        /// <summary>
        /// Get tax code by ID
        /// </summary>
        public TaxCode GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get tax code by ID (async)
        /// </summary>
        public async Task<TaxCode> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Get tax code by code and company
        /// </summary>
        public TaxCode GetByCode(string code, long? companyId = null)
        {
            var query = Queryable().Where(x => x.Code == code);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            return query.First();
        }

        /// <summary>
        /// Get tax code by code and company (async)
        /// </summary>
        public async Task<TaxCode> GetByCodeAsync(string code, long? companyId = null)
        {
            var query = Context.Queryable<TaxCode>().Where(x => x.Code == code);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            return await query.FirstAsync();
        }

        /// <summary>
        /// Insert tax code
        /// </summary>
        public int InsertTaxCode(TaxCode model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Insert tax code (async)
        /// </summary>
        public async Task<int> InsertTaxCodeAsync(TaxCode model)
        {
            var result = await InsertAsync(model);
            return result ? 1 : 0;
        }

        /// <summary>
        /// Update tax code
        /// </summary>
        public int UpdateTaxCode(TaxCode model)
        {
            return Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommand();
        }

        /// <summary>
        /// Update tax code (async)
        /// </summary>
        public async Task<int> UpdateTaxCodeAsync(TaxCode model)
        {
            return await Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete tax codes by IDs
        /// </summary>
        public int DeleteTaxCodes(Guid[] ids)
        {
            return Context.Deleteable<TaxCode>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete tax codes by IDs (async)
        /// </summary>
        public async Task<int> DeleteTaxCodesAsync(Guid[] ids)
        {
            return await Context.Deleteable<TaxCode>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommandAsync();
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax codes for a company
        /// </summary>
        public List<TaxCode> GetActiveTaxCodes(long? companyId = null)
        {
            var query = Queryable().Where(x => x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            return query.OrderBy(x => x.SortOrder).OrderBy(x => x.Code).ToList();
        }

        /// <summary>
        /// Get all active tax codes for a company (async)
        /// </summary>
        public async Task<List<TaxCode>> GetActiveTaxCodesAsync(long? companyId = null)
        {
            var query = Context.Queryable<TaxCode>().Where(x => x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            return await query.OrderBy(x => x.SortOrder).OrderBy(x => x.Code).ToListAsync();
        }

        /// <summary>
        /// Get default tax code for a company
        /// </summary>
        public TaxCode GetDefaultTaxCode(long? companyId = null)
        {
            var query = Queryable().Where(x => x.IsDefault && x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            return query.First();
        }

        /// <summary>
        /// Get default tax code for a company (async)
        /// </summary>
        public async Task<TaxCode> GetDefaultTaxCodeAsync(long? companyId = null)
        {
            var query = Context.Queryable<TaxCode>().Where(x => x.IsDefault && x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            return await query.FirstAsync();
        }

        /// <summary>
        /// Get tax codes by type
        /// </summary>
        public List<TaxCode> GetByTaxType(TaxType taxType, long? companyId = null)
        {
            var query = Queryable().Where(x => x.TaxType == taxType && x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            return query.OrderBy(x => x.SortOrder).ToList();
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if code exists for company
        /// </summary>
        public bool CodeExists(string code, long? companyId, Guid? excludeId = null)
        {
            var query = Queryable().Where(x => x.Code == code);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }
            return query.Any();
        }

        /// <summary>
        /// Check if code exists for company (async)
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code, long? companyId, Guid? excludeId = null)
        {
            var query = Context.Queryable<TaxCode>().Where(x => x.Code == code);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        /// <summary>
        /// Check if tax code is used in any journal lines
        /// </summary>
        public bool IsUsedInJournals(Guid taxCodeId)
        {
            return Context.Queryable<TaxJournalLine>()
                .Any(x => x.TaxCodeId == taxCodeId);
        }

        /// <summary>
        /// Check if tax code is used in any journal lines (async)
        /// </summary>
        public async Task<bool> IsUsedInJournalsAsync(Guid taxCodeId)
        {
            return await Context.Queryable<TaxJournalLine>()
                .AnyAsync(x => x.TaxCodeId == taxCodeId);
        }

        #endregion
    }
}
