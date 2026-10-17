
namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerDocType
    /// </summary>
    [SugarTable("bp_businesspartnerdoctype")]
    
    [SqlDict(
        dictType: "bp_businesspartnerdoctype",
        dictName: "Bp Businesspartnerdoctype",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BpBusinesspartnerdoctype table"
    )]
    public class BpBusinesspartnerdoctype :SysBase
    {
        /// <summary>
        /// ID 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long? Id { get; set; }
       public long  Id { get; set; }

        /// <summary>
        /// name 
        /// </summary>
       //  public string Name { get; set; }
       public string  Name { get; set; }
    }










     
}
