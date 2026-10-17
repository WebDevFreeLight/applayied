using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Append-only audit of every change-request state transition.
    /// Written atomically with the change-request update inside one DB transaction.
    /// </summary>
    [SugarTable("pmo_change_history", "PMO Change History")]
    [SugarIndex("idx_pmo_ch_cr", nameof(ChangeRequestId), OrderByType.Asc)]
    public class PmoChangeHistory : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "history_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        [SugarColumn(ColumnName = "change_request_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ChangeRequestId { get; set; }

        [SugarColumn(ColumnName = "from_status", Length = 20, IsNullable = true)]
        public string FromStatus { get; set; }

        [SugarColumn(ColumnName = "to_status", Length = 20)]
        public string ToStatus { get; set; }

        [SugarColumn(ColumnName = "actor_user_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActorUserId { get; set; }

        [SugarColumn(ColumnName = "reason", Length = 1000, IsNullable = true)]
        public string Reason { get; set; }

        [SugarColumn(ColumnName = "acted_at")]
        public DateTime ActedAt { get; set; }

        [SugarColumn(IsIgnore = true)] public string ActorName { get; set; }
    }
}
