 using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Infrastructure.Model
{
    public class TokenModel
    {
        /// <summary>
        /// ??id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// ??id
        /// </summary>
        public long DeptId { get; set; }
        /// <summary>
        /// ?????
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// ????(eg:admin,common)
        /// </summary>
        public List<string> RoleKeys { get; set; } = [];
        /// <summary>
        /// ????(????????)
        /// </summary>
        public List<Roles> Roles { get; set; }
        /// <summary>
        /// Jwt????
        /// </summary>
        public DateTime ExpireTime { get; set; }
        /// <summary>
        /// ??ID
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// ??????
        /// </summary>
        public List<string> Permissions { get; set; } = [];



        public long CompanyID { get; set; } = 0;
        public long BranchID { get; set; } = 0;



        public TokenModel()
        {
        }

        public TokenModel(TokenModel info, List<Roles> roles)
        {
            UserId = info.UserId;
            UserName = info.UserName;




            CompanyID = info.CompanyID;
            BranchID = info.BranchID;



            DeptId = info.DeptId;
            Roles = roles;
            NickName = info.NickName;
            RoleKeys = roles.Select(f => f.RoleKey).ToList();
        }

        public bool HasPermission(string permission)
        {
            if (IsAdmin()) return true;
            return Permissions != null && Permissions.Contains(permission);
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <returns></returns>
        public bool IsAdmin()
        {
            return RoleKeys.Contains(GlobalConstant.AdminRole) || UserId == 1;
        }
    }

    public class Roles
    {
        public long RoleId { get; set; }
        public string RoleKey { get; set; }
        public int DataScope { get; set; }
    }
}

