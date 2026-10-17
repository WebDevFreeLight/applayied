using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Balance Ledger - سجل الرصيد
    /// Immutable record of all balance movements for audit trail and reconciliation
    /// Every balance change MUST create a ledger entry - this is the source of truth
    /// </summary>
    [SugarTable("treasury_balance_ledger", "Balance Ledger - سجل الرصيد")]
    public class BalanceLedger : SysBase
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Account ID (Bank or Cash) - معرف الحساب
        /// </summary>
        [SugarColumn(ColumnName = "account_id", IsNullable = false)]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Type - نوع الحساب
        /// </summary>
        [SugarColumn(ColumnName = "account_type", IsNullable = false)]
        public TreasuryAccountType AccountType { get; set; }

        /// <summary>
        /// Transaction Type - نوع المعاملة
        /// </summary>
        [SugarColumn(ColumnName = "transaction_type", IsNullable = false)]
        public BalanceLedgerTransactionType TransactionType { get; set; }

        /// <summary>
        /// Reference Entity Type (Payment, Cheque, etc.) - نوع الكيان المرجعي
        /// </summary>
        [SugarColumn(ColumnName = "reference_entity_type", IsNullable = false)]
        public TreasuryEntityType ReferenceEntityType { get; set; }

        /// <summary>
        /// Reference Entity ID - معرف الكيان المرجعي
        /// </summary>
        [SugarColumn(ColumnName = "reference_entity_id", IsNullable = false)]
        public Guid ReferenceEntityId { get; set; }

        /// <summary>
        /// Reference Number (Payment number, Cheque number, etc.) - رقم المرجع
        /// </summary>
        [SugarColumn(ColumnName = "reference_number", Length = 100, IsNullable = true)]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Transaction Date - تاريخ المعاملة
        /// </summary>
        [SugarColumn(ColumnName = "transaction_date", IsNullable = false)]
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Value Date (when the transaction affects balance) - تاريخ القيمة
        /// </summary>
        [SugarColumn(ColumnName = "value_date", IsNullable = true)]
        public DateTime? ValueDate { get; set; }

        /// <summary>
        /// Debit Amount (increases balance for assets) - مبلغ المدين
        /// </summary>
        [SugarColumn(ColumnName = "debit_amount", DecimalDigits = 4, IsNullable = false, DefaultValue = "0")]
        public decimal DebitAmount { get; set; }

        /// <summary>
        /// Credit Amount (decreases balance for assets) - مبلغ الدائن
        /// </summary>
        [SugarColumn(ColumnName = "credit_amount", DecimalDigits = 4, IsNullable = false, DefaultValue = "0")]
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Net Amount (Debit - Credit, positive = increase) - صافي المبلغ
        /// </summary>
        [SugarColumn(ColumnName = "net_amount", DecimalDigits = 4, IsNullable = false)]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Balance Before Transaction - الرصيد قبل المعاملة
        /// </summary>
        [SugarColumn(ColumnName = "balance_before", DecimalDigits = 4, IsNullable = false)]
        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// Balance After Transaction - الرصيد بعد المعاملة
        /// </summary>
        [SugarColumn(ColumnName = "balance_after", DecimalDigits = 4, IsNullable = false)]
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Available Balance Before (for bank accounts) - الرصيد المتاح قبل
        /// </summary>
        [SugarColumn(ColumnName = "available_balance_before", DecimalDigits = 4, IsNullable = true)]
        public decimal? AvailableBalanceBefore { get; set; }

        /// <summary>
        /// Available Balance After (for bank accounts) - الرصيد المتاح بعد
        /// </summary>
        [SugarColumn(ColumnName = "available_balance_after", DecimalDigits = 4, IsNullable = true)]
        public decimal? AvailableBalanceAfter { get; set; }

        /// <summary>
        /// Currency Code - رمز العملة
        /// </summary>
        [SugarColumn(ColumnName = "currency", Length = 3, IsNullable = false)]
        public string Currency { get; set; }

        /// <summary>
        /// Exchange Rate (if multi-currency) - سعر الصرف
        /// </summary>
        [SugarColumn(ColumnName = "exchange_rate", DecimalDigits = 6, IsNullable = false, DefaultValue = "1")]
        public decimal ExchangeRate { get; set; } = 1;

        /// <summary>
        /// Base Currency Amount - المبلغ بالعملة الأساسية
        /// </summary>
        [SugarColumn(ColumnName = "base_currency_amount", DecimalDigits = 4, IsNullable = true)]
        public decimal? BaseCurrencyAmount { get; set; }

        /// <summary>
        /// GL Journal ID (if posted) - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(ColumnName = "gl_journal_id", IsNullable = true)]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(ColumnName = "description", Length = 500, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Is Reversal Entry - هل هو قيد عكسي
        /// </summary>
        [SugarColumn(ColumnName = "is_reversal", IsNullable = false, DefaultValue = "0")]
        public bool IsReversal { get; set; }

        /// <summary>
        /// Original Entry ID (for reversals) - معرف القيد الأصلي
        /// </summary>
        [SugarColumn(ColumnName = "original_entry_id", IsNullable = true)]
        public Guid? OriginalEntryId { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_year", IsNullable = false)]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Fiscal Period - الفترة المالية
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_period", IsNullable = false)]
        public int FiscalPeriod { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [SugarColumn(ColumnName = "cost_center_id", IsNullable = true)]
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        [SugarColumn(ColumnName = "department_id", IsNullable = true)]
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        [SugarColumn(ColumnName = "project_id", IsNullable = true)]
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// User who created the entry - المستخدم الذي أنشأ القيد
        /// </summary>
        [SugarColumn(ColumnName = "created_by_user_id", Length = 100, IsNullable = false)]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// User Name - اسم المستخدم
        /// </summary>
        [SugarColumn(ColumnName = "created_by_user_name", Length = 200, IsNullable = true)]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// IP Address - عنوان IP
        /// </summary>
        [SugarColumn(ColumnName = "ip_address", Length = 50, IsNullable = true)]
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent - وكيل المستخدم
        /// </summary>
        [SugarColumn(ColumnName = "user_agent", Length = 500, IsNullable = true)]
        public string UserAgent { get; set; }

        /// <summary>
        /// Trace ID for distributed tracing - معرف التتبع
        /// </summary>
        [SugarColumn(ColumnName = "trace_id", Length = 100, IsNullable = true)]
        public string TraceId { get; set; }

        /// <summary>
        /// Request ID - معرف الطلب
        /// </summary>
        [SugarColumn(ColumnName = "request_id", Length = 100, IsNullable = true)]
        public string RequestId { get; set; }

        /// <summary>
        /// Timestamp (UTC) - الطابع الزمني
        /// </summary>
        [SugarColumn(ColumnName = "timestamp", IsNullable = false)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Is Reconciled - هل تم تسويته
        /// </summary>
        [SugarColumn(ColumnName = "is_reconciled", IsNullable = false, DefaultValue = "0")]
        public bool IsReconciled { get; set; }

        /// <summary>
        /// Reconciliation ID - معرف التسوية
        /// </summary>
        [SugarColumn(ColumnName = "reconciliation_id", IsNullable = true)]
        public Guid? ReconciliationId { get; set; }

        /// <summary>
        /// Row Version for optimistic concurrency - إصدار الصف
        /// </summary>
        [SugarColumn(ColumnName = "row_version", IsNullable = false, DefaultValue = "1")]
        public int RowVersion { get; set; } = 1;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(ColumnName = "notes", Length = 1000, IsNullable = true)]
        public string Notes { get; set; }

        /// <summary>
        /// Ignore BranchID from SysBase
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }
    }

    /// <summary>
    /// Treasury Account Type - نوع حساب الخزينة
    /// </summary>
    public enum TreasuryAccountType
    {
        /// <summary>Bank Account - حساب بنكي</summary>
        BankAccount = 0,
        /// <summary>Cash Account - حساب نقدي</summary>
        CashAccount = 1
    }

    /// <summary>
    /// Balance Ledger Transaction Type - نوع معاملة سجل الرصيد
    /// </summary>
    public enum BalanceLedgerTransactionType
    {
        /// <summary>Opening Balance - رصيد افتتاحي</summary>
        OpeningBalance = 0,
        /// <summary>Payment Receipt - إيصال دفعة</summary>
        PaymentReceipt = 1,
        /// <summary>Payment Disbursement - صرف دفعة</summary>
        PaymentDisbursement = 2,
        /// <summary>Transfer In - تحويل وارد</summary>
        TransferIn = 3,
        /// <summary>Transfer Out - تحويل صادر</summary>
        TransferOut = 4,
        /// <summary>Cheque Issuance - إصدار شيك</summary>
        ChequeIssuance = 5,
        /// <summary>Cheque Clearing - تسوية شيك</summary>
        ChequeClearing = 6,
        /// <summary>Cheque Bounce - ارتجاع شيك</summary>
        ChequeBounce = 7,
        /// <summary>Bank Charges - رسوم بنكية</summary>
        BankCharges = 8,
        /// <summary>Bank Interest - فائدة بنكية</summary>
        BankInterest = 9,
        /// <summary>Manual Adjustment - تعديل يدوي</summary>
        ManualAdjustment = 10,
        /// <summary>Reconciliation Adjustment - تعديل تسوية</summary>
        ReconciliationAdjustment = 11,
        /// <summary>FX Gain/Loss - ربح/خسارة صرف</summary>
        FxGainLoss = 12,
        /// <summary>Reversal - عكس</summary>
        Reversal = 13,
        /// <summary>Available Balance Reserve - حجز رصيد متاح</summary>
        AvailableBalanceReserve = 14,
        /// <summary>Available Balance Release - تحرير رصيد متاح</summary>
        AvailableBalanceRelease = 15,
        /// <summary>Cheque Deposit (incoming cheque) - إيداع شيك وارد</summary>
        ChequeDeposit = 16,
        /// <summary>Bank Fees - رسوم بنكية</summary>
        BankFees = 17
    }
}
