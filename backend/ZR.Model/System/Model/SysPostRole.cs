using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;

namespace ZR.Model.System
{
    /// <summary>
    /// Position-Role mapping table - Links positions to their default roles
    /// sys_post_role
    /// </summary>
    [SugarTable("sys_post_role", "Position and role association table")]
    [Tenant("0")]
    public class SysPostRole : SysBase
    {
        /// <summary>
        /// Position ID
        /// </summary>
        [SugarColumn(ColumnName = "post_id", IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PostId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        [SugarColumn(ColumnName = "role_id", IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }

        /// <summary>
        /// Is primary role for this position (0: No, 1: Yes)
        /// </summary>
        [SugarColumn(ColumnName = "is_primary", DefaultValue = "0")]
        public int IsPrimary { get; set; } = 0;
    }
}
