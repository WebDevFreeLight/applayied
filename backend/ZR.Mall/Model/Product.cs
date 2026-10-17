using Infrastructure.Converter;
using ZR.Mall.Enum;
using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// å•†å“ç®¡ç†
    /// </summary>
    [SugarTable("mms_product", "å•†å“ç®¡ç†")]
    [Tenant("1")]
    
    [SqlDict(
        dictType: "mms_product",
        dictName: "Product",
        valueColumn: "ProductId",
        labelColumns: "productname",
        RemarkColumn = "remark",
        OrderBy = "sortid ASC",
        Description = "Auto-generated from Product table"
    )]
    public class Product : SysBase
    {
        /// <summary>
        /// å•†å“ID 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }

        /// <summary>
        /// å•†å“å 
        /// </summary>
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string ProductName { get; set; }

        /// <summary>
        /// å•†å“ç¼–ç 
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// ä»‹ç» 
        /// </summary>
        public string Introduce { get; set; }

        /// <summary>
        /// å“ç‰ŒId
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BrandId { get; set; }

        /// <summary>
        /// å•†å“åˆ†ç±» 
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// skuæœ€ä½Žä»·æ ¼(è‡ªåŠ¨æ ¹æ®skuè®¡ç®—)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// æœ€é«˜ä»·æ ¼(ç”¨äºŽå‰ç«¯æœç´¢ä»·æ ¼åŒºé—´)è‡ªåŠ¨æ ¹æ®skuè®¡ç®—
        /// </summary>
        public decimal MaxPrice { get; set; }
        /// <summary>
        /// å•†å“åŽŸä»¶ï¼ˆåˆ’çº¿ä»·ï¼‰
        /// </summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// å•ä½ ï¼ˆå¦‚ï¼šä»¶ï¼Œç“¶ï¼Œæ–¤ï¼‰
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// è½®æ’­å›¾ç‰‡ 
        /// </summary>
        [SugarColumn(Length = 2000)]
        public string ImageUrls { get; set; }

        /// <summary>
        /// å•†å“ä¸»å›¾
        /// </summary>
        public string MainImage { get; set; }

        /// <summary>
        /// è§†é¢‘ä»‹ç»
        /// </summary>
        public string VideoUrl { get; set; }

        /// <summary>
        /// é”€é‡ç»Ÿè®¡ï¼ˆå¯åŠ ç¼“å­˜ï¼‰
        /// </summary>
        public int TotalSalesVolume { get; set; }

        /// <summary>
        /// æŽ’åºID (è¶Šå¤§æŽ’åºè¶Šé å‰)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int SortId { get; set; } = 0;

        /// <summary>
        /// æ˜¾ç¤ºçŠ¶æ€ 1.åœ¨å”® 0.ä¸‹æž¶
        /// </summary>
        [SugarColumn(DefaultValue = "0", ExtendedAttribute = ProteryConstant.NOTNULL)]
        public SaleStatus SaleStatus { get; set; }

        /// <summary>
        /// æ·»åŠ æ—¶é—´ 
        /// </summary>
        [SugarColumn(InsertServerTime = true)]
        public DateTime? AddTime { get; set; }

        /// <summary>
        /// è¯¦æƒ…é¡µå¯Œæ–‡æœ¬
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string DetailsHtml { get; set; }

        /// <summary>
        /// æ˜¯å¦åˆ é™¤
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int IsDelete { get; set; }

        /// <summary>
        /// è§„æ ¼ç®€ä»‹(åªè¯»ï¼Œå­˜æ¡£ä½¿ç”¨)
        /// </summary>
        public string SpecSummary { get; set; }

        /// <summary>
        /// è§„æ ¼ç±»åž‹
        /// </summary>
        public SpecType SpecType { get; set; } = SpecType.Multiple;

        /// <summary>
        /// é™è´­
        /// </summary>
        [SugarColumn(IsJson = true)]
        public PurchaseLimit PurchaseLimit { get; set; } = new PurchaseLimit();

        /// <summary>
        /// sku
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ProductId), nameof(Model.Skus.ProductId))] //è‡ªå®šä¹‰å…³ç³»æ˜ å°„
        public List<Skus> Skus { get; set; }

        /// <summary>
        /// åˆ†ç±»
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(CategoryId), nameof(Model.Category.CategoryId))] //è‡ªå®šä¹‰å…³ç³»æ˜ å°„
        public Category Category { get; set; }

        /// <summary>
        /// å“ç‰Œ
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(BrandId), nameof(Model.Brand.Id))] //è‡ªå®šä¹‰å…³ç³»æ˜ å°„
        public Brand Brand { get; set; }
    }

    public class PurchaseLimit
    {
        public bool Limit { get; set; }
        /// <summary>
        /// æ€»é™è´­ä»¶æ•°
        /// </summary>
        public int TotalLimit { get; set; }
        /// <summary>
        /// æ¯å•é™è´­
        /// </summary>
        public int SingleLimit { get; set; }

    }
}

