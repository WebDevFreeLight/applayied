using System.ComponentModel.DataAnnotations;

namespace ZR.Model.System.Dto
{
    public class RegisterDto
    {
        /// <summary>
        /// ???
        /// </summary>
        [Required(ErrorMessage = "Username cannot be empty")]
        public string Username { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [Required(ErrorMessage = "Password cannot be empty")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm password cannot be empty")]
        public string ConfirmPassword { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        public string Uuid { get; set; } = "";
        /// <summary>
        /// ??
        /// </summary>
        public string Photo { get; set; }
        public string UserIP { get; set; }
    }
}

