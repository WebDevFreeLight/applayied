namespace ZR.Model
{
    /// <summary>
    /// Attribute to define SQL-based dictionary generation for SugarTable entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SqlDictAttribute : Attribute
    {
        /// <summary>
        /// Dictionary type code (e.g., "event_type", "zone_type")
        /// Will be prefixed with "sql_" automatically if not already present
        /// </summary>
        public string DictType { get; set; }

        /// <summary>
        /// Dictionary name for display (supports bilingual names)
        /// </summary>
        public string DictName { get; set; }

        /// <summary>
        /// Column name to use as DictValue (the actual value stored)
        /// </summary>
        public string ValueColumn { get; set; }

        /// <summary>
        /// Column name(s) to use as DictLabel (display label)
        /// Supports single column or CONCAT expression
        /// Example: "typename" or "typecode, ' - ', typename, ' - ', typenamear"
        /// </summary>
        public string LabelColumns { get; set; }

        /// <summary>
        /// Column name for remarks/description (optional)
        /// </summary>
        public string RemarkColumn { get; set; }

        /// <summary>
        /// ORDER BY clause for the SQL query (optional)
        /// Example: "typecode ASC" or "typename DESC"
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Additional WHERE clause conditions (optional)
        /// Example: "status = '0' AND is_active = true"
        /// </summary>
        public string WhereClause { get; set; }

        /// <summary>
        /// Custom SQL query (overrides auto-generation if provided)
        /// Use {tableName} placeholder for table name
        /// </summary>
        public string CustomSql { get; set; }

        /// <summary>
        /// Dictionary status (default: "0" for active)
        /// </summary>
        public string Status { get; set; } = "0";

        /// <summary>
        /// Description/remark for the dictionary type
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Constructor for SQL dictionary attribute
        /// </summary>
        /// <param name="dictType">Dictionary type code</param>
        /// <param name="dictName">Dictionary name</param>
        /// <param name="valueColumn">Column for DictValue</param>
        /// <param name="labelColumns">Column(s) for DictLabel</param>
        public SqlDictAttribute(string dictType, string dictName, string valueColumn, string labelColumns)
        {
            DictType = dictType;
            DictName = dictName;
            ValueColumn = valueColumn;
            LabelColumns = labelColumns;
        }
        public SqlDictAttribute()
        {
            
        }

        /// <summary>
        /// Ensures DictType starts with "sql_"
        /// </summary>
        public string GetPrefixedDictType()
        {
            if (string.IsNullOrEmpty(DictType))
                return null;

            return DictType.StartsWith("sql_", StringComparison.OrdinalIgnoreCase)
                ? DictType
                : $"sql_{DictType}";
        }

        /// <summary>
        /// Validates that all required properties are set
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(DictType) &&
                   !string.IsNullOrEmpty(DictName) &&
                   !string.IsNullOrEmpty(ValueColumn) &&
                   !string.IsNullOrEmpty(LabelColumns);
        }
    }
}

