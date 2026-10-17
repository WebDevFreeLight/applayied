namespace ZR.Model.System
{
    /// <summary>
    /// sys_logininfor table
    /// </summary>
    [SugarTable("sys_logininfor", "Login log table")]
    [Tenant("0")]
    public class SysLogininfor : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InfoId { get; set; }

        /// <summary>
        /// User account
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Login status: 0 success, 1 failure
        /// </summary>
        [SugarColumn(Length = 1, DefaultValue = "0")]
        public string Status { get; set; }

        /// <summary>
        /// Login IP address
        /// </summary>
        public string Ipaddr { get; set; } = string.Empty;

        /// <summary>
        /// Login location
        /// </summary>
        public string LoginLocation { get; set; } = string.Empty;

        /// <summary>
        /// Browser type
        /// </summary>
        public string Browser { get; set; } = string.Empty;

        /// <summary>
        /// Operating system
        /// </summary>
        public string Os { get; set; } = string.Empty;

        /// <summary>
        /// Tip message
        /// </summary>
        public string Msg { get; set; } = string.Empty;

        /// <summary>
        /// Access time
        /// </summary>
        public DateTime LoginTime { get; set; } = DateTime.Now;
        [SugarColumn(IsIgnore = true)]
        public DateTime? BeginTime { get; set; }
        [SugarColumn(IsIgnore = true)]
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// Client ID
        /// </summary>
        public string ClientId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string Position
        {
            get
            {
                var temp_location = LoginLocation.Split("-")?[0];
                if (temp_location == "0")
                {
                    return "IP unknown";
                }
                return temp_location;
            }
        }
    }
}

