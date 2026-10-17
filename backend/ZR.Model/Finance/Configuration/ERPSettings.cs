using System;
using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// ERP Settings - إعدادات النظام المالي
    /// Only one settings record per company
    /// </summary>
    [SugarTable("finance_erp_settings")]
    [SugarIndex("UX_erp_settings_company", nameof(CompanyID), OrderByType.Asc, true)] // Unique index on CompanyID
    public class ERPSettings : SysBase
    {
        /// <summary>
        /// Settings ID - معرف الإعدادات
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        // Note: CompanyID and BranchID are inherited from SysBase - do not duplicate

        /// <summary>
        /// Default Currency ID (FK to Currency) - العملة الافتراضية
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? DefaultCurrencyId { get; set; }

        /// <summary>
        /// Enable Multi-Currency - تفعيل العملات المتعددة
        /// </summary>
        public bool EnableMultiCurrency { get; set; } = false;

        /// <summary>
        /// Default Approval Limit - حد الموافقة الافتراضي
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal DefaultApprovalLimit { get; set; } = 0;

        /// <summary>
        /// Auto Post to GL - الترحيل التلقائي للقيد العام
        /// </summary>
        public bool AutoPostToGL { get; set; } = false;

        /// <summary>
        /// Require Approval for Vouchers - يتطلب الموافقة للسندات
        /// </summary>
        public bool RequireApprovalForVouchers { get; set; } = false;

        /// <summary>
        /// Allow Backdating - السماح بالتاريخ الرجعي
        /// </summary>
        public bool AllowBackdating { get; set; } = true;

        /// <summary>
        /// Max Backdating Days - الحد الأقصى لأيام التاريخ الرجعي
        /// </summary>
        public int MaxBackdatingDays { get; set; } = 30;

        /// <summary>
        /// Enable Budget Control - تفعيل مراقبة الميزانية
        /// </summary>
        public bool EnableBudgetControl { get; set; } = false;

        /// <summary>
        /// Fiscal Year Start Month - شهر بداية السنة المالية
        /// </summary>
        public int FiscalYearStartMonth { get; set; } = 1; // January

        /// <summary>
        /// Number of Fiscal Periods - عدد الفترات المالية
        /// </summary>
        public int NumberOfFiscalPeriods { get; set; } = 12; // Monthly

        /// <summary>
        /// Default Tax Rule ID - قاعدة الضريبة الافتراضية
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? DefaultTaxRuleId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string Notes { get; set; }
    }
}


