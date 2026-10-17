using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance
{
    /// <summary>
    /// HR Attendance Audit Log Model
    /// </summary>
    [SugarTable("hr_attendance_audit_log")]
    public class HrAttendanceAuditLog : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long AuditLogId { get; set; }

        public long EmployeeId { get; set; }

        public DateTime ChangeDate { get; set; }

        public string TableName { get; set; } // e.g., hr_attendance_day

        public long? RecordId { get; set; }

        public string ActionType { get; set; } // Insert, Update, Delete, Adjustment

        public long ChangedBy { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public string Reason { get; set; }
    }
}
