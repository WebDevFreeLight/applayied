using System.ComponentModel.DataAnnotations;

namespace ZR.Model.System.Dto
{
    /// <summary>
    /// Position-Role mapping DTO
    /// </summary>
    public class SysPostRoleDto
    {
        /// <summary>
        /// Position ID
        /// </summary>
        [Required(ErrorMessage = "Position ID is required")]
        public long PostId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// Role IDs array (for batch operations)
        /// </summary>
        public long[] RoleIds { get; set; }

        /// <summary>
        /// Is primary role
        /// </summary>
        public int IsPrimary { get; set; } = 0;
    }

    /// <summary>
    /// Position with its assigned roles
    /// </summary>
    public class PostWithRolesDto
    {
        /// <summary>
        /// Position ID
        /// </summary>
        public long PostId { get; set; }

        /// <summary>
        /// Position Code
        /// </summary>
        public string PostCode { get; set; }

        /// <summary>
        /// Position Name
        /// </summary>
        public string PostName { get; set; }

        /// <summary>
        /// Position Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Assigned Role IDs
        /// </summary>
        public long[] RoleIds { get; set; }

        /// <summary>
        /// Assigned Roles (for display)
        /// </summary>
        public List<SysRole> Roles { get; set; } = new List<SysRole>();
    }

    /// <summary>
    /// Query parameters for Position-Role listing
    /// </summary>
    public class SysPostRoleQueryDto : PagerInfo
    {
        /// <summary>
        /// Position ID filter
        /// </summary>
        public long? PostId { get; set; }

        /// <summary>
        /// Role ID filter
        /// </summary>
        public long? RoleId { get; set; }

        /// <summary>
        /// Position Name filter
        /// </summary>
        public string PostName { get; set; }
    }
}
