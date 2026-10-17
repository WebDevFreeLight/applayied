using MiniExcelLibs.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.System.Dto
{
    /// <summary>
    /// 通知公告表输入对象
    /// </summary>
    public class SysNoticeDto
    {
        //[Required(ErrorMessage = "公告ID不能为空")]
        [ExcelColumn(Name = "公告ID")]
        [ExcelColumnName("公告ID")]
        public int NoticeId { get; set; }

        [Required(ErrorMessage = "Notice title cannot be empty")]
        [ExcelColumn(Name = "公告标题", Width = 40)]
        [ExcelColumnName("公告标题")]
        public string NoticeTitle { get; set; }

        [Required(ErrorMessage = "Notice type cannot be empty")]
        [ExcelColumn(Name = "公告类型", Ignore = true)]
        [ExcelColumnName("公告类型")]
        public int NoticeType { get; set; }

        [ExcelColumn(Name = "公告内容", Width = 80)]
        [ExcelColumnName("公告内容")]
        public string NoticeContent { get; set; }

        [Required(ErrorMessage = "Notice status (0 normal 1 closed) cannot be empty")]
        [ExcelColumn(Name = "公告状态", Ignore = true)]
        [ExcelColumnName("公告状态")]
        public int Status { get; set; }

        [ExcelColumn(Name = "创建人")]
        [ExcelColumnName("创建人")]
        public string Create_by { get; set; }

        [ExcelColumn(Name = "创建时间", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("创建时间")]
        public DateTime? Create_time { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        [ExcelColumn(Name = "公告类型")]
        public string NoticeTypeLabel { get; set; }
        [ExcelColumn(Name = "公告状态")]
        public string StatusLabel { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Publisher { get; set; }
        public int Popup { get; set; }
    }

    /// <summary>
    /// 通知公告表查询对象
    /// </summary>
    public class SysNoticeQueryDto : PagerInfo
    {
        public string NoticeTitle { get; set; }
        public int? NoticeType { get; set; }
        public string Create_by { get; set; }
        public int? Status { get; set; }
    }
}

