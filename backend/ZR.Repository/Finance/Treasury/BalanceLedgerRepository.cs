using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Balance Ledger Repository - مستودع سجل الرصيد
    /// Provides data access for balance ledger entries
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BalanceLedgerRepository : BaseRepository<BalanceLedger>
    {
        /// <summary>
        /// Get ledger entries for an account within a date range
        /// </summary>
        public List<BalanceLedger> GetAccountLedger(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var predicate = Expressionable.Create<BalanceLedger>();
            predicate = predicate.And(x => x.AccountId == accountId);

            if (fromDate.HasValue)
            {
                predicate = predicate.And(x => x.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                predicate = predicate.And(x => x.TransactionDate <= toDate.Value);
            }

            return GetList(predicate.ToExpression())
                .OrderBy(x => x.TransactionDate)
                .ThenBy(x => x.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Get the latest ledger entry for an account (for balance verification)
        /// </summary>
        public BalanceLedger GetLatestEntry(Guid accountId)
        {
            var predicate = Expressionable.Create<BalanceLedger>();
            predicate = predicate.And(x => x.AccountId == accountId);

            return GetList(predicate.ToExpression())
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get ledger entries for a reference entity (e.g., all entries for a payment)
        /// </summary>
        public List<BalanceLedger> GetByReferenceEntity(Guid referenceEntityId)
        {
            var predicate = Expressionable.Create<BalanceLedger>();
            predicate = predicate.And(x => x.ReferenceEntityId == referenceEntityId);

            return GetList(predicate.ToExpression())
                .OrderBy(x => x.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Calculate running balance from ledger entries
        /// </summary>
        public decimal CalculateBalanceFromLedger(Guid accountId, DateTime? asOfDate = null)
        {
            var predicate = Expressionable.Create<BalanceLedger>();
            predicate = predicate.And(x => x.AccountId == accountId);

            if (asOfDate.HasValue)
            {
                predicate = predicate.And(x => x.TransactionDate <= asOfDate.Value);
            }

            var entries = GetList(predicate.ToExpression());

            // Sum all net amounts
            return entries.Sum(x => x.NetAmount);
        }

        /// <summary>
        /// Get unreconciled ledger entries for an account
        /// </summary>
        public List<BalanceLedger> GetUnreconciledEntries(Guid accountId)
        {
            var predicate = Expressionable.Create<BalanceLedger>();
            predicate = predicate.And(x => x.AccountId == accountId);
            predicate = predicate.And(x => !x.IsReconciled);

            return GetList(predicate.ToExpression())
                .OrderBy(x => x.TransactionDate)
                .ToList();
        }

        /// <summary>
        /// Get ledger entries for reconciliation
        /// </summary>
        public List<BalanceLedger> GetEntriesForReconciliation(Guid accountId, DateTime periodStart, DateTime periodEnd)
        {
            var predicate = Expressionable.Create<BalanceLedger>();
            predicate = predicate.And(x => x.AccountId == accountId);
            predicate = predicate.And(x => x.TransactionDate >= periodStart);
            predicate = predicate.And(x => x.TransactionDate <= periodEnd);

            return GetList(predicate.ToExpression())
                .OrderBy(x => x.TransactionDate)
                .ThenBy(x => x.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Mark entries as reconciled
        /// </summary>
        public int MarkAsReconciled(List<Guid> entryIds, Guid reconciliationId)
        {
            var count = 0;
            foreach (var entryId in entryIds)
            {
                var entry = GetById(entryId);
                if (entry != null)
                {
                    entry.IsReconciled = true;
                    entry.ReconciliationId = reconciliationId;
                    Update(entry);
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Get summary by transaction type for reporting
        /// </summary>
        public Dictionary<BalanceLedgerTransactionType, decimal> GetSummaryByTransactionType(
            Guid accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var entries = GetAccountLedger(accountId, fromDate, toDate);

            return entries
                .GroupBy(x => x.TransactionType)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.NetAmount));
        }
    }
}
