using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Model;
using SqlSugar;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ZR.Common;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.ServiceCore.Services;

namespace ZR.Service.Auth
{
    /// <summary>
    /// Password reset service implementation
    /// </summary>
    [AppService(ServiceType = typeof(IPasswordResetService), ServiceLifetime = LifeTime.Transient)]
    public class PasswordResetService : BaseService<SysUser>, IPasswordResetService
    {
        // Token expiration time
        private const int RESET_TOKEN_EXPIRY_HOURS = 1;

        public PasswordResetService()
        {
        }

        /// <summary>
        /// Request password reset email
        /// </summary>
        public async Task<PasswordResetResultDto> RequestPasswordResetAsync(string email)
        {
            // Always return success message to prevent email enumeration attacks
            var successResult = new PasswordResetResultDto
            {
                Success = true,
                Message = "If an account exists with this email, a password reset link has been sent."
            };

            if (string.IsNullOrEmpty(email))
            {
                return successResult;
            }

            // Find user by email or username
            var user = await Context.Queryable<SysUser>()
                .Where(u => (u.Email == email || u.UserName == email) && u.DelFlag == 0)
                .FirstAsync();

            if (user == null)
            {
                // Don't reveal that user doesn't exist
                return successResult;
            }

            // Check if account is activated
            if (user.IsActivated == 0)
            {
                // Account not activated - don't send reset email
                return successResult;
            }

            // Generate reset token
            string token = GenerateSecureToken();

            // Update user with reset token
            await Context.Updateable<SysUser>()
                .SetColumns(u => new SysUser
                {
                    PasswordResetToken = token,
                    PasswordResetExpiry = DateTime.Now.AddHours(RESET_TOKEN_EXPIRY_HOURS),
                    Update_by = "system",
                    Update_time = DateTime.Now
                })
                .Where(u => u.UserId == user.UserId)
                .ExecuteCommandAsync();

            // Send reset email
            await SendPasswordResetEmailAsync(user.Email, user.NickName ?? user.UserName, token);

            return successResult;
        }

        /// <summary>
        /// Validate password reset token
        /// </summary>
        public async Task<ResetTokenValidationResultDto> ValidateResetTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new ResetTokenValidationResultDto
                {
                    Valid = false,
                    ErrorMessage = "Token is required"
                };
            }

            // Find user with this token
            var user = await Context.Queryable<SysUser>()
                .Where(u => u.PasswordResetToken == token && u.DelFlag == 0)
                .FirstAsync();

            if (user == null)
            {
                return new ResetTokenValidationResultDto
                {
                    Valid = false,
                    ErrorMessage = "Invalid password reset token"
                };
            }

            // Check if expired
            if (user.PasswordResetExpiry.HasValue && user.PasswordResetExpiry.Value < DateTime.Now)
            {
                return new ResetTokenValidationResultDto
                {
                    Valid = false,
                    ErrorMessage = "Password reset token has expired"
                };
            }

