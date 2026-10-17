
namespace ZR.Model.Hr.Dto
{
    /// <summary>
    /// Holiday Table查询对象
    /// </summary>
    public class HrHolidayQueryDto : PagerInfo 
    {
        public long? Holidayid { get; set; }
        public DateTime? BeginHolidaydate { get; set; }
        public DateTime? EndHolidaydate { get; set; }
        public string Holidayname { get; set; }
        public string Holidaynamear { get; set; }
        public string Holidaytype { get; set; }
        public string Ispaid { get; set; }
        public int? Year { get; set; }
        public string Description { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Holiday Table输入输出对象
    /// </summary>
    public class HrHolidayDto
    {
        [Required(ErrorMessage = "Holidayid不能为空")]
        [ExcelColumn(Name = "Holidayid")]
        [ExcelColumnName("Holidayid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Holidayid { get; set; }

        [ExcelColumn(Name = "Holidaydate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Holidaydate")]
        public DateTime? Holidaydate { get; set; }

        [ExcelColumn(Name = "Holidayname")]
        [ExcelColumnName("Holidayname")]
        public string Holidayname { get; set; }

        [ExcelColumn(Name = "Holidaynamear")]
        [ExcelColumnName("Holidaynamear")]
        public string Holidaynamear { get; set; }

        [ExcelColumn(Name = "Holidaytype")]
        [ExcelColumnName("Holidaytype")]
        public string Holidaytype { get; set; }

        [ExcelColumn(Name = "Ispaid")]
        [ExcelColumnName("Ispaid")]
        public string Ispaid { get; set; }

        [ExcelColumn(Name = "Year")]
        [ExcelColumnName("Year")]
        public int? Year { get; set; }

        [ExcelColumn(Name = "Description")]
        [ExcelColumnName("Description")]
        public string Description { get; set; }

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



        [ExcelColumn(Name = "Holidaytype")]
        public string HolidaytypeLabel { get; set; }
        [ExcelColumn(Name = "Ispaid")]
        public string IspaidLabel { get; set; }
    }
}
