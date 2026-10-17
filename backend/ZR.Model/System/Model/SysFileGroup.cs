namespace ZR.Model.System.Model
{
    /// <summary>
    /// File group
    /// </summary>
    [SugarTable("sys_file_group")]
    [Tenant(0)]
    public class SysFileGroup : SysBase
    {
        /// <summary>
        /// ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int GroupId { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Parent ID
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int ParentId { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// Is deleted
        /// </summary>
        public int? IsDelete { get; set; }

        /// <summary>
        /// Add time
        /// </summary>
        [SugarColumn(InsertServerTime = true)]
        public DateTime? AddTime { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<SysFileGroup> Children { get; set; }
    }
}
