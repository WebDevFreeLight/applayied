namespace ZR.Model.System
{
    /// <summary>
    /// User table
    /// </summary>
    [SugarTable("sys_user", "User table")]
    [Tenant("0")]
    public class SysUser : SysBase
    {
        /// <summary>
        /// User ID
        /// </summary>
        [SugarColumn(IsIdentity = false, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        /// <summary>
        /// Login username
        /// </summary>
        [SugarColumn(  ColumnDescription = "User account", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string UserName { get; set; }
        /// <summary>
        /// User nickname
        /// </summary>
        [SugarColumn(  ColumnDescription = "User nickname", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string NickName { get; set; }
        /// <summary>
        /// User type (00 system user)
        /// </summary>
        [SugarColumn(Length = 2, ColumnDescription = "User type (00 system user)", DefaultValue = "00")]
        public string UserType { get; set; } = "00";
        //[SugarColumn(IsOnlyIgnoreInsert = true)]
        public string Avatar { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// Email verification status (0 unverified, 1 verified)
        /// </summary>
        [SugarColumn(DefaultValue = "0", ColumnDescription = "Email verification status")]
        public int EmailVerified { get; set; } = 1;

        /// <summary>
        /// Email verification token
        /// </summary>
        [SugarColumn(  ColumnDescription = "Email verification token")]
        public string EmailVerificationToken { get; set; }

        /// <summary>
        /// Email verification token expiry time
        /// </summary>
        [SugarColumn(ColumnDescription = "Email verification token expiry time")]
        public DateTime? EmailVerificationExpiry { get; set; }

        /// <summary>
        /// Password reset token
        /// </summary>
        [SugarColumn(ColumnName = "password_reset_token", ColumnDescription = "Password reset token")]
        public string PasswordResetToken { get; set; }

        /// <summary>
        /// Password reset token expiry time
        /// </summary>
        [SugarColumn(ColumnName = "password_reset_expiry", ColumnDescription = "Password reset token expiry time")]
        public DateTime? PasswordResetExpiry { get; set; }

        /// <summary>
        /// Invitation token (for employee invitation flow)
        /// </summary>
        [SugarColumn(ColumnName = "invitation_token", ColumnDescription = "Invitation token")]
        public string InvitationToken { get; set; }

        /// <summary>
        /// Invitation token expiry time
        /// </summary>
        [SugarColumn(ColumnName = "invitation_expiry", ColumnDescription = "Invitation token expiry time")]
        public DateTime? InvitationExpiry { get; set; }

        /// <summary>
        /// Account activation status (0=pending invitation, 1=activated)
        /// </summary>
        [SugarColumn(ColumnName = "is_activated", DefaultValue = "1", ColumnDescription = "Account activated: 0=pending, 1=activated")]
        public int IsActivated { get; set; } = 1;




        [JsonIgnore]
        [ExcelIgnore]
        [SugarColumn(  ColumnDescription = "Password", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string Password { get; set; }
        /// <summary>
        /// Phone number
        /// </summary>
        public string Phonenumber { get; set; }
        /// <summary>
        /// User gender (0 male, 1 female, 2 unknown)
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// Account status (0 normal, 1 disabled)
        /// </summary>
        [ExcelIgnore]
        [SugarColumn(DefaultValue = "0")]
        public int Status { get; set; }

        /// <summary>
        /// Delete flag (0 exists, 2 deleted)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int DelFlag { get; set; }

        /// <summary>
        /// Last login IP
        /// </summary>
        [SugarColumn(IsOnlyIgnoreInsert = true)]
        public string LoginIP { get; set; }

        /// <summary>
        /// Last login time
        /// </summary>
        [SugarColumn(IsOnlyIgnoreInsert = true)]
        [ExcelColumn(Name = "Login Date", Format = "yyyy-MM-dd HH:mm:ss")]
        public DateTime? LoginDate { get; set; }

        /// <summary>
        /// Department ID
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }
        public string Province { get; set; }
        public string City { get; set; }

        [SugarColumn(IsIgnore = true)]
        public bool IsAdmin
        {
            get
            {
                return UserId == 1;
            }
        }
        /// <summary>
        /// Role ID collection
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public long[] RoleIds { get; set; }
        /// <summary>
        /// Position collection
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int[] PostIds { get; set; }
        [SugarColumn(IsIgnore = true)]

        public bool IsUseTran { get; set; } = true;
    }
}

