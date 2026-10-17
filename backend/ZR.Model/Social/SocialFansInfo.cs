namespace ZR.Model.social
{
    /// <summary>
    /// Fans information table
    /// </summary>
    [SugarTable("social_fans_info")]
    
    [SqlDict(
        dictType: "social_fans_info",
        dictName: "Social Fans Info",
        valueColumn: "userid",
        labelColumns: "userid",
        OrderBy = "userid ASC",
        Description = "Auto-generated from SocialFansInfo table"
    )]
    public class SocialFansInfo
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Userid { get; set; }
        /// <summary>
        /// Follow count
        /// </summary>
        public int FollowNum { get; set; }
        /// <summary>
        /// Fans count
        /// </summary>
        public int FansNum { get; set; }
        [JsonIgnore]
        public DateTime Update_time { get; set; }
    }
}

