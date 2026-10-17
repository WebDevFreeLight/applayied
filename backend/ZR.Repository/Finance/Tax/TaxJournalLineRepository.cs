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
    /// Tax Journal Line Repository Implementation
    /// تنفيذ مستودع سطور دفتر الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxJournalLineRepository), ServiceLifetime = LifeTime.Transient)]
    public class TaxJournalLineRepository : BaseRepository<TaxJournalLine>, ITaxJournalLineRepository
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax journal lines with tenant filtering
        /// </summary>
        public PagedInfo<TaxJournalLine> GetList(TaxJournalLine parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<TaxJournalLine>();

            // Tenant filtering - CRITICAL for multi-tenant isolation
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            // Filter by journal
            predicate = predicate.AndIF(parm.JournalId.HasValue, it => it.JournalId == parm.JournalId);

            // Filter by invoice
            predicate = predicate.AndIF(parm.InvoiceId.HasValue, it => it.InvoiceId == parm.InvoiceId);

            // Filter by source module
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.SourceModule),
                it => it.SourceModule == parm.SourceModule);

            // Filter by tax code
            predicate = predicate.AndIF(parm.TaxCodeId != Guid.Empty, it => it.TaxCodeId == parm.TaxCodeId);

            // Filter by posted status
            predicate = predicate.AndIF(parm.IsPosted, it => it.IsPosted == parm.IsPosted);

            // Filter by recoverable status
            predicate = predicate.AndIF(parm.IsRecoverable, it => it.IsRecoverable == parm.IsRecoverable);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.TaxDate)
                .OrderBy(x => x.Create_time)
                .ToPage(pager);
        }

        /// <summary>
        /// Get tax journal line by ID
        /// </summary>
        public TaxJournalLine GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get tax journal line by ID (async)
        /// </summary>
        public async Task<TaxJournalLine> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Insert tax journal line
        /// </summary>
        public int InsertTaxJournalLine(TaxJournalLine model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
            }
            return Insert(model);
        }

        /// <summary>
        /// Insert tax journal line (async)
        /// </summary>
        public async Task<int> InsertTaxJournalLineAsync(TaxJournalLine model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
            }
            var result = await InsertAsync(model);
            return result ? 1 : 0;
        }

        /// <summary>
        /// Insert multiple tax journal lines (batch)
        /// </summary>
        public int InsertTaxJournalLines(List<TaxJournalLine> models)
        {
            foreach (var model in models)
            {
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }
            }
            return Context.Insertable(models).ExecuteCommand();
        }

        /// <summary>
        /// Insert multiple tax journal lines (async, batch)
        /// </summary>
        public async Task<int> InsertTaxJournalLinesAsync(List<TaxJournalLine> models)
        {
            foreach (var model in models)
            {
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }
            }
            return await Context.Insertable(models).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update tax journal line (only allowed if not posted)
        /// </summary>
        public int UpdateTaxJournalLine(TaxJournalLine model)
        {
            // Check if posted - immutable if posted
            var existing = GetById(model.Id);
            if (existing != null && existing.IsPosted)
            {
                throw new InvalidOperationException("Cannot modify posted tax journal line - لا يمكن تعديل سطر الضريبة المرحّل");
            }

            return Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time, it.IsPosted, it.PostedAt, it.PostedBy })
                .ExecuteCommand();
        }

        /// <summary>
        /// Update tax journal line (async, only allowed if not posted)
        /// </summary>
        public async Task<int> UpdateTaxJournalLineAsync(TaxJournalLine model)
        {
            var existing = await GetByIdAsync(model.Id);
            if (existing != null && existing.IsPosted)
            {
                throw new InvalidOperationException("Cannot modify posted tax journal line - لا يمكن تعديل سطر الضريبة المرحّل");
            }

            return await Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time, it.IsPosted, it.PostedAt, it.PostedBy })
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete tax journal lines by IDs (only allowed if not posted)
        /// </summary>
        public int DeleteTaxJournalLines(Guid[] ids)
        {
            // Check for posted lines
            var postedCount = Queryable()
                .Where(x => ids.Contains(x.Id) && x.IsPosted)
                .Count();

            if (postedCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete {postedCount} posted tax journal line(s) - لا يمكن حذف سطور الضريبة المرحّلة");
            }

            return Context.Deleteable<TaxJournalLine>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete tax journal lines by IDs (async, only allowed if not posted)
        /// </summary>
        public async Task<int> DeleteTaxJournalLinesAsync(Guid[] ids)
        {
            var postedCount = await Context.Queryable<TaxJournalLine>()
                .Where(x => ids.Contains(x.Id) && x.IsPosted)
                .CountAsync();

            if (postedCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete {postedCount} posted tax journal line(s) - لا يمكن حذف سطور الضريبة المرحّلة");
            }

            return await Context.Deleteable<TaxJournalLine>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommandAsync();
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get tax journal lines by journal ID
        /// </summary>
        public List<TaxJournalLine> GetByJournalId(Guid journalId)
        {
            return Queryable()
                .Where(x => x.JournalId == journalId)
                .OrderBy(x => x.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get tax journal lines by journal ID (async)
        /// </summary>
        public async Task<List<TaxJournalLine>> GetByJournalIdAsync(Guid journalId)
        {
            return await Context.Queryable<TaxJournalLine>()
                .Where(x => x.JournalId == journalId)
                .OrderBy(x => x.Create_time)
                .ToListAsync();
        }

        /// <summary>
        /// Get tax journal lines by invoice ID
        /// </summary>
        public List<TaxJournalLine> GetByInvoiceId(Guid invoiceId)
        {
            return Queryable()
                .Where(x => x.InvoiceId == invoiceId)
                .OrderBy(x => x.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get tax journal lines by invoice ID (async)
        /// </summary>
        public async Task<List<TaxJournalLine>> GetByInvoiceIdAsync(Guid invoiceId)
        {
            return await Context.Queryable<TaxJournalLine>()
                .Where(x => x.InvoiceId == invoiceId)
                .OrderBy(x => x.Create_time)
                .ToListAsync();
        }

        /// <summary>
        /// Get tax journal lines by source document
        /// </summary>
        public List<TaxJournalLine> GetBySourceDocument(string sourceModule, Guid sourceDocumentId)
        {
            return Queryable()
                .Where(x => x.SourceModule == sourceModule && x.SourceDocumentId == sourceDocumentId)
                .OrderBy(x => x.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get tax journal lines by source document (async)
        /// </summary>
        public async Task<List<TaxJournalLine>> GetBySourceDocumentAsync(string sourceModule, Guid sourceDocumentId)
        {
            return await Context.Queryable<TaxJournalLine>()
                .Where(x => x.SourceModule == sourceModule && x.SourceDocumentId == sourceDocumentId)
                .OrderBy(x => x.Create_time)
                .ToListAsync();
        }

        /// <summary>
        /// Get tax journal lines by tax code
        /// </summary>
        public List<TaxJournalLine> GetByTaxCode(Guid taxCodeId, long? companyId = null)
        {
            var query = Queryable().Where(x => x.TaxCodeId == taxCodeId);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderByDescending(x => x.TaxDate).ToList();
        }

        /// <summary>
        /// Get tax journal lines by date range
        /// </summary>
        public List<TaxJournalLine> GetByDateRange(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.TaxDate).ToList();
        }

        /// <summary>
        /// Get tax journal lines by date range (async)
        /// </summary>
        public async Task<List<TaxJournalLine>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Context.Queryable<TaxJournalLine>()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return await query.OrderBy(x => x.TaxDate).ToListAsync();
        }

        /// <summary>
        /// Get unposted tax journal lines
        /// </summary>
        public List<TaxJournalLine> GetUnposted(long? companyId = null)
        {
            var query = Queryable().Where(x => !x.IsPosted);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.TaxDate).ToList();
        }

        /// <summary>
        /// Get unposted tax journal lines (async)
        /// </summary>
        public async Task<List<TaxJournalLine>> GetUnpostedAsync(long? companyId = null)
        {
            var query = Context.Queryable<TaxJournalLine>().Where(x => !x.IsPosted);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return await query.OrderBy(x => x.TaxDate).ToListAsync();
        }

        #endregion

        #region Posting Operations

        /// <summary>
        /// Mark tax journal lines as posted
        /// </summary>
        public int MarkAsPosted(Guid[] ids, string postedBy)
        {
            return Context.Updateable<TaxJournalLine>()
                .SetColumns(it => new TaxJournalLine
                {
                    IsPosted = true,
                    PostedAt = DateTime.Now,
                    PostedBy = postedBy
                })
                .Where(it => ids.Contains(it.Id) && !it.IsPosted)
                .ExecuteCommand();
        }

        /// <summary>
        /// Mark tax journal lines as posted (async)
        /// </summary>
        public async Task<int> MarkAsPostedAsync(Guid[] ids, string postedBy)
        {
            return await Context.Updateable<TaxJournalLine>()
                .SetColumns(it => new TaxJournalLine
                {
                    IsPosted = true,
                    PostedAt = DateTime.Now,
                    PostedBy = postedBy
                })
                .Where(it => ids.Contains(it.Id) && !it.IsPosted)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Mark tax journal lines by journal ID as posted
        /// </summary>
        public int MarkJournalLinesAsPosted(Guid journalId, string postedBy)
        {
            return Context.Updateable<TaxJournalLine>()
                .SetColumns(it => new TaxJournalLine
                {
                    IsPosted = true,
                    PostedAt = DateTime.Now,
                    PostedBy = postedBy
                })
                .Where(it => it.JournalId == journalId && !it.IsPosted)
                .ExecuteCommand();
        }

        /// <summary>
        /// Mark tax journal lines by journal ID as posted (async)
        /// </summary>
        public async Task<int> MarkJournalLinesAsPostedAsync(Guid journalId, string postedBy)
        {
            return await Context.Updateable<TaxJournalLine>()
                .SetColumns(it => new TaxJournalLine
                {
                    IsPosted = true,
                    PostedAt = DateTime.Now,
                    PostedBy = postedBy
                })
                .Where(it => it.JournalId == journalId && !it.IsPosted)
                .ExecuteCommandAsync();
        }

        #endregion

        #region Summary & Reporting Operations

        /// <summary>
        /// Get tax summary by tax code for date range
        /// </summary>
        public List<TaxSummary> GetTaxSummaryByCode(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate && x.IsPosted);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            return query
                .GroupBy(x => new { x.TaxCodeId, x.TaxRate })
                .Select(g => new TaxSummary
                {
                    TaxCodeId = g.TaxCodeId,
                    TaxRate = g.TaxRate,
                    TransactionCount = SqlFunc.AggregateCount(g.Id),
                    TotalTaxableBase = SqlFunc.AggregateSum(g.TaxableAmountBase),
                    TotalTaxBase = SqlFunc.AggregateSum(g.TaxAmountBase),
                    TotalTaxableForeign = SqlFunc.AggregateSum(g.TaxableAmountForeign ?? 0),
                    TotalTaxForeign = SqlFunc.AggregateSum(g.TaxAmountForeign ?? 0)
                })
                .ToList();
        }

        /// <summary>
        /// Get tax summary by tax code for date range (async)
        /// </summary>
        public async Task<List<TaxSummary>> GetTaxSummaryByCodeAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Context.Queryable<TaxJournalLine>()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate && x.IsPosted);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            return await query
                .GroupBy(x => new { x.TaxCodeId, x.TaxRate })
                .Select(g => new TaxSummary
                {
                    TaxCodeId = g.TaxCodeId,
                    TaxRate = g.TaxRate,
                    TransactionCount = SqlFunc.AggregateCount(g.Id),
                    TotalTaxableBase = SqlFunc.AggregateSum(g.TaxableAmountBase),
                    TotalTaxBase = SqlFunc.AggregateSum(g.TaxAmountBase),
                    TotalTaxableForeign = SqlFunc.AggregateSum(g.TaxableAmountForeign ?? 0),
                    TotalTaxForeign = SqlFunc.AggregateSum(g.TaxAmountForeign ?? 0)
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get total tax amounts for a period
        /// </summary>
        public TaxPeriodTotals GetPeriodTotals(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate && x.IsPosted);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            var lines = query.ToList();

            return new TaxPeriodTotals
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalTransactions = lines.Count,
                TotalTaxableBase = lines.Sum(x => x.TaxableAmountBase),
                TotalTaxBase = lines.Sum(x => x.TaxAmountBase),
                TotalRecoverableTax = lines.Where(x => x.IsRecoverable).Sum(x => x.TaxAmountBase),
                TotalPayableTax = lines.Where(x => !x.IsRecoverable).Sum(x => x.TaxAmountBase),
                NetTaxPosition = lines.Where(x => !x.IsRecoverable).Sum(x => x.TaxAmountBase) -
                                 lines.Where(x => x.IsRecoverable).Sum(x => x.TaxAmountBase)
            };
        }

        /// <summary>
        /// Get total tax amounts for a period (async)
        /// </summary>
        public async Task<TaxPeriodTotals> GetPeriodTotalsAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Context.Queryable<TaxJournalLine>()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate && x.IsPosted);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            var lines = await query.ToListAsync();

            return new TaxPeriodTotals
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalTransactions = lines.Count,
                TotalTaxableBase = lines.Sum(x => x.TaxableAmountBase),
                TotalTaxBase = lines.Sum(x => x.TaxAmountBase),
                TotalRecoverableTax = lines.Where(x => x.IsRecoverable).Sum(x => x.TaxAmountBase),
                TotalPayableTax = lines.Where(x => !x.IsRecoverable).Sum(x => x.TaxAmountBase),
                NetTaxPosition = lines.Where(x => !x.IsRecoverable).Sum(x => x.TaxAmountBase) -
                                 lines.Where(x => x.IsRecoverable).Sum(x => x.TaxAmountBase)
            };
        }

        /// <summary>
        /// Get recoverable vs payable tax totals
        /// </summary>
        public TaxRecoverableSummary GetRecoverableSummary(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate && x.IsPosted);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            var lines = query.ToList();

            var recoverable = lines.Where(x => x.IsRecoverable).ToList();
            var payable = lines.Where(x => !x.IsRecoverable).ToList();

            return new TaxRecoverableSummary
            {
                TotalRecoverable = recoverable.Sum(x => x.TaxAmountBase),
                TotalPayable = payable.Sum(x => x.TaxAmountBase),
                NetPosition = payable.Sum(x => x.TaxAmountBase) - recoverable.Sum(x => x.TaxAmountBase),
                RecoverableCount = recoverable.Count,
                PayableCount = payable.Count
            };
        }

        /// <summary>
        /// Get recoverable vs payable tax totals (async)
        /// </summary>
        public async Task<TaxRecoverableSummary> GetRecoverableSummaryAsync(DateTime fromDate, DateTime toDate, long? companyId = null)
        {
            var query = Context.Queryable<TaxJournalLine>()
                .Where(x => x.TaxDate >= fromDate && x.TaxDate <= toDate && x.IsPosted);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            var lines = await query.ToListAsync();

            var recoverable = lines.Where(x => x.IsRecoverable).ToList();
            var payable = lines.Where(x => !x.IsRecoverable).ToList();

            return new TaxRecoverableSummary
            {
                TotalRecoverable = recoverable.Sum(x => x.TaxAmountBase),
                TotalPayable = payable.Sum(x => x.TaxAmountBase),
                NetPosition = payable.Sum(x => x.TaxAmountBase) - recoverable.Sum(x => x.TaxAmountBase),
                RecoverableCount = recoverable.Count,
                PayableCount = payable.Count
            };
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if any lines exist for a journal
        /// </summary>
        public bool HasLinesForJournal(Guid journalId)
        {
            return Queryable().Any(x => x.JournalId == journalId);
        }

        /// <summary>
        /// Check if any lines exist for a journal (async)
        /// </summary>
        public async Task<bool> HasLinesForJournalAsync(Guid journalId)
        {
            return await Context.Queryable<TaxJournalLine>().AnyAsync(x => x.JournalId == journalId);
        }

        /// <summary>
        /// Check if tax journal line can be modified (not posted)
        /// </summary>
        public bool CanModify(Guid id)
        {
            var line = GetById(id);
            return line != null && !line.IsPosted;
        }

        /// <summary>
        /// Check if tax journal line can be modified (async)
        /// </summary>
        public async Task<bool> CanModifyAsync(Guid id)
        {
            var line = await GetByIdAsync(id);
            return line != null && !line.IsPosted;
        }

        #endregion
    }
}
