using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Enums;

namespace ZR.Model.Finance.GL.Dto
{
    #region Query DTOs

    /// <summary>
    /// Account Query DTO - استعلام الحساب
    /// </summary>
    public class GLAccountQueryDto : PagerInfo
    {
        public Guid? AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public AccountType? AccountType { get; set; }
        public NormalBalance? NormalBalance { get; set; }
        public Guid? ParentAccountId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsLeaf { get; set; }
        public bool? AllowManualEntries { get; set; }

        /// <summary>
        /// Dimension Requirements Filter - فلتر متطلبات الأبعاد المحاسبية
        /// </summary>
        public Enums.DimensionRequirements? DimensionRequirements { get; set; }

        /// <summary>
        /// Account Nature Filter - فلتر طبيعة الحساب (نقدي/بنكي)
        /// </summary>
        public Enums.AccountNature? AccountNature { get; set; }

        public bool? IsControlAccount { get; set; }

        /// <summary>
        /// Reconciliation Mode Filter - فلتر وضع التسوية
        /// </summary>
        public Enums.ReconciliationMode? ReconciliationMode { get; set; }

        public int? Level { get; set; }
        public string Currency { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }

        // Cash Flow Classification Filters
        public Enums.CashFlowCategory? CashFlowCategory { get; set; }
        public Enums.GLAccountFlags? AccountFlags { get; set; }
        public Enums.WorkingCapitalType? WorkingCapitalType { get; set; }
    }

    #endregion

    #region Create/Update DTOs

