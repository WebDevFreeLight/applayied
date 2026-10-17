using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// Payment Terms - شروط الدفع
    /// Defines payment terms for customers and vendors (e.g., Net 30, 2/10 Net 30)
    /// يحدد شروط الدفع للعملاء والموردين
    /// </summary>
    [SugarTable("arap_payment_terms")]
    [SugarIndex("uk_payment_terms_code_company", nameof(Code), OrderByType.Asc, nameof(CompanyID), OrderByType.Asc, true)]
    [SugarIndex("ix_payment_terms_code", nameof(Code), OrderByType.Asc)]
    [SugarIndex("ix_payment_terms_active", nameof(IsActive), OrderByType.Asc)]
    public class PaymentTerms : SysBase
    {
        /// <summary>
        /// Payment Terms ID - معرف شروط الدفع
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Code - رمز شروط الدفع
        /// Unique identifier code (e.g., NET30, 2-10-NET30)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Name (English) - الاسم بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false, ColumnName = "name_en")]
        public string NameEn { get; set; }

        /// <summary>
        /// Name (Arabic) - الاسم بالعربي
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "name_ar")]
        public string NameAr { get; set; }

        /// <summary>
        /// Net Days - عدد أيام الاستحقاق
        /// Number of days until payment is due
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "net_days")]
        public int NetDays { get; set; } = 30;

        /// <summary>
        /// Discount Days - أيام الخصم
        /// Number of days to receive early payment discount
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "discount_days")]
        public int? DiscountDays { get; set; }

        /// <summary>
        /// Discount Percent - نسبة الخصم
        /// Percentage discount for early payment
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "discount_percent")]
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_en")]
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_ar")]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Sort Order - ترتيب العرض
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "sort_order")]
        public int SortOrder { get; set; } = 0;

        #region Helper Methods

        /// <summary>
        /// Calculate due date from invoice date
        /// حساب تاريخ الاستحقاق من تاريخ الفاتورة
        /// </summary>
        /// <param name="invoiceDate">Invoice date</param>
        /// <returns>Due date</returns>
        public DateTime CalculateDueDate(DateTime invoiceDate)
        {
            return invoiceDate.AddDays(NetDays);
        }

        /// <summary>
        /// Calculate discount cutoff date from invoice date
        /// حساب آخر تاريخ للخصم من تاريخ الفاتورة
        /// </summary>
        /// <param name="invoiceDate">Invoice date</param>
        /// <returns>Discount cutoff date, or null if no discount applies</returns>
        public DateTime? CalculateDiscountCutoffDate(DateTime invoiceDate)
        {
            if (!DiscountDays.HasValue || !DiscountPercent.HasValue || DiscountPercent.Value <= 0)
                return null;

            return invoiceDate.AddDays(DiscountDays.Value);
        }

        /// <summary>
        /// Calculate discount amount if eligible
        /// حساب مبلغ الخصم إذا كان مؤهلاً
        /// </summary>
        /// <param name="amount">Invoice amount</param>
        /// <param name="paymentDate">Payment date</param>
        /// <param name="invoiceDate">Invoice date</param>
        /// <returns>Discount amount, or 0 if not eligible</returns>
        public decimal CalculateDiscount(decimal amount, DateTime paymentDate, DateTime invoiceDate)
        {
            var discountCutoff = CalculateDiscountCutoffDate(invoiceDate);
            if (!discountCutoff.HasValue || paymentDate > discountCutoff.Value)
                return 0;

            return Math.Round(amount * (DiscountPercent.Value / 100m), 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Get formatted display name based on language
        /// الحصول على الاسم المنسق حسب اللغة
        /// </summary>
        /// <param name="preferArabic">True for Arabic, False for English</param>
        /// <returns>Display name</returns>
        public string GetDisplayName(bool preferArabic = false)
        {
            if (preferArabic && !string.IsNullOrEmpty(NameAr))
                return NameAr;
            return NameEn;
        }

        #endregion
    }
}
