using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// Exchange Rate - سعر الصرف
    /// Daily exchange rates for foreign currencies
    /// </summary>
    [SugarTable("exchange_rates")]
    public class ExchangeRate : SysBase
    {
        /// <summary>
        /// Exchange Rate ID - معرف سعر الصرف
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Currency ID (Foreign Key to currencies)
        /// معرف العملة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "currency_id")]
        public Guid CurrencyId { get; set; }

        /// <summary>
        /// Rate Date - تاريخ السعر
        /// RULE: Daily rates required
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "rate_date")]
        public DateTime RateDate { get; set; }

        /// <summary>
        /// Exchange Rate (how many base currency units = 1 foreign currency unit)
        /// سعر الصرف
        /// Example: If SAR is base and USD rate is 3.75, then 1 USD = 3.75 SAR
        /// RULE: Must be > 0
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 10, ColumnName = "rate")]
        public decimal Rate { get; set; }

        /// <summary>
        /// Inverse Rate (auto-calculated: 1 / Rate)
        /// السعر العكسي
        /// Example: If Rate = 3.75, InverseRate = 0.266667
        /// </summary>
        [SugarColumn(IsNullable = true, DecimalDigits = 10, ColumnName = "inverse_rate")]
        public decimal? InverseRate { get; set; }

        /// <summary>
        /// Is this rate manually overridden?
        /// هل السعر معدّل يدوياً؟
        /// </summary>
        [SugarColumn(ColumnName = "is_override")]
        public bool IsOverride { get; set; }

        /// <summary>
        /// Source of the rate (e.g., "Central Bank", "Manual", "API")
        /// مصدر السعر
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Notes / Comments
        /// ملاحظات
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Is this rate active/valid?
        /// هل السعر نشط؟
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Approved By User ID
        /// وافق عليه المستخدم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_by")]
        public Guid? ApprovedBy { get; set; }

        /// <summary>
        /// Approved At
        /// تاريخ الموافقة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_at")]
        public DateTime? ApprovedAt { get; set; }

        // Navigation property (not mapped to database)
        [SugarColumn(IsIgnore = true)]
        public Currency Currency { get; set; }
    }
}
