
using ZR.Model.System;

namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// ContactMethod
    /// </summary>
    [SugarTable("bp_contactmethod")]
    
    [SqlDict(
        dictType: "bp_contactmethod",
        dictName: "Bp Contactmethod",
        valueColumn: "contactmethodenumid",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "contactmethodenumid ASC",
        Description = "Auto-generated from BpContactmethod table"
    )]
    public class BpContactmethod:SysBase
    {
        /// <summary>
        /// ContactMethod  ID 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Contactmethodenumid { get; set; }

        /// <summary>
        /// Type name 
        /// </summary>
        public string Name { get; set; }
         

    }
}
