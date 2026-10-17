using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Assets.Enums;

namespace ZR.Model.Finance.Assets.Dto
{
    /// <summary>
    /// Asset Transaction Query DTO - استعلام معاملات الأصول
    /// </summary>
    public class AssetTransactionQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public Guid? AssetId { get; set; }
        public AssetTransactionType? TransactionType { get; set; }
        public DateTime? TransactionDateFrom { get; set; }
        public DateTime? TransactionDateTo { get; set; }
        public string ReferenceNumber { get; set; }
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Asset Transaction DTO - إنشاء معاملة أصل
    /// </summary>
    public class CreateAssetTransactionDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid AssetId { get; set; }

        [Required(ErrorMessage = "نوع المعاملة مطلوب")]
        public AssetTransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "تاريخ المعاملة مطلوب")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gain/Loss amount - used for sale transactions
        /// </summary>
        public decimal? GainLoss { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Optional GL Journal ID for integration - معرف القيد المحاسبي
        /// </summary>
        public Guid? GLJournalId { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Asset Transaction List DTO - قائمة معاملات الأصول
    /// </summary>
    public class AssetTransactionListDto
    {
        [ExcelColumn(Name = "Transaction ID")]
        public Guid Id { get; set; }

        public Guid AssetId { get; set; }

        [ExcelColumn(Name = "Asset Code")]
        public string AssetCode { get; set; }

        [ExcelColumn(Name = "Asset Name")]
        public string AssetName { get; set; }

        [ExcelColumn(Name = "Transaction Type")]
        public AssetTransactionType TransactionType { get; set; }

        public string TransactionTypeName => TransactionType switch
        {
            AssetTransactionType.Purchase => "Purchase - شراء",
            AssetTransactionType.Depreciation => "Depreciation - إهلاك",
            AssetTransactionType.Sale => "Sale - بيع",
            AssetTransactionType.Disposal => "Disposal - استبعاد",
            AssetTransactionType.Revaluation => "Revaluation - إعادة تقييم",
            _ => TransactionType.ToString()
        };

        [ExcelColumn(Name = "Transaction Date", Format = "yyyy-MM-dd")]
        public DateTime TransactionDate { get; set; }

        [ExcelColumn(Name = "Amount")]
        public decimal Amount { get; set; }

        [ExcelColumn(Name = "Previous Book Value")]
        public decimal PreviousBookValue { get; set; }

        [ExcelColumn(Name = "New Book Value")]
        public decimal NewBookValue { get; set; }

        [ExcelColumn(Name = "Gain/Loss")]
        public decimal? GainLossAmount { get; set; }

        [ExcelColumn(Name = "Reference Number")]
        public string ReferenceNumber { get; set; }

        [ExcelColumn(Name = "Notes")]
        public string Notes { get; set; }

        public Guid? GLJournalId { get; set; }

        [ExcelColumn(Name = "Created At", Format = "yyyy-MM-dd HH:mm")]
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Asset Transaction Detail DTO - تفاصيل معاملة الأصل
    /// </summary>
    public class AssetTransactionDetailDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public AssetTransactionType TransactionType { get; set; }
        public string TransactionTypeName => TransactionType switch
        {
            AssetTransactionType.Purchase => "Purchase - شراء",
            AssetTransactionType.Depreciation => "Depreciation - إهلاك",
            AssetTransactionType.Sale => "Sale - بيع",
            AssetTransactionType.Disposal => "Disposal - استبعاد",
            AssetTransactionType.Revaluation => "Revaluation - إعادة تقييم",
            _ => TransactionType.ToString()
        };
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBookValue { get; set; }
        public decimal NewBookValue { get; set; }
        public decimal? GainLossAmount { get; set; }
        public string Notes { get; set; }
        public string ReferenceNumber { get; set; }
        public Guid? GLJournalId { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}

