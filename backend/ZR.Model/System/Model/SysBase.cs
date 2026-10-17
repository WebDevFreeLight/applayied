namespace ZR.Model.System
{
    public class SysBase
    {
        /// <summary>
        /// Creator
        /// </summary>
        [SugarColumn(IsOnlyIgnoreUpdate = true, Length = 64, IsNullable = true)]
        [JsonProperty(propertyName: "Create_by")]
        [ExcelIgnore]
        public string Create_by { get; set; }

        /// <summary>
        /// Create time
        /// </summary>
        [SugarColumn(IsOnlyIgnoreUpdate = true, IsNullable = true, InsertServerTime = true)]
        [JsonProperty(propertyName: "Create_time")]
        [ExcelColumn(Format = "yyyy-MM-dd HH:mm:ss")]
        public DateTime Create_time { get; set; } = DateTime.Now;

        /// <summary>
        /// Updater
        /// </summary>
        [JsonIgnore]
        [JsonProperty(propertyName: "Update_by")]
        [SugarColumn(IsOnlyIgnoreInsert = true, Length = 64, IsNullable = true)]
        [ExcelIgnore]
        public string Update_by { get; set; }
                                                                                                                                                                                                                                                                        
        /// <summary>
        /// Update time
        /// </summary>
        //[JsonIgnore]
        [SugarColumn(IsOnlyIgnoreInsert = true, IsNullable = true, UpdateServerTime = true)]
        [JsonProperty(propertyName: "Update_time")]
        [ExcelIgnore]
        public DateTime? Update_time { get; set; }
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Remark { get; set; }


        [JsonIgnore]


        [SugarColumn(IsNullable = true )]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CompanyID { get; set; }
        [JsonIgnore]

        /// <summary>
        /// BranchID Branch Name
        /// </summary>
        [SugarColumn(IsNullable = true )]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BranchID { get; set; }

    }
}

