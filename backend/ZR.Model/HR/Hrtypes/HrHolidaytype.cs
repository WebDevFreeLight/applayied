
using ZR.Model.System;

namespace ZR.Model.HR.Hrtypes
{
    /// <summary>
    /// HolidayType Table
    /// </summary>
    [SugarTable("hr_holidaytype")]
    
    [SqlDict(
        dictType: "holidaytype",
        dictName: "Hr Holidaytype",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from HrHolidaytype table"
    )]
    public class HrHolidaytype : SysBase
    {
        /// <summary>
        /// Id 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Id { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; } 
    }
}
