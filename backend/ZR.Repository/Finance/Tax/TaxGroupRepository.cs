using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Tax;

namespace ZR.Repository.Finance.Tax
{
    /// <summary>
    /// Tax Group Repository Implementation
    /// تنفيذ مستودع مجموعات الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxGroupRepository), ServiceLifetime = LifeTime.Transient)]
    public class TaxGroupRepository : BaseRepository<TaxGroup>, ITaxGroupRepository
    {
        private readonly ITaxRateRepository _taxRateRepository;

        public TaxGroupRepository(ITaxRateRepository taxRateRepository)
        {
            _taxRateRepository = taxRateRepository;
        }

        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax groups with tenant filtering
        /// </summary>
        public PagedInfo<TaxGroup> GetList(TaxGroup parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<TaxGroup>();

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
        /// Get tax group by ID with optional lines
        /// </summary>
        public TaxGroup GetById(Guid id, bool includeLines = true)
        {
            var group = GetFirst(x => x.Id == id);
            if (group != null && includeLines)
            {
                group.Lines = GetGroupLines(id);
            }
            return group;
        }

        /// <summary>
        /// Get tax group by ID with optional lines (async)
        /// </summary>
        public async Task<TaxGroup> GetByIdAsync(Guid id, bool includeLines = true)
        {
            var group = await GetFirstAsync(x => x.Id == id);
            if (group != null && includeLines)
            {
                group.Lines = await GetGroupLinesAsync(id);
            }
            return group;
        }

        /// <summary>
        /// Get tax group by code and company
        /// </summary>
        public TaxGroup GetByCode(string code, long? companyId = null)
        {
            var query = Queryable().Where(x => x.Code == code);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            var group = query.First();
            if (group != null)
            {
                group.Lines = GetGroupLines(group.Id);
            }
            return group;
        }

        /// <summary>
        /// Get tax group by code and company (async)
        /// </summary>
        public async Task<TaxGroup> GetByCodeAsync(string code, long? companyId = null)
        {
            var query = Context.Queryable<TaxGroup>().Where(x => x.Code == code);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            var group = await query.FirstAsync();
            if (group != null)
            {
                group.Lines = await GetGroupLinesAsync(group.Id);
            }
            return group;
        }

        /// <summary>
        /// Insert tax group with lines (transactional)
        /// </summary>
        public int InsertTaxGroup(TaxGroup model)
        {
            var result = 0;
            Context.Ado.BeginTran();
            try
            {
                // Insert group
                result = Insert(model);

                // Insert lines
                if (model.Lines != null && model.Lines.Any())
                {
                    foreach (var line in model.Lines)
                    {
                        line.TaxGroupId = model.Id;
                        if (line.Id == Guid.Empty)
                        {
                            line.Id = Guid.NewGuid();
                        }
                        // Initialize SysBase properties
                        line.Create_time = DateTime.Now;
                    }
                    Context.Insertable(model.Lines).ExecuteCommand();
                }

                Context.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                Context.Ado.RollbackTran();
                throw new InvalidOperationException($"Failed to insert tax group: {ex.Message}", ex);
            }
            return result;
        }

        /// <summary>
        /// Insert tax group with lines (async, transactional)
        /// </summary>
        public async Task<int> InsertTaxGroupAsync(TaxGroup model)
        {
            var result = 0;
            await Context.Ado.BeginTranAsync();
            try
            {
                // Insert group
                var insertResult = await InsertAsync(model);
                result = insertResult ? 1 : 0;

                // Insert lines
                if (model.Lines != null && model.Lines.Any())
                {
                    foreach (var line in model.Lines)
                    {
                        line.TaxGroupId = model.Id;
                        if (line.Id == Guid.Empty)
                        {
                            line.Id = Guid.NewGuid();
                        }
                        // Initialize SysBase properties
                        line.Create_time = DateTime.Now;
                    }
                    await Context.Insertable(model.Lines).ExecuteCommandAsync();
                }

                await Context.Ado.CommitTranAsync();
            }
            catch (Exception ex)
            {
                await Context.Ado.RollbackTranAsync();
                throw new InvalidOperationException($"Failed to insert tax group: {ex.Message}", ex);
            }
            return result;
        }

        /// <summary>
        /// Update tax group with lines (transactional)
        /// </summary>
        public int UpdateTaxGroup(TaxGroup model)
        {
            var result = 0;
            Context.Ado.BeginTran();
            try
            {
                // Update group
                result = Context.Updateable(model)
                    .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                    .ExecuteCommand();

                // Delete existing lines and insert new ones
                RemoveAllGroupLines(model.Id);

                if (model.Lines != null && model.Lines.Any())
                {
                    foreach (var line in model.Lines)
                    {
                        line.TaxGroupId = model.Id;
                        if (line.Id == Guid.Empty)
                        {
                            line.Id = Guid.NewGuid();
                        }
                    }
                    Context.Insertable(model.Lines).ExecuteCommand();
                }

                Context.Ado.CommitTran();
            }
            catch
            {
                Context.Ado.RollbackTran();
                throw;
            }
            return result;
        }

        /// <summary>
        /// Update tax group with lines (async, transactional)
        /// </summary>
        public async Task<int> UpdateTaxGroupAsync(TaxGroup model)
        {
            var result = 0;
            await Context.Ado.BeginTranAsync();
            try
            {
                // Update group
                result = await Context.Updateable(model)
                    .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                    .ExecuteCommandAsync();

                // Delete existing lines and insert new ones
                await RemoveAllGroupLinesAsync(model.Id);

                if (model.Lines != null && model.Lines.Any())
                {
                    foreach (var line in model.Lines)
                    {
                        line.TaxGroupId = model.Id;
                        if (line.Id == Guid.Empty)
                        {
                            line.Id = Guid.NewGuid();
                        }
                    }
                    await Context.Insertable(model.Lines).ExecuteCommandAsync();
                }

                await Context.Ado.CommitTranAsync();
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
            return result;
        }

        /// <summary>
        /// Delete tax groups by IDs
        /// </summary>
        public int DeleteTaxGroups(Guid[] ids)
        {
            Context.Ado.BeginTran();
            try
            {
                // Delete lines first
                Context.Deleteable<TaxGroupLine>()
                    .Where(it => ids.Contains(it.TaxGroupId))
                    .ExecuteCommand();

                // Delete groups
                var result = Context.Deleteable<TaxGroup>()
                    .Where(it => ids.Contains(it.Id))
                    .ExecuteCommand();

                Context.Ado.CommitTran();
                return result;
            }
            catch
            {
                Context.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// Delete tax groups by IDs (async)
        /// </summary>
        public async Task<int> DeleteTaxGroupsAsync(Guid[] ids)
        {
            await Context.Ado.BeginTranAsync();
            try
            {
                // Delete lines first
                await Context.Deleteable<TaxGroupLine>()
                    .Where(it => ids.Contains(it.TaxGroupId))
                    .ExecuteCommandAsync();

                // Delete groups
                var result = await Context.Deleteable<TaxGroup>()
                    .Where(it => ids.Contains(it.Id))
                    .ExecuteCommandAsync();

                await Context.Ado.CommitTranAsync();
                return result;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax groups for a company
        /// </summary>
        public List<TaxGroup> GetActiveTaxGroups(long? companyId = null)
        {
            var query = Queryable().Where(x => x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            var groups = query.OrderBy(x => x.SortOrder).OrderBy(x => x.Code).ToList();

            // Load lines for each group
            foreach (var group in groups)
            {
                group.Lines = GetGroupLines(group.Id);
            }
            return groups;
        }

        /// <summary>
        /// Get all active tax groups for a company (async)
        /// </summary>
        public async Task<List<TaxGroup>> GetActiveTaxGroupsAsync(long? companyId = null)
        {
            var query = Context.Queryable<TaxGroup>().Where(x => x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId || x.CompanyID == null);
            }
            var groups = await query.OrderBy(x => x.SortOrder).OrderBy(x => x.Code).ToListAsync();

            // Load lines for each group
            foreach (var group in groups)
            {
                group.Lines = await GetGroupLinesAsync(group.Id);
            }
            return groups;
        }

        /// <summary>
        /// Get tax group lines by group ID
        /// </summary>
        public List<TaxGroupLine> GetGroupLines(Guid groupId)
        {
            return Context.Queryable<TaxGroupLine>()
                .Where(x => x.TaxGroupId == groupId)
                .OrderBy(x => x.Sequence)
                .ToList();
        }

        /// <summary>
        /// Get tax group lines by group ID (async)
        /// </summary>
        public async Task<List<TaxGroupLine>> GetGroupLinesAsync(Guid groupId)
        {
            return await Context.Queryable<TaxGroupLine>()
                .Where(x => x.TaxGroupId == groupId)
                .OrderBy(x => x.Sequence)
                .ToListAsync();
        }

        /// <summary>
        /// Get tax groups containing a specific tax code
        /// </summary>
        public List<TaxGroup> GetGroupsByTaxCode(Guid taxCodeId)
        {
            var groupIds = Context.Queryable<TaxGroupLine>()
                .Where(x => x.TaxCodeId == taxCodeId && x.IsActive)
                .Select(x => x.TaxGroupId)
                .Distinct()
                .ToList();

            return Queryable()
                .Where(x => groupIds.Contains(x.Id))
                .OrderBy(x => x.Code)
                .ToList();
        }

        #endregion

        #region Line Operations

        /// <summary>
        /// Add a line to tax group
        /// </summary>
        public int AddGroupLine(TaxGroupLine line)
        {
            if (line.Id == Guid.Empty)
            {
                line.Id = Guid.NewGuid();
            }
            return Context.Insertable(line).ExecuteCommand();
        }

        /// <summary>
        /// Add a line to tax group (async)
        /// </summary>
        public async Task<int> AddGroupLineAsync(TaxGroupLine line)
        {
            if (line.Id == Guid.Empty)
            {
                line.Id = Guid.NewGuid();
            }
            return await Context.Insertable(line).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update a group line
        /// </summary>
        public int UpdateGroupLine(TaxGroupLine line)
        {
            return Context.Updateable(line).ExecuteCommand();
        }

        /// <summary>
        /// Update a group line (async)
        /// </summary>
        public async Task<int> UpdateGroupLineAsync(TaxGroupLine line)
        {
            return await Context.Updateable(line).ExecuteCommandAsync();
        }

        /// <summary>
        /// Remove a line from tax group
        /// </summary>
        public int RemoveGroupLine(Guid lineId)
        {
            return Context.Deleteable<TaxGroupLine>()
                .Where(x => x.Id == lineId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Remove a line from tax group (async)
        /// </summary>
        public async Task<int> RemoveGroupLineAsync(Guid lineId)
        {
            return await Context.Deleteable<TaxGroupLine>()
                .Where(x => x.Id == lineId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Remove all lines from tax group
        /// </summary>
        public int RemoveAllGroupLines(Guid groupId)
        {
            return Context.Deleteable<TaxGroupLine>()
                .Where(x => x.TaxGroupId == groupId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Remove all lines from tax group (async)
        /// </summary>
        public async Task<int> RemoveAllGroupLinesAsync(Guid groupId)
        {
            return await Context.Deleteable<TaxGroupLine>()
                .Where(x => x.TaxGroupId == groupId)
                .ExecuteCommandAsync();
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if group code exists
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
        /// Check if group code exists (async)
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code, long? companyId, Guid? excludeId = null)
        {
            var query = Context.Queryable<TaxGroup>().Where(x => x.Code == code);
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
        /// Check if tax group is used in any journal lines
        /// </summary>
        public bool IsUsedInJournals(Guid groupId)
        {
            // Tax groups are referenced via their contained tax codes
            // We check if any tax journal line references a tax code that belongs to this group
            var taxCodeIds = Context.Queryable<TaxGroupLine>()
                .Where(x => x.TaxGroupId == groupId)
                .Select(x => x.TaxCodeId)
                .ToList();

            return Context.Queryable<TaxJournalLine>()
                .Where(x => taxCodeIds.Contains(x.TaxCodeId))
                .Any();
        }

        /// <summary>
        /// Check if tax group is used in any journal lines (async)
        /// </summary>
        public async Task<bool> IsUsedInJournalsAsync(Guid groupId)
        {
            var taxCodeIds = await Context.Queryable<TaxGroupLine>()
                .Where(x => x.TaxGroupId == groupId)
                .Select(x => x.TaxCodeId)
                .ToListAsync();

            return await Context.Queryable<TaxJournalLine>()
                .Where(x => taxCodeIds.Contains(x.TaxCodeId))
                .AnyAsync();
        }

        #endregion

        #region Calculation Operations

        /// <summary>
        /// Calculate combined rate for a tax group
        /// For compound groups: rate = (1 + r1) * (1 + r2) - 1
        /// For non-compound groups: rate = r1 + r2
        /// </summary>
        public decimal CalculateCombinedRate(Guid groupId, DateTime? effectiveDate = null)
        {
            var group = GetById(groupId, true);
            if (group == null || group.Lines == null || !group.Lines.Any())
            {
                return 0;
            }

            var date = effectiveDate ?? DateTime.Today;
            var activeLines = group.Lines.Where(l => l.IsActive).OrderBy(l => l.Sequence).ToList();

            if (group.IsCompound)
            {
                // Compound calculation: (1 + r1%) * (1 + r2%) - 1
                decimal factor = 1;
                foreach (var line in activeLines)
                {
                    var rate = line.OverrideRate ?? GetEffectiveRateForTaxCode(line.TaxCodeId, date);
                    factor *= (1 + rate / 100);
                }
                return (factor - 1) * 100;
            }
            else
            {
                // Simple addition
                decimal total = 0;
                foreach (var line in activeLines)
                {
                    var rate = line.OverrideRate ?? GetEffectiveRateForTaxCode(line.TaxCodeId, date);
                    total += rate;
                }
                return total;
            }
        }

        /// <summary>
        /// Calculate combined rate for a tax group (async)
        /// </summary>
        public async Task<decimal> CalculateCombinedRateAsync(Guid groupId, DateTime? effectiveDate = null)
        {
            var group = await GetByIdAsync(groupId, true);
            if (group == null || group.Lines == null || !group.Lines.Any())
            {
                return 0;
            }

            var date = effectiveDate ?? DateTime.Today;
            var activeLines = group.Lines.Where(l => l.IsActive).OrderBy(l => l.Sequence).ToList();

            if (group.IsCompound)
            {
                decimal factor = 1;
                foreach (var line in activeLines)
                {
                    var rate = line.OverrideRate ?? await GetEffectiveRateForTaxCodeAsync(line.TaxCodeId, date);
                    factor *= (1 + rate / 100);
                }
                return (factor - 1) * 100;
            }
            else
            {
                decimal total = 0;
                foreach (var line in activeLines)
                {
                    var rate = line.OverrideRate ?? await GetEffectiveRateForTaxCodeAsync(line.TaxCodeId, date);
                    total += rate;
                }
                return total;
            }
        }

        /// <summary>
        /// Helper method to get effective rate for a tax code
        /// </summary>
        private decimal GetEffectiveRateForTaxCode(Guid taxCodeId, DateTime date)
        {
            var taxRate = _taxRateRepository.GetEffectiveRate(taxCodeId, date);
            return taxRate?.Rate ?? 0;
        }

        /// <summary>
        /// Helper method to get effective rate for a tax code (async)
        /// </summary>
        private async Task<decimal> GetEffectiveRateForTaxCodeAsync(Guid taxCodeId, DateTime date)
        {
            var taxRate = await _taxRateRepository.GetEffectiveRateAsync(taxCodeId, date);
            return taxRate?.Rate ?? 0;
        }

        #endregion
    }
}
