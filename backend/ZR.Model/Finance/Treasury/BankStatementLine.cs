using System;
using SqlSugar;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Line - بند كشف حساب بنكي
    /// </summary>
    [SugarTable("bank_statement_line")]
    public class BankStatementLine : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Line ID - معرف البند
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Bank Statement ID - معرف كشف الحساب
        /// </summary>
        [SugarColumn(ColumnName = "bank_statement_id")]
        public Guid BankStatementId { get; set; }

        /// <summary>
        /// Line Date - تاريخ البند
        /// </summary>
        [SugarColumn(ColumnName = "line_date")]
        public DateTime LineDate { get; set; }

        /// <summary>
        /// Value Date - تاريخ القيمة
        /// </summary>
        [SugarColumn(ColumnName = "value_date")]
        public DateTime ValueDate { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Amount - المبلغ (موجب للائتمان/وارد، سالب للخصم/صادر)
        /// </summary>
        [SugarColumn(ColumnName = "amount", DecimalDigits = 4)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Running Balance - الرصيد المتجمع
        /// </summary>
        [SugarColumn(ColumnName = "running_balance", DecimalDigits = 4)]
        public decimal RunningBalance { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_no")]
        public string ReferenceNo { get; set; }

        /// <summary>
        /// Is Matched - مطابق
        /// </summary>
        [SugarColumn(ColumnName = "matched")]
        public bool Matched { get; set; }

        /// <summary>
        /// Matched Transaction ID - معرف المعاملة المطابقة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "matched_transaction_id")]
        public Guid? MatchedTransactionId { get; set; }

        /// <summary>
        /// Navigation - Bank Statement
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BankStatement BankStatement { get; set; }

        /// <summary>
        /// Navigation - Matched Payment
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Payment MatchedPayment { get; set; }
    }
}






























