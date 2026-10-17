using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Model;

namespace ZR.Model.PMO
{
    /// <summary>PMO role keys (map to sys_role.role_key seeded rows).</summary>
    public static class PmoRoleKeys
    {
        public const string Admin = "pmo_admin";
        public const string ProjectManager = "pmo_pm";
        public const string Contributor = "pmo_contributor";
    }

    /// <summary>
    /// The acting user for a PMO operation. Built from the JWT in controllers, or directly in tests.
    /// Carrying this explicitly (instead of reading a static HttpContext) keeps services unit-testable.
    /// </summary>
    public class PmoActor
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; } = new();
        /// <summary>System super-admin (sys_role "admin" or user_id == 1).</summary>
        public bool IsSuperAdmin { get; set; }

        public bool IsAdmin => IsSuperAdmin || Roles.Contains(PmoRoleKeys.Admin);
        public bool IsProjectManager => IsAdmin || Roles.Contains(PmoRoleKeys.ProjectManager);
        public bool IsContributor => IsProjectManager || Roles.Contains(PmoRoleKeys.Contributor);

        /// <summary>Numeric rank for floor checks: Admin 3, PM 2, Contributor 1, none 0.</summary>
        public int Level => IsAdmin ? 3 : Roles.Contains(PmoRoleKeys.ProjectManager) ? 2
            : Roles.Contains(PmoRoleKeys.Contributor) ? 1 : 0;

        public static int RankOf(string roleKey) => roleKey switch
        {
            PmoRoleKeys.Admin => 3,
            PmoRoleKeys.ProjectManager => 2,
            PmoRoleKeys.Contributor => 1,
            _ => 0
        };

        public static PmoActor From(TokenModel token) => new()
        {
            UserId = token.UserId,
            UserName = token.UserName,
            Roles = token.RoleKeys ?? new List<string>(),
            IsSuperAdmin = token.IsAdmin()
        };
    }
}
