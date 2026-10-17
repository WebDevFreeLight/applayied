using System;
using System.ComponentModel.DataAnnotations;
using ZR.Model;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury.Dto
{
    /// <summary>
    /// Treasury Config Query DTO - استعلام إعدادات الخزينة
    /// </summary>
    public class TreasuryConfigQueryDto : PagerInfo
    {
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create Treasury Config DTO - إنشاء إعدادات خزينة
    /// </summary>
    public class CreateTreasuryConfigDto
    {
        public bool AutoImportBankStatements { get; set; }

        public BankStatementImportSource BankStatementImportFormat { get; set; } = BankStatementImportSource.CSV;

        public bool AutoReconcile { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "حد الموافقة يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DefaultPaymentApprovalThreshold { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "حد الصندوق الصغير يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal PettyCashLimit { get; set; }

        public bool ApprovalWorkflowEnabled { get; set; }

        public string DefaultCashAccountId { get; set; }
        public string DefaultBankAccountId { get; set; }
        public string CashGLAccountId { get; set; }
        public string BankGLAccountId { get; set; }
        public string PaymentGLAccountId { get; set; }
        public string ChequeIssuedGLAccountId { get; set; }
        public string ChequeClearedGLAccountId { get; set; }
        public string ReconciliationAdjustmentGLAccountId { get; set; }
        public string AccountsReceivableGLAccountId { get; set; }
        public string Notes { get; set; }

        public  long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Treasury Config DTO - تحديث إعدادات خزينة
    /// </summary>
    public class UpdateTreasuryConfigDto
    {
        [Required(ErrorMessage = "معرف الإعداد مطلوب")]
        public Guid Id { get; set; }

        public bool AutoImportBankStatements { get; set; }

        /// <summary>
        /// Optional to avoid overwriting stored value when client doesn't send it
        /// </summary>
        public BankStatementImportSource? BankStatementImportFormat { get; set; }

        public bool AutoReconcile { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "حد الموافقة يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DefaultPaymentApprovalThreshold { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "حد الصندوق الصغير يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal PettyCashLimit { get; set; }

        public bool ApprovalWorkflowEnabled { get; set; }

        public string DefaultCashAccountId { get; set; }
        public string DefaultBankAccountId { get; set; }
        public string CashGLAccountId { get; set; }
        public string BankGLAccountId { get; set; }
        public string PaymentGLAccountId { get; set; }
        public string ChequeIssuedGLAccountId { get; set; }
        public string ChequeClearedGLAccountId { get; set; }
        public string ReconciliationAdjustmentGLAccountId { get; set; }
        public string AccountsReceivableGLAccountId { get; set; }
        public string Notes { get; set; }

        public  long? TenantId { get; set; }
    }

    /// <summary>
    /// Treasury Config Detail DTO - تفاصيل إعدادات الخزينة
    /// </summary>
    public class TreasuryConfigDetailDto
    {
        public Guid Id { get; set; }
        public bool AutoImportBankStatements { get; set; }
        public BankStatementImportSource BankStatementImportFormat { get; set; }
        public string BankStatementImportFormatName => BankStatementImportFormat switch
        {
            BankStatementImportSource.Manual => "Manual - يدوي",
            BankStatementImportSource.CSV => "CSV",
            BankStatementImportSource.MT940 => "MT940",
            BankStatementImportSource.API => "API - واجهة برمجية",
            _ => BankStatementImportFormat.ToString()
        };
        public bool AutoReconcile { get; set; }
        public decimal DefaultPaymentApprovalThreshold { get; set; }
        public decimal PettyCashLimit { get; set; }
        public bool ApprovalWorkflowEnabled { get; set; }

        // GL Account Mappings
        public string DefaultCashAccountId { get; set; }
        public string DefaultBankAccountId { get; set; }
        public string CashGLAccountId { get; set; }
        public string BankGLAccountId { get; set; }
        public string PaymentGLAccountId { get; set; }
        public string ChequeIssuedGLAccountId { get; set; }
        public string ChequeClearedGLAccountId { get; set; }
        public string ReconciliationAdjustmentGLAccountId { get; set; }
        public string AccountsReceivableGLAccountId { get; set; }
        public string Notes { get; set; }

        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}

















