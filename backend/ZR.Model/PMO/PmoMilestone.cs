using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Project milestone. Dashboard surfaces milestones due within 14 days and missed ones.
    /// </summary>
    [SugarTable("pmo_milestone", "PMO Milestone")]
    [SugarIndex("idx_pmo_milestone_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_milestone_due", nameof(DueDate), OrderByType.Asc)]
    public class PmoMilestone : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "milestone_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MilestoneId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [SugarColumn(ColumnName = "name", Length = 200)]
        public string Name { get; set; }

        [SugarColumn(ColumnName = "due_date", IsNullable = true)]
        public DateTime? DueDate { get; set; }

        /// <summary>Pending | Achieved | Missed</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "Pending";

        [SugarColumn(IsIgnore = true)] public string ProjectName { get; set; }
    }
}
