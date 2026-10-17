 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;

namespace ZR.Model.System.Dto
{

    public class SysRoleMenuDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }
        /// <summary>
        /// ??????
        /// </summary>
        public List<long> MenuIds { get; set; } = new List<long>();
        public string RoleName { get; set; }
        public string RoleKey { get; set; }
        public string Create_by { get; set; }
        public DateTime Create_time{ get; set; }
    }
}

