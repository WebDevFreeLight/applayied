using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// Vendor - المورد
    /// </summary>
    [SugarTable("arap_vendor")]
    public class Vendor : SysBase
    {
        /// <summary>
        /// Vendor ID - معرف المورد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Vendor Code - رمز المورد
        /// </summary>
        [SugarColumn(Length = 50)]
        public string VendorCode { get; set; }

        /// <summary>
        /// Name (Arabic) - الاسم بالعربي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameAr { get; set; }

        /// <summary>
        /// Name (English) - الاسم بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameEn { get; set; }

        /// <summary>
        /// Contact Person - الشخص المسؤول
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Email { get; set; }

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Phone { get; set; }

        /// <summary>
        /// GL Account ID (AP Control Account) - معرف الحساب المحاسبي (حساب الموردين)
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? GLAccountId { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        #region New Fields - ARAP Critical Fixes

        /// <summary>
        /// Address (Arabic) - العنوان بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "address_ar")]
        public string AddressAr { get; set; }

        /// <summary>
        /// Address (English) - العنوان بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "address_en")]
        public string AddressEn { get; set; }

        /// <summary>
        /// Tax Number - الرقم الضريبي
        /// VAT registration number for ZATCA compliance
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "tax_number")]
        public string TaxNumber { get; set; }

        /// <summary>
        /// Default Currency ID - معرف العملة الافتراضية
        /// FK to currencies table
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_currency_id")]
        public Guid? DefaultCurrencyId { get; set; }

        /// <summary>
        /// Default Tax Group ID - معرف مجموعة الضريبة الافتراضية
        /// FK to tax_group table
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_tax_group_id")]
        public Guid? DefaultTaxGroupId { get; set; }

        /// <summary>
        /// Payment Terms ID - معرف شروط الدفع
        /// FK to arap_payment_terms
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "payment_terms_id")]
        public int? PaymentTermsId { get; set; }

        /// <summary>
        /// Credit Days - أيام الائتمان
        /// Number of days for credit payment (overrides payment terms if set)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "credit_days")]
        public int? CreditDays { get; set; } = 30;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Payment Terms (Navigation Property) - شروط الدفع
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public PaymentTerms PaymentTerms { get; set; }

        #endregion
    }
}


