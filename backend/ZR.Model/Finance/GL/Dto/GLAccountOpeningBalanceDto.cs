using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.Finance.GL.Dto
{
    /// <summary>
    /// GL Account Opening Balance Query DTO - استعلام أرصدة الافتتاحية
    /// </summary>
    public class GLAccountOpeningBalanceQueryDto
    {
        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int? FiscalYear { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Account Number - رقم الحساب
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربي
        /// </summary>
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزي
        /// </summary>
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Source Type - نوع المصدر
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// Is Locked - مقفل
        /// </summary>
        public bool? IsLocked { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyID { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public Guid? BranchId { get; set; }
    }

    /// <summary>
    /// Create GL Account Opening Balance DTO - إنشاء رصيد افتتاحي
    /// </summary>
    public class CreateGLAccountOpeningBalanceDto
    {
        /// <summary>
        /// Account ID - معرف الحساب (Required)
        /// </summary>
        [Required(ErrorMessage = "Account ID is required - معرف الحساب مطلوب")]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية (Required)
        /// </summary>
        [Required(ErrorMessage = "Fiscal Year is required - السنة المالية مطلوبة")]
        [Range(2000, 2100, ErrorMessage = "Fiscal Year must be between 2000 and 2100 - السنة المالية يجب أن تكون بين 2000 و 2100")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Opening Debit - المدين الافتتاحي
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Opening Debit cannot be negative - المدين الافتتاحي لا يمكن أن يكون سالباً")]
        public decimal OpeningDebit { get; set; }

        /// <summary>
        /// Opening Credit - الدائن الافتتاحي
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Opening Credit cannot be negative - الدائن الافتتاحي لا يمكن أن يكون سالباً")]
        public decimal OpeningCredit { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [Required(ErrorMessage = "Currency is required - العملة مطلوبة")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters (ISO code) - العملة يجب أن تكون 3 أحرف")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        [Range(0.000001, double.MaxValue, ErrorMessage = "Exchange Rate must be positive - سعر الصرف يجب أن يكون موجباً")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        /// <summary>
        /// Source Type - نوع المصدر
        /// </summary>
        public string SourceType { get; set; } = "Manual";

        /// <summary>
        /// Source Reference - مرجع المصدر
        /// </summary>
        [StringLength(100)]
        public string SourceReference { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyID { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public Guid? BranchId { get; set; }
    }

    /// <summary>
    /// Update GL Account Opening Balance DTO - تحديث رصيد افتتاحي
    /// </summary>
    public class UpdateGLAccountOpeningBalanceDto
    {
        /// <summary>
        /// ID - المعرف (Required)
        /// </summary>
        [Required(ErrorMessage = "ID is required - المعرف مطلوب")]
        public Guid Id { get; set; }

        /// <summary>
        /// Opening Debit - المدين الافتتاحي
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Opening Debit cannot be negative - المدين الافتتاحي لا يمكن أن يكون سالباً")]
        public decimal OpeningDebit { get; set; }

        /// <summary>
        /// Opening Credit - الدائن الافتتاحي
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Opening Credit cannot be negative - الدائن الافتتاحي لا يمكن أن يكون سالباً")]
        public decimal OpeningCredit { get; set; }

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        [Range(0.000001, double.MaxValue, ErrorMessage = "Exchange Rate must be positive - سعر الصرف يجب أن يكون موجباً")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// GL Account Opening Balance List DTO - قائمة الأرصدة الافتتاحية
    /// </summary>
    public class GLAccountOpeningBalanceListDto
    {
        /// <summary>
        /// ID - المعرف
        /// </summary>
        [ExcelColumn(Name = "ID")]
        public Guid Id { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        [ExcelColumn(Name = "Account ID")]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Number - رقم الحساب
        /// </summary>
        [ExcelColumn(Name = "Account Number")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربي
        /// </summary>
        [ExcelColumn(Name = "Account Name (AR)")]
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزي
        /// </summary>
        [ExcelColumn(Name = "Account Name (EN)")]
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [ExcelColumn(Name = "Fiscal Year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Opening Debit - المدين الافتتاحي
        /// </summary>
        [ExcelColumn(Name = "Opening Debit", Format = "N4")]
        public decimal OpeningDebit { get; set; }

        /// <summary>
        /// Opening Credit - الدائن الافتتاحي
        /// </summary>
        [ExcelColumn(Name = "Opening Credit", Format = "N4")]
        public decimal OpeningCredit { get; set; }

        /// <summary>
        /// Opening Balance - الرصيد الافتتاحي (Debit - Credit)
        /// </summary>
        [ExcelColumn(Name = "Opening Balance", Format = "N4")]
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        [ExcelColumn(Name = "Exchange Rate", Format = "N6")]
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Source Type - نوع المصدر
        /// </summary>
        [ExcelColumn(Name = "Source Type")]
        public string SourceTypeDisplay { get; set; }

        /// <summary>
        /// Is Locked - مقفل
        /// </summary>
        [ExcelColumn(Name = "Locked")]
        public bool IsLocked { get; set; }

        /// <summary>
        /// Created Date - تاريخ الإنشاء
        /// </summary>
        [ExcelColumn(Name = "Created Date", Format = "yyyy-MM-dd")]
        public DateTime? Create_time { get; set; }
    }

    /// <summary>
    /// GL Account Opening Balance Detail DTO - تفاصيل الرصيد الافتتاحي
    /// </summary>
    public class GLAccountOpeningBalanceDetailDto
    {
        /// <summary>
        /// ID - المعرف
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Number - رقم الحساب
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربي
        /// </summary>
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزي
        /// </summary>
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Opening Debit - المدين الافتتاحي
        /// </summary>
        public decimal OpeningDebit { get; set; }

        /// <summary>
        /// Opening Credit - الدائن الافتتاحي
        /// </summary>
        public decimal OpeningCredit { get; set; }

        /// <summary>
        /// Opening Balance - الرصيد الافتتاحي
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Base Opening Debit - المدين الافتتاحي بالعملة الأساسية
        /// </summary>
        public decimal BaseOpeningDebit { get; set; }

        /// <summary>
        /// Base Opening Credit - الدائن الافتتاحي بالعملة الأساسية
        /// </summary>
        public decimal BaseOpeningCredit { get; set; }

        /// <summary>
        /// Source Type - نوع المصدر
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// Source Type Display - عرض نوع المصدر
        /// </summary>
        public string SourceTypeDisplay { get; set; }

        /// <summary>
        /// Source Reference - مرجع المصدر
        /// </summary>
        public string SourceReference { get; set; }

        /// <summary>
        /// Is Locked - مقفل
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Locked Date - تاريخ القفل
        /// </summary>
        public DateTime? LockedDate { get; set; }

        /// <summary>
        /// Locked By User ID - المستخدم الذي قام بالقفل
        /// </summary>
        public string LockedByUserId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyID { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Created Date - تاريخ الإنشاء
        /// </summary>
        public DateTime? Create_time { get; set; }

        /// <summary>
        /// Created By - أنشئ بواسطة
        /// </summary>
        public string Create_by { get; set; }

        /// <summary>
        /// Updated Date - تاريخ التحديث
        /// </summary>
        public DateTime? Update_time { get; set; }

        /// <summary>
        /// Updated By - حدث بواسطة
        /// </summary>
        public string Update_by { get; set; }
    }

    /// <summary>
    /// Validation Result DTO - نتيجة التحقق
    /// </summary>
    public class GLOpeningBalanceValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Difference { get; set; }
        public bool IsBalanced => Math.Abs(Difference) < 0.01m;
    }

    /// <summary>
    /// Lock/Unlock Request DTO - طلب قفل/فتح القفل
    /// </summary>
    public class GLOpeningBalanceLockRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public bool Lock { get; set; }

        [StringLength(200)]
        public string Reason { get; set; }
    }
}
