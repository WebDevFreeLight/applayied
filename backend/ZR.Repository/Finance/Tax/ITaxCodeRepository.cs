using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Tax;

namespace ZR.Repository.Finance.Tax
{
    /// <summary>
    /// Tax Code Repository Interface
    /// واجهة مستودع رموز الضرائب
    /// </summary>
    public interface ITaxCodeRepository : IBaseRepository<TaxCode>
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax codes
        /// </summary>
        PagedInfo<TaxCode> GetList(TaxCode parm, PagerInfo pager);

        /// <summary>
        /// Get tax code by ID
        /// </summary>
        TaxCode GetById(Guid id);

        /// <summary>
        /// Get tax code by ID (async)
        /// </summary>
        Task<TaxCode> GetByIdAsync(Guid id);

        /// <summary>
        /// Get tax code by code and company
        /// </summary>
        TaxCode GetByCode(string code, long? companyId = null);

        /// <summary>
        /// Get tax code by code and company (async)
        /// </summary>
        Task<TaxCode> GetByCodeAsync(string code, long? companyId = null);

        /// <summary>
        /// Insert tax code
        /// </summary>
        int InsertTaxCode(TaxCode model);

        /// <summary>
        /// Insert tax code (async)
        /// </summary>
        Task<int> InsertTaxCodeAsync(TaxCode model);

        /// <summary>
        /// Update tax code
        /// </summary>
        int UpdateTaxCode(TaxCode model);

        /// <summary>
        /// Update tax code (async)
        /// </summary>
        Task<int> UpdateTaxCodeAsync(TaxCode model);

        /// <summary>
        /// Delete tax codes by IDs
        /// </summary>
        int DeleteTaxCodes(Guid[] ids);

        /// <summary>
        /// Delete tax codes by IDs (async)
        /// </summary>
        Task<int> DeleteTaxCodesAsync(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax codes for a company
        /// </summary>
        List<TaxCode> GetActiveTaxCodes(long? companyId = null);

        /// <summary>
        /// Get all active tax codes for a company (async)
        /// </summary>
        Task<List<TaxCode>> GetActiveTaxCodesAsync(long? companyId = null);

        /// <summary>
        /// Get default tax code for a company
        /// </summary>
        TaxCode GetDefaultTaxCode(long? companyId = null);

        /// <summary>
        /// Get default tax code for a company (async)
        /// </summary>
        Task<TaxCode> GetDefaultTaxCodeAsync(long? companyId = null);

        /// <summary>
        /// Get tax codes by type
        /// </summary>
        List<TaxCode> GetByTaxType(Model.Finance.Enums.TaxType taxType, long? companyId = null);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if code exists for company
        /// </summary>
        bool CodeExists(string code, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Check if code exists for company (async)
        /// </summary>
        Task<bool> CodeExistsAsync(string code, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Check if tax code is used in any journal lines
        /// </summary>
        bool IsUsedInJournals(Guid taxCodeId);

        /// <summary>
        /// Check if tax code is used in any journal lines (async)
        /// </summary>
        Task<bool> IsUsedInJournalsAsync(Guid taxCodeId);

        #endregion
    }
}
