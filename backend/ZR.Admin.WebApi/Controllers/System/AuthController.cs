using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ZR.Model.System.Dto;
using ZR.Service.Auth;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Authentication endpoints (public)
    /// Handles account activation and password reset flows
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class AuthController : BaseController
    {
        private readonly IEmployeeInvitationService _invitationService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(
            IEmployeeInvitationService invitationService,
            IPasswordResetService passwordResetService)
        {
            _invitationService = invitationService;
            _passwordResetService = passwordResetService;
        }

        #region Account Activation

        /// <summary>
        /// Validate invitation token
        /// </summary>
        /// <param name="token">Invitation token from email</param>
        /// <returns>Validation result with employee info if valid</returns>
        [HttpGet("validate-invitation")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateInvitation([FromQuery] string token)
        {
            var result = await _invitationService.ValidateInvitationTokenAsync(token);

            if (!result.Valid)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, result.ErrorMessage);
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Activate account with password
        /// </summary>
        /// <param name="dto">Activation data including token and password</param>
        /// <returns>Activation result</returns>
        [HttpPost("activate-account")]
        [AllowAnonymous]
        [Log(Title = "Account Activation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivateAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request parameters");
            }

            var result = await _invitationService.ActivateAccountAsync(dto);

            if (!result.Success)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, result.Message);
            }

            return SUCCESS(result);
        }

        #endregion

        #region Password Reset

        /// <summary>
        /// Request password reset email
        /// </summary>
        /// <param name="dto">Email address</param>
        /// <returns>Success message (always returns success to prevent email enumeration)</returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [Log(Title = "Forgot Password", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request parameters");
            }

            var result = await _passwordResetService.RequestPasswordResetAsync(dto.Email);

            // Always return success to prevent email enumeration
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate password reset token
        /// </summary>
        /// <param name="token">Reset token from email</param>
        /// <returns>Validation result</returns>
        [HttpGet("validate-reset-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
        {
            var result = await _passwordResetService.ValidateResetTokenAsync(token);

            if (!result.Valid)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, result.ErrorMessage);
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Reset password with valid token
        /// </summary>
        /// <param name="dto">Reset data including token and new password</param>
        /// <returns>Reset result</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [Log(Title = "Password Reset", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request parameters");
            }

            var result = await _passwordResetService.ResetPasswordAsync(dto);

            if (!result.Success)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, result.Message);
            }

            return SUCCESS(result);
        }

        #endregion
    }
}
