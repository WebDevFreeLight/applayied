namespace ZR.Model.System.Dto
{
    public class SysDictTypeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DictId { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string DictName { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        [Required(ErrorMessage = "Dictionary type cannot be empty")]
        [RegularExpression(pattern: "^[a-z][a-z0-9_]*$", ErrorMessage = "Dictionary type must start with a letter and can only contain lowercase letters, underscores, and numbers")]
        public string DictType { get; set; }
        public string Status { get; set; }
        /// <summary>
        /// ???? Y? N?
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// ???sql
        /// </summary>
        public string CustomSql { get; set; }
        public string Remark { get; set; }
    }
}

