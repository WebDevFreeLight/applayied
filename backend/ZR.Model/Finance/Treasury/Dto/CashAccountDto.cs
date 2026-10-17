using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury.Dto
{
    /// <summary>
    /// Cash Account Query DTO - استعلام الحساب النقدي
    /// </summary>
    public class CashAccountQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public AccountType? Type { get; set; }
        public AccountStatus? Status { get; set; }
        public string Currency { get; set; }
        public Guid? BranchId { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create Cash Account DTO - إنشاء حساب نقدي
    /// </summary>
    public class CreateCashAccountDto
    {
        /// <summary>
        /// Account Code - رمز الحساب
        /// Optional - auto-generated if not provided
        /// </summary>
        [StringLength(50)]
        public string Code { get; set; }

        /// <summary>
        /// Name in Arabic - الاسم بالعربي
        /// Optional
        /// </summary>
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة")]
        [StringLength(3)]
        public string Currency { get; set; }

        public Guid? BranchId { get; set; }

        [Required(ErrorMessage = "نوع الحساب مطلوب")]
        public AccountType Type { get; set; }

        public AccountStatus Status { get; set; } = AccountStatus.Active;

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// Optional if AutoCreateGLAccount is true
        /// </summary>
        public Guid? GLAccountId { get; set; }

        /// <summary>
        /// Auto-create GL Account - إنشاء حساب دفتر الأستاذ تلقائياً
        /// If true, a new GL account will be created under the specified parent
        /// </summary>
        public bool AutoCreateGLAccount { get; set; } = true;

        /// <summary>
        /// Parent GL Account ID - معرف حساب دفتر الأستاذ الأب
        /// Required when AutoCreateGLAccount is true
        /// </summary>
        public Guid? ParentGLAccountId { get; set; }

        /// <summary>
        /// Custom GL Account Code - رقم حساب دفتر الأستاذ المخصص
        /// Optional - if not provided, will be auto-generated
        /// </summary>
        public string CustomGLAccountCode { get; set; }
    }

    /// <summary>
    /// Update Cash Account DTO - تحديث حساب نقدي
    /// </summary>
    public class UpdateCashAccountDto
    {
        [Required(ErrorMessage = "معرف الحساب مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز الحساب مطلوب")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة")]
        [StringLength(3)]
        public string Currency { get; set; }

        public Guid? BranchId { get; set; }

        public AccountType Type { get; set; }

        public AccountStatus Status { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ (مطلوب - يجب أن يكون is_cash_account = true)
        /// Required - must link to GL Account where is_cash_account = true
        /// </summary>
        [Required(ErrorMessage = "GL Account is required - حساب دفتر الأستاذ مطلوب")]
        public Guid GLAccountId { get; set; }
    }

    /// <summary>
    /// Cash Account List DTO - قائمة الحسابات النقدية
    /// </summary>
    public class CashAccountListDto
    {
        [ExcelColumn(Name = "Account ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Code")]
        public string Code { get; set; }

        [ExcelColumn(Name = "Name (Arabic)")]
        public string NameAr { get; set; }

        [ExcelColumn(Name = "Name (English)")]
        public string NameEn { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Type")]
        public AccountType Type { get; set; }

        public string TypeName => Type switch
        {
            AccountType.CashBox => "Cash Box - صندوق نقدي",
            AccountType.PettyCash => "Petty Cash - صندوق صغير",
            _ => Type.ToString()
        };

        [ExcelColumn(Name = "Status")]
        public AccountStatus Status { get; set; }

        public string StatusName => Status switch
        {
            AccountStatus.Active => "Active - نشط",
            AccountStatus.Inactive => "Inactive - غير نشط",
            AccountStatus.Closed => "Closed - مغلق",
            _ => Status.ToString()
        };
    }

    /// <summary>
    /// Cash Account Detail DTO - تفاصيل الحساب النقدي
    /// </summary>
    public class CashAccountDetailDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Currency { get; set; }
        public Guid? BranchId { get; set; }
        public AccountType Type { get; set; }
        public string TypeName => Type switch
        {
            AccountType.CashBox => "Cash Box - صندوق نقدي",
            AccountType.PettyCash => "Petty Cash - صندوق صغير",
            _ => Type.ToString()
        };
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
}






























