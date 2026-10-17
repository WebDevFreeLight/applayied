using ZR.Model.System.Generate;

namespace ZR.Model.System.Dto
{
    public class GenTableDto
    {
        public int TableId { get; set; }
        public string TableName { get; set; }
        public string TableComment { get; set; }
        public string SubTableName { get; set; }
        public string SubTableFkName { get; set; }
        public string ClassName { get; set; }
        public string TplCategory { get; set; }
        public string BaseNameSpace { get; set; }
        public string ModuleName { get; set; }
        public string BusinessName { get; set; }
        public string FunctionName { get; set; }
        public string FunctionAuthor { get; set; }
        public string GenType { get; set; }
        public string GenPath { get; set; }
        //public string PermissionPrefix { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public CodeOptions Params { get; set; }
        public List<GenTableColumnDto> Columns { get; set; }
    }

    public class GenTableColumnDto
    {
        public int ColumnId { get; set; }
        public int TableId { get; set; }
        public string ColumnComment { get; set; }
        public string CsharpType { get; set; }
        public string CsharpField { get; set; }
        public bool IsInsert { get; set; }
        public bool IsEdit { get; set; }
        public bool IsList { get; set; }
        public bool IsQuery { get; set; }
        public bool IsSort { get; set; }
        public bool IsRequired { get; set; }
        public bool IsExport { get; set; }
        /// <summary>
        /// ????(????????????????????????)
        /// </summary>
        public string HtmlType { get; set; }
        /// <summary>
        /// ????(???????????????)
        /// </summary>
        public string QueryType { get; set; } = "EQ";
        public int Sort { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string DictType { get; set; }
        /// <summary>
        /// ??
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// ??????
        /// </summary>
        public int? AutoFillType { get; set; }
    }
}

