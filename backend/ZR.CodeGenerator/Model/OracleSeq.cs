using SqlSugar;
using System.Text.Json.Serialization;

namespace ZR.CodeGenerator.Model
{
    /// <summary>
    /// Oracle database sequence
    /// </summary>
    public class OracleSeq
    {
        public string SEQUENCE_NAME { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LAST_NUMBER { get; set; }
    }
}

