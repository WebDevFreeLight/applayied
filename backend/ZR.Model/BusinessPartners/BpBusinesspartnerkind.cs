
using ZR.Model.System;

namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerkind
    /// </summary>
    [SugarTable("bp_businesspartnerkind")]
    
    [SqlDict(
        dictType: "bp_businesspartnerkind",
        dictName: "Bp Businesspartnerkind",
        valueColumn: "kindid",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "kindid ASC",
        Description = "Auto-generated from BpBusinesspartnerkind table"
    )]
    public class BpBusinesspartnerkind:SysBase
    {
        /// <summary>
        /// kind ID 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Kindid { get; set; }

        /// <summary>
        /// kind name 
        /// </summary>
        public string Name { get; set; }
    }
}
