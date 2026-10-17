using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// Product Brand
    /// </summary>
    [SugarTable("mms_brand", "Brand Table")]
    [Tenant("1")]
    [SqlDict(
        dictType: "mms_brand",
        dictName: "Brand",
        valueColumn: "id",
        labelColumns: "name",
        ValueColumn = "id",
        RemarkColumn = "description",
        OrderBy = "id ASC",
        Description = "Auto-generated from Brand table"
    )]
    public class Brand : SysBase
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        /// <summary>
        /// Brand Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Brand Logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// Brand Description
        /// </summary>
        public string Description { get; set; }
    }
}

