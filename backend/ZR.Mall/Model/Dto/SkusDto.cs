
namespace ZR.Mall.Model.Dto
{
    /// <summary>
    /// 商品库存查询对象
    /// </summary>
    public class ShoppingSkusQueryDto : PagerInfo
    {
    }

    /// <summary>
    /// 商品库存输入输出对象
    /// </summary>
    public class SkusDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SkuId { get; set; }

        [Required(ErrorMessage = "商品ID不能为空")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }

        [Required(ErrorMessage = "售卖价格不能为空")]
        public decimal Price { get; set; }
        public int SalesVolume { get; set; }
        public int Stock { get; set; }
        public int SortId { get; set; }
        /// <summary>
        /// 重量（千克 (Kg)）
        /// </summary>
        public decimal Weight { get; set; }
        public string ImageUrl { get; set; }
        ///// <summary>
        ///// 暂时没用到
        ///// </summary>
        //public string SpecValue { get; set; }
        public string SpecCombination { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        [SugarColumn(IsJson = true)]
        public List<ProductSpecGroup> Specs { get; set; }
    }
}

