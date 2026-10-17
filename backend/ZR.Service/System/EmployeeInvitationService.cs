using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Model;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ZR.Common;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Model.HR.EmployeeCore;
using ZR.ServiceCore.Services;

namespace ZR.Service.Auth
{
    /// <summary>
    /// Employee invitation service implementation
    /// </summary>
    [AppService(ServiceType = typeof(IEmployeeInvitationService), ServiceLifetime = LifeTime.Transient)]
    public class EmployeeInvitationService : BaseService<SysUser>, IEmployeeInvitationService
    {
        private readonly ISysUserService _userService;
        private readonly ISysUserRoleService _userRoleService;
        private readonly ISysPostRoleService _postRoleService;

        // Token expiration times
        private const int INVITATION_EXPIRY_HOURS = 48;

        public EmployeeInvitationService(
            ISysUserService userService,
            ISysUserRoleService userRoleService,
            ISysPostRoleService postRoleService)
        {
            _userService = userService;
            _userRoleService = userRoleService;
            _postRoleService = postRoleService;
        }

        /// <summary>
        /// Invite an employee to create a system account
        /// </summary>
        public async Task<InvitationResultDto> InviteEmployeeAsync(InviteEmployeeDto dto)
        {
            // 1. Check if employee exists
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.EmployeeId == dto.EmployeeId)
                .FirstAsync();

            if (employee == null)
            {
                throw new CustomException("Employee not found");
            }

            // 2. Check if employee already has a linked user account
            if (employee.UserId.HasValue)
            {
                var existingUser = await Context.Queryable<SysUser>()
                    .Where(u => u.UserId == employee.UserId.Value && u.DelFlag == 0)
                    .FirstAsync();

                if (existingUser != null)
                {
                    if (existingUser.IsActivated == 1)
                    {
                        throw new CustomException("This employee already has an active account");
                    }

                    // Resend invitation for existing pending account
                    return await ResendInvitationForUserAsync(existingUser, dto.SendEmail);
                }
            }

            // 3. Check if email is already used by another user
            var emailUser = await Context.Queryable<SysUser>()
                .Where(u => u.Email == dto.Email && u.DelFlag == 0)
                .FirstAsync();

            if (emailUser != null)
            {
                throw new CustomException("This email is already associated with another account");
            }

            // 4. Get employee name
            string employeeName = await GetEmployeeNameAsync(dto.EmployeeId);

            // 5. Generate invitation token
            string token = GenerateSecureToken();

            // 6. Generate username from employee code or email
            string username = employee.EmployeeCode ?? dto.Email.Split('@')[0];

            // Check if username exists and make it unique
            int counter = 0;
            string originalUsername = username;
            while (await Context.Queryable<SysUser>().Where(u => u.UserName == username).AnyAsync())
            {
                counter++;
                username = $"{originalUsername}{counter}";
            }

            // 7. Create new user account (disabled, pending activation)
            var newUser = new SysUser
            {
                UserId = SnowFlakeSingle.Instance.NextId(),
                UserName = username,
                NickName = employeeName,
                Email = dto.Email,
                Password = "PENDING_ACTIVATION", // Will be set during activation
                Status = 1, // Disabled until activated
                IsActivated = 0, // Pending activation
                InvitationToken = token,
                InvitationExpiry = DateTime.Now.AddHours(INVITATION_EXPIRY_HOURS),
                EmailVerified = 0,
                Create_by = App.HttpContext?.GetName() ?? "system",
                Create_time = DateTime.Now
            };

            // 8. Insert user
            await Context.Insertable(newUser).ExecuteCommandAsync();

            // 9. Get roles - either from provided roleIds or from employee's position
            long[] roleIdsToAssign = dto.RoleIds;

            // If no roles provided, get roles from employee's position
            if (roleIdsToAssign == null || roleIdsToAssign.Length == 0)
            {
                roleIdsToAssign = await GetRolesFromEmployeePositionAsync(dto.EmployeeId);
            }

            // Assign roles
            if (roleIdsToAssign != null && roleIdsToAssign.Length > 0)
            {
                var userRoles = new List<SysUserRole>();
                foreach (var roleId in roleIdsToAssign)
                {
                    userRoles.Add(new SysUserRole
                    {
                        UserId = newUser.UserId,
                        RoleId = roleId
                    });
                }
                await Context.Insertable(userRoles).ExecuteCommandAsync();
            }

            // 10. Link employee to user
            await Context.Updateable<HrEmployeeCore>()
                .SetColumns(e => e.UserId == newUser.UserId)
                .Where(e => e.EmployeeId == dto.EmployeeId)
                .ExecuteCommandAsync();

