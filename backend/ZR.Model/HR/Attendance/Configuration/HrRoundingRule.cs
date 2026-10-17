using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Configuration
{
    /// <summary>
    /// HR Rounding Rule Model
    /// </summary>
    [SugarTable("hr_rounding_rule")]
    public class HrRoundingRule : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long RoundingRuleId { get; set; }

        public string RuleName { get; set; }

        public int IntervalMinutes { get; set; } // e.g., 5, 15, 30

        public bool RoundUp { get; set; } // true = up, false = down

        public bool RoundToNearest { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
