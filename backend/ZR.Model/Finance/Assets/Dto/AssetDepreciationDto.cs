using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace ZR.Model.Finance.Assets.Dto
{
    /// <summary>
    /// Asset Depreciation Query DTO - استعلام إهلاك الأصول
    /// </summary>
    public class AssetDepreciationQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public Guid? AssetId { get; set; }
        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }
        public DateTime? DepreciationDateFrom { get; set; }
        public DateTime? DepreciationDateTo { get; set; }
        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Depreciation Record DTO - إنشاء سجل إهلاك
    /// </summary>
    public class CreateDepreciationRecordDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid AssetId { get; set; }

        [Required(ErrorMessage = "تاريخ الإهلاك مطلوب")]
        public DateTime DepreciationDate { get; set; }

        /// <summary>
        /// Depreciation amount - if not provided, will be calculated automatically
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ الإهلاك يجب أن يكون أكبر من صفر")]
        public decimal DepreciationAmount { get; set; }

        [Required(ErrorMessage = "السنة المالية مطلوبة")]
        public int FiscalYear { get; set; }

        [Required(ErrorMessage = "الفترة المالية مطلوبة")]
        [Range(1, 12, ErrorMessage = "الفترة المالية يجب أن تكون بين 1 و 12")]
        public int FiscalPeriod { get; set; }

        /// <summary>
        /// Optional GL Journal ID for integration - معرف القيد المحاسبي
        /// </summary>
        public Guid? GLJournalId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Generate Monthly Depreciation DTO - توليد إهلاك شهري
    /// </summary>
    public class GenerateMonthlyDepreciationDto
    {
        [Required(ErrorMessage = "السنة المالية مطلوبة")]
        [Range(2000, 2100, ErrorMessage = "السنة المالية يجب أن تكون بين 2000 و 2100")]
        public int FiscalYear { get; set; }

        [Required(ErrorMessage = "الفترة المالية مطلوبة")]
        [Range(1, 12, ErrorMessage = "الفترة المالية يجب أن تكون بين 1 و 12")]
        public int FiscalPeriod { get; set; }

        [Required(ErrorMessage = "تاريخ الإهلاك مطلوب")]
        public DateTime DepreciationDate { get; set; }

        /// <summary>
        /// Optional - if not provided, will depreciate all active assets
        /// </summary>
        public Guid? AssetId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Depreciation Record List DTO - قائمة سجلات الإهلاك
    /// </summary>
    public class DepreciationRecordListDto
    {
        [ExcelColumn(Name = "Depreciation ID")]
        public Guid Id { get; set; }

        public Guid AssetId { get; set; }

        [ExcelColumn(Name = "Asset Code")]
        public string AssetCode { get; set; }

        [ExcelColumn(Name = "Asset Name")]
        public string AssetName { get; set; }

        [ExcelColumn(Name = "Depreciation Date", Format = "yyyy-MM-dd")]
        public DateTime DepreciationDate { get; set; }

        [ExcelColumn(Name = "Depreciation Amount")]
        public decimal DepreciationAmount { get; set; }

        [ExcelColumn(Name = "Accumulated Value After")]
        public decimal AccumulatedValueAfter { get; set; }

        [ExcelColumn(Name = "Book Value After")]
        public decimal BookValueAfter { get; set; }

        [ExcelColumn(Name = "Fiscal Year")]
        public int FiscalYear { get; set; }

        [ExcelColumn(Name = "Fiscal Period")]
        public int FiscalPeriod { get; set; }

        public Guid? GLJournalId { get; set; }

        [ExcelColumn(Name = "Created At", Format = "yyyy-MM-dd HH:mm")]
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Depreciation Summary DTO - ملخص الإهلاك
    /// </summary>
    public class DepreciationSummaryDto
    {
        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }
        public int AssetCount { get; set; }
        public decimal TotalDepreciation { get; set; }
        public DateTime ProcessedDate { get; set; }
        public string Message { get; set; }
        public List<DepreciationDetailItemDto> Details { get; set; } = new List<DepreciationDetailItemDto>();
    }

    /// <summary>
    /// Depreciation Detail Item DTO - تفاصيل سجل الإهلاك
    /// </summary>
    public class DepreciationDetailItemDto
    {
        public Guid AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public decimal DepreciationAmount { get; set; }
        public decimal BookValueAfter { get; set; }
        public Guid? JournalId { get; set; }
    }
}

