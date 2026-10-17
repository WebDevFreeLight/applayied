using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    #region Query DTO

    /// <summary>
    /// Contract Unit Query DTO - معايير البحث عن وحدات العقد
    /// Used for filtering and searching contract units
    /// </summary>
    public class ErpContractUnitQueryDto : PagerInfo
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Unit Type - نوع الوحدة
        /// </summary>
        public ContractUnitType? UnitType { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Unit Name - اسم الوحدة
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Building - المبنى
        /// </summary>
        public string Building { get; set; }

        /// <summary>
        /// Floor - الطابق
        /// </summary>
        public string Floor { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Area From - المساحة من
        /// </summary>
        public decimal? AreaFrom { get; set; }

        /// <summary>
        /// Area To - المساحة إلى
        /// </summary>
        public decimal? AreaTo { get; set; }

        /// <summary>
        /// Base Price From - السعر الأساسي من
        /// </summary>
        public decimal? BasePriceFrom { get; set; }

        /// <summary>
        /// Base Price To - السعر الأساسي إلى
        /// </summary>
        public decimal? BasePriceTo { get; set; }

        /// <summary>
        /// External Reference Code - رمز المرجع الخارجي
        /// </summary>
        public string ExternalRefCode { get; set; }

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
    /// Create Contract Unit DTO - بيانات إنشاء وحدة العقد
    /// Used for creating a new contract unit
    /// </summary>
    public class CreateErpContractUnitDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Unit Type - نوع الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit type is required / نوع الوحدة مطلوب")]
        public ContractUnitType UnitType { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit code is required / رمز الوحدة مطلوب")]
        [StringLength(50, ErrorMessage = "Unit code cannot exceed 50 characters / رمز الوحدة لا يتجاوز 50 حرف")]
        public string UnitCode { get; set; }

        /// <summary>
        /// Unit Name - اسم الوحدة
        /// </summary>
        [StringLength(200, ErrorMessage = "Unit name cannot exceed 200 characters / اسم الوحدة لا يتجاوز 200 حرف")]
        public string UnitName { get; set; }

        /// <summary>
        /// Unit Name (Arabic) - اسم الوحدة بالعربية
        /// </summary>
        [StringLength(200, ErrorMessage = "Arabic unit name cannot exceed 200 characters / اسم الوحدة بالعربية لا يتجاوز 200 حرف")]
        public string UnitNameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters / الوصف لا يتجاوز 500 حرف")]
        public string Description { get; set; }

        /// <summary>
        /// Location - الموقع
        /// </summary>
        [StringLength(300, ErrorMessage = "Location cannot exceed 300 characters / الموقع لا يتجاوز 300 حرف")]
        public string Location { get; set; }

        /// <summary>
        /// Building - المبنى
        /// </summary>
        [StringLength(100, ErrorMessage = "Building cannot exceed 100 characters / المبنى لا يتجاوز 100 حرف")]
        public string Building { get; set; }

        /// <summary>
        /// Floor - الطابق
        /// </summary>
        [StringLength(20, ErrorMessage = "Floor cannot exceed 20 characters / الطابق لا يتجاوز 20 حرف")]
        public string Floor { get; set; }

        /// <summary>
        /// Area - المساحة
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Area must be positive / المساحة يجب أن تكون موجبة")]
        public decimal? Area { get; set; }

        /// <summary>
        /// Area Unit - وحدة المساحة
        /// </summary>
        [StringLength(20, ErrorMessage = "Area unit cannot exceed 20 characters / وحدة المساحة لا تتجاوز 20 حرف")]
        public string AreaUnit { get; set; } = "sqm";

        /// <summary>
        /// Base Price - السعر الأساسي
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Base price must be positive / السعر الأساسي يجب أن يكون موجباً")]
        public decimal? BasePrice { get; set; }

        /// <summary>
        /// Price Per Area - السعر لكل وحدة مساحة
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Price per area must be positive / السعر لكل وحدة مساحة يجب أن يكون موجباً")]
        public decimal? PricePerArea { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters / العملة لا تتجاوز 10 أحرف")]
        public string Currency { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// External Reference ID - معرف المرجع الخارجي
        /// </summary>
        public Guid? ExternalRefId { get; set; }

        /// <summary>
        /// External Reference Code - رمز المرجع الخارجي
        /// </summary>
        [StringLength(100, ErrorMessage = "External reference code cannot exceed 100 characters / رمز المرجع الخارجي لا يتجاوز 100 حرف")]
        public string ExternalRefCode { get; set; }

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// </summary>
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// </summary>
        public bool? IsTaxInclusive { get; set; }

        /// <summary>
        /// Revenue Account ID - معرف حساب الإيرادات
        /// </summary>
        public Guid? RevenueAccountId { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Sequence Number - رقم التسلسل
        /// </summary>
        public int SequenceNumber { get; set; } = 0;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters / الملاحظات لا تتجاوز 1000 حرف")]
        public string Notes { get; set; }

        #region Handover Fields

        /// <summary>
        /// Handover Date - تاريخ التسليم
        /// </summary>
        public DateTime? HandoverDate { get; set; }

        /// <summary>
        /// Handover Condition - حالة التسليم
        /// </summary>
        [StringLength(500, ErrorMessage = "Handover condition cannot exceed 500 characters / حالة التسليم لا تتجاوز 500 حرف")]
        public string HandoverCondition { get; set; }

        #endregion

        #region Equipment/Vehicle Fields

        /// <summary>
        /// Serial Number - الرقم التسلسلي
        /// </summary>
        [StringLength(100, ErrorMessage = "Serial number cannot exceed 100 characters / الرقم التسلسلي لا يتجاوز 100 حرف")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Model - الموديل
        /// </summary>
        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters / الموديل لا يتجاوز 100 حرف")]
        public string Model { get; set; }

        /// <summary>
        /// Manufacturer - الشركة المصنعة
        /// </summary>
        [StringLength(100, ErrorMessage = "Manufacturer cannot exceed 100 characters / الشركة المصنعة لا تتجاوز 100 حرف")]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Year - السنة
        /// </summary>
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100 / السنة بين 1900 و 2100")]
        public int? Year { get; set; }

        /// <summary>
        /// Plate Number - رقم اللوحة
        /// </summary>
        [StringLength(50, ErrorMessage = "Plate number cannot exceed 50 characters / رقم اللوحة لا يتجاوز 50 حرف")]
        public string PlateNumber { get; set; }

        #endregion
    }

    #endregion

    #region Update DTO

    /// <summary>
    /// Update Contract Unit DTO - بيانات تحديث وحدة العقد
    /// Used for updating an existing contract unit
    /// </summary>
    public class UpdateErpContractUnitDto : CreateErpContractUnitDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit ID is required / معرف الوحدة مطلوب")]
        public Guid UnitId { get; set; }
    }

    #endregion

    #region List DTO (Full Implementation - replacing placeholder)

    /// <summary>
    /// Contract Unit List DTO - قائمة وحدات العقد
    /// Used for displaying units in list views
    /// </summary>
    public class ErpContractUnitFullListDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid UnitId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Unit Type - نوع الوحدة
        /// </summary>
        [ExcelColumn(Name = "Type / النوع")]
        public ContractUnitType UnitType { get; set; }

        /// <summary>
        /// Unit Type Name - اسم نوع الوحدة
        /// </summary>
        public string UnitTypeName { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        [ExcelColumn(Name = "Code / الرمز")]
        public string UnitCode { get; set; }

        /// <summary>
        /// Unit Name - اسم الوحدة
        /// </summary>
        [ExcelColumn(Name = "Name / الاسم")]
        public string UnitName { get; set; }

        /// <summary>
        /// Unit Name (Arabic) - اسم الوحدة بالعربية
        /// </summary>
        public string UnitNameAr { get; set; }

        /// <summary>
        /// Location - الموقع
        /// </summary>
        [ExcelColumn(Name = "Location / الموقع")]
        public string Location { get; set; }

        /// <summary>
        /// Building - المبنى
        /// </summary>
        [ExcelColumn(Name = "Building / المبنى")]
        public string Building { get; set; }

        /// <summary>
        /// Floor - الطابق
        /// </summary>
        [ExcelColumn(Name = "Floor / الطابق")]
        public string Floor { get; set; }

        /// <summary>
        /// Area - المساحة
        /// </summary>
        [ExcelColumn(Name = "Area / المساحة", Format = "#,##0.00")]
        public decimal? Area { get; set; }

        /// <summary>
        /// Area Unit - وحدة المساحة
        /// </summary>
        public string AreaUnit { get; set; }

        /// <summary>
        /// Base Price - السعر الأساسي
        /// </summary>
        [ExcelColumn(Name = "Base Price / السعر الأساسي", Format = "#,##0.00")]
        public decimal? BasePrice { get; set; }

        /// <summary>
        /// Price Per Area - السعر لكل وحدة مساحة
        /// </summary>
        public decimal? PricePerArea { get; set; }

        /// <summary>
        /// Calculated Price - السعر المحسوب
        /// </summary>
        [ExcelColumn(Name = "Calculated Price / السعر المحسوب", Format = "#,##0.00")]
        public decimal? CalculatedPrice { get; set; }

        /// <summary>
        /// Effective Price - السعر الفعلي
        /// Returns BasePrice if set, otherwise CalculatedPrice
        /// </summary>
        public decimal? EffectivePrice => BasePrice ?? CalculatedPrice;

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [ExcelColumn(Name = "Currency / العملة")]
        public string Currency { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [ExcelColumn(Name = "Active / نشط")]
        public bool IsActive { get; set; }

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
        /// External Reference Code - رمز المرجع الخارجي
        /// </summary>
        public string ExternalRefCode { get; set; }

        /// <summary>
        /// Sequence Number - رقم التسلسل
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Created Time - وقت الإنشاء
        /// </summary>
        public DateTime? Create_time { get; set; }
    }

    #endregion

    #region Detail DTO

    /// <summary>
    /// Contract Unit Detail DTO - تفاصيل وحدة العقد
    /// Used for displaying full unit details
    /// </summary>
    public class ErpContractUnitDetailDto : ErpContractUnitFullListDto
    {
        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// External Reference ID - معرف المرجع الخارجي
        /// </summary>
        public Guid? ExternalRefId { get; set; }

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
        public bool? IsTaxInclusive { get; set; }

        /// <summary>
        /// Revenue Account ID - معرف حساب الإيرادات
        /// </summary>
        public Guid? RevenueAccountId { get; set; }

        /// <summary>
        /// Revenue Account Name - اسم حساب الإيرادات
        /// </summary>
        public string RevenueAccountName { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Cost Center Name - اسم مركز التكلفة
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        #region Handover/Return Fields

        /// <summary>
        /// Handover Date - تاريخ التسليم
        /// </summary>
        public DateTime? HandoverDate { get; set; }

        /// <summary>
        /// Handover Condition - حالة التسليم
        /// </summary>
        public string HandoverCondition { get; set; }

        /// <summary>
        /// Return Date - تاريخ الإرجاع
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Return Condition - حالة الإرجاع
        /// </summary>
        public string ReturnCondition { get; set; }

        #endregion

        #region Equipment/Vehicle Fields

        /// <summary>
        /// Serial Number - الرقم التسلسلي
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Model - الموديل
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Manufacturer - الشركة المصنعة
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Year - السنة
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Plate Number - رقم اللوحة
        /// </summary>
        public string PlateNumber { get; set; }

        #endregion

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

    #region Select DTO

    /// <summary>
    /// Contract Unit Select DTO - اختيار وحدة العقد
    /// Used for dropdown/selection lists
    /// </summary>
    public class ErpContractUnitSelectDto
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
        /// Unit Type - نوع الوحدة
        /// </summary>
        public ContractUnitType UnitType { get; set; }

        /// <summary>
        /// Area - المساحة
        /// </summary>
        public decimal? Area { get; set; }

        /// <summary>
        /// Effective Price - السعر الفعلي
        /// </summary>
        public decimal? EffectivePrice { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Display Text - نص العرض
        /// </summary>
        public string DisplayText => $"{UnitCode} - {UnitName}";
    }

    #endregion

    #region Bulk Operations DTOs

    /// <summary>
    /// Bulk Add Units DTO - إضافة وحدات متعددة
    /// Used for adding multiple units at once
    /// </summary>
    public class BulkAddContractUnitsDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Units - الوحدات
        /// </summary>
        [Required(ErrorMessage = "At least one unit is required / يجب إضافة وحدة واحدة على الأقل")]
        [MinLength(1, ErrorMessage = "At least one unit is required / يجب إضافة وحدة واحدة على الأقل")]
        public List<CreateErpContractUnitDto> Units { get; set; }
    }

    /// <summary>
    /// Unit Return DTO - إرجاع الوحدة
    /// Used for recording unit return
    /// </summary>
    public class UnitReturnDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit ID is required / معرف الوحدة مطلوب")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// Return Date - تاريخ الإرجاع
        /// </summary>
        [Required(ErrorMessage = "Return date is required / تاريخ الإرجاع مطلوب")]
        public DateTime ReturnDate { get; set; }

        /// <summary>
        /// Return Condition - حالة الإرجاع
        /// </summary>
        [StringLength(500, ErrorMessage = "Return condition cannot exceed 500 characters / حالة الإرجاع لا تتجاوز 500 حرف")]
        public string ReturnCondition { get; set; }

        /// <summary>
        /// Deactivate Unit - إلغاء تفعيل الوحدة
        /// </summary>
        public bool DeactivateUnit { get; set; } = true;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Unit Price Update DTO - تحديث سعر الوحدة
    /// Used for updating unit pricing
    /// </summary>
    public class UpdateUnitPriceDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit ID is required / معرف الوحدة مطلوب")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// Base Price - السعر الأساسي
        /// </summary>
        public decimal? BasePrice { get; set; }

        /// <summary>
        /// Price Per Area - السعر لكل وحدة مساحة
        /// </summary>
        public decimal? PricePerArea { get; set; }

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// </summary>
        [Required(ErrorMessage = "Effective date is required / تاريخ السريان مطلوب")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Create Amendment - إنشاء تعديل
        /// Whether to create an amendment record
        /// </summary>
        public bool CreateAmendment { get; set; } = true;

        /// <summary>
        /// Recalculate Schedules - إعادة حساب الجداول
        /// Whether to recalculate future billing schedules
        /// </summary>
        public bool RecalculateSchedules { get; set; } = true;
    }

    #endregion

    #region Change Shop DTO

    /// <summary>
    /// Change Contract Unit Shop DTO - تغيير محل وحدة العقد
    /// Used for changing the boulevard shop assigned to a contract unit
    /// </summary>
    public class ChangeContractUnitShopDto
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        [Required(ErrorMessage = "Unit ID is required / معرف الوحدة مطلوب")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// New Shop ID - معرف المحل الجديد
        /// </summary>
        [Required(ErrorMessage = "New Shop ID is required / معرف المحل الجديد مطلوب")]
        public long NewShopId { get; set; }
    }

    #endregion

    #region Summary DTO

    /// <summary>
    /// Contract Units Summary DTO - ملخص وحدات العقد
    /// Aggregated summary of units for a contract
    /// </summary>
    public class ContractUnitsSummaryDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Total Units - إجمالي الوحدات
        /// </summary>
        public int TotalUnits { get; set; }

        /// <summary>
        /// Active Units - الوحدات النشطة
        /// </summary>
        public int ActiveUnits { get; set; }

        /// <summary>
        /// Inactive Units - الوحدات غير النشطة
        /// </summary>
        public int InactiveUnits { get; set; }

        /// <summary>
        /// Total Area - إجمالي المساحة
        /// </summary>
        public decimal TotalArea { get; set; }

        /// <summary>
        /// Total Base Price - إجمالي السعر الأساسي
        /// </summary>
        public decimal TotalBasePrice { get; set; }

        /// <summary>
        /// Total Calculated Price - إجمالي السعر المحسوب
        /// </summary>
        public decimal TotalCalculatedPrice { get; set; }

        /// <summary>
        /// Total Effective Price - إجمالي السعر الفعلي
        /// </summary>
        public decimal TotalEffectivePrice { get; set; }

        /// <summary>
        /// Units By Type - الوحدات حسب النوع
        /// </summary>
        public Dictionary<ContractUnitType, int> UnitsByType { get; set; }
    }

    #endregion
}
