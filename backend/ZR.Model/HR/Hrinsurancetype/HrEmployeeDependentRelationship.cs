
using ZR.Model.System;

namespace ZR.Model.HR.Hrinsurancetype
{
    /// <summary>
    /// employeeependentrelationship
    /// </summary>
    [SugarTable("hr_employee_dependent_relationship")]
    
    [SqlDict(
        dictType: "employee_dependent_relationship",
        dictName: "Hr Employee Dependent Relationship",
        valueColumn: "Dependentid",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "Dependentid ASC",
        Description = "Auto-generated from HrEmployeeDependentRelationship table"
    )]
    public class HrEmployeeDependentRelationship:SysBase
    {
        /// <summary>
        /// Dependentid 
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Dependentid { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; }
    }





     


}
