using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Overtime
{
    [SugarTable("hr_overtime_verification")]
    public class HrOvertimeVerification : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long VerificationId { get; set; }

        public long SegmentId { get; set; }

        public bool IsVerified { get; set; }

        [SugarColumn(Length = 50)]
        public string VerificationMethod { get; set; }

        public long? VerifiedByEmployeeId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [SugarColumn(Length = 300)]
        public string Notes { get; set; }
    }

}
