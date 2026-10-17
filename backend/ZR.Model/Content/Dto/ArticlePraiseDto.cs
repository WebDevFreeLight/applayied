namespace ZR.Model.Content.Dto
{
    public class ArticlePraiseDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ArticleId { get; set; }
        public string UserIP { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ToUserId { get; set; }
        public int IsDelete { get; set; }
        public string Location { get; set; }
    }
}

