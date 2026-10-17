using System.ComponentModel.DataAnnotations;

namespace ZR.Model.System.Dto
{
    public class LoginBodyDto
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

        /// <summary>
        /// ???
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        public string Uuid { get; set; } = "";
        public string LoginIP { get; set; }
        public string ClientId { get; set; }
        /// <summary>
        /// ??ID
        /// </summary>
        public string TenantId { get; set; }
    }
}

