
namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// Bp_PartnerDocsments
    /// </summary>
    [SugarTable("bp_partnerdocsments")]
    
    [SqlDict(
        dictType: "bp_partnerdocsment",
        dictName: "Bp Partnerdocsments",
        valueColumn: "id",
        labelColumns: "fileurl",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BpPartnerdocsments table"
    )]
    public class BpPartnerdocsments :SysBase
    {
        /// <summary>
        /// ID 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long? Id { get; set; }
       public long  Id { get; set; }

        /// <summary>
        /// document type 
        /// </summary>
       //  public long Doctype { get; set; }
       public long  Doctype { get; set; }

        /// <summary>
        /// Partner 
        /// </summary>
       //  public long Partner { get; set; }
       public long  Partner { get; set; }

        /// <summary>
        /// FileUrl 
        /// </summary>
        [SugarColumn(ColumnName = "file_url")]
       //  public string FileUrl { get; set; }
       public string  FileUrl { get; set; }
         

    }
}
