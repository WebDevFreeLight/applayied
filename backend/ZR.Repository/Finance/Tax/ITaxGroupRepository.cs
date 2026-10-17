using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Tax;

namespace ZR.Repository.Finance.Tax
{
    /// <summary>
    /// Tax Group Repository Interface
    /// واجهة مستودع مجموعات الضرائب
    /// </summary>
    public interface ITaxGroupRepository : IBaseRepository<TaxGroup>
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax groups
        /// </summary>
        PagedInfo<TaxGroup> GetList(TaxGroup parm, PagerInfo pager);

        /// <summary>
        /// Get tax group by ID with lines
        /// </summary>
        TaxGroup GetById(Guid id, bool includeLines = true);

        /// <summary>
        /// Get tax group by ID with lines (async)
        /// </summary>
        Task<TaxGroup> GetByIdAsync(Guid id, bool includeLines = true);

        /// <summary>
        /// Get tax group by code
        /// </summary>
        TaxGroup GetByCode(string code, long? companyId = null);

        /// <summary>
        /// Get tax group by code (async)
        /// </summary>
        Task<TaxGroup> GetByCodeAsync(string code, long? companyId = null);

        /// <summary>
        /// Insert tax group with lines
        /// </summary>
        int InsertTaxGroup(TaxGroup model);

        /// <summary>
        /// Insert tax group with lines (async)
        /// </summary>
        Task<int> InsertTaxGroupAsync(TaxGroup model);

        /// <summary>
        /// Update tax group with lines
        /// </summary>
        int UpdateTaxGroup(TaxGroup model);

        /// <summary>
        /// Update tax group with lines (async)
        /// </summary>
        Task<int> UpdateTaxGroupAsync(TaxGroup model);

        /// <summary>
        /// Delete tax groups by IDs
        /// </summary>
        int DeleteTaxGroups(Guid[] ids);

        /// <summary>
        /// Delete tax groups by IDs (async)
        /// </summary>
        Task<int> DeleteTaxGroupsAsync(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax groups for a company
        /// </summary>
        List<TaxGroup> GetActiveTaxGroups(long? companyId = null);

        /// <summary>
        /// Get all active tax groups for a company (async)
        /// </summary>
        Task<List<TaxGroup>> GetActiveTaxGroupsAsync(long? companyId = null);

        /// <summary>
        /// Get tax group lines by group ID
        /// </summary>
        List<TaxGroupLine> GetGroupLines(Guid groupId);

        /// <summary>
        /// Get tax group lines by group ID (async)
        /// </summary>
        Task<List<TaxGroupLine>> GetGroupLinesAsync(Guid groupId);

        /// <summary>
        /// Get tax groups containing a specific tax code
        /// </summary>
        List<TaxGroup> GetGroupsByTaxCode(Guid taxCodeId);

        #endregion

        #region Line Operations

        /// <summary>
        /// Add a line to tax group
        /// </summary>
        int AddGroupLine(TaxGroupLine line);

        /// <summary>
        /// Add a line to tax group (async)
        /// </summary>
        Task<int> AddGroupLineAsync(TaxGroupLine line);

        /// <summary>
        /// Update a group line
        /// </summary>
        int UpdateGroupLine(TaxGroupLine line);

        /// <summary>
        /// Update a group line (async)
        /// </summary>
        Task<int> UpdateGroupLineAsync(TaxGroupLine line);

        /// <summary>
        /// Remove a line from tax group
        /// </summary>
        int RemoveGroupLine(Guid lineId);

        /// <summary>
        /// Remove a line from tax group (async)
        /// </summary>
        Task<int> RemoveGroupLineAsync(Guid lineId);

        /// <summary>
        /// Remove all lines from tax group
        /// </summary>
        int RemoveAllGroupLines(Guid groupId);

        /// <summary>
        /// Remove all lines from tax group (async)
        /// </summary>
        Task<int> RemoveAllGroupLinesAsync(Guid groupId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if group code exists
        /// </summary>
        bool CodeExists(string code, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Check if group code exists (async)
        /// </summary>
        Task<bool> CodeExistsAsync(string code, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Check if tax group is used in any journal lines
        /// </summary>
        bool IsUsedInJournals(Guid groupId);

        /// <summary>
        /// Check if tax group is used in any journal lines (async)
        /// </summary>
        Task<bool> IsUsedInJournalsAsync(Guid groupId);

        #endregion

        #region Calculation Operations

        /// <summary>
        /// Calculate combined rate for a tax group
        /// Handles both compound and non-compound groups
        /// </summary>
        decimal CalculateCombinedRate(Guid groupId, DateTime? effectiveDate = null);

        /// <summary>
        /// Calculate combined rate for a tax group (async)
        /// </summary>
        Task<decimal> CalculateCombinedRateAsync(Guid groupId, DateTime? effectiveDate = null);

        #endregion
    }
}
