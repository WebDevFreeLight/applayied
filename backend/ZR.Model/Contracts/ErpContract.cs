using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Master - العقد الرئيسي
    /// Core contract entity containing only identity and reference data.
    /// Financial, lifecycle, and billing configuration data are stored in extension tables.
    /// </summary>
    [SugarTable("erp_contract")]
    [SugarIndex("IX_contract_number", nameof(ContractNumber), OrderByType.Asc, true)]
    [SugarIndex("IX_contract_customer", nameof(CustomerId), OrderByType.Asc)]
    [SugarIndex("IX_contract_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("IX_contract_dates", nameof(StartDate), OrderByType.Asc, nameof(EndDate), OrderByType.Asc)]
    [SugarIndex("IX_contract_parent", nameof(ParentContractId), OrderByType.Asc)]
    public class ErpContract : SysBase, ISoftDelete
    {
        #region Primary Key

        /// <summary>
        /// Contract ID - معرف العقد
        /// Primary key (GUID)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        #endregion

        #region Contract Identity - هوية العقد

        /// <summary>
        /// Contract Number - رقم العقد
        /// Auto-generated unique contract number (e.g., CNT-2024-00001)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "contract_number")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// Foreign key to Customer table
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "customer_id")]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Contract Type - نوع العقد
        /// Category of the contract (Rental, Service, Subscription, etc.)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_type")]
        public ContractType ContractType { get; set; }

        /// <summary>
        /// Contract Title - عنوان العقد
        /// Descriptive title for the contract
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "contract_title")]
        public string ContractTitle { get; set; }

        /// <summary>
        /// Contract Title (Arabic) - عنوان العقد بالعربية
        /// Arabic title for bilingual support
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "contract_title_ar")]
        public string ContractTitleAr { get; set; }

        #endregion

        #region Contract Duration - مدة العقد

        /// <summary>
        /// Start Date - تاريخ البداية
        /// Contract effective start date
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// Contract end date (null for indefinite contracts)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "end_date")]
        public DateTime? EndDate { get; set; }

        #endregion

        #region Renewal Settings - إعدادات التجديد

        /// <summary>
        /// Auto Renew - التجديد التلقائي
        /// Whether the contract automatically renews
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "auto_renew")]
        public bool AutoRenew { get; set; } = false;

        /// <summary>
        /// Renewal Type - نوع التجديد
        /// How the contract renews (Manual, Monthly, Yearly, etc.)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "renewal_type")]
        public RenewalType RenewalType { get; set; } = RenewalType.Manual;

        /// <summary>
        /// Renewal Notice Days - أيام إشعار التجديد
        /// Days before end date to send renewal notice
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "renewal_notice_days")]
        public int? RenewalNoticeDays { get; set; } = 30;

        /// <summary>
        /// Renewal Count - عدد التجديدات
        /// Number of times this contract has been renewed
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "renewal_count")]
        public int RenewalCount { get; set; } = 0;

        /// <summary>
        /// Parent Contract ID - معرف العقد الأصلي
        /// Reference to original contract if this is a renewal
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "parent_contract_id")]
        public Guid? ParentContractId { get; set; }

        #endregion

        #region Status - الحالة

        /// <summary>
        /// Status - الحالة
        /// Current contract status (Draft, Active, Terminated, etc.)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        #endregion

        #region References - المراجع

        /// <summary>
        /// External Reference - المرجع الخارجي
        /// External reference number (customer PO, etc.)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "external_reference")]
        public string ExternalReference { get; set; }

        #endregion

        #region Description & Notes - الوصف والملاحظات

        /// <summary>
        /// Description - الوصف
        /// Detailed contract description
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Terms and Conditions - الشروط والأحكام
        /// Contract terms and conditions text
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true, ColumnName = "terms_conditions")]
        public string TermsConditions { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// Internal notes about the contract
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #endregion

        #region Soft Delete

        [SugarColumn(IsNullable = false, ColumnName = "is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [SugarColumn(IsNullable = true, ColumnName = "deleted_at")]
        public DateTime? DeletedAt { get; set; }

        #endregion

        #region Navigation Properties - Extension Tables (1:1)

        /// <summary>
        /// Financial Settings - الإعدادات المالية
        /// Financial configuration for this contract
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContractFinancial Financial { get; set; }

        /// <summary>
        /// Lifecycle Events - أحداث دورة الحياة
        /// Lifecycle state changes for this contract
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContractLifecycle Lifecycle { get; set; }

        /// <summary>
        /// Billing Configuration - إعدادات الفوترة
        /// Billing settings for this contract
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContractBillingConfig BillingConfig { get; set; }

        #endregion

        #region Navigation Properties - Child Collections (1:M)

        /// <summary>
        /// Contract Units - وحدات العقد
        /// List of units included in this contract
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractUnit.ContractId))]
        public List<ErpContractUnit> Units { get; set; }

        /// <summary>
        /// Pricing Rules - قواعد التسعير
        /// List of pricing rules for this contract
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractPricingRule.ContractId))]
        public List<ErpContractPricingRule> PricingRules { get; set; }

        /// <summary>
        /// Billing Schedules - جداول الفوترة
        /// List of billing schedule entries for this contract
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractBillingSchedule.ContractId))]
        public List<ErpContractBillingSchedule> BillingSchedules { get; set; }

        /// <summary>
        /// Amendments - التعديلات
        /// List of amendments made to this contract
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractAmendment.ContractId))]
        public List<ErpContractAmendment> Amendments { get; set; }

        /// <summary>
        /// Renewals - التجديدات
        /// List of renewals for this contract
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractRenewal.ContractId))]
        public List<ErpContractRenewal> Renewals { get; set; }

        /// <summary>
        /// Terminations - الإنهاءات
        /// List of termination records for this contract
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractTermination.ContractId))]
        public List<ErpContractTermination> Terminations { get; set; }

        #endregion

        #region Navigation Properties - Display Fields

        /// <summary>
        /// Customer Name - اسم العميل
        /// Loaded from Customer table (for display)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string CustomerName { get; set; }

        /// <summary>
        /// Customer Name Arabic - اسم العميل بالعربية
        /// Loaded from Customer table (for display)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string CustomerNameAr { get; set; }

        /// <summary>
        /// Contract Type Name - اسم نوع العقد
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string ContractTypeName { get; set; }

        /// <summary>
        /// Status Name - اسم الحالة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string StatusName { get; set; }

        #endregion

        #region Computed Properties - Convenience Accessors

        /// <summary>
        /// Is Active - نشط
        /// Whether the contract is currently active
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsActive => Status == ContractStatus.Active;

        /// <summary>
        /// Duration Days - مدة العقد بالأيام
        /// Contract duration in days (null if no end date)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int? DurationDays => EndDate.HasValue ? (int?)(EndDate.Value - StartDate).TotalDays : null;

        #endregion
    }
}
