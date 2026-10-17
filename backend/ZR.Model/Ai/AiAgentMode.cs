
namespace ZR.Model.Ai
{
    /// <summary>
    /// Agent Mode Table
    /// </summary>
    [SugarTable("ai_agent_mode")]
    [SqlDict(
        dictType: "ai_agent_mode",
        dictName: "Ai Agent Mode",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from AiAgentMode table"
    )]
    public class AiAgentMode :SysBase
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
