using System.ComponentModel.DataAnnotations;

namespace ZR.Model.System.Dto
{
    /// <summary>
    /// Invite employee request DTO
    /// </summary>
    public class InviteEmployeeDto
    {
        /// <summary>
        /// Employee ID to invite
        /// </summary>
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Email address for the invitation
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// Role IDs to assign to the user
        /// </summary>
        public long[] RoleIds { get; set; }

        /// <summary>
        /// Whether to send invitation email
        /// </summary>
        public bool SendEmail { get; set; } = true;
    }

    /// <summary>
    /// Resend invitation request DTO
    /// </summary>
    public class ResendInvitationDto
    {
        /// <summary>
        /// Employee ID to resend invitation
        /// </summary>
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }
    }

    /// <summary>
    /// Invitation result response
    /// </summary>
    public class InvitationResultDto
    {
        public long UserId { get; set; }
        public string Email { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool EmailSent { get; set; }
    }

    /// <summary>
    /// Validate invitation token response
    /// </summary>
    public class InvitationValidationResultDto
    {
        public bool Valid { get; set; }
        public string EmployeeName { get; set; }
        public string Email { get; set; }
        public string EmployeeCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Activate account request DTO
    /// </summary>
    public class ActivateAccountDto
    {
        /// <summary>
        /// Invitation token
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
    /// Activation result response
    /// </summary>
    public class ActivationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
    }
}
