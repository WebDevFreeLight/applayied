
using ZR.Model.System;

namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerType
    /// </summary>
    [SugarTable("bp_businesspartnertype")]
    
    [SqlDict(
        dictType: "bp_businesspartnertype",
        dictName: "Bp Businesspartnertype",
        valueColumn: "code",
        labelColumns: "code, ' - ', name",
        RemarkColumn = "remark",
        OrderBy = "code ASC",
        Description = "Auto-generated from BpBusinesspartnertype table"
    )]
    public class BpBusinesspartnertype:SysBase
    {
        /// <summary>
        /// Type ID 
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Typeid { get; set; }

        /// <summary>
        /// Review  Type name 
        /// </summary>
        public string Name { get; set; }
        public string code { get; set; }


    }
}
