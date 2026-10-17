namespace ZR.Model.System
{
    [SugarTable("sys_oper_log", "Operation log table")]
    [Tenant("0")]
    public class SysOperLog : SysBase
    {
        /// <summary>
        /// Operation ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OperId { get; set; }
        /// <summary>
        /// Operation module
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Business type (0: Other, 1: Add, 2: Modify, 3: Delete, 4: Authorize, 5: Export, 6: Import, 7: Force logout, 8: Generate code, 9: Clear data)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int BusinessType { get; set; }

        /// <summary>
        /// Business type array
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [ExcelIgnore]
        public int[] BusinessTypes { get; set; }

        /// <summary>
        /// Request method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Request type
        /// </summary>
        public string RequestMethod { get; set; }

        /// <summary>
        /// Operator category (0: Other, 1: Backend user, 2: Mobile user)
        /// </summary>
        //@Excel(name = "Operation category", readConverterExp = "0=Other,1=Backend user,2=Mobile user")
        [SugarColumn(DefaultValue = "0")]
        public int OperatorType { get; set; }

        /// <summary>
        /// Operator
        /// </summary>
        public string OperName { get; set; }

        /// <summary>
        /// Request URL
        /// </summary>
        public string OperUrl { get; set; }

        /// <summary>
        /// Operation address
        /// </summary>
        public string OperIp { get; set; }

        /// <summary>
        /// Operation location
        /// </summary>
        public string OperLocation { get; set; }

        /// <summary>
        /// Request parameters
        /// </summary>
        [SugarColumn(Length = 4000)]
        public string OperParam { get; set; }

        /// <summary>
        /// Return parameters
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string JsonResult { get; set; }

        /// <summary>
        /// Operation status (0: Normal, 1: Exception)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int Status { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [SugarColumn(Length = 4000)]
        public string ErrorMsg { get; set; }

        /// <summary>
        /// Operation time
        /// </summary>
        public DateTime? OperTime { get; set; }
        /// <summary>
        /// Operation duration
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Elapsed { get; set; }
        public string DeptName { get; set; }
    }
}

