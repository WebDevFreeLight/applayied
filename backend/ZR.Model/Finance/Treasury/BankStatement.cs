using System;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Bank Statement - كشف حساب بنكي
    /// </summary>
    [SugarTable("bank_statement")]
    public class BankStatement : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Statement ID - معرف الكشف
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Bank Account ID - معرف الحساب البنكي
        /// </summary>
        [SugarColumn(ColumnName = "bank_account_id")]
        public Guid BankAccountId { get; set; }

        /// <summary>
        /// Statement Date - تاريخ الكشف
        /// </summary>
        [SugarColumn(ColumnName = "statement_date")]
        public DateTime StatementDate { get; set; }

        /// <summary>
        /// Statement Reference - مرجع الكشف
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "statement_reference")]
        public string StatementReference { get; set; }

        /// <summary>
        /// Opening Balance - الرصيد الافتتاحي
        /// </summary>
        [SugarColumn(ColumnName = "opening_balance", DecimalDigits = 4)]
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Closing Balance - الرصيد الختامي
        /// </summary>
        [SugarColumn(ColumnName = "closing_balance", DecimalDigits = 4)]
        public decimal ClosingBalance { get; set; }

        /// <summary>
        /// Is Imported - مستورد
        /// </summary>
        [SugarColumn(ColumnName = "imported")]
        public bool Imported { get; set; }

        /// <summary>
        /// Import Source - مصدر الاستيراد
        /// </summary>
        [SugarColumn(ColumnName = "import_source")]
        public BankStatementImportSource ImportSource { get; set; } = BankStatementImportSource.Manual;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Navigation - Bank Account
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BankAccount BankAccount { get; set; }
    }
}






























