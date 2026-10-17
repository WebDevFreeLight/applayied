namespace ZR.Model.System
{
    /// <summary>
    /// Task log
    /// </summary>
    [SugarTable("sys_tasks_log", "Task log table")]
    [Tenant("0")]
    public class SysTasksLog : SysBase
    {
        /// <summary>
        /// Log ID
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long JobLogId { get; set; }
        /// <summary>
        /// Task ID
        /// </summary>
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string JobId { get; set; }
        /// <summary>
        /// Task name
        /// </summary>
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string JobName { get; set; }
        /// <summary>
        /// Task group
        /// </summary>
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string JobGroup { get; set; }
        /// <summary>
        /// Execution status (0: Normal, 1: Failed)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public string Status { get; set; }
        /// <summary>
        /// Exception
        /// </summary>
        public string Exception { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string JobMessage { get; set; }
        /// <summary>
        /// Invoke target string
        /// </summary>
        public string InvokeTarget { get; set; }

        /// <summary>
        /// Create time
        /// </summary>
        public DateTime Create_time { get; set; }
        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        //[SqlSugar.SugarColumn(IsIgnore = true)]
        public double Elapsed { get; set; }
    }
}

