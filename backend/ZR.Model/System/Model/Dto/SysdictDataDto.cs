namespace ZR.Model.System.Dto
{
    public class SysdictDataParamDto
    {
        public string DictType { get; set; }
        //public string ColumnName { get; set; }
        public List<SysDictDataDto> List { get; set; }
    }

    public class SysDictDataDto
    {
        public string DictLabel { get; set; }
        public string DictValue { get; set; }
        public string DictType { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public string ListClass { get; set; } = string.Empty;
        /// <summary>
        /// ?????key?
        /// </summary>
        public string LangKey { get; set; } = string.Empty;
        /// <summary>
        /// ??1
        /// </summary>
        public string Extend1 { get; set; }
        /// <summary>
        /// ??2
        /// </summary>
        public string Extend2 { get; set; }
        #region uniapp????
        public string Label { get; set; }
        public string Value { get; set; }
        #endregion
    }
}

