namespace ZR.Model.System
{
    /// <summary>
    /// Role menu
    /// </summary>
    [SugarTable("sys_role_menu", "Role menu")]
    [Tenant("0")]
    public class SysRoleMenu : SysBase
    {
        [JsonProperty("roleId")]
        [SugarColumn(IsPrimaryKey = true, ExtendedAttribute = ProteryConstant.NOTNULL)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Role_id { get; set; }
        [JsonProperty("menuId")]
        [SugarColumn(IsPrimaryKey = true, ExtendedAttribute = ProteryConstant.NOTNULL)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Menu_id { get; set; }
    }
}

