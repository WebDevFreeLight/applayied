 
using Infrastructure.Converter;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.System.Dto
{
    public class RoleUserQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }

        public string UserName { get; set; }
    }

    public class RoleUsersCreateDto
    {
        /// <summary>
        /// ??id
        /// </summary>
        [Display(Name = "??id")]
        [Required(ErrorMessage = "Role ID cannot be empty")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RoleId { get; set; }

        /// <summary>
        /// ???? [1,2,3,4] 
        /// </summary>
        [Display(Name = "???? [1,2,3,4]")]
        public List<long> UserIds { get; set; }
    }
}

