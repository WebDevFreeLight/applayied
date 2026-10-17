 
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR
{
    /// <summary>
    /// Hybrid Work Schedule Table
    /// </summary>
    [SugarTable("hr_hybrid_schedule")]
    public class HrHybridSchedule : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ScheduleId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public DateTime WeekStartDate { get; set; }

        [SugarColumn(Length = 20)]
        public string MondayLocation { get; set; } // office, home, leave

        [SugarColumn(Length = 20)]
        public string TuesdayLocation { get; set; }

        [SugarColumn(Length = 20)]
        public string WednesdayLocation { get; set; }

        [SugarColumn(Length = 20)]
        public string ThursdayLocation { get; set; }

        [SugarColumn(Length = 20)]
        public string FridayLocation { get; set; }

        [SugarColumn(Length = 20)]
        public string SaturdayLocation { get; set; }

        [SugarColumn(Length = 20)]
        public string SundayLocation { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, approved, rejected

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApproverId { get; set; }

        public DateTime? ApproveDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Comments { get; set; }
    }

    /// <summary>
    /// Employee Warnings Table
    /// </summary>
    [SugarTable("hr_warning")]
    public class HrWarning : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WarningId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 20)]
        public string WarningType { get; set; } // verbal, written, final

        [SugarColumn(Length = 500)]
        public string Reason { get; set; }
        public DateTime IssueDate { get; set; }

        [SugarColumn(Length = 100)]
        public string IssuedBy { get; set; }
        public string Details { get; set; }

        [SugarColumn(Length = 500)]
        public string ActionTaken { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // active, resolved, expired

        public DateTime? ExpiryDate { get; set; }

    }
}

