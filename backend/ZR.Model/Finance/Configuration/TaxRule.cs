using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// Tax Rule - قاعدة الضريبة
    /// </summary>
    [SugarTable("finance_tax_rule")]
    public class TaxRule : SysBase
    {
        /// <summary>
        /// Tax Rule ID - معرف قاعدة الضريبة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Tax Name - اسم الضريبة
        /// </summary>
        [SugarColumn(Length = 200)]
        public string TaxName { get; set; }

        /// <summary>
        /// Tax Type - نوع الضريبة
        /// </summary>
        public TaxType TaxType { get; set; }

        /// <summary>
        /// Tax Rate - نسبة الضريبة
        /// </summary>
        [SugarColumn(DecimalDigits = 4)]
        public decimal Rate { get; set; }

        /// <summary>
        /// Applicable Account ID (FK to GL Account) - حساب الضريبة المطبق
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? ApplicableAccountId { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public bool Status { get; set; } = true; // Active/Inactive

        /// <summary>
        /// Effective From - ساري من تاريخ
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// Effective To - ساري حتى تاريخ
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string DescriptionEn { get; set; }
    }
}


