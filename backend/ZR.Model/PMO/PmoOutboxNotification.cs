using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// In-app notification outbox populated by automation rules. dedupe_key is UNIQUE so the
    /// same notification is never sent twice even if the worker retries.
    /// </summary>
    [SugarTable("pmo_outbox_notification", "PMO Outbox Notification")]
    [SugarIndex("uk_pmo_outbox_dedupe", nameof(DedupeKey), OrderByType.Asc, true)]
    [SugarIndex("idx_pmo_outbox_target", nameof(TargetUserId), OrderByType.Asc)]
    public class PmoOutboxNotification : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "notification_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }

        /// <summary>e.g. OverdueEscalation | OverrunAlert | MonthEndSummary</summary>
        [SugarColumn(ColumnName = "type", Length = 40)]
        public string Type { get; set; }

        [SugarColumn(ColumnName = "subject", Length = 200)]
        public string Subject { get; set; }

        [SugarColumn(ColumnName = "body", Length = 2000, IsNullable = true)]
        public string Body { get; set; }

        [SugarColumn(ColumnName = "target_user_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TargetUserId { get; set; }

        [SugarColumn(ColumnName = "created_at")]
        public DateTime CreatedAt { get; set; }

        [SugarColumn(ColumnName = "sent_at", IsNullable = true)]
        public DateTime? SentAt { get; set; }

        /// <summary>Pending | Sent | Failed</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "Pending";

        [SugarColumn(ColumnName = "dedupe_key", Length = 128)]
        public string DedupeKey { get; set; }
    }
}
