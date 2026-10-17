namespace ZR.Model.System.Vo
{
    /// <summary>
    /// Route display
    /// </summary>
    public class RouterVo
    {
        /// <summary>
        /// When a route has more than 1 child route declared, it automatically becomes nested mode - like component pages
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool AlwaysShow { get; set; }
        /// <summary>
        /// Component path
        /// </summary>
        private string component;
        /// <summary>
        /// Whether to hide the route, when set to true, the route will not appear in the sidebar
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Hidden { get; set; }
        /// <summary>
        /// Route name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Route path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Redirect address, when set to noRedirect, the route cannot be clicked in breadcrumb navigation
        /// </summary>
        public string Redirect { get; set; }
        /// <summary>
        /// Route parameters: like { "key": "value"}
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// Other metadata
        /// </summary>
        public Meta Meta { get; set; }
        /// <summary>
        /// Child routes
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<RouterVo> Children { get; set; }
        public string Component { get => component; set => component = value; }
    }

    public class Meta
    {
        /// <summary>
        /// Set the name displayed in the sidebar and breadcrumb for this route
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Set the icon for this route, corresponding to path src/assets/icons/svg
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// When set to true, it will not be cached by <keep-alive>
        /// </summary>
        public bool NoCache { get; set; }
        public string TitleKey { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public int IsNew { get; set; }
        /// <summary>
        /// Icon color
        /// </summary>
        public string IconColor { get; set; }
        /// <summary>
        /// Permission string
        /// </summary>
        public string Permi { get; set; }
        public Meta(string title, string icon)
        {
            Title = title;
            Icon = icon;
        }
        public Meta(string title, string icon, string path)
        {
            Title = title;
            Icon = icon;
            Link = path;
        }
        public Meta(string title, string icon, bool noCache, string titleKey, string path, DateTime addTime)
        {
            Title = title;
            Icon = icon;
            NoCache = noCache;
            TitleKey = titleKey;
            if (!string.IsNullOrEmpty(path) && (path.StartsWith(UserConstants.HTTP) || path.StartsWith(UserConstants.HTTPS)))
            {
                Link = path;
            }
            if (addTime != DateTime.MinValue)
            {
                TimeSpan ts = DateTime.Now - addTime;
                if (ts.Days < 7)
                {
                    IsNew = 1;
                }
            }
        }
    }
}

