using System;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Bank Account - حساب بنكي
    /// Enhanced with Available Balance, Soft Delete, Currency FK, and Audit Fields
    /// </summary>
    [SugarTable("bank_account")]
    public class BankAccount : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - ignore it as we use Branch (string) instead
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Bank Code - رمز البنك
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "bank_code")]
        public string BankCode { get; set; }

        /// <summary>
        /// Bank Name (Arabic) - اسم البنك بالعربي
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "bank_name_ar")]
        public string BankNameAr { get; set; }

        /// <summary>
        /// Bank Name (English) - اسم البنك بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "bank_name_en")]
        public string BankNameEn { get; set; }

        /// <summary>
        /// Account Number - رقم الحساب
        /// </summary>
        [SugarColumn(Length = 100, ColumnName = "account_number")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// IBAN - رقم الآيبان
        /// </summary>
        [SugarColumn(Length = 34, IsNullable = true, ColumnName = "iban")]
        public string IBAN { get; set; }

        /// <summary>
        /// SWIFT Code - رمز السويفت
        /// </summary>
        [SugarColumn(Length = 11, IsNullable = true, ColumnName = "swift_code")]
        public string SwiftCode { get; set; }

        /// <summary>
        /// Currency Code - رمز العملة (legacy, kept for backward compatibility)
        /// </summary>
        [SugarColumn(Length = 3, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Currency ID - معرف العملة (FK to Currency table)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "currency_id")]
        public Guid? CurrencyId { get; set; }

        /// <summary>
        /// Branch - الفرع
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "branch")]
        public string Branch { get; set; }

        /// <summary>
        /// Bank Contact - جهة اتصال البنك
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "bank_contact")]
        public string BankContact { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ المرتبط
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_account_id")]
        public Guid? GLAccountId { get; set; }

        /// <summary>
        /// Pending Cheques Amount - مبلغ الشيكات المعلقة
        /// </summary>
        [SugarColumn(ColumnName = "pending_cheques_amount", ColumnDataType = "decimal(18,4)")]
        public decimal PendingChequesAmount { get; set; }

        /// <summary>
        /// Overdraft Limit - حد السحب على المكشوف
        /// </summary>
        [SugarColumn(ColumnName = "overdraft_limit", ColumnDataType = "decimal(18,4)")]
        public decimal OverdraftLimit { get; set; }

        /// <summary>
        /// Is Overdraft Allowed - هل السحب على المكشوف مسموح
        /// </summary>
        [SugarColumn(ColumnName = "is_overdraft_allowed")]
        public bool IsOverdraftAllowed { get; set; }

        /// <summary>
        /// Reconciliation Date - تاريخ آخر تسوية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reconciliation_date")]
        public DateTime? ReconciliationDate { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public AccountStatus Status { get; set; } = AccountStatus.Active;

        /// <summary>
        /// Is Deleted (Soft Delete) - هل محذوف
        /// </summary>
        [SugarColumn(ColumnName = "is_deleted")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Deleted At - تاريخ الحذف
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "deleted_at")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Deleted By - محذوف بواسطة
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "deleted_by")]
        public string DeletedBy { get; set; }

        /// <summary>
        /// Row Version - نسخة الصف (للتزامن المتفائل)
        /// </summary>
        [SugarColumn(ColumnName = "row_version", IsEnableUpdateVersionValidation = true)]
        public int RowVersion { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cost_center_id")]
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "department_id")]
        public Guid? DepartmentId { get; set; }
    }
}





