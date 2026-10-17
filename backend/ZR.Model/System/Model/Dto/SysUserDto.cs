using System.Linq;

namespace ZR.Model.System.Dto
{
    public class SysUserDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }
        public string Phonenumber { get; set; }
        public string Avatar { get; set; }
        /// <summary>
        /// User gender (0=Male, 1=Female, 2=Unknown)
        /// </summary>
        public int Sex { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Account status (0=Normal, 1=Disabled)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Account activation status (0=Pending, 1=Activated)
        /// </summary>
        public int IsActivated { get; set; } = 1;

        /// <summary>
        /// Invitation token expiry time
        /// </summary>
        public DateTime? InvitationExpiry { get; set; }

        /// <summary>
        /// Last login IP
        /// </summary>
        public string LoginIP { get; set; }

        /// <summary>
        /// Department ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }

        public string WelcomeContent { get; set; }

        #region Additional table fields

        /// <summary>
        /// Check if user is admin based on having RoleId = 1 (Admin Role)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsAdmin
        {
            get
            {
                // Check if user has the admin role (RoleId = 1)
                if (RoleIds != null && RoleIds.Length > 0)
                {
                    return RoleIds.Contains(1);
                }
                if (Roles != null && Roles.Count > 0)
                {
                    return Roles.Any(r => r.RoleId == 1);
                }
                return false;
            }
        }
        /// <summary>
        /// Number of roles owned
        /// </summary>
        //[SugarColumn(IsIgnore = true)]
        //public int RoleNum { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string DeptName { get; set; }
        /// <summary>
        /// Role ID collection
        /// </summary>
        [ExcelIgnore]
        public long[] RoleIds { get; set; }
        /// <summary>
        /// Position collection
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [ExcelIgnore]
        public long[] PostIds { get; set; }

        [SugarColumn(IsIgnore = true)]
        [ExcelIgnore]
        public List<SysRole> Roles { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string WelcomeMessage
        {
            get
            {
                int now = DateTime.Now.Hour;

                if (now > 0 && now <= 6)
                {
                    return "Good midnight";
                }
                else if (now > 6 && now <= 11)
                {
                    return "Good morning";
                }
                else if (now > 11 && now <= 14)
                {
                    return "Good noon";
                }
                else if (now > 14 && now <= 18)
                {
                    return "Good afternoon";
                }
                else
                {
                    return "Good evening";
                }
            }
        }

        public bool IsUseTran { get; set; }



        #endregion
    }

    public class SysUserQueryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }
        public string Phonenumber { get; set; }
        /// <summary>
        /// User gender (0=Male, 1=Female, 2=Unknown)
        /// </summary>
        public int Sex { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }
    }
}

