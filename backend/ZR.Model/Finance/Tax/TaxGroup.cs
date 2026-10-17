using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Tax
{
    /// <summary>
    /// Tax Group - مجموعة الضرائب
    /// Composite tax codes that apply multiple taxes in sequence
    /// Example: Provincial + Federal tax, or VAT + Local tax
    /// </summary>
    [SugarTable("tax_group")]
    [SugarIndex("UX_tax_group_company_code", nameof(CompanyID), OrderByType.Asc, nameof(Code), OrderByType.Asc, true)]
    public class TaxGroup : SysBase
    {
        /// <summary>
        /// Tax Group ID
        /// معرف مجموعة الضرائب
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Company ID for multi-tenant isolation
        /// معرف الشركة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "company_id")]
        public new long? CompanyID { get; set; }

        /// <summary>
        /// Group Code (e.g., "COMBO_TAX", "PST_GST")
        /// رمز المجموعة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 50, ColumnName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Group Name in English
        /// اسم المجموعة بالإنجليزية
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 200, ColumnName = "name_en")]
        public string NameEn { get; set; }

        /// <summary>
        /// Group Name in Arabic
        /// اسم المجموعة بالعربية
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
        /// Is Compound (tax on tax)
        /// هل هي مركبة (ضريبة على ضريبة)
        /// If true, later taxes are calculated on (amount + previous taxes)
        /// If false, all taxes are calculated on the base amount
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_compound")]
        public bool IsCompound { get; set; } = false;

        /// <summary>
        /// Is this tax group active?
        /// هل مجموعة الضرائب نشطة؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Sort order for display
        /// ترتيب العرض
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "sort_order")]
        public int SortOrder { get; set; } = 0;

        // Navigation properties
        [SugarColumn(IsIgnore = true)]
        public List<TaxGroupLine> Lines { get; set; } = new List<TaxGroupLine>();

        /// <summary>
        /// Calculate combined rate (simple sum for non-compound)
        /// </summary>
        public decimal GetCombinedRate()
        {
            if (Lines == null || Lines.Count == 0)
                return 0m;

            decimal total = 0m;
            foreach (var line in Lines)
            {
                if (line.TaxCode != null)
                    total += line.TaxCode.Rate;
            }
            return total;
        }
    }

    
}
