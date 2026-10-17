
namespace ZR.Model.System.Public
{
    /// <summary>
    /// Advertisement management
    /// </summary>
    [SugarTable("banner_config")]
    
    [SqlDict(
        dictType: "banner_config",
        dictName: "Banner Config",
        valueColumn: "id",
        labelColumns: "title",
        OrderBy = "sortid ASC",
        Description = "Auto-generated from BannerConfig table"
    )]
    public class BannerConfig
    {
        /// <summary>
        /// id 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// Title 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description 
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Link 
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Image 
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// Jump type 0=No jump, 1=External link, 2=Internal jump
        /// </summary>
        public int JumpType { get; set; }

        /// <summary>
        /// Add time 
        /// </summary>
        public DateTime? AddTime { get; set; }

        /// <summary>
        /// Click count 
        /// </summary>
        public int? ClicksNumber { get; set; }

        /// <summary>
        /// Whether to show 
        /// </summary>
        public int ShowStatus { get; set; }

        /// <summary>
        /// Advertisement type 
        /// </summary>
        public int AdType { get; set; }

        /// <summary>
        /// BeginTime 
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// EndTime 
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Sort ID 
        /// </summary>
        public int? SortId { get; set; }

    }
}
