
namespace ZR.Model.System.Public.Dto
{
    /// <summary>
    /// Advertisement management query object
    /// </summary>
    public class BannerConfigQueryDto : PagerInfo 
    {
        public string Title { get; set; }
        public int? JumpType { get; set; }
        public int? ShowStatus { get; set; }
        public int? AdType { get; set; }
    }

    /// <summary>
    /// Advertisement management input/output object
    /// </summary>
    public class BannerConfigDto
    {
        [Required(ErrorMessage = "ID cannot be empty")]
        [ExcelColumn(Name = "id")]
        [ExcelColumnName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title cannot be empty")]
        [ExcelColumn(Name = "Title")]
        [ExcelColumnName("Title")]
        public string Title { get; set; }

        [ExcelColumn(Name = "Description")]
        [ExcelColumnName("Description")]
        public string Content { get; set; }

        [ExcelColumn(Name = "Link")]
        [ExcelColumnName("Link")]
        public string Link { get; set; }

        [ExcelColumn(Name = "Image")]
        [ExcelColumnName("Image")]
        public string ImgUrl { get; set; }

        [Required(ErrorMessage = "Jump type cannot be empty")]
        [ExcelColumn(Name = "Jump type")]
        [ExcelColumnName("Jump type")]
        public int JumpType { get; set; }

        [ExcelColumn(Name = "Add time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Add time")]
        public DateTime? AddTime { get; set; }

        [ExcelColumn(Name = "Click count")]
        [ExcelColumnName("Click count")]
        public int ClicksNumber { get; set; }

        [Required(ErrorMessage = "Show status cannot be empty")]
        [ExcelColumn(Name = "Whether to show")]
        [ExcelColumnName("Whether to show")]
        public int ShowStatus { get; set; }

        [Required(ErrorMessage = "Advertisement type cannot be empty")]
        [ExcelColumn(Name = "Ad type")]
        [ExcelColumnName("Ad type")]
        public int AdType { get; set; }

        [ExcelColumn(Name = "Start time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("BeginTime")]
        public DateTime? BeginTime { get; set; }

        [ExcelColumn(Name = "End time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("EndTime")]
        public DateTime? EndTime { get; set; }

        [ExcelColumn(Name = "Sort ID")]
        [ExcelColumnName("Sort ID")]
        public int? SortId { get; set; }



        [ExcelColumn(Name = "Jump type")]
        public string JumpTypeLabel { get; set; }
        [ExcelColumn(Name = "Whether to show")]
        public string ShowStatusLabel { get; set; }
    }
}
