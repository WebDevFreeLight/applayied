namespace ZR.Model.System
{
    /// <summary>
    /// Dictionary data table
    /// </summary>
    [Tenant("0")]
    [SugarTable("sys_dict_data", "Dictionary data table")]
    public class SysDictData : SysBase, IMainDbEntity
    {
        /// <summary>
        /// Dictionary code
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DictCode { get; set; }
        /// <summary>
        /// Dictionary sort
        /// </summary>
        public int DictSort { get; set; }
        /// <summary>
        /// Dictionary label
        /// </summary>
        [SugarColumn(Length = 100, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string DictLabel { get; set; }
        /// <summary>
        /// Dictionary key value
        /// </summary>
        [SugarColumn(Length = 100, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string DictValue { get; set; }
        /// <summary>
        /// Dictionary type
        /// </summary>
        [SugarColumn(Length = 100, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string DictType { get; set; }
        /// <summary>
        /// Style attributes (other style extensions)
        /// </summary>
        [SugarColumn(Length = 100)]
        public string CssClass { get; set; } = string.Empty;
        /// <summary>
        /// Table display style
        /// </summary>
        [SugarColumn(Length = 100)]
        public string ListClass { get; set; } = string.Empty;
        /// <summary>
        /// Is default (Y=Yes, N=No)
        /// </summary>
        [SugarColumn(Length = 1, DefaultValue = "N")]
        public string IsDefault { get; set; }
        /// <summary>
        /// Status (0=Normal, 1=Disabled)
        /// </summary>
        [SugarColumn(Length = 1)]
        public string Status { get; set; }
        /// <summary>
        /// Multi-language translation key value
        /// </summary>
        public string LangKey { get; set; } = string.Empty;
        /// <summary>
        /// Extension 1
        /// </summary>
        public string Extend1 { get; set; }
        /// <summary>
        /// Extension 2
        /// </summary>
        public string Extend2 { get; set; }
    }
}

