using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury.Dto
{
    /// <summary>
    /// Cheque Query DTO - استعلام شيك
    /// </summary>
    public class ChequeQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string ChequeNumber { get; set; }
        public Guid? BankAccountId { get; set; }
        public ChequeStatus? Status { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Cheque DTO - إنشاء شيك
    /// </summary>
    public class CreateChequeDto
    {
        [Required(ErrorMessage = "رقم الشيك مطلوب")]
        [StringLength(50)]
        public string ChequeNumber { get; set; }

        /// <summary>
        /// Cheque Type - نوع الشيك (صادر/وارد)
        /// 0 = Outgoing, 1 = Incoming
        /// </summary>
        [Required(ErrorMessage = "نوع الشيك مطلوب")]
        public ChequeType ChequeType { get; set; }

        [Required(ErrorMessage = "معرف الحساب البنكي مطلوب")]
        public Guid BankAccountId { get; set; }

        [Required(ErrorMessage = "اسم المستفيد مطلوب")]
        [StringLength(200)]
        public string PayeeName { get; set; }

        /// <summary>
        /// Drawer Name - اسم الساحب (for incoming cheques)
        /// </summary>
        [StringLength(200)]
        public string DrawerName { get; set; }

        /// <summary>
        /// Drawer Bank Name - اسم بنك الساحب (for incoming cheques)
        /// </summary>
        [StringLength(200)]
        public string DrawerBankName { get; set; }

        [Required(ErrorMessage = "تاريخ الإصدار مطلوب")]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "تاريخ الاستحقاق مطلوب")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency Code - رمز العملة (e.g., SAR, USD)
        /// </summary>
        [Required(ErrorMessage = "رمز العملة مطلوب")]
        [StringLength(3)]
        public string Currency { get; set; }

        /// <summary>
        /// Currency ID - معرف العملة (optional FK)
        /// </summary>
        public Guid? CurrencyId { get; set; }

        public Guid? RelatedPaymentId { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [StringLength(100)]
        public string ReferenceNo { get; set; }

        /// <summary>
        /// Memo - الملاحظة
        /// </summary>
        [StringLength(500)]
        public string Memo { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Impacts Available Balance - هل يؤثر على الرصيد المتاح
        /// </summary>
        public bool ImpactsAvailableBalance { get; set; } = true;

        [StringLength(1000)]
        public string Notes { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Cheque DTO - تحديث شيك
    /// </summary>
    public class UpdateChequeDto
    {
        [Required(ErrorMessage = "معرف الشيك مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رقم الشيك مطلوب")]
        [StringLength(50)]
        public string ChequeNumber { get; set; }

        [Required(ErrorMessage = "معرف الحساب البنكي مطلوب")]
        public Guid BankAccountId { get; set; }

        [Required(ErrorMessage = "اسم المستفيد مطلوب")]
        [StringLength(200)]
        public string PayeeName { get; set; }

        [Required(ErrorMessage = "تاريخ الإصدار مطلوب")]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "تاريخ الاستحقاق مطلوب")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        public ChequeStatus Status { get; set; }

        public Guid? RelatedPaymentId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Cheque List DTO - قائمة الشيكات
    /// </summary>
    public class ChequeListDto
    {
        [ExcelColumn(Name = "Cheque ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Cheque Number")]
        public string ChequeNumber { get; set; }

        /// <summary>
        /// Cheque Type - نوع الشيك
        /// 0 = Outgoing (صادر), 1 = Incoming (وارد)
        /// </summary>
        [ExcelColumn(Name = "Cheque Type")]
        public ChequeType ChequeType { get; set; }

        public string ChequeTypeName => ChequeType switch
        {
            ChequeType.Outgoing => "Outgoing - شيك صادر",
            ChequeType.Incoming => "Incoming - شيك وارد",
            _ => ChequeType.ToString()
        };

        [ExcelColumn(Name = "Bank Account ID")]
        public Guid BankAccountId { get; set; }

        [ExcelColumn(Name = "Bank Account")]
        public string BankAccountName { get; set; }

        [ExcelColumn(Name = "Payee Name")]
        public string PayeeName { get; set; }

        [ExcelColumn(Name = "Issue Date", Format = "yyyy-MM-dd")]
        public DateTime IssueDate { get; set; }

        [ExcelColumn(Name = "Due Date", Format = "yyyy-MM-dd")]
        public DateTime DueDate { get; set; }

        [ExcelColumn(Name = "Amount")]
        public decimal Amount { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Status")]
        public ChequeStatus Status { get; set; }

        public string StatusName => Status switch
        {
            ChequeStatus.Issued => "Issued - صادر",
            ChequeStatus.Presented => "Presented - مقدم",
            ChequeStatus.Cleared => "Cleared - مسدد",
            ChequeStatus.Bounced => "Bounced - مرتجع",
            ChequeStatus.Cancelled => "Cancelled - ملغي",
            _ => Status.ToString()
        };
    }

    /// <summary>
    /// Cheque Detail DTO - تفاصيل الشيك
    /// </summary>
    public class ChequeDetailDto
    {
        public Guid Id { get; set; }
        public string ChequeNumber { get; set; }

        /// <summary>
        /// Cheque Type - نوع الشيك
        /// 0 = Outgoing (صادر), 1 = Incoming (وارد)
        /// </summary>
        public ChequeType ChequeType { get; set; }

        public string ChequeTypeName => ChequeType switch
        {
            ChequeType.Outgoing => "Outgoing - شيك صادر",
            ChequeType.Incoming => "Incoming - شيك وارد",
            _ => ChequeType.ToString()
        };

        public Guid BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public string PayeeName { get; set; }

        /// <summary>
        /// Drawer Name - اسم الساحب (for incoming cheques)
        /// </summary>
        public string DrawerName { get; set; }

        /// <summary>
        /// Drawer Bank Name - اسم بنك الساحب (for incoming cheques)
        /// </summary>
        public string DrawerBankName { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ClearedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency Code - رمز العملة
        /// </summary>
        public string Currency { get; set; }

        public ChequeStatus Status { get; set; }
        public string StatusName => Status switch
        {
            ChequeStatus.Issued => "Issued - صادر",
            ChequeStatus.Presented => "Presented - مقدم",
            ChequeStatus.Cleared => "Cleared - مسدد",
            ChequeStatus.Bounced => "Bounced - مرتجع",
            ChequeStatus.Cancelled => "Cancelled - ملغي",
            _ => Status.ToString()
        };

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        public string ReferenceNo { get; set; }

        /// <summary>
        /// Memo - الملاحظة
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// Bounce Reason - سبب الارتجاع
        /// </summary>
        public string BounceReason { get; set; }

        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        public string CancellationReason { get; set; }

        public Guid? RelatedPaymentId { get; set; }
        public string RelatedPaymentNumber { get; set; }
        public string Notes { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Update Cheque Status DTO - تحديث حالة شيك
    /// </summary>
    public class UpdateChequeStatusDto
    {
        [Required(ErrorMessage = "معرف الشيك مطلوب")]
        public Guid ChequeId { get; set; }

        [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
        public ChequeStatus NewStatus { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Cheque Status History DTO - سجل تغييرات حالة الشيك
    /// </summary>
    public class ChequeStatusHistoryDto
    {
        public Guid Id { get; set; }
        public Guid ChequeId { get; set; }
        public int PreviousStatus { get; set; }
        public string PreviousStatusName { get; set; }
        public string PreviousStatusNameAr { get; set; }
        public int NewStatus { get; set; }
        public string NewStatusName { get; set; }
        public string NewStatusNameAr { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public decimal? BankFees { get; set; }
        public string BankReference { get; set; }
        public Guid? GLJournalId { get; set; }
    }

    /// <summary>
    /// Cheque Timeline Item DTO - عنصر الجدول الزمني للشيك
    /// </summary>
    public class ChequeTimelineItemDto
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public string StatusLabel { get; set; }
        public string StatusLabelAr { get; set; }
        public string StatusColor { get; set; }
        public string StatusIcon { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public decimal? BankFees { get; set; }
        public string BankReference { get; set; }
        public Guid? GLJournalId { get; set; }
    }

    /// <summary>
    /// Bounce Cheque Request DTO - طلب ارتجاع الشيك
    /// </summary>
    public class BounceChequeDto
    {
        [Required(ErrorMessage = "سبب الارتجاع مطلوب")]
        [StringLength(500)]
        public string Reason { get; set; }

        /// <summary>
        /// Bank fees charged for bounce - رسوم البنك على الارتجاع
        /// </summary>
        public decimal BankFees { get; set; } = 0;

        /// <summary>
        /// Additional notes - ملاحظات إضافية
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Cancel Cheque Request DTO - طلب إلغاء الشيك
    /// </summary>
    public class CancelChequeDto
    {
        [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
        [StringLength(500)]
        public string Reason { get; set; }

        /// <summary>
        /// Additional notes - ملاحظات إضافية
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Cheque Statistics DTO - إحصائيات الشيكات
    /// </summary>
    public class ChequeStatisticsDto
    {
        public ChequeStatItem Total { get; set; }
        public ChequeStatItem Issued { get; set; }
        public ChequeStatItem Presented { get; set; }
        public ChequeStatItem Cleared { get; set; }
        public ChequeStatItem Bounced { get; set; }
        public ChequeStatItem Cancelled { get; set; }
    }

    /// <summary>
    /// Cheque Statistics Item - عنصر إحصائيات
    /// </summary>
    public class ChequeStatItem
    {
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}






























