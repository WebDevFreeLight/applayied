using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury GL Account Mapping - تعيين حسابات دفتر الأستاذ للخزينة
    /// Persists GL account mappings for treasury transactions (requirement: Phase 2.2)
    /// </summary>
    [SugarTable("treasury_gl_account_mapping")]
    public class TreasuryGLAccountMapping : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Mapping ID - معرف التعيين
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        // Note: CompanyID and BranchID are inherited from SysBase - do not duplicate

        /// <summary>
        /// Mapping Type - نوع التعيين
        /// E.g., "BankAccount", "CashAccount", "ChequesIssued", "ChequesReceived", etc.
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "mapping_type")]
        public string MappingType { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// </summary>
        [SugarColumn(ColumnName = "gl_account_id")]
        public Guid GLAccountId { get; set; }

        /// <summary>
        /// GL Account Code - رمز حساب دفتر الأستاذ
        /// Denormalized for display purposes
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "gl_account_code")]
        public string GLAccountCode { get; set; }

        /// <summary>
        /// GL Account Name - اسم حساب دفتر الأستاذ
        /// Denormalized for display purposes
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "gl_account_name")]
        public string GLAccountName { get; set; }

        /// <summary>
        /// Currency Code - رمز العملة (null = default/all currencies)
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Description - وصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Priority - الأولوية (lower = higher priority for matching)
        /// </summary>
        [SugarColumn(ColumnName = "priority")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Effective From Date - تاريخ بدء السريان
        /// </summary>
        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective To Date - تاريخ انتهاء السريان
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "effective_to")]
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// GL Account Mapping Types - أنواع تعيين حسابات دفتر الأستاذ
    /// </summary>
    public static class GLMappingType
    {
        // Bank Account Mappings
        public const string DefaultBankAccount = "DefaultBankAccount";
        public const string BankCharges = "BankCharges";
        public const string BankInterestIncome = "BankInterestIncome";

        // Cash Account Mappings
        public const string DefaultCashAccount = "DefaultCashAccount";
        public const string PettyCash = "PettyCash";

        // Payment Mappings
        public const string AccountsPayable = "AccountsPayable";
        public const string AccountsReceivable = "AccountsReceivable";

        // Cheque Mappings
        public const string ChequesIssued = "ChequesIssued";
        public const string ChequesReceived = "ChequesReceived";
        public const string BouncedCheques = "BouncedCheques";

        // Reconciliation Mappings
        public const string ReconciliationDifference = "ReconciliationDifference";
        public const string SuspenseAccount = "SuspenseAccount";

        // FX Mappings
        public const string FxGainLoss = "FxGainLoss";
        public const string UnrealizedFxGainLoss = "UnrealizedFxGainLoss";
    }
}
