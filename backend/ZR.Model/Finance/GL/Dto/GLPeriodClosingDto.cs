using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Enums;

namespace ZR.Model.Finance.GL.Dto
{
    /// <summary>
    /// Period Closing Query DTO - استعلام إقفال الفترات
    /// </summary>
    public class GLPeriodClosingQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public PeriodType? PeriodType { get; set; }
        public PeriodStatus? Status { get; set; }
        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }
        public long? TenantId { get; set; }

        
    }

    /// <summary>
    /// Create Period Closing DTO - إنشاء إقفال فترة
    /// </summary>
    public class CreatePeriodClosingDto
    {
        [Required(ErrorMessage = "نوع الفترة مطلوب")]
        public PeriodType PeriodType { get; set; }

        [Required(ErrorMessage = "تاريخ بداية الفترة مطلوب")]
        public DateTime PeriodStartDate { get; set; }

        [Required(ErrorMessage = "تاريخ نهاية الفترة مطلوب")]
        public DateTime PeriodEndDate { get; set; }

        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        public long? TenantId { get; set; }

    
    }

    /// <summary>
    /// Close Period DTO - إقفال فترة
    /// </summary>
    public class ClosePeriodDto
    {
        [Required(ErrorMessage = "معرف الفترة مطلوب")]
        public Guid Id { get; set; }

        [StringLength(1000)]
        public string ClosingNotes { get; set; }

        /// <summary>
        /// Force Close - إقفال إجباري (تجاوز التحقق)
        /// </summary>
        public bool ForceClose { get; set; } = false;
    }

    /// <summary>
    /// Reopen Period DTO - إعادة فتح فترة
    /// </summary>
    public class ReopenPeriodDto
    {
        [Required(ErrorMessage = "معرف الفترة مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "سبب إعادة الفتح مطلوب")]
        [StringLength(1000)]
        public string Reason { get; set; }
    }

    /// <summary>
    /// Period Closing Response DTO - استجابة إقفال الفترة
    /// </summary>
    public class PeriodClosingResponseDto
    {
        [ExcelColumn(Name = "Period ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Period Type")]
        public PeriodType PeriodType { get; set; }

        [ExcelColumn(Name = "Period Type Name")]
        public string PeriodTypeName => GetPeriodTypeName(PeriodType);

        [ExcelColumn(Name = "Period Start", Format = "yyyy-MM-dd")]
        public DateTime PeriodStartDate { get; set; }

        [ExcelColumn(Name = "Period End", Format = "yyyy-MM-dd")]
        public DateTime PeriodEndDate { get; set; }

        [ExcelColumn(Name = "Fiscal Year")]
        public int FiscalYear { get; set; }

        [ExcelColumn(Name = "Fiscal Period")]
        public int FiscalPeriod { get; set; }

        [ExcelColumn(Name = "Status")]
        public PeriodStatus Status { get; set; }

        [ExcelColumn(Name = "Status Name")]
        public string StatusName => GetStatusName(Status);

        [ExcelColumn(Name = "Description (Arabic)")]
        public string DescriptionAr { get; set; }

        [ExcelColumn(Name = "Description (English)")]
        public string DescriptionEn { get; set; }

        [ExcelColumn(Name = "Total Vouchers")]
        public int TotalVouchers { get; set; }

        [ExcelColumn(Name = "Posted Vouchers")]
        public int PostedVouchers { get; set; }

        [ExcelColumn(Name = "Total Debit")]
        public decimal TotalDebit { get; set; }

        [ExcelColumn(Name = "Total Credit")]
        public decimal TotalCredit { get; set; }

        public string ClosingNotes { get; set; }

        [ExcelColumn(Name = "Closed At", Format = "yyyy-MM-dd HH:mm")]
        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedBy { get; set; }
        public DateTime? ReopenedAt { get; set; }
        public Guid? ReopenedBy { get; set; }
        public long? TenantId { get; set; }

  

        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Is Balanced - متوازن
        /// </summary>
        public bool IsBalanced => TotalDebit == TotalCredit;

        /// <summary>
        /// All Vouchers Posted - جميع السندات مرحّلة
        /// </summary>
        public bool AllVouchersPosted => TotalVouchers == PostedVouchers;

        /// <summary>
        /// Can Close - يمكن الإقفال
        /// </summary>
        public bool CanClose => Status == PeriodStatus.Open && IsBalanced && AllVouchersPosted;

        private static string GetPeriodTypeName(PeriodType type) => type switch
        {
            Enums.PeriodType.Month => "Month - شهر",
            Enums.PeriodType.Quarter => "Quarter - ربع سنة",
            Enums.PeriodType.Year => "Year - سنة",
            _ => type.ToString()
        };

        private static string GetStatusName(PeriodStatus status) => status switch
        {
            PeriodStatus.Open => "Open - مفتوحة",
            PeriodStatus.Closed => "Closed - مغلقة",
            PeriodStatus.Adjusted => "Adjusted - معدّلة",
            PeriodStatus.Locked => "Locked - مقفلة نهائياً",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Period Status Response DTO - استجابة حالة الفترة
    /// </summary>
    public class PeriodStatusResponseDto
    {
        public Guid PeriodId { get; set; }
        public PeriodStatus Status { get; set; }
        public string StatusName { get; set; }
        public int TotalVouchers { get; set; }
        public int PostedVouchers { get; set; }
        public int UnpostedVouchers => TotalVouchers - PostedVouchers;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Difference => TotalDebit - TotalCredit;
        public bool IsBalanced => TotalDebit == TotalCredit;
        public bool AllVouchersPosted => TotalVouchers == PostedVouchers;
        public bool CanClose => Status == PeriodStatus.Open && IsBalanced && AllVouchersPosted;
        public string Message { get; set; }
    }
}

