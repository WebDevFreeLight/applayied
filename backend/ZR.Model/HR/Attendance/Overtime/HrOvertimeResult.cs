using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Overtime
{
    [SugarTable("hr_overtime_result")]
    public class HrOvertimeResult : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long OvertimeResultId { get; set; }

        public long EmployeeId { get; set; }
        public DateTime WorkDate { get; set; }

        public int ApprovedMinutes { get; set; }

        public decimal Rate { get; set; }
        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }
    }
}
