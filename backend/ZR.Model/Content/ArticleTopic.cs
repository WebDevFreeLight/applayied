
namespace ZR.Model.Content
{
    /// <summary>
    /// è¯é¢˜
    /// </summary>
    [SugarTable("article_topic", TableDescription = "æ–‡ç« è¯é¢˜")]
    [Tenant(0)]
    
    [SqlDict(
        dictType: "article_topic",
        dictName: "Article Topic",
        valueColumn: "topicid",
        labelColumns: "topicname",
        RemarkColumn = "topicdescription",
        OrderBy = "topicid ASC",
        Description = "Auto-generated from ArticleTopic table"
    )]
    public class ArticleTopic
    {
        /// <summary>
        /// è¯é¢˜ID 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TopicId { get; set; }

        /// <summary>
        /// è¯é¢˜å 
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// è¯é¢˜æè¿° 
        /// </summary>
        public string TopicDescription { get; set; }

        /// <summary>
        /// å‚ä¸Ž/å‘èµ·æ¬¡æ•° 
        /// </summary>
        public int JoinNum { get; set; }

        /// <summary>
        /// æµè§ˆæ¬¡æ•° 
        /// </summary>
        public int ViewNum { get; set; }

        /// <summary>
        /// åˆ›å»ºæ—¶é—´ 
        /// </summary>
        public DateTime? AddTime { get; set; }

        /// <summary>
        /// è¯é¢˜åˆ†ç±» 
        /// </summary>
        public int? TopicType { get; set; }

    }
}

