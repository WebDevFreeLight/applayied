
namespace ZR.Mall.Model.Dto
{
    /// <summary>
    /// 商品规格查询对象
    /// </summary>
    public class ShoppingProductSpecQueryDto : PagerInfo
    {
    }

    /// <summary>
    /// 商品规格输入输出对象
    /// </summary>
    public class ProductSpecDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }
        public string Name { get; set; }
        [SugarColumn(IsJson = true)]
        [JsonProperty("values")]
        public List<string> SpecValues { get; set; }
    }
}

