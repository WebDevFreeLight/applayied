namespace ZR.Model.Content.Dto
{
    public class UserDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Avatar { get; set; }
        /// <summary>
        /// ????(0? 1? 2??)
        /// </summary>
        public int Sex { get; set; }
    }
}

