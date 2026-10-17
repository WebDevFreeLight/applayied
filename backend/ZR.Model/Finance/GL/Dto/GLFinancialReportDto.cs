using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Enums;

namespace ZR.Model.Finance.GL.Dto
{
    /// <summary>
    /// Financial Report Query DTO - استعلام التقارير المالية
    /// </summary>
    public class GLFinancialReportQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public ReportType? ReportType { get; set; }
        public int? FiscalYear { get; set; }
        public DateTime? BeginPeriodDate { get; set; }
        public DateTime? EndPeriodDate { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Generate Report DTO - توليد تقرير مالي
    /// </summary>
    public class GenerateReportDto
    {
        [Required(ErrorMessage = "نوع التقرير مطلوب")]
        public ReportType ReportType { get; set; }

        [Required(ErrorMessage = "تاريخ بداية الفترة مطلوب")]
        public DateTime PeriodStartDate { get; set; }

        [Required(ErrorMessage = "تاريخ نهاية الفترة مطلوب")]
        public DateTime PeriodEndDate { get; set; }

        public int? FiscalYear { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Financial Report Response DTO - استجابة التقرير المالي
    /// </summary>
    public class FinancialReportResponseDto
    {
        [ExcelColumn(Name = "Report ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Report Type")]
        public ReportType ReportType { get; set; }

        [ExcelColumn(Name = "Report Type Name")]
        public string ReportTypeName => GetReportTypeName(ReportType);

        [ExcelColumn(Name = "Report Number")]
        public string ReportNumber { get; set; }

        [ExcelColumn(Name = "Period Start", Format = "yyyy-MM-dd")]
        public DateTime PeriodStartDate { get; set; }

        [ExcelColumn(Name = "Period End", Format = "yyyy-MM-dd")]
        public DateTime PeriodEndDate { get; set; }

        [ExcelColumn(Name = "Fiscal Year")]
        public int FiscalYear { get; set; }

        [ExcelColumn(Name = "Description (Arabic)")]
        public string DescriptionAr { get; set; }

        [ExcelColumn(Name = "Description (English)")]
        public string DescriptionEn { get; set; }

        [ExcelColumn(Name = "Total Debit")]
        public decimal TotalDebit { get; set; }

        [ExcelColumn(Name = "Total Credit")]
        public decimal TotalCredit { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        public string ReportData { get; set; }

        [ExcelColumn(Name = "Generated At", Format = "yyyy-MM-dd HH:mm")]
        public DateTime GeneratedAt { get; set; }

        public Guid? GeneratedBy { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Is Balanced - متوازن
        /// </summary>
        public bool IsBalanced => TotalDebit == TotalCredit;

        /// <summary>
        /// Report Lines - بنود التقرير (for detailed reports)
        /// </summary>
        public List<ReportLineDto> Lines { get; set; }

        private static string GetReportTypeName(ReportType type) => type switch
        {
            Enums.ReportType.TrialBalance => "Trial Balance - ميزان المراجعة",
            Enums.ReportType.BalanceSheet => "Balance Sheet - الميزانية العمومية",
            Enums.ReportType.IncomeStatement => "Income Statement - قائمة الدخل",
            Enums.ReportType.CashFlow => "Cash Flow - التدفقات النقدية",
            Enums.ReportType.GeneralLedger => "General Ledger - دفتر الأستاذ",
            Enums.ReportType.AccountStatement => "Account Statement - كشف حساب",
            _ => type.ToString()
        };
    }

    /// <summary>
    /// Report Line DTO - بند التقرير
    /// </summary>
    public class ReportLineDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public string AccountType { get; set; }
        public int Level { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal PeriodDebit { get; set; }
        public decimal PeriodCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
        public decimal Balance { get; set; }
    }

    #region Enhanced Trial Balance DTOs - IFRS Compliant

    /// <summary>
    /// Trial Balance Query DTO - استعلام ميزان المراجعة
    /// </summary>
    public class TrialBalanceQueryDto : PagerInfo
    {
        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        [Required(ErrorMessage = "تاريخ بداية الفترة مطلوب")]
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        [Required(ErrorMessage = "تاريخ نهاية الفترة مطلوب")]
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Account Type Filter - فلتر نوع الحساب
        /// </summary>
        public AccountType? AccountType { get; set; }

        /// <summary>
        /// Level Filter - فلتر المستوى (1=Root, 2=Sub, etc.)
        /// </summary>
        public int? Level { get; set; }

        /// <summary>
        /// Include Inactive Accounts - تضمين الحسابات غير النشطة
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Show Zero Balances - عرض الأرصدة الصفرية
        /// </summary>
        public bool ShowZeroBalances { get; set; } = false;

        /// <summary>
        /// Show Hierarchical Structure - عرض الهيكل الهرمي
        /// </summary>
        public bool ShowHierarchy { get; set; } = true;

        /// <summary>
        /// View Mode - وضع العرض
        /// "hierarchical" = Tree view with roll-up totals
        /// "flat" = Flat list without hierarchy or roll-ups
        /// </summary>
        public string ViewMode { get; set; } = "hierarchical";

        /// <summary>
        /// Parent Account ID - معرف الحساب الأب (for subtree queries)
        /// </summary>
        public Guid? ParentAccountId { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Cost Center ID Filter - فلتر مركز التكلفة
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Branch ID Filter - فلتر الفرع
        /// </summary>
        public long? BranchId { get; set; }
    }

    /// <summary>
    /// Enhanced Trial Balance Report DTO - تقرير ميزان المراجعة المحسن (IFRS)
    /// </summary>
    public class TrialBalanceReportDto
    {
        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Company Name - اسم الشركة
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Generated At - تاريخ التوليد
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// View Mode - وضع العرض
        /// "hierarchical" = Tree view with roll-up totals
        /// "flat" = Flat list without hierarchy or roll-ups
        /// </summary>
        public string ViewMode { get; set; } = "hierarchical";

        #region Opening Balance Totals - إجماليات الرصيد الافتتاحي

        /// <summary>
        /// Total Opening Debit - إجمالي المدين الافتتاحي
        /// </summary>
        public decimal TotalOpeningDebit { get; set; }

        /// <summary>
        /// Total Opening Credit - إجمالي الدائن الافتتاحي
        /// </summary>
        public decimal TotalOpeningCredit { get; set; }

        #endregion

        #region Period Movement Totals - إجماليات حركة الفترة

        /// <summary>
        /// Total Period Debit - إجمالي مدين الفترة
        /// </summary>
        public decimal TotalPeriodDebit { get; set; }

        /// <summary>
        /// Total Period Credit - إجمالي دائن الفترة
        /// </summary>
        public decimal TotalPeriodCredit { get; set; }

        #endregion

        #region Closing Balance Totals - إجماليات الرصيد الختامي

        /// <summary>
        /// Total Closing Debit - إجمالي المدين الختامي
        /// </summary>
        public decimal TotalClosingDebit { get; set; }

        /// <summary>
        /// Total Closing Credit - إجمالي الدائن الختامي
        /// </summary>
        public decimal TotalClosingCredit { get; set; }

        #endregion

        /// <summary>
        /// Legacy: Total Debit (equals TotalClosingDebit for backward compatibility)
        /// </summary>
        public decimal TotalDebit => TotalClosingDebit;

        /// <summary>
        /// Legacy: Total Credit (equals TotalClosingCredit for backward compatibility)
        /// </summary>
        public decimal TotalCredit => TotalClosingCredit;

        /// <summary>
        /// Is Opening Balanced - الرصيد الافتتاحي متوازن
        /// </summary>
        public bool IsOpeningBalanced => TotalOpeningDebit == TotalOpeningCredit;

        /// <summary>
        /// Is Period Balanced - حركة الفترة متوازنة
        /// </summary>
        public bool IsPeriodBalanced => TotalPeriodDebit == TotalPeriodCredit;

        /// <summary>
        /// Is Closing Balanced - الرصيد الختامي متوازن
        /// </summary>
        public bool IsClosingBalanced => TotalClosingDebit == TotalClosingCredit;

        /// <summary>
        /// Is Balanced - متوازن (legacy compatibility)
        /// </summary>
        public bool IsBalanced => IsClosingBalanced;

        /// <summary>
        /// Trial Balance Lines - بنود ميزان المراجعة
        /// </summary>
        public List<TrialBalanceLineDto> Lines { get; set; } = new();

        /// <summary>
        /// Total Account Count - عدد الحسابات
        /// </summary>
        public int TotalAccountCount => Lines?.Count ?? 0;
    }

    /// <summary>
    /// Enhanced Trial Balance Line DTO - بند ميزان المراجعة المحسن (IFRS)
    /// </summary>
    public class TrialBalanceLineDto
    {
        #region Account Information - معلومات الحساب

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Code (Number) - رقم الحساب
        /// </summary>
        [ExcelColumn(Name = "Account Code")]
        public string AccountCode { get; set; }

        /// <summary>
        /// Full Account Number - الرقم الكامل للحساب (including parent codes)
        /// Example: 1-100-1001
        /// </summary>
        [ExcelColumn(Name = "Full Account Number")]
        public string FullAccountNumber { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربية
        /// </summary>
        [ExcelColumn(Name = "Account Name (AR)")]
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزية
        /// </summary>
        [ExcelColumn(Name = "Account Name (EN)")]
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Account Type - نوع الحساب
        /// </summary>
        [ExcelColumn(Name = "Account Type")]
        public string AccountType { get; set; }

        /// <summary>
        /// Account Type Enum - نوع الحساب (enum)
        /// </summary>
        public AccountType AccountTypeEnum { get; set; }

        #endregion

        #region Hierarchy Information - معلومات الهيكل الهرمي

        /// <summary>
        /// Account Level - مستوى الحساب
        /// 1 = Root/Main, 2 = Sub-category, etc.
        /// </summary>
        [ExcelColumn(Name = "Level")]
        public int Level { get; set; }

        /// <summary>
        /// Parent Account ID - معرف الحساب الأب
        /// </summary>
        public Guid? ParentAccountId { get; set; }

        /// <summary>
        /// Is Leaf Account - حساب نهائي
        /// </summary>
        public bool IsLeaf { get; set; }

        /// <summary>
        /// Is Expanded - موسع (for UI state)
        /// </summary>
        public bool IsExpanded { get; set; } = false;

        /// <summary>
        /// Children - الحسابات الفرعية
        /// </summary>
        public List<TrialBalanceLineDto> Children { get; set; } = new();

        /// <summary>
        /// Has Children - لديه حسابات فرعية
        /// </summary>
        public bool HasChildren => Children?.Count > 0;

        #endregion

        #region Opening Balance - الرصيد الافتتاحي

        /// <summary>
        /// Opening Debit Balance - رصيد المدين الافتتاحي
        /// </summary>
        [ExcelColumn(Name = "Opening Debit")]
        public decimal OpeningDebit { get; set; }

        /// <summary>
        /// Opening Credit Balance - رصيد الدائن الافتتاحي
        /// </summary>
        [ExcelColumn(Name = "Opening Credit")]
        public decimal OpeningCredit { get; set; }

        /// <summary>
        /// Opening Balance (Net) - الرصيد الافتتاحي الصافي
        /// Positive = Debit balance, Negative = Credit balance
        /// </summary>
        public decimal OpeningBalance => OpeningDebit - OpeningCredit;

        #endregion

        #region Period Movement - حركة الفترة

        /// <summary>
        /// Period Debit - مدين الفترة
        /// </summary>
        [ExcelColumn(Name = "Period Debit")]
        public decimal PeriodDebit { get; set; }

        /// <summary>
        /// Period Credit - دائن الفترة
        /// </summary>
        [ExcelColumn(Name = "Period Credit")]
        public decimal PeriodCredit { get; set; }

        /// <summary>
        /// Period Net Movement - صافي حركة الفترة
        /// </summary>
        public decimal PeriodMovement => PeriodDebit - PeriodCredit;

        /// <summary>
        /// Legacy: Debit (equals PeriodDebit for backward compatibility)
        /// </summary>
        public decimal Debit => PeriodDebit;

        /// <summary>
        /// Legacy: Credit (equals PeriodCredit for backward compatibility)
        /// </summary>
        public decimal Credit => PeriodCredit;

        #endregion

        #region Closing Balance - الرصيد الختامي

        /// <summary>
        /// Closing Debit Balance - رصيد المدين الختامي
        /// Calculated: OpeningDebit + PeriodDebit - (credits applied to debit)
        /// </summary>
        [ExcelColumn(Name = "Closing Debit")]
        public decimal ClosingDebit { get; set; }

        /// <summary>
        /// Closing Credit Balance - رصيد الدائن الختامي
        /// Calculated: OpeningCredit + PeriodCredit - (debits applied to credit)
        /// </summary>
        [ExcelColumn(Name = "Closing Credit")]
        public decimal ClosingCredit { get; set; }

        /// <summary>
        /// Closing Balance (Net) - الرصيد الختامي الصافي
        /// Positive = Debit balance, Negative = Credit balance
        /// </summary>
        public decimal ClosingBalance => ClosingDebit - ClosingCredit;

        /// <summary>
        /// Legacy: Balance (equals ClosingBalance for backward compatibility)
        /// </summary>
        public decimal Balance => ClosingBalance;

        #endregion

        #region Additional Information - معلومات إضافية

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Normal Balance - الرصيد الطبيعي
        /// </summary>
        public NormalBalance NormalBalance { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        #endregion

        /// <summary>
        /// Calculate Closing Balances from Opening + Period
        /// حساب الأرصدة الختامية من الافتتاحي + الفترة
        /// </summary>
        public void CalculateClosingBalances()
        {
            // Net balance approach: Calculate net position and place in appropriate column
            var netOpening = OpeningDebit - OpeningCredit;
            var netPeriod = PeriodDebit - PeriodCredit;
            var netClosing = netOpening + netPeriod;

            if (netClosing >= 0)
            {
                ClosingDebit = netClosing;
                ClosingCredit = 0;
            }
            else
            {
                ClosingDebit = 0;
                ClosingCredit = Math.Abs(netClosing);
            }
        }
    }

    /// <summary>
    /// Trial Balance Summary DTO - ملخص ميزان المراجعة
    /// For quick overview without full detail
    /// </summary>
    public class TrialBalanceSummaryDto
    {
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public int FiscalYear { get; set; }
        public string Currency { get; set; }
        public long? CompanyId { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsBalanced => TotalDebit == TotalCredit;
        public decimal Variance => TotalDebit - TotalCredit;
        public int AccountCount { get; set; }
        public int AccountsWithActivity { get; set; }
    }

    #endregion

    /// <summary>
    /// Balance Sheet Report DTO - تقرير الميزانية العمومية
    /// </summary>
    public class BalanceSheetReportDto
    {
        public DateTime AsOfDate { get; set; }
        public int FiscalYear { get; set; }
        public string Currency { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public bool IsBalanced => TotalAssets == (TotalLiabilities + TotalEquity);
        public List<BalanceSheetSectionDto> Assets { get; set; }
        public List<BalanceSheetSectionDto> Liabilities { get; set; }
        public List<BalanceSheetSectionDto> Equity { get; set; }
    }

    /// <summary>
    /// Balance Sheet Section DTO - قسم الميزانية
    /// </summary>
    public class BalanceSheetSectionDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public int Level { get; set; }
        public decimal Balance { get; set; }
        public List<BalanceSheetSectionDto> Children { get; set; }
    }

    #region IFRS Balance Sheet DTOs - الميزانية العمومية وفق المعايير الدولية

    /// <summary>
    /// IFRS Balance Sheet Query Parameters
    /// معاملات استعلام الميزانية العمومية وفق IFRS
    /// </summary>
    public class BalanceSheetQueryDto
    {
        /// <summary>
        /// As of Date - تاريخ الميزانية
        /// The date for which the balance sheet is prepared
        /// </summary>
        [Required(ErrorMessage = "تاريخ الميزانية مطلوب")]
        public DateTime AsOfDate { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// View Mode - وضع العرض
        /// "hierarchical" = Tree view with roll-up totals
        /// "flat" = Flat list without hierarchy
        /// </summary>
        public string ViewMode { get; set; } = "hierarchical";

        /// <summary>
        /// Include Previous Period - تضمين الفترة السابقة
        /// If true, includes comparative data from same date last year
        /// </summary>
        public bool IncludePreviousPeriod { get; set; } = false;

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";
    }

    /// <summary>
    /// IFRS Balance Sheet Report Response DTO
    /// تقرير الميزانية العمومية وفق المعايير الدولية (IAS 1)
    /// </summary>
    public class IFRSBalanceSheetReportDto
    {
        /// <summary>
        /// As of Date - تاريخ الميزانية
        /// </summary>
        public DateTime AsOfDate { get; set; }

        /// <summary>
        /// Previous Period Date - تاريخ الفترة السابقة
        /// For comparative reporting (typically same date last year)
        /// </summary>
        public DateTime? PreviousPeriodDate { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// View Mode - وضع العرض
        /// </summary>
        public string ViewMode { get; set; } = "hierarchical";

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Company Name - اسم الشركة
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Generated At - تاريخ التوليد
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        #region Assets Section - قسم الأصول

        /// <summary>
        /// Current Assets - الأصول المتداولة
        /// IAS 1.66: Assets expected to be realized within normal operating cycle
        /// </summary>
        public BalanceSheetCategoryDto CurrentAssets { get; set; } = new()
        {
            CategoryNameEn = "Current Assets",
            CategoryNameAr = "الأصول المتداولة"
        };

        /// <summary>
        /// Non-Current Assets - الأصول غير المتداولة
        /// IAS 1.66: Assets not classified as current
        /// </summary>
        public BalanceSheetCategoryDto NonCurrentAssets { get; set; } = new()
        {
            CategoryNameEn = "Non-Current Assets",
            CategoryNameAr = "الأصول غير المتداولة"
        };

        /// <summary>
        /// Total Assets - إجمالي الأصول
        /// </summary>
        public decimal TotalAssets { get; set; }

        #endregion

        #region Liabilities Section - قسم الالتزامات

        /// <summary>
        /// Current Liabilities - الالتزامات المتداولة
        /// IAS 1.69: Liabilities expected to be settled within normal operating cycle
        /// </summary>
        public BalanceSheetCategoryDto CurrentLiabilities { get; set; } = new()
        {
            CategoryNameEn = "Current Liabilities",
            CategoryNameAr = "الالتزامات المتداولة"
        };

        /// <summary>
        /// Non-Current Liabilities - الالتزامات غير المتداولة
        /// IAS 1.69: Liabilities not classified as current
        /// </summary>
        public BalanceSheetCategoryDto NonCurrentLiabilities { get; set; } = new()
        {
            CategoryNameEn = "Non-Current Liabilities",
            CategoryNameAr = "الالتزامات غير المتداولة"
        };

        /// <summary>
        /// Total Liabilities - إجمالي الالتزامات
        /// </summary>
        public decimal TotalLiabilities { get; set; }

        #endregion

        #region Equity Section - قسم حقوق الملكية

        /// <summary>
        /// Equity - حقوق الملكية
        /// Share capital, retained earnings, reserves, etc.
        /// </summary>
        public BalanceSheetCategoryDto Equity { get; set; } = new()
        {
            CategoryNameEn = "Equity",
            CategoryNameAr = "حقوق الملكية"
        };

        /// <summary>
        /// Total Equity - إجمالي حقوق الملكية
        /// </summary>
        public decimal TotalEquity { get; set; }

        #endregion

        #region Validation - التحقق

        /// <summary>
        /// Total Liabilities and Equity - إجمالي الالتزامات وحقوق الملكية
        /// </summary>
        public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;

        /// <summary>
        /// Is Balanced - متوازن
        /// Accounting equation: Assets = Liabilities + Equity
        /// </summary>
        public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;

        /// <summary>
        /// Variance - الفرق
        /// Difference between Assets and (Liabilities + Equity)
        /// </summary>
        public decimal Variance => TotalAssets - TotalLiabilitiesAndEquity;

        #endregion
    }

    /// <summary>
    /// Balance Sheet Category DTO - فئة الميزانية العمومية
    /// Represents a category like Current Assets, Non-Current Liabilities, etc.
    /// </summary>
    public class BalanceSheetCategoryDto
    {
        /// <summary>
        /// Category Name (English) - اسم الفئة بالإنجليزية
        /// </summary>
        public string CategoryNameEn { get; set; }

        /// <summary>
        /// Category Name (Arabic) - اسم الفئة بالعربية
        /// </summary>
        public string CategoryNameAr { get; set; }

        /// <summary>
        /// Line Items - بنود الفئة
        /// </summary>
        public List<BalanceSheetLineDto> Lines { get; set; } = new();

        /// <summary>
        /// Total for this Category - إجمالي الفئة
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Previous Period Total - إجمالي الفترة السابقة
        /// For comparative reporting
        /// </summary>
        public decimal? PreviousPeriodTotal { get; set; }

        /// <summary>
        /// Change from Previous Period - التغير عن الفترة السابقة
        /// </summary>
        public decimal? Change => PreviousPeriodTotal.HasValue ? Total - PreviousPeriodTotal.Value : null;

        /// <summary>
        /// Change Percentage - نسبة التغير
        /// </summary>
        public decimal? ChangePercentage => PreviousPeriodTotal.HasValue && PreviousPeriodTotal.Value != 0
            ? Math.Round(((Total - PreviousPeriodTotal.Value) / Math.Abs(PreviousPeriodTotal.Value)) * 100, 2)
            : null;
    }

    /// <summary>
    /// Balance Sheet Line DTO - بند الميزانية العمومية
    /// Individual account line item
    /// </summary>
    public class BalanceSheetLineDto
    {
        #region Account Information - معلومات الحساب

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Code - رقم الحساب
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// Full Account Number - الرقم الكامل للحساب
        /// Including parent codes (e.g., 1-100-1001)
        /// </summary>
        public string FullAccountNumber { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربية
        /// </summary>
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزية
        /// </summary>
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Account Type - نوع الحساب
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Account Type Enum - نوع الحساب (enum)
        /// </summary>
        public AccountType AccountTypeEnum { get; set; }

        #endregion

        #region Hierarchy Information - معلومات الهيكل الهرمي

        /// <summary>
        /// Account Level - مستوى الحساب
        /// 1 = Root/Main, 2 = Sub-category, etc.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Parent Account ID - معرف الحساب الأب
        /// </summary>
        public Guid? ParentAccountId { get; set; }

        /// <summary>
        /// Is Leaf Account - حساب نهائي (لا يحتوي على حسابات فرعية)
        /// </summary>
        public bool IsLeaf { get; set; }

        /// <summary>
        /// Is Expanded - موسع (for UI state in hierarchical view)
        /// </summary>
        public bool IsExpanded { get; set; } = true;

        /// <summary>
        /// Children Accounts - الحسابات الفرعية
        /// </summary>
        public List<BalanceSheetLineDto> Children { get; set; } = new();

        /// <summary>
        /// Has Children - لديه حسابات فرعية
        /// </summary>
        public bool HasChildren => Children?.Count > 0;

        #endregion

        #region Balance Information - معلومات الرصيد

        /// <summary>
        /// Balance - الرصيد
        /// Net balance as of the report date
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Previous Period Balance - رصيد الفترة السابقة
        /// For comparative reporting
        /// </summary>
        public decimal? PreviousPeriodBalance { get; set; }

        /// <summary>
        /// Change - التغير
        /// Difference from previous period
        /// </summary>
        public decimal Change => Balance - (PreviousPeriodBalance ?? 0);

        /// <summary>
        /// Change Percentage - نسبة التغير
        /// </summary>
        public decimal? ChangePercentage => PreviousPeriodBalance.HasValue && PreviousPeriodBalance.Value != 0
            ? Math.Round(((Balance - PreviousPeriodBalance.Value) / Math.Abs(PreviousPeriodBalance.Value)) * 100, 2)
            : null;

        #endregion

        #region Classification Info - معلومات التصنيف

        /// <summary>
        /// Balance Sheet Category - فئة الميزانية
        /// CurrentAssets, NonCurrentAssets, CurrentLiabilities, NonCurrentLiabilities, Equity
        /// </summary>
        public string BalanceSheetCategory { get; set; }

        /// <summary>
        /// Is Current Item - بند متداول
        /// True for Current Assets or Current Liabilities
        /// </summary>
        public bool IsCurrent { get; set; }

        #endregion
    }

    #endregion

    /// <summary>
    /// Income Statement Report DTO - تقرير قائمة الدخل
    /// </summary>
    public class IncomeStatementReportDto
    {
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public int FiscalYear { get; set; }
        public string Currency { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome => TotalRevenue - TotalExpenses;
        public List<IncomeStatementLineDto> Revenue { get; set; }
        public List<IncomeStatementLineDto> Expenses { get; set; }
    }

    /// <summary>
    /// Income Statement Line DTO - بند قائمة الدخل
    /// </summary>
    public class IncomeStatementLineDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public decimal Amount { get; set; }
    }

    #region IFRS Income Statement DTOs - قائمة الدخل وفق المعايير الدولية

    /// <summary>
    /// IFRS Income Statement Query Parameters
    /// معاملات استعلام قائمة الدخل وفق المعايير الدولية
    /// </summary>
    public class IncomeStatementQueryDto
    {
        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// View Mode: "hierarchical" (tree with roll-ups) or "flat" (no hierarchy)
        /// وضع العرض: "hierarchical" (شجري) أو "flat" (مسطح)
        /// </summary>
        public string ViewMode { get; set; } = "hierarchical";

        /// <summary>
        /// Include Previous Period Comparison
        /// تضمين مقارنة الفترة السابقة
        /// </summary>
        public bool IncludePreviousPeriod { get; set; } = false;

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";
    }

    /// <summary>
    /// IFRS Income Statement Report DTO
    /// تقرير قائمة الدخل وفق المعايير الدولية (IAS 1)
    /// </summary>
    public class IFRSIncomeStatementReportDto
    {
        #region Report Metadata

        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Previous Period Start (for comparison)
        /// </summary>
        public DateTime? PreviousPeriodStart { get; set; }

        /// <summary>
        /// Previous Period End (for comparison)
        /// </summary>
        public DateTime? PreviousPeriodEnd { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// View Mode - وضع العرض
        /// </summary>
        public string ViewMode { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Generated At - تاريخ التوليد
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        #endregion

        #region Revenue Section - قسم الإيرادات

        /// <summary>
        /// Revenue Category - الإيرادات
        /// </summary>
        public IncomeStatementCategoryDto Revenue { get; set; }

        /// <summary>
        /// Total Revenue - إجمالي الإيرادات
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Previous Period Total Revenue
        /// </summary>
        public decimal? PreviousTotalRevenue { get; set; }

        #endregion

        #region Cost of Goods Sold - تكلفة البضاعة المباعة

        /// <summary>
        /// Cost of Goods Sold Category - تكلفة البضاعة المباعة
        /// </summary>
        public IncomeStatementCategoryDto CostOfGoodsSold { get; set; }

        /// <summary>
        /// Total COGS - إجمالي تكلفة البضاعة المباعة
        /// </summary>
        public decimal TotalCOGS { get; set; }

        /// <summary>
        /// Previous Period Total COGS
        /// </summary>
        public decimal? PreviousTotalCOGS { get; set; }

        /// <summary>
        /// Gross Profit - إجمالي الربح
        /// Revenue - COGS
        /// </summary>
        public decimal GrossProfit => TotalRevenue - TotalCOGS;

        /// <summary>
        /// Previous Period Gross Profit
        /// </summary>
        public decimal? PreviousGrossProfit => PreviousTotalRevenue.HasValue && PreviousTotalCOGS.HasValue
            ? PreviousTotalRevenue.Value - PreviousTotalCOGS.Value
            : null;

        #endregion

        #region Operating Expenses - المصروفات التشغيلية

        /// <summary>
        /// Operating Expenses Category - المصروفات التشغيلية
        /// </summary>
        public IncomeStatementCategoryDto OperatingExpenses { get; set; }

        /// <summary>
        /// Total Operating Expenses - إجمالي المصروفات التشغيلية
        /// </summary>
        public decimal TotalOperatingExpenses { get; set; }

        /// <summary>
        /// Previous Period Total Operating Expenses
        /// </summary>
        public decimal? PreviousTotalOperatingExpenses { get; set; }

        /// <summary>
        /// Operating Profit - الربح التشغيلي
        /// Gross Profit - Operating Expenses
        /// </summary>
        public decimal OperatingProfit => GrossProfit - TotalOperatingExpenses;

        /// <summary>
        /// Previous Period Operating Profit
        /// </summary>
        public decimal? PreviousOperatingProfit => PreviousGrossProfit.HasValue && PreviousTotalOperatingExpenses.HasValue
            ? PreviousGrossProfit.Value - PreviousTotalOperatingExpenses.Value
            : null;

        #endregion

        #region Other Income/Expense - الإيرادات/المصروفات الأخرى

        /// <summary>
        /// Other Income Category - الإيرادات الأخرى
        /// </summary>
        public IncomeStatementCategoryDto OtherIncome { get; set; }

        /// <summary>
        /// Total Other Income - إجمالي الإيرادات الأخرى
        /// </summary>
        public decimal TotalOtherIncome { get; set; }

        /// <summary>
        /// Other Expenses Category - المصروفات الأخرى
        /// </summary>
        public IncomeStatementCategoryDto OtherExpenses { get; set; }

        /// <summary>
        /// Total Other Expenses - إجمالي المصروفات الأخرى
        /// </summary>
        public decimal TotalOtherExpenses { get; set; }

        /// <summary>
        /// Net Other Income/Expense - صافي الإيرادات/المصروفات الأخرى
        /// </summary>
        public decimal NetOtherIncomeExpense => TotalOtherIncome - TotalOtherExpenses;

        /// <summary>
        /// Previous Period Net Other Income/Expense
        /// </summary>
        public decimal? PreviousNetOtherIncomeExpense { get; set; }

        #endregion

        #region Net Profit - صافي الربح

        /// <summary>
        /// Net Profit Before Tax - صافي الربح قبل الضريبة
        /// Operating Profit + Net Other Income/Expense
        /// </summary>
        public decimal NetProfitBeforeTax => OperatingProfit + NetOtherIncomeExpense;

        /// <summary>
        /// Previous Period Net Profit Before Tax
        /// </summary>
        public decimal? PreviousNetProfitBeforeTax { get; set; }

        /// <summary>
        /// Total Expenses - إجمالي المصروفات
        /// </summary>
        public decimal TotalExpenses => TotalCOGS + TotalOperatingExpenses + TotalOtherExpenses;

        /// <summary>
        /// Net Profit (same as NetProfitBeforeTax for now, tax calculation can be added)
        /// صافي الربح
        /// </summary>
        public decimal NetProfit => NetProfitBeforeTax;

        /// <summary>
        /// Previous Period Net Profit
        /// </summary>
        public decimal? PreviousNetProfit { get; set; }

        /// <summary>
        /// Profit Margin Percentage
        /// </summary>
        public decimal? ProfitMargin => TotalRevenue != 0 ? Math.Round((NetProfit / TotalRevenue) * 100, 2) : null;

        /// <summary>
        /// Previous Period Profit Margin
        /// </summary>
        public decimal? PreviousProfitMargin => PreviousTotalRevenue.HasValue && PreviousTotalRevenue.Value != 0 && PreviousNetProfit.HasValue
            ? Math.Round((PreviousNetProfit.Value / PreviousTotalRevenue.Value) * 100, 2)
            : null;

        #endregion
    }

    /// <summary>
    /// Income Statement Category DTO
    /// فئة في قائمة الدخل
    /// </summary>
    public class IncomeStatementCategoryDto
    {
        /// <summary>
        /// Category Name (English)
        /// </summary>
        public string CategoryNameEn { get; set; }

        /// <summary>
        /// Category Name (Arabic)
        /// </summary>
        public string CategoryNameAr { get; set; }

        /// <summary>
        /// Line Items - بنود الفئة
        /// </summary>
        public List<IncomeStatementLineItemDto> Lines { get; set; } = new();

        /// <summary>
        /// Category Total - إجمالي الفئة
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Previous Period Total
        /// </summary>
        public decimal? PreviousPeriodTotal { get; set; }
    }

    /// <summary>
    /// Income Statement Line Item DTO (enhanced)
    /// بند قائمة الدخل المحسن
    /// </summary>
    public class IncomeStatementLineItemDto
    {
        #region Account Information

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Code - رمز الحساب
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// Full Account Number - الرقم الكامل للحساب
        /// </summary>
        public string FullAccountNumber { get; set; }

        /// <summary>
        /// Account Name (Arabic)
        /// </summary>
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English)
        /// </summary>
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Account Type - نوع الحساب
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Account Type Enum
        /// </summary>
        public AccountType AccountTypeEnum { get; set; }

        #endregion

        #region Hierarchy Information

        /// <summary>
        /// Account Level - مستوى الحساب
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Parent Account ID - معرف الحساب الأب
        /// </summary>
        public Guid? ParentAccountId { get; set; }

        /// <summary>
        /// Is Leaf Account - حساب نهائي
        /// </summary>
        public bool IsLeaf { get; set; }

        /// <summary>
        /// Children Accounts - الحسابات الفرعية
        /// </summary>
        public List<IncomeStatementLineItemDto> Children { get; set; } = new();

        /// <summary>
        /// Has Children - لديه حسابات فرعية
        /// </summary>
        public bool HasChildren => Children?.Count > 0;

        #endregion

        #region Balance Information

        /// <summary>
        /// Balance - الرصيد (closing balance for the period)
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Previous Period Balance - رصيد الفترة السابقة
        /// </summary>
        public decimal? PreviousPeriodBalance { get; set; }

        /// <summary>
        /// Change from Previous Period
        /// </summary>
        public decimal Change => Balance - (PreviousPeriodBalance ?? 0);

        /// <summary>
        /// Percentage of Category Total
        /// </summary>
        public decimal? PercentageOfTotal { get; set; }

        #endregion

        #region Classification

        /// <summary>
        /// Income Statement Category - فئة قائمة الدخل
        /// Revenue, COGS, OperatingExpenses, OtherIncome, OtherExpenses
        /// </summary>
        public string IncomeStatementCategory { get; set; }

        #endregion
    }

    #endregion

    #region IAS 7 Cash Flow Statement DTOs - قائمة التدفقات النقدية (معيار المحاسبة الدولي 7)

    /// <summary>
    /// IAS 7 Cash Flow Statement Report DTO - قائمة التدفقات النقدية وفق IAS 7
    /// Implements the Indirect Method as per IAS 7
    /// </summary>
    public class CashFlowStatementDto
    {
        /// <summary>
        /// Report Period Start - بداية فترة التقرير
        /// </summary>
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Report Period End - نهاية فترة التقرير
        /// </summary>
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Operating Activities Section - قسم الأنشطة التشغيلية
        /// IAS 7.18: Cash flows from operating activities using indirect method
        /// </summary>
        public CashFlowOperatingActivitiesDto OperatingActivities { get; set; } = new();

        /// <summary>
        /// Investing Activities Section - قسم الأنشطة الاستثمارية
        /// IAS 7.16: Cash flows from investing activities
        /// </summary>
        public CashFlowInvestingActivitiesDto InvestingActivities { get; set; } = new();

        /// <summary>
        /// Financing Activities Section - قسم الأنشطة التمويلية
        /// IAS 7.17: Cash flows from financing activities
        /// </summary>
        public CashFlowFinancingActivitiesDto FinancingActivities { get; set; } = new();

        /// <summary>
        /// Opening Cash and Cash Equivalents - رصيد النقدية في بداية الفترة
        /// </summary>
        public decimal OpeningCashBalance { get; set; }

        /// <summary>
        /// Net Cash from Operating Activities - صافي التدفقات من الأنشطة التشغيلية
        /// </summary>
        public decimal NetCashFromOperating => OperatingActivities?.NetCashFromOperating ?? 0;

        /// <summary>
        /// Net Cash from Investing Activities - صافي التدفقات من الأنشطة الاستثمارية
        /// </summary>
        public decimal NetCashFromInvesting => InvestingActivities?.NetCashFromInvesting ?? 0;

        /// <summary>
        /// Net Cash from Financing Activities - صافي التدفقات من الأنشطة التمويلية
        /// </summary>
        public decimal NetCashFromFinancing => FinancingActivities?.NetCashFromFinancing ?? 0;

        /// <summary>
        /// Net Increase/(Decrease) in Cash - صافي الزيادة/(النقص) في النقدية
        /// </summary>
        public decimal NetChangeInCash => NetCashFromOperating + NetCashFromInvesting + NetCashFromFinancing;

        /// <summary>
        /// Closing Cash and Cash Equivalents - رصيد النقدية في نهاية الفترة
        /// </summary>
        public decimal ClosingCashBalance => OpeningCashBalance + NetChangeInCash;

        /// <summary>
        /// Validation: Closing balance should match actual cash accounts
        /// تحقق: يجب أن يتطابق الرصيد الختامي مع حسابات النقدية الفعلية
        /// </summary>
        public decimal ActualCashBalance { get; set; }

        /// <summary>
        /// Is Reconciled - متسق
        /// </summary>
        public bool IsReconciled => Math.Abs(ClosingCashBalance - ActualCashBalance) < 0.01m;

        /// <summary>
        /// Reconciliation Difference - فرق التسوية
        /// </summary>
        public decimal ReconciliationDifference => ActualCashBalance - ClosingCashBalance;
    }

    /// <summary>
    /// Operating Activities Section (Indirect Method) - الأنشطة التشغيلية (الطريقة غير المباشرة)
    /// IAS 7.18-20: Indirect method starts with profit/loss and adjusts for non-cash items
    /// </summary>
    public class CashFlowOperatingActivitiesDto
    {
        #region Starting Point - نقطة البداية

        /// <summary>
        /// Net Income/Loss from Income Statement - صافي الربح/الخسارة من قائمة الدخل
        /// IAS 7.18(b): Profit or loss for the period
        /// </summary>
        public decimal NetIncome { get; set; }

        #endregion

        #region Non-Cash Adjustments - التعديلات غير النقدية

        /// <summary>
        /// Depreciation Expense - مصروف الإهلاك
        /// IAS 7.20(b): Add back depreciation of property, plant and equipment
        /// </summary>
        public decimal Depreciation { get; set; }

        /// <summary>
        /// Amortization Expense - مصروف الاستهلاك
        /// IAS 7.20(b): Add back amortization of intangible assets
        /// </summary>
        public decimal Amortization { get; set; }

        /// <summary>
        /// Impairment Losses - خسائر انخفاض القيمة
        /// Add back impairment losses on assets
        /// </summary>
        public decimal ImpairmentLosses { get; set; }

        /// <summary>
        /// Gain on Disposal of Assets - أرباح بيع الأصول
        /// IAS 7.14: Subtract gains (non-operating)
        /// Negative value as it's subtracted
        /// </summary>
        public decimal GainOnDisposalOfAssets { get; set; }

        /// <summary>
        /// Loss on Disposal of Assets - خسائر بيع الأصول
        /// IAS 7.14: Add back losses (non-operating)
        /// </summary>
        public decimal LossOnDisposalOfAssets { get; set; }

        /// <summary>
        /// Increase in Provisions - الزيادة في المخصصات
        /// Add back non-cash provision increases
        /// </summary>
        public decimal IncreaseInProvisions { get; set; }

        /// <summary>
        /// Unrealized Foreign Exchange Gains/Losses - أرباح/خسائر صرف غير محققة
        /// </summary>
        public decimal UnrealizedFXGainLoss { get; set; }

        /// <summary>
        /// Interest Expense (if classified as financing) - مصروف الفوائد
        /// IAS 7.33: May be classified as operating or financing
        /// </summary>
        public decimal InterestExpense { get; set; }

        /// <summary>
        /// Interest Income (if classified as investing) - إيراد الفوائد
        /// IAS 7.33: May be classified as operating or investing
        /// </summary>
        public decimal InterestIncome { get; set; }

        /// <summary>
        /// Total Non-Cash Adjustments - إجمالي التعديلات غير النقدية
        /// </summary>
        public decimal TotalNonCashAdjustments =>
            Depreciation + Amortization + ImpairmentLosses +
            LossOnDisposalOfAssets - GainOnDisposalOfAssets +
            IncreaseInProvisions + UnrealizedFXGainLoss;

        #endregion

        #region Working Capital Changes - التغيرات في رأس المال العامل

        /// <summary>
        /// Change in Trade Receivables - التغير في المدينون التجاريون
        /// IAS 7.20(a): Increase is negative (cash outflow), decrease is positive (cash inflow)
        /// </summary>
        public decimal ChangeInTradeReceivables { get; set; }

        /// <summary>
        /// Change in Inventory - التغير في المخزون
        /// IAS 7.20(a): Increase is negative, decrease is positive
        /// </summary>
        public decimal ChangeInInventory { get; set; }

        /// <summary>
        /// Change in Prepaid Expenses - التغير في المصروفات المدفوعة مقدماً
        /// Increase is negative, decrease is positive
        /// </summary>
        public decimal ChangeInPrepaidExpenses { get; set; }

        /// <summary>
        /// Change in Other Current Assets - التغير في الأصول المتداولة الأخرى
        /// </summary>
        public decimal ChangeInOtherCurrentAssets { get; set; }

        /// <summary>
        /// Change in Trade Payables - التغير في الدائنون التجاريون
        /// IAS 7.20(a): Increase is positive (cash inflow), decrease is negative (cash outflow)
        /// </summary>
        public decimal ChangeInTradePayables { get; set; }

        /// <summary>
        /// Change in Accrued Expenses - التغير في المصروفات المستحقة
        /// Increase is positive, decrease is negative
        /// </summary>
        public decimal ChangeInAccruedExpenses { get; set; }

        /// <summary>
        /// Change in Other Current Liabilities - التغير في الالتزامات المتداولة الأخرى
        /// </summary>
        public decimal ChangeInOtherCurrentLiabilities { get; set; }

        /// <summary>
        /// Total Working Capital Changes - إجمالي التغيرات في رأس المال العامل
        /// </summary>
        public decimal TotalWorkingCapitalChanges =>
            ChangeInTradeReceivables + ChangeInInventory +
            ChangeInPrepaidExpenses + ChangeInOtherCurrentAssets +
            ChangeInTradePayables + ChangeInAccruedExpenses +
            ChangeInOtherCurrentLiabilities;

        #endregion

        #region Cash Generated from Operations - النقدية المتولدة من العمليات

        /// <summary>
        /// Cash Generated from Operations (before interest and taxes)
        /// النقدية المتولدة من العمليات (قبل الفوائد والضرائب)
        /// </summary>
        public decimal CashGeneratedFromOperations =>
            NetIncome + TotalNonCashAdjustments + TotalWorkingCapitalChanges;

        /// <summary>
        /// Interest Paid - الفوائد المدفوعة
        /// IAS 7.31: Must be disclosed separately
        /// </summary>
        public decimal InterestPaid { get; set; }

        /// <summary>
        /// Income Taxes Paid - الضرائب المدفوعة
        /// IAS 7.35: Must be disclosed separately and classified as operating unless specifically identified
        /// </summary>
        public decimal IncomeTaxesPaid { get; set; }

        /// <summary>
        /// Net Cash from Operating Activities - صافي التدفقات النقدية من الأنشطة التشغيلية
        /// </summary>
        public decimal NetCashFromOperating =>
            CashGeneratedFromOperations - InterestPaid - IncomeTaxesPaid;

        #endregion

        /// <summary>
        /// Detailed line items for Operating Activities
        /// بنود تفصيلية للأنشطة التشغيلية
        /// </summary>
        public List<CashFlowLineItemDto> LineItems { get; set; } = new();
    }

    /// <summary>
    /// Investing Activities Section - الأنشطة الاستثمارية
    /// IAS 7.16: Cash flows from acquisition/disposal of long-term assets and investments
    /// </summary>
    public class CashFlowInvestingActivitiesDto
    {
        /// <summary>
        /// Purchase of Property, Plant & Equipment - شراء الأصول الثابتة
        /// IAS 7.16(a): Payments to acquire PPE
        /// </summary>
        public decimal PurchaseOfPPE { get; set; }

        /// <summary>
        /// Sale of Property, Plant & Equipment - بيع الأصول الثابتة
        /// IAS 7.16(b): Receipts from sales of PPE
        /// </summary>
        public decimal SaleOfPPE { get; set; }

        /// <summary>
        /// Purchase of Intangible Assets - شراء الأصول غير الملموسة
        /// IAS 7.16(a): Payments for intangibles
        /// </summary>
        public decimal PurchaseOfIntangibles { get; set; }

        /// <summary>
        /// Sale of Intangible Assets - بيع الأصول غير الملموسة
        /// </summary>
        public decimal SaleOfIntangibles { get; set; }

        /// <summary>
        /// Purchase of Investments - شراء الاستثمارات
        /// IAS 7.16(c): Payments to acquire equity/debt instruments of other entities
        /// </summary>
        public decimal PurchaseOfInvestments { get; set; }

        /// <summary>
        /// Sale of Investments - بيع الاستثمارات
        /// IAS 7.16(d): Receipts from sales of investments
        /// </summary>
        public decimal SaleOfInvestments { get; set; }

        /// <summary>
        /// Interest Received - الفوائد المقبوضة
        /// IAS 7.33: May be classified as operating or investing
        /// </summary>
        public decimal InterestReceived { get; set; }

        /// <summary>
        /// Dividends Received - الأرباح الموزعة المقبوضة
        /// IAS 7.33: May be classified as operating or investing
        /// </summary>
        public decimal DividendsReceived { get; set; }

        /// <summary>
        /// Loans Made to Other Parties - قروض مقدمة للغير
        /// IAS 7.16(e): Cash advances and loans made
        /// </summary>
        public decimal LoansMade { get; set; }

        /// <summary>
        /// Repayment of Loans Made - استرداد القروض المقدمة
        /// IAS 7.16(f): Cash receipts from repayment of advances and loans
        /// </summary>
        public decimal LoansRepaid { get; set; }

        /// <summary>
        /// Net Cash from Investing Activities - صافي التدفقات من الأنشطة الاستثمارية
        /// </summary>
        public decimal NetCashFromInvesting =>
            SaleOfPPE + SaleOfIntangibles + SaleOfInvestments +
            InterestReceived + DividendsReceived + LoansRepaid -
            PurchaseOfPPE - PurchaseOfIntangibles - PurchaseOfInvestments - LoansMade;

        /// <summary>
        /// Detailed line items for Investing Activities
        /// بنود تفصيلية للأنشطة الاستثمارية
        /// </summary>
        public List<CashFlowLineItemDto> LineItems { get; set; } = new();
    }

    /// <summary>
    /// Financing Activities Section - الأنشطة التمويلية
    /// IAS 7.17: Cash flows from changes in equity and borrowings
    /// </summary>
    public class CashFlowFinancingActivitiesDto
    {
        /// <summary>
        /// Proceeds from Issuing Shares - متحصلات إصدار الأسهم
        /// IAS 7.17(a): Cash proceeds from issuing shares
        /// </summary>
        public decimal ProceedsFromIssuingShares { get; set; }

        /// <summary>
        /// Repurchase of Shares (Treasury Stock) - إعادة شراء الأسهم
        /// IAS 7.17(b): Payments for repurchase of shares
        /// </summary>
        public decimal RepurchaseOfShares { get; set; }

        /// <summary>
        /// Proceeds from Borrowings - متحصلات الاقتراض
        /// IAS 7.17(c): Cash proceeds from loans, notes, bonds
        /// </summary>
        public decimal ProceedsFromBorrowings { get; set; }

        /// <summary>
        /// Repayment of Borrowings - سداد القروض
        /// IAS 7.17(d): Cash repayments of amounts borrowed
        /// </summary>
        public decimal RepaymentOfBorrowings { get; set; }

        /// <summary>
        /// Interest Paid (if classified as financing) - الفوائد المدفوعة
        /// IAS 7.33: May be classified as operating or financing
        /// </summary>
        public decimal InterestPaid { get; set; }

        /// <summary>
        /// Dividends Paid - الأرباح الموزعة المدفوعة
        /// IAS 7.34: Must be disclosed separately
        /// </summary>
        public decimal DividendsPaid { get; set; }

        /// <summary>
        /// Payment of Lease Liabilities - سداد التزامات الإيجار
        /// IFRS 16: Principal portion of lease payments
        /// </summary>
        public decimal PaymentOfLeaseLiabilities { get; set; }

        /// <summary>
        /// Net Cash from Financing Activities - صافي التدفقات من الأنشطة التمويلية
        /// </summary>
        public decimal NetCashFromFinancing =>
            ProceedsFromIssuingShares + ProceedsFromBorrowings -
            RepurchaseOfShares - RepaymentOfBorrowings -
            InterestPaid - DividendsPaid - PaymentOfLeaseLiabilities;

        /// <summary>
        /// Detailed line items for Financing Activities
        /// بنود تفصيلية للأنشطة التمويلية
        /// </summary>
        public List<CashFlowLineItemDto> LineItems { get; set; } = new();
    }

    /// <summary>
    /// Cash Flow Line Item DTO - بند التدفق النقدي
    /// Used for detailed breakdown of each activity
    /// </summary>
    public class CashFlowLineItemDto
    {
        /// <summary>
        /// Line Item ID - معرف البند
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Account Code - رقم الحساب
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزية
        /// </summary>
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربية
        /// </summary>
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Cash Flow Category - تصنيف التدفق النقدي
        /// </summary>
        public CashFlowCategory Category { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// Positive for inflows, negative for outflows
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Is Inflow - تدفق داخل
        /// </summary>
        public bool IsInflow => Amount > 0;

        /// <summary>
        /// Sort Order - ترتيب العرض
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Cash Flow Category - تصنيف التدفق النقدي
    /// IAS 7 classification of cash flow activities
    /// NOTE: Use ZR.Model.Finance.Enums.CashFlowCategory for new code
    /// This enum kept for backward compatibility with existing report DTOs
    /// </summary>
    [Obsolete("Use ZR.Model.Finance.Enums.CashFlowCategory instead. This duplicate will be removed in a future version.")]
    public enum CashFlowCategory
    {
        /// <summary>
        /// None - غير محدد
        /// </summary>
        None = 0,

        /// <summary>
        /// Operating - تشغيلي
        /// </summary>
        Operating = 1,

        /// <summary>
        /// Investing - استثماري
        /// </summary>
        Investing = 2,

        /// <summary>
        /// Financing - تمويلي
        /// </summary>
        Financing = 3
    }

    /// <summary>
    /// Legacy Cash Flow Report DTO (for backward compatibility)
    /// تقرير التدفق النقدي القديم (للتوافق مع الإصدارات السابقة)
    /// </summary>
    public class CashFlowReportDto
    {
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal NetCashFlow { get; set; }

        /// <summary>
        /// Convert to IAS 7 compliant format (partial - only totals)
        /// </summary>
        public CashFlowStatementDto ToIAS7Format()
        {
            return new CashFlowStatementDto
            {
                PeriodStartDate = PeriodStartDate,
                PeriodEndDate = PeriodEndDate,
                FiscalYear = PeriodStartDate.Year,
                OperatingActivities = new CashFlowOperatingActivitiesDto
                {
                    NetIncome = NetCashFlow // Simplified mapping
                }
            };
        }
    }

    #endregion

    #region Cost Center & Project P&L Reports - تقارير الربح والخسارة حسب مركز التكلفة والمشروع

    /// <summary>
    /// Query DTO for Dimension P&L Reports (Cost Center / Project)
    /// معاملات الاستعلام لتقارير الربح والخسارة حسب البعد
    /// </summary>
    public class DimensionPLQueryDto
    {
        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        [Required(ErrorMessage = "تاريخ بداية الفترة مطلوب")]
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        [Required(ErrorMessage = "تاريخ نهاية الفترة مطلوب")]
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة (for multi-tenant isolation)
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Include Inactive - تضمين غير النشط
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";
    }

    /// <summary>
    /// Cost Center P&L Query DTO
    /// معاملات الاستعلام لتقرير الربح والخسارة حسب مركز التكلفة
    /// </summary>
    public class CostCenterPLQueryDto : DimensionPLQueryDto
    {
        /// <summary>
        /// Specific Cost Center ID (null = all cost centers)
        /// معرف مركز التكلفة المحدد (فارغ = كل مراكز التكلفة)
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Include Unassigned Entries - تضمين القيود غير المخصصة
        /// Entries with null CostCenterId
        /// </summary>
        public bool IncludeUnassigned { get; set; } = true;
    }

    /// <summary>
    /// Project P&L Query DTO
    /// معاملات الاستعلام لتقرير الربح والخسارة حسب المشروع
    /// </summary>
    public class ProjectPLQueryDto : DimensionPLQueryDto
    {
        /// <summary>
        /// Specific Project ID (null = all projects)
        /// معرف المشروع المحدد (فارغ = كل المشاريع)
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Include Unassigned Entries - تضمين القيود غير المخصصة
        /// Entries with null ProjectId
        /// </summary>
        public bool IncludeUnassigned { get; set; } = true;

        /// <summary>
        /// Project Status Filter - فلتر حالة المشروع
        /// Active, Completed, Cancelled, or null for all
        /// </summary>
        public string ProjectStatus { get; set; }
    }

    /// <summary>
    /// Cost Center P&L Report DTO - تقرير الربح والخسارة حسب مركز التكلفة
    /// </summary>
    public class CostCenterPLReportDto
    {
        /// <summary>
        /// Report Period Start - بداية فترة التقرير
        /// </summary>
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Report Period End - نهاية فترة التقرير
        /// </summary>
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Generated At - تاريخ التوليد
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Cost Center P&L Items - بنود الربح والخسارة حسب مركز التكلفة
        /// </summary>
        public List<CostCenterPLItemDto> Items { get; set; } = new();

        /// <summary>
        /// Total Revenue (All Cost Centers) - إجمالي الإيرادات
        /// </summary>
        public decimal TotalRevenue => Items?.Sum(x => x.TotalRevenue) ?? 0;

        /// <summary>
        /// Total Expenses (All Cost Centers) - إجمالي المصروفات
        /// </summary>
        public decimal TotalExpenses => Items?.Sum(x => x.TotalExpenses) ?? 0;

        /// <summary>
        /// Net Profit/Loss (All Cost Centers) - صافي الربح/الخسارة
        /// </summary>
        public decimal NetProfitLoss => TotalRevenue - TotalExpenses;

        /// <summary>
        /// Is Profitable - مربح
        /// </summary>
        public bool IsProfitable => NetProfitLoss > 0;
    }

    /// <summary>
    /// Cost Center P&L Item DTO - بند الربح والخسارة لمركز التكلفة
    /// </summary>
    public class CostCenterPLItemDto
    {
        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Cost Center Code - كود مركز التكلفة
        /// </summary>
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name (English) - اسم مركز التكلفة بالإنجليزية
        /// </summary>
        public string CostCenterNameEn { get; set; }

        /// <summary>
        /// Cost Center Name (Arabic) - اسم مركز التكلفة بالعربية
        /// </summary>
        public string CostCenterNameAr { get; set; }

        /// <summary>
        /// Is Unassigned (entries without cost center)
        /// قيود بدون مركز تكلفة
        /// </summary>
        public bool IsUnassigned => !CostCenterId.HasValue;

        /// <summary>
        /// Revenue Details - تفاصيل الإيرادات
        /// </summary>
        public List<PLAccountLineDto> RevenueAccounts { get; set; } = new();

        /// <summary>
        /// Expense Details - تفاصيل المصروفات
        /// </summary>
        public List<PLAccountLineDto> ExpenseAccounts { get; set; } = new();

        /// <summary>
        /// Total Revenue - إجمالي الإيرادات
        /// </summary>
        public decimal TotalRevenue => RevenueAccounts?.Sum(x => x.Amount) ?? 0;

        /// <summary>
        /// Total Expenses - إجمالي المصروفات
        /// </summary>
        public decimal TotalExpenses => ExpenseAccounts?.Sum(x => x.Amount) ?? 0;

        /// <summary>
        /// Net Profit/Loss - صافي الربح/الخسارة
        /// </summary>
        public decimal NetProfitLoss => TotalRevenue - TotalExpenses;

        /// <summary>
        /// Profit Margin Percentage - نسبة هامش الربح
        /// </summary>
        public decimal ProfitMarginPercent => TotalRevenue != 0 ? Math.Round((NetProfitLoss / TotalRevenue) * 100, 2) : 0;

        /// <summary>
        /// Is Profitable - مربح
        /// </summary>
        public bool IsProfitable => NetProfitLoss > 0;
    }

    /// <summary>
    /// Project P&L Report DTO - تقرير الربح والخسارة حسب المشروع
    /// </summary>
    public class ProjectPLReportDto
    {
        /// <summary>
        /// Report Period Start - بداية فترة التقرير
        /// </summary>
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Report Period End - نهاية فترة التقرير
        /// </summary>
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// Generated At - تاريخ التوليد
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Project P&L Items - بنود الربح والخسارة حسب المشروع
        /// </summary>
        public List<ProjectPLItemDto> Items { get; set; } = new();

        /// <summary>
        /// Total Revenue (All Projects) - إجمالي الإيرادات
        /// </summary>
        public decimal TotalRevenue => Items?.Sum(x => x.TotalRevenue) ?? 0;

        /// <summary>
        /// Total Expenses (All Projects) - إجمالي المصروفات
        /// </summary>
        public decimal TotalExpenses => Items?.Sum(x => x.TotalExpenses) ?? 0;

        /// <summary>
        /// Net Profit/Loss (All Projects) - صافي الربح/الخسارة
        /// </summary>
        public decimal NetProfitLoss => TotalRevenue - TotalExpenses;

        /// <summary>
        /// Is Profitable - مربح
        /// </summary>
        public bool IsProfitable => NetProfitLoss > 0;

        /// <summary>
        /// Profitable Projects Count - عدد المشاريع المربحة
        /// </summary>
        public int ProfitableProjectsCount => Items?.Count(x => x.IsProfitable) ?? 0;

        /// <summary>
        /// Loss-Making Projects Count - عدد المشاريع الخاسرة
        /// </summary>
        public int LossProjectsCount => Items?.Count(x => !x.IsProfitable && x.NetProfitLoss != 0) ?? 0;
    }

    /// <summary>
    /// Project P&L Item DTO - بند الربح والخسارة للمشروع
    /// </summary>
    public class ProjectPLItemDto
    {
        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Project Code - كود المشروع
        /// </summary>
        public string ProjectCode { get; set; }

        /// <summary>
        /// Project Name (English) - اسم المشروع بالإنجليزية
        /// </summary>
        public string ProjectNameEn { get; set; }

        /// <summary>
        /// Project Name (Arabic) - اسم المشروع بالعربية
        /// </summary>
        public string ProjectNameAr { get; set; }

        /// <summary>
        /// Project Status - حالة المشروع
        /// </summary>
        public string ProjectStatus { get; set; }

        /// <summary>
        /// Project Start Date - تاريخ بداية المشروع
        /// </summary>
        public DateTime? ProjectStartDate { get; set; }

        /// <summary>
        /// Project End Date - تاريخ نهاية المشروع
        /// </summary>
        public DateTime? ProjectEndDate { get; set; }

        /// <summary>
        /// Is Unassigned (entries without project)
        /// قيود بدون مشروع
        /// </summary>
        public bool IsUnassigned => !ProjectId.HasValue;

        /// <summary>
        /// Revenue Details - تفاصيل الإيرادات
        /// </summary>
        public List<PLAccountLineDto> RevenueAccounts { get; set; } = new();

        /// <summary>
        /// Expense Details - تفاصيل المصروفات
        /// </summary>
        public List<PLAccountLineDto> ExpenseAccounts { get; set; } = new();

        /// <summary>
        /// Total Revenue - إجمالي الإيرادات
        /// </summary>
        public decimal TotalRevenue => RevenueAccounts?.Sum(x => x.Amount) ?? 0;

        /// <summary>
        /// Total Expenses - إجمالي المصروفات
        /// </summary>
        public decimal TotalExpenses => ExpenseAccounts?.Sum(x => x.Amount) ?? 0;

        /// <summary>
        /// Net Profit/Loss - صافي الربح/الخسارة
        /// </summary>
        public decimal NetProfitLoss => TotalRevenue - TotalExpenses;

        /// <summary>
        /// Profit Margin Percentage - نسبة هامش الربح
        /// </summary>
        public decimal ProfitMarginPercent => TotalRevenue != 0 ? Math.Round((NetProfitLoss / TotalRevenue) * 100, 2) : 0;

        /// <summary>
        /// Is Profitable - مربح
        /// </summary>
        public bool IsProfitable => NetProfitLoss > 0;

        /// <summary>
        /// Is Capital Project - مشروع رأسمالي
        /// </summary>
        public bool IsCapitalProject { get; set; }
    }

    /// <summary>
    /// P&L Account Line DTO - بند حساب الربح والخسارة
    /// </summary>
    public class PLAccountLineDto
    {
        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Code - رقم الحساب
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزية
        /// </summary>
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربية
        /// </summary>
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Type (Revenue/Expense) - نوع الحساب
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Total Debit - إجمالي المدين
        /// </summary>
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Total Credit - إجمالي الدائن
        /// </summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// For Revenue: Credit - Debit (positive = revenue)
        /// For Expense: Debit - Credit (positive = expense)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Transaction Count - عدد المعاملات
        /// </summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Dimension P&L Summary DTO - ملخص الربح والخسارة حسب البعد
    /// For dashboard widgets
    /// </summary>
    public class DimensionPLSummaryDto
    {
        /// <summary>
        /// Dimension Type - نوع البعد (CostCenter/Project)
        /// </summary>
        public string DimensionType { get; set; }

        /// <summary>
        /// Dimension ID - معرف البعد
        /// </summary>
        public Guid? DimensionId { get; set; }

        /// <summary>
        /// Dimension Code - كود البعد
        /// </summary>
        public string DimensionCode { get; set; }

        /// <summary>
        /// Dimension Name - اسم البعد
        /// </summary>
        public string DimensionName { get; set; }

        /// <summary>
        /// Total Revenue - إجمالي الإيرادات
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Total Expenses - إجمالي المصروفات
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// Net Profit/Loss - صافي الربح/الخسارة
        /// </summary>
        public decimal NetProfitLoss { get; set; }

        /// <summary>
        /// Profit Margin Percentage - نسبة هامش الربح
        /// </summary>
        public decimal ProfitMarginPercent { get; set; }

        /// <summary>
        /// Is Profitable - مربح
        /// </summary>
        public bool IsProfitable { get; set; }
    }

    #endregion
}

