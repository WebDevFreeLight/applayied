 
using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// Product Specification
    /// </summary>
    [SugarTable("mms_product_spec", "Product Specification")]
    [Tenant("1")]
    
    [SqlDict(
        dictType: "mms_product_spec",
        dictName: "Product Spec",
        valueColumn: "Id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "Id ASC",
        Description = "Auto-generated from ProductSpec table"
    )]
    public class ProductSpec : SysBase
    {
        /// <summary>
        /// Id 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        /// <summary>
        /// Product ID 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }

        /// <summary>
        /// Specification Name 
        /// </summary>
        [SugarColumn(ColumnName = "SpecName")]
        public string Name { get; set; }

        /// <summary>
        /// Specification Values
        /// </summary>
        [SugarColumn(IsJson = true)]
        public List<string> SpecValues { get; set; }
    }
}

