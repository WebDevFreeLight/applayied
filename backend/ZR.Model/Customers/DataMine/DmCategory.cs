using System;
using Infrastructure.Converter;

namespace ZR.Model.Customers.DataMine
{
    [SugarTable("dm_category")]
    public class DmCategory : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "category_id", IsPrimaryKey = true, IsIdentity = false)]
        public long CategoryId { get; set; }

        [SugarColumn(ColumnName = "name", Length = 200)]
        public string Name { get; set; }

        [SugarColumn(ColumnName = "name_ar", Length = 200, IsNullable = true)]
        public string NameAr { get; set; }

        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        [SugarColumn(ColumnName = "sort_order")]
        public int SortOrder { get; set; }
    }
}
