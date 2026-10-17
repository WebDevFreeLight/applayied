using Microsoft.AspNetCore.Mvc;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Personal Center
    /// </summary>
    [Route("system/user/profile")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysProfileController : BaseController
    {
        private readonly ISysUserService UserService;
        private readonly ISysRoleService RoleService;
        private readonly ISysUserPostService UserPostService;
        private readonly ISysDeptService DeptService;
        private readonly ISysFileService FileService;
        private IWebHostEnvironment hostEnvironment;

        public SysProfileController(
            ISysUserService userService,
            ISysRoleService roleService,
            ISysUserPostService postService,
            ISysDeptService deptService,
            ISysFileService sysFileService,
            IWebHostEnvironment hostEnvironment)
        {
            UserService = userService;
            RoleService = roleService;
            UserPostService = postService;
            DeptService = deptService;
            FileService = sysFileService;
            this.hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Personal center user information retrieval
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Profile()
        {
            long userId = HttpContext.GetUId();
            var user = UserService.SelectUserById(userId);

            var roles = RoleService.SelectUserRoleNames(userId);
            var postGroup = UserPostService.GetPostsStrByUserId(userId);
            var deptInfo = DeptService.GetFirst(f => f.DeptId == user.DeptId);
            user.DeptName = deptInfo?.DeptName ?? "-";

            return SUCCESS(new { user, roles, postGroup }, TIME_FORMAT_FULL);
        }

        /// <summary>
        /// Modify user
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "common")]
        [Log(Title = "Modify Information", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateProfile([FromBody] SysUserDto userDto)
        {
            if (userDto == null)
            {
                throw new CustomException(ResultCode.PARAM_ERROR, "Request parameters are invalid");
            }
            var user = userDto.Adapt<SysUser>().ToUpdate(HttpContext);

            int result = UserService.ChangeUser(user);
            return ToResponse(result);
        }

        /// <summary>
        /// Modify password
        /// </summary>
        /// <returns></returns>
        [HttpPut("updatePwd")]
        [ActionPermissionFilter(Permission = "common")]
        [Log(Title = "Modify Password", BusinessType = BusinessType.UPDATE, IsSaveRequestData = false)]
        public IActionResult UpdatePwd([FromBody] UpdatePwdDto dto)
        {
            long userId = HttpContext.GetUId();
            SysUser user = UserService.GetFirst(f => f.UserId == userId);
            string oldMd5 = NETCore.Encrypt.EncryptProvider.Md5(dto.OldPassword);
            string newMd5 = NETCore.Encrypt.EncryptProvider.Md5(dto.NewPassword);

            if (!user.Password.Equals(oldMd5, StringComparison.OrdinalIgnoreCase))
            {
                return ToResponse(ApiResult.Error("Change password failed, old password is incorrect"));
            }
            if (user.Password.Equals(newMd5, StringComparison.OrdinalIgnoreCase))
            {
                return ToResponse(ApiResult.Error("New password cannot be the same as old password"));
            }
            if (UserService.ResetPwd(userId, newMd5) > 0)
            {
                //TODO Update cache

                return SUCCESS(1);
            }

            return ToResponse(ApiResult.Error("Change password error, please contact administrator"));
        }

        /// <summary>
        /// Modify avatar
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("Avatar")]
        [ActionPermissionFilter(Permission = "common")]
        [Log(Title = "Modify Avatar", BusinessType = BusinessType.UPDATE, IsSaveRequestData = false)]
        public async Task<IActionResult> Avatar([FromForm(Name = "picture")] IFormFile formFile)
        {
            long userId = HttpContext.GetUId();
            if (formFile == null) throw new CustomException("Please select a file");
            Model.Dto.UploadDto dto = new()
            {
                FileDir = "avatar",
                ClassifyType = "avatar",
                UserName = HttpContext.GetName()
            };
            SysFile file = await FileService.SaveFileToLocal(hostEnvironment.WebRootPath, dto, dto.UserName, formFile);

            UserService.UpdatePhoto(new SysUser() { Avatar = file.AccessUrl, UserId = userId });
            return SUCCESS(new { imgUrl = file.AccessUrl });
        }
    }
}

