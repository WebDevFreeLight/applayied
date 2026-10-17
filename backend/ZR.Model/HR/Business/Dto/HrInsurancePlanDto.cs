
namespace ZR.Model.HR.Business.Dto
{
    /// <summary>
    /// Insurance Plans Table查询对象
    /// </summary>
    public class HrInsurancePlanQueryDto : PagerInfo 
    {
        public string Planname { get; set; }
        public string Provider { get; set; }
        public string Plantype { get; set; }
        public string Coveragelevel { get; set; }
        public string Coveragedetails { get; set; }
        public float? Monthlypremium { get; set; }
        public float? Employercontribution { get; set; }
        public float? Employeecontribution { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// Insurance Plans Table输入输出对象
    /// </summary>
    public class HrInsurancePlanDto
    {
        [ExcelColumn(Name = "Planid")]
        [ExcelColumnName("Planid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Planid { get; set; }

        [Required(ErrorMessage = "اسم الخطة不能为空")]
        [ExcelColumn(Name = "اسم الخطة")]
        [ExcelColumnName("اسم الخطة")]
        public string Planname { get; set; }

        [Required(ErrorMessage = "مزود不能为空")]
        [ExcelColumn(Name = "مزود")]
        [ExcelColumnName("مزود")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "نوع الخطة不能为空")]
        [ExcelColumn(Name = "نوع الخطة")]
        [ExcelColumnName("نوع الخطة")]
        public string Plantype { get; set; }

        [Required(ErrorMessage = "مستوى التغطية不能为空")]
        [ExcelColumn(Name = "مستوى التغطية")]
        [ExcelColumnName("مستوى التغطية")]
        public string Coveragelevel { get; set; }

        [ExcelColumn(Name = "تفاصيل التغطية")]
        [ExcelColumnName("تفاصيل التغطية")]
        public string Coveragedetails { get; set; }

        [ExcelColumn(Name = "القسط الشهري")]
        [ExcelColumnName("القسط الشهري")]
        public float Monthlypremium { get; set; }

        [ExcelColumn(Name = "مساهمة صاحب العمل")]
        [ExcelColumnName("مساهمة صاحب العمل")]
        public float Employercontribution { get; set; }

        [ExcelColumn(Name = "مساهمة الموظف")]
        [ExcelColumnName("مساهمة الموظف")]
        public float Employeecontribution { get; set; }

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



        [ExcelColumn(Name = "نوع الخطة")]
        public string PlantypeLabel { get; set; }
        [ExcelColumn(Name = "مستوى التغطية")]
        public string CoveragelevelLabel { get; set; }
    }
}
