 
using Infrastructure.Converter;
using ZR.Mall.Model.Dto;
using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// Product Inventory
    /// </summary>
    [SugarTable("mms_skus", "Product Inventory SKU")]
    [Tenant("1")]
    [SqlDict(
        dictType: "mms_sku",
        dictName: "Skus",
        valueColumn: "SkuId",
        labelColumns: "imageurl",
        RemarkColumn = "remark",
        OrderBy = "sortid ASC",
        Description = "Auto-generated from Skus table"
    )]
    public class Skus : SysBase
    {
        /// <summary>
        /// SkuId 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SkuId { get; set; }

        /// <summary>
        /// Product ID 
        /// </summary>
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }

        /// <summary>
        /// Selling Price 
        /// </summary>
        [SugarColumn(ExtendedAttribute = ProteryConstant.NOTNULL)]
        public decimal Price { get; set; }

        /// <summary>
        /// Stock 
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int Stock { get; set; }

        /// <summary>
        /// Sales Volume
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int SalesVolume { get; set; }

        /// <summary>
        /// Thumbnail Image 
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Specification Value IDs
        /// </summary>
        public string SpecIds { get; set; }
        /// <summary>
        /// Snapshot Storage, Specifications like "Color:Red;Size:XL"
        /// </summary>
        public string SpecCombination { get; set; }
        /// <summary>
        /// Add Time
        /// </summary>
        [SugarColumn(InsertServerTime = true, IsOnlyIgnoreUpdate = true)]
        public DateTime AddTime { get; set; }
 
        /// <summary>
        /// Is Deleted
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int IsDelete { get; set; }

        /// <summary>
        /// Sort ID
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int SortId { get; set; }

        /// <summary>
        /// Weight (Kilograms (Kg))
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public decimal Weight { get; set; }

        /// <summary>
        /// Version Number (Prevent Concurrent Updates)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int Version { get; set; }

        /// <summary>
        /// Specifications
        /// </summary>
        [SugarColumn(IsJson = true)]
        public List<ProductSpecGroup> Specs { get; set; }
    }
}

