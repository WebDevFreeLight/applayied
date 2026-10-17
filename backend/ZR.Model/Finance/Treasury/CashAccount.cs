using System;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Cash Account - حساب نقدي
    /// Enhanced with Available Balance, Soft Delete, Currency FK, Cash Limit, and Audit Fields
    /// </summary>
    [SugarTable("cash_account")]
    public class CashAccount : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - ignore it as we use BranchId (Guid) instead
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Account Code - رمز الحساب
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Name (Arabic) - الاسم بالعربي
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "name_ar")]
        public string NameAr { get; set; }

        /// <summary>
        /// Name (English) - الاسم بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "name_en")]
        public string NameEn { get; set; }

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
        /// Branch ID - معرف الفرع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "branch_id")]
        public Guid? BranchId { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ المرتبط
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_account_id")]
        public Guid? GLAccountId { get; set; }

        /// <summary>
        /// Cash Limit - حد الصندوق (maximum allowed balance)
        /// </summary>
        [SugarColumn(ColumnName = "cash_limit", ColumnDataType = "decimal(18,4)")]
        public decimal CashLimit { get; set; }

        /// <summary>
        /// Minimum Balance - الحد الأدنى للرصيد
        /// </summary>
        [SugarColumn(ColumnName = "minimum_balance", ColumnDataType = "decimal(18,4)")]
        public decimal MinimumBalance { get; set; }

        /// <summary>
        /// Custodian ID - معرف أمين الصندوق
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "custodian_id")]
        public Guid? CustodianId { get; set; }

        /// <summary>
        /// Custodian Name - اسم أمين الصندوق
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "custodian_name")]
        public string CustodianName { get; set; }

        /// <summary>
        /// Location - الموقع
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "location")]
        public string Location { get; set; }

        /// <summary>
        /// Account Type - نوع الحساب
        /// </summary>
        [SugarColumn(ColumnName = "type")]
        public AccountType Type { get; set; }

        /// <summary>
        /// Accepts Cheques - هل يقبل الشيكات (cash accounts typically don't)
        /// </summary>
        [SugarColumn(ColumnName = "accepts_cheques")]
        public bool AcceptsCheques { get; set; }

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






























