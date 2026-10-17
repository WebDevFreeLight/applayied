using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// ERP Settings Query DTO - استعلام إعدادات النظام
    /// </summary>
    public class ERPSettingsQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create ERP Settings DTO - إنشاء إعدادات نظام جديدة
    /// </summary>
    public class CreateERPSettingsDto
    {
        public Guid? DefaultCurrencyId { get; set; }

        public bool EnableMultiCurrency { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "حد الموافقة يجب أن يكون موجباً")]
        public decimal DefaultApprovalLimit { get; set; } = 0;

        public bool AutoPostToGL { get; set; } = false;

        public bool RequireApprovalForVouchers { get; set; } = false;

        public bool AllowBackdating { get; set; } = true;

        [Range(0, 365, ErrorMessage = "الحد الأقصى لأيام التاريخ الرجعي يجب أن يكون بين 0 و 365")]
        public int MaxBackdatingDays { get; set; } = 30;

        public bool EnableBudgetControl { get; set; } = false;

        [Range(1, 12, ErrorMessage = "شهر بداية السنة المالية يجب أن يكون بين 1 و 12")]
        public int FiscalYearStartMonth { get; set; } = 1;

        [Range(1, 52, ErrorMessage = "عدد الفترات المالية يجب أن يكون بين 1 و 52")]
        public int NumberOfFiscalPeriods { get; set; } = 12;

        public Guid? DefaultTaxRuleId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update ERP Settings DTO - تحديث إعدادات النظام
    /// </summary>
    public class UpdateERPSettingsDto
    {
        [Required(ErrorMessage = "معرف الإعدادات مطلوب")]
        public Guid Id { get; set; }

        public Guid? DefaultCurrencyId { get; set; }

        public bool EnableMultiCurrency { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "حد الموافقة يجب أن يكون موجباً")]
        public decimal DefaultApprovalLimit { get; set; } = 0;

        public bool AutoPostToGL { get; set; } = false;

        public bool RequireApprovalForVouchers { get; set; } = false;

        public bool AllowBackdating { get; set; } = true;

        [Range(0, 365, ErrorMessage = "الحد الأقصى لأيام التاريخ الرجعي يجب أن يكون بين 0 و 365")]
        public int MaxBackdatingDays { get; set; } = 30;

        public bool EnableBudgetControl { get; set; } = false;

        [Range(1, 12, ErrorMessage = "شهر بداية السنة المالية يجب أن يكون بين 1 و 12")]
        public int FiscalYearStartMonth { get; set; } = 1;

        [Range(1, 52, ErrorMessage = "عدد الفترات المالية يجب أن يكون بين 1 و 52")]
        public int NumberOfFiscalPeriods { get; set; } = 12;

        public Guid? DefaultTaxRuleId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// ERP Settings List DTO - قائمة إعدادات النظام
    /// </summary>
    public class ERPSettingsListDto
    {
        [ExcelColumn(Name = "Settings ID")]
        public Guid Id { get; set; }
        public long? CompanyId { get; set; }

        [ExcelColumn(Name = "Default Currency ID")]
        public Guid? DefaultCurrencyId { get; set; }

        [ExcelColumn(Name = "Default Currency Code")]
        public string DefaultCurrencyCode { get; set; }

        [ExcelColumn(Name = "Enable Multi-Currency")]
        public bool EnableMultiCurrency { get; set; }

        [ExcelColumn(Name = "Default Approval Limit")]
        public decimal DefaultApprovalLimit { get; set; }

        [ExcelColumn(Name = "Auto Post to GL")]
        public bool AutoPostToGL { get; set; }

        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ERP Settings Detail DTO - تفاصيل إعدادات النظام
    /// </summary>
    public class ERPSettingsDetailDto : ERPSettingsListDto
    {
        public bool RequireApprovalForVouchers { get; set; }
        public bool AllowBackdating { get; set; }
        public int MaxBackdatingDays { get; set; }
        public bool EnableBudgetControl { get; set; }
        public int FiscalYearStartMonth { get; set; }
        public int NumberOfFiscalPeriods { get; set; }
        public Guid? DefaultTaxRuleId { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}


