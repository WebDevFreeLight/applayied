
using ZR.Model.System;

namespace ZR.Model.Ai
{
    /// <summary>
    /// Tool Type Table
    /// </summary>
    [SugarTable("ai_tooltype")]
    
    [SqlDict(
        dictType: "ai_tooltype",
        dictName: "Ai Tooltype",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from AiTooltype table"
    )]
    public class AiTooltype :SysBase
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
