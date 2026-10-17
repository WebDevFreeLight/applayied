
using ZR.Model.System;

namespace ZR.Model.Hr
{
    /// <summary>
    /// hrinsuranceenrollmentstatus
    /// </summary>
    [SugarTable("hr_insurance_enrollment_status")]
    
    [SqlDict(
        dictType: "insurance_enrollment_status",
        dictName: "Hr Insurance Enrollment Status",
        valueColumn: "Id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "Id ASC",
        Description = "Auto-generated from HrInsuranceEnrollmentStatus table"
    )]
    public class HrInsuranceEnrollmentStatus : SysBase
    {
        /// <summary>
        /// Id 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Id { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; }
    }
}
