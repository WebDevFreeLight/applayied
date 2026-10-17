using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Converter;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.AttendanceProcessing
{
    /// <summary>
    /// HR Attendance Adjustment Model
    /// Manual corrections for attendance records (HR/Admin use)
    /// </summary>
    [SugarTable("hr_attendance_adjustment")]
    public class HrAttendanceAdjustment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long AdjustmentId { get; set; }

        public long EmployeeId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public long? ShiftId { get; set; }

        public long? SegmentId { get; set; } // Null if adjustment for whole day

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AdjustmentTypeDictCode { get; set; } // e.g., ForgotCheckIn, ManualOvertime, LateForgiven, etc.

        public int AdjustedMinutes { get; set; } // Positive or negative

        public string AdjustmentField { get; set; } // e.g., WorkedMinutes, LateMinutes, OvertimeMinutes

        public string Reason { get; set; }

        public long RequestedBy { get; set; } // Usually the employee or manager

        public long? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusDictCode { get; set; } // Pending, Approved, Rejected

        public bool AppliedToSnapshot { get; set; } // Flag after processing

        public string Remark { get; set; }

        // Compatibility properties
        public DateTime AdjustmentDate { get => AttendanceDate; set => AttendanceDate = value; }
        public bool IsApproved
        {
            get => ApprovedBy.HasValue && ApprovalDate.HasValue;
            set
            {
                if (value && !ApprovedBy.HasValue)
                {
                    ApprovalDate = DateTime.Now;
                }
            }
        }
    }
}
