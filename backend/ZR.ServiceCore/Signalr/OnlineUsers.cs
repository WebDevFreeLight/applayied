namespace ZR.ServiceCore.Signalr
{
    public class OnlineUsers
    {
        /// <summary>
        /// Client connection ID
        /// </summary>
        public string ConnnectionId { get; set; }
        /// <summary>
        /// User ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Userid { get; set; }
        public string Name { get; set; }
        public DateTime LoginTime { get; set; }
        public string UserIP { get; set; }
        /// <summary>
        /// Login location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// User unique identifier
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// Browser
        /// </summary>
        public string Browser { get; set; }
        /// <summary>
        /// Platform
        /// </summary>
        public string Platform { get; set; } = string.Empty;
        /// <summary>
        /// Online duration
        /// </summary>
        public double OnlineTime
        {
            get
            {
                var ts = DateTime.Now - LoginTime;
                return Math.Round(ts.TotalMinutes, 2);
            }
        }
        /// <summary>
        /// Today's online duration
        /// </summary>
        public double TodayOnlineTime { get; set; }
        /// <summary>
        /// Number of online devices
        /// </summary>
        public int ClientNum { get; set; }
        /// <summary>
        /// Client ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public OnlineUsers()
        {
        }
        public OnlineUsers(string clientid, string name, long userid, string userip, string browser)
        {
            ConnnectionId = clientid;
            Name = name;
            LoginTime = DateTime.Now;
            Userid = userid;
            UserIP = userip;
            Browser = browser;
        }
    }
}

