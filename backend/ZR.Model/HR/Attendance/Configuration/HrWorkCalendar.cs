using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Configuration
{
    /// <summary>
    /// HR Work Calendar Model
    /// </summary>
    [SugarTable("hr_work_calendar")]
    public class HrWorkCalendar : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long CalendarId { get; set; }

        public string CalendarName { get; set; }

        public string WorkingDaysPattern { get; set; } // e.g., "Sun-Thu" or bitmask

        public bool SundayIsWorking { get; set; }

        public bool MondayIsWorking { get; set; }

        public bool TuesdayIsWorking { get; set; }

        public bool WednesdayIsWorking { get; set; }

        public bool ThursdayIsWorking { get; set; }

        public bool FridayIsWorking { get; set; }

        public bool SaturdayIsWorking { get; set; }

        public TimeSpan? DefaultStartTime { get; set; }

        public TimeSpan? DefaultEndTime { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }
    }
}
