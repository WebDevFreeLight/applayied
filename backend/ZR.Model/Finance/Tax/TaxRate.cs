using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Tax
{
    /// <summary>
    /// Tax Rate - معدل الضريبة
    /// Effective-dated tax rates for historical accuracy
    /// RULE: For any date, only ONE rate should be effective per tax code
    /// </summary>
    [SugarTable("tax_rate")]
    [SugarIndex("IX_tax_rate_code_eff", nameof(TaxCodeId), OrderByType.Asc, nameof(EffectiveFrom), OrderByType.Asc)]
    public class TaxRate : SysBase
    {
        /// <summary>
        /// Tax Rate ID
        /// معرف معدل الضريبة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Tax Code ID (FK to tax_code)
        /// معرف رمز الضريبة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "tax_code_id")]
        public Guid TaxCodeId { get; set; }

        /// <summary>
        /// Tax Rate Value
        /// قيمة معدل الضريبة
        /// For percentage: 5.00 = 5%, 15.00 = 15%
        /// For fixed: the fixed amount
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 6, ColumnName = "rate")]
        public decimal Rate { get; set; }

        /// <summary>
        /// Effective From Date (inclusive)
        /// تاريخ البداية (شامل)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective To Date (inclusive, null = no end date)
        /// تاريخ النهاية (شامل، فارغ = بدون تاريخ انتهاء)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "effective_to")]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Reason for rate change
        /// سبب تغيير المعدل
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "change_reason")]
        public string ChangeReason { get; set; }

        /// <summary>
        /// Reference to legal/regulatory document
        /// مرجع الوثيقة القانونية/التنظيمية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200, ColumnName = "legal_reference")]
        public string LegalReference { get; set; }

        /// <summary>
        /// Is this rate approved/verified?
        /// هل هذا المعدل معتمد/موثق؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_approved")]
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// Approved by user
        /// معتمد من قبل المستخدم
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 64, ColumnName = "approved_by")]
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Approved at timestamp
        /// تاريخ الاعتماد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_at")]
        public DateTime? ApprovedAt { get; set; }

        // Navigation property
        [SugarColumn(IsIgnore = true)]
        public TaxCode TaxCode { get; set; }

        /// <summary>
        /// Check if this rate is effective for a given date
        /// </summary>
        public bool IsEffectiveForDate(DateTime date)
        {
            var dateOnly = date.Date;
            return dateOnly >= EffectiveFrom.Date &&
                   (!EffectiveTo.HasValue || dateOnly <= EffectiveTo.Value.Date);
        }
    }
}
