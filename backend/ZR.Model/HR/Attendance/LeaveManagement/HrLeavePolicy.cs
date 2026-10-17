using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.LeaveManagement
{
    /// <summary>
    /// HR Leave Policy Model
    /// </summary>
    [SugarTable("hr_leave_policy")]
    public class HrLeavePolicy : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long LeavePolicyId { get; set; }

        public string PolicyName { get; set; }

        public int MaxAnnualDays { get; set; }

        public int MaxSickDays { get; set; }

        public bool AllowCarryForward { get; set; }

        public int MaxCarryForwardDays { get; set; }

        public bool RequireMedicalCertificateForSick { get; set; }

        public int MinAdvanceNoticeDays { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccrualMethodDictCode { get; set; } // Monthly, Yearly, etc.

        public bool IsActive { get; set; }
    }
}
