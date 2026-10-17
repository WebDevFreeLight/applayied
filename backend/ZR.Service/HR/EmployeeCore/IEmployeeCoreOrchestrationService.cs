using System.Threading.Tasks;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Employee Core Orchestration Service Interface
    /// </summary>
    public interface IEmployeeCoreOrchestrationService
    {
        /// <summary>
        /// Create complete employee (Person + Employee + all related entities) in one transaction
        /// </summary>
        Task<EmployeeCreationResult> CreateCompleteEmployeeAsync(CreateEmployeeCompleteDto dto);

        /// <summary>
        /// Update complete employee (all fields that are provided)
        /// </summary>
        Task<bool> UpdateCompleteEmployeeAsync(UpdateEmployeeCompleteDto dto);

        /// <summary>
        /// Terminate employee
        /// </summary>
        Task<bool> TerminateEmployeeAsync(TerminateEmployeeDto dto);

        /// <summary>
        /// Activate employee
        /// </summary>
        Task<bool> ActivateEmployeeAsync(long employeeId);

        /// <summary>
        /// Transfer employee to new department/position
        /// </summary>
        Task<bool> TransferEmployeeAsync(TransferEmployeeDto dto);

        /// <summary>
        /// Promote employee with new compensation
        /// </summary>
        Task<bool> PromoteEmployeeAsync(PromoteEmployeeDto dto);
    }
}
