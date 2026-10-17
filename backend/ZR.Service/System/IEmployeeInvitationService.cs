using System.Threading.Tasks;
using ZR.Model.System.Dto;

namespace ZR.Service.Auth
{
    /// <summary>
    /// Employee invitation service interface
    /// </summary>
    public interface IEmployeeInvitationService
    {
        /// <summary>
        /// Invite an employee to create a system account
        /// </summary>
        Task<InvitationResultDto> InviteEmployeeAsync(InviteEmployeeDto dto);

        /// <summary>
        /// Resend invitation email with new token
        /// </summary>
        Task<InvitationResultDto> ResendInvitationAsync(long employeeId);

        /// <summary>
        /// Validate invitation token
        /// </summary>
        Task<InvitationValidationResultDto> ValidateInvitationTokenAsync(string token);

        /// <summary>
        /// Activate account with password
        /// </summary>
        Task<ActivationResultDto> ActivateAccountAsync(ActivateAccountDto dto);

        /// <summary>
        /// Get employee email from hr_person_contact
        /// </summary>
        Task<string> GetEmployeeEmailAsync(long employeeId);

        /// <summary>
        /// Get employee name from hr_person_name
        /// </summary>
        Task<string> GetEmployeeNameAsync(long employeeId);
    }
}
