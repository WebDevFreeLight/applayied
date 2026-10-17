using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    #region Query DTO

    /// <summary>
    /// Contract Amendment Query DTO - معايير البحث عن تعديلات العقد
    /// </summary>
    public class ErpContractAmendmentQueryDto : PagerInfo
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        public Guid? AmendmentId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Amendment Type - نوع التعديل
        /// </summary>
        public AmendmentType? AmendmentType { get; set; }

        /// <summary>
        /// Multiple Amendment Types - أنواع تعديلات متعددة
        /// </summary>
        public List<AmendmentType> AmendmentTypes { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public AmendmentStatus? Status { get; set; }

        /// <summary>
        /// Multiple Statuses - حالات متعددة
        /// </summary>
        public List<AmendmentStatus> Statuses { get; set; }

        /// <summary>
        /// Effective Date From - تاريخ السريان من
        /// </summary>
        public DateTime? EffectiveDateFrom { get; set; }

        /// <summary>
        /// Effective Date To - تاريخ السريان إلى
        /// </summary>
        public DateTime? EffectiveDateTo { get; set; }

        /// <summary>
        /// Requested Date From - تاريخ الطلب من
        /// </summary>
        public DateTime? RequestedDateFrom { get; set; }

        /// <summary>
        /// Requested Date To - تاريخ الطلب إلى
        /// </summary>
        public DateTime? RequestedDateTo { get; set; }

        /// <summary>
        /// Requested By - طلب بواسطة
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool? RequiresApproval { get; set; }

        /// <summary>
        /// Pending Approval Only - في انتظار الموافقة فقط
        /// </summary>
        public bool? PendingApprovalOnly { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyId { get; set; }
    }

    #endregion

    #region Create DTOs by Amendment Type

    /// <summary>
    /// Base Create Amendment DTO - قاعدة بيانات إنشاء التعديل
    /// Common fields for all amendment types
    /// </summary>
    public abstract class CreateAmendmentBaseDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// </summary>
        [Required(ErrorMessage = "Effective date is required / تاريخ السريان مطلوب")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Amendment Title - عنوان التعديل
        /// </summary>
        [StringLength(200)]
        public string AmendmentTitle { get; set; }

        /// <summary>
        /// Amendment Title (Arabic) - عنوان التعديل بالعربية
        /// </summary>
        [StringLength(200)]
        public string AmendmentTitleAr { get; set; }

        /// <summary>
        /// Reason - السبب
        /// </summary>
        [Required(ErrorMessage = "Reason is required / السبب مطلوب")]
        [StringLength(500)]
        public string Reason { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Document Reference - مرجع المستند
        /// </summary>
        [StringLength(100)]
        public string DocumentReference { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool RequiresApproval { get; set; } = true;

        /// <summary>
        /// Apply to Pending Only - تطبيق على المعلق فقط
        /// </summary>
        public bool ApplyToPendingOnly { get; set; } = true;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Create Price Change Amendment DTO - إنشاء تعديل تغيير السعر
    /// </summary>
    public class CreatePriceChangeAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// If null, applies to entire contract base amount
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Change Type - نوع التغيير
        /// "Absolute" or "Percentage"
        /// </summary>
        [Required(ErrorMessage = "Change type is required / نوع التغيير مطلوب")]
        public string ChangeType { get; set; } = "Absolute";

        /// <summary>
        /// New Amount - المبلغ الجديد
        /// For absolute change: the new price
        /// </summary>
        public decimal? NewAmount { get; set; }

        /// <summary>
        /// Change Percentage - نسبة التغيير
        /// For percentage change: the increase/decrease percentage
        /// </summary>
        public decimal? ChangePercentage { get; set; }

        /// <summary>
        /// Expiry Date - تاريخ انتهاء السريان
        /// For temporary price changes
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// Create Duration Extension Amendment DTO - إنشاء تعديل تمديد المدة
    /// </summary>
    public class CreateDurationExtensionAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// New End Date - تاريخ النهاية الجديد
        /// </summary>
        [Required(ErrorMessage = "New end date is required / تاريخ النهاية الجديد مطلوب")]
        public DateTime NewEndDate { get; set; }

        /// <summary>
        /// Keep Same Pricing - الإبقاء على نفس التسعير
        /// If true, uses existing pricing; if false, can specify new pricing
        /// </summary>
        public bool KeepSamePricing { get; set; } = true;

        /// <summary>
        /// New Base Amount - المبلغ الأساسي الجديد
        /// For price adjustment with extension
        /// </summary>
        public decimal? NewBaseAmount { get; set; }

        /// <summary>
        /// Generate Schedules - إنشاء الجداول
        /// Whether to generate billing schedules for the extended period
        /// </summary>
        public bool GenerateSchedules { get; set; } = true;
    }

    /// <summary>
    /// Create Duration Reduction Amendment DTO - إنشاء تعديل تقليص المدة
    /// </summary>
    public class CreateDurationReductionAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// New End Date - تاريخ النهاية الجديد
        /// </summary>
        [Required(ErrorMessage = "New end date is required / تاريخ النهاية الجديد مطلوب")]
        public DateTime NewEndDate { get; set; }

        /// <summary>
        /// Cancel Future Schedules - إلغاء الجداول المستقبلية
        /// </summary>
        public bool CancelFutureSchedules { get; set; } = true;

        /// <summary>
        /// Refund Amount - مبلغ الاسترداد
        /// If applicable
        /// </summary>
        public decimal? RefundAmount { get; set; }
    }

    /// <summary>
    /// Create Unit Addition Amendment DTO - إنشاء تعديل إضافة وحدة
    /// </summary>
    public class CreateUnitAdditionAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// Unit Details - تفاصيل الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit details are required / تفاصيل الوحدة مطلوبة")]
        public CreateErpContractUnitDto UnitDetails { get; set; }

        /// <summary>
        /// Pro-Rate First Period - تقسيط الفترة الأولى
        /// </summary>
        public bool ProRateFirstPeriod { get; set; } = true;
    }

    /// <summary>
    /// Create Unit Removal Amendment DTO - إنشاء تعديل إزالة وحدة
    /// </summary>
    public class CreateUnitRemovalAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit ID is required / معرف الوحدة مطلوب")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// Removal Date - تاريخ الإزالة
        /// </summary>
        [Required(ErrorMessage = "Removal date is required / تاريخ الإزالة مطلوب")]
        public DateTime RemovalDate { get; set; }

        /// <summary>
        /// Pro-Rate Final Period - تقسيط الفترة الأخيرة
        /// </summary>
        public bool ProRateFinalPeriod { get; set; } = true;

        /// <summary>
        /// Credit Amount - مبلغ الائتمان
        /// Credit amount for remaining period
        /// </summary>
        public decimal? CreditAmount { get; set; }
    }

    /// <summary>
    /// Create Unit Change Amendment DTO - إنشاء تعديل تغيير الوحدة
    /// For changing unit details (area, price per area, etc.)
    /// </summary>
    public class CreateUnitChangeAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit ID is required / معرف الوحدة مطلوب")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// New Area - المساحة الجديدة
        /// </summary>
        public decimal? NewArea { get; set; }

        /// <summary>
        /// New Base Price - السعر الأساسي الجديد
        /// </summary>
        public decimal? NewBasePrice { get; set; }

        /// <summary>
        /// New Price Per Area - السعر الجديد لكل وحدة مساحة
        /// </summary>
        public decimal? NewPricePerArea { get; set; }

        /// <summary>
        /// New Unit Type - نوع الوحدة الجديد
        /// </summary>
        public ContractUnitType? NewUnitType { get; set; }
    }

    /// <summary>
    /// Create Discount Amendment DTO - إنشاء تعديل خصم
    /// </summary>
    public class CreateDiscountAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// Is Addition - إضافة
        /// True for adding discount, false for removing
        /// </summary>
        public bool IsAddition { get; set; } = true;

        /// <summary>
        /// Discount Type - نوع الخصم
        /// "Percentage" or "Fixed"
        /// </summary>
        [Required(ErrorMessage = "Discount type is required / نوع الخصم مطلوب")]
        public string DiscountType { get; set; }

        /// <summary>
        /// Discount Value - قيمة الخصم
        /// </summary>
        [Required(ErrorMessage = "Discount value is required / قيمة الخصم مطلوبة")]
        [Range(0.01, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Discount Name - اسم الخصم
        /// </summary>
        [StringLength(100)]
        public string DiscountName { get; set; }

        /// <summary>
        /// Expiry Date - تاريخ انتهاء السريان
        /// For temporary discounts
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Existing Pricing Rule ID - معرف قاعدة التسعير الموجودة
        /// For removing existing discount
        /// </summary>
        public Guid? ExistingPricingRuleId { get; set; }
    }

    /// <summary>
    /// Create Billing Frequency Change Amendment DTO - إنشاء تعديل تغيير تكرار الفوترة
    /// </summary>
    public class CreateBillingFrequencyChangeAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// New Billing Frequency - تكرار الفوترة الجديد
        /// </summary>
        [Required(ErrorMessage = "New billing frequency is required / تكرار الفوترة الجديد مطلوب")]
        public BillingFrequency NewBillingFrequency { get; set; }

        /// <summary>
        /// New Billing Day - يوم الفوترة الجديد
        /// </summary>
        [Range(1, 31)]
        public int? NewBillingDay { get; set; }

        /// <summary>
        /// Regenerate All Pending - إعادة إنشاء جميع المعلقة
        /// </summary>
        public bool RegenerateAllPending { get; set; } = true;
    }

    /// <summary>
    /// Create Terms Change Amendment DTO - إنشاء تعديل تغيير الشروط
    /// </summary>
    public class CreateTermsChangeAmendmentDto : CreateAmendmentBaseDto
    {
        /// <summary>
        /// New Payment Terms - شروط الدفع الجديدة
        /// </summary>
        [StringLength(100)]
        public string NewPaymentTerms { get; set; }

        /// <summary>
        /// New Payment Due Days - أيام استحقاق الدفع الجديدة
        /// </summary>
        [Range(0, 365)]
        public int? NewPaymentDueDays { get; set; }

        /// <summary>
        /// New Auto Renew - التجديد التلقائي الجديد
        /// </summary>
        public bool? NewAutoRenew { get; set; }

        /// <summary>
        /// New Renewal Type - نوع التجديد الجديد
        /// </summary>
        public RenewalType? NewRenewalType { get; set; }
    }

    #endregion

    #region List DTO

    /// <summary>
    /// Contract Amendment List DTO - قائمة تعديلات العقد
    /// </summary>
    public class ErpContractAmendmentFullListDto
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        public Guid AmendmentId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        [ExcelColumn(Name = "Contract / العقد")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        [ExcelColumn(Name = "Customer / العميل")]
        public string CustomerName { get; set; }

        /// <summary>
        /// Amendment Number - رقم التعديل
        /// </summary>
        [ExcelColumn(Name = "AMD # / رقم التعديل")]
        public int AmendmentNumber { get; set; }

        /// <summary>
        /// Amendment Reference - مرجع التعديل
        /// </summary>
        [ExcelColumn(Name = "Reference / المرجع")]
        public string AmendmentReference { get; set; }

        /// <summary>
        /// Amendment Type - نوع التعديل
        /// </summary>
        [ExcelColumn(Name = "Type / النوع")]
        public AmendmentType AmendmentType { get; set; }

        /// <summary>
        /// Amendment Type Name - اسم نوع التعديل
        /// </summary>
        public string AmendmentTypeName { get; set; }

        /// <summary>
        /// Amendment Title - عنوان التعديل
        /// </summary>
        [ExcelColumn(Name = "Title / العنوان")]
        public string AmendmentTitle { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [ExcelColumn(Name = "Status / الحالة")]
        public AmendmentStatus Status { get; set; }

        /// <summary>
        /// Status Name - اسم الحالة
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// </summary>
        [ExcelColumn(Name = "Effective Date / تاريخ السريان", Format = "yyyy-MM-dd")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Previous Amount - المبلغ السابق
        /// </summary>
        [ExcelColumn(Name = "Previous / السابق", Format = "#,##0.00")]
        public decimal? PreviousAmount { get; set; }

        /// <summary>
        /// New Amount - المبلغ الجديد
        /// </summary>
        [ExcelColumn(Name = "New / الجديد", Format = "#,##0.00")]
        public decimal? NewAmount { get; set; }

        /// <summary>
        /// Amount Difference - فرق المبلغ
        /// </summary>
        [ExcelColumn(Name = "Difference / الفرق", Format = "#,##0.00")]
        public decimal? AmountDifference { get; set; }

        /// <summary>
        /// Monthly Impact - الأثر الشهري
        /// </summary>
        [ExcelColumn(Name = "Monthly Impact / الأثر الشهري", Format = "#,##0.00")]
        public decimal? MonthlyImpact { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Requested By - طلب بواسطة
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// Requested Date - تاريخ الطلب
        /// </summary>
        [ExcelColumn(Name = "Requested / تاريخ الطلب", Format = "yyyy-MM-dd")]
        public DateTime? RequestedDate { get; set; }

        /// <summary>
        /// Approved By - تمت الموافقة بواسطة
        /// </summary>
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Approved Date - تاريخ الموافقة
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Is Pending Approval - في انتظار الموافقة
        /// </summary>
        public bool IsPendingApproval => Status == AmendmentStatus.PendingApproval;

        /// <summary>
        /// Can Approve - يمكن الموافقة
        /// </summary>
        public bool CanApprove => Status == AmendmentStatus.PendingApproval;

        /// <summary>
        /// Can Revert - يمكن التراجع
        /// </summary>
        public bool CanRevert => Status == AmendmentStatus.Approved && !Reverted;

        /// <summary>
        /// Reverted - تم التراجع
        /// </summary>
        public bool Reverted { get; set; }

        /// <summary>
        /// Created Time - وقت الإنشاء
        /// </summary>
        public DateTime? Create_time { get; set; }
    }

    #endregion

    #region Detail DTO

    /// <summary>
    /// Contract Amendment Detail DTO - تفاصيل تعديل العقد
    /// </summary>
    public class ErpContractAmendmentDetailDto : ErpContractAmendmentFullListDto
    {
        /// <summary>
        /// Amendment Title (Arabic) - عنوان التعديل بالعربية
        /// </summary>
        public string AmendmentTitleAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Reason - السبب
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Expiry Date - تاريخ انتهاء السريان
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        #region Value Changes

        /// <summary>
        /// Previous Value - القيمة السابقة
        /// </summary>
        public string PreviousValue { get; set; }

        /// <summary>
        /// New Value - القيمة الجديدة
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Percentage Change - نسبة التغيير
        /// </summary>
        public decimal? PercentageChange { get; set; }

        #endregion

        #region Date Changes

        /// <summary>
        /// Previous Start Date - تاريخ البداية السابق
        /// </summary>
        public DateTime? PreviousStartDate { get; set; }

        /// <summary>
        /// New Start Date - تاريخ البداية الجديد
        /// </summary>
        public DateTime? NewStartDate { get; set; }

        /// <summary>
        /// Previous End Date - تاريخ النهاية السابق
        /// </summary>
        public DateTime? PreviousEndDate { get; set; }

        /// <summary>
        /// New End Date - تاريخ النهاية الجديد
        /// </summary>
        public DateTime? NewEndDate { get; set; }

        /// <summary>
        /// Duration Change Days - أيام تغيير المدة
        /// </summary>
        public int? DurationChangeDays { get; set; }

        #endregion

        #region Unit/Rule References

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        public Guid? PricingRuleId { get; set; }

        #endregion

        #region Financial Impact

        /// <summary>
        /// Total Impact - الأثر الإجمالي
        /// </summary>
        public decimal? TotalImpact { get; set; }

        #endregion

        #region Billing Schedule Impact

        /// <summary>
        /// Recalculate Schedules - إعادة حساب الجداول
        /// </summary>
        public bool RecalculateSchedules { get; set; }

        /// <summary>
        /// Schedules Recalculated - تم إعادة حساب الجداول
        /// </summary>
        public bool SchedulesRecalculated { get; set; }

        /// <summary>
        /// Recalculation Date - تاريخ إعادة الحساب
        /// </summary>
        public DateTime? RecalculationDate { get; set; }

        /// <summary>
        /// Schedules Affected Count - عدد الجداول المتأثرة
        /// </summary>
        public int? SchedulesAffectedCount { get; set; }

        /// <summary>
        /// Apply to Pending Only - تطبيق على المعلق فقط
        /// </summary>
        public bool ApplyToPendingOnly { get; set; }

        #endregion

        #region Approval Details

        /// <summary>
        /// Approval Notes - ملاحظات الموافقة
        /// </summary>
        public string ApprovalNotes { get; set; }

        /// <summary>
        /// Rejected By - تم الرفض بواسطة
        /// </summary>
        public string RejectedBy { get; set; }

        /// <summary>
        /// Rejected Date - تاريخ الرفض
        /// </summary>
        public DateTime? RejectedDate { get; set; }

        /// <summary>
        /// Rejection Reason - سبب الرفض
        /// </summary>
        public string RejectionReason { get; set; }

        #endregion

        #region Application Tracking

        /// <summary>
        /// Applied Date - تاريخ التطبيق
        /// </summary>
        public DateTime? AppliedDate { get; set; }

        /// <summary>
        /// Applied By - تم التطبيق بواسطة
        /// </summary>
        public string AppliedBy { get; set; }

        /// <summary>
        /// Reverted Date - تاريخ التراجع
        /// </summary>
        public DateTime? RevertedDate { get; set; }

        /// <summary>
        /// Reverted By - تم التراجع بواسطة
        /// </summary>
        public string RevertedBy { get; set; }

        /// <summary>
        /// Revert Reason - سبب التراجع
        /// </summary>
        public string RevertReason { get; set; }

        #endregion

        #region Document Reference

        /// <summary>
        /// Document Reference - مرجع المستند
        /// </summary>
        public string DocumentReference { get; set; }

        /// <summary>
        /// Signed By Customer - موقع من العميل
        /// </summary>
        public bool SignedByCustomer { get; set; }

        /// <summary>
        /// Customer Signature Date - تاريخ توقيع العميل
        /// </summary>
        public DateTime? CustomerSignatureDate { get; set; }

        /// <summary>
        /// Customer Signatory Name - اسم موقع العميل
        /// </summary>
        public string CustomerSignatoryName { get; set; }

        #endregion

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        #region Audit Fields

        /// <summary>
        /// Created By - أنشئ بواسطة
        /// </summary>
        public string Create_by { get; set; }

        /// <summary>
        /// Updated By - تم التحديث بواسطة
        /// </summary>
        public string Update_by { get; set; }

        /// <summary>
        /// Updated Time - وقت التحديث
        /// </summary>
        public DateTime? Update_time { get; set; }

        #endregion
    }

    #endregion

    #region Action DTOs

    /// <summary>
    /// Approve Amendment DTO - موافقة على التعديل
    /// </summary>
    public class ApproveAmendmentDto
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        [Required(ErrorMessage = "Amendment ID is required / معرف التعديل مطلوب")]
        public Guid AmendmentId { get; set; }

        /// <summary>
        /// Approval Notes - ملاحظات الموافقة
        /// </summary>
        [StringLength(500)]
        public string ApprovalNotes { get; set; }

        /// <summary>
        /// Apply Immediately - تطبيق فوري
        /// If true, applies amendment immediately after approval
        /// </summary>
        public bool ApplyImmediately { get; set; } = true;
    }

    /// <summary>
    /// Reject Amendment DTO - رفض التعديل
    /// </summary>
    public class RejectAmendmentDto
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        [Required(ErrorMessage = "Amendment ID is required / معرف التعديل مطلوب")]
        public Guid AmendmentId { get; set; }

        /// <summary>
        /// Rejection Reason - سبب الرفض
        /// </summary>
        [Required(ErrorMessage = "Rejection reason is required / سبب الرفض مطلوب")]
        [StringLength(500)]
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// Apply Amendment DTO - تطبيق التعديل
    /// </summary>
    public class ApplyAmendmentDto
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        [Required(ErrorMessage = "Amendment ID is required / معرف التعديل مطلوب")]
        public Guid AmendmentId { get; set; }

        /// <summary>
        /// Recalculate Schedules Now - إعادة حساب الجداول الآن
        /// </summary>
        public bool RecalculateSchedulesNow { get; set; } = true;
    }

    /// <summary>
    /// Revert Amendment DTO - التراجع عن التعديل
    /// </summary>
    public class RevertAmendmentDto
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        [Required(ErrorMessage = "Amendment ID is required / معرف التعديل مطلوب")]
        public Guid AmendmentId { get; set; }

        /// <summary>
        /// Revert Reason - سبب التراجع
        /// </summary>
        [Required(ErrorMessage = "Revert reason is required / سبب التراجع مطلوب")]
        [StringLength(500)]
        public string RevertReason { get; set; }

        /// <summary>
        /// Recalculate Schedules - إعادة حساب الجداول
        /// </summary>
        public bool RecalculateSchedules { get; set; } = true;
    }

    /// <summary>
    /// Record Customer Signature DTO - تسجيل توقيع العميل
    /// </summary>
    public class RecordCustomerSignatureDto
    {
        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        [Required(ErrorMessage = "Amendment ID is required / معرف التعديل مطلوب")]
        public Guid AmendmentId { get; set; }

        /// <summary>
        /// Signature Date - تاريخ التوقيع
        /// </summary>
        [Required(ErrorMessage = "Signature date is required / تاريخ التوقيع مطلوب")]
        public DateTime SignatureDate { get; set; }

        /// <summary>
        /// Signatory Name - اسم الموقع
        /// </summary>
        [Required(ErrorMessage = "Signatory name is required / اسم الموقع مطلوب")]
        [StringLength(100)]
        public string SignatoryName { get; set; }
    }

    #endregion

    #region Summary DTOs

    /// <summary>
    /// Amendment Impact Preview DTO - معاينة أثر التعديل
    /// Shows what will change before applying amendment
    /// </summary>
    public class AmendmentImpactPreviewDto
    {
        /// <summary>
        /// Amendment Type - نوع التعديل
        /// </summary>
        public AmendmentType AmendmentType { get; set; }

        /// <summary>
        /// Amendment Type Name - اسم نوع التعديل
        /// </summary>
        public string AmendmentTypeName { get; set; }

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Monthly Impact - الأثر الشهري
        /// </summary>
        public decimal MonthlyImpact { get; set; }

        /// <summary>
        /// Total Impact - الأثر الإجمالي
        /// </summary>
        public decimal TotalImpact { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Schedules to Recalculate - الجداول لإعادة الحساب
        /// </summary>
        public int SchedulesToRecalculate { get; set; }

        /// <summary>
        /// Schedules to Cancel - الجداول للإلغاء
        /// </summary>
        public int SchedulesToCancel { get; set; }

        /// <summary>
        /// Schedules to Generate - الجداول للإنشاء
        /// </summary>
        public int SchedulesToGenerate { get; set; }

        /// <summary>
        /// Before Summary - ملخص قبل
        /// </summary>
        public ContractSummarySnapshot Before { get; set; }

        /// <summary>
        /// After Summary - ملخص بعد
        /// </summary>
        public ContractSummarySnapshot After { get; set; }

        /// <summary>
        /// Warnings - التحذيرات
        /// </summary>
        public List<string> Warnings { get; set; }
    }

    /// <summary>
    /// Contract Summary Snapshot DTO - لقطة ملخص العقد
    /// </summary>
    public class ContractSummarySnapshot
    {
        /// <summary>
        /// Base Amount - المبلغ الأساسي
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Total Units - إجمالي الوحدات
        /// </summary>
        public int TotalUnits { get; set; }

        /// <summary>
        /// Total Area - إجمالي المساحة
        /// </summary>
        public decimal TotalArea { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Remaining Months - الأشهر المتبقية
        /// </summary>
        public int RemainingMonths { get; set; }

        /// <summary>
        /// Total Contract Value - إجمالي قيمة العقد
        /// </summary>
        public decimal TotalContractValue { get; set; }
    }

    /// <summary>
    /// Amendment History Summary DTO - ملخص سجل التعديلات
    /// </summary>
    public class AmendmentHistorySummaryDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Total Amendments - إجمالي التعديلات
        /// </summary>
        public int TotalAmendments { get; set; }

        /// <summary>
        /// Approved Amendments - التعديلات الموافق عليها
        /// </summary>
        public int ApprovedAmendments { get; set; }

        /// <summary>
        /// Pending Amendments - التعديلات المعلقة
        /// </summary>
        public int PendingAmendments { get; set; }

        /// <summary>
        /// Rejected Amendments - التعديلات المرفوضة
        /// </summary>
        public int RejectedAmendments { get; set; }

        /// <summary>
        /// Reverted Amendments - التعديلات المتراجع عنها
        /// </summary>
        public int RevertedAmendments { get; set; }

        /// <summary>
        /// Total Price Increase - إجمالي زيادة السعر
        /// </summary>
        public decimal TotalPriceIncrease { get; set; }

        /// <summary>
        /// Total Price Decrease - إجمالي نقصان السعر
        /// </summary>
        public decimal TotalPriceDecrease { get; set; }

        /// <summary>
        /// Net Price Change - صافي تغيير السعر
        /// </summary>
        public decimal NetPriceChange { get; set; }

        /// <summary>
        /// By Type - حسب النوع
        /// </summary>
        public Dictionary<AmendmentType, int> ByType { get; set; }
    }

    #endregion
}
