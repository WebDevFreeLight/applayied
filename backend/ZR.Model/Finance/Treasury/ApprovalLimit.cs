using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Approval Limit Configuration - إعدادات حدود الموافقة
    /// Role-based approval limits for treasury transactions (requirement #37, Phase 2.1)
    /// </summary>
    [SugarTable("treasury_approval_limit")]
    public class ApprovalLimit : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Approval Limit ID - معرف حد الموافقة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        // Note: CompanyID and BranchID are inherited from SysBase - do not duplicate

        /// <summary>
        /// Role ID - معرف الدور
        /// </summary>
        [SugarColumn(ColumnName = "role_id")]
        public long RoleId { get; set; }

        /// <summary>
        /// Role Name - اسم الدور
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "role_name")]
        public string RoleName { get; set; }

        /// <summary>
        /// Transaction Type - نوع المعاملة
        /// "Payment", "Cheque", "Transfer", "All"
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "transaction_type")]
        public string TransactionType { get; set; } = "All";

        /// <summary>
        /// Minimum Amount - الحد الأدنى للمبلغ
        /// </summary>
        [SugarColumn(ColumnName = "min_amount", ColumnDataType = "decimal(18,4)")]
        public decimal MinAmount { get; set; }

        /// <summary>
        /// Maximum Amount - الحد الأقصى للمبلغ
        /// </summary>
        [SugarColumn(ColumnName = "max_amount", ColumnDataType = "decimal(18,4)")]
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// Currency Code - رمز العملة (null = all currencies)
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Daily Limit - الحد اليومي
        /// </summary>
        [SugarColumn(ColumnName = "daily_limit", ColumnDataType = "decimal(18,4)")]
        public decimal DailyLimit { get; set; }

        /// <summary>
        /// Monthly Limit - الحد الشهري
        /// </summary>
        [SugarColumn(ColumnName = "monthly_limit", ColumnDataType = "decimal(18,4)")]
        public decimal MonthlyLimit { get; set; }

        /// <summary>
        /// Requires Dual Approval - يتطلب موافقة مزدوجة
        /// For amounts above a certain threshold
        /// </summary>
        [SugarColumn(ColumnName = "requires_dual_approval")]
        public bool RequiresDualApproval { get; set; }

        /// <summary>
        /// Dual Approval Threshold - حد الموافقة المزدوجة
        /// </summary>
        [SugarColumn(ColumnName = "dual_approval_threshold", ColumnDataType = "decimal(18,4)")]
        public decimal DualApprovalThreshold { get; set; }

        /// <summary>
        /// Can Approve Own Transactions - يمكنه الموافقة على معاملاته الخاصة
        /// </summary>
        [SugarColumn(ColumnName = "can_approve_own")]
        public bool CanApproveOwn { get; set; }

        /// <summary>
        /// Effective From Date - تاريخ بدء السريان
        /// </summary>
        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective To Date - تاريخ انتهاء السريان
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "effective_to")]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Priority - الأولوية (lower = higher priority)
        /// </summary>
        [SugarColumn(ColumnName = "priority")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }
    }
}
