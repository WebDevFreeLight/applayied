namespace ZR.Model.System
{
    /// <summary>
    /// User position
    /// </summary>
    [SugarTable("sys_user_post", "User and position association table")]
    [Tenant("0")]
    public class SysUserPost : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, ExtendedAttribute = ProteryConstant.NOTNULL)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        [SugarColumn(IsPrimaryKey = true, ExtendedAttribute = ProteryConstant.NOTNULL)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PostId { get; set; }
    }
}