            // 11. Send invitation email
            bool emailSent = false;
            if (dto.SendEmail)
            {
                emailSent = await SendInvitationEmailAsync(dto.Email, employeeName, token);
            }

            return new InvitationResultDto
            {
                UserId = newUser.UserId,
                Email = dto.Email,
                ExpiresAt = newUser.InvitationExpiry.Value,
                EmailSent = emailSent
            };
        }

        /// <summary>
        /// Resend invitation with new token for existing pending user
        /// </summary>
        private async Task<InvitationResultDto> ResendInvitationForUserAsync(SysUser user, bool sendEmail)
        {
            string token = GenerateSecureToken();
            string updateBy = App.HttpContext?.GetName() ?? "system";
            DateTime expiryTime = DateTime.Now.AddHours(INVITATION_EXPIRY_HOURS);
            DateTime updateTime = DateTime.Now;

            await Context.Updateable<SysUser>()
                .SetColumns(u => new SysUser
                {
                    InvitationToken = token,
                    InvitationExpiry = expiryTime,
                    Update_by = updateBy,
                    Update_time = updateTime
                })
                .Where(u => u.UserId == user.UserId)
                .ExecuteCommandAsync();

            bool emailSent = false;
            if (sendEmail)
            {
                emailSent = await SendInvitationEmailAsync(user.Email, user.NickName, token);
            }

            return new InvitationResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                ExpiresAt = DateTime.Now.AddHours(INVITATION_EXPIRY_HOURS),
                EmailSent = emailSent
            };
        }

        /// <summary>
        /// Resend invitation email with new token
        /// </summary>
        public async Task<InvitationResultDto> ResendInvitationAsync(long employeeId)
        {
            // Get employee with user
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.EmployeeId == employeeId)
                .FirstAsync();

            if (employee == null)
            {
                throw new CustomException("Employee not found");
            }

            if (!employee.UserId.HasValue)
            {
                throw new CustomException("Employee has not been invited yet");
            }

            var user = await Context.Queryable<SysUser>()
                .Where(u => u.UserId == employee.UserId.Value && u.DelFlag == 0)
                .FirstAsync();

            if (user == null)
            {
                throw new CustomException("User account not found");
            }

            if (user.IsActivated == 1)
            {
                throw new CustomException("Account is already activated");
            }

            return await ResendInvitationForUserAsync(user, true);
        }

        /// <summary>
        /// Validate invitation token
        /// </summary>
        public async Task<InvitationValidationResultDto> ValidateInvitationTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new InvitationValidationResultDto
                {
                    Valid = false,
                    ErrorMessage = "Token is required"
                };
            }

            // Find user with this token
            var user = await Context.Queryable<SysUser>()
                .Where(u => u.InvitationToken == token && u.DelFlag == 0)
                .FirstAsync();

            if (user == null)
            {
                return new InvitationValidationResultDto
                {
                    Valid = false,
                    ErrorMessage = "Invalid invitation token"
                };
            }

            // Check if expired
            if (user.InvitationExpiry.HasValue && user.InvitationExpiry.Value < DateTime.Now)
            {
                return new InvitationValidationResultDto
                {
                    Valid = false,
                    ErrorMessage = "Invitation token has expired"
                };
            }

            // Check if already activated
            if (user.IsActivated == 1)
            {
                return new InvitationValidationResultDto
                {
                    Valid = false,
                    ErrorMessage = "Account is already activated"
                };
            }

            // Get employee info
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.UserId == user.UserId)
                .FirstAsync();

            string employeeCode = employee?.EmployeeCode ?? "";

            return new InvitationValidationResultDto
            {
                Valid = true,
                EmployeeName = user.NickName,
                Email = user.Email,
                EmployeeCode = employeeCode
            };
        }

        /// <summary>
        /// Activate account with password
        /// </summary>
        public async Task<ActivationResultDto> ActivateAccountAsync(ActivateAccountDto dto)
        {
            // Validate token first
            var validation = await ValidateInvitationTokenAsync(dto.Token);
            if (!validation.Valid)
            {
                return new ActivationResultDto
                {
                    Success = false,
                    Message = validation.ErrorMessage
                };
            }

            // Get user
            var user = await Context.Queryable<SysUser>()
                .Where(u => u.InvitationToken == dto.Token && u.DelFlag == 0)
                .FirstAsync();

            // Hash password
            string hashedPassword = HashPassword(dto.Password);

            // Update user: set password, activate, clear token
            await Context.Updateable<SysUser>()
                .SetColumns(u => new SysUser
                {
                    Password = hashedPassword,
                    Status = 0, // Enable account
                    IsActivated = 1, // Mark as activated
                    EmailVerified = 1, // Email is verified through this process
                    InvitationToken = null,
                    InvitationExpiry = null,
                    Update_by = "system",
                    Update_time = DateTime.Now
                })
                .Where(u => u.UserId == user.UserId)
                .ExecuteCommandAsync();

            return new ActivationResultDto
            {
                Success = true,
                Message = "Account activated successfully",
                RedirectUrl = "/auth/sign-in"
            };
        }

        /// <summary>
        /// Get employee email from hr_person_contact
        /// </summary>
        public async Task<string> GetEmployeeEmailAsync(long employeeId)
        {
            // Get personId from employee
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.EmployeeId == employeeId)
                .FirstAsync();

            if (employee == null)
            {
                return null;
            }

            // Get primary email contact
            // Assuming contact type code for email is a specific value (need to check dictionary)
            var contact = await Context.Queryable<HrPersonContact>()
                .Where(c => c.PersonId == employee.PersonId && c.IsPrimary == true)
                .Where(c => c.EffectiveTo == null || c.EffectiveTo > DateTime.Now)
                .FirstAsync();

            return contact?.ContactValue;
        }

        /// <summary>
        /// Get employee name from hr_person_name
        /// </summary>
        public async Task<string> GetEmployeeNameAsync(long employeeId)
        {
            // Get personId from employee
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.EmployeeId == employeeId)
                .FirstAsync();

            if (employee == null)
            {
                return "Employee";
            }

            // Get primary name
            var name = await Context.Queryable<HrPersonName>()
                .Where(n => n.PersonId == employee.PersonId && n.IsPrimary == true)
                .Where(n => n.EffectiveTo == null || n.EffectiveTo > DateTime.Now)
                .FirstAsync();

            if (name == null)
            {
                return "Employee";
            }

            // Prefer Arabic full name, then construct from parts
            if (!string.IsNullOrEmpty(name.FullNameAr))
            {
                return name.FullNameAr;
            }

            var parts = new List<string>();
            if (!string.IsNullOrEmpty(name.GivenName)) parts.Add(name.GivenName);
            if (!string.IsNullOrEmpty(name.MiddleName)) parts.Add(name.MiddleName);
            if (!string.IsNullOrEmpty(name.FamilyName)) parts.Add(name.FamilyName);

            return parts.Count > 0 ? string.Join(" ", parts) : "Employee";
        }

        /// <summary>
        /// Get roles from employee's current position assignment
        /// </summary>
        private async Task<long[]> GetRolesFromEmployeePositionAsync(long employeeId)
        {
            try
            {
                // Get the employee's current primary assignment
                var assignment = await Context.Queryable<HrEmployeeAssignment>()
                    .Where(a => a.EmployeeId == employeeId)
                    .Where(a => a.IsPrimary == true)
                    .Where(a => a.EffectiveTo == null || a.EffectiveTo > DateTime.Now)
                    .OrderByDescending(a => a.EffectiveFrom)
                    .FirstAsync();

                if (assignment == null || !assignment.PositionId.HasValue)
                {
                    // No assignment or no position, return empty array
                    return Array.Empty<long>();
                }

                // Get roles associated with this position using the PostRoleService
                var roleIds = _postRoleService.GetRoleIdsByPostId(assignment.PositionId.Value);

                return roleIds ?? Array.Empty<long>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting roles from employee position: {ex.Message}");
                return Array.Empty<long>();
            }
        }

        /// <summary>
        /// Send invitation email (Bilingual: Arabic & English)
        /// </summary>
        private async Task<bool> SendInvitationEmailAsync(string email, string employeeName, string token)
        {
            try
            {
                // Get frontend URL from configuration or use default
                string baseUrl = AppSettings.Get<string>("FrontendUrl") ?? "http://localhost:3006";
                string activationUrl = $"{baseUrl}/auth/activate?token={token}";
                string logoUrl = $"{baseUrl}/logo.png";
                string currentYear = DateTime.Now.Year.ToString();

                string subject = "Welcome to One May | مرحباً بك في شركة واحد مايو - Account Activation | تفعيل الحساب";

                string htmlContent = $@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta name=""x-apple-disable-message-reformatting"" />
    <title>Account Activation - One May</title>
    <!--[if mso]>
    <noscript>
        <xml>
            <o:OfficeDocumentSettings>
                <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
        </xml>
    </noscript>
    <![endif]-->
    <style type=""text/css"">
        body {{ margin: 0; padding: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }}
        table, td {{ border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt; }}
        img {{ border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }}
        .button-td, .button-a {{ transition: all 100ms ease-in; }}
        .button-a:hover {{ background-color: #1a365d !important; }}
        @media screen and (max-width: 600px) {{
            .email-container {{ width: 100% !important; margin: auto !important; }}
            .stack-column {{ display: block !important; width: 100% !important; }}
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f0f4f8; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">

    <!-- Preview Text -->
    <div style=""display: none; font-size: 1px; color: #f0f4f8; line-height: 1px; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;"">
        Your One May account is ready to be activated | حسابك في شركة واحد مايو جاهز للتفعيل
    </div>

    <!-- Email Wrapper -->
    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #f0f4f8;"">
        <tr>
            <td style=""padding: 40px 10px;"">

                <!-- Email Container -->
                <table class=""email-container"" role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""600"" align=""center"" style=""margin: auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.12);"">

                    <!-- HEADER -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #0f2744 0%, #1a365d 50%, #2c5282 100%); padding: 0;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td style=""padding: 40px 40px 30px 40px; text-align: center;"">
                                        <img src=""{logoUrl}"" alt=""One May"" width=""150"" style=""display: block; margin: 0 auto 20px auto; max-width: 150px;"" />
                                        <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: 0.5px;"">One May</h1>
                                        <p style=""margin: 8px 0 0 0; color: rgba(255,255,255,0.85); font-size: 16px; font-weight: 500;"">شركة واحد مايو لتنظيم المعارض والمؤتمرات</p>
                                    </td>
                                </tr>
                                <!-- Decorative Wave -->
                                <tr>
                                    <td style=""height: 30px; background: #ffffff; border-radius: 30px 30px 0 0;""></td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- WELCOME ICON -->
                    <tr>
                        <td style=""padding: 0 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"" style=""padding: 0 0 20px 0;"">
                                        <div style=""width: 80px; height: 80px; background: linear-gradient(135deg, #e6f2ff 0%, #cce5ff 100%); border-radius: 50%; display: inline-block; line-height: 80px; text-align: center; margin-top: -40px; border: 4px solid #ffffff; box-shadow: 0 4px 20px rgba(44, 82, 130, 0.25);"">
                                            <span style=""font-size: 36px;"">🎉</span>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ENGLISH CONTENT -->
                    <tr>
                        <td style=""padding: 10px 40px 30px 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td style=""text-align: center;"">
                                        <h2 style=""margin: 0 0 15px 0; color: #1a365d; font-size: 24px; font-weight: 600;"">Welcome, {employeeName}!</h2>
                                        <p style=""margin: 0 0 20px 0; color: #4a5568; font-size: 16px; line-height: 1.7;"">
                                            Your account has been created in the <strong style=""color: #2c5282;"">One May System</strong>.<br/>
                                            Click the button below to activate your account and set your password.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ACTIVATION BUTTON -->
                    <tr>
                        <td style=""padding: 0 40px 35px 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"">
                                        <!-- Button : BEGIN -->
                                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td class=""button-td"" style=""border-radius: 10px; background: linear-gradient(135deg, #2c5282 0%, #1a365d 100%); box-shadow: 0 6px 20px rgba(26, 54, 93, 0.35);"">
                                                    <a class=""button-a"" href=""{activationUrl}"" style=""background: linear-gradient(135deg, #2c5282 0%, #1a365d 100%); border: none; border-radius: 10px; color: #ffffff; display: inline-block; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 18px; font-weight: 600; line-height: 1; padding: 18px 45px; text-decoration: none;"">
                                                        ✨ Activate Account | تفعيل الحساب
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                        <!-- Button : END -->
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- EXPIRY NOTICE -->
                    <tr>
                        <td style=""padding: 0 40px 30px 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #fff8e6; border-radius: 12px; border: 1px solid #ffd966;"">
                                <tr>
                                    <td style=""padding: 18px 20px;"">
                                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                            <tr>
                                                <td width=""40"" valign=""top"">
                                                    <span style=""font-size: 24px;"">⏰</span>
                                                </td>
                                                <td style=""padding-left: 10px;"">
                                                    <p style=""margin: 0; color: #856404; font-size: 14px; line-height: 1.5;"">
                                                        <strong>This link expires in 48 hours</strong><br/>
                                                        <span dir=""rtl"" style=""display: block; margin-top: 5px;"">هذا الرابط صالح لمدة ٤٨ ساعة فقط</span>
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- DIVIDER -->
                    <tr>
                        <td style=""padding: 0 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td style=""border-top: 2px solid #e2e8f0;""></td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ARABIC CONTENT -->
                    <tr>
                        <td dir=""rtl"" style=""padding: 30px 40px; text-align: right;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td>
                                        <h2 style=""margin: 0 0 15px 0; color: #1a365d; font-size: 24px; font-weight: 600; font-family: 'Segoe UI', Tahoma, Arial, sans-serif;"">مرحباً بك، {employeeName}!</h2>
                                        <p style=""margin: 0; color: #4a5568; font-size: 16px; line-height: 1.9; font-family: 'Segoe UI', Tahoma, Arial, sans-serif;"">
                                            تم إنشاء حسابك في نظام <strong style=""color: #2c5282;"">شركة واحد مايو لتنظيم المعارض والمؤتمرات</strong>.<br/>
                                            انقر على الزر أعلاه لتفعيل حسابك وإنشاء كلمة المرور الخاصة بك.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- SECURITY NOTICE -->
                    <tr>
                        <td style=""padding: 0 40px 30px 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #fef2f2; border-radius: 12px; border: 1px solid #fecaca;"">
                                <tr>
                                    <td style=""padding: 18px 20px;"">
                                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                            <tr>
                                                <td width=""40"" valign=""top"">
                                                    <span style=""font-size: 24px;"">🔒</span>
                                                </td>
                                                <td style=""padding-left: 10px;"">
                                                    <p style=""margin: 0; color: #991b1b; font-size: 13px; line-height: 1.6;"">
                                                        <strong>Security Notice:</strong> If you did not request this account, please ignore this email and contact support.<br/>
                                                        <span dir=""rtl"" style=""display: block; margin-top: 8px;""><strong>تنبيه أمني:</strong> إذا لم تطلب هذا الحساب، يرجى تجاهل هذا البريد والتواصل مع الدعم الفني.</span>
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- FOOTER -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #0f2744 0%, #1a365d 100%); padding: 35px 40px; text-align: center;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td>
                                        <p style=""margin: 0 0 10px 0; color: #ffffff; font-size: 18px; font-weight: 600;"">
                                            One May
                                        </p>
                                        <p style=""margin: 0 0 15px 0; color: rgba(255,255,255,0.8); font-size: 15px;"">
                                            شركة واحد مايو لتنظيم المعارض والمؤتمرات
                                        </p>
                                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
                                            <tr>
                                                <td style=""padding: 0 10px;"">
                                                    <a href=""mailto:support@onemay.com"" style=""color: rgba(255,255,255,0.7); font-size: 13px; text-decoration: none;"">support@onemay.com</a>
                                                </td>
                                            </tr>
                                        </table>
                                        <p style=""margin: 20px 0 0 0; color: rgba(255,255,255,0.5); font-size: 12px;"">
                                            © {currentYear} One May. All rights reserved. | جميع الحقوق محفوظة
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";

                string plainText = $@"
══════════════════════════════════════════════════════════════
                         ONE MAY
              شركة واحد مايو لتنظيم المعارض والمؤتمرات
══════════════════════════════════════════════════════════════

Hello {employeeName},

Your account has been created in the One May System.
Please visit the following link to activate your account:

🔗 {activationUrl}

⏰ This link is valid for 48 hours only.

If you did not request this account, please ignore this email.

──────────────────────────────────────────────────────────────

مرحباً {employeeName}،

تم إنشاء حسابك في نظام شركة واحد مايو لتنظيم المعارض والمؤتمرات.
لتفعيل حسابك، يرجى زيارة الرابط التالي:

🔗 {activationUrl}

⏰ هذا الرابط صالح لمدة ٤٨ ساعة فقط.

إذا لم تطلب هذا الحساب، يرجى تجاهل هذا البريد الإلكتروني.

══════════════════════════════════════════════════════════════
© {currentYear} One May | شركة واحد مايو لتنظيم المعارض والمؤتمرات
support@onemay.com
══════════════════════════════════════════════════════════════
";

                MailHelper mailHelper = new MailHelper();
                var result = mailHelper.SendMail(email, subject, plainText, "", htmlContent);

                return result != "fail";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send invitation email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate secure random token
        /// </summary>
        private string GenerateSecureToken(int byteLength = 32)
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(byteLength);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ""); // URL-safe base64
        }

        /// <summary>
        /// Hash password using the same method as the system
        /// </summary>
        private string HashPassword(string password)
        {
            return NETCore.Encrypt.EncryptProvider.Md5(password);
        }
    }
}
