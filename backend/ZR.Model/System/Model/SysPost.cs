namespace ZR.Model.System
{
    [SugarTable("sys_post", "Position table")]
    [Tenant("0")]
    public class SysPost : SysBase
    {
        /// <summary>
        /// Position ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PostId { get; set; }
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string PostCode { get; set; }
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string PostName { get; set; }
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public int PostSort { get; set; }
        public string Status { get; set; }





        [SugarColumn(Length = 20, IsNullable = true)]
        public string Level { get; set; } // junior, mid, senior, executive

        [SugarColumn(IsNullable = true)]
        public decimal? MinSalary { get; set; }

        [SugarColumn(IsNullable = true)]
        public decimal? MaxSalary { get; set; }

        [SugarColumn(IsNullable = true)]
        public string JobDescription { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Requirements { get; set; }




    }
}

