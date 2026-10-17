using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using SqlSugar;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// User Management
    /// </summary>
    [Route("system/user")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysUserController : BaseController
    {
        private readonly ISysUserService UserService;
        private readonly ISysRoleService RoleService;
        private readonly ISysPostService PostService;
        private readonly ISysUserPostService UserPostService;

        public SysUserController(
            ISysUserService userService,
            ISysRoleService roleService,
            ISysPostService postService,
            ISysUserPostService userPostService)
        {
            UserService = userService;
            RoleService = roleService;
            PostService = postService;
            UserPostService = userPostService;
        }

        /// <summary>
        /// User Management -> Get Users
        /// /system/user/list
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:user:list")]
        [HttpGet("list")]
        public IActionResult List([FromQuery] SysUserQueryDto user, PagerInfo pager)
        {
            var list = UserService.SelectUserList(user, pager);
            
            return SUCCESS(list);
        }

        /// <summary>
        /// User Management -> Edit, Add User Get User, Information Query
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("")]
        [HttpGet("{userId:long=0}")]
        [ActionPermissionFilter(Permission = "system:user:query")]
        public IActionResult GetInfo(long userId)
        {
            Dictionary<string, object> dic = new();
            var roles = RoleService.SelectRoleAll();
            dic.Add("roles", roles);
            //dic.Add("roles", SysUser.IsAdmin(userId) ? roles : roles.FindAll(f => !f.IsAdmin()));
            dic.Add("posts", PostService.GetAll());

            //Edit
            if (userId > 0)
            {
                SysUserDto sysUser = UserService.SelectUserById(userId);
                dic.Add("user", sysUser);
                dic.Add("postIds", UserPostService.GetUserPostsByUserId(userId));
                dic.Add("roleIds", sysUser.RoleIds);
            }

            return SUCCESS(dic);
        }

        /// <summary>
        /// Add user
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [Log(Title = "User Management", BusinessType = BusinessType.INSERT)]
        [ActionPermissionFilter(Permission = "system:user:add")]
        public async Task<IActionResult> AddUser([FromBody] SysUserDto parm)
        {
            try
            {
                parm.IsUseTran=true;
                var data = await UserService.AddUser(parm);
                return SUCCESS(data);
            }
            catch (ArgumentNullException ex)
            {
                return ToResponse(ApiResult.Error(101, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ApiResult.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to add user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Modify user
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpPut("edit")]
        [Log(Title = "User Management", BusinessType = BusinessType.UPDATE)]
        [ActionPermissionFilter(Permission = "system:user:edit")]
        public IActionResult UpdateUser([FromBody] SysUserDto parm)
        {
            if (parm == null || parm.UserId <= 0) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }
            parm = parm.ToUpdate();
            int upResult = UserService.UpdateUser(parm);

            return ToResponse(upResult);
        }

        /// <summary>
        /// Change user status
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut("changeStatus")]
        [Log(Title = "Modify User Status", BusinessType = BusinessType.UPDATE)]
        [ActionPermissionFilter(Permission = "system:user:update")]
        public IActionResult ChangeStatus([FromBody] SysUser user)
        {
            if (user == null) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }

            int result = UserService.ChangeUserStatus(user);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        [Log(Title = "User Management", BusinessType = BusinessType.DELETE)]
        [ActionPermissionFilter(Permission = "system:user:remove")]
        public IActionResult Remove(long userid = 0)
        {
            if (userid <= 0) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }
            if (userid == 1) return ToResponse(ResultCode.FAIL, "Cannot delete administrator account");
            int result = UserService.DeleteUser(userid);

            return ToResponse(result);
        }

        /// <summary>
        /// Reset password
        /// </summary>
        /// <returns></returns>
        [HttpPut("resetPwd")]
        [Log(Title = "Reset Password", BusinessType = BusinessType.UPDATE)]
        [ActionPermissionFilter(Permission = "system:user:resetPwd")]
        public IActionResult ResetPwd([FromBody] SysUserDto sysUser)
        {
            //Password md5
            sysUser.Password = NETCore.Encrypt.EncryptProvider.Md5(sysUser.Password);

            int result = UserService.ResetPwd(sysUser.UserId, sysUser.Password);
            return ToResponse(result);
        }

        /// <summary>
        /// Import
        /// </summary>
        /// <param name="formFile">Using IFromFile must use name attribute otherwise cannot get file</param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "User Import", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false, IsSaveResponseData = true)]
        [ActionPermissionFilter(Permission = "system:user:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<SysUser> users = new();
            using (var stream = formFile.OpenReadStream())
            {
                users = stream.Query<SysUser>(startCell: "A2").ToList();
            }

            return SUCCESS(UserService.ImportUsers(users));
        }

        /// <summary>
        /// User import template download
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "User Template", BusinessType = BusinessType.EXPORT, IsSaveRequestData = true, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            (string, string) result = DownloadImportTemplate("user");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// User export
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("export")]
        [Log(Title = "User Export", BusinessType = BusinessType.EXPORT)]
        [ActionPermissionFilter(Permission = "system:user:export")]
        public IActionResult UserExport([FromQuery] SysUserQueryDto user)
        {
            var list = UserService.SelectUserList(user, new PagerInfo(1, 10000));

            var result = ExportExcelMini(list.Result, "user", "User List");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

