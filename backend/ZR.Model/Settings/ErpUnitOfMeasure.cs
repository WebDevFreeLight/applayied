using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Settings
{
    /// <summary>
    /// Unit of Measure - الوحدات
    /// </summary>
    [SugarTable("erp_units_of_measure")]
    [SqlDict(
        dictType: "erp_unit",
        dictName: "Unit of Measure - الوحدة",
        valueColumn: "unitid",
        labelColumns: "unitname",
        RemarkColumn = "unitnamear",
        OrderBy = "unitcode ASC",
        Description = "Auto-generated from ErpUnitOfMeasure table"
    )]
    public class ErpUnitOfMeasure : SysBase
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UnitId { get; set; }

        /// <summary>
        /// Unit Code - كود الوحدة
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Unit Name - اسم الوحدة
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Unit Name Arabic - اسم الوحدة بالعربي
        /// </summary>
        public string UnitNameAr { get; set; }

        /// <summary>
        /// Unit Symbol - رمز الوحدة
        /// </summary>
        public string UnitSymbol { get; set; }

        /// <summary>
        /// Unit Type - نوع الوحدة (quantity/weight/volume/length/time)
        /// </summary>
        public string UnitType { get; set; }

        /// <summary>
        /// Base Unit ID - معرف الوحدة الأساسية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BaseUnitId { get; set; }

        /// <summary>
        /// Conversion Factor - معامل التحويل
        /// </summary>
        public decimal ConversionFactor { get; set; } = 1;

        /// <summary>
        /// Is Base Unit - وحدة أساسية
        /// </summary>
        public bool IsBaseUnit { get; set; } = true;

        /// <summary>
        /// Decimal Places - عدد الخانات العشرية
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

