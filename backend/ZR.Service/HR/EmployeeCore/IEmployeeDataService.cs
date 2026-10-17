using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// SINGLE SOURCE OF TRUTH for employee data access
    /// All HR services should use THIS service instead of writing complex joins
    /// </summary>
    public interface IEmployeeDataService
    {
        /// <summary>
        /// Get complete employee data with all joins - SINGLE SOURCE OF TRUTH
        /// </summary>
        Task<EmployeeFullDto> GetEmployeeFullDataAsync(long employeeId);

        /// <summary>
        /// Get multiple employees with all data
        /// </summary>
        Task<List<EmployeeFullDto>> GetEmployeesFullDataAsync(List<long> employeeIds);

        /// <summary>
        /// Get all active employees with full data
        /// </summary>
        Task<List<EmployeeFullDto>> GetAllActiveEmployeesAsync();

        /// <summary>
        /// Get employees by department with full data
        /// </summary>
        Task<List<EmployeeFullDto>> GetEmployeesByDepartmentAsync(long departmentId);

        /// <summary>
        /// Get team members for a manager with full data
        /// </summary>
        Task<List<EmployeeFullDto>> GetTeamMembersAsync(long managerEmployeeId);

        /// <summary>
        /// Search employees by name or code
        /// </summary>
        Task<List<EmployeeFullDto>> SearchEmployeesAsync(string searchTerm);
    }
}
