using ZR.Model.System;

namespace ZR.Model.HR.Attendance.Configuration
{
    /// <summary>
    /// Holiday Table
    /// </summary>
    [SugarTable("hr_holiday")]
    
    [SqlDict(
        dictType: "holiday",
        dictName: "Hr Holiday - أنواع",
        valueColumn: "Holidayid",
        labelColumns: "Holidayid, ' - ', holidayname, ' - ', holidaynamear",
        RemarkColumn = "description",
        OrderBy = "Holidayid ASC",
        Description = "Auto-generated from HrHoliday table"
    )]
    public class HrHoliday:SysBase
    {
        /// <summary>
        /// Holidayid 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Holidayid { get; set; }

        /// <summary>
        /// Holidaydate 
        /// </summary>
        public DateTime Holidaydate { get; set; }

        /// <summary>
        /// Holidayname 
        /// </summary>
        public string Holidayname { get; set; }

        /// <summary>
        /// Holidaynamear 
        /// </summary>
        public string Holidaynamear { get; set; }

        /// <summary>
        /// Holidaytype 
        /// </summary>
        public string Holidaytype { get; set; }

        /// <summary>
        /// Ispaid 
        /// </summary>
        public string Ispaid { get; set; }

        /// <summary>
        /// Year 
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Description 
        /// </summary>
        public string Description { get; set; } 

    }
}
