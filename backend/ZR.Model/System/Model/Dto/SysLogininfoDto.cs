namespace ZR.Model.System.Dto
{
    public class SysLogininfoQueryDto : PagerInfo
    {
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }
        public string Ipaddr { get; set; } = string.Empty; 
        public string UserName { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class SysLogininfoDto : SysBase
    {
        public int PageNum { get; set; }
        /// <summary>
        /// IP ??
        /// </summary>
        public string Ipaddr { get; set; }
        /// <summary>
        /// ???? 0?? 1??
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public string UserName { get; set; }

    }
}

