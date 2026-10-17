using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Risk or Issue. Exposure score = probability × impact (set by the service).
    /// </summary>
    [SugarTable("pmo_risk", "PMO Risk/Issue")]
    [SugarIndex("idx_pmo_risk_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_risk_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("idx_pmo_risk_score", nameof(Score), OrderByType.Desc)]
    public class PmoRisk : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "risk_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RiskId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        /// <summary>Risk | Issue</summary>
        [SugarColumn(ColumnName = "kind", Length = 10)]
        public string Kind { get; set; } = "Risk";

        [SugarColumn(ColumnName = "title", Length = 200)]
        public string Title { get; set; }

        [SugarColumn(ColumnName = "owner_user_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OwnerUserId { get; set; }

        /// <summary>1–5</summary>
        [SugarColumn(ColumnName = "probability")]
        public int Probability { get; set; }

        /// <summary>1–5</summary>
        [SugarColumn(ColumnName = "impact")]
        public int Impact { get; set; }

        /// <summary>probability × impact (1–25). Set server-side; never trusted from client.</summary>
        [SugarColumn(ColumnName = "score")]
        public int Score { get; set; }

        [SugarColumn(ColumnName = "mitigation", Length = 2000, IsNullable = true)]
        public string Mitigation { get; set; }

        [SugarColumn(ColumnName = "due_date", IsNullable = true)]
        public DateTime? DueDate { get; set; }

        /// <summary>Open | Mitigating | Closed</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "Open";

        [SugarColumn(IsIgnore = true)] public string OwnerName { get; set; }
        [SugarColumn(IsIgnore = true)] public string ProjectName { get; set; }
    }
}
