namespace ZR.Mall.Model.Dto
{
    /// <summary>
    /// 
    /// </summary>
    public class CategoryDto
    {
        [Required(ErrorMessage = "目录id不能为空")]
        [ExcelColumn(Name = "目录ID")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "目录名不能为空")]
        [ExcelColumn(Name = "名称")]
        public string Name { get; set; }
        [ExcelColumn(Name = "图标")]
        public string Icon { get; set; }
        [ExcelColumn(Name = "排序")]
        public int OrderNum { get; set; }
        [ExcelColumn(Name = "创建时间", Width = 90)]
        public DateTime? Create_time { get; set; }
        [ExcelColumn(Name = "上级目录ID")]
        public int? ParentId { get; set; }
        [ExcelColumn(Name = "是否删除")]
        public int IsDelete { get; set; }
        [ExcelColumn(Name = "状态")]
        public int ShowStatus { get; set; }
        [ExcelColumn(Ignore = true)]
        public List<CategoryDto> Children { get; set; }
    }

    /// <summary>
    /// 目录查询对象
    /// </summary>
    public class ShoppingCategoryQueryDto : PagerInfo
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public int? ShowStatus { get; set; }
        public int? IsDelete { get; set; }
    }
}

