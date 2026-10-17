using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    public interface ISysUserService : IBaseService<SysUser>
    {
        public PagedInfo<SysUserDto> SelectUserList(SysUserQueryDto user, PagerInfo pager);

        /// <summary>
        /// Query user by user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public SysUserDto SelectUserById(long userId);

        /// <summary>
        /// Check if username is unique
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string CheckUserNameUnique(string userName);

        /// <summary>
        /// Add and save user information
        /// </summary>
        /// <param name="sysUser"></param>
        /// <returns></returns>
        public Task<SysUser> InsertUser(SysUser sysUser);

        /// <summary>
        /// Add user with complete validation and business logic
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        public Task<SysUser> AddUser(SysUserDto userDto);

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int UpdateUser(SysUserDto user);

        public int ChangeUser(SysUser user);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phoneNum"></param>
        /// <returns></returns>
        List<long> CheckPhoneBind(string phoneNum);
        int ChangePhoneNum(long userid, string phoneNum);

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int ResetPwd(long userid, string password);

        public int ChangeUserStatus(SysUser user);

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int DeleteUser(long userid);

        /// <summary>
        /// Update user avatar
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int UpdatePhoto(SysUser user);

        /// <summary>
        /// Register
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        SysUser Register(RegisterDto dto);
        void CheckUserAllowed(SysUser user);
        void CheckUserDataScope(long userid, long loginUserId);

        /// <summary>
        /// Import
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        (string, object, object) ImportUsers(List<SysUser> users);

        SysUser Login(LoginBodyDto user);

        void UpdateLoginInfo(string userIP, long userId);
    }
}

