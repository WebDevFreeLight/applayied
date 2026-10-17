using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    #region Query DTO

    /// <summary>
    /// Contract Query DTO - معايير البحث عن العقود
    /// Used for filtering and searching contracts
    /// </summary>
    public class ErpContractQueryDto : PagerInfo
    {
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
        /// Customer Name (Search) - اسم العميل
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Contract Type - نوع العقد
        /// </summary>
        public ContractType? ContractType { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public ContractStatus? Status { get; set; }

        /// <summary>
        /// Multiple Statuses - حالات متعددة
        /// </summary>
        public List<ContractStatus> Statuses { get; set; }

        /// <summary>
        /// Start Date From - تاريخ البداية من
        /// </summary>
        public DateTime? StartDateFrom { get; set; }

        /// <summary>
        /// Start Date To - تاريخ البداية إلى
        /// </summary>
        public DateTime? StartDateTo { get; set; }

        /// <summary>
        /// End Date From - تاريخ النهاية من
        /// </summary>
        public DateTime? EndDateFrom { get; set; }

        /// <summary>
        /// End Date To - تاريخ النهاية إلى
        /// </summary>
        public DateTime? EndDateTo { get; set; }

        /// <summary>
        /// Auto Renew - التجديد التلقائي
        /// </summary>
        public bool? AutoRenew { get; set; }

        /// <summary>
        /// Billing Frequency - تكرار الفوترة
        /// </summary>
        public BillingFrequency? BillingFrequency { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Expiring Within Days - تنتهي خلال أيام
        /// Filter contracts expiring within X days
        /// </summary>
        public int? ExpiringWithinDays { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public int? BranchId { get; set; }
    }

    #endregion

    #region Create DTO

    /// <summary>
    /// Create Contract DTO - بيانات إنشاء العقد
    /// Used for creating a new contract
    /// </summary>
    public class CreateErpContractDto
    {
        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        [Required(ErrorMessage = "Customer is required / العميل مطلوب")]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Contract Type - نوع العقد
        /// </summary>
        [Required(ErrorMessage = "Contract type is required / نوع العقد مطلوب")]
        public ContractType ContractType { get; set; }

        /// <summary>
        /// Contract Title - عنوان العقد
        /// </summary>
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters / العنوان لا يتجاوز 200 حرف")]
        public string ContractTitle { get; set; }

        /// <summary>
        /// Contract Title (Arabic) - عنوان العقد بالعربية
        /// </summary>
        [StringLength(200, ErrorMessage = "Arabic title cannot exceed 200 characters / العنوان العربي لا يتجاوز 200 حرف")]
        public string ContractTitleAr { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        [Required(ErrorMessage = "Start date is required / تاريخ البداية مطلوب")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Auto Renew - التجديد التلقائي
        /// </summary>
        public bool AutoRenew { get; set; } = false;

        /// <summary>
        /// Renewal Type - نوع التجديد
        /// </summary>
        public RenewalType RenewalType { get; set; } = RenewalType.Manual;

        /// <summary>
        /// Renewal Notice Days - أيام إشعار التجديد
        /// </summary>
        [Range(0, 365, ErrorMessage = "Renewal notice days must be between 0 and 365 / أيام الإشعار بين 0 و 365")]
        public int? RenewalNoticeDays { get; set; } = 30;

        /// <summary>
        /// Billing Frequency - تكرار الفوترة
        /// </summary>
        [Required(ErrorMessage = "Billing frequency is required / تكرار الفوترة مطلوب")]
        public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly;

        /// <summary>
        /// Billing Day - يوم الفوترة
        /// </summary>
        [Range(1, 31, ErrorMessage = "Billing day must be between 1 and 31 / يوم الفوترة بين 1 و 31")]
        public int? BillingDay { get; set; } = 1;

        /// <summary>
        /// Is Billing Variable - هل الفوترة متغيرة
        /// </summary>
        public bool IsBillingVariable { get; set; } = false;

        /// <summary>
        /// Base Amount - المبلغ الأساسي
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Base amount must be positive / المبلغ الأساسي يجب أن يكون موجباً")]
        public decimal? BaseAmount { get; set; }

        /// <summary>
        /// Total Contract Value - القيمة الإجمالية للعقد
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be positive / المبلغ الإجمالي يجب أن يكون موجباً")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [Required(ErrorMessage = "Currency is required / العملة مطلوبة")]
        [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters / رمز العملة لا يتجاوز 10 أحرف")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Payment Terms - شروط الدفع
        /// </summary>
        [StringLength(100, ErrorMessage = "Payment terms cannot exceed 100 characters / شروط الدفع لا تتجاوز 100 حرف")]
        public string PaymentTerms { get; set; }

        /// <summary>
        /// Payment Due Days - أيام استحقاق الدفع
        /// </summary>
        [Range(0, 365, ErrorMessage = "Payment due days must be between 0 and 365 / أيام الاستحقاق بين 0 و 365")]
        public int? PaymentDueDays { get; set; } = 30;

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// </summary>
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// </summary>
        public bool IsTaxInclusive { get; set; } = false;

        /// <summary>
        /// Security Deposit - التأمين
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Security deposit must be positive / التأمين يجب أن يكون موجباً")]
        public decimal? SecurityDeposit { get; set; }

        /// <summary>
        /// Signed Date - تاريخ التوقيع
        /// </summary>
        public DateTime? SignedDate { get; set; }

        /// <summary>
        /// Signatory Name - اسم الموقع
        /// </summary>
        [StringLength(100, ErrorMessage = "Signatory name cannot exceed 100 characters / اسم الموقع لا يتجاوز 100 حرف")]
        public string SignatoryName { get; set; }

        /// <summary>
        /// External Reference - المرجع الخارجي
        /// </summary>
        [StringLength(100, ErrorMessage = "External reference cannot exceed 100 characters / المرجع الخارجي لا يتجاوز 100 حرف")]
        public string ExternalReference { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters / الوصف لا يتجاوز 1000 حرف")]
        public string Description { get; set; }

        /// <summary>
        /// Terms and Conditions - الشروط والأحكام
        /// </summary>
        public string TermsConditions { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters / الملاحظات لا تتجاوز 1000 حرف")]
        public string Notes { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public int? BranchId { get; set; }
    }

    #endregion

    #region Update DTO

    /// <summary>
    /// Update Contract DTO - بيانات تحديث العقد
    /// Used for updating an existing contract
    /// </summary>
    public class UpdateErpContractDto : CreateErpContractDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }
    }

    #endregion

    #region List DTO

    /// <summary>
    /// Contract List DTO - عرض قائمة العقود
    /// Used for displaying contracts in list views
    /// </summary>
    public class ErpContractListDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        [ExcelColumn(Name = "Contract Number / رقم العقد")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        [ExcelColumn(Name = "Customer / العميل")]
        public string CustomerName { get; set; }

        /// <summary>
        /// Customer Name Arabic - اسم العميل بالعربية
        /// </summary>
        public string CustomerNameAr { get; set; }

        /// <summary>
        /// Contract Type - نوع العقد
        /// </summary>
        [ExcelColumn(Name = "Type / النوع")]
        public ContractType ContractType { get; set; }

        /// <summary>
        /// Contract Type Name - اسم نوع العقد
        /// </summary>
        public string ContractTypeName { get; set; }

        /// <summary>
        /// Contract Title - عنوان العقد
        /// </summary>
        [ExcelColumn(Name = "Title / العنوان")]
        public string ContractTitle { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        [ExcelColumn(Name = "Start Date / تاريخ البداية", Format = "yyyy-MM-dd")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        [ExcelColumn(Name = "End Date / تاريخ النهاية", Format = "yyyy-MM-dd")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [ExcelColumn(Name = "Status / الحالة")]
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Status Name - اسم الحالة
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Billing Frequency - تكرار الفوترة
        /// </summary>
        public BillingFrequency BillingFrequency { get; set; }

        /// <summary>
        /// Billing Frequency Name - اسم تكرار الفوترة
        /// </summary>
        public string BillingFrequencyName { get; set; }

        /// <summary>
        /// Base Amount - المبلغ الأساسي
        /// </summary>
        [ExcelColumn(Name = "Base Amount / المبلغ الأساسي", Format = "#,##0.00")]
        public decimal? BaseAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        [ExcelColumn(Name = "Total Amount / المبلغ الإجمالي", Format = "#,##0.00")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [ExcelColumn(Name = "Currency / العملة")]
        public string Currency { get; set; }

        /// <summary>
        /// Auto Renew - التجديد التلقائي
        /// </summary>
        public bool AutoRenew { get; set; }

        /// <summary>
        /// Next Billing Date - تاريخ الفاتورة القادمة
        /// </summary>
        [ExcelColumn(Name = "Next Billing / الفوترة القادمة", Format = "yyyy-MM-dd")]
        public DateTime? NextBillingDate { get; set; }

        /// <summary>
        /// Days Until Expiry - أيام حتى الانتهاء
        /// </summary>
        public int? DaysUntilExpiry { get; set; }

        /// <summary>
        /// Created Time - وقت الإنشاء
        /// </summary>
        public DateTime? Create_time { get; set; }
    }

    #endregion

    #region Detail DTO

    /// <summary>
    /// Contract Detail DTO - تفاصيل العقد
    /// Used for displaying full contract details
    /// </summary>
    public class ErpContractDetailDto : ErpContractListDto
    {
        /// <summary>
        /// Contract Title (Arabic) - عنوان العقد بالعربية
        /// </summary>
        public string ContractTitleAr { get; set; }

        /// <summary>
        /// Renewal Type - نوع التجديد
        /// </summary>
        public RenewalType RenewalType { get; set; }

        /// <summary>
        /// Renewal Type Name - اسم نوع التجديد
        /// </summary>
        public string RenewalTypeName { get; set; }

        /// <summary>
        /// Renewal Notice Days - أيام إشعار التجديد
        /// </summary>
        public int? RenewalNoticeDays { get; set; }

        /// <summary>
        /// Billing Day - يوم الفوترة
        /// </summary>
        public int? BillingDay { get; set; }

        /// <summary>
        /// Is Billing Variable - هل الفوترة متغيرة
        /// </summary>
        public bool IsBillingVariable { get; set; }

        /// <summary>
        /// Payment Terms - شروط الدفع
        /// </summary>
        public string PaymentTerms { get; set; }

        /// <summary>
        /// Payment Due Days - أيام استحقاق الدفع
        /// </summary>
        public int? PaymentDueDays { get; set; }

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// </summary>
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Tax Group Name - اسم مجموعة الضريبة
        /// </summary>
        public string TaxGroupName { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// </summary>
        public bool IsTaxInclusive { get; set; }

        /// <summary>
        /// Security Deposit - التأمين
        /// </summary>
        public decimal? SecurityDeposit { get; set; }

        /// <summary>
        /// Security Deposit Status - حالة التأمين
        /// </summary>
        public string SecurityDepositStatus { get; set; }

        /// <summary>
        /// Signed Date - تاريخ التوقيع
        /// </summary>
        public DateTime? SignedDate { get; set; }

        /// <summary>
        /// Signatory Name - اسم الموقع
        /// </summary>
        public string SignatoryName { get; set; }

        /// <summary>
        /// External Reference - المرجع الخارجي
        /// </summary>
        public string ExternalReference { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Terms and Conditions - الشروط والأحكام
        /// </summary>
        public string TermsConditions { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Last Billing Date - تاريخ آخر فاتورة
        /// </summary>
        public DateTime? LastBillingDate { get; set; }

        /// <summary>
        /// Renewal Count - عدد التجديدات
        /// </summary>
        public int RenewalCount { get; set; }

        /// <summary>
        /// Parent Contract ID - معرف العقد الأصلي
        /// </summary>
        public Guid? ParentContractId { get; set; }

        /// <summary>
        /// Parent Contract Number - رقم العقد الأصلي
        /// </summary>
        public string ParentContractNumber { get; set; }

        /// <summary>
        /// Activated Date - تاريخ التفعيل
        /// </summary>
        public DateTime? ActivatedDate { get; set; }

        /// <summary>
        /// Activated By - تم التفعيل بواسطة
        /// </summary>
        public string ActivatedBy { get; set; }

        /// <summary>
        /// Terminated Date - تاريخ الإنهاء
        /// </summary>
        public DateTime? TerminatedDate { get; set; }

        /// <summary>
        /// Terminated By - تم الإنهاء بواسطة
        /// </summary>
        public string TerminatedBy { get; set; }

        /// <summary>
        /// Termination Reason - سبب الإنهاء
        /// </summary>
        public TerminationReason? TerminationReason { get; set; }

        /// <summary>
        /// Termination Reason Name - اسم سبب الإنهاء
        /// </summary>
        public string TerminationReasonName { get; set; }

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

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyID { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public int? BranchID { get; set; }

        #region Related Data - البيانات المرتبطة

        /// <summary>
        /// Units - الوحدات
        /// </summary>
        public List<ErpContractUnitListDto> Units { get; set; }

        /// <summary>
        /// Pricing Rules - قواعد التسعير
        /// </summary>
        public List<ErpContractPricingRuleListDto> PricingRules { get; set; }

        /// <summary>
        /// Upcoming Billing Schedules - جداول الفوترة القادمة
        /// </summary>
        public List<ErpContractBillingScheduleListDto> UpcomingBillingSchedules { get; set; }

        /// <summary>
        /// Recent Amendments - التعديلات الأخيرة
        /// </summary>
        public List<ErpContractAmendmentListDto> RecentAmendments { get; set; }

        #endregion

        #region Summary Statistics - إحصائيات ملخصة

        /// <summary>
        /// Total Units - إجمالي الوحدات
        /// </summary>
        public int TotalUnits { get; set; }

        /// <summary>
        /// Total Invoiced Amount - إجمالي المبالغ المفوترة
        /// </summary>
        public decimal TotalInvoicedAmount { get; set; }

        /// <summary>
        /// Total Paid Amount - إجمالي المبالغ المدفوعة
        /// </summary>
        public decimal TotalPaidAmount { get; set; }

        /// <summary>
        /// Outstanding Amount - المبلغ المستحق
        /// </summary>
        public decimal OutstandingAmount { get; set; }

        /// <summary>
        /// Pending Invoices Count - عدد الفواتير المعلقة
        /// </summary>
        public int PendingInvoicesCount { get; set; }

        #endregion
    }

    #endregion

    #region Select DTO

    /// <summary>
    /// Contract Select DTO - اختيار العقد
    /// Used for dropdown/selection lists
    /// </summary>
    public class ErpContractSelectDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Contract Title - عنوان العقد
        /// </summary>
        public string ContractTitle { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Display Text - نص العرض
        /// Combines contract number and customer for display
        /// </summary>
        public string DisplayText => $"{ContractNumber} - {CustomerName}";
    }

    #endregion

    #region Status Change DTOs

    /// <summary>
    /// Activate Contract DTO - تفعيل العقد
    /// </summary>
    public class ActivateContractDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Activation Date - تاريخ التفعيل
        /// </summary>
        public DateTime? ActivationDate { get; set; }

        /// <summary>
        /// Generate Billing Schedule - إنشاء جدول الفوترة
        /// Whether to automatically generate billing schedule on activation
        /// </summary>
        public bool GenerateBillingSchedule { get; set; } = true;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Suspend Contract DTO - تعليق العقد
    /// </summary>
    public class SuspendContractDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Suspension Reason - سبب التعليق
        /// </summary>
        [Required(ErrorMessage = "Suspension reason is required / سبب التعليق مطلوب")]
        [StringLength(500)]
        public string SuspensionReason { get; set; }

        /// <summary>
        /// Suspension Date - تاريخ التعليق
        /// </summary>
        public DateTime? SuspensionDate { get; set; }

        /// <summary>
        /// Cancel Pending Schedules - إلغاء الجداول المعلقة
        /// Whether to cancel pending billing schedules
        /// </summary>
        public bool CancelPendingSchedules { get; set; } = false;
    }

    /// <summary>
    /// Terminate Contract DTO - إنهاء العقد
    /// </summary>
    public class TerminateContractDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Termination Reason - سبب الإنهاء
        /// </summary>
        [Required(ErrorMessage = "Termination reason is required / سبب الإنهاء مطلوب")]
        public TerminationReason TerminationReason { get; set; }

        /// <summary>
        /// Termination Date - تاريخ الإنهاء
        /// </summary>
        [Required(ErrorMessage = "Termination date is required / تاريخ الإنهاء مطلوب")]
        public DateTime TerminationDate { get; set; }

        /// <summary>
        /// Termination Notes - ملاحظات الإنهاء
        /// </summary>
        [StringLength(1000)]
        public string TerminationNotes { get; set; }

        /// <summary>
        /// Refund Security Deposit - استرداد التأمين
        /// Whether to refund the security deposit
        /// </summary>
        public bool RefundSecurityDeposit { get; set; } = true;

        /// <summary>
        /// Security Deposit Deduction - خصم من التأمين
        /// Amount to deduct from security deposit
        /// </summary>
        public decimal? SecurityDepositDeduction { get; set; }

        /// <summary>
        /// Cancel Future Schedules - إلغاء الجداول المستقبلية
        /// Whether to cancel all future billing schedules
        /// </summary>
        public bool CancelFutureSchedules { get; set; } = true;

        /// <summary>
        /// Generate Final Invoice - إنشاء الفاتورة النهائية
        /// Whether to generate a final invoice for any outstanding amounts
        /// </summary>
        public bool GenerateFinalInvoice { get; set; } = false;
    }

    #endregion

    #region Contract Unit DTOs

    /// <summary>
    /// Contract Unit List DTO - قائمة وحدات العقد
    /// </summary>
    public class ErpContractUnitListDto
    {
        public Guid UnitId { get; set; }
        public Guid ContractId { get; set; }
        public string UnitCode { get; set; }
        public ContractUnitType UnitType { get; set; }
        public string UnitTypeName { get; set; }
        public string UnitName { get; set; }
        public string UnitNameAr { get; set; }
        public string Description { get; set; }
        public decimal? Area { get; set; }
        public string AreaUnit { get; set; }
        public decimal? BasePrice { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Create_time { get; set; }
        public long? BlvShopId { get; set; }
        public PricingType PricingType { get; set; }
        public bool PricingLocked { get; set; }
    }

    #endregion

    #region Contract Pricing Rule DTOs

    /// <summary>
    /// Contract Pricing Rule List DTO - قائمة قواعد التسعير
    /// </summary>
    public class ErpContractPricingRuleListDto
    {
        public Guid PricingRuleId { get; set; }
        public Guid ContractId { get; set; }
        public PricingRuleType RuleType { get; set; }
        public string RuleTypeName { get; set; }
        public string RuleName { get; set; }
        public string RuleNameAr { get; set; }
        public decimal Value { get; set; }
        public string ValueType { get; set; }
        public int Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region Contract Billing Schedule DTOs

    /// <summary>
    /// Contract Billing Schedule List DTO - قائمة جداول الفوترة
    /// </summary>
    public class ErpContractBillingScheduleListDto
    {
        public Guid ScheduleId { get; set; }
        public int ScheduleNumber { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public BillingScheduleStatus Status { get; set; }
        public string StatusName { get; set; }
        public Guid? InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Generate Billing Schedule DTO - إنشاء جدول الفوترة
    /// </summary>
    public class GenerateBillingScheduleDto
    {
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Optional start date. If not provided, uses contract start date.
        /// تاريخ البداية (اختياري). إذا لم يتم تحديده، يستخدم تاريخ بداية العقد.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date. If not provided, uses contract end date.
        /// تاريخ النهاية (اختياري). إذا لم يتم تحديده، يستخدم تاريخ نهاية العقد.
        /// </summary>
        public DateTime? EndDate { get; set; }

        public decimal? BaseAmount { get; set; }

        public bool RegeneratePending { get; set; } = false;
    }

    /// <summary>
    /// Update Billing Schedule DTO - تحديث جدول الفوترة
    /// </summary>
    public class UpdateBillingScheduleDto
    {
        [Required(ErrorMessage = "Schedule ID is required / معرف الجدول مطلوب")]
        public Guid ScheduleId { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public string Notes { get; set; }
    }

    #endregion

    #region Contract Amendment DTOs

    /// <summary>
    /// Contract Amendment List DTO - قائمة التعديلات
    /// </summary>
    public class ErpContractAmendmentListDto
    {
        public Guid AmendmentId { get; set; }
        public AmendmentType AmendmentType { get; set; }
        public string AmendmentTypeName { get; set; }
        public AmendmentStatus Status { get; set; }
        public string StatusName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Create Contract Amendment DTO - إنشاء تعديل العقد
    /// </summary>
    public class CreateErpContractAmendmentDto
    {
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        [Required(ErrorMessage = "Amendment type is required / نوع التعديل مطلوب")]
        public AmendmentType AmendmentType { get; set; }

        [StringLength(200)]
        public string AmendmentTitle { get; set; }

        [StringLength(200)]
        public string AmendmentTitleAr { get; set; }

        [Required(ErrorMessage = "Effective date is required / تاريخ السريان مطلوب")]
        public DateTime EffectiveDate { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        public string PreviousValue { get; set; }

        public string NewValue { get; set; }

        public decimal? PreviousAmount { get; set; }

        public decimal? NewAmount { get; set; }

        public DateTime? PreviousEndDate { get; set; }

        public DateTime? NewEndDate { get; set; }

        public Guid? UnitId { get; set; }

        public bool RecalculateSchedules { get; set; } = true;

        public bool RequiresApproval { get; set; } = true;

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update Contract Amendment DTO - تحديث تعديل العقد
    /// </summary>
    public class UpdateErpContractAmendmentDto : CreateErpContractAmendmentDto
    {
        [Required(ErrorMessage = "Amendment ID is required / معرف التعديل مطلوب")]
        public Guid AmendmentId { get; set; }
    }

    #endregion

    #region Statistics DTOs

    /// <summary>
    /// Contract Statistics DTO - إحصائيات العقود
    /// </summary>
    public class ContractStatisticsDto
    {
        /// <summary>
        /// Total Contracts - إجمالي العقود
        /// </summary>
        public int TotalContracts { get; set; }

        /// <summary>
        /// Active Contracts - العقود النشطة
        /// </summary>
        public int ActiveContracts { get; set; }

        /// <summary>
        /// Draft Contracts - العقود المسودة
        /// </summary>
        public int DraftContracts { get; set; }

        /// <summary>
        /// Suspended Contracts - العقود المعلقة
        /// </summary>
        public int SuspendedContracts { get; set; }

        /// <summary>
        /// Expired Contracts - العقود المنتهية
        /// </summary>
        public int ExpiredContracts { get; set; }

        /// <summary>
        /// Total Value - القيمة الإجمالية
        /// </summary>
        public decimal TotalValue { get; set; }
    }

    /// <summary>
    /// Billing Schedule Statistics DTO - إحصائيات جدول الفوترة
    /// </summary>
    public class BillingScheduleStatisticsDto
    {
        /// <summary>
        /// Total Count - إجمالي العدد
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Pending Count - العدد المعلق
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Invoiced Count - العدد المفوتر
        /// </summary>
        public int InvoicedCount { get; set; }

        /// <summary>
        /// Paid Count - العدد المدفوع
        /// </summary>
        public int PaidCount { get; set; }

        /// <summary>
        /// Overdue Count - العدد المتأخر
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// Due Count - العدد المستحق
        /// </summary>
        public int DueCount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Pending Amount - المبلغ المعلق
        /// </summary>
        public decimal PendingAmount { get; set; }
    }

    #endregion

    #region Contract Update Result DTO

    /// <summary>
    /// Contract Update Result DTO - نتيجة تحديث العقد
    /// Returned when updating a contract to provide pricing change warnings
    /// </summary>
    public class ContractUpdateResultDto
    {
        public bool Success { get; set; }
        public int InvoicedSchedulesKept { get; set; }
        public int PendingSchedulesRegenerated { get; set; }
        public string Warning { get; set; }
        public Guid? AmendmentId { get; set; }
    }

    /// <summary>
    /// Schedule Impact DTO - تأثير التغيير على الجداول
    /// Shows how many schedules would be affected by a pricing change
    /// </summary>
    public class ScheduleImpactDto
    {
        public bool HasInvoicedSchedules { get; set; }
        public int InvoicedCount { get; set; }
        public int PendingCount { get; set; }
        public int PaidCount { get; set; }
        public int DueCount { get; set; }
    }

    /// <summary>
    /// Unit Price Update Result DTO - نتيجة تحديث سعر الوحدة
    /// Returned when updating a unit price on an active contract
    /// </summary>
    public class UnitPriceUpdateResultDto
    {
        public bool Success { get; set; }
        public bool PriceChanged { get; set; }
        public decimal OldUnitPrice { get; set; }
        public decimal NewUnitPrice { get; set; }
        public decimal OldContractBaseAmount { get; set; }
        public decimal NewContractBaseAmount { get; set; }
        public decimal AlreadyCollected { get; set; }
        public decimal RemainingToCollect { get; set; }
        public int SchedulesRecalculated { get; set; }
        public decimal NewInstallmentAmount { get; set; }
        public string Warning { get; set; }
        public Guid? AmendmentId { get; set; }
    }

    #endregion

    #region Shop Contract Info DTO

    /// <summary>
    /// Shop Contract Info DTO - معلومات عقد المحل
    /// Lightweight DTO for boulevard reports: maps shop → contract → customer
    /// </summary>
    public class ShopContractInfoDto
    {
        public long BlvShopId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractTitle { get; set; }
        public int ContractStatus { get; set; }
        public string CustomerNameAr { get; set; }
        public string CustomerNameEn { get; set; }
        public string BrandName { get; set; }
    }

    #endregion
}
