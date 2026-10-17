namespace ZR.Model.System
{
    /// <summary>
    /// Role table sys_role
    /// </summary>
    [SugarTable("sys_role", "Role table")]
    [Tenant("0")]
    public class SysRole : SysBase
    {
        /// <summary>
        /// Role ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        [SugarColumn(Length = 30, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string RoleName { get; set; }

        /// <summary>
        /// Role permission
        /// </summary>
        [SugarColumn(Length = 100, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string RoleKey { get; set; }

        /// <summary>
        /// Role sort
        /// </summary>
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int RoleSort { get; set; }

        /// <summary>
        /// Account status (0 normal, 1 disabled)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int Status { get; set; }

        /// <summary>
        /// Delete flag (0 exists, 2 deleted)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int DelFlag { get; set; }
        /// <summary>
        /// Data scope (1: All data permissions, 2: Custom data permissions, 3: Department data permissions, 4: Department and subordinate data permissions)
        /// </summary>
        [SugarColumn(DefaultValue = "1")]
        public int DataScope { get; set; }
        /// <summary>
        /// Whether menu tree selection items are associated
        /// </summary>
        [SugarColumn(ColumnName = "menu_check_strictly")]
        public bool MenuCheckStrictly { get; set; } = true;
        /// <summary>
        /// Whether department tree selection items are associated
        /// </summary>
        [SugarColumn(ColumnName = "dept_check_strictly")]
        public bool DeptCheckStrictly { get; set; } = true;
        /// <summary>
        /// Menu group
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public long[] MenuIds { get; set; }
        /// <summary>
        /// Department group (data permissions)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public long[] DeptIds { get; set; }
        /// <summary>
        /// User count
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int UserNum { get; set; }

        public SysRole() { }

        public SysRole(long roleId)
        {
            RoleId = roleId;
        }

        public bool IsAdmin()
        {
            return IsAdmin(RoleId);
        }

        public static bool IsAdmin(long roleId)
        {
            return 1 == roleId;
        }
    }
}

