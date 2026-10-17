using System;

namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Dimension Requirements - متطلبات الأبعاد المحاسبية
    /// Flags to indicate which dimensions are required for journal entries on this account
    /// Replaces RequiresCostCenter, RequiresDepartment, RequiresProject boolean fields
    /// </summary>
    [Flags]
    public enum DimensionRequirements
    {
        /// <summary>
        /// None - لا شيء
        /// No dimension requirements for this account
        /// </summary>
        None = 0,

        /// <summary>
        /// CostCenter - مركز تكلفة
        /// Journal entries must include a cost center reference
        /// </summary>
        CostCenter = 1,

        /// <summary>
        /// Department - قسم
        /// Journal entries must include a department reference
        /// </summary>
        Department = 2,

        /// <summary>
        /// Project - مشروع
        /// Journal entries must include a project reference
        /// </summary>
        Project = 4
    }

    /// <summary>
    /// Account Nature - طبيعة الحساب (نقدي/بنكي)
    /// Replaces IsCashAccount and IsBankAccount boolean fields
    /// </summary>
    public enum AccountNature
    {
        /// <summary>
        /// None - لا شيء
        /// Account is neither a cash nor bank account
        /// </summary>
        None = 0,

        /// <summary>
        /// Cash - نقدي
        /// Account represents physical cash (petty cash, cash on hand)
        /// </summary>
        Cash = 1,

        /// <summary>
        /// Bank - بنكي
        /// Account represents a bank account (checking, savings, etc.)
        /// </summary>
        Bank = 2
    }

    /// <summary>
    /// Reconciliation Mode - وضع التسوية
    /// Indicates whether an account requires periodic reconciliation
    /// Replaces IsReconciliationAccount boolean field
    /// </summary>
    public enum ReconciliationMode
    {
        /// <summary>
        /// None - غير مطلوب
        /// Account does not require reconciliation
        /// </summary>
        None = 0,

        /// <summary>
        /// Required - مطلوب
        /// Account requires periodic reconciliation (bank statements, subsidiary ledgers, etc.)
        /// </summary>
        Required = 1
    }

    /// <summary>
    /// Reconciliation Status - حالة التسوية
    /// Current reconciliation state of an account
    /// Replaces integer-based ReconciliationStatus field
    /// </summary>
    public enum ReconciliationStatus
    {
        /// <summary>
        /// NotReconciled - غير مُسوَّى
        /// Account has not been reconciled
        /// </summary>
        NotReconciled = 0,

        /// <summary>
        /// Reconciled - مُسوَّى
        /// Account has been fully reconciled
        /// </summary>
        Reconciled = 1,

        /// <summary>
        /// PendingReview - قيد المراجعة
        /// Reconciliation is complete but awaiting review/approval
        /// </summary>
        PendingReview = 2
    }
}
