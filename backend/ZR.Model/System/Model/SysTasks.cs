namespace ZR.Model.System
{
    ///<summary>
    ///Scheduled tasks
    ///</summary>
    [SugarTable("sys_tasks", "Scheduled tasks table")]
    [Tenant("0")]
    public class SysTasks : SysBase
    {
        public SysTasks()
        {
        }

        /// <summary>
        /// Task ID
        /// </summary>
        [Display(Name = "Task ID")]
        //[JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; }

        /// <summary>
        /// Task name 
        /// </summary>
        [Display(Name = "Task name")]
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string Name { get; set; }

        /// <summary>
        /// Task group 
        /// </summary>
        [Display(Name = "Task group")]
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string JobGroup { get; set; }

        /// <summary>
        /// Runtime expression 
        /// </summary>
        [Display(Name = "Runtime expression")]
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string Cron { get; set; }

        /// <summary>
        /// Assembly name 
        /// </summary>
        [Display(Name = "Assembly name")]
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string AssemblyName { get; set; }

        /// <summary>
        /// Task class 
        /// </summary>
        [Display(Name = "Task class")]
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string ClassName { get; set; }

        /// <summary>
        /// Execution count 
        /// </summary>
        [Display(Name = "Execution count")]
        [SugarColumn(DefaultValue = "0", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int RunTimes { get; set; }

        /// <summary>
        /// Start time 
        /// </summary>
        [Display(Name = "Start time")]
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// End time 
        /// </summary>
        [Display(Name = "End time")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Trigger type (0=simple, 1=cron) 
        /// Default: 1
        /// </summary>
        [Display(Name = "Trigger type (0=simple, 1=cron)")]
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int TriggerType { get; set; }

        /// <summary>
        /// Execution interval time (unit: seconds) 
        /// Default: 0
        /// </summary>
        [Display(Name = "Execution interval time (unit: seconds)")]
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int IntervalSecond { get; set; }

        /// <summary>
        /// Whether to start
        /// </summary>
        [Display(Name = "Whether to start")]
        [SugarColumn(DefaultValue = "0", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int IsStart { get; set; }

        /// <summary>
        /// Input parameters
        /// </summary>
        [Display(Name = "Input parameters")]
        public string JobParams { get; set; }

        /// <summary>
        /// Last run time
        /// </summary>
        public DateTime? LastRunTime { get; set; }
        /// <summary>
        /// API execution URL
        /// </summary>
        public string ApiUrl { get; set; }
        /// <summary>
        /// Task type 1=Assembly, 2=Network request, 3=SQL statement
        /// </summary>
        [SugarColumn(DefaultValue = "1")]
        public int TaskType { get; set; }

        /// <summary>
        /// SQL statement
        /// </summary>
        public string SqlText { get; set; }
        /// <summary>
        /// Network request method
        /// </summary>
        [SugarColumn(Length = 20)]
        public string RequestMethod { get; set; }
    }
}

