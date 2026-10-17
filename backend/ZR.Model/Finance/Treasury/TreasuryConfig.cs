using System;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury Configuration - إعدادات الخزينة
    /// </summary>
    [SugarTable("treasury_config")]
    public class TreasuryConfig : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Config ID - معرف الإعداد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Auto Import Bank Statements - استيراد تلقائي لكشوف الحساب
        /// </summary>
        [SugarColumn(ColumnName = "auto_import_bank_statements")]
        public bool AutoImportBankStatements { get; set; }

        /// <summary>
        /// Bank Statement Import Format - تنسيق استيراد كشف الحساب
        /// </summary>
        [SugarColumn(ColumnName = "bank_statement_import_format")]
        public BankStatementImportSource BankStatementImportFormat { get; set; } = BankStatementImportSource.CSV;

        /// <summary>
        /// Auto Reconcile - تسوية تلقائية
        /// </summary>
        [SugarColumn(ColumnName = "auto_reconcile")]
        public bool AutoReconcile { get; set; }

        /// <summary>
        /// Default Payment Approval Threshold - حد الموافقة الافتراضي للدفعات
        /// </summary>
        [SugarColumn(ColumnName = "default_payment_approval_threshold", DecimalDigits = 4)]
        public decimal DefaultPaymentApprovalThreshold { get; set; }

        /// <summary>
        /// Petty Cash Limit - حد الصندوق الصغير
        /// </summary>
        [SugarColumn(ColumnName = "petty_cash_limit", DecimalDigits = 4)]
        public decimal PettyCashLimit { get; set; }

        /// <summary>
        /// Approval Workflow Enabled - تفعيل سير عمل الموافقة
        /// </summary>
        [SugarColumn(ColumnName = "approval_workflow_enabled")]
        public bool ApprovalWorkflowEnabled { get; set; }

        #region GL Auto-Creation Settings

        /// <summary>
        /// Whether to auto-create GL accounts when creating treasury accounts
        /// Default: true
        /// </summary>
        [SugarColumn(ColumnName = "auto_create_gl_accounts")]
        public bool AutoCreateGLAccounts { get; set; } = true;

        /// <summary>
        /// Default parent GL Account ID for auto-created bank GL accounts
        /// References: gl_account.account_id where account_number = '1112000000' (Bank Accounts)
        /// معرف حساب الأصل الافتراضي للحسابات البنكية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_bank_gl_parent_id")]
        public Guid? DefaultBankGLParentAccountId { get; set; }

        /// <summary>
        /// Default parent GL Account ID for auto-created cash GL accounts
        /// References: gl_account.account_id where account_number = '1111000000' (Cash Boxes)
        /// معرف حساب الأصل الافتراضي للحسابات النقدية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_cash_gl_parent_id")]
        public Guid? DefaultCashGLParentAccountId { get; set; }

        /// <summary>
        /// GL Account code prefix for bank accounts (e.g., "1112")
        /// Used for auto-generating account codes
        /// بادئة رقم الحساب للحسابات البنكية
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "bank_gl_code_prefix")]
        public string BankGLCodePrefix { get; set; } = "1112";

        /// <summary>
        /// GL Account code prefix for cash accounts (e.g., "1111")
        /// بادئة رقم الحساب للحسابات النقدية
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "cash_gl_code_prefix")]
        public string CashGLCodePrefix { get; set; } = "1111";

        #endregion
    }
}






























