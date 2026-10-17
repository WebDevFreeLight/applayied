
namespace ZR.Model.Boulevard.Dto
{
    /// <summary>
    /// CustomerRegistrationForm查询对象
    /// </summary>
    public class BlvCustomerregistrationformQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Name { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Phone { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Email { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Notes { get; set; }
    }

    /// <summary>
    /// CustomerRegistrationForm输入输出对象
    /// </summary>
    public class BlvCustomerregistrationformDto
    {
        [Required(ErrorMessage = "id不能为空")]
        [ExcelColumn(Name = "id")]
        [ExcelColumnName("id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        [ExcelColumn(Name = "Name")]
        [ExcelColumnName("Name")]
        public string Name { get; set; }
        [ExcelColumn(Name = "Phone")]
        [ExcelColumnName("Phone")]
        public string Phone { get; set; }
        [ExcelColumn(Name = "Email")]
        [ExcelColumnName("Email")]
        public string Email { get; set; }
        [ExcelColumn(Name = "Notes")]
        [ExcelColumnName("Notes")]
        public string Notes { get; set; }
        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }
        [ExcelColumn(Name = "Create time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create time")]
        public DateTime? CreateTime { get; set; }
        [ExcelColumn(Name = "Updater")]
        [ExcelColumnName("Updater")]
        public string UpdateBy { get; set; }
        [ExcelColumn(Name = "Update time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Update time")]
        public DateTime? UpdateTime { get; set; }
        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }
        [ExcelColumn(Name = "Companyid")]
        [ExcelColumnName("Companyid")]
        public int? Companyid { get; set; }
        [ExcelColumn(Name = "BranchID")]
        [ExcelColumnName("BranchID")]
        public int? BranchID { get; set; }


    }
}
