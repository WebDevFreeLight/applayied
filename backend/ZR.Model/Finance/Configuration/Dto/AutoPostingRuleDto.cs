using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Configuration.Enums;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// Auto Posting Rule Query DTO - استعلام قاعدة الترحيل التلقائي
    /// </summary>
    public class AutoPostingRuleQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public TransactionType? TransactionType { get; set; }
        public bool? Active { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Auto Posting Rule DTO - إنشاء قاعدة ترحيل تلقائي جديدة
    /// </summary>
    public class CreateAutoPostingRuleDto
    {
        [Required(ErrorMessage = "نوع المعاملة مطلوب")]
        public TransactionType TransactionType { get; set; }

        public Guid? DebitAccountId { get; set; }

        public Guid? CreditAccountId { get; set; }

        [StringLength(500)]
        public string DescriptionTemplateAr { get; set; }

        [StringLength(500)]
        public string DescriptionTemplateEn { get; set; }

        public bool Active { get; set; } = true;

        public int Priority { get; set; } = 0;

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Auto Posting Rule DTO - تحديث قاعدة ترحيل تلقائي
    /// </summary>
    public class UpdateAutoPostingRuleDto
    {
        [Required(ErrorMessage = "معرف القاعدة مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "نوع المعاملة مطلوب")]
        public TransactionType TransactionType { get; set; }

        public Guid? DebitAccountId { get; set; }

        public Guid? CreditAccountId { get; set; }

        [StringLength(500)]
        public string DescriptionTemplateAr { get; set; }

        [StringLength(500)]
        public string DescriptionTemplateEn { get; set; }

        public bool Active { get; set; } = true;

        public int Priority { get; set; } = 0;

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Auto Posting Rule List DTO - قائمة قواعد الترحيل التلقائي
    /// </summary>
    public class AutoPostingRuleListDto
    {
        [ExcelColumn(Name = "Rule ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [ExcelColumn(Name = "Debit Account ID")]
        public Guid? DebitAccountId { get; set; }

        [ExcelColumn(Name = "Debit Account Name")]
        public string DebitAccountName { get; set; }

        [ExcelColumn(Name = "Credit Account ID")]
        public Guid? CreditAccountId { get; set; }

        [ExcelColumn(Name = "Credit Account Name")]
        public string CreditAccountName { get; set; }

        [ExcelColumn(Name = "Active")]
        public bool Active { get; set; }

        [ExcelColumn(Name = "Priority")]
        public int Priority { get; set; }

        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Auto Posting Rule Detail DTO - تفاصيل قاعدة الترحيل التلقائي
    /// </summary>
    public class AutoPostingRuleDetailDto : AutoPostingRuleListDto
    {
        public string DescriptionTemplateAr { get; set; }
        public string DescriptionTemplateEn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}


