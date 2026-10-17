using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.Finance.Budget.Dto
{
    #region Budget Header DTOs

    /// <summary>
    /// Budget Header Query DTO - استعلام رأس الميزانية
    /// </summary>
    public class BudgetHeaderQueryDto
    {
        /// <summary>
        /// Budget Code - كود الميزانية
        /// </summary>
        public string BudgetCode { get; set; }

        /// <summary>
        /// Budget Name - اسم الميزانية
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int? FiscalYear { get; set; }

        /// <summary>
        /// Budget Type (Operational, Capital, Departmental, Project) - نوع الميزانية
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Budget Status (Draft, Approved, Closed) - حالة الميزانية
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CostCenterId { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Budget Header List DTO - قائمة رؤوس الميزانية
    /// </summary>
    public class BudgetHeaderListDto
    {
        /// <summary>
        /// Budget ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Budget Code
        /// </summary>
        public string BudgetCode { get; set; }

        /// <summary>
        /// Budget Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Budget Name (Arabic)
        /// </summary>
        public string NameAr { get; set; }

        /// <summary>
        /// Fiscal Year
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Budget Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Budget Status (Draft, Approved, Closed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Project ID
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Project Name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Department ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Department Name
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// Cost Center ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CostCenterId { get; set; }

        /// <summary>
        /// Cost Center Name
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Created At
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Approved By
        /// </summary>
        public Guid? ApprovedBy { get; set; }

        /// <summary>
        /// Approver Name
        /// </summary>
        public string ApproverName { get; set; }

        /// <summary>
        /// Approved At
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Is Active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is Draft
        /// </summary>
        public bool IsDraft => Status == "Draft";

        /// <summary>
        /// Is Approved
        /// </summary>
        public bool IsApproved => Status == "Approved";

        /// <summary>
        /// Is Closed
        /// </summary>
        public bool IsClosed => Status == "Closed";

        /// <summary>
        /// Can Edit - Only draft budgets can be edited
        /// </summary>
        public bool CanEdit => Status == "Draft";

        /// <summary>
        /// Total Budgeted Amount (sum of all lines)
        /// </summary>
        public decimal TotalBudgetedAmount { get; set; }

        /// <summary>
        /// Total Actual Amount (sum of all lines)
        /// </summary>
        public decimal TotalActualAmount { get; set; }

        /// <summary>
        /// Total Committed Amount
        /// </summary>
        public decimal TotalCommittedAmount { get; set; }

        /// <summary>
        /// Total Available Amount
        /// </summary>
        public decimal TotalAvailableAmount => TotalBudgetedAmount - TotalActualAmount - TotalCommittedAmount;

        /// <summary>
        /// Total Variance (Actual - Budget)
        /// </summary>
        public decimal TotalVariance => TotalActualAmount - TotalBudgetedAmount;

        /// <summary>
        /// Total Variance Percentage
        /// </summary>
        public decimal TotalVariancePercentage => TotalBudgetedAmount != 0
            ? Math.Round((TotalVariance / TotalBudgetedAmount) * 100, 2)
            : 0;

        /// <summary>
        /// Utilization Percentage
        /// </summary>
        public decimal UtilizationPercentage => TotalBudgetedAmount != 0
            ? Math.Round((TotalActualAmount / TotalBudgetedAmount) * 100, 2)
            : 0;
    }

    /// <summary>
    /// Budget Header Detail DTO - تفاصيل رأس الميزانية
    /// </summary>
    public class BudgetHeaderDetailDto : BudgetHeaderListDto
    {
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Closed By
        /// </summary>
        public Guid? ClosedBy { get; set; }

        /// <summary>
        /// Closed At
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// Budget Lines
        /// </summary>
        public List<BudgetLineDto> Lines { get; set; } = new List<BudgetLineDto>();

        /// <summary>
        /// Variance Summaries
        /// </summary>
        public List<BudgetVarianceSummaryDto> VarianceSummaries { get; set; } = new List<BudgetVarianceSummaryDto>();

        /// <summary>
        /// Created By
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// Created Time
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Updated By
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// Updated Time
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// Create Budget Header DTO - إنشاء رأس الميزانية
    /// </summary>
    public class CreateBudgetHeaderDto
    {
        /// <summary>
        /// Budget Code - كود الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget Code is required - كود الميزانية مطلوب")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string BudgetCode { get; set; }

        /// <summary>
        /// Budget Name - اسم الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget Name is required - اسم الميزانية مطلوب")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Budget Name (Arabic) - اسم الميزانية بالعربي
        /// </summary>
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters")]
        public string NameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [Required(ErrorMessage = "Fiscal Year is required - السنة المالية مطلوبة")]
        [Range(2000, 2100, ErrorMessage = "Fiscal Year must be between 2000 and 2100")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [Required(ErrorMessage = "Currency is required - العملة مطلوبة")]
        [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Budget Type (Operational, Capital, Departmental, Project) - نوع الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget Type is required - نوع الميزانية مطلوب")]
        public string Type { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CostCenterId { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Company ID - معرف الشركة (for multi-tenant isolation)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CompanyID { get; set; }

        /// <summary>
        /// Budget Lines - بنود الميزانية
        /// </summary>
        public List<CreateBudgetLineDto> Lines { get; set; } = new List<CreateBudgetLineDto>();
    }

    /// <summary>
    /// Update Budget Header DTO - تحديث رأس الميزانية
    /// </summary>
    public class UpdateBudgetHeaderDto
    {
        /// <summary>
        /// Budget ID - معرف الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget ID is required - معرف الميزانية مطلوب")]
        public Guid Id { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة (for multi-tenant isolation)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CompanyID { get; set; }

        /// <summary>
        /// Budget Code - كود الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget Code is required - كود الميزانية مطلوب")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string BudgetCode { get; set; }

        /// <summary>
        /// Budget Name - اسم الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget Name is required - اسم الميزانية مطلوب")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Budget Name (Arabic) - اسم الميزانية بالعربي
        /// </summary>
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters")]
        public string NameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [Required(ErrorMessage = "Fiscal Year is required - السنة المالية مطلوبة")]
        [Range(2000, 2100, ErrorMessage = "Fiscal Year must be between 2000 and 2100")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [Required(ErrorMessage = "Currency is required - العملة مطلوبة")]
        [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters")]
        public string Currency { get; set; }

        /// <summary>
        /// Budget Type (Operational, Capital, Departmental, Project) - نوع الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget Type is required - نوع الميزانية مطلوب")]
        public string Type { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CostCenterId { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Budget Lines - بنود الميزانية
        /// </summary>
        public List<UpdateBudgetLineDto> Lines { get; set; } = new List<UpdateBudgetLineDto>();
    }

    #endregion

    #region Budget Line DTOs

    /// <summary>
    /// Budget Line DTO - بند الميزانية
    /// </summary>
    public class BudgetLineDto
    {
        /// <summary>
        /// Line ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Budget Header ID
        /// </summary>
        public Guid BudgetHeaderId { get; set; }

        /// <summary>
        /// GL Account ID
        /// </summary>
        public Guid GLAccountId { get; set; }

        /// <summary>
        /// GL Account Code
        /// </summary>
        public string GLAccountCode { get; set; }

        /// <summary>
        /// GL Account Name
        /// </summary>
        public string GLAccountName { get; set; }

        /// <summary>
        /// Period (1-12 for months, or 1-4 for quarters)
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Budgeted Amount
        /// </summary>
        public decimal BudgetedAmount { get; set; }

        /// <summary>
        /// Revised Amount
        /// </summary>
        public decimal RevisedAmount { get; set; }

        /// <summary>
        /// Actual Amount
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Committed Amount
        /// </summary>
        public decimal CommittedAmount { get; set; }

        /// <summary>
        /// Effective Budget (Revised if > 0, else Budgeted)
        /// </summary>
        public decimal EffectiveBudget => RevisedAmount > 0 ? RevisedAmount : BudgetedAmount;

        /// <summary>
        /// Available Amount
        /// </summary>
        public decimal AvailableAmount => EffectiveBudget - ActualAmount - CommittedAmount;

        /// <summary>
        /// Variance (Actual - Budget)
        /// </summary>
        public decimal Variance => ActualAmount - BudgetedAmount;

        /// <summary>
        /// Variance Percentage
        /// </summary>
        public decimal VariancePercentage => BudgetedAmount != 0
            ? Math.Round((Variance / BudgetedAmount) * 100, 2)
            : 0;

        /// <summary>
        /// Utilization Percentage
        /// </summary>
        public decimal UtilizationPercentage => EffectiveBudget != 0
            ? Math.Round((ActualAmount / EffectiveBudget) * 100, 2)
            : 0;

        /// <summary>
        /// Is Over Budget
        /// </summary>
        public bool IsOverBudget => ActualAmount > EffectiveBudget;

        /// <summary>
        /// Last Actual Update
        /// </summary>
        public DateTime? LastActualUpdate { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// Create Budget Line DTO - إنشاء بند الميزانية
    /// </summary>
    public class CreateBudgetLineDto
    {
        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// </summary>
        [Required(ErrorMessage = "GL Account is required - حساب دفتر الأستاذ مطلوب")]
        public Guid GLAccountId { get; set; }

        /// <summary>
        /// Period (1-12 for months, or 1-4 for quarters) - الفترة
        /// </summary>
        [Required(ErrorMessage = "Period is required - الفترة مطلوبة")]
        [Range(1, 12, ErrorMessage = "Period must be between 1 and 12")]
        public int Period { get; set; }

        /// <summary>
        /// Budgeted Amount - المبلغ المخطط
        /// </summary>
        [Required(ErrorMessage = "Budgeted Amount is required - المبلغ المخطط مطلوب")]
        public decimal BudgetedAmount { get; set; }

        /// <summary>
        /// Revised Amount - المبلغ المعدل
        /// </summary>
        public decimal RevisedAmount { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update Budget Line DTO - تحديث بند الميزانية
    /// </summary>
    public class UpdateBudgetLineDto : CreateBudgetLineDto
    {
        /// <summary>
        /// Line ID - معرف البند
        /// </summary>
        public Guid? Id { get; set; }
    }

    #endregion

    #region Budget Variance Summary DTOs

    /// <summary>
    /// Budget Variance Summary DTO - ملخص انحراف الميزانية
    /// </summary>
    public class BudgetVarianceSummaryDto
    {
        /// <summary>
        /// Summary ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Budget Header ID
        /// </summary>
        public Guid BudgetHeaderId { get; set; }

        /// <summary>
        /// GL Account ID
        /// </summary>
        public Guid GLAccountId { get; set; }

        /// <summary>
        /// GL Account Code
        /// </summary>
        public string GLAccountCode { get; set; }

        /// <summary>
        /// GL Account Name
        /// </summary>
        public string GLAccountName { get; set; }

        /// <summary>
        /// Period
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Budget Amount
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Actual Amount
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Committed Amount
        /// </summary>
        public decimal CommittedAmount { get; set; }

        /// <summary>
        /// Available Amount
        /// </summary>
        public decimal AvailableAmount { get; set; }

        /// <summary>
        /// Variance (Actual - Budget)
        /// </summary>
        public decimal Variance { get; set; }

        /// <summary>
        /// Variance Percentage
        /// </summary>
        public decimal VariancePercentage { get; set; }

        /// <summary>
        /// Utilization Percentage
        /// </summary>
        public decimal UtilizationPercentage => BudgetAmount != 0
            ? Math.Round((ActualAmount / BudgetAmount) * 100, 2)
            : 0;

        /// <summary>
        /// Is Over Budget
        /// </summary>
        public bool IsOverBudget => Variance > 0;

        /// <summary>
        /// Is Stale
        /// </summary>
        public bool IsStale { get; set; }

        /// <summary>
        /// Last Refreshed
        /// </summary>
        public DateTime LastRefreshed { get; set; }
    }

    #endregion

    #region Report DTOs

    /// <summary>
    /// Budget vs Actual Report DTO - تقرير الميزانية مقابل الفعلي
    /// </summary>
    public class BudgetVsActualReportDto
    {
        /// <summary>
        /// Budget Header ID
        /// </summary>
        public Guid BudgetHeaderId { get; set; }

        /// <summary>
        /// Budget Code
        /// </summary>
        public string BudgetCode { get; set; }

        /// <summary>
        /// Budget Name
        /// </summary>
        public string BudgetName { get; set; }

        /// <summary>
        /// Budget Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Fiscal Year
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Report Lines by Account
        /// </summary>
        public List<BudgetVsActualLineDto> Lines { get; set; } = new List<BudgetVsActualLineDto>();

        /// <summary>
        /// Total Budgeted
        /// </summary>
        public decimal TotalBudgeted { get; set; }

        /// <summary>
        /// Total Actual
        /// </summary>
        public decimal TotalActual { get; set; }

        /// <summary>
        /// Total Committed
        /// </summary>
        public decimal TotalCommitted { get; set; }

        /// <summary>
        /// Total Available
        /// </summary>
        public decimal TotalAvailable => TotalBudgeted - TotalActual - TotalCommitted;

        /// <summary>
        /// Total Variance (Actual - Budget)
        /// </summary>
        public decimal TotalVariance => TotalActual - TotalBudgeted;

        /// <summary>
        /// Overall Variance Percentage
        /// </summary>
        public decimal OverallVariancePercentage => TotalBudgeted != 0
            ? Math.Round((TotalVariance / TotalBudgeted) * 100, 2)
            : 0;
    }

    /// <summary>
    /// Budget vs Actual Line DTO - بند تقرير الميزانية مقابل الفعلي
    /// </summary>
    public class BudgetVsActualLineDto
    {
        /// <summary>
        /// GL Account ID
        /// </summary>
        public Guid GLAccountId { get; set; }

        /// <summary>
        /// GL Account Code
        /// </summary>
        public string GLAccountCode { get; set; }

        /// <summary>
        /// GL Account Name
        /// </summary>
        public string GLAccountName { get; set; }

        /// <summary>
        /// Budgeted Amount
        /// </summary>
        public decimal BudgetedAmount { get; set; }

        /// <summary>
        /// Actual Amount
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Committed Amount
        /// </summary>
        public decimal CommittedAmount { get; set; }

        /// <summary>
        /// Available Amount
        /// </summary>
        public decimal AvailableAmount => BudgetedAmount - ActualAmount - CommittedAmount;

        /// <summary>
        /// Variance (Actual - Budget)
        /// </summary>
        public decimal Variance => ActualAmount - BudgetedAmount;

        /// <summary>
        /// Variance Percentage
        /// </summary>
        public decimal VariancePercentage => BudgetedAmount != 0
            ? Math.Round((Variance / BudgetedAmount) * 100, 2)
            : 0;

        /// <summary>
        /// Periods breakdown
        /// </summary>
        public List<BudgetPeriodBreakdownDto> Periods { get; set; } = new List<BudgetPeriodBreakdownDto>();
    }

    /// <summary>
    /// Budget Period Breakdown DTO - تفصيل فترة الميزانية
    /// </summary>
    public class BudgetPeriodBreakdownDto
    {
        /// <summary>
        /// Period Number
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Budgeted Amount
        /// </summary>
        public decimal BudgetedAmount { get; set; }

        /// <summary>
        /// Actual Amount
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Committed Amount
        /// </summary>
        public decimal CommittedAmount { get; set; }

        /// <summary>
        /// Variance (Actual - Budget)
        /// </summary>
        public decimal Variance => ActualAmount - BudgetedAmount;
    }

    /// <summary>
    /// Budget Select Item DTO - عنصر اختيار الميزانية
    /// </summary>
    public class BudgetSelectItemDto
    {
        /// <summary>
        /// Budget ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Budget Code
        /// </summary>
        public string BudgetCode { get; set; }

        /// <summary>
        /// Budget Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Fiscal Year
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Budget Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Budget Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Is Active
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Approve Budget Request DTO - طلب اعتماد الميزانية
    /// </summary>
    public class ApproveBudgetRequestDto
    {
        /// <summary>
        /// Budget ID - معرف الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget ID is required - معرف الميزانية مطلوب")]
        public Guid BudgetId { get; set; }

        /// <summary>
        /// Approval Notes - ملاحظات الاعتماد
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Close Budget Request DTO - طلب إغلاق الميزانية
    /// </summary>
    public class CloseBudgetRequestDto
    {
        /// <summary>
        /// Budget ID - معرف الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget ID is required - معرف الميزانية مطلوب")]
        public Guid BudgetId { get; set; }

        /// <summary>
        /// Closing Notes - ملاحظات الإغلاق
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Refresh Actuals Request DTO - طلب تحديث الفعلي
    /// </summary>
    public class RefreshActualsRequestDto
    {
        /// <summary>
        /// Budget ID - معرف الميزانية
        /// </summary>
        [Required(ErrorMessage = "Budget ID is required - معرف الميزانية مطلوب")]
        public Guid BudgetId { get; set; }

        /// <summary>
        /// From Date - من تاريخ
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// To Date - إلى تاريخ
        /// </summary>
        public DateTime? ToDate { get; set; }
    }

    #endregion
}
