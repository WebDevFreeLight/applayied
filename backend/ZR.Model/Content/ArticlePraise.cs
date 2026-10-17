namespace ZR.Model.Content
{
    [SugarTable("article_praise", "内容点赞记录")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "article_praise",
        dictName: "Article Praise",
        valueColumn: "pid",
        labelColumns: "userip",
        OrderBy = "pid ASC",
        Description = "Auto-generated from ArticlePraise table"
    )]
    public class ArticlePraise
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ArticleId { get; set; }
        public string UserIP { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ToUserId { get; set; }
        public int IsDelete { get; set; }
        [SugarColumn(InsertServerTime = true)]
        public DateTime AddTime { get; set; }
        public string Location { get; set; }
    }
}

