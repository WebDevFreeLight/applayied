
using ZR.Model.System;

namespace ZR.Model.HR.Business
{
    /// <summary>
    /// Reviewtype Information Table
    /// </summary>
    [SugarTable("hr_reviewtype")]

    [SqlDict(
        dictType: "reviewtype",
        dictName: "Hr Reviewtype",
        valueColumn: "Reviewtypeid",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "Reviewtypeid ASC",
        Description = "Auto-generated from HrReviewtype table"
    )]
    public class HrReviewtype:SysBase
    {
        /// <summary>
        /// Review Type ID
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Reviewtypeid { get; set; }

        /// <summary>
        /// Review  Type name
        /// </summary>
        public string Name { get; set; }
    }
}
