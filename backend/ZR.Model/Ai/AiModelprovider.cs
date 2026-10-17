
namespace ZR.Model.Ai
{
    /// <summary>
    /// Model Provider Table
    /// </summary>
    [SugarTable("ai_modelprovider")]
    
    [SqlDict(
        dictType: "ai_modelprovider",
        dictName: "Ai Modelprovider",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from AiModelprovider table"
    )]
    public class AiModelprovider :SysBase
    {
        /// <summary>
        /// Id 
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
