using System;
using SqlSugar;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// Auto Posting Rule - قاعدة الترحيل التلقائي
    /// </summary>
    [SugarTable("finance_auto_posting_rule")]
    public class AutoPostingRule : SysBase
    {
        /// <summary>
        /// Rule ID - معرف القاعدة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Transaction Type - نوع المعاملة
        /// </summary>
        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// Debit Account ID (FK to GL Account) - حساب المدين
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? DebitAccountId { get; set; }

        /// <summary>
        /// Credit Account ID (FK to GL Account) - حساب الدائن
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? CreditAccountId { get; set; }

        /// <summary>
        /// Description Template (Arabic) - قالب الوصف بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string DescriptionTemplateAr { get; set; }

        /// <summary>
        /// Description Template (English) - قالب الوصف بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string DescriptionTemplateEn { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Priority - الأولوية (for multiple rules)
        /// </summary>
        public int Priority { get; set; } = 0;
    }
}


