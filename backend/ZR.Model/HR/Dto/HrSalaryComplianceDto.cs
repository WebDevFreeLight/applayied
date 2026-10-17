
namespace ZR.Model.Hr.Dto
{
    /// <summary>
    /// Salary Compliance - امتثال الرواتب查询对象
    /// </summary>
    public class HrSalaryComplianceQueryDto : PagerInfo 
    {
        public string Country { get; set; }
        public float? Socialinsurancerate { get; set; }
        public string Endofserviceformula { get; set; }
        public string Vatrequirements { get; set; }
        public string Procedures { get; set; }
        public string Currencycode { get; set; }
        public float? Minimumwage { get; set; }
        public int? Maxworkinghoursperweek { get; set; }
        public float? Overtimeratemultiplier { get; set; }
        public int? Annualleavedays { get; set; }
        public int? Sickleavedays { get; set; }
        public int? Maternityleavedays { get; set; }
        public int? Paternityleavedays { get; set; }
        public string Taxbrackets { get; set; }
        public string Notes { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// Salary Compliance - امتثال الرواتب输入输出对象
    /// </summary>
    public class HrSalaryComplianceDto
    {
        [ExcelColumn(Name = "Compliance ID")]
        [ExcelColumnName("Compliance ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Complianceid { get; set; }

        [Required(ErrorMessage = "Country - البلد不能为空")]
        [ExcelColumn(Name = "Country - البلد")]
        [ExcelColumnName("Country - البلد")]
        public string Country { get; set; }

        [ExcelColumn(Name = "Social Insurance Rate - معدل التأمينات الاجتماعية")]
        [ExcelColumnName("Social Insurance Rate - معدل التأمينات الاجتماعية")]
        public float Socialinsurancerate { get; set; }

        [ExcelColumn(Name = "End of Service Formula - صيغة نهاية الخدمة")]
        [ExcelColumnName("End of Service Formula - صيغة نهاية الخدمة")]
        public string Endofserviceformula { get; set; }

        [ExcelColumn(Name = "VAT Requirements - متطلبات ضريبة القيمة المضافة")]
        [ExcelColumnName("VAT Requirements - متطلبات ضريبة القيمة المضافة")]
        public string Vatrequirements { get; set; }

        [ExcelColumn(Name = "Procedures - الإجراءات")]
        [ExcelColumnName("Procedures - الإجراءات")]
        public string Procedures { get; set; }

        [ExcelColumn(Name = "Currency Code")]
        [ExcelColumnName("Currency Code")]
        public string Currencycode { get; set; }

        [ExcelColumn(Name = "Minimum Wage")]
        [ExcelColumnName("Minimum Wage")]
        public float Minimumwage { get; set; }

        [ExcelColumn(Name = "Maximum Working Hours Per Week")]
        [ExcelColumnName("Maximum Working Hours Per Week")]
        public int? Maxworkinghoursperweek { get; set; }

        [ExcelColumn(Name = "Overtime Rate Multiplier")]
        [ExcelColumnName("Overtime Rate Multiplier")]
        public float Overtimeratemultiplier { get; set; }

        [ExcelColumn(Name = "Annual Leave Days")]
        [ExcelColumnName("Annual Leave Days")]
        public int? Annualleavedays { get; set; }

        [ExcelColumn(Name = "Sick Leave Days")]
        [ExcelColumnName("Sick Leave Days")]
        public int? Sickleavedays { get; set; }

        [ExcelColumn(Name = "Maternity Leave Days")]
        [ExcelColumnName("Maternity Leave Days")]
        public int? Maternityleavedays { get; set; }

        [ExcelColumn(Name = "Paternity Leave Days")]
        [ExcelColumnName("Paternity Leave Days")]
        public int? Paternityleavedays { get; set; }

        [ExcelColumn(Name = "Tax Brackets")]
        [ExcelColumnName("Tax Brackets")]
        public string Taxbrackets { get; set; }

        [ExcelColumn(Name = "Compliance Notes")]
        [ExcelColumnName("Compliance Notes")]
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