    /// <summary>
    /// Create Account DTO - إنشاء حساب جديد
    /// </summary>
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "اسم الحساب بالعربي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم الحساب بالعربي يجب ألا يتجاوز 200 حرف")]
        public string AccountNameAr { get; set; }

        [Required(ErrorMessage = "Account name in English is required")]
        [StringLength(200, ErrorMessage = "Account name must not exceed 200 characters")]
        public string AccountNameEn { get; set; }

        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        [StringLength(50, ErrorMessage = "رقم الحساب يجب ألا يتجاوز 50 حرف")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "نوع الحساب مطلوب")]
        public AccountType AccountType { get; set; }

        [Required(ErrorMessage = "طبيعة الحساب مطلوبة")]
        public NormalBalance NormalBalance { get; set; } = Enums.NormalBalance.Debit;

        public int Level { get; set; }

        public Guid? ParentAccountId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsLeaf { get; set; } = true;

        public bool AllowManualEntries { get; set; } = true;

        public bool AllowPostingToParent { get; set; } = false;

        /// <summary>
        /// Dimension Requirements - متطلبات الأبعاد المحاسبية
        /// Flags to indicate which dimensions are required for journal entries on this account
        /// </summary>
        public Enums.DimensionRequirements DimensionRequirements { get; set; } = Enums.DimensionRequirements.None;

        /// <summary>
        /// Requires Cost Center - يتطلب مركز تكلفة (backward compatibility)
        /// </summary>
        public bool RequiresCostCenter
        {
            get => DimensionRequirements.HasFlag(Enums.DimensionRequirements.CostCenter);
            set
            {
                if (value)
                    DimensionRequirements |= Enums.DimensionRequirements.CostCenter;
                else
                    DimensionRequirements &= ~Enums.DimensionRequirements.CostCenter;
            }
        }

        /// <summary>
        /// Requires Department - يتطلب قسم (backward compatibility)
        /// </summary>
        public bool RequiresDepartment
        {
            get => DimensionRequirements.HasFlag(Enums.DimensionRequirements.Department);
            set
            {
                if (value)
                    DimensionRequirements |= Enums.DimensionRequirements.Department;
                else
                    DimensionRequirements &= ~Enums.DimensionRequirements.Department;
            }
        }

        /// <summary>
        /// Requires Project - يتطلب مشروع (backward compatibility)
        /// </summary>
        public bool RequiresProject
        {
            get => DimensionRequirements.HasFlag(Enums.DimensionRequirements.Project);
            set
            {
                if (value)
                    DimensionRequirements |= Enums.DimensionRequirements.Project;
                else
                    DimensionRequirements &= ~Enums.DimensionRequirements.Project;
            }
        }

        /// <summary>
        /// Account Nature - طبيعة الحساب (نقدي/بنكي)
        /// None = neither, Cash = petty cash, Bank = bank account
        /// </summary>
        public Enums.AccountNature AccountNature { get; set; } = Enums.AccountNature.None;

        public bool IsControlAccount { get; set; } = false;

        /// <summary>
        /// Reconciliation Mode - وضع التسوية
        /// None = no reconciliation needed, Required = periodic reconciliation required
        /// </summary>
        public Enums.ReconciliationMode ReconciliationMode { get; set; } = Enums.ReconciliationMode.None;

        /// <summary>
        /// Is Retained Earnings Account - حساب الأرباح المحتجزة
        /// Only ONE account per company can have this flag set to true
        /// Used for Year-End closing to transfer net income/loss
        /// </summary>
        public bool IsRetainedEarningsAccount { get; set; } = false;

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        public bool IsForeignCurrency { get; set; } = false;

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        [StringLength(50)]
        public string ExternalCode { get; set; }

        [StringLength(20)]
        public string TaxCode { get; set; }

        public int DisplayOrder { get; set; } = 0;

        #region Cash Flow Classification (IAS 7)

        /// <summary>
        /// Cash Flow Category - تصنيف التدفق النقدي
        /// Operating, Investing, Financing, or None
        /// </summary>
        public Enums.CashFlowCategory CashFlowCategory { get; set; } = Enums.CashFlowCategory.None;

        /// <summary>
        /// Account Classification Flags - أعلام تصنيف الحساب
        /// Bitwise flags: Depreciation, Amortization, Provision, WorkingCapital, etc.
        /// </summary>
        public Enums.GLAccountFlags AccountFlags { get; set; } = Enums.GLAccountFlags.None;

        /// <summary>
        /// Working Capital Type - نوع رأس المال العامل
        /// Only applicable when AccountFlags includes WorkingCapital
        /// </summary>
        public Enums.WorkingCapitalType? WorkingCapitalType { get; set; }

        #endregion
    }

    /// <summary>
    /// Update Account DTO - تحديث حساب
    /// </summary>
    public class UpdateAccountDto
    {
        [Required(ErrorMessage = "معرف الحساب مطلوب")]
        public Guid AccountId { get; set; }

        [Required(ErrorMessage = "اسم الحساب بالعربي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم الحساب بالعربي يجب ألا يتجاوز 200 حرف")]
        public string AccountNameAr { get; set; }

        [Required(ErrorMessage = "Account name in English is required")]
        [StringLength(200, ErrorMessage = "Account name must not exceed 200 characters")]
        public string AccountNameEn { get; set; }

        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        [StringLength(50, ErrorMessage = "رقم الحساب يجب ألا يتجاوز 50 حرف")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "نوع الحساب مطلوب")]
        public AccountType AccountType { get; set; }

        [Required(ErrorMessage = "طبيعة الحساب مطلوبة")]
        public NormalBalance NormalBalance { get; set; } = Enums.NormalBalance.Debit;

        public int Level { get; set; }

        public Guid? ParentAccountId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsLeaf { get; set; } = true;

        public bool AllowManualEntries { get; set; } = true;

        public bool AllowPostingToParent { get; set; } = false;

        /// <summary>
        /// Dimension Requirements - متطلبات الأبعاد المحاسبية
        /// Flags to indicate which dimensions are required for journal entries on this account
        /// </summary>
        public Enums.DimensionRequirements DimensionRequirements { get; set; } = Enums.DimensionRequirements.None;

        /// <summary>
        /// Requires Cost Center - يتطلب مركز تكلفة (backward compatibility)
        /// </summary>
        public bool RequiresCostCenter
        {
            get => DimensionRequirements.HasFlag(Enums.DimensionRequirements.CostCenter);
            set
            {
                if (value)
                    DimensionRequirements |= Enums.DimensionRequirements.CostCenter;
                else
                    DimensionRequirements &= ~Enums.DimensionRequirements.CostCenter;
            }
        }

        /// <summary>
        /// Requires Department - يتطلب قسم (backward compatibility)
        /// </summary>
        public bool RequiresDepartment
        {
            get => DimensionRequirements.HasFlag(Enums.DimensionRequirements.Department);
            set
            {
                if (value)
                    DimensionRequirements |= Enums.DimensionRequirements.Department;
                else
                    DimensionRequirements &= ~Enums.DimensionRequirements.Department;
            }
        }

        /// <summary>
        /// Requires Project - يتطلب مشروع (backward compatibility)
        /// </summary>
        public bool RequiresProject
        {
            get => DimensionRequirements.HasFlag(Enums.DimensionRequirements.Project);
            set
            {
                if (value)
                    DimensionRequirements |= Enums.DimensionRequirements.Project;
                else
                    DimensionRequirements &= ~Enums.DimensionRequirements.Project;
            }
        }

        /// <summary>
        /// Account Nature - طبيعة الحساب (نقدي/بنكي)
        /// None = neither, Cash = petty cash, Bank = bank account
        /// </summary>
        public Enums.AccountNature AccountNature { get; set; } = Enums.AccountNature.None;

        public bool IsControlAccount { get; set; } = false;

        /// <summary>
        /// Reconciliation Mode - وضع التسوية
        /// None = no reconciliation needed, Required = periodic reconciliation required
        /// </summary>
        public Enums.ReconciliationMode ReconciliationMode { get; set; } = Enums.ReconciliationMode.None;

        /// <summary>
        /// Is Retained Earnings Account - حساب الأرباح المحتجزة
        /// Only ONE account per company can have this flag set to true
        /// Used for Year-End closing to transfer net income/loss
        /// </summary>
        public bool IsRetainedEarningsAccount { get; set; } = false;

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        public bool IsForeignCurrency { get; set; } = false;

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        [StringLength(50)]
        public string ExternalCode { get; set; }

        [StringLength(20)]
        public string TaxCode { get; set; }

        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Row version for optimistic concurrency
        /// </summary>
        public int RowVersion { get; set; }

        #region Cash Flow Classification (IAS 7)

        /// <summary>
        /// Cash Flow Category - تصنيف التدفق النقدي
        /// Operating, Investing, Financing, or None
        /// </summary>
        public Enums.CashFlowCategory CashFlowCategory { get; set; } = Enums.CashFlowCategory.None;

        /// <summary>
        /// Account Classification Flags - أعلام تصنيف الحساب
        /// Bitwise flags: Depreciation, Amortization, Provision, WorkingCapital, etc.
        /// </summary>
        public Enums.GLAccountFlags AccountFlags { get; set; } = Enums.GLAccountFlags.None;

        /// <summary>
        /// Working Capital Type - نوع رأس المال العامل
        /// Only applicable when AccountFlags includes WorkingCapital
        /// </summary>
        public Enums.WorkingCapitalType? WorkingCapitalType { get; set; }

        #endregion
    }

    #endregion

    #region Response DTOs

    /// <summary>
    /// Account Response DTO - استجابة الحساب
    /// </summary>
    public class AccountResponseDto
    {
        [ExcelColumn(Name = "Account ID")]
        public Guid AccountId { get; set; }

        [ExcelColumn(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [ExcelColumn(Name = "Full Account Number")]
        public string FullAccountNumber { get; set; }

        [ExcelColumn(Name = "Account Name (Arabic)")]
        public string AccountNameAr { get; set; }

        [ExcelColumn(Name = "Account Name (English)")]
        public string AccountNameEn { get; set; }

        [ExcelColumn(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [ExcelColumn(Name = "Account Type Name")]
        public string AccountTypeName => AccountType.ToString();

        [ExcelColumn(Name = "Normal Balance")]
        public NormalBalance NormalBalance { get; set; }

        [ExcelColumn(Name = "Normal Balance Name")]
        public string NormalBalanceName => NormalBalance.ToString();

        [ExcelColumn(Name = "Level")]
        public int Level { get; set; }

        [ExcelColumn(Name = "Parent Account ID")]
        public Guid? ParentAccountId { get; set; }

        [ExcelColumn(Name = "Is Active")]
        public bool IsActive { get; set; }

        [ExcelColumn(Name = "Is Leaf")]
        public bool IsLeaf { get; set; }

        [ExcelColumn(Name = "Allow Manual Entries")]
        public bool AllowManualEntries { get; set; }

        [ExcelColumn(Name = "Allow Posting To Parent")]
        public bool AllowPostingToParent { get; set; }

        /// <summary>
        /// Dimension Requirements - متطلبات الأبعاد المحاسبية
        /// </summary>
        [ExcelColumn(Name = "Dimension Requirements")]
        public Enums.DimensionRequirements DimensionRequirements { get; set; }

        /// <summary>
        /// Dimension Requirements List - قائمة متطلبات الأبعاد
        /// </summary>
        [ExcelColumn(Name = "Dimension Requirements List")]
        public string DimensionRequirementsListStr => string.Join(", ", GetDimensionRequirementsList());

        [ExcelIgnore]
        public List<string> DimensionRequirementsList => GetDimensionRequirementsList();

        [ExcelColumn(Name = "Requires Cost Center")]
        public bool RequiresCostCenter => DimensionRequirements.HasFlag(Enums.DimensionRequirements.CostCenter);

        [ExcelColumn(Name = "Requires Department")]
        public bool RequiresDepartment => DimensionRequirements.HasFlag(Enums.DimensionRequirements.Department);

        [ExcelColumn(Name = "Requires Project")]
        public bool RequiresProject => DimensionRequirements.HasFlag(Enums.DimensionRequirements.Project);

        private List<string> GetDimensionRequirementsList()
        {
            var requirements = new List<string>();
            if (DimensionRequirements == Enums.DimensionRequirements.None) return requirements;

            if (DimensionRequirements.HasFlag(Enums.DimensionRequirements.CostCenter))
                requirements.Add("CostCenter");
            if (DimensionRequirements.HasFlag(Enums.DimensionRequirements.Department))
                requirements.Add("Department");
            if (DimensionRequirements.HasFlag(Enums.DimensionRequirements.Project))
                requirements.Add("Project");

            return requirements;
        }

        /// <summary>
        /// Account Nature - طبيعة الحساب (نقدي/بنكي)
        /// </summary>
        [ExcelColumn(Name = "Account Nature")]
        public Enums.AccountNature AccountNature { get; set; }

        /// <summary>
        /// Account Nature Name - اسم طبيعة الحساب
        /// </summary>
        [ExcelColumn(Name = "Account Nature Name")]
        public string AccountNatureName => AccountNature.ToString();

        /// <summary>
        /// Is Cash Account - حساب نقدي (computed from AccountNature)
        /// </summary>
        [ExcelColumn(Name = "Is Cash Account")]
        public bool IsCashAccount => AccountNature == Enums.AccountNature.Cash;

        /// <summary>
        /// Is Bank Account - حساب بنكي (computed from AccountNature)
        /// </summary>
        [ExcelColumn(Name = "Is Bank Account")]
        public bool IsBankAccount => AccountNature == Enums.AccountNature.Bank;

        [ExcelColumn(Name = "Is Control Account")]
        public bool IsControlAccount { get; set; }

        /// <summary>
        /// Reconciliation Mode - وضع التسوية
        /// </summary>
        [ExcelColumn(Name = "Reconciliation Mode")]
        public Enums.ReconciliationMode ReconciliationMode { get; set; }

        /// <summary>
        /// Reconciliation Mode Name - اسم وضع التسوية
        /// </summary>
        [ExcelColumn(Name = "Reconciliation Mode Name")]
        public string ReconciliationModeName => ReconciliationMode.ToString();

        /// <summary>
        /// Is Reconciliation Account - حساب تسوية (computed from ReconciliationMode)
        /// </summary>
        [ExcelColumn(Name = "Is Reconciliation Account")]
        public bool IsReconciliationAccount => ReconciliationMode == Enums.ReconciliationMode.Required;

        [ExcelColumn(Name = "Is System Account")]
        public bool IsSystemAccount { get; set; }

        /// <summary>
        /// Is Retained Earnings Account - حساب الأرباح المحتجزة
        /// </summary>
        [ExcelColumn(Name = "Is Retained Earnings Account")]
        public bool IsRetainedEarningsAccount { get; set; }

        #region Reconciliation Tracking Fields

        /// <summary>
        /// Last Reconciled Date - تاريخ آخر تسوية
        /// </summary>
        [ExcelColumn(Name = "Last Reconciled Date")]
        public DateTime? LastReconciledDate { get; set; }

        /// <summary>
        /// Last Reconciled By - آخر من قام بالتسوية
        /// </summary>
        [ExcelColumn(Name = "Last Reconciled By")]
        public string LastReconciledBy { get; set; }

        /// <summary>
        /// Reconciliation Status - حالة التسوية
        /// </summary>
        [ExcelColumn(Name = "Reconciliation Status")]
        public Enums.ReconciliationStatus ReconciliationStatus { get; set; }

        /// <summary>
        /// Reconciliation Status Name - اسم حالة التسوية
        /// </summary>
        [ExcelColumn(Name = "Reconciliation Status Name")]
        public string ReconciliationStatusName => ReconciliationStatus switch
        {
            Enums.ReconciliationStatus.NotReconciled => "Not Reconciled - غير مسوى",
            Enums.ReconciliationStatus.Reconciled => "Reconciled - مسوى",
            Enums.ReconciliationStatus.PendingReview => "Pending Review - معلق للمراجعة",
            _ => "Unknown"
        };

        #endregion

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Is Foreign Currency")]
        public bool IsForeignCurrency { get; set; }

        [ExcelColumn(Name = "Description (Arabic)")]
        public string DescriptionAr { get; set; }

        [ExcelColumn(Name = "Description (English)")]
        public string DescriptionEn { get; set; }

        [ExcelColumn(Name = "External Code")]
        public string ExternalCode { get; set; }

        [ExcelColumn(Name = "Tax Code")]
        public string TaxCode { get; set; }

        [ExcelColumn(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [ExcelColumn(Name = "Account Path")]
        public string AccountPath { get; set; }

        [ExcelColumn(Name = "Row Version")]
        public int RowVersion { get; set; }

        [ExcelColumn(Name = "Last Transaction Date")]
        public DateTime? LastTransactionDate { get; set; }

        #region Cash Flow Classification (IAS 7)

        /// <summary>
        /// Cash Flow Category - تصنيف التدفق النقدي
        /// </summary>
        [ExcelColumn(Name = "Cash Flow Category")]
        public Enums.CashFlowCategory CashFlowCategory { get; set; }

        /// <summary>
        /// Cash Flow Category Name - اسم تصنيف التدفق النقدي
        /// </summary>
        [ExcelColumn(Name = "Cash Flow Category Name")]
        public string CashFlowCategoryName => CashFlowCategory.ToString();

        /// <summary>
        /// Account Classification Flags - أعلام تصنيف الحساب
        /// </summary>
        [ExcelColumn(Name = "Account Flags")]
        public Enums.GLAccountFlags AccountFlags { get; set; }

        /// <summary>
        /// Account Flags Description - وصف أعلام الحساب
        /// </summary>
        [ExcelColumn(Name = "Account Flags List")]
        public string AccountFlagsListStr => string.Join(", ", GetAccountFlagsList());

        [ExcelIgnore]
        public List<string> AccountFlagsList => GetAccountFlagsList();

        /// <summary>
        /// Working Capital Type - نوع رأس المال العامل
        /// </summary>
        [ExcelColumn(Name = "Working Capital Type")]
        public Enums.WorkingCapitalType? WorkingCapitalType { get; set; }

        /// <summary>
        /// Working Capital Type Name - اسم نوع رأس المال العامل
        /// </summary>
        [ExcelColumn(Name = "Working Capital Type Name")]
        public string WorkingCapitalTypeName => WorkingCapitalType?.ToString();

        /// <summary>
        /// Is Non-Cash Adjustment - تعديل غير نقدي
        /// Depreciation, Amortization, Provision, etc.
        /// </summary>
        [ExcelColumn(Name = "Is Non-Cash Adjustment")]
        public bool IsNonCashAdjustment =>
            AccountFlags.HasFlag(Enums.GLAccountFlags.Depreciation) ||
            AccountFlags.HasFlag(Enums.GLAccountFlags.Amortization) ||
            AccountFlags.HasFlag(Enums.GLAccountFlags.Provision) ||
            AccountFlags.HasFlag(Enums.GLAccountFlags.Impairment) ||
            AccountFlags.HasFlag(Enums.GLAccountFlags.GainLossOnDisposal) ||
            AccountFlags.HasFlag(Enums.GLAccountFlags.ForeignExchange);

        private List<string> GetAccountFlagsList()
        {
            var flags = new List<string>();
            if (AccountFlags == Enums.GLAccountFlags.None) return flags;

            foreach (Enums.GLAccountFlags flag in global::System.Enum.GetValues(typeof(Enums.GLAccountFlags)))
            {
                if (flag != Enums.GLAccountFlags.None && AccountFlags.HasFlag(flag))
                {
                    flags.Add(flag.ToString());
                }
            }
            return flags;
        }

        #endregion

        // SysBase fields
        [ExcelColumn(Name = "Created At")]
        public DateTime Create_time { get; set; }

        [ExcelColumn(Name = "Updated At")]
        public DateTime? Update_time { get; set; }

        [ExcelColumn(Name = "Created By")]
        public string Create_by { get; set; }

        [ExcelColumn(Name = "Updated By")]
        public string Update_by { get; set; }

        [ExcelColumn(Name = "Company ID")]
        public long? CompanyID { get; set; }

        [ExcelColumn(Name = "Branch ID")]
        public long? BranchID { get; set; }

        // Navigation properties for tree view
        [ExcelColumn(Name = "Parent Name")]
        public string ParentName { get; set; }

        [ExcelIgnore]
        public List<AccountResponseDto> Children { get; set; }

        // Balance information (computed, not stored in GLAccount)
        [ExcelIgnore]
        public AccountBalanceDto Balance { get; set; }
    }

    /// <summary>
    /// Account Balance DTO - رصيد الحساب
    /// Computed balance information
    /// </summary>
    public class AccountBalanceDto
    {
        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Opening Debit - الرصيد الافتتاحي مدين
        /// </summary>
        public decimal OpeningDebit { get; set; }

        /// <summary>
        /// Opening Credit - الرصيد الافتتاحي دائن
        /// </summary>
        public decimal OpeningCredit { get; set; }

        /// <summary>
        /// Opening Balance (Net) - صافي الرصيد الافتتاحي
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Period Debit - حركة المدين
        /// </summary>
        public decimal PeriodDebit { get; set; }

        /// <summary>
        /// Period Credit - حركة الدائن
        /// </summary>
        public decimal PeriodCredit { get; set; }

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// Opening + Period movements
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// As Of Date - كما في تاريخ
        /// </summary>
        public DateTime? AsOfDate { get; set; }
    }

    #endregion

    #region Opening Balance DTOs

    /// <summary>
    /// Create Opening Balance DTO - إنشاء رصيد افتتاحي
    /// </summary>
    public class CreateOpeningBalanceDto
    {
        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public int FiscalYear { get; set; }

        public DateTime PeriodStartDate { get; set; }

        public DateTime PeriodEndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OpeningDebit { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OpeningCredit { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        public decimal ExchangeRate { get; set; } = 1;

        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update Opening Balance DTO - تحديث رصيد افتتاحي
    /// </summary>
    public class UpdateOpeningBalanceDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public int FiscalYear { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OpeningDebit { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OpeningCredit { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        public decimal ExchangeRate { get; set; } = 1;

        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Opening Balance Response DTO - استجابة الرصيد الافتتاحي
    /// </summary>
    public class OpeningBalanceResponseDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public int FiscalYear { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal OpeningBalance { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal BaseOpeningDebit { get; set; }
        public decimal BaseOpeningCredit { get; set; }
        public string SourceType { get; set; }
        public string SourceReference { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedAt { get; set; }
        public string Notes { get; set; }
        public DateTime Create_time { get; set; }
        public string Create_by { get; set; }
    }

    /// <summary>
    /// Opening Balance Query DTO
    /// </summary>
    public class OpeningBalanceQueryDto : PagerInfo
    {
        public Guid? AccountId { get; set; }
        public int? FiscalYear { get; set; }
        public bool? IsLocked { get; set; }
        public string SourceType { get; set; }
    }

    /// <summary>
    /// Bulk Opening Balance Import DTO - استيراد أرصدة افتتاحية
    /// </summary>
    public class BulkOpeningBalanceDto
    {
        [Required]
        public int FiscalYear { get; set; }

        public DateTime PeriodStartDate { get; set; }

        public DateTime PeriodEndDate { get; set; }

        [Required]
        public List<OpeningBalanceLineDto> Balances { get; set; }
    }

    /// <summary>
    /// Opening Balance Line DTO - سطر رصيد افتتاحي
    /// </summary>
    public class OpeningBalanceLineDto
    {
        [Required]
        public string AccountNumber { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OpeningDebit { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OpeningCredit { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";
    }

    #endregion

    #region Account Validation DTOs

    /// <summary>
    /// Account Validation Result DTO - نتيجة التحقق من الحساب
    /// </summary>
    public class AccountValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
    }

    /// <summary>
    /// Account Tree Node DTO - عقدة شجرة الحسابات
    /// </summary>
    public class AccountTreeNodeDto
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string FullAccountNumber { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public AccountType AccountType { get; set; }
        public int Level { get; set; }
        public Guid? ParentAccountId { get; set; }
        public bool IsLeaf { get; set; }
        public bool IsActive { get; set; }
        public bool HasChildren { get; set; }
        public List<AccountTreeNodeDto> Children { get; set; } = new List<AccountTreeNodeDto>();
    }

    #endregion

    #region Account Balance Summary DTOs

    /// <summary>
    /// Balance Summary Query DTO
    /// </summary>
    public class BalanceSummaryQueryDto
    {
        public Guid? AccountId { get; set; }
        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }
        public DateTime? AsOfDate { get; set; }
        public bool IncludeSubAccounts { get; set; } = false;
    }

    /// <summary>
    /// Balance Summary Response DTO - ملخص رصيد الحساب
    /// </summary>
    public class BalanceSummaryResponseDto
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public AccountType AccountType { get; set; }
        public NormalBalance NormalBalance { get; set; }
        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }

        // Opening
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal OpeningBalance { get; set; }

        // Period Movement
        public decimal PeriodDebit { get; set; }
        public decimal PeriodCredit { get; set; }
        public decimal PeriodNet { get; set; }

        // Closing
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
        public decimal ClosingBalance { get; set; }

        // YTD
        public decimal YtdDebit { get; set; }
        public decimal YtdCredit { get; set; }
        public decimal YtdBalance { get; set; }

        // Metadata
        public int TransactionCount { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public DateTime LastCalculatedAt { get; set; }
    }

    #endregion

    #region Excel Import DTOs

    /// <summary>
    /// Excel Import Row DTO - سطر استيراد من Excel
    /// Matches the Excel template structure for bulk account import
    /// </summary>
    public class ExcelAccountImportRowDto
    {
        [ExcelColumn(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [ExcelColumn(Name = "Full Account Number")]
        public string FullAccountNumber { get; set; }

        [ExcelColumn(Name = "Account Name (EN)")]
        public string AccountNameEn { get; set; }

        [ExcelColumn(Name = "Account Name (AR)")]
        public string AccountNameAr { get; set; }

        [ExcelColumn(Name = "Account Type")]
        public string AccountType { get; set; }

        [ExcelColumn(Name = "Normal Balance")]
        public string NormalBalance { get; set; }

        [ExcelColumn(Name = "Level")]
        public int Level { get; set; }

        [ExcelColumn(Name = "Parent Account Number")]
        public string ParentAccountNumber { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Is Leaf")]
        public string IsLeaf { get; set; }

        [ExcelColumn(Name = "Is Active")]
        public string IsActive { get; set; }

        [ExcelColumn(Name = "Allow Manual Entries")]
        public string AllowManualEntries { get; set; }

        [ExcelColumn(Name = "Allow Posting To Parent")]
        public string AllowPostingToParent { get; set; }

        [ExcelColumn(Name = "Is Cash Account")]
        public string IsCashAccount { get; set; }

        [ExcelColumn(Name = "Is Bank Account")]
        public string IsBankAccount { get; set; }

        [ExcelColumn(Name = "Is Control Account")]
        public string IsControlAccount { get; set; }

        [ExcelColumn(Name = "Is Reconciliation Account")]
        public string IsReconciliationAccount { get; set; }

        [ExcelColumn(Name = "Is System Account")]
        public string IsSystemAccount { get; set; }

        [ExcelColumn(Name = "Is Foreign Currency")]
        public string IsForeignCurrency { get; set; }

        [ExcelColumn(Name = "Cash Flow Category")]
        public string CashFlowCategory { get; set; }

        [ExcelColumn(Name = "Working Capital Type")]
        public string WorkingCapitalType { get; set; }

        [ExcelColumn(Name = "Account Flags")]
        public string AccountFlags { get; set; }

        [ExcelColumn(Name = "Requires Cost Center")]
        public string RequiresCostCenter { get; set; }

        [ExcelColumn(Name = "Requires Department")]
        public string RequiresDepartment { get; set; }

        [ExcelColumn(Name = "Requires Project")]
        public string RequiresProject { get; set; }

        [ExcelColumn(Name = "External Code")]
        public string ExternalCode { get; set; }

        [ExcelColumn(Name = "Tax Code")]
        public string TaxCode { get; set; }

        [ExcelColumn(Name = "Description (EN)")]
        public string DescriptionEn { get; set; }

        [ExcelColumn(Name = "Description (AR)")]
        public string DescriptionAr { get; set; }
    }

    /// <summary>
    /// Excel Import Result DTO - نتيجة الاستيراد من Excel
    /// </summary>
    public class ExcelImportResultDto
    {
        public int SuccessCount { get; set; }
        public int UpdatedCount { get; set; }
        public int FailedCount { get; set; }
        public int TotalRows { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    #endregion
}
