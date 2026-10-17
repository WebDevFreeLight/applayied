using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// Mall Category
    /// </summary>
    [SugarTable("mms_category", "Mall Category")]
    [Tenant("1")]
    
    [SqlDict(
        dictType: "mms_category",
        dictName: "Category",
        valueColumn: "category_id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "category_id ASC",
        Description = "Auto-generated from Category table"
    )]
    public class Category : SysBase
    {
        /// <summary>
        /// Category ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "category_id")]
        public int CategoryId { get; set; }

        [SugarColumn(ColumnDescription = "Category Name", Length = 20, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string Name { get; set; }
        [SugarColumn(ColumnDescription = "Icon")]
        public string Icon { get; set; }
        /// <summary>
        /// Sort Order
        /// </summary>
        public int OrderNum { get; set; }
        public int? ParentId { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Introduce { get; set; }
        /// <summary>
        /// Is Deleted
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int IsDelete { get; set; }
        /// <summary>
        /// Display Status
        /// </summary>
        [SugarColumn(DefaultValue = "1")]
        public int ShowStatus { get; set; }

        /// <summary>
        /// Create Time
        /// </summary>
        [SugarColumn(ColumnDescription = "Create Time", ColumnName = "create_time", InsertServerTime = true)]
        public DateTime? Create_time { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [SugarColumn(IsIgnore = true)]
        public List<Category> Children { get; set; }
    }
}

