namespace ZR.Model.Dto
{
    public class ChatMessageDto
    {
        public ChatUserDto FromUser { get; set; }
        public bool Self { get; set; }
        [SugarColumn(IsPrimaryKey = true)]
        public string ChatId { get; set; }
        /// <summary>
        /// 0??? 1??? 2??? 3??? 4???
        /// </summary>
        public int MsgType { get; set; }
        public string StoredKey { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ToUserId { get; set; }

        /// <summary>
        /// ????,??type=1/2/3/4,????????URL??
        /// </summary>
        public string Message { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ChatTime { get; set; }
        public int Online { get; set; }
    }

    public class ChatUserDto
    {
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Avatar { get; set; }
    }
}

