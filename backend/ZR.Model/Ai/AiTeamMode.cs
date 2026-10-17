
namespace ZR.Model.AI
{
    /// <summary>
    /// Team Mode Table
    /// </summary>
    [SugarTable("ai_team_mode")]
    
    [SqlDict(
        dictType: "ai_team_mode",
        dictName: "Ai Team Mode",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from AiTeamMode table"
    )]
    public class AiTeamMode :SysBase
    {
        /// <summary>
        /// Id 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long Id { get; set; }
       public long  Id { get; set; }

        /// <summary>
        /// name 
        /// </summary>
       //  public string Name { get; set; }
       public string  Name { get; set; }
    }
}
