namespace ZR.Model.System.Dto
{
    public class MenuDto
    {
        //{"parentId":0,"menuName":"aaa","icon":"documentation","menuType":"M","orderNum":999,"visible":0,"status":0,"path":"aaa"}
        [Required(ErrorMessage = "Menu ID cannot be empty")]
        [ExcelColumn(Name = "Menu ID")]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MenuId { get; set; }
        [ExcelColumn(Name = "Menu name")]
        public string MenuName { get; set; }
        public string RouteName { get; set; }
        /// <summary>
        /// Parent menu ID
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        
         
        public long? ParentId { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int OrderNum { get; set; }

        /// <summary>
        /// Route path
        /// </summary>
        [ExcelColumn(Name = "Route path")] 
        public string Path { get; set; } = "";

        /// <summary>
        /// Component path
        /// </summary>
        [ExcelColumn(Name = "Component address")]
        public string Component { get; set; }

        /// <summary>
        /// Whether to cache (1=Cache, 0=No cache)
        /// </summary>
        [Required(ErrorMessage = "Cache setting cannot be empty")]
        [ExcelColumn(Name = "Whether to cache")]
        public int IsCache { get; set; }
        /// <summary>
        /// Is external link 1=Yes, 0=No
        /// </summary>
        public int IsFrame { get; set; }

        /// <summary>
        /// Type (M=Directory, C=Menu, F=Button, L=Link)
        /// </summary>
        [Required(ErrorMessage = "Menu type cannot be empty")]
        public string MenuType { get; set; }

        /// <summary>
        /// Display status (0=Show, 1=Hide)
        /// </summary>
        [Required(ErrorMessage = "Visibility status cannot be empty")]
        public string Visible { get; set; }

        /// <summary>
        /// Menu status (0=Normal, 1=Disabled)
        /// </summary>
        [Required(ErrorMessage = "Menu status cannot be empty")]
        public string Status { get; set; }

        /// <summary>
        /// Permission string
        /// </summary>
        [ExcelColumn(Name = "Permission string")]
        public string Perms { get; set; }

        /// <summary>
        /// Menu icon
        /// </summary>
        public string Icon { get; set; } = string.Empty;
        /// <summary>
        /// Route parameters
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// Translation key
        /// </summary>
        public string MenuNameKey { get; set; }
        public List<MenuDto> Children { get; set; } = new List<MenuDto>();
    }

    public class MenuQueryDto
    {
        public string MenuName { get; set; }
        public string Visible { get; set; }
        public string Status { get; set; }
        public string MenuTypeIds { get; set; } = string.Empty;
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentId { get; set; }
        public string[] MenuTypeIdArr
        {
            get
            {
                return MenuTypeIds?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}

