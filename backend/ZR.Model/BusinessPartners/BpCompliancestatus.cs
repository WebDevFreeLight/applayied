
using ZR.Model.System;

namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// compliancestatus
    /// </summary>
    [SugarTable("bp_compliancestatus")]
    
    [SqlDict(
        dictType: "bp_compliancestatus",
        dictName: "Bp Compliancestatus",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BpCompliancestatus table"
    )]
    public class BpCompliancestatus:SysBase
    {
        /// <summary>
        /// ComplianceStatus  ID 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true )]
        public long  Id { get; set; }

        /// <summary>
        /// Type name 
        /// </summary>
        public string Name { get; set; }
 
    }
}
