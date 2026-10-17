 
using Infrastructure.Converter;
using ZR.Model.Content.Dto;

namespace ZR.Model.social
{
    /// <summary>
    /// Fans table
    /// </summary>
    public class SocialFansDto
    {
        [JsonIgnore]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Userid { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ToUserid { get; set; }
        [JsonIgnore]
        public DateTime FollowTime { get; set; }
        [JsonIgnore]
        public string UserIP { get; set; }
        public int Status { get; set; }

        public ArticleUser User { get; set; }
    }

    public class FansQueryDto : PagerInfo
    {
        public int SelectType { get; set; }
    }
}

