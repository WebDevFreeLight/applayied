using System.Threading.Tasks;
using ZR.Model.System.Dto;

namespace ZR.Service.Auth
{
    /// <summary>
    /// Password reset service interface
    /// </summary>
    public interface IPasswordResetService
    {
        /// <summary>
        /// Request password reset email
        /// </summary>
        Task<PasswordResetResultDto> RequestPasswordResetAsync(string email);

        /// <summary>
        /// Validate password reset token
        /// </summary>
        Task<ResetTokenValidationResultDto> ValidateResetTokenAsync(string token);

        /// <summary>
        /// Reset password with valid token
        /// </summary>
        Task<PasswordResetResultDto> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
