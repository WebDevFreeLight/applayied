using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Repository;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// GL Journal Entry Service - خدمة قيود اليومية
    /// </summary>
    [AppService(ServiceType = typeof(IGLJournalEntryService), ServiceLifetime = LifeTime.Transient)]
    public class GLJournalEntryService : BaseService<GLJournalEntry>, IGLJournalEntryService
    {
        private readonly IGLJournalEntryRepository _entryRepository;
        private readonly IGLAccountRepository _accountRepository;

        public GLJournalEntryService(
            IGLJournalEntryRepository entryRepository,
            IGLAccountRepository accountRepository)
        {
            _entryRepository = entryRepository;
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Get paginated list of entries
        /// </summary>
        public PagedInfo<JournalEntryResponseDto> GetList(GLJournalEntryQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLJournalEntry>();
            predicate = predicate.AndIF(query.JournalId.HasValue, it => it.JournalId == query.JournalId.Value);
            predicate = predicate.AndIF(query.AccountId.HasValue, it => it.AccountId == query.AccountId.Value);
            predicate = predicate.AndIF(query.MinDebit.HasValue, it => it.Debit >= query.MinDebit.Value);
            predicate = predicate.AndIF(query.MaxDebit.HasValue, it => it.Debit <= query.MaxDebit.Value);
            predicate = predicate.AndIF(query.MinCredit.HasValue, it => it.Credit >= query.MinCredit.Value);
            predicate = predicate.AndIF(query.MaxCredit.HasValue, it => it.Credit <= query.MaxCredit.Value);
            predicate = predicate.AndIF(query.BeginCreateTime.HasValue, it => it.Create_time >= query.BeginCreateTime.Value);
            predicate = predicate.AndIF(query.EndCreateTime.HasValue, it => it.Create_time <= query.EndCreateTime.Value);

            // Get paged entries first
            var entriesResult = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .ToPage(pager);

            // Get account and journal info for the entries
            var accountIds = entriesResult.Result.Select(e => e.AccountId).Distinct().ToList();
            var journalIds = entriesResult.Result.Select(e => e.JournalId).Distinct().ToList();

            var accounts = Context.Queryable<GLAccount>().Where(a => accountIds.Contains(a.AccountId)).ToList();
            var journals = Context.Queryable<GLJournal>().Where(j => journalIds.Contains(j.Id)).ToList();

            var dtos = entriesResult.Result.Select(e =>
            {
                var account = accounts.FirstOrDefault(a => a.AccountId == e.AccountId);
                var journal = journals.FirstOrDefault(j => j.Id == e.JournalId);
                return new JournalEntryResponseDto
                {
                    Id = e.Id,
                    JournalId = e.JournalId,
                    AccountId = e.AccountId,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    DescriptionAr = e.DescriptionAr,
                    DescriptionEn = e.DescriptionEn,
                    Create_time = e.Create_time,
                    Update_time = e.Update_time,
                    Create_by = e.Create_by,
                    Update_by = e.Update_by,
                    CompanyID = e.CompanyID,
                    AccountNameAr = account?.AccountNameAr,
                    AccountNameEn = account?.AccountNameEn,
                    AccountCode = account?.AccountNumber,
                    JournalNumber = journal?.JournalNumber
                };
            }).ToList();

            return new PagedInfo<JournalEntryResponseDto>
            {
                PageIndex = entriesResult.PageIndex,
                PageSize = entriesResult.PageSize,
                TotalNum = entriesResult.TotalNum,
                Result = dtos
            };
        }

        /// <summary>
        /// Get entry by ID
        /// </summary>
        public JournalEntryResponseDto GetById(Guid id)
        {
            var entry = Context.Queryable<GLJournalEntry, GLAccount, GLJournal>((e, a, j) =>
                    new JoinQueryInfos(
                        JoinType.Left, e.AccountId == a.AccountId,
                        JoinType.Left, e.JournalId == j.Id))
                .Where((e, a, j) => e.Id == id)
                .Select((e, a, j) => new JournalEntryResponseDto
                {
                    Id = e.Id,
                    JournalId = e.JournalId,
                    AccountId = e.AccountId,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    DescriptionAr = e.DescriptionAr,
                    DescriptionEn = e.DescriptionEn,
                    Create_time = e.Create_time,
                    Update_time = e.Update_time,
                    Create_by = e.Create_by,
                    Update_by = e.Update_by,
                    CompanyID = e.CompanyID,
                    AccountNameAr = a.AccountNameAr,
                    AccountNameEn = a.AccountNameEn,
                    AccountCode = a.AccountNumber,
                    JournalNumber = j.JournalNumber
                })
                .First();

            return entry;
        }

        /// <summary>
        /// Create entry
        /// </summary>
        public GLJournalEntry Create(CreateJournalEntryDto dto, string userName)
        {
            var model = dto.Adapt<GLJournalEntry>();
            model.Id = Guid.NewGuid();
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            // Calculate base amounts (in base currency)
            var exchangeRate = model.ExchangeRate > 0 ? model.ExchangeRate : 1m;
            model.BaseDebit = model.Debit * exchangeRate;
            model.BaseCredit = model.Credit * exchangeRate;

            Insert(model);
            return model;
        }

        /// <summary>
        /// Create multiple entries
        /// </summary>
        public int CreateMultiple(List<CreateJournalEntryDto> dtos, Guid journalId, string userName)
        {
            var models = dtos.Select(dto =>
            {
                var model = dto.Adapt<GLJournalEntry>();
                model.Id = Guid.NewGuid();
                model.JournalId = journalId;
                model.Create_by = userName;
                model.Create_time = DateTime.Now;

                // Calculate base amounts (in base currency)
                var exchangeRate = model.ExchangeRate > 0 ? model.ExchangeRate : 1m;
                model.BaseDebit = model.Debit * exchangeRate;
                model.BaseCredit = model.Credit * exchangeRate;

                return model;
            }).ToList();

            return InsertRange(models) ? models.Count : 0;
        }

        /// <summary>
        /// Update entry
        /// </summary>
        public int Update(UpdateJournalEntryDto dto, string userName)
        {
            // Check if journal is not posted
            var journal = Context.Queryable<GLJournal>().First(x => x.Id == dto.JournalId);
            if (journal?.Status == JournalStatus.Posted)
                return 0; // Cannot update entries of posted journals

            var model = dto.Adapt<GLJournalEntry>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            // Calculate base amounts (in base currency)
            var exchangeRate = model.ExchangeRate > 0 ? model.ExchangeRate : 1m;
            model.BaseDebit = model.Debit * exchangeRate;
            model.BaseCredit = model.Credit * exchangeRate;

            return Update(model, it => new
            {
                it.AccountId,
                it.Debit,
                it.Credit,
                it.BaseDebit,
                it.BaseCredit,
                it.DescriptionAr,
                it.DescriptionEn,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete entries
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Check if any entry belongs to a posted journal
            foreach (var id in ids)
            {
                var entry = GetFirst(x => x.Id == id);
                if (entry != null)
                {
                    var journal = Context.Queryable<GLJournal>().First(x => x.Id == entry.JournalId);
                    if (journal?.Status == JournalStatus.Posted)
                        return 0; // Cannot delete entries of posted journals
                }
            }

            return Context.Deleteable<GLJournalEntry>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete entries by journal ID
        /// </summary>
        public int DeleteByJournalId(Guid journalId)
        {
            // Check if journal is not posted
            var journal = Context.Queryable<GLJournal>().First(x => x.Id == journalId);
            if (journal?.Status == JournalStatus.Posted)
                return 0; // Cannot delete entries of posted journals

            return Context.Deleteable<GLJournalEntry>()
                .Where(it => it.JournalId == journalId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get entries by journal ID
        /// </summary>
        public List<JournalEntryResponseDto> GetByJournalId(Guid journalId)
        {
            return Context.Queryable<GLJournalEntry, GLAccount, GLJournal>((e, a, j) =>
                    new JoinQueryInfos(
                        JoinType.Left, e.AccountId == a.AccountId,
                        JoinType.Left, e.JournalId == j.Id))
                .Where((e, a, j) => e.JournalId == journalId)
                .OrderBy((e, a, j) => e.Create_time)
                .Select((e, a, j) => new JournalEntryResponseDto
                {
                    Id = e.Id,
                    JournalId = e.JournalId,
                    AccountId = e.AccountId,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    DescriptionAr = e.DescriptionAr,
                    DescriptionEn = e.DescriptionEn,
                    Create_time = e.Create_time,
                    Update_time = e.Update_time,
                    Create_by = e.Create_by,
                    Update_by = e.Update_by,
                    CompanyID = e.CompanyID,
                    AccountNameAr = a.AccountNameAr,
                    AccountNameEn = a.AccountNameEn,
                    AccountCode = a.AccountNumber,
                    JournalNumber = j.JournalNumber
                })
                .ToList();
        }

        /// <summary>
        /// Get entries by account ID
        /// </summary>
        public List<JournalEntryResponseDto> GetByAccountId(Guid accountId)
        {
            return Context.Queryable<GLJournalEntry, GLAccount, GLJournal>((e, a, j) =>
                    new JoinQueryInfos(
                        JoinType.Left, e.AccountId == a.AccountId,
                        JoinType.Left, e.JournalId == j.Id))
                .Where((e, a, j) => e.AccountId == accountId)
                .OrderByDescending((e, a, j) => e.Create_time)
                .Select((e, a, j) => new JournalEntryResponseDto
                {
                    Id = e.Id,
                    JournalId = e.JournalId,
                    AccountId = e.AccountId,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    DescriptionAr = e.DescriptionAr,
                    DescriptionEn = e.DescriptionEn,
                    Create_time = e.Create_time,
                    Update_time = e.Update_time,
                    Create_by = e.Create_by,
                    Update_by = e.Update_by,
                    CompanyID = e.CompanyID,
                    AccountNameAr = a.AccountNameAr,
                    AccountNameEn = a.AccountNameEn,
                    AccountCode = a.AccountNumber,
                    JournalNumber = j.JournalNumber
                })
                .ToList();
        }

        /// <summary>
        /// Get journal totals
        /// </summary>
        public (decimal TotalDebit, decimal TotalCredit) GetJournalTotals(Guid journalId)
        {
            var entries = Queryable()
                .Where(x => x.JournalId == journalId)
                .ToList();

            return (entries.Sum(e => e.Debit), entries.Sum(e => e.Credit));
        }
    }
}

