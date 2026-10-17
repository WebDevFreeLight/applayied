namespace ZR.Model.System
{
    /// <summary>
    /// Notice announcement table
    ///
    /// @author zr
    /// @date 2021-12-15
    /// </summary>
    [SugarTable("sys_notice", "Notice announcement table")]
    [Tenant(0)]
    public class SysNotice : SysBase
    {
        /// <summary>
        /// Notice ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "notice_id")]
        public int NoticeId { get; set; }
        /// <summary>
        /// Notice title
        /// </summary>
        [SugarColumn(ColumnName = "notice_title", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string NoticeTitle { get; set; }
        /// <summary>
        /// Notice type (1=Notification 2=Announcement)
        /// </summary>
        [SugarColumn(ColumnName = "notice_type", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int NoticeType { get; set; }
        /// <summary>
        /// Notice content
        /// </summary>
        [SugarColumn(ColumnName = "notice_content", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string NoticeContent { get; set; }
        /// <summary>
        /// Notice status (0=Normal 1=Closed)
        /// </summary>
        [SugarColumn(DefaultValue = "0", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int Status { get; set; }
        /// <summary>
        /// Publisher
        /// </summary>
        public string Publisher { get; set; }
        /// <summary>
        /// Start time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// End time
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// Popup notification
        /// </summary>
        public int Popup { get; set; }
    }
}
