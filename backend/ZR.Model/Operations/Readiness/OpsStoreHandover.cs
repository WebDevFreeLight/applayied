using System;
using System.Collections.Generic;

namespace ZR.Model.Operations.Readiness
{
    /// <summary>
    /// Store Handover - تسليم المحل (percentage-based tracking + workflow)
    /// </summary>
    [SugarTable("ops_store_handover")]
    public class OpsStoreHandover : SysBase
    {
        /// <summary>
        /// Handover ID (Snowflake)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "handover_id", IsPrimaryKey = true, IsIdentity = false)]
        public long HandoverId { get; set; }

        /// <summary>
        /// Shop FK → blv_boulevard_shops
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "shop_id", IsNullable = false)]
        public long ShopId { get; set; }

        /// <summary>
        /// Season FK (nullable)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "season_id", IsNullable = true)]
        public long? SeasonId { get; set; }

        /// <summary>
        /// Progress 0.00–100.00 (sum of completed requirement percentages)
        /// </summary>
        [SugarColumn(ColumnName = "progress", DecimalDigits = 2, IsNullable = false)]
        public decimal Progress { get; set; }

        /// <summary>
        /// Status: Pending, InProgress, ReadyForHandover, Approved, Rejected
        /// </summary>
        [SugarColumn(ColumnName = "status", Length = 30, IsNullable = false)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Notes
        /// </summary>
        [SugarColumn(ColumnName = "notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string Notes { get; set; }

        /// <summary>
        /// Who requested handover
        /// </summary>
        [SugarColumn(ColumnName = "requested_by", Length = 100, IsNullable = true)]
        public string RequestedBy { get; set; }

        /// <summary>
        /// When handover was requested
        /// </summary>
        [SugarColumn(ColumnName = "requested_at", IsNullable = true)]
        public DateTime? RequestedAt { get; set; }

        /// <summary>
        /// Who approved handover
        /// </summary>
        [SugarColumn(ColumnName = "approved_by", Length = 100, IsNullable = true)]
        public string ApprovedBy { get; set; }

        /// <summary>
        /// When handover was approved
        /// </summary>
        [SugarColumn(ColumnName = "approved_at", IsNullable = true)]
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Who rejected handover
        /// </summary>
        [SugarColumn(ColumnName = "rejected_by", Length = 100, IsNullable = true)]
        public string RejectedBy { get; set; }

        /// <summary>
        /// When handover was rejected
        /// </summary>
        [SugarColumn(ColumnName = "rejected_at", IsNullable = true)]
        public DateTime? RejectedAt { get; set; }

        /// <summary>
        /// Rejection reason
        /// </summary>
        [SugarColumn(ColumnName = "rejection_reason", Length = 500, IsNullable = true)]
        public string RejectionReason { get; set; }

        // ─── Navigation Properties (not mapped to DB) ───

        [SugarColumn(IsIgnore = true)]
        public string ShopName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ZoneName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int UnitType { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string BrandName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string BrandImage { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string CustomerNameAr { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<OpsStoreHandoverItem> Items { get; set; }
    }
}
