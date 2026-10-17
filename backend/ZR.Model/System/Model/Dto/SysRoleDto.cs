 
using Infrastructure.Converter;
using System.Collections.Generic;

namespace ZR.Model.System.Dto
{
    public class SysRoleDto : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }
        /// <summary>
        /// Menu collection to add
        /// </summary>
        public List<long> MenuIds { get; set; } = new List<long>();
        public string RoleName { get; set; }
        public string RoleKey { get; set; }
        public int RoleSort { get; set; }
        public int Status { get; set; }
        public int DataScope { get; set; }
        public int[] DeptIds { get; set; }
        /// <summary>
        /// Menu collection to remove
        /// </summary>
        public List<long> DelMenuIds { get; set; } = new List<long>();
        public bool MenuCheckStrictly { get; set; }
        public bool DeptCheckStrictly { get; set; }

    }
}

