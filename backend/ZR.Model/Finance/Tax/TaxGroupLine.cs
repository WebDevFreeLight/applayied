using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Finance.Tax
{
    /// <summary>
    /// Tax Group Line - سطر مجموعة الضرائب
    /// Individual tax codes within a tax group
    /// </summary>
    [SugarTable("tax_group_line")]
    [SugarIndex("IX_tax_group_line_group", nameof(TaxGroupId), OrderByType.Asc)]
    public class TaxGroupLine : SysBase
    {
        /// <summary>
        /// Line ID
        /// معرف السطر
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Tax Group ID (FK)
        /// معرف مجموعة الضرائب
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "tax_group_id")]
        public Guid TaxGroupId { get; set; }

        /// <summary>
        /// Tax Code ID (FK)
        /// معرف رمز الضريبة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "tax_code_id")]
        public Guid TaxCodeId { get; set; }

        /// <summary>
        /// Sequence/Order (taxes apply in this order)
        /// الترتيب (الضرائب تُطبق بهذا الترتيب)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "sequence")]
        public int Sequence { get; set; }

        /// <summary>
        /// Override rate (if different from tax code default)
        /// معدل تجاوز (إذا كان مختلفًا عن الافتراضي)
        /// </summary>
        [SugarColumn(IsNullable = true, DecimalDigits = 6, ColumnName = "override_rate")]
        public decimal? OverrideRate { get; set; }

        /// <summary>
        /// Is this line active within the group?
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [SugarColumn(IsIgnore = true)]
        public TaxGroup TaxGroup { get; set; }

        [SugarColumn(IsIgnore = true)]
        public TaxCode TaxCode { get; set; }

        /// <summary>
        /// Get effective rate (override or default)
        /// </summary>
        public decimal GetEffectiveRate()
        {
            return OverrideRate ?? TaxCode?.Rate ?? 0m;
        }
    }
}
