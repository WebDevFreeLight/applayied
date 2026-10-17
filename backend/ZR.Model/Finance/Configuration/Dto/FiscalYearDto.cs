using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Configuration.Enums;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// Fiscal Year Query DTO - استعلام السنة المالية
    /// </summary>
    public class FiscalYearQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public long? CompanyId { get; set; }
        public FiscalYearStatus? Status { get; set; }
        public DateTime? BeginYearStartDate { get; set; }
        public DateTime? EndYearStartDate { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create Fiscal Year DTO - إنشاء سنة مالية جديدة
    /// </summary>
    public class CreateFiscalYearDto
    {
        public long? CompanyId { get; set; }

        [Required(ErrorMessage = "تاريخ بداية السنة مطلوب")]
        public DateTime YearStartDate { get; set; }

        [Required(ErrorMessage = "تاريخ نهاية السنة مطلوب")]
        public DateTime YearEndDate { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Fiscal Year DTO - تحديث سنة مالية
    /// </summary>
    public class UpdateFiscalYearDto
    {
        [Required(ErrorMessage = "معرف السنة المالية مطلوب")]
        public Guid Id { get; set; }

        public long? CompanyId { get; set; }

        [Required(ErrorMessage = "تاريخ بداية السنة مطلوب")]
        public DateTime YearStartDate { get; set; }

        [Required(ErrorMessage = "تاريخ نهاية السنة مطلوب")]
        public DateTime YearEndDate { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Fiscal Year List DTO - قائمة السنوات المالية
    /// </summary>
    public class FiscalYearListDto
    {
        [ExcelColumn(Name = "Fiscal Year ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Company ID")]
        public long? CompanyId { get; set; }

        [ExcelColumn(Name = "Year Start Date")]
        public DateTime YearStartDate { get; set; }

        [ExcelColumn(Name = "Year End Date")]
        public DateTime YearEndDate { get; set; }

        [ExcelColumn(Name = "Status")]
        public FiscalYearStatus Status { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Fiscal Year Detail DTO - تفاصيل السنة المالية
    /// </summary>
    public class FiscalYearDetailDto : FiscalYearListDto
    {
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}


