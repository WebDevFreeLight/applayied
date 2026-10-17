namespace ZR.Model.Content.Dto
{
    /// <summary>
    /// ??
    /// </summary>
    public class MessageQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CommentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ParentId { get; set; }
        public int OrderBy { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }
        public int ClassifyId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TargetId { get; set; }
        public DateTime? BeginAddTime { get; set; }
        public DateTime? EndAddTime { get; set; }
    }

    public class ArticleCommentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CommentId { get; set; }
        /// <summary>
        /// ??id
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// ?????id
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ParentId { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public DateTime AddTime { get; set; }
        /// <summary>
        /// ????id
        /// </summary>
        public int ReplyUserId { get; set; }
        public string ReplyNickName { get; set; }
        /// <summary>
        /// ????id
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReplyId { get; set; }
        /// <summary>
        /// ??ip
        /// </summary>
        [JsonIgnore]
        public string UserIP { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        [JsonIgnore]
        public string Location { get; set; } = string.Empty;
        /// <summary>
        /// ????
        /// </summary>
        public int PraiseNum { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        public int ReplyNum { get; set; }
        ///// <summary>
        ///// ???? 0???? 1??? -1????
        ///// </summary>
        //public int AuditStatus { get; set; }
        /// <summary>
        /// ?? :???? 1??? 0???
        /// ?? : true  
        /// </summary>
        [JsonIgnore]
        public int IsDelete { get; set; } = 0;
        public string NickName { get; set; }

        ///// <summary>
        ///// ????
        ///// </summary>
        //public int ShareNunm { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string ChatImg { get; set; }
        /// <summary>
        /// ??id
        /// </summary>
        public int ClassifyId { get; set; }
        /// <summary>
        /// ??id
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TargetId { get; set; }
        /// <summary>
        /// ??
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Top { get; set; }
        public bool HasMore { get; set; } = false;
        public string Position
        {
            get
            {
                var temp_location = Location.Split("-")?[0];

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
        public List<ArticleCommentDto> ReplyList { get; set; }
    }
}

