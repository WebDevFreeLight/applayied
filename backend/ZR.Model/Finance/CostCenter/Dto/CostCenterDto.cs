using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.Finance.CostCenter.Dto
{
    /// <summary>
    /// Cost Center Query DTO - استعلام مراكز التكلفة
    /// </summary>
    public class CostCenterQueryDto
    {
        /// <summary>
        /// Cost Center Code - كود مركز التكلفة
        /// </summary>
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name - اسم مركز التكلفة
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Cost Center Type - نوع مركز التكلفة
        /// </summary>
        public string CostCenterType { get; set; }

        /// <summary>
        /// Parent ID - معرف الأب
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public long? BranchId { get; set; }
    }

    /// <summary>
    /// Cost Center List DTO - قائمة مراكز التكلفة
    /// </summary>
    public class CostCenterListDto
    {
        /// <summary>
        /// Cost Center ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CostCenterId { get; set; }

        /// <summary>
        /// Parent ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentId { get; set; }

        /// <summary>
        /// Parent Name
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// Cost Center Code
        /// </summary>
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name (English)
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Cost Center Name (Arabic)
        /// </summary>
        public string CostCenterNameAr { get; set; }

        /// <summary>
        /// Cost Center Type
        /// </summary>
        public string CostCenterType { get; set; }

        /// <summary>
        /// Level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Is Parent
        /// </summary>
        public bool IsParent { get; set; }

        /// <summary>
        /// Budget Amount
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Actual Amount
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Variance
        /// </summary>
        public decimal Variance { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is Active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Children - for tree view
        /// </summary>
        public List<CostCenterListDto> Children { get; set; }
    }

    /// <summary>
    /// Cost Center Detail DTO - تفاصيل مركز التكلفة
    /// </summary>
    public class CostCenterDetailDto : CostCenterListDto
    {
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
    /// Create Cost Center DTO - إنشاء مركز تكلفة
    /// </summary>
    public class CreateCostCenterDto
    {
        /// <summary>
        /// Parent ID - معرف الأب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentId { get; set; }

        /// <summary>
        /// Cost Center Code - كود مركز التكلفة
        /// </summary>
        [Required(ErrorMessage = "Cost Center Code is required - كود مركز التكلفة مطلوب")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name - اسم مركز التكلفة
        /// </summary>
        [Required(ErrorMessage = "Cost Center Name is required - اسم مركز التكلفة مطلوب")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string CostCenterName { get; set; }

        /// <summary>
        /// Cost Center Name Arabic - اسم مركز التكلفة بالعربي
        /// </summary>
        [StringLength(200, ErrorMessage = "Arabic Name cannot exceed 200 characters")]
        public string CostCenterNameAr { get; set; }

        /// <summary>
        /// Cost Center Type - نوع مركز التكلفة (branch/department/project)
        /// </summary>
        [Required(ErrorMessage = "Cost Center Type is required - نوع مركز التكلفة مطلوب")]
        public string CostCenterType { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update Cost Center DTO - تحديث مركز تكلفة
    /// </summary>
    public class UpdateCostCenterDto : CreateCostCenterDto
    {
        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [Required(ErrorMessage = "Cost Center ID is required - معرف مركز التكلفة مطلوب")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CostCenterId { get; set; }
    }

    /// <summary>
    /// Cost Center Select Item DTO - عنصر اختيار مركز التكلفة
    /// </summary>
    public class CostCenterSelectItemDto
    {
        /// <summary>
        /// Cost Center ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CostCenterId { get; set; }

        /// <summary>
        /// Cost Center Code
        /// </summary>
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Cost Center Name Arabic
        /// </summary>
        public string CostCenterNameAr { get; set; }

        /// <summary>
        /// Cost Center Type
        /// </summary>
        public string CostCenterType { get; set; }

        /// <summary>
        /// Parent ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentId { get; set; }

        /// <summary>
        /// Level
        /// </summary>
        public int Level { get; set; }
    }

    /// <summary>
    /// Cost Center Balance DTO - رصيد مركز التكلفة
    /// </summary>
    public class CostCenterBalanceDto
    {
        /// <summary>
        /// Cost Center ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CostCenterId { get; set; }

        /// <summary>
        /// Cost Center Code
        /// </summary>
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Budget Amount
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Actual Amount
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Variance (Budget - Actual)
        /// </summary>
        public decimal Variance { get; set; }

        /// <summary>
        /// Variance Percentage
        /// </summary>
        public decimal VariancePercentage { get; set; }

        /// <summary>
        /// As Of Date
        /// </summary>
        public DateTime AsOfDate { get; set; }
    }

    /// <summary>
    /// Cost Center Report Item DTO - عنصر تقرير مركز التكلفة
    /// </summary>
    public class CostCenterReportItemDto
    {
        /// <summary>
        /// Cost Center ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CostCenterId { get; set; }

        /// <summary>
        /// Cost Center Code
        /// </summary>
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Cost Center Name Arabic
        /// </summary>
        public string CostCenterNameAr { get; set; }

        /// <summary>
        /// Cost Center Type
        /// </summary>
        public string CostCenterType { get; set; }

        /// <summary>
        /// Budget Amount
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Total Debit
        /// </summary>
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Total Credit
        /// </summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Net Amount
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Variance
        /// </summary>
        public decimal Variance { get; set; }

        /// <summary>
        /// Utilization Percentage
        /// </summary>
        public decimal UtilizationPercentage { get; set; }
    }
}
