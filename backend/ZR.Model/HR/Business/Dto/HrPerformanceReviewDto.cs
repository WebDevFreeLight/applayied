
namespace ZR.Model.HR.Business.Dto
{
    /// <summary>
    /// Performance Review Table查询对象
    /// </summary>
    public class HrPerformanceReviewQueryDto : PagerInfo 
    {
        public long? Reviewid { get; set; }
        public DateTime? BeginDuedate { get; set; }
        public DateTime? EndDuedate { get; set; }
        public int? Reviewyear { get; set; }
        public DateTime? BeginReviewdate { get; set; }
        public DateTime? EndReviewdate { get; set; }
        public int? Reviewquarter { get; set; }
        public string Reviewtype { get; set; }
        public string Reviewperiod { get; set; }
        public float? Knowledgerating { get; set; }
        public string Remark { get; set; }
        public long? Employeeid { get; set; }
        public long? Reviewerid { get; set; }
        public DateTime? BeginReviewperiodstart { get; set; }
        public DateTime? EndReviewperiodstart { get; set; }
        public DateTime? BeginReviewperiodend { get; set; }
        public DateTime? EndReviewperiodend { get; set; }
        public float? Overallrating { get; set; }
        public float? Productivityrating { get; set; }
        public float? Qualityrating { get; set; }
        public float? Attendancerating { get; set; }
        public float? Teamworkrating { get; set; }
        public float? Leadershiprating { get; set; }
        public string Strengths { get; set; }
        public string Improvements { get; set; }
        public string Goals { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public float? Communicationrating { get; set; }
        public float? Initiativerating { get; set; }
        public string Areasforimprovement { get; set; }
        public string Goalsachieved { get; set; }
        public string Trainingneeds { get; set; }
        public string Careeraspirations { get; set; }
        public string Managercomments { get; set; }
        public string Employeecomments { get; set; }
        public DateTime? BeginSubmitteddate { get; set; }
        public DateTime? EndSubmitteddate { get; set; }
        public DateTime? BeginApproveddate { get; set; }
        public DateTime? EndApproveddate { get; set; }
        public DateTime? BeginSelfreviewdate { get; set; }
        public DateTime? EndSelfreviewdate { get; set; }
        public string Developmentareas { get; set; }
        public string Category { get; set; }
    }

