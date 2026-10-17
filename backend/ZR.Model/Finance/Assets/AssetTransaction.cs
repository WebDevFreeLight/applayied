using System;
using SqlSugar;
using ZR.Model.Finance.Assets.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Assets
{
    /// <summary>
    /// Asset Transaction - معاملة الأصل
    /// </summary>
    [SugarTable("asset_transaction")]
    public class AssetTransaction : SysBase
    {
        /// <summary>
        /// Transaction ID - معرف المعاملة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Asset ID - معرف الأصل
        /// </summary>
        [SugarColumn(ColumnName = "asset_id")]
        public Guid AssetId { get; set; }

        /// <summary>
        /// Transaction Type - نوع المعاملة
        /// </summary>
        [SugarColumn(ColumnName = "transaction_type")]
        public AssetTransactionType TransactionType { get; set; }

        /// <summary>
        /// Transaction Date - تاريخ المعاملة
        /// </summary>
        [SugarColumn(ColumnName = "transaction_date")]
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        [SugarColumn(ColumnName = "amount", DecimalDigits = 4)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Previous Book Value - القيمة الدفترية السابقة
        /// </summary>
        [SugarColumn(ColumnName = "previous_book_value", DecimalDigits = 4)]
        public decimal PreviousBookValue { get; set; }

        /// <summary>
        /// New Book Value - القيمة الدفترية الجديدة
        /// </summary>
        [SugarColumn(ColumnName = "new_book_value", DecimalDigits = 4)]
        public decimal NewBookValue { get; set; }

        /// <summary>
        /// Gain/Loss Amount - مبلغ الربح/الخسارة
        /// </summary>
        [SugarColumn(ColumnName = "gain_loss_amount", DecimalDigits = 4, IsNullable = true)]
        public decimal? GainLossAmount { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_number")]
        public string ReferenceNumber { get; set; }

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

