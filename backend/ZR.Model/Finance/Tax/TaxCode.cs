using System;
using SqlSugar;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.GL;
using ZR.Model.System;

namespace ZR.Model.Finance.Tax
{
    /// <summary>
    /// Tax Code - رمز الضريبة
    /// Defines a tax type with its default rate and GL account mappings
    /// Multi-tenant: Scoped by CompanyID
    /// </summary>
    [SugarTable("tax_code")]
    [SugarIndex("UX_tax_code_company_code", nameof(CompanyID), OrderByType.Asc, nameof(Code), OrderByType.Asc, true)]
    public class TaxCode : SysBase
    {
        /// <summary>
        /// Tax Code ID - معرف رمز الضريبة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Company ID for multi-tenant isolation
        /// معرف الشركة للعزل متعدد المستأجرين
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "company_id")]
        public new long? CompanyID { get; set; }

        /// <summary>
        /// Tax Code (e.g., "VAT5", "VAT15", "EXEMPT")
        /// رمز الضريبة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 50, ColumnName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Tax Name in English
        /// اسم الضريبة بالإنجليزية
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 200, ColumnName = "name_en")]
        public string NameEn { get; set; }

        /// <summary>
        /// Tax Name in Arabic
        /// اسم الضريبة بالعربية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200, ColumnName = "name_ar")]
        public string NameAr { get; set; }

        /// <summary>
        /// Description in English
        /// الوصف بالإنجليزية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 1000, ColumnName = "description_en")]
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Description in Arabic
        /// الوصف بالعربية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 1000, ColumnName = "description_ar")]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Tax Type (Standard, ZeroRated, Exempt, ReverseCharge, Withholding)
        /// نوع الضريبة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "tax_type")]
        public TaxType TaxType { get; set; } = TaxType.Standard;

        /// <summary>
        /// Is Percentage (true) or Fixed Amount (false)
        /// هل هي نسبة مئوية أم مبلغ ثابت
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_percent")]
        public bool IsPercent { get; set; } = true;

        /// <summary>
        /// Default Tax Rate (e.g., 5.00 for 5%, or 0.15 for 15%)
        /// معدل الضريبة الافتراضي
        /// Note: This is the current default; use TaxRate table for effective-dated rates
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 6, ColumnName = "rate")]
        public decimal Rate { get; set; }

        /// <summary>
        /// Default GL Account for Tax Payable (Sales Tax / Output VAT)
        /// حساب الضريبة المستحقة الافتراضي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_payable_account_id")]
        public Guid? DefaultPayableAccountId { get; set; }

        /// <summary>
        /// Default GL Account for Tax Recoverable (Purchase Tax / Input VAT)
        /// حساب الضريبة القابلة للاسترداد الافتراضي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_recoverable_account_id")]
        public Guid? DefaultRecoverableAccountId { get; set; }

        /// <summary>
        /// GL Account for Reverse Charge (when applicable)
        /// حساب الاحتساب العكسي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reverse_charge_account_id")]
        public Guid? ReverseChargeAccountId { get; set; }

        /// <summary>
        /// Is this tax code active?
        /// هل رمز الضريبة نشط؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Sort order for display
        /// ترتيب العرض
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "sort_order")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Country/Region code for tax jurisdiction (ISO 3166-1)
        /// رمز الدولة/المنطقة للولاية الضريبية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 10, ColumnName = "country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Is this the default tax code for new items?
        /// هل هذا هو رمز الضريبة الافتراضي للعناصر الجديدة؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_default")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Is this a recoverable (input) tax?
        /// Used to determine if tax goes to recoverable (input VAT) or payable (output VAT) account
        /// هل هذه ضريبة قابلة للاسترداد؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_recoverable")]
        public bool IsRecoverable { get; set; } = false;

        // Navigation properties (not mapped)
        [SugarColumn(IsIgnore = true)]
        public GLAccount DefaultPayableAccount { get; set; }

        [SugarColumn(IsIgnore = true)]
        public GLAccount DefaultRecoverableAccount { get; set; }

        [SugarColumn(IsIgnore = true)]
        public GLAccount ReverseChargeAccount { get; set; }
    }
}
