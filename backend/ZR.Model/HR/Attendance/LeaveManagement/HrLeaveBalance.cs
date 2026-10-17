using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.LeaveManagement
{

    /// <summary>
    /// Leave Balance Table
    /// </summary>
    [SugarTable("hr_leave_balance")]
    
    [SqlDict(
        dictType: "leave_balance",
        dictName: "Hr Leave Balance",
        valueColumn: "BalanceId",
        labelColumns: "entitleddays",
        RemarkColumn = "remark",
        OrderBy = "BalanceId ASC",
        Description = "Auto-generated from HrLeaveBalance table"
    )]
    public class HrLeaveBalance : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BalanceId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }

        public int Year { get; set; }

        // ÇáÑÕíÏ ÈÇáÏÞÇÆÞ (ÇáÃÓÇÓí ÇáÂä)
        public decimal EntitledMinutes { get; set; }
        public decimal UsedMinutes { get; set; } = 0;
        public decimal BalanceMinutes { get; set; }

        // ááÊæÇÝÞ ãÚ ÇáßæÏ ÇáÞÏíã (ÇÎÊíÇÑí - computed properties)
        public decimal EntitledDays => EntitledMinutes / 480; // ÇÝÊÑÇÖ 8 ÓÇÚÇÊ Úãá = 480 ÏÞíÞÉ
        public decimal UsedDays => UsedMinutes / 480;
        public decimal AvailableDays => BalanceMinutes / 480;

        public decimal CarriedOverMinutes { get; set; } = 0;
        public decimal ExpiredMinutes { get; set; } = 0;
    }

}

