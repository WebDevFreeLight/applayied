namespace ZR.Model.Content
{
    [SugarTable("article_browsing_log", "内容浏览记录")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "article_browsing_log",
        dictName: "Article Browsing Log",
        valueColumn: "logid",
        labelColumns: "location",
        OrderBy = "logid ASC",
        Description = "Auto-generated from ArticleBrowsingLog table"
    )]
    public class ArticleBrowsingLog
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LogId { get; set; }
        public string Location { get; set; }
        public string UserIP { get; set; }
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 文章ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ArticleId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
    }
}

