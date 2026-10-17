using Lazy.Captcha.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZR.Model.Models;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.ServiceCore.Model.Dto;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Login
    /// </summary>
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysLoginController : BaseController
    {


        private readonly ISysUserService sysUserService;
        private readonly ISysMenuService sysMenuService;
        private readonly ISysLoginService sysLoginService;
        private readonly ISysPermissionService permissionService;
        private readonly ICaptcha SecurityCodeHelper;
        private readonly ISysConfigService sysConfigService;
        private readonly ISysRoleService roleService;
        private readonly ISmsCodeLogService smsCodeLogService;
        private readonly IHrEmployeeCoreService _employeeCoreService;

        public SysLoginController(
            ISysMenuService sysMenuService,
            ISysUserService sysUserService,
            ISysLoginService sysLoginService,
            ISysPermissionService permissionService,
            ISysConfigService configService,
            ISysRoleService sysRoleService,
            ISmsCodeLogService smsCodeLogService,
            IHrEmployeeCoreService employeeCoreService,
            ICaptcha captcha)
        {
            SecurityCodeHelper = captcha;
            this.sysMenuService = sysMenuService;
            this.sysUserService = sysUserService;
            this.sysLoginService = sysLoginService;
            this.permissionService = permissionService;
            this.sysConfigService = configService;
            this.smsCodeLogService = smsCodeLogService;
            this._employeeCoreService = employeeCoreService;
            roleService = sysRoleService;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginBody">Login object</param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        [Log(Title = "Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginBodyDto loginBody)
        {
            if (loginBody == null) { throw new CustomException("Request parameters are invalid"); }
            loginBody.LoginIP = HttpContextExtension.GetClientUserIp(HttpContext);
            SysConfig sysConfig = sysConfigService.GetSysConfigByKey("sys.account.captchaOnOff");
            if (sysConfig?.ConfigValue != "off" && !SecurityCodeHelper.Validate(loginBody.Uuid, loginBody.Code))
            {
                return ToResponse(ResultCode.CAPTCHA_ERROR, "Verification code error");
            }

            sysLoginService.CheckLockUser(loginBody.Username);
            string location = HttpContextExtension.GetIpInfo(loginBody.LoginIP);
            var user = sysLoginService.Login(loginBody, new SysLogininfor() { LoginLocation = location });

            // Get user details with roles for permission checking
            var userDto = sysUserService.SelectUserById(user.UserId).Adapt<SysUserDto>();
            userDto.Password = string.Empty; // Don't send password to frontend

            List<SysRole> roles = roleService.SelectUserRoleListByUserId(user.UserId);

            // Set RoleIds on userDto so IsAdmin check works correctly
            userDto.RoleIds = roles.Select(r => r.RoleId).ToArray();
            userDto.Roles = roles;

            //Permission set eg *:*:*,system:user:list
            List<string> permissions = permissionService.GetMenuPermission(userDto);

            TokenModel loginUser = new(user.Adapt<TokenModel>(), roles.Adapt<List<Roles>>())
            {
                TenantId = loginBody.TenantId,
                Permissions = permissions,
            };

            // Generate JWT token
            string token = JwtUtil.GenerateJwtToken(JwtUtil.AddClaims(loginUser));

            // Get role keys for frontend (e.g., ["admin", "common"])
            List<string> roleKeys = permissionService.GetRolePermission(userDto);

            // Get dynamic menu structure for frontend (non-fatal if it fails)
            List<ZR.Model.System.SysMenu> menus = new();
            try
            {
                menus = sysMenuService.SelectMenuTreeByUserId(user.UserId);
            }
            catch (Exception ex)
            {
                // Admin loads all menus — non-fatal if query/tree-build fails
                Console.WriteLine($"Menu load failed for user {user.UserId}: {ex.Message}");
            }

            // Cache permissions for later use (non-fatal if Redis/cache is down)
            try
            {
                CacheService.SetUserPerms(GlobalConstant.UserPermKEY + user.UserId, permissions);
            }
            catch (Exception ex)
            {
                // Admin has large permissions list — cache write may fail under memory pressure
                Console.WriteLine($"Permission cache failed for user {user.UserId}: {ex.Message}");
            }

            // Get employee ID linked to this user (if exists)
            long? employeeId = null;
            string employeeName = null;
            try
            {
                long userId = user.UserId;
                var employees = await _employeeCoreService.GetListAsync(e => e.UserId.HasValue && e.UserId.Value == userId);
                var employee = employees.FirstOrDefault();
                if (employee != null)
                {
                    employeeId = employee.EmployeeId;
                    employeeName = userDto.NickName ?? userDto.UserName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Employee lookup failed for user {user.UserId}: {ex.Message}");
            }

            // Return complete authentication data in one response
            return SUCCESS(new {
                token = token,
                user = userDto,
                roles = roleKeys,
                permissions = permissions,
                menus = menus,
                employeeId = employeeId,
                employeeName = employeeName
            });
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Logout")]
        [HttpPost("logout")]
        [AllowAnonymous]
        public IActionResult LogOut()
        {
            var userid = HttpContext.GetUId();
            var name = HttpContext.GetName();

            CacheService.RemoveUserPerms(GlobalConstant.UserPermKEY + userid);
            return SUCCESS(new { name, id = userid });
        }

        /// <summary>
        /// Get user information
        /// </summary>
        /// <returns></returns>
        [HttpGet("getInfo")]
        public IActionResult GetUserInfo()
        {
            long userId = HttpContext.GetUId();
            var user = sysUserService.SelectUserById(userId);
            
            //Frontend validation button permission usage
            //Role set eg: admin,yunying,common
            List<string> roles = permissionService.GetRolePermission(user);
            //Permission set eg *:*:*,system:user:list
            List<string> permissions = permissionService.GetMenuPermission(user);
            user.WelcomeContent = GlobalConstant.WelcomeMessages[new Random().Next(0, GlobalConstant.WelcomeMessages.Length)];
            user.Password = string.Empty;
            CacheService.SetUserPerms(GlobalConstant.UserPermKEY + userId, permissions);
            return SUCCESS(new { user = user.Adapt<SysUserDto>(), roles, permissions });
        }

        /// <summary>
        /// Get routing information
        /// </summary>
        /// <returns></returns>
        [HttpGet("getRouters")]
        public IActionResult GetRouters()
        {
            long uid = HttpContext.GetUId();
            var menus = sysMenuService.SelectMenuTreeByUserId(uid);

            return SUCCESS(sysMenuService.BuildMenus(menus));
        }

        /// <summary>
        /// Get routing information
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAppRouters")]
        public IActionResult GetAppRouters(int v = 0)
        {
            long uid = HttpContext.GetUId();
            var perms = permissionService.GetMenuPermission(new SysUserDto() { UserId = uid });

            return SUCCESS(sysMenuService.GetAppMenus(perms, v));
        }

        /// <summary>
        /// Generate image verification code
        /// </summary>
        /// <returns></returns>
        [HttpGet("captchaImage")]
        [AllowAnonymous]
        public IActionResult CaptchaImage()
        {
            string uuid = Guid.NewGuid().ToString().Replace("-", "");

            SysConfig sysConfig = sysConfigService.GetSysConfigByKey("sys.account.captchaOnOff");
            var captchaOff = sysConfig?.ConfigValue ?? "0";
            var info = SecurityCodeHelper.Generate(uuid, 60);
            var obj = new { captchaOff, uuid, img = info.Base64 };

            //// Convert base64 to byte array and return as file
            //byte[] imageBytes = Convert.FromBase64String(info.Base64);
            //MemoryStream stream = new MemoryStream(imageBytes);

            //// Return as file with appropriate content type
            //return File(stream, "image/png");
            // File(stream, "image/png")

            return SUCCESS(obj);
        }

        /// <summary>
        /// Register
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("/register")]
        [AllowAnonymous]
        [Log(Title = "Register", BusinessType = BusinessType.INSERT)]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            SysConfig config = sysConfigService.GetSysConfigByKey("sys.account.register");
            if (config?.ConfigValue != "true")
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Current system has not enabled registration function!");
            }
            SysConfig sysConfig = sysConfigService.GetSysConfigByKey("sys.account.captchaOnOff");
            if (sysConfig?.ConfigValue != "off" && !SecurityCodeHelper.Validate(dto.Uuid, dto.Code))
            {
                return ToResponse(ResultCode.CAPTCHA_ERROR, "Verification code error");
            }
            dto.UserIP = HttpContext.GetClientUserIp();
            SysUser user = sysUserService.Register(dto);
            if (user.UserId > 0)
            {
                // Send verification email
                try
                {
                    var mailOptions = sysConfigService.GetSysConfigByKey("sys.email.default")?.ConfigValue;
                    if (!string.IsNullOrEmpty(mailOptions))
                    {
                        var options = JsonConvert.DeserializeObject<MailOptions>(mailOptions);
                        if (options != null)
                        {
                            MailHelper mailHelper = new(options);
                            string verificationUrl = $"{Request.Scheme}://{Request.Host}/verifyEmail?token={user.EmailVerificationToken}";
                            string emailContent = $@"
                            <h2>Email Verification</h2>
                            <p>Dear {user.UserName},</p>
                            <p>Thank you for registering. Please click the link below to verify your email address:</p>
                            <p><a href='{verificationUrl}'>{verificationUrl}</a></p>
                            <p>This link will expire in 24 hours.</p>
                            <p>If you did not register for this account, please ignore this email.</p>";

                            mailHelper.SendMail(new string[] { user.Email }, "Email Verification", "", "", emailContent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't prevent registration
                    Console.WriteLine($"Failed to send verification email: {ex.Message}");
                }
                
                return SUCCESS(new { user.UserId, user.UserName, Message = "Registration successful. Please check your email to verify your account." });
            }

            
            
            return ToResponse(ResultCode.CUSTOM_ERROR, "Registration failed, please contact administrator");
        }

        #region QR Code Login

        /// <summary>
        /// Generate QR code
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet("/GenerateQrcode")]
        [AllowAnonymous]
        public IActionResult GenerateQrcode(string uuid, string deviceId)
        {
            var state = Guid.NewGuid().ToString();
            var dict = new Dictionary<string, object>
            {
                { "state", state }
            };
            CacheService.SetScanLogin(uuid, dict);
            return SUCCESS(new
            {
                status = 1,
                state,
                uuid,
                codeContent = new { uuid, deviceId }// "https://qm.qq.com/cgi-bin/qm/qr?k=kgt4HsckdljU0VM-0kxND6d_igmfuPlL&authKey=r55YUbruiKQ5iwC/folG7KLCmZ++Y4rQVgNlvLbUniUMkbk24Y9+zNuOmOnjAjRc&noverify=0"
            });
        }

        /// <summary>
        /// Poll to check scan status
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("/VerifyScan")]
        [AllowAnonymous]
        public IActionResult VerifyScan([FromBody] ScanDto dto)
        {
            int status = -1;
            object token = string.Empty;
            if (CacheService.GetScanLogin(dto.Uuid) is Dictionary<string, object> str)
            {
                status = 0;
                str.TryGetValue("token", out token);
                if (str.ContainsKey("status") && (string)str.GetValueOrDefault("status") == "success")
                {
                    status = 2;//Scan successful
                    CacheService.RemoveScanLogin(dto.Uuid);
                }
            }

            return SUCCESS(new { status, token });
        }

        /// <summary>
        /// Mobile scan login
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("/ScanLogin")]
        [Log(Title = "Scan Login")]
        public IActionResult ScanLogin([FromBody] ScanDto dto)
        {
            if (dto == null) { return ToResponse(ResultCode.CUSTOM_ERROR, "QR code scanning failed"); }
            var name = App.HttpContext.GetName();

            sysLoginService.CheckLockUser(name);

            TokenModel tokenModel = JwtUtil.GetLoginUser(HttpContext);
            if (CacheService.GetScanLogin(dto.Uuid) is not null)
            {
                Dictionary<string, object> dict = new() { };
                dict.Add("status", "success");
                dict.Add("token", JwtUtil.GenerateJwtToken(JwtUtil.AddClaims(tokenModel)));
                CacheService.SetScanLogin(dto.Uuid, dict);

                return SUCCESS(1);
            }
            return ToResponse(ResultCode.FAIL, "QR code has expired");
        }
        #endregion

        /// <summary>
        /// Verify email address with token
        /// </summary>
        /// <param name="token">Verification token</param>
        /// <returns></returns>
        [HttpGet("/verifyEmail")]
        [AllowAnonymous]
        [Log(Title = "Email Verification", BusinessType = BusinessType.UPDATE)]
        public IActionResult VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Invalid verification token");
            }

            //bool result = sysUserService.VerifyEmail(token);
            //if (result)
            //{
            //    return SUCCESS("Email verification successful. You can now log in to your account.");
            //}
            
            return ToResponse(ResultCode.CUSTOM_ERROR, "Email verification failed. The token may be invalid or expired.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("/checkMobile")]
        [Log(Title = "Send SMS", BusinessType = BusinessType.INSERT)]
        [AllowAnonymous]
        public IActionResult CheckMobile([FromBody] PhoneLoginDto dto)
        {
            dto.LoginIP = HttpContextExtension.GetClientUserIp(HttpContext);
            var uid = HttpContext.GetUId();
            //SysConfig sysConfig = sysConfigService.GetSysConfigByKey("sys.account.captchaOnOff");
            //if (!SecurityCodeHelper.Validate(dto.Uuid, dto.Code, false))
            //{
            //    return ToResponse(ResultCode.CUSTOM_ERROR, "Verification code error");
            //}
            if (dto.SendType == 0)
            {
                var info = sysUserService.GetFirst(f => f.Phonenumber == dto.PhoneNum) ?? throw new CustomException(ResultCode.CUSTOM_ERROR, "This phone number does not exist", false);
                uid = info.UserId;
            }
            if (dto.SendType == 1)
            {
                if (sysUserService.CheckPhoneBind(dto.PhoneNum).Count > 0)
                {
                    return ToResponse(ResultCode.CUSTOM_ERROR, "Phone number already bound to other account");
                }
            }

            string location = HttpContextExtension.GetIpInfo(dto.LoginIP);

            smsCodeLogService.AddSmscodeLog(new SmsCodeLog()
            {
                Userid = uid,
                PhoneNum = dto.PhoneNum.ParseToLong(),
                SendType = dto.SendType,
                UserIP = dto.LoginIP,
                Location = location,
            });

            return SUCCESS(1);
        }

        /// <summary>
        /// Phone number login
        /// </summary>
        /// <param name="loginBody">Login object</param>
        /// <returns></returns>
        [Route("PhoneLogin")]
        [HttpPost]
        [Log(Title = "Phone Number Login")]
        [AllowAnonymous]
        public async Task<IActionResult> PhoneLogin([FromBody] PhoneLoginDto loginBody)
        {
            if (loginBody == null) { throw new CustomException("Request parameters are invalid"); }
            loginBody.LoginIP = HttpContextExtension.GetClientUserIp(HttpContext);

            if (!CacheService.CheckPhoneCode(loginBody.PhoneNum, loginBody.PhoneCode))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "SMS verification code error");
            }
            var info = sysUserService.GetFirst(f => f.Phonenumber == loginBody.PhoneNum) ?? throw new CustomException(ResultCode.CUSTOM_ERROR, "This phone number does not exist", false);
            var infoModel = info.Adapt<SysUserDto>();
            sysLoginService.CheckLockUser(info.UserName);
            string location = HttpContextExtension.GetIpInfo(loginBody.LoginIP);
            var user = sysLoginService.PhoneLogin(loginBody, new SysLogininfor() { LoginLocation = location }, infoModel);

            // Get user details with roles for permission checking
            var userDto = sysUserService.SelectUserById(user.UserId).Adapt<SysUserDto>();
            userDto.Password = string.Empty; // Don't send password to frontend

            List<SysRole> roles = roleService.SelectUserRoleListByUserId(user.UserId);

            // Set RoleIds on userDto so IsAdmin check works correctly
            userDto.RoleIds = roles.Select(r => r.RoleId).ToArray();
            userDto.Roles = roles;

            //Permission set eg *:*:*,system:user:list
            List<string> permissions = permissionService.GetMenuPermission(userDto);

            TokenModel loginUser = new(user.Adapt<TokenModel>(), roles.Adapt<List<Roles>>())
            {
                TenantId = loginBody.TenantId,
                Permissions = permissions,
            };

            // Generate JWT token
            string token = JwtUtil.GenerateJwtToken(JwtUtil.AddClaims(loginUser));

            // Get role keys for frontend (e.g., ["admin", "common"])
            List<string> roleKeys = permissionService.GetRolePermission(userDto);

            // Get dynamic menu structure for frontend (non-fatal if it fails)
            List<ZR.Model.System.SysMenu> phoneMenus = new();
            try
            {
                phoneMenus = sysMenuService.SelectMenuTreeByUserId(user.UserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Menu load failed for user {user.UserId}: {ex.Message}");
            }

            // Cache permissions for later use (non-fatal if cache fails)
            try
            {
                CacheService.SetUserPerms(GlobalConstant.UserPermKEY + user.UserId, permissions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Permission cache failed for user {user.UserId}: {ex.Message}");
            }

            // Get employee ID linked to this user (if exists)
            long? employeeId = null;
            string employeeName = null;
            try
            {
                long userId = user.UserId;
                var employees = await _employeeCoreService.GetListAsync(e => e.UserId.HasValue && e.UserId.Value == userId);
                var employee = employees.FirstOrDefault();
                if (employee != null)
                {
                    employeeId = employee.EmployeeId;
                    employeeName = userDto.NickName ?? userDto.UserName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Employee lookup failed for user {user.UserId}: {ex.Message}");
            }

            // Return complete authentication data in one response
            return SUCCESS(new {
                token = token,
                user = userDto,
                roles = roleKeys,
                permissions = permissions,
                menus = phoneMenus,
                employeeId = employeeId,
                employeeName = employeeName
            });
        }

        /// <summary>
        /// Phone number binding
        /// </summary>
        /// <param name="loginBody"></param>
        /// <returns></returns>
        [Route("/PhoneBind")]
        [HttpPost]
        [Log(Title = "Phone Number Binding")]
        [AllowAnonymous]
        public IActionResult PhoneBind([FromBody] PhoneLoginDto loginBody)
        {
            if (loginBody == null) { throw new CustomException("Request parameters are invalid"); }
            loginBody.LoginIP = HttpContextExtension.GetClientUserIp(HttpContext);
            var uid = HttpContext.GetUId();
            if (!CacheService.CheckPhoneCode(loginBody.PhoneNum, loginBody.PhoneCode))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "SMS verification code error");
            }
            var result = sysUserService.ChangePhoneNum(uid, loginBody.PhoneNum);

            return SUCCESS(result);
        }
    }
}

