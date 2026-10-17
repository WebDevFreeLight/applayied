using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Infrastructure.Model;
using ZR.Model.Marketing.Dto;
using ZR.Model.System.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Marketing Employee Service Interface
    /// Handles marketing team employee data and queries
    /// </summary>
    public interface IMarketingEmployeeService
    {
        /// <summary>
        /// Get marketing employees with pagination and filtering
        /// </summary>
        Task<PagedInfo<MarketingEmployeeDto>> GetMarketingEmployeesAsync(MarketingEmployeeQueryDto queryDto);

        /// <summary>
        /// Get employee by ID with details
        /// </summary>
        Task<MarketingEmployeeDto> GetEmployeeByIdAsync(long id);

        /// <summary>
        /// Get all departments for filtering
        /// </summary>
        List<SysDeptDto> GetDepartments();

        /// <summary>
        /// Get employee KPIs (Key Performance Indicators)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="periodDays">Number of days to calculate KPIs for (default: 30)</param>
        Task<EmployeeKpiDto> GetEmployeeKpisAsync(long employeeId, int periodDays = 30);
    }
}
