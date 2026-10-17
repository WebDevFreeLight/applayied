using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Assets
{
    /// <summary>
    /// Asset Depreciation - إهلاك الأصل
    /// </summary>
    [SugarTable("asset_depreciation")]
    public class AssetDepreciation : SysBase
    {
        /// <summary>
        /// Depreciation ID - معرف الإهلاك
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Asset ID - معرف الأصل
        /// </summary>
        [SugarColumn(ColumnName = "asset_id")]
        public Guid AssetId { get; set; }

        /// <summary>
        /// Depreciation Date - تاريخ الإهلاك
        /// </summary>
        [SugarColumn(ColumnName = "depreciation_date")]
        public DateTime DepreciationDate { get; set; }

        /// <summary>
        /// Depreciation Amount - مبلغ الإهلاك
        /// </summary>
        [SugarColumn(ColumnName = "depreciation_amount", DecimalDigits = 4)]
        public decimal DepreciationAmount { get; set; }

        /// <summary>
        /// Accumulated Value After - القيمة المتراكمة بعد الإهلاك
        /// </summary>
        [SugarColumn(ColumnName = "accumulated_value_after", DecimalDigits = 4)]
        public decimal AccumulatedValueAfter { get; set; }

        /// <summary>
        /// Book Value After - القيمة الدفترية بعد الإهلاك
        /// </summary>
        [SugarColumn(ColumnName = "book_value_after", DecimalDigits = 4)]
        public decimal BookValueAfter { get; set; }

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Fiscal Period - الفترة المالية
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_period")]
        public int FiscalPeriod { get; set; }

        /// <summary>
        /// Navigation - Fixed Asset
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public FixedAsset Asset { get; set; }
    }
}

