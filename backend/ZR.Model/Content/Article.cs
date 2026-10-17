 
using Infrastructure.Converter;
using ZR.Model.Content.Enums;
using ZR.Model.System.Enums;

namespace ZR.Model.Content
{
    /// <summary>
    /// Article table
    /// </summary>
    [SugarTable("article", "Content management")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "article",
        dictName: "Article",
        valueColumn: "userid",
        labelColumns: "title",
        RemarkColumn = "commentnum",
        OrderBy = "userid ASC",
        Description = "Auto-generated from Article table"
    )]
    public class Article
    {
        /// <summary>
        /// Article ID
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Cid { get; set; }
        /// <summary>
        /// Article title
        /// </summary>
        [SugarColumn(ColumnDescription = "Article title", Length = 254)]
        public string Title { get; set; }
        /// <summary>
        /// Publish time
        /// </summary>
        [SugarColumn(ColumnDescription = "Publish time")]
        public DateTime? Create_time { get; set; }
        /// <summary>
        /// Modify time
        /// </summary>
        [SugarColumn(IsOnlyIgnoreInsert = true, ColumnDescription = "Update time")]
        public DateTime? Update_time { get; set; }
        /// <summary>
        /// Article content
        /// </summary>
        [SugarColumn(ColumnDescription = "Article content", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string Content { get; set; }
        /// <summary>
        /// Author name
        /// </summary>
        [SugarColumn(ColumnDescription = "Author name", Length = 20, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string AuthorName { get; set; }
        /// <summary>
        /// Publisher user ID
        /// </summary>
        [SugarColumn(ColumnDescription = "Publisher user ID", ExtendedAttribute = ProteryConstant.NOTNULL)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        /// <summary>
        /// Article status: 1=Published, 2=Draft
        /// </summary>
        [SugarColumn(ColumnDescription = "Article status: 1=Published, 2=Draft", Length = 20)]
        public string Status { get; set; }
        /// <summary>
        /// Editor type: markdown, html
        /// </summary>
        [SugarColumn(ColumnDescription = "Editor type: markdown, html", ColumnName = "editorType", Length = 20, IsNullable = true)]
        public string EditorType { get; set; }
        /// <summary>
        /// Article tags e.g.: Net5, java
        /// </summary>
        [SugarColumn(ColumnDescription = "Article tags", Length = 20)]
        public string Tags { get; set; }
        /// <summary>
        /// Hit count
        /// </summary>
        [SugarColumn(ColumnDescription = "Hit count", DefaultValue = "0")]
        public int Hits { get; set; }
        [SugarColumn(ColumnDescription = "Category ID", ColumnName = "category_Id")]
        public int CategoryId { get; set; }
        /// <summary>
        /// Cover URL
        /// </summary>
        [SugarColumn(ColumnDescription = "Cover URL", Length = 5000)]
        public string CoverUrl { get; set; }
        /// <summary>
        /// Is public: 1=Public, 0=Private
        /// </summary>
        [SugarColumn(ColumnDescription = "Is public: 1=Public, 0=Private", DefaultValue = "0")]
        public int IsPublic { get; set; }
        /// <summary>
        /// Is top
        /// </summary>
        public int IsTop { get; set; }
        /// <summary>
        /// 0=Article, 1=Essay, 2=Dynamic
        /// </summary>
        [SugarColumn(ColumnDescription = "Content type: 0=Article, 1=Essay, 2=Dynamic", DefaultValue = "0")]
        public ArticleTypeEnum ArticleType { get; set; }
        /// <summary>
        /// Abstract
        /// </summary>
        public string AbstractText { get; set; }
        /// <summary>
        /// Category
        /// </summary>

        [Navigate(NavigateType.OneToOne, nameof(CategoryId), nameof(ArticleCategory.CategoryId))] //Custom relationship mapping
        public ArticleCategory CategoryNav { get; set; }
        /// <summary>
        /// Comment count
        /// </summary>
        public int CommentNum { get; set; }
        /// <summary>
        /// Praise count
        /// </summary>
        public int PraiseNum { get; set; }
        /// <summary>
        /// User IP
        /// </summary>
        public string UserIP { get; set; }
        /// <summary>
        /// Location
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Topic ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TopicId { get; set; }
        /// <summary>
        /// Comment switch: 0=Everyone can comment, 1=Fans only, 2=Self only
        /// </summary>
        public CommentSwitchEnum CommentSwitch { get; set; }
        /// <summary>
        /// Audit status: 0=Pending, 1=Approved, 2=Rejected
        /// </summary>
        public AuditStatusEnum AuditStatus { get; set; }
    }
}

