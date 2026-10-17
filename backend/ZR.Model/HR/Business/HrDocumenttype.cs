
using ZR.Model.System;

namespace ZR.Model.HR.Business
{
    /// <summary>
    /// DocumentType
    /// </summary>
    [SugarTable("hr_documenttype")]
    
    [SqlDict(
        dictType: "documenttype",
        dictName: "Hr Documenttype",
        valueColumn: "Documenttypeid",
        labelColumns: "documenttypename",
        RemarkColumn = "remark",
        OrderBy = "Documenttypeid ASC",
        Description = "Auto-generated from HrDocumenttype table"
    )]
    public class HrDocumenttype :SysBase
    {
        /// <summary>
        /// Organization ID 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]


        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Documenttypeid { get; set; }

        /// <summary>
        /// DocumentType name 
        /// </summary>
        public string Documenttypename { get; set; }
          

    }
}
