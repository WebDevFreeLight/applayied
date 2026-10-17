using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Project membership — which users may act on a project (drives Contributor scope).
    /// </summary>
    [SugarTable("pmo_project_member", "PMO Project Member")]
    [SugarIndex("uk_pmo_member", nameof(ProjectId), OrderByType.Asc, nameof(UserId), OrderByType.Asc, true)]
    public class PmoProjectMember : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "member_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [SugarColumn(ColumnName = "user_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }

        /// <summary>e.g. Manager | Contributor | Reviewer</summary>
        [SugarColumn(ColumnName = "role_in_project", Length = 40)]
        public string RoleInProject { get; set; }

        [SugarColumn(IsIgnore = true)] public string UserName { get; set; }
    }
}
