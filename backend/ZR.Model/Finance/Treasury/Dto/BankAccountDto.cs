using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury.Dto
{
    /// <summary>
    /// Bank Account Query DTO - استعلام الحساب البنكي
    /// </summary>
    public class BankAccountQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public AccountStatus? Status { get; set; }
        public string Currency { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create Bank Account DTO - إنشاء حساب بنكي
    /// Now supports automatic GL account creation and bank code auto-generation
    /// </summary>
    public class CreateBankAccountDto
    {
        /// <summary>
        /// Bank Code - رمز البنك
        /// OPTIONAL: If not provided, auto-generated from bank name and account number
        /// اختياري: إذا لم يتم توفيره، يتم توليده تلقائياً
        /// </summary>
        [StringLength(50)]
        public string BankCode { get; set; }

        /// <summary>
        /// Bank Name in Arabic - اسم البنك بالعربي
        /// </summary>
        [StringLength(200)]
        public string BankNameAr { get; set; }

        [Required(ErrorMessage = "اسم البنك بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string BankNameEn { get; set; }

        [Required(ErrorMessage = "رقم الحساب البنكي مطلوب")]
        [StringLength(100)]
        public string AccountNumber { get; set; }

        [StringLength(34)]
        public string IBAN { get; set; }

        [StringLength(11)]
        public string SwiftCode { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة")]
        [StringLength(3)]
        public string Currency { get; set; }

        [StringLength(200)]
        public string Branch { get; set; }

        [StringLength(500)]
        public string BankContact { get; set; }

        public AccountStatus Status { get; set; } = AccountStatus.Active;

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }

        #region GL Account Settings

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// OPTIONAL: If not provided, a new GL account will be auto-created.
        /// If provided, the bank account will be linked to the existing GL account.
        /// اختياري: إذا لم يتم توفيره، سيتم إنشاء حساب دفتر أستاذ جديد تلقائياً
        /// </summary>
        public Guid? GLAccountId { get; set; }

        /// <summary>
        /// Parent GL Account ID - معرف الحساب الأب
        /// REQUIRED when AutoCreateGLAccount is true: Specifies where to place the new GL account
        /// مطلوب عند الإنشاء التلقائي: يحدد الحساب الأب الذي سيوضع تحته الحساب الجديد
        /// </summary>
        [Required(ErrorMessage = "الحساب الأب مطلوب - Parent account is required")]
        public Guid? ParentGLAccountId { get; set; }

        /// <summary>
        /// GL Account code - رقم حساب دفتر الأستاذ
        /// REQUIRED when AutoCreateGLAccount is true: The account number for the new GL account
        /// مطلوب عند الإنشاء التلقائي: رقم حساب دفتر الأستاذ الجديد
        /// </summary>
        [Required(ErrorMessage = "رقم حساب دفتر الأستاذ مطلوب - GL account number is required")]
        [StringLength(50)]
        public string CustomGLAccountCode { get; set; }

        /// <summary>
        /// Whether to auto-create GL account - إنشاء حساب دفتر الأستاذ تلقائياً
        /// Default: true (auto-create if GLAccountId not provided)
        /// </summary>
        public bool AutoCreateGLAccount { get; set; } = true;

        #endregion
    }

    /// <summary>
    /// Update Bank Account DTO - تحديث حساب بنكي
    /// </summary>
    public class UpdateBankAccountDto
    {
        [Required(ErrorMessage = "معرف الحساب مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز البنك مطلوب")]
        [StringLength(50)]
        public string BankCode { get; set; }

        [Required(ErrorMessage = "اسم البنك بالعربي مطلوب")]
        [StringLength(200)]
        public string BankNameAr { get; set; }

        [Required(ErrorMessage = "اسم البنك بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string BankNameEn { get; set; }

        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        [StringLength(100)]
        public string AccountNumber { get; set; }

        [StringLength(34)]
        public string IBAN { get; set; }

        [StringLength(11)]
        public string SwiftCode { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة")]
        [StringLength(3)]
        public string Currency { get; set; }

        [StringLength(200)]
        public string Branch { get; set; }

        [StringLength(500)]
        public string BankContact { get; set; }

        public AccountStatus Status { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// Required for update - the linked GL account
        /// مطلوب للتحديث - حساب دفتر الأستاذ المرتبط
        /// </summary>
        [Required(ErrorMessage = "GL Account is required - حساب دفتر الأستاذ مطلوب")]
        public Guid GLAccountId { get; set; }
    }

    /// <summary>
    /// Bank Account List DTO - قائمة الحسابات البنكية
    /// </summary>
    public class BankAccountListDto
    {
        [ExcelColumn(Name = "Account ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Bank Code")]
        public string BankCode { get; set; }

        [ExcelColumn(Name = "Bank Name (Arabic)")]
        public string BankNameAr { get; set; }

        [ExcelColumn(Name = "Bank Name (English)")]
        public string BankNameEn { get; set; }

        [ExcelColumn(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [ExcelColumn(Name = "IBAN")]
        public string IBAN { get; set; }

        [ExcelColumn(Name = "SWIFT Code")]
        public string SwiftCode { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Status")]
        public AccountStatus Status { get; set; }

        public string StatusName => Status switch
        {
            AccountStatus.Active => "Active - نشط",
            AccountStatus.Inactive => "Inactive - غير نشط",
            AccountStatus.Closed => "Closed - مغلق",
            _ => Status.ToString()
        };

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// Computed from GL Account: Opening Balance + (Debits - Credits)
        /// </summary>
        [ExcelColumn(Name = "Current Balance")]
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Available Balance - الرصيد المتاح
        /// Current Balance - Reserved/Pending Amounts
        /// </summary>
        [ExcelColumn(Name = "Available Balance")]
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// </summary>
        public Guid? GLAccountId { get; set; }
    }

    /// <summary>
    /// Bank Account Detail DTO - تفاصيل الحساب البنكي
    /// </summary>
    public class BankAccountDetailDto
    {
        public Guid Id { get; set; }
        public string BankCode { get; set; }
        public string BankNameAr { get; set; }
        public string BankNameEn { get; set; }
        public string AccountNumber { get; set; }
        public string IBAN { get; set; }
        public string SwiftCode { get; set; }
        public string Currency { get; set; }
        public string Branch { get; set; }
        public string BankContact { get; set; }
        public DateTime? ReconciliationDate { get; set; }
        public AccountStatus Status { get; set; }
        public string StatusName => Status switch
        {
            AccountStatus.Active => "Active - نشط",
            AccountStatus.Inactive => "Inactive - غير نشط",
            AccountStatus.Closed => "Closed - مغلق",
            _ => Status.ToString()
        };
        public string Notes { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ المرتبط
        /// </summary>
        public Guid? GLAccountId { get; set; }
    }

    /// <summary>
    /// Bank Account Balance DTO - رصيد الحساب البنكي
    /// Balance information retrieved from GLAccountOpeningBalance and computed from journal entries
    /// </summary>
    public class BankAccountBalanceDto
    {
        public Guid BankAccountId { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public DateTime? LastReconciliationDate { get; set; }

        /// <summary>
        /// Opening Balance for the fiscal year - الرصيد الافتتاحي
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// Opening Balance + (Total Debits - Total Credits)
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Available Balance - الرصيد المتاح
        /// Current Balance - Reserved Amounts (pending cheques, etc.)
        /// </summary>
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// Total Debits in period - إجمالي المدين
        /// </summary>
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Total Credits in period - إجمالي الدائن
        /// </summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Pending Cheques Amount - مبلغ الشيكات المعلقة
        /// </summary>
        public decimal PendingChequesAmount { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Balance as of date - الرصيد كما في التاريخ
        /// </summary>
        public DateTime AsOfDate { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// </summary>
        public Guid? GLAccountId { get; set; }
    }
}






