            return new ResetTokenValidationResultDto
            {
                Valid = true,
                Email = MaskEmail(user.Email)
            };
        }

        /// <summary>
        /// Reset password with valid token
        /// </summary>
        public async Task<PasswordResetResultDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            // Validate token first
            var validation = await ValidateResetTokenAsync(dto.Token);
            if (!validation.Valid)
            {
                return new PasswordResetResultDto
                {
                    Success = false,
                    Message = validation.ErrorMessage
                };
            }

            // Get user
            var user = await Context.Queryable<SysUser>()
                .Where(u => u.PasswordResetToken == dto.Token && u.DelFlag == 0)
                .FirstAsync();

            // Validate password strength
            var passwordValidation = ValidatePassword(dto.Password);
            if (!passwordValidation.IsValid)
            {
                return new PasswordResetResultDto
                {
                    Success = false,
                    Message = passwordValidation.Message
                };
            }

            // Hash password
            string hashedPassword = HashPassword(dto.Password);

            // Update user: set password, clear reset token
            await Context.Updateable<SysUser>()
                .SetColumns(u => new SysUser
                {
                    Password = hashedPassword,
                    PasswordResetToken = null,
                    PasswordResetExpiry = null,
                    Update_by = "system",
                    Update_time = DateTime.Now
                })
                .Where(u => u.UserId == user.UserId)
                .ExecuteCommandAsync();

            return new PasswordResetResultDto
            {
                Success = true,
                Message = "Password has been reset successfully"
            };
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        private async Task<bool> SendPasswordResetEmailAsync(string email, string userName, string token)
        {
            try
            {
                // Get frontend URL from configuration or use default
                string baseUrl = AppSettings.Get<string>("FrontendUrl") ?? "http://localhost:3006";
                string resetUrl = $"{baseUrl}/auth/reset-password?token={token}";

                string subject = "Password Reset | إعادة تعيين كلمة المرور - One May";
                string currentYear = DateTime.Now.Year.ToString();
                string logoUrl = $"{baseUrl}/logo.png";

                string htmlContent = $@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Password Reset - One May</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f0f4f8; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #f0f4f8;"">
        <tr>
            <td style=""padding: 40px 10px;"">
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""600"" align=""center"" style=""margin: auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.12);"">

                    <!-- HEADER -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #0f2744 0%, #1a365d 50%, #2c5282 100%); padding: 40px 40px 30px 40px; text-align: center;"">
                            <img src=""{logoUrl}"" alt=""One May"" width=""120"" style=""display: block; margin: 0 auto 15px auto; max-width: 120px;"" />
                            <h1 style=""margin: 0; color: #ffffff; font-size: 24px; font-weight: 700;"">One May</h1>
                            <p style=""margin: 8px 0 0 0; color: rgba(255,255,255,0.85); font-size: 14px;"">شركة واحد مايو لتنظيم المعارض والمؤتمرات</p>
                        </td>
                    </tr>

                    <!-- ICON -->
                    <tr>
                        <td align=""center"" style=""padding: 30px 40px 20px 40px;"">
                            <div style=""width: 70px; height: 70px; background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); border-radius: 50%; line-height: 70px; text-align: center; border: 3px solid #f59e0b;"">
                                <span style=""font-size: 32px;"">🔐</span>
                            </div>
                        </td>
                    </tr>

                    <!-- ENGLISH CONTENT -->
                    <tr>
                        <td style=""padding: 0 40px 25px 40px; text-align: center;"">
                            <h2 style=""margin: 0 0 15px 0; color: #1a365d; font-size: 22px; font-weight: 600;"">Password Reset Request</h2>
                            <p style=""margin: 0 0 10px 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                Hello <strong>{userName}</strong>,<br/>
                                We received a request to reset your password. Click the button below to create a new password.
                            </p>
                        </td>
                    </tr>

                    <!-- BUTTON -->
                    <tr>
                        <td style=""padding: 0 40px 30px 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"">
                                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                            <tr>
                                                <td style=""border-radius: 10px; background: linear-gradient(135deg, #2c5282 0%, #1a365d 100%); box-shadow: 0 6px 20px rgba(26, 54, 93, 0.35);"">
                                                    <a href=""{resetUrl}"" style=""background: linear-gradient(135deg, #2c5282 0%, #1a365d 100%); border: none; border-radius: 10px; color: #ffffff; display: inline-block; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 16px; font-weight: 600; line-height: 1; padding: 16px 40px; text-decoration: none;"">
                                                        🔑 Reset Password | إعادة تعيين كلمة المرور
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- EXPIRY NOTICE -->
                    <tr>
                        <td style=""padding: 0 40px 25px 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #fff8e6; border-radius: 10px; border: 1px solid #ffd966;"">
                                <tr>
                                    <td style=""padding: 15px;"">
                                        <p style=""margin: 0; color: #856404; font-size: 13px; text-align: center;"">
                                            ⏰ <strong>This link expires in 1 hour</strong> | هذا الرابط صالح لمدة ساعة واحدة فقط
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- DIVIDER -->
                    <tr>
                        <td style=""padding: 0 40px;"">
                            <hr style=""border: none; border-top: 2px solid #e2e8f0; margin: 0;"" />
                        </td>
                    </tr>

                    <!-- ARABIC CONTENT -->
                    <tr>
                        <td dir=""rtl"" style=""padding: 25px 40px; text-align: right;"">
                            <h2 style=""margin: 0 0 15px 0; color: #1a365d; font-size: 20px; font-weight: 600;"">طلب إعادة تعيين كلمة المرور</h2>
                            <p style=""margin: 0; color: #4a5568; font-size: 15px; line-height: 1.8;"">
                                مرحباً <strong>{userName}</strong>،<br/>
                                تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بك. انقر على الزر أعلاه لإنشاء كلمة مرور جديدة.
                            </p>
                        </td>
                    </tr>

                    <!-- SECURITY NOTICE -->
                    <tr>
                        <td style=""padding: 0 40px 25px 40px;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #fef2f2; border-radius: 10px; border: 1px solid #fecaca;"">
                                <tr>
                                    <td style=""padding: 15px;"">
                                        <p style=""margin: 0; color: #991b1b; font-size: 12px; text-align: center;"">
                                            🔒 If you didn't request this, please ignore this email. Your account is safe.<br/>
                                            <span dir=""rtl"">إذا لم تطلب هذا، يرجى تجاهل هذا البريد. حسابك آمن.</span>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- FOOTER -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #0f2744 0%, #1a365d 100%); padding: 30px 40px; text-align: center;"">
                            <p style=""margin: 0 0 8px 0; color: #ffffff; font-size: 16px; font-weight: 600;"">One May</p>
                            <p style=""margin: 0 0 12px 0; color: rgba(255,255,255,0.8); font-size: 13px;"">شركة واحد مايو لتنظيم المعارض والمؤتمرات</p>
                            <p style=""margin: 0; color: rgba(255,255,255,0.5); font-size: 11px;"">© {currentYear} One May. All rights reserved. | جميع الحقوق محفوظة</p>
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

Hello {userName},

We received a request to reset your password.
Please visit the following link to create a new password:

🔗 {resetUrl}

⏰ This link is valid for 1 hour only.

If you didn't request this, please ignore this email.

──────────────────────────────────────────────────────────────

مرحباً {userName}،

تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بك.
لإعادة تعيين كلمة المرور، يرجى زيارة الرابط التالي:

🔗 {resetUrl}

⏰ هذا الرابط صالح لمدة ساعة واحدة فقط.

إذا لم تطلب هذا، يرجى تجاهل هذا البريد الإلكتروني.

══════════════════════════════════════════════════════════════
© {currentYear} One May | شركة واحد مايو لتنظيم المعارض والمؤتمرات
══════════════════════════════════════════════════════════════
";

                MailHelper mailHelper = new MailHelper();
                var result = mailHelper.SendMail(email, subject, plainText, "", htmlContent);

                return result != "fail";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password reset email: {ex.Message}");
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

        /// <summary>
        /// Mask email for display
        /// </summary>
        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                return "***@***.***";
            }

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            // Show first and last character of local part
            string maskedLocal;
            if (localPart.Length <= 2)
            {
                maskedLocal = new string('*', localPart.Length);
            }
            else
            {
                maskedLocal = localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];
            }

            return $"{maskedLocal}@{domain}";
        }

        /// <summary>
        /// Validate password strength
        /// </summary>
        private (bool IsValid, string Message) ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, "Password is required");
            }

            if (password.Length < 8)
            {
                return (false, "Password must be at least 8 characters long");
            }

            // Check for at least one uppercase letter
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter");
            }

            // Check for at least one lowercase letter
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter");
            }

            // Check for at least one digit
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "Password must contain at least one number");
            }

            return (true, null);
        }
    }
}
