using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Overtime
{
    /// <summary>
    /// HR Overtime Policy Model
    /// </summary>
    [SugarTable("hr_overtime_policy")]
    public class HrOvertimePolicy : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long OvertimePolicyId { get; set; }

        [SugarColumn(Length = 150, IsNullable = false)]
        public string PolicyName { get; set; }

        // Rates
        public decimal DailyRate { get; set; }
        public decimal WeekendRate { get; set; }
        public decimal HolidayRate { get; set; }

        // Limits
        public int MaxMinutesPerDay { get; set; }
        public int MaxMinutesPerWeek { get; set; }

        // Approval logic
        public bool RequirePreApproval { get; set; }
        public int AutoApproveBelowMinutes { get; set; }

        // Validity
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; }
    }
}
