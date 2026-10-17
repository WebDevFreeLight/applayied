using System;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Billing Configuration - إعدادات فوترة العقد
    /// Stores billing settings for a contract (1:1 relationship with ErpContract)
    /// </summary>
    [SugarTable("erp_contract_billing_config")]
    public class ErpContractBillingConfig : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Contract ID - معرف العقد
        /// Primary key and foreign key to ErpContract (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        #endregion

        #region Billing Settings - إعدادات الفوترة

        /// <summary>
        /// Billing Frequency - تكرار الفوترة
        /// How often invoices are generated
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "billing_frequency")]
        public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly;

        /// <summary>
        /// Billing Day - يوم الفوترة
        /// Day of the month/period when billing occurs (1-31)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "billing_day")]
        public int? BillingDay { get; set; } = 1;

        /// <summary>
        /// Is Billing Variable - هل الفوترة متغيرة
        /// True if billing amount varies (e.g., usage-based)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_billing_variable")]
        public bool IsBillingVariable { get; set; } = false;

        /// <summary>
        /// Billing In Advance - الفوترة مقدماً
        /// True if billing occurs at the start of the period (advance), false for arrears
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "billing_in_advance")]
        public bool BillingInAdvance { get; set; } = true;

        #endregion

        #region Billing Tracking - تتبع الفوترة

        /// <summary>
        /// Last Billing Date - تاريخ آخر فاتورة
        /// Date of the last generated invoice
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_billing_date")]
        public DateTime? LastBillingDate { get; set; }

        /// <summary>
        /// Next Billing Date - تاريخ الفاتورة القادمة
        /// Date of the next scheduled invoice
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "next_billing_date")]
        public DateTime? NextBillingDate { get; set; }

        #endregion

        #region Pro-Rata Settings - إعدادات التقسيط النسبي

        /// <summary>
        /// Prorate First Period - تقسيط الفترة الأولى
        /// Whether to prorate the first billing period
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "prorate_first_period")]
        public bool ProrateFirstPeriod { get; set; } = true;

        /// <summary>
        /// Prorate Last Period - تقسيط الفترة الأخيرة
        /// Whether to prorate the last billing period
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "prorate_last_period")]
        public bool ProrateLastPeriod { get; set; } = true;

        /// <summary>
        /// Prorate Calculation - طريقة حساب التقسيط
        /// How to calculate pro-rata amounts (Daily, Monthly, 30Day)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "prorate_calculation")]
        public string ProrateCalculation { get; set; } = "Daily";

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Contract - العقد
        /// Parent contract (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Billing Frequency Name - اسم تكرار الفوترة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string BillingFrequencyName { get; set; }

        #endregion
    }
}
