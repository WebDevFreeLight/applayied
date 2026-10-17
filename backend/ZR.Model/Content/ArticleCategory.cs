namespace ZR.Model.Content
{
    /// <summary>
    /// Article category
    /// </summary>
    [SugarTable("articleCategory", "Article category")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "articlecategory",
        dictName: "Article Category",
        valueColumn: "categoryid",
        labelColumns: "name",
        OrderBy = "categoryid ASC",
        Description = "Auto-generated from ArticleCategory table"
    )]
    public class ArticleCategory
    {
        /// <summary>
        /// Category ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "category_id")]
        public int CategoryId { get; set; }

        [SugarColumn(ColumnDescription = "Category name", Length = 20, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string Name { get; set; }
        [SugarColumn(ColumnDescription = "Icon")]
        public string Icon { get; set; }
        /// <summary>
        /// Order ID
        /// </summary>
        public int OrderNum { get; set; }
        public int? ParentId { get; set; }
        /// <summary>
        /// Background image
        /// </summary>
        public string BgImg { get; set; }
        /// <summary>
        /// Introduction
        /// </summary>
        public string Introduce { get; set; }
        /// <summary>
        /// Category type 0=Article, 1=Circle
        /// </summary>
        public int CategoryType { get; set; }
        /// <summary>
        /// Article count
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int ArticleNum { get; set; }
        /// <summary>
        /// Join count
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int JoinNum { get; set; }
        /// <summary>
        /// Create time
        /// </summary>
        [SugarColumn(ColumnDescription = "Create time", ColumnName = "create_time", InsertServerTime = true)]
        public DateTime? Create_time { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [SugarColumn(IsIgnore = true)]
        public List<ArticleCategory> Children { get; set; }
    }
}

