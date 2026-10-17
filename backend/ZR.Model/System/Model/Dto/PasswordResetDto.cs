using System.ComponentModel.DataAnnotations;

namespace ZR.Model.System.Dto
{
    /// <summary>
    /// Forgot password request DTO
    /// </summary>
    public class ForgotPasswordDto
    {
        /// <summary>
        /// Email address or username
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
    }

    /// <summary>
    /// Validate reset token response
    /// </summary>
    public class ResetTokenValidationResultDto
    {
        public bool Valid { get; set; }
        public string Email { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Reset password request DTO
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Password reset token
        /// </summary>
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        /// <summary>
        /// New password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }

        /// <summary>
        /// Confirm password
        /// </summary>
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Password reset result response
    /// </summary>
    public class PasswordResetResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Change password request DTO (for logged-in users)
    /// </summary>
    public class UpdatePwdDto
    {
        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        public string NewPassword { get; set; }
    }
}
