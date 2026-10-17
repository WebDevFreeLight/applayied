 
using Infrastructure.Converter;
using ZR.Model.Content.Enums;
using ZR.Model.System.Enums;

namespace ZR.Model.Content.Dto
{
    public class ArticleQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string AbstractText { get; set; }
        public int? IsPublic { get; set; }
        public int? IsTop { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? ArticleType { get; set; }
        /// <summary>
        /// 1??? 2??? 3???
        /// </summary>
        public int TabId { get; set; }
        /// <summary>
        /// ??ID
        /// </summary>
        public int? TopicId { get; set; }
        /// <summary>
        /// ?? 1??? 2???
        /// </summary>
        public int? OrderBy { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public AuditStatusEnum? AuditStatus { get; set; }
        /// <summary>
        /// ????????
        /// </summary>
        public bool QueryMyJoin { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string SearchText { get; set; }
    }

    /// <summary>
    /// ??????
    /// </summary>
    public class ArticleDto
    {
        [Required(ErrorMessage = "Cid????")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Cid { get; set; }
        //[Required(ErrorMessage = "??????")]
        public string Title { get; set; }
        [Required(ErrorMessage = "??????")]
        public string Content { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }

        public string Status { get; set; }

        public string EditorType { get; set; }

        public string Tags { get; set; }

        public int Hits { get; set; }

        public int? CategoryId { get; set; }

        public DateTime? Create_time { get; set; }

        public DateTime? Update_time { get; set; }

        public string AuthorName { get; set; }

        public string CoverUrl { get; set; }

        public ArticleCategoryDto CategoryNav { get; set; }
        public string[] TagList
        {
            get
            {
                return Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            }
        }
        public int IsPublic { get; set; } = 1;
        public string AbstractText { get; set; }
        public int IsTop { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public ArticleTypeEnum ArticleType { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public int PraiseNum { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public int CommentNum { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public int ShareNum { get; set; }
        /// <summary>
        /// ??IP
        /// </summary>
        [JsonIgnore]
        public string UserIP { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        [JsonIgnore]
        public string Location { get; set; }
        /// <summary>
        /// ??????
        /// </summary>
        public string[] CoverImgList
        {
            get
            {
                return CoverUrl?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            }
        }
        /// <summary>
        /// ????
        /// </summary>
        public string Position
        {
            get
            {
                var temp_location = Location?.Split("-")?[0];

                if (temp_location == "0")
                {
                    return "IP??";
                }
                return temp_location?.Replace("?", "");
            }
        }
        /// <summary>
        /// ????
        /// </summary>
        public int IsPraise { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TopicId { get; set; }
        public string TopicName { get; set; }
        public ArticleUser User { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public AuditStatusEnum? AuditStatus { get; set; }
        public CommentSwitchEnum? CommentSwitch { get; set; }
    }
    public class ArticleUser
    {
        public string Avatar { get; set; } = string.Empty;
        public string NickName { get; set; }
        public int Sex { get; set; }
    }
}

