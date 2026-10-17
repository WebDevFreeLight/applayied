using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Shifts_Scheduling
{
    /// <summary>
    /// HR Employee Shift Assignment Model
    /// </summary>
    [SugarTable("hr_employee_shift_assignment")]
    public class HrEmployeeShiftAssignment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long AssignmentId { get; set; }

        public long EmployeeId { get; set; }

        public long ShiftId { get; set; }

        public long? PolicyId { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsPrimary { get; set; }
    }
}
