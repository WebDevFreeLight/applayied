namespace ZR.Model.System
{
    /// <summary>
    /// Dictionary type table
    /// </summary>
    [SugarTable("sys_dict_type", "Dictionary type table")]
    [SugarIndex("index_dict_type", nameof(DictType), OrderByType.Asc, true)]
    [Tenant("0")]
    public class SysDictType : SysBase, IMainDbEntity
    {
        /// <summary>
        /// Dictionary primary key
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DictId { get; set; }
        /// <summary>
        /// Dictionary name
        /// </summary>
        [SugarColumn(Length = 100, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string DictName { get; set; }
        /// <summary>
        /// Dictionary type
        /// </summary>
        [SugarColumn(Length = 100, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string DictType { get; set; }
        /// <summary>
        /// Status 0=Normal, 1=Disabled
        /// </summary>
        [SugarColumn(Length = 1, DefaultValue = "0")]
        public string Status { get; set; }
        /// <summary>
        /// System built-in Y=Yes, N=No
        /// </summary>
        [SugarColumn(Length = 1, DefaultValue = "N")]
        public string Type { get; set; }
        /// <summary>
        /// Custom SQL
        /// </summary>
        public string CustomSql { get; set; }
    }
}

