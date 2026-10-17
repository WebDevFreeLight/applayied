using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Seasonal Rule - قاعدة موسمية العقد
    /// Extension table for pricing rules of type Seasonal (1:1 with ErpContractPricingRule)
    /// </summary>
    [SugarTable("erp_contract_seasonal_rule")]
    public class ErpContractSeasonalRule : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// Primary key and foreign key to ErpContractPricingRule (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "pricing_rule_id")]
        public Guid PricingRuleId { get; set; }

        #endregion

        #region Season Identification - تعريف الموسم

        /// <summary>
        /// Season Name - اسم الموسم
        /// Name of the season (e.g., "Summer", "Ramadan", "Peak Season")
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, ColumnName = "season_name")]
        public string SeasonName { get; set; }

        /// <summary>
        /// Season Name (Arabic) - اسم الموسم بالعربية
        /// Arabic name for bilingual support
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "season_name_ar")]
        public string SeasonNameAr { get; set; }

        #endregion

        #region Recurrence - التكرار

        /// <summary>
        /// Recurrence Pattern - نمط التكرار
        /// How the season recurs (Yearly, Custom, OneTime)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "recurrence_pattern")]
        public string RecurrencePattern { get; set; } = "Yearly";

        #endregion

        #region Period Definition - تعريف الفترة

        /// <summary>
        /// Month Start - شهر البداية
        /// Start month for yearly recurring (1-12)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "month_start")]
        public int? MonthStart { get; set; }

        /// <summary>
        /// Day Start - يوم البداية
        /// Start day for yearly recurring (1-31)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "day_start")]
        public int? DayStart { get; set; }

        /// <summary>
        /// Month End - شهر النهاية
        /// End month for yearly recurring (1-12)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "month_end")]
        public int? MonthEnd { get; set; }

        /// <summary>
        /// Day End - يوم النهاية
        /// End day for yearly recurring (1-31)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "day_end")]
        public int? DayEnd { get; set; }

        #endregion

        #region Specific Year - سنة محددة

        /// <summary>
        /// Applies To Year - السنة المطبقة
        /// Specific year if not recurring (null for recurring seasons)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "applies_to_year")]
        public int? AppliesToYear { get; set; }

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Pricing Rule - قاعدة التسعير
        /// Parent pricing rule (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(PricingRuleId))]
        public ErpContractPricingRule PricingRule { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// Season Period Display - عرض فترة الموسم
        /// Formatted display of the season period
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string SeasonPeriodDisplay
        {
            get
            {
                if (MonthStart.HasValue && DayStart.HasValue && MonthEnd.HasValue && DayEnd.HasValue)
                {
                    return $"{DayStart}/{MonthStart} - {DayEnd}/{MonthEnd}";
                }
                return null;
            }
        }

        #endregion
    }
}
