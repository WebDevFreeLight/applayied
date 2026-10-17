namespace ZR.ServiceCore.Model.Dto
{
    public class PhoneLoginDto
    {
        /// <summary>
        /// ???
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        public string Uuid { get; set; } = "";
        public string LoginIP { get; set; }
        [Required(ErrorMessage = "Phone number cannot be empty")]
        public string PhoneNum { get; set; }
        /// <summary>
        /// ???????
        /// </summary>
        //[Required(ErrorMessage = "?????????")]
        public string PhoneCode { get; set; }
        /// <summary>
        /// 0??? 1???
        /// </summary>
        public int SendType { get; set; }
        /// <summary>
        /// ??id
        /// </summary>
        public string TenantId { get; set; }
    }
}

