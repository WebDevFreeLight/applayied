using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Cost category (e.g. Labour, Hardware, Software, Services). Used for spend-by-category analytics.
    /// </summary>
    [SugarTable("pmo_cost_category", "PMO Cost Category")]
    [SugarIndex("uk_pmo_cost_category_code", nameof(Code), OrderByType.Asc, true)]
    public class PmoCostCategory : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "category_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CategoryId { get; set; }

        [SugarColumn(ColumnName = "code", Length = 32)]
        public string Code { get; set; }

        [SugarColumn(ColumnName = "name", Length = 100)]
        public string Name { get; set; }
    }
}
