using System.Threading.Tasks;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Employee user-account provisioning/linking service (SRP: identity concerns)
    /// </summary>
    public interface IEmployeeUserAccountService
    {
        /// <summary>
        /// Ensure employee has a linked SysUser on create (create or link existing by username).
        /// Returns linked userId or null if not linked.
        /// </summary>
        Task<long?> EnsureUserLinkedOnCreateAsync(EmployeeApiDto employee);

        /// <summary>
        /// Ensure employee has a linked SysUser on update (create or link existing by username if employee doesn't already have UserId).
        /// Returns linked userId or null if not linked.
        /// </summary>
        Task<long?> EnsureUserLinkedOnUpdateAsync(EmployeeApiDto employee, EmployeeApiDto existingEmployee);

        /// <summary>
        /// Activate system access for an employee.
        /// Creates a user account with roles derived from the employee's position.
        /// Returns the created userId or null if activation failed.
        /// </summary>
        Task<EmployeeSystemAccessResult> ActivateSystemAccessAsync(long employeeId);

        /// <summary>
        /// Preview the roles that would be assigned when activating system access.
        /// Returns the position and its associated roles.
        /// </summary>
        Task<EmployeeAccessPreviewDto> PreviewSystemAccessAsync(long employeeId);
    }

    /// <summary>
    /// Result of system access activation
    /// </summary>
    public class EmployeeSystemAccessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public long[] RoleIds { get; set; }
        public string[] RoleNames { get; set; }
    }

    /// <summary>
    /// Preview of what roles an employee would get
    /// </summary>
    public class EmployeeAccessPreviewDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Email { get; set; }
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
        public bool HasSystemAccess { get; set; }
        public long? ExistingUserId { get; set; }
        public long[] DerivedRoleIds { get; set; }
        public string[] DerivedRoleNames { get; set; }
    }
}















