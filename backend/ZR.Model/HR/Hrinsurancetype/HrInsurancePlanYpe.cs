
using ZR.Model.System;

namespace ZR.Model.HR.Hrinsurancetype
{
    /// <summary>
    /// hrinsuranceplanype
    /// </summary>
    [SugarTable("hr_insurance_plan_ype")]
    
    [SqlDict(
        dictType: "insurance_plan_ype",
        dictName: "Hr Insurance Plan Ype",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from HrInsurancePlanYpe table"
    )]
    public class HrInsurancePlanYpe:SysBase
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
