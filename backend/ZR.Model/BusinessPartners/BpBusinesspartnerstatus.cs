
using ZR.Model.System;

namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerStatus
    /// </summary>
    [SugarTable("bp_businesspartnerstatus")]
    
    [SqlDict(
        dictType: "bp_businesspartnerstatus",
        dictName: "Bp Businesspartnerstatus",
        valueColumn: "statusid",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "statusid ASC",
        Description = "Auto-generated from BpBusinesspartnerstatus table"
    )]
    public class BpBusinesspartnerstatus:SysBase
    {
        /// <summary>
        /// Status ID 
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long   Statusid { get; set; }

        /// <summary>
        /// Status name 
        /// </summary>
        public string Name { get; set; } 

    }
}
