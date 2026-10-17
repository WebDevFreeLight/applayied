using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Amendment - تعديل العقد
    /// Tracks all modifications made to a contract after activation.
    /// Each amendment records what changed, when, and by whom.
    /// Amendments can trigger recalculation of billing schedules.
    /// </summary>
    [SugarTable("erp_contract_amendment")]
    [SugarIndex("IX_amendment_contract", nameof(ContractId), OrderByType.Asc)]
    [SugarIndex("IX_amendment_type", nameof(AmendmentType), OrderByType.Asc)]
    [SugarIndex("IX_amendment_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("IX_amendment_effective_date", nameof(EffectiveDate), OrderByType.Asc)]
    [SugarIndex("UX_amendment_number", nameof(ContractId), OrderByType.Asc, nameof(AmendmentNumber), OrderByType.Asc, true)]
    public class ErpContractAmendment : SysBase
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// Primary key (GUID)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "amendment_id")]
        public Guid AmendmentId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// Foreign key to ErpContract
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Amendment Number - رقم التعديل
        /// Sequential number within the contract (1, 2, 3...)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "amendment_number")]
        public int AmendmentNumber { get; set; }

        /// <summary>
        /// Amendment Reference - مرجع التعديل
        /// Auto-generated reference (e.g., CNT-2024-00001-AMD-001)
        /// </summary>
        [SugarColumn(Length = 70, IsNullable = false, ColumnName = "amendment_reference")]
        public string AmendmentReference { get; set; }

        /// <summary>
        /// Amendment Type - نوع التعديل
        /// Type of change (PriceChange, DurationExtension, UnitAddition, etc.)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "amendment_type")]
        public AmendmentType AmendmentType { get; set; }

        /// <summary>
        /// Amendment Title - عنوان التعديل
        /// Short descriptive title
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "amendment_title")]
        public string AmendmentTitle { get; set; }

        /// <summary>
        /// Amendment Title (Arabic) - عنوان التعديل بالعربية
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "amendment_title_ar")]
        public string AmendmentTitleAr { get; set; }

        /// <summary>
        /// Status - الحالة
        /// Current status of the amendment (Draft, PendingApproval, Approved, etc.)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public AmendmentStatus Status { get; set; } = AmendmentStatus.Draft;

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// Date when the amendment takes effect
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "effective_date")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Expiry Date - تاريخ انتهاء السريان
        /// Optional: Date when the amendment effect expires (for temporary changes)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Description - الوصف
        /// Detailed description of the amendment
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Reason for Amendment - سبب التعديل
        /// Business reason for the change
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "reason")]
        public string Reason { get; set; }

        #region Changes JSON - تغييرات JSON

        /// <summary>
        /// Changes JSON - تفاصيل التغييرات
        /// JSON object containing all changes made by this amendment.
        /// Replaces multiple previous/new column pairs with flexible structure.
        /// Structure: { "field_name": { "previous": value, "new": value }, ... }
        /// Example: { "base_amount": { "previous": 1000, "new": 1200 },
        ///            "end_date": { "previous": "2024-12-31", "new": "2025-06-30" } }
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true, ColumnName = "changes_json")]
        public string ChangesJson { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// For unit-related amendments (reference only)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "unit_id")]
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// For pricing rule amendments (reference only)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "pricing_rule_id")]
        public Guid? PricingRuleId { get; set; }

        #endregion

        #region Financial Impact - الأثر المالي

        /// <summary>
        /// Monthly Impact Amount - الأثر الشهري
        /// Estimated monthly financial impact
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "monthly_impact")]
        public decimal? MonthlyImpact { get; set; }

        /// <summary>
        /// Total Contract Impact - الأثر الإجمالي للعقد
        /// Total financial impact over remaining contract duration
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "total_impact")]
        public decimal? TotalImpact { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        #endregion

        #region Billing Schedule Impact - أثر جدول الفوترة

        /// <summary>
        /// Recalculate Schedules - إعادة حساب الجداول
        /// Whether this amendment requires schedule recalculation
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "recalculate_schedules")]
        public bool RecalculateSchedules { get; set; } = true;

        /// <summary>
        /// Schedules Recalculated - تم إعادة حساب الجداول
        /// Whether schedules have been recalculated
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "schedules_recalculated")]
        public bool SchedulesRecalculated { get; set; } = false;

        /// <summary>
        /// Recalculation Date - تاريخ إعادة الحساب
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "recalculation_date")]
        public DateTime? RecalculationDate { get; set; }

        /// <summary>
        /// Schedules Affected Count - عدد الجداول المتأثرة
        /// Number of billing schedules affected
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "schedules_affected_count")]
        public int? SchedulesAffectedCount { get; set; }

        /// <summary>
        /// Apply to Pending Only - تطبيق على المعلق فقط
        /// If true, only affects pending schedules; if false, can affect invoiced schedules
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "apply_to_pending_only")]
        public bool ApplyToPendingOnly { get; set; } = true;

        #endregion

        #region Approval Workflow - سير عمل الموافقة

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// Whether this amendment requires approval
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "requires_approval")]
        public bool RequiresApproval { get; set; } = true;

        /// <summary>
        /// Requested By - طلب بواسطة
        /// User who requested the amendment
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "requested_by")]
        public string RequestedBy { get; set; }

        /// <summary>
        /// Requested Date - تاريخ الطلب
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "requested_date")]
        public DateTime? RequestedDate { get; set; }

        /// <summary>
        /// Approved By - تمت الموافقة بواسطة
        /// User who approved the amendment
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "approved_by")]
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Approved Date - تاريخ الموافقة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_date")]
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Approval Notes - ملاحظات الموافقة
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "approval_notes")]
        public string ApprovalNotes { get; set; }

        /// <summary>
        /// Rejected By - تم الرفض بواسطة
        /// User who rejected the amendment
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "rejected_by")]
        public string RejectedBy { get; set; }

        /// <summary>
        /// Rejected Date - تاريخ الرفض
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "rejected_date")]
        public DateTime? RejectedDate { get; set; }

        /// <summary>
        /// Rejection Reason - سبب الرفض
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "rejection_reason")]
        public string RejectionReason { get; set; }

        #endregion

        #region Application Tracking - تتبع التطبيق

        /// <summary>
        /// Applied Date - تاريخ التطبيق
        /// Date when amendment was applied to the contract
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "applied_date")]
        public DateTime? AppliedDate { get; set; }

        /// <summary>
        /// Applied By - تم التطبيق بواسطة
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "applied_by")]
        public string AppliedBy { get; set; }

        /// <summary>
        /// Reverted - تم التراجع
        /// Whether this amendment has been reverted
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "reverted")]
        public bool Reverted { get; set; } = false;

        /// <summary>
        /// Reverted Date - تاريخ التراجع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reverted_date")]
        public DateTime? RevertedDate { get; set; }

        /// <summary>
        /// Reverted By - تم التراجع بواسطة
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "reverted_by")]
        public string RevertedBy { get; set; }

        /// <summary>
        /// Revert Reason - سبب التراجع
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "revert_reason")]
        public string RevertReason { get; set; }

        #endregion

        #region Document Reference - مرجع المستند

        /// <summary>
        /// Document Reference - مرجع المستند
        /// External document reference (addendum number, etc.)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "document_reference")]
        public string DocumentReference { get; set; }

        /// <summary>
        /// Signed By Customer - موقع من العميل
        /// Whether customer has signed the amendment
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "signed_by_customer")]
        public bool SignedByCustomer { get; set; } = false;

        /// <summary>
        /// Customer Signature Date - تاريخ توقيع العميل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "customer_signature_date")]
        public DateTime? CustomerSignatureDate { get; set; }

        /// <summary>
        /// Customer Signatory Name - اسم موقع العميل
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "customer_signatory_name")]
        public string CustomerSignatoryName { get; set; }

        #endregion

        /// <summary>
        /// Notes - ملاحظات
        /// Internal notes
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Contract - العقد
        /// Parent contract
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string CustomerName { get; set; }

        /// <summary>
        /// Unit - الوحدة
        /// Related unit (if applicable)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ErpContractUnit Unit { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string UnitCode { get; set; }

        /// <summary>
        /// Amendment Type Name - اسم نوع التعديل
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string AmendmentTypeName { get; set; }

        /// <summary>
        /// Status Name - اسم الحالة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string StatusName { get; set; }

        #endregion
    }
}
