using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    // ---- Project members ----------------------------------------------------
    public class PmoMemberQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long? ProjectId { get; set; }
    }

    public class PmoMemberInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long MemberId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long UserId { get; set; }
        public string RoleInProject { get; set; }
    }

    // ---- Progress snapshots -------------------------------------------------
    public class PmoSnapshotQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long? ProjectId { get; set; }
    }

    // ---- Automation runs / outbox -------------------------------------------
    public class PmoAutomationQueryDto : PagerInfo
    {
        public string RuleKey { get; set; }
        public string Status { get; set; }
    }

    public class PmoOutboxQueryDto : PagerInfo
    {
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long? TargetUserId { get; set; }
    }

    // ---- Audit events -------------------------------------------------------
    public class PmoAuditQueryDto : PagerInfo
    {
        public string EntityType { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long? EntityId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long? ActorUserId { get; set; }
    }
}
