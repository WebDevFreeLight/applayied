namespace ZR.Model.System
{
    [SugarTable("sqlDiffLog", "Data difference log")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "sqldifflog",
        dictName: "Sql Diff Log",
        valueColumn: "pid",
        labelColumns: "tablename",
        RemarkColumn = "remark",
        OrderBy = "pid ASC",
        Description = "Auto-generated from SqlDiffLog table"
    )]
    public class SqlDiffLog : SysBase
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PId { get; set; }
        /// <summary>
        /// Table name
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Business data content
        /// </summary>
        [SugarColumn(Length = 4000)]
        public string BusinessData { get; set; }
        /// <summary>
        /// Difference type: insert, update, delete
        /// </summary>
        public string DiffType { get; set; }
        /// <summary>
        /// Executed SQL statement
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string Sql { get; set; }
        /// <summary>
        /// Data before change
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string BeforeData { get; set; }
        /// <summary>
        /// Data after change
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string AfterData { get; set; }
        /// <summary>
        /// Operating user name
        /// </summary>
        public string UserName { get; set; }
        public DateTime AddTime { get; set; }
        /// <summary>
        /// Database configuration ID
        /// </summary>
        public string ConfigId { get; set; }
    }
}