    /// <summary>
    /// Performance Review Table输入输出对象
    /// </summary>
    public class HrPerformanceReviewDto
    {
        [ExcelColumn(Name = "Reviewid")]
        [ExcelColumnName("Reviewid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Reviewid { get; set; }

        [ExcelColumn(Name = "Duedate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Duedate")]
        public DateTime? Duedate { get; set; }

        [ExcelColumn(Name = "Reviewyear")]
        [ExcelColumnName("Reviewyear")]
        public int? Reviewyear { get; set; }

        [ExcelColumn(Name = "Reviewdate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Reviewdate")]
        public DateTime? Reviewdate { get; set; }

        [ExcelColumn(Name = "Reviewquarter")]
        [ExcelColumnName("Reviewquarter")]
        public int? Reviewquarter { get; set; }

        [Required(ErrorMessage = "Reviewtype不能为空")]
        [ExcelColumn(Name = "Reviewtype")]
        [ExcelColumnName("Reviewtype")]
        public string Reviewtype { get; set; }

        [ExcelColumn(Name = "Reviewperiod")]
        [ExcelColumnName("Reviewperiod")]
        public string Reviewperiod { get; set; }

        [ExcelColumn(Name = "Knowledgerating")]
        [ExcelColumnName("Knowledgerating")]
        public float Knowledgerating { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        [ExcelColumn(Name = "Employeeid")]
        [ExcelColumnName("Employeeid")]
        public long? Employeeid { get; set; }

        [ExcelColumn(Name = "Reviewerid")]
        [ExcelColumnName("Reviewerid")]
        public long? Reviewerid { get; set; }

        [ExcelColumn(Name = "Reviewperiodstart", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Reviewperiodstart")]
        public DateTime? Reviewperiodstart { get; set; }

        [ExcelColumn(Name = "Reviewperiodend", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Reviewperiodend")]
        public DateTime? Reviewperiodend { get; set; }

        [ExcelColumn(Name = "Overallrating")]
        [ExcelColumnName("Overallrating")]
        public float Overallrating { get; set; }

        [ExcelColumn(Name = "Productivityrating")]
        [ExcelColumnName("Productivityrating")]
        public float Productivityrating { get; set; }

        [ExcelColumn(Name = "Qualityrating")]
        [ExcelColumnName("Qualityrating")]
        public float Qualityrating { get; set; }

        [ExcelColumn(Name = "Attendancerating")]
        [ExcelColumnName("Attendancerating")]
        public float Attendancerating { get; set; }

        [ExcelColumn(Name = "Teamworkrating")]
        [ExcelColumnName("Teamworkrating")]
        public float Teamworkrating { get; set; }

        [ExcelColumn(Name = "Leadershiprating")]
        [ExcelColumnName("Leadershiprating")]
        public float Leadershiprating { get; set; }

        [ExcelColumn(Name = "Strengths")]
        [ExcelColumnName("Strengths")]
        public string Strengths { get; set; }

        [ExcelColumn(Name = "Improvements")]
        [ExcelColumnName("Improvements")]
        public string Improvements { get; set; }

        [ExcelColumn(Name = "Goals")]
        [ExcelColumnName("Goals")]
        public string Goals { get; set; }

        [ExcelColumn(Name = "Comments")]
        [ExcelColumnName("Comments")]
        public string Comments { get; set; }

        [ExcelColumn(Name = "Status")]
        [ExcelColumnName("Status")]
        public string Status { get; set; }

        [ExcelColumn(Name = "Communicationrating")]
        [ExcelColumnName("Communicationrating")]
        public float Communicationrating { get; set; }

        [ExcelColumn(Name = "Initiativerating")]
        [ExcelColumnName("Initiativerating")]
        public float Initiativerating { get; set; }

        [ExcelColumn(Name = "Areasforimprovement")]
        [ExcelColumnName("Areasforimprovement")]
        public string Areasforimprovement { get; set; }

        [ExcelColumn(Name = "Goalsachieved")]
        [ExcelColumnName("Goalsachieved")]
        public string Goalsachieved { get; set; }

        [ExcelColumn(Name = "Trainingneeds")]
        [ExcelColumnName("Trainingneeds")]
        public string Trainingneeds { get; set; }

        [ExcelColumn(Name = "Careeraspirations")]
        [ExcelColumnName("Careeraspirations")]
        public string Careeraspirations { get; set; }

        [ExcelColumn(Name = "Managercomments")]
        [ExcelColumnName("Managercomments")]
        public string Managercomments { get; set; }

        [ExcelColumn(Name = "Employeecomments")]
        [ExcelColumnName("Employeecomments")]
        public string Employeecomments { get; set; }

        [ExcelColumn(Name = "Submitteddate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Submitteddate")]
        public DateTime? Submitteddate { get; set; }

        [ExcelColumn(Name = "Approveddate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Approveddate")]
        public DateTime? Approveddate { get; set; }

        [ExcelColumn(Name = "Approvedby")]
        [ExcelColumnName("Approvedby")]
        public string Approvedby { get; set; }

        [ExcelColumn(Name = "Selfreviewdate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Selfreviewdate")]
        public DateTime? Selfreviewdate { get; set; }

        [ExcelColumn(Name = "Developmentareas")]
        [ExcelColumnName("Developmentareas")]
        public string Developmentareas { get; set; }

        [ExcelColumn(Name = "Category")]
        [ExcelColumnName("Category")]
        public string Category { get; set; }

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

        [ExcelColumn(Name = "Companyid")]
        [ExcelColumnName("Companyid")]
        public int? Companyid { get; set; }

        [ExcelColumn(Name = "BranchID Branch Name")]
        [ExcelColumnName("BranchID Branch Name")]
        public int? BranchID { get; set; }



        [ExcelColumn(Name = "Reviewtype")]
        public string ReviewtypeLabel { get; set; }
        [ExcelColumn(Name = "Employeeid")]
        public string EmployeeidLabel { get; set; }
        [ExcelColumn(Name = "Status")]
        public string StatusLabel { get; set; }
    }
}
