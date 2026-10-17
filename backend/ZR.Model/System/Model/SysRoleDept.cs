namespace ZR.Model.System
{
    [SugarTable("sys_role_dept", "Role department")]
    [Tenant(0)]
    public class SysRoleDept : SysBase
    {
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }

        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL, IsPrimaryKey = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }
    }
}

