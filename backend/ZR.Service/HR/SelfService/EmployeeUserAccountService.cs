using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Common;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Service.HR.IService;
using ZR.Service.HR.EmployeeCore;
using ZR.ServiceCore;
using ZR.ServiceCore.Services;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// SRP: handles only employee ⇄ user account provisioning/linking (identity concerns).
    /// Now supports Position-based Role assignment (Phase 1).
    /// </summary>
    [AppService(ServiceType = typeof(IEmployeeUserAccountService), ServiceLifetime = LifeTime.Transient)]
    public class EmployeeUserAccountService : BaseService<SysUser>, IEmployeeUserAccountService
    {
        private readonly ISysUserService _userService;
        private readonly ISysRoleService _roleService;
        private readonly ISysPostRoleService _postRoleService;
        //private readonly IEmployeeApiMappingService _employeeMappingService;

        public EmployeeUserAccountService(
            ISysUserService userService,
            ISysRoleService roleService,
            ISysPostRoleService postRoleService )
        {
            _userService = userService;
            _roleService = roleService;
            _postRoleService = postRoleService;
            //_employeeMappingService = employeeMappingService;
        }

        public async Task<long?> EnsureUserLinkedOnCreateAsync(EmployeeApiDto employee)
        {
            if (employee == null) return null;
            if (string.IsNullOrEmpty(employee.EmployeeCode)) return null;
            if (string.IsNullOrEmpty(employee.Email)) return null;

            try
            {
                // If username already exists, link to it instead of failing employee creation.
                if (UserConstants.NOT_UNIQUE.Equals(_userService.CheckUserNameUnique(employee.EmployeeCode)))
                {
                    var existingUser = await Context.Queryable<SysUser>()
                        .Where(u => u.UserName == employee.EmployeeCode && u.DelFlag == 0)
                        .FirstAsync();
                    return existingUser?.UserId;
                }

                // Resolve roles based on position (Phase 1 enhancement)
                var roleIds = await ResolveRoleIdsAsync(employee.PositionId);
                var userDto = BuildUserDto(employee, employee.Email, roleIds, remark: $"User account created for employee {employee.EmployeeCode}");

                var createdUser = await _userService.AddUser(userDto);
                return createdUser?.UserId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create/link user account for employee {employee.EmployeeCode} during create: {ex.Message}");
                return null;
            }
        }

        public async Task<long?> EnsureUserLinkedOnUpdateAsync(EmployeeApiDto employee, EmployeeApiDto existingEmployee)
        {
            if (employee == null || existingEmployee == null) return null;
            if (existingEmployee.UserId.HasValue) return existingEmployee.UserId.Value;

            var emailForUser = employee.Email ?? existingEmployee.Email;
            if (string.IsNullOrEmpty(employee.EmployeeCode)) return null;
            if (string.IsNullOrEmpty(emailForUser)) return null;

            try
            {
                // If username already exists, link to it instead of failing the employee update.
                if (UserConstants.NOT_UNIQUE.Equals(_userService.CheckUserNameUnique(employee.EmployeeCode)))
                {
                    var existingUser = await Context.Queryable<SysUser>()
                        .Where(u => u.UserName == employee.EmployeeCode && u.DelFlag == 0)
                        .FirstAsync();
                    return existingUser?.UserId;
                }

                // Resolve roles based on position (Phase 1 enhancement)
                var positionId = employee.PositionId ?? existingEmployee.PositionId;
                var roleIds = await ResolveRoleIdsAsync(positionId);
                var userDto = BuildUserDto(employee, emailForUser, roleIds, remark: $"User account created for employee {employee.EmployeeCode} during update");

                var createdUser = await _userService.AddUser(userDto);
                return createdUser?.UserId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create/link user account for employee {employee.EmployeeCode} during update: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resolves roles for a user based on their position.
        /// Phase 1: Position-based role assignment.
        /// </summary>
        /// <param name="positionId">The employee's position ID (nullable)</param>
        /// <returns>Array of role IDs to assign</returns>
        private async Task<long[]> ResolveRoleIdsAsync(long? positionId)
        {
            var roleIds = new List<long>();

            // 1. Try to get roles from Position mapping (Phase 1 enhancement)
            if (positionId.HasValue && positionId.Value > 0)
            {
                try
                {
                    var positionRoleIds = _postRoleService.GetRoleIdsByPostId(positionId.Value);
                    if (positionRoleIds != null && positionRoleIds.Length > 0)
                    {
                        roleIds.AddRange(positionRoleIds);
                        Console.WriteLine($"[Position-Role] Resolved {positionRoleIds.Length} role(s) for position {positionId.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to get roles for position {positionId.Value}: {ex.Message}");
                }
            }

            // 2. Fallback to default Employee role if no position-based roles found
            if (!roleIds.Any())
            {
                var employeeRole = await EnsureEmployeeRoleExistsAsync();
                if (employeeRole != null)
                {
                    roleIds.Add(employeeRole.RoleId);
                    Console.WriteLine($"[Position-Role] No position roles found, using default Employee role (ID: {employeeRole.RoleId})");
                }
            }

            return roleIds.Distinct().ToArray();
        }

        private async Task<SysRole> EnsureEmployeeRoleExistsAsync()
        {
            try
            {
                const string roleKey = "employ";

                // 1) Fast path: role exists (active)
                var existingRole = _roleService.SelectRoleByKey(roleKey);
                if (existingRole != null)
                {
                    return existingRole;
                }

                // 2) Role exists but was soft-deleted / disabled? Reactivate instead of inserting a new PK.
                var anyRole = await Context.Queryable<SysRole>()
                    .Where(r => r.RoleKey == roleKey)
                    .FirstAsync();

                if (anyRole != null)
                {
                    if (anyRole.DelFlag != 0 || anyRole.Status != 0)
                    {
                        anyRole.DelFlag = 0;
                        anyRole.Status = 0;
                        anyRole.Update_by = "System";
                        anyRole.Update_time = DateTime.Now;

                        await Context.Updateable(anyRole)
                            .UpdateColumns(r => new { r.DelFlag, r.Status, r.Update_by, r.Update_time })
                            .Where(r => r.RoleId == anyRole.RoleId)
                            .ExecuteCommandAsync();
                    }

                    return anyRole;
                }

                // 3) Create role (idempotent: tolerate unique violations due to sequence mismatch / race)
                var newRole = new SysRole
                {
                    RoleName = "Employee",
                    RoleKey = roleKey,
                    RoleSort = 3,
                    DataScope = 5,
                    Status = 0, // Active
                    DelFlag = 0,
                    Remark = "Default role for employees",
                    Create_by = "System",
                    Create_time = DateTime.Now
                };

                try
                {
                    var roleId = _roleService.InsertRole(newRole);
                    newRole.RoleId = roleId;
                    return newRole;
                }
                catch (Exception ex)
                {
                    // Another request may have created it, or DB sequence may be misaligned. Re-read by key.
                    Console.WriteLine($"Warning: Failed to insert employee role, retrying read by key. {ex.Message}");
                    var retryRole = _roleService.SelectRoleByKey(roleKey);
                    return retryRole;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to ensure employee role exists: {ex.Message}");
                return null;
            }
        }

        private SysUserDto BuildUserDto(EmployeeApiDto employee, string email, long[] roleIds, string remark)
        {
            return new SysUserDto
            {
                UserName = employee.EmployeeCode,
                NickName = employee.FullName,
                Email = email,
                Phonenumber = employee.Phone,
                IsUseTran = false,
                PostIds = employee.PositionId.HasValue ? new[] { employee.PositionId.Value } : Array.Empty<long>(),
                Password = GenerateEmployeePassword(employee),
                Status = 0,
                DeptId = employee.DepartmentId ?? 0,
                Sex = employee.Gender?.ToLower() == "male" ? 0 : employee.Gender?.ToLower() == "female" ? 1 : 2,
                RoleIds = roleIds,
                Remark = remark
            };
        }

        private string GenerateEmployeePassword(EmployeeApiDto employee)
        {
            try
            {
                var firstNamePart = !string.IsNullOrEmpty(employee.FirstName)
                    ? employee.FirstName.Substring(0, Math.Min(3, employee.FirstName.Length))
                    : "Emp";
                var lastNamePart = !string.IsNullOrEmpty(employee.LastName)
                    ? employee.LastName.Substring(0, Math.Min(2, employee.LastName.Length))
                    : "";
                var codePart = employee.EmployeeCode?.Substring(Math.Max(0, employee.EmployeeCode.Length - 3)) ?? "001";

                var password = $"{firstNamePart.ToLower()}{lastNamePart.ToLower()}{codePart}@2024";
                if (password.Length < 8)
                {
                    password += "Emp123!";
                }
                return password;
            }
            catch
            {
                return "Employee123!";
            }
        }

        /// <summary>
        /// Activate system access for an employee by creating a user account with position-based roles.
        /// </summary>
        public async Task<EmployeeSystemAccessResult> ActivateSystemAccessAsync(long employeeId)
        {
            try
            {
                // 1. Get employee data with assignment info
                var employee = await GetEmployeeWithAssignmentAsync(employeeId);
                if (employee == null)
                {
                    return new EmployeeSystemAccessResult
                    {
                        Success = false,
                        Message = $"Employee with ID {employeeId} not found"
                    };
                }

                // 2. Check if already has system access
                if (employee.UserId.HasValue)
                {
                    var existingUser = await Context.Queryable<SysUser>()
                        .Where(u => u.UserId == employee.UserId.Value && u.DelFlag == 0)
                        .FirstAsync();

                    if (existingUser != null)
                    {
                        return new EmployeeSystemAccessResult
                        {
                            Success = false,
                            Message = $"Employee already has system access (User: {existingUser.UserName})",
                            UserId = existingUser.UserId,
                            UserName = existingUser.UserName
                        };
                    }
                }

                // 3. Validate required fields
                if (string.IsNullOrEmpty(employee.EmployeeCode))
                {
                    return new EmployeeSystemAccessResult
                    {
                        Success = false,
                        Message = "Employee code is required for system access activation"
                    };
                }

                if (string.IsNullOrEmpty(employee.Email))
                {
                    return new EmployeeSystemAccessResult
                    {
                        Success = false,
                        Message = "Employee email is required for system access activation"
                    };
                }

                // 4. Resolve roles based on position
                var roleIds = await ResolveRoleIdsAsync(employee.PositionId);
                var roleNames = await GetRoleNamesAsync(roleIds);

                // 5. Create user account
                var userDto = BuildUserDto(employee, employee.Email, roleIds,
                    $"System access activated for employee {employee.EmployeeCode}");

                var createdUser = await _userService.AddUser(userDto);

                if (createdUser == null)
                {
                    return new EmployeeSystemAccessResult
                    {
                        Success = false,
                        Message = "Failed to create user account"
                    };
                }

                // 6. Link user to employee
                await LinkUserToEmployeeAsync(employeeId, createdUser.UserId);

                return new EmployeeSystemAccessResult
                {
                    Success = true,
                    Message = "System access activated successfully",
                    UserId = createdUser.UserId,
                    UserName = createdUser.UserName,
                    RoleIds = roleIds,
                    RoleNames = roleNames
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error activating system access for employee {employeeId}: {ex.Message}");
                return new EmployeeSystemAccessResult
                {
                    Success = false,
                    Message = $"Activation failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Preview the roles an employee would receive upon system access activation.
        /// </summary>
        public async Task<EmployeeAccessPreviewDto> PreviewSystemAccessAsync(long employeeId)
        {
            try
            {
                var employee = await GetEmployeeWithAssignmentAsync(employeeId);
                if (employee == null)
                {
                    return null;
                }

                var roleIds = await ResolveRoleIdsAsync(employee.PositionId);
                var roleNames = await GetRoleNamesAsync(roleIds);

                // Get position name
                string positionName = null;
                if (employee.PositionId.HasValue)
                {
                    var position = await Context.Queryable<SysPost>()
                        .Where(p => p.PostId == employee.PositionId.Value)
                        .FirstAsync();
                    positionName = position?.PostName;
                }

                return new EmployeeAccessPreviewDto
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    Email = employee.Email,
                    PositionId = employee.PositionId,
                    PositionName = positionName,
                    HasSystemAccess = employee.UserId.HasValue,
                    ExistingUserId = employee.UserId,
                    DerivedRoleIds = roleIds,
                    DerivedRoleNames = roleNames
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error previewing system access for employee {employeeId}: {ex.Message}");
                return null;
            }
        }

        private async Task<EmployeeApiDto> GetEmployeeWithAssignmentAsync(long employeeId)
        {
            // Use the existing mapping service which handles all the normalized data

            return await GetEmployeeWithAssignmentAsync(employeeId);
            //return await _employeeMappingService.ToApiDtoAsync(employeeId);
        }

        private async Task<string[]> GetRoleNamesAsync(long[] roleIds)
        {
            if (roleIds == null || roleIds.Length == 0)
                return Array.Empty<string>();

            var roles = await Context.Queryable<SysRole>()
                .Where(r => roleIds.Contains(r.RoleId))
                .ToListAsync();

            return roles.Select(r => r.RoleName).ToArray();
        }

        private async Task LinkUserToEmployeeAsync(long employeeId, long userId)
        {
            await Context.Updateable<ZR.Model.HR.EmployeeCore.HrEmployeeCore>()
                .SetColumns(e => e.UserId == userId)
                .Where(e => e.EmployeeId == employeeId)
                .ExecuteCommandAsync();
        }
    }
}















