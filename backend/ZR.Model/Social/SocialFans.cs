namespace ZR.Model.social
{
    /// <summary>
    /// Fans table
    /// </summary>
    [SugarTable("social_fans")]
    
    [SqlDict(
        dictType: "social_fan",
        dictName: "Social Fans",
        valueColumn: "pid",
        labelColumns: "remarkname",
        RemarkColumn = "remarkname",
        OrderBy = "pid ASC",
        Description = "Auto-generated from SocialFans table"
    )]
    public class SocialFans
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Userid { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ToUserid { get; set; }
        /// <summary>
        /// Remark name
        /// </summary>
        public string RemarkName { get; set; }
        public DateTime FollowTime { get; set; }
        public string UserIP { get; set; }
    }
}

