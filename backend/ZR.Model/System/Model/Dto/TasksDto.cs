namespace ZR.Model.System.Dto
{
    public class TasksQueryDto : PagerInfo
    {
        /// <summary>
        /// ?????
        /// </summary>
        [Display(Name = "?????")]
        public string QueryText { get; set; }
        public int? TaskType { get; set; }
        public int? TriggerType { get; set; }
        public int? IsStart { get; set; }
    }

    /// <summary>
    /// ????
    /// </summary>
    public class TasksCreateDto
    {
        /// <summary>
        /// ??id
        /// </summary>
        [Display(Name = "??id")]
        //[Required(ErrorMessage = "??????")]
        public string ID { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [Display(Name = "????")]
        [Required(ErrorMessage = "Task name cannot be empty")]
        public string Name { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [Display(Name = "????")]
        [Required(ErrorMessage = "Task group cannot be empty")]
        public string JobGroup { get; set; }

        /// <summary>
        /// ???????
        /// </summary>
        [Display(Name = "???????")]
        public string Cron { get; set; }

        /// <summary>
        /// ?????
        /// </summary>
        [Display(Name = "?????")]
        public string AssemblyName { get; set; }

        /// <summary>
        /// ?????
        /// </summary>
        [Display(Name = "?????")]
        public string ClassName { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [Display(Name = "????")]
        public string Remark { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [Display(Name = "????")]
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [Display(Name = "????")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// ?????(0?simple 1?cron)
        /// </summary>
        [Display(Name = "?????(0?simple 1?cron)")]
        public int TriggerType { get; set; }

        /// <summary>
        /// ??????(??:?)
        /// </summary>
        [Display(Name = "??????(??:?)")]
        public int IntervalSecond { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        [Display(Name = "????")]
        public string JobParams { get; set; }
        public string ApiUrl { get; set; }
        /// <summary>
        /// 1?????? 2?apiUrl?? 3?SQL??
        /// </summary>
        public int TaskType { get; set; }
        /// <summary>
        /// SQL??
        /// </summary>
        public string SqlText { get; set; }

        /// <summary>
        /// ??????
        /// </summary>
        public string RequestMethod { get; set; }
    }
}

