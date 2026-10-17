
using ZR.Model.System;

namespace ZR.Model.HR.Hrinsurancetype
{
    /// <summary>
    /// insurancetype
    /// </summary>
    [SugarTable("hr_insurance_coveragelevel")]
    
    [SqlDict(
        dictType: "insurance_coveragelevel",
        dictName: "Hr Insurance Coveragelevel",
        valueColumn: "Id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "Id ASC",
        Description = "Auto-generated from HrInsuranceCoveragelevel table"
    )]
    public class HrInsuranceCoveragelevel:SysBase
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
