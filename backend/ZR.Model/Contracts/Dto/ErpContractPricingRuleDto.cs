using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    #region Query DTO

    /// <summary>
    /// Contract Pricing Rule Query DTO - معايير البحث عن قواعد التسعير
    /// Used for filtering and searching pricing rules
    /// </summary>
    public class ErpContractPricingRuleQueryDto : PagerInfo
    {
        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        public Guid? PricingRuleId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Rule Type - نوع القاعدة
        /// </summary>
        public PricingRuleType? RuleType { get; set; }

        /// <summary>
        /// Multiple Rule Types - أنواع قواعد متعددة
        /// </summary>
        public List<PricingRuleType> RuleTypes { get; set; }

        /// <summary>
        /// Rule Name - اسم القاعدة
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Value Type - نوع القيمة
        /// </summary>
        public string ValueType { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Effective On Date - ساري بتاريخ
        /// Find rules effective on this date
        /// </summary>
        public DateTime? EffectiveOnDate { get; set; }

        /// <summary>
        /// Start Date From - تاريخ البداية من
        /// </summary>
        public DateTime? StartDateFrom { get; set; }

        /// <summary>
        /// Start Date To - تاريخ البداية إلى
        /// </summary>
        public DateTime? StartDateTo { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool? RequiresApproval { get; set; }

        /// <summary>
        /// Is Approved - تمت الموافقة
        /// </summary>
        public bool? IsApproved { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyId { get; set; }
    }

    #endregion

    #region Create DTO

    /// <summary>
    /// Create Contract Pricing Rule DTO - بيانات إنشاء قاعدة التسعير
    /// Used for creating a new pricing rule
    /// </summary>
    public class CreateErpContractPricingRuleDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// Optional: If set, rule applies only to this unit
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Rule Name - اسم القاعدة
        /// </summary>
        [StringLength(200, ErrorMessage = "Rule name cannot exceed 200 characters / اسم القاعدة لا يتجاوز 200 حرف")]
        public string RuleName { get; set; }

        /// <summary>
        /// Rule Name (Arabic) - اسم القاعدة بالعربية
        /// </summary>
        [StringLength(200, ErrorMessage = "Arabic rule name cannot exceed 200 characters / اسم القاعدة بالعربية لا يتجاوز 200 حرف")]
        public string RuleNameAr { get; set; }

        /// <summary>
        /// Rule Type - نوع القاعدة
        /// </summary>
        [Required(ErrorMessage = "Rule type is required / نوع القاعدة مطلوب")]
        public PricingRuleType RuleType { get; set; }

        /// <summary>
        /// Value - القيمة
        /// </summary>
        [Required(ErrorMessage = "Value is required / القيمة مطلوبة")]
        public decimal Value { get; set; }

        /// <summary>
        /// Value Type - نوع القيمة
        /// </summary>
        [Required(ErrorMessage = "Value type is required / نوع القيمة مطلوب")]
        [StringLength(20)]
        public string ValueType { get; set; } = "Amount"; // Amount, Percentage, PerUnit

        /// <summary>
        /// Minimum Value - الحد الأدنى
        /// </summary>
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Maximum Value - الحد الأقصى
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Priority - الأولوية
        /// </summary>
        [Range(1, 999, ErrorMessage = "Priority must be between 1 and 999 / الأولوية بين 1 و 999")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Is Compounding - تراكمي
        /// </summary>
        public bool IsCompounding { get; set; } = false;

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [StringLength(10)]
        public string Currency { get; set; }

        #region Escalation Fields

        /// <summary>
        /// Escalation Frequency - تكرار التصاعد
        /// </summary>
        [StringLength(20)]
        public string EscalationFrequency { get; set; }

        /// <summary>
        /// Escalation Base Date - تاريخ أساس التصاعد
        /// </summary>
        public DateTime? EscalationBaseDate { get; set; }

        /// <summary>
        /// Escalation Cap Percentage - سقف نسبة التصاعد
        /// </summary>
        [Range(0, 100, ErrorMessage = "Escalation cap must be between 0 and 100% / سقف التصاعد بين 0 و 100%")]
        public decimal? EscalationCapPercentage { get; set; }

        #endregion

        #region Seasonal Fields

        /// <summary>
        /// Season Name - اسم الموسم
        /// </summary>
        [StringLength(100)]
        public string SeasonName { get; set; }

        /// <summary>
        /// Recurrence Pattern - نمط التكرار
        /// </summary>
        [StringLength(50)]
        public string RecurrencePattern { get; set; }

        /// <summary>
        /// Month Start - شهر البداية
        /// </summary>
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12 / الشهر بين 1 و 12")]
        public int? MonthStart { get; set; }

        /// <summary>
        /// Day Start - يوم البداية
        /// </summary>
        [Range(1, 31, ErrorMessage = "Day must be between 1 and 31 / اليوم بين 1 و 31")]
        public int? DayStart { get; set; }

        /// <summary>
        /// Month End - شهر النهاية
        /// </summary>
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12 / الشهر بين 1 و 12")]
        public int? MonthEnd { get; set; }

        /// <summary>
        /// Day End - يوم النهاية
        /// </summary>
        [Range(1, 31, ErrorMessage = "Day must be between 1 and 31 / اليوم بين 1 و 31")]
        public int? DayEnd { get; set; }

        #endregion

        #region Tiered Pricing Fields

        /// <summary>
        /// Tier From - من المستوى
        /// </summary>
        public decimal? TierFrom { get; set; }

        /// <summary>
        /// Tier To - إلى المستوى
        /// </summary>
        public decimal? TierTo { get; set; }

        /// <summary>
        /// Tier Unit - وحدة المستوى
        /// </summary>
        [StringLength(50)]
        public string TierUnit { get; set; }

        #endregion

        #region Discount Conditions

        /// <summary>
        /// Discount Condition - شرط الخصم
        /// </summary>
        [StringLength(100)]
        public string DiscountCondition { get; set; }

        /// <summary>
        /// Discount Condition Value - قيمة شرط الخصم
        /// </summary>
        public decimal? DiscountConditionValue { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool RequiresApproval { get; set; } = false;

        #endregion

        #region GL Integration

        /// <summary>
        /// Revenue Account ID - معرف حساب الإيرادات
        /// </summary>
        public Guid? RevenueAccountId { get; set; }

        /// <summary>
        /// Show On Invoice - إظهار في الفاتورة
        /// </summary>
        public bool ShowOnInvoice { get; set; } = true;

        /// <summary>
        /// Invoice Line Description - وصف بند الفاتورة
        /// </summary>
        [StringLength(200)]
        public string InvoiceLineDescription { get; set; }

        /// <summary>
        /// Invoice Line Description (Arabic) - وصف بند الفاتورة بالعربية
        /// </summary>
        [StringLength(200)]
        public string InvoiceLineDescriptionAr { get; set; }

        #endregion

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }
    }

    #endregion

    #region Update DTO

    /// <summary>
    /// Update Contract Pricing Rule DTO - بيانات تحديث قاعدة التسعير
    /// Used for updating an existing pricing rule
    /// </summary>
    public class UpdateErpContractPricingRuleDto : CreateErpContractPricingRuleDto
    {
        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        [Required(ErrorMessage = "Pricing Rule ID is required / معرف قاعدة التسعير مطلوب")]
        public Guid PricingRuleId { get; set; }
    }

    #endregion

    #region List DTO (Full Implementation)

    /// <summary>
    /// Contract Pricing Rule List DTO - قائمة قواعد التسعير
    /// Used for displaying pricing rules in list views
    /// </summary>
    public class ErpContractPricingRuleFullListDto
    {
        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        public Guid PricingRuleId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Rule Name - اسم القاعدة
        /// </summary>
        [ExcelColumn(Name = "Rule Name / اسم القاعدة")]
        public string RuleName { get; set; }

        /// <summary>
        /// Rule Name (Arabic) - اسم القاعدة بالعربية
        /// </summary>
        public string RuleNameAr { get; set; }

        /// <summary>
        /// Rule Type - نوع القاعدة
        /// </summary>
        [ExcelColumn(Name = "Type / النوع")]
        public PricingRuleType RuleType { get; set; }

        /// <summary>
        /// Rule Type Name - اسم نوع القاعدة
        /// </summary>
        public string RuleTypeName { get; set; }

        /// <summary>
        /// Value - القيمة
        /// </summary>
        [ExcelColumn(Name = "Value / القيمة", Format = "#,##0.00")]
        public decimal Value { get; set; }

        /// <summary>
        /// Value Type - نوع القيمة
        /// </summary>
        [ExcelColumn(Name = "Value Type / نوع القيمة")]
        public string ValueType { get; set; }

        /// <summary>
        /// Formatted Value - القيمة المنسقة
        /// Value with appropriate suffix (%, SAR, etc.)
        /// </summary>
        public string FormattedValue
        {
            get
            {
                return ValueType?.ToLower() switch
                {
                    "percentage" => $"{Value}%",
                    "perunit" => $"{Value}/unit",
                    _ => $"{Value:N2} {Currency ?? "SAR"}"
                };
            }
        }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        [ExcelColumn(Name = "Start Date / تاريخ البداية", Format = "yyyy-MM-dd")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        [ExcelColumn(Name = "End Date / تاريخ النهاية", Format = "yyyy-MM-dd")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [ExcelColumn(Name = "Active / نشط")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Priority - الأولوية
        /// </summary>
        [ExcelColumn(Name = "Priority / الأولوية")]
        public int Priority { get; set; }

        /// <summary>
        /// Is Compounding - تراكمي
        /// </summary>
        public bool IsCompounding { get; set; }

        /// <summary>
        /// Show On Invoice - إظهار في الفاتورة
        /// </summary>
        public bool ShowOnInvoice { get; set; }

        /// <summary>
        /// Is Effective Today - ساري اليوم
        /// </summary>
        public bool IsEffectiveToday
        {
            get
            {
                var today = DateTime.Today;
                return IsActive &&
                       (!StartDate.HasValue || StartDate.Value <= today) &&
                       (!EndDate.HasValue || EndDate.Value >= today);
            }
        }

        /// <summary>
        /// Calculated Amount - المبلغ المحسوب
        /// </summary>
        [ExcelColumn(Name = "Calculated / المحسوب", Format = "#,##0.00")]
        public decimal? CalculatedAmount { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Is Approved - تمت الموافقة
        /// </summary>
        public bool IsApproved => !RequiresApproval || ApprovedDate.HasValue;

        /// <summary>
        /// Approved Date - تاريخ الموافقة
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Created Time - وقت الإنشاء
        /// </summary>
        public DateTime? Create_time { get; set; }
    }

    #endregion

    #region Detail DTO

    /// <summary>
    /// Contract Pricing Rule Detail DTO - تفاصيل قاعدة التسعير
    /// Used for displaying full rule details
    /// </summary>
    public class ErpContractPricingRuleDetailDto : ErpContractPricingRuleFullListDto
    {
        /// <summary>
        /// Minimum Value - الحد الأدنى
        /// </summary>
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Maximum Value - الحد الأقصى
        /// </summary>
        public decimal? MaxValue { get; set; }

        #region Escalation Fields

        /// <summary>
        /// Escalation Frequency - تكرار التصاعد
        /// </summary>
        public string EscalationFrequency { get; set; }

        /// <summary>
        /// Escalation Base Date - تاريخ أساس التصاعد
        /// </summary>
        public DateTime? EscalationBaseDate { get; set; }

        /// <summary>
        /// Escalation Cap Percentage - سقف نسبة التصاعد
        /// </summary>
        public decimal? EscalationCapPercentage { get; set; }

        /// <summary>
        /// Escalation Applied Count - عدد مرات التصاعد المطبقة
        /// </summary>
        public int? EscalationAppliedCount { get; set; }

        /// <summary>
        /// Last Escalation Date - تاريخ آخر تصاعد
        /// </summary>
        public DateTime? LastEscalationDate { get; set; }

        /// <summary>
        /// Current Escalated Value - القيمة المتصاعدة الحالية
        /// </summary>
        public decimal? CurrentEscalatedValue { get; set; }

        #endregion

        #region Seasonal Fields

        /// <summary>
        /// Season Name - اسم الموسم
        /// </summary>
        public string SeasonName { get; set; }

        /// <summary>
        /// Recurrence Pattern - نمط التكرار
        /// </summary>
        public string RecurrencePattern { get; set; }

        /// <summary>
        /// Month Start - شهر البداية
        /// </summary>
        public int? MonthStart { get; set; }

        /// <summary>
        /// Day Start - يوم البداية
        /// </summary>
        public int? DayStart { get; set; }

        /// <summary>
        /// Month End - شهر النهاية
        /// </summary>
        public int? MonthEnd { get; set; }

        /// <summary>
        /// Day End - يوم النهاية
        /// </summary>
        public int? DayEnd { get; set; }

        /// <summary>
        /// Seasonal Period Display - عرض الفترة الموسمية
        /// </summary>
        public string SeasonalPeriodDisplay
        {
            get
            {
                if (!MonthStart.HasValue || !MonthEnd.HasValue) return null;
                return $"{MonthStart}/{DayStart ?? 1} - {MonthEnd}/{DayEnd ?? 28}";
            }
        }

        #endregion

        #region Tiered Pricing Fields

        /// <summary>
        /// Tier From - من المستوى
        /// </summary>
        public decimal? TierFrom { get; set; }

        /// <summary>
        /// Tier To - إلى المستوى
        /// </summary>
        public decimal? TierTo { get; set; }

        /// <summary>
        /// Tier Unit - وحدة المستوى
        /// </summary>
        public string TierUnit { get; set; }

        /// <summary>
        /// Tier Display - عرض المستوى
        /// </summary>
        public string TierDisplay
        {
            get
            {
                if (!TierFrom.HasValue && !TierTo.HasValue) return null;
                return $"{TierFrom ?? 0} - {(TierTo.HasValue ? TierTo.Value.ToString() : "∞")} {TierUnit}";
            }
        }

        #endregion

        #region Discount Conditions

        /// <summary>
        /// Discount Condition - شرط الخصم
        /// </summary>
        public string DiscountCondition { get; set; }

        /// <summary>
        /// Discount Condition Value - قيمة شرط الخصم
        /// </summary>
        public decimal? DiscountConditionValue { get; set; }

        /// <summary>
        /// Approved By - تمت الموافقة بواسطة
        /// </summary>
        public string ApprovedBy { get; set; }

        #endregion

        #region GL Integration

        /// <summary>
        /// Revenue Account ID - معرف حساب الإيرادات
        /// </summary>
        public Guid? RevenueAccountId { get; set; }

        /// <summary>
        /// Revenue Account Name - اسم حساب الإيرادات
        /// </summary>
        public string RevenueAccountName { get; set; }

        /// <summary>
        /// Invoice Line Description - وصف بند الفاتورة
        /// </summary>
        public string InvoiceLineDescription { get; set; }

        /// <summary>
        /// Invoice Line Description (Arabic) - وصف بند الفاتورة بالعربية
        /// </summary>
        public string InvoiceLineDescriptionAr { get; set; }

        #endregion

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        public Guid? AmendmentId { get; set; }

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

    #region Approval DTO

    /// <summary>
    /// Approve Pricing Rule DTO - موافقة على قاعدة التسعير
    /// </summary>
    public class ApprovePricingRuleDto
    {
        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        [Required(ErrorMessage = "Pricing Rule ID is required / معرف قاعدة التسعير مطلوب")]
        public Guid PricingRuleId { get; set; }

        /// <summary>
        /// Approval Notes - ملاحظات الموافقة
        /// </summary>
        [StringLength(500)]
        public string ApprovalNotes { get; set; }
    }

    #endregion

    #region Calculation DTOs

    /// <summary>
    /// Pricing Calculation Request DTO - طلب حساب التسعير
    /// Used to calculate pricing for a contract on a specific date
    /// </summary>
    public class PricingCalculationRequestDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Calculation Date - تاريخ الحساب
        /// </summary>
        [Required(ErrorMessage = "Calculation date is required / تاريخ الحساب مطلوب")]
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// Optional: Calculate for specific unit only
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Include Inactive Rules - تضمين القواعد غير النشطة
        /// </summary>
        public bool IncludeInactiveRules { get; set; } = false;
    }

    /// <summary>
    /// Pricing Calculation Result DTO - نتيجة حساب التسعير
    /// </summary>
    public class PricingCalculationResultDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Calculation Date - تاريخ الحساب
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Base Amount - المبلغ الأساسي
        /// Sum of all unit prices (or contract base amount)
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Adjustments - التعديلات
        /// List of applied pricing rules with their amounts
        /// </summary>
        public List<PricingAdjustmentDto> Adjustments { get; set; }

        /// <summary>
        /// Total Adjustments - إجمالي التعديلات
        /// </summary>
        public decimal TotalAdjustments { get; set; }

        /// <summary>
        /// Net Amount (Before Tax) - المبلغ الصافي (قبل الضريبة)
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Breakdown By Unit - التفصيل حسب الوحدة
        /// </summary>
        public List<UnitPricingBreakdownDto> UnitBreakdown { get; set; }
    }

    /// <summary>
    /// Pricing Adjustment DTO - تعديل التسعير
    /// Represents a single pricing rule application
    /// </summary>
    public class PricingAdjustmentDto
    {
        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        public Guid PricingRuleId { get; set; }

        /// <summary>
        /// Rule Name - اسم القاعدة
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Rule Type - نوع القاعدة
        /// </summary>
        public PricingRuleType RuleType { get; set; }

        /// <summary>
        /// Applied Value - القيمة المطبقة
        /// </summary>
        public decimal AppliedValue { get; set; }

        /// <summary>
        /// Value Type - نوع القيمة
        /// </summary>
        public string ValueType { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// Calculated amount (positive for additions, negative for discounts)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Is Addition - إضافة
        /// True if this adds to the total, false if it reduces
        /// </summary>
        public bool IsAddition { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Priority - الأولوية
        /// </summary>
        public int Priority { get; set; }
    }

    /// <summary>
    /// Unit Pricing Breakdown DTO - تفصيل تسعير الوحدة
    /// </summary>
    public class UnitPricingBreakdownDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid UnitId { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Unit Name - اسم الوحدة
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Base Price - السعر الأساسي
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Adjustments - التعديلات
        /// </summary>
        public List<PricingAdjustmentDto> Adjustments { get; set; }

        /// <summary>
        /// Net Price - السعر الصافي
        /// </summary>
        public decimal NetPrice { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Price - السعر الإجمالي
        /// </summary>
        public decimal TotalPrice { get; set; }
    }

    #endregion

    #region Bulk Operations DTOs

    /// <summary>
    /// Bulk Add Pricing Rules DTO - إضافة قواعد تسعير متعددة
    /// </summary>
    public class BulkAddPricingRulesDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Rules - القواعد
        /// </summary>
        [Required(ErrorMessage = "At least one rule is required / يجب إضافة قاعدة واحدة على الأقل")]
        [MinLength(1)]
        public List<CreateErpContractPricingRuleDto> Rules { get; set; }
    }

    /// <summary>
    /// Copy Pricing Rules DTO - نسخ قواعد التسعير
    /// Copy pricing rules from one contract to another
    /// </summary>
    public class CopyPricingRulesDto
    {
        /// <summary>
        /// Source Contract ID - معرف العقد المصدر
        /// </summary>
        [Required(ErrorMessage = "Source contract ID is required / معرف العقد المصدر مطلوب")]
        public Guid SourceContractId { get; set; }

        /// <summary>
        /// Target Contract ID - معرف العقد الهدف
        /// </summary>
        [Required(ErrorMessage = "Target contract ID is required / معرف العقد الهدف مطلوب")]
        public Guid TargetContractId { get; set; }

        /// <summary>
        /// Rule Types to Copy - أنواع القواعد للنسخ
        /// If null, copies all rules
        /// </summary>
        public List<PricingRuleType> RuleTypesToCopy { get; set; }

        /// <summary>
        /// Adjust Dates - تعديل التواريخ
        /// Whether to adjust rule dates based on contract dates
        /// </summary>
        public bool AdjustDates { get; set; } = true;
    }

    #endregion

    #region Summary DTO

    /// <summary>
    /// Contract Pricing Summary DTO - ملخص تسعير العقد
    /// </summary>
    public class ContractPricingSummaryDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Total Rules - إجمالي القواعد
        /// </summary>
        public int TotalRules { get; set; }

        /// <summary>
        /// Active Rules - القواعد النشطة
        /// </summary>
        public int ActiveRules { get; set; }

        /// <summary>
        /// Rules By Type - القواعد حسب النوع
        /// </summary>
        public Dictionary<PricingRuleType, int> RulesByType { get; set; }

        /// <summary>
        /// Has Escalation - يوجد تصاعد
        /// </summary>
        public bool HasEscalation { get; set; }

        /// <summary>
        /// Has Seasonal Pricing - يوجد تسعير موسمي
        /// </summary>
        public bool HasSeasonalPricing { get; set; }

        /// <summary>
        /// Total Fixed Additions - إجمالي الإضافات الثابتة
        /// </summary>
        public decimal TotalFixedAdditions { get; set; }

        /// <summary>
        /// Total Fixed Discounts - إجمالي الخصومات الثابتة
        /// </summary>
        public decimal TotalFixedDiscounts { get; set; }

        /// <summary>
        /// Total Percentage Discounts - إجمالي الخصومات النسبية
        /// </summary>
        public decimal TotalPercentageDiscounts { get; set; }

        /// <summary>
        /// Next Escalation Date - تاريخ التصاعد القادم
        /// </summary>
        public DateTime? NextEscalationDate { get; set; }

        /// <summary>
        /// Pending Approval Count - عدد قواعد في انتظار الموافقة
        /// </summary>
        public int PendingApprovalCount { get; set; }
    }

    #endregion
}
