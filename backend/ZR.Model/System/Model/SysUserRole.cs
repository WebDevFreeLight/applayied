namespace ZR.Model.System
{
    /// <summary>
    /// User-role association table User N-1 Role
    /// </summary>
    [SugarTable("sys_user_role", "User and role association table")]
    [Tenant("0")]
    public class SysUserRole : SysBase
    {
        [SugarColumn(ColumnName = "user_id", IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }

        [SugarColumn(ColumnName = "role_id", IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }
    }
}

