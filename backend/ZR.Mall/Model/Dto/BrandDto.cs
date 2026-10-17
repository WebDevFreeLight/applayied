
namespace ZR.Mall.Model.Dto
{
    /// <summary>
    /// 品牌表查询对象
    /// </summary>
    public class ShopBrandQueryDto : PagerInfo 
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// 品牌表输入输出对象
    /// </summary>
    public class BrandDto
    {
        [Required(ErrorMessage = "Id不能为空")]
        [ExcelColumn(Name = "Id")]
        [ExcelColumnName("Id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        [Required(ErrorMessage = "品牌名不能为空")]
        [ExcelColumn(Name = "品牌名")]
        [ExcelColumnName("品牌名")]
        public string Name { get; set; }

        [ExcelColumn(Name = "品牌logo")]
        [ExcelColumnName("品牌logo")]
        public string Logo { get; set; }

        [ExcelColumn(Name = "品牌介绍")]
        [ExcelColumnName("品牌介绍")]
        public string Description { get; set; }
    }
}

