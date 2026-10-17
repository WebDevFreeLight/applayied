using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Unit Guarantee - ضمان وحدة العقد
    /// Stores guarantees and warranties for contract units
    /// </summary>
    [SugarTable("erp_contract_unit_guarantee")]
    [SugarIndex("ix_unit_guarantee_unit", nameof(UnitId), OrderByType.Asc)]
    [SugarIndex("ix_unit_guarantee_dates", nameof(StartDate), OrderByType.Asc, nameof(EndDate), OrderByType.Asc)]
    [SugarIndex("ix_unit_guarantee_status", nameof(Status), OrderByType.Asc)]
    public class ErpContractUnitGuarantee : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Guarantee ID - معرف الضمان
        /// Primary key (GUID)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "guarantee_id")]
        public Guid GuaranteeId { get; set; }

        #endregion

        #region Foreign Key

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// Foreign key to ErpContractUnit
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "unit_id")]
        public Guid UnitId { get; set; }

        #endregion

        #region Guarantee Details - تفاصيل الضمان

        /// <summary>
        /// Guarantee Type - نوع الضمان
        /// Type of guarantee (Warranty, Performance, BankGuarantee, Insurance, Other)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "guarantee_type")]
        public string GuaranteeType { get; set; }

        /// <summary>
        /// Guarantee Number - رقم الضمان
        /// Reference number for the guarantee
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "guarantee_number")]
        public string GuaranteeNumber { get; set; }

        /// <summary>
        /// Provider Name - اسم مقدم الضمان
        /// Name of the guarantor or provider
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "provider_name")]
        public string ProviderName { get; set; }

        /// <summary>
        /// Provider Contact - جهة اتصال مقدم الضمان
        /// Contact information for the provider
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "provider_contact")]
        public string ProviderContact { get; set; }

        #endregion

        #region Financial - المالية

        /// <summary>
        /// Amount - مبلغ الضمان
        /// Value of the guarantee
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "amount")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// Currency of the guarantee amount
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        #endregion

        #region Dates - التواريخ

        /// <summary>
        /// Start Date - تاريخ البداية
        /// Start date of the guarantee coverage
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ الانتهاء
        /// Expiry date of the guarantee (null for indefinite)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "end_date")]
        public DateTime? EndDate { get; set; }

        #endregion

        #region Details - التفاصيل

        /// <summary>
        /// Coverage Details - تفاصيل التغطية
        /// Description of what is covered by the guarantee
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "coverage_details")]
        public string CoverageDetails { get; set; }

        /// <summary>
        /// Terms - الشروط
        /// Terms and conditions of the guarantee
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "terms")]
        public string Terms { get; set; }

        /// <summary>
        /// Exclusions - الاستثناءات
        /// What is not covered by the guarantee
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "exclusions")]
        public string Exclusions { get; set; }

        #endregion

        #region Status - الحالة

        /// <summary>
        /// Status - حالة الضمان
        /// Current status (Active, Expired, Claimed, Cancelled, Pending)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "status")]
        public string Status { get; set; } = "Active";

        #endregion

        #region Document Reference - مرجع المستند

        /// <summary>
        /// Document ID - معرف المستند
        /// Reference to attached document
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "document_id")]
        public Guid? DocumentId { get; set; }

        /// <summary>
        /// Document Number - رقم المستند
        /// Document reference number
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "document_number")]
        public string DocumentNumber { get; set; }

        #endregion

        #region Notes - الملاحظات

        /// <summary>
        /// Notes - ملاحظات
        /// Additional notes about the guarantee
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Unit - الوحدة
        /// Parent unit (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(UnitId))]
        public ErpContractUnit Unit { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string UnitCode { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string ContractNumber { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// Is Active - نشط
        /// Whether the guarantee is currently active
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsActive => Status == "Active" && (!EndDate.HasValue || EndDate.Value >= DateTime.UtcNow);

        /// <summary>
        /// Is Expired - منتهي
        /// Whether the guarantee has expired
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsExpired => Status == "Expired" || (EndDate.HasValue && EndDate.Value < DateTime.UtcNow);

        /// <summary>
        /// Days Until Expiry - الأيام حتى انتهاء الصلاحية
        /// Number of days until the guarantee expires (null if no end date)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int? DaysUntilExpiry => EndDate.HasValue ? (int?)(EndDate.Value - DateTime.UtcNow).TotalDays : null;

        #endregion
    }
}
