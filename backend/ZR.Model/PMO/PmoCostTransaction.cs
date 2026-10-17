using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// A cost/expense entry. Only Posted transactions count as actual spend; Draft is excluded.
    /// Posting captures actor + time and is restricted to authorized users; posted rows are not
    /// silently deleted (void/reverse instead).
    /// </summary>
    [SugarTable("pmo_cost_transaction", "PMO Cost Transaction")]
    [SugarIndex("idx_pmo_tx_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_tx_category", nameof(CategoryId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_tx_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("idx_pmo_tx_date", nameof(TxnDate), OrderByType.Asc)]
    public class PmoCostTransaction : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "transaction_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransactionId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [SugarColumn(ColumnName = "category_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CategoryId { get; set; }

        /// <summary>Must be &gt; 0 (validated server-side).</summary>
        [SugarColumn(ColumnName = "amount", Length = 18, DecimalDigits = 2)]
        public decimal Amount { get; set; }

        [SugarColumn(ColumnName = "txn_date")]
        public DateTime TxnDate { get; set; }

        [SugarColumn(ColumnName = "description", Length = 500, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>Draft | Posted</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "Draft";

        [SugarColumn(ColumnName = "posted_by_user_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PostedByUserId { get; set; }

        [SugarColumn(ColumnName = "posted_at", IsNullable = true)]
        public DateTime? PostedAt { get; set; }

        [SugarColumn(IsIgnore = true)] public string ProjectName { get; set; }
        [SugarColumn(IsIgnore = true)] public string CategoryName { get; set; }
        [SugarColumn(IsIgnore = true)] public string PostedByName { get; set; }
    }
}
