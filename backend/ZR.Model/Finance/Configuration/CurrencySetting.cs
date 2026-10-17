using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// Currency System Settings - إعدادات نظام العملات
    /// Global configuration for multi-currency module
    /// </summary>
    [SugarTable("currency_settings")]
    public class CurrencySetting : SysBase
    {
        /// <summary>
        /// Setting ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Base Currency Code (ISO 4217)
        /// رمز العملة الأساسية
        /// Example: "SAR", "USD"
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = false, ColumnName = "base_currency_code")]
        public string BaseCurrencyCode { get; set; }

        /// <summary>
        /// Default Display Currency Code
        /// العملة الافتراضية للعرض
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = true, ColumnName = "default_display_currency")]
        public string DefaultDisplayCurrency { get; set; }

        /// <summary>
        /// Allow Manual Exchange Rate Entry?
        /// السماح بإدخال سعر الصرف يدوياً؟
        /// </summary>
        [SugarColumn(ColumnName = "allow_manual_exchange_rate")]
        public bool AllowManualExchangeRate { get; set; } = false;

        /// <summary>
        /// Require Exchange Rate Approval?
        /// طلب الموافقة على سعر الصرف؟
        /// </summary>
        [SugarColumn(ColumnName = "require_rate_approval")]
        public bool RequireRateApproval { get; set; } = false;

        /// <summary>
        /// Maximum Rate Age (days)
        /// أقصى عمر للسعر بالأيام
        /// Block posting if using rate older than this
        /// </summary>
        [SugarColumn(ColumnName = "max_rate_age_days")]
        public int MaxRateAgeDays { get; set; } = 30;

        /// <summary>
        /// Auto-create Rounding Adjustment Line?
        /// إنشاء سطر تسوية التقريب تلقائياً؟
        /// </summary>
        [SugarColumn(ColumnName = "auto_create_rounding_adjustment")]
        public bool AutoCreateRoundingAdjustment { get; set; } = true;

        /// <summary>
        /// Rounding Adjustment Account ID
        /// حساب تسوية التقريب
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "rounding_adjustment_account_id")]
        public Guid? RoundingAdjustmentAccountId { get; set; }

        /// <summary>
        /// Maximum Rounding Difference Allowed
        /// أقصى فرق تقريب مسموح
        /// Default: 0.05 (5 cents/halalas)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, ColumnName = "max_rounding_difference")]
        public decimal MaxRoundingDifference { get; set; } = 0.05m;

        /// <summary>
        /// Allow Multi-Currency Reporting?
        /// السماح بتقارير متعددة العملات؟
        /// </summary>
        [SugarColumn(ColumnName = "allow_multicurrency_reporting")]
        public bool AllowMulticurrencyReporting { get; set; } = true;

        /// <summary>
        /// Default Exchange Rate Source
        /// مصدر سعر الصرف الافتراضي
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "default_rate_source")]
        public string DefaultRateSource { get; set; }

        /// <summary>
        /// Tenant/Company ID
        /// معرف الشركة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "tenant_id")]
        public long? TenantId { get; set; }
    }
}
