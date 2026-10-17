
namespace ZR.Model.Hr.Dto
{
    /// <summary>
    /// insuranceenrollment查询对象
    /// </summary>
    public class HrInsuranceEnrollmentQueryDto : PagerInfo 
    {
        public long? Employeeid { get; set; }
        public long? Planid { get; set; }
        public DateTime? BeginEnrollmentdate { get; set; }
        public DateTime? EndEnrollmentdate { get; set; }
        public DateTime? BeginTerminationdate { get; set; }
        public DateTime? EndTerminationdate { get; set; }
        public float? Coverageamount { get; set; }
        public float? Monthlypremium { get; set; }
        public string Status { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// insuranceenrollment输入输出对象
    /// </summary>
    public class HrInsuranceEnrollmentDto
    {
        [ExcelColumn(Name = "Enrollmentid")]
        [ExcelColumnName("Enrollmentid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Enrollmentid { get; set; }

        [Required(ErrorMessage = "معرف الموظف不能为空")]
        [ExcelColumn(Name = "معرف الموظف")]
        [ExcelColumnName("معرف الموظف")]
        [JsonConverter(typeof(ValueToStringConverter))]

        public long Employeeid { get; set; }

        [Required(ErrorMessage = "معرف الخطة不能为空")]
        [ExcelColumn(Name = "معرف الخطة")]
        [ExcelColumnName("معرف الخطة")]
        [JsonConverter(typeof(ValueToStringConverter))]

        public long Planid { get; set; }

        [Required(ErrorMessage = "تاريخ التسجيل不能为空")]
        [ExcelColumn(Name = "تاريخ التسجيل", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("تاريخ التسجيل")]
        public DateTime? Enrollmentdate { get; set; }

        [ExcelColumn(Name = "تاريخ الإنهاء", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("تاريخ الإنهاء")]
        public DateTime? Terminationdate { get; set; }

        [Required(ErrorMessage = "مبلغ التغطية不能为空")]
        [ExcelColumn(Name = "مبلغ التغطية")]
        [ExcelColumnName("مبلغ التغطية")]
        public float Coverageamount { get; set; }

        [ExcelColumn(Name = "القسط الشهري")]
        [ExcelColumnName("القسط الشهري")]
        public float Monthlypremium { get; set; }

        [ExcelColumn(Name = "حالة التأمين")]
        [ExcelColumnName("حالة التأمين")]
        public string Status { get; set; }

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



        [ExcelColumn(Name = "معرف الموظف")]
        public string EmployeeidLabel { get; set; }
        [ExcelColumn(Name = "معرف الخطة")]
        public string PlanidLabel { get; set; }
        [ExcelColumn(Name = "حالة التأمين")]
        public string StatusLabel { get; set; }
    }
}
