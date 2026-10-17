namespace ZR.Model.System
{
    /// <summary>
    /// Sys_menu table
    /// </summary>
    [SugarTable("sys_menu", "System menu table")]
    [Tenant("0")]
    public class SysMenu : SysBase
    {
        /// <summary>
        /// Menu ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true
            
            //, IsIdentity = true
            
            
            )]
        [JsonConverter(typeof(ValueToStringConverter))] 
        public long MenuId { get; set; }
        /// <summary>
        /// Menu name
        /// </summary>
        [SugarColumn(Length = 50, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string MenuName { get; set; }

        /// <summary>
        /// Parent menu ID
        /// </summary>
        [SugarColumn(DefaultValue = "0")]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ParentId { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int OrderNum { get; set; }

        /// <summary>
        /// Route path
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Component path
        /// </summary>
        public string Component { get; set; }

        /// <summary>
        /// Is cached (1 not cached, 0 cached)
        /// </summary>
        [SugarColumn(DefaultValue = "0" )]
        public string IsCache { get; set; } = "0";
        /// <summary>
        /// Is external link (1 yes, 0 no)
        /// </summary>
        [SugarColumn(DefaultValue = "0" )]
        public string IsFrame { get; set; } ="0";

        /// <summary>
        /// Type (M directory, C menu, F button, L link)
        /// </summary>
        [SugarColumn(Length = 1)]
        public string MenuType { get; set; } = string.Empty;

        /// <summary>
        /// Display status (0 visible, 1 hidden)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public string Visible { get; set; }

        /// <summary>
        /// Menu status (0 normal, 1 disabled)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public string Status { get; set; }

        /// <summary>
        /// Permission string
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Perms { get; set; }

        /// <summary>
        /// Menu icon
        /// </summary>
        [SugarColumn(DefaultValue = "#")]
        public string Icon { get; set; } = string.Empty;
        /// <summary>
        /// Menu name key
        /// </summary>
        [SugarColumn(ColumnName = "menuName_key")]
        public string MenuNameKey { get; set; }
        /// <summary>
        /// Route parameters
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// Submenu
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<SysMenu> Children { get; set; } = new List<SysMenu>();
        /// <summary>
        /// Submenu count
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int SubNum { get; set; }
        /// <summary>
        /// Whether contains child nodes, for frontend use
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool HasChildren
        {
            get
            {
                return SubNum > 0 || Children.Count > 0;
            }
        }

        /// <summary>
        /// Route name
        /// </summary>
        public string RouteName { get; set; }
    }
}

