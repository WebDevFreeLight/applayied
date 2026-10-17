namespace ZR.Model.System.Vo
{
    /// <summary>
    /// TreeSelect tree structure entity class
    /// </summary>
    public class TreeSelectVo
    {
        /// <summary>
        /// Node ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        /// <summary>
        /// Node name
        /// </summary>
        public string Label { get; set; }
        public string Status { get; set; }
        public string Visible { get; set; }
        public string MenuType { get; set; }
        public string Permission { get; set; }

        public TreeSelectVo() { }

        public TreeSelectVo(SysMenu menu)
        {
            Id = menu.MenuId;
            Label = menu.MenuName;
            Status = menu.Status;
            MenuType = menu.MenuType;
            Permission = menu.Perms;
            Visible = menu.Visible;

            List<TreeSelectVo> child = new List<TreeSelectVo>();
            foreach (var item in menu.Children)
            {
                child.Add(new TreeSelectVo(item));
            }

            Children = child;
        }

        public TreeSelectVo(SysDept dept)
        {
            Id = dept.DeptId;
            Label = dept.DeptName;

            //menu.getChildren().stream().map(TreeSelect::new).collect(Collectors.toList()); java??
            List<TreeSelectVo> child = new List<TreeSelectVo>();
            foreach (var item in dept.children)
            {
                child.Add(new TreeSelectVo(item));
            }

            Children = child;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TreeSelectVo> Children { get; set; }
    }
}

