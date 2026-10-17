using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// Tax Configuration - إعدادات الضرائب
    /// Stores the VAT Input and Output GL Account mappings for the company
    /// </summary>
    [SugarTable("tax_configuration")]
    public class TaxConfiguration : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in this table
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Config ID - معرف الإعداد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// VAT Input Account ID - معرف حساب ضريبة المدخلات
        /// GL Account to debit when posting AP invoices with VAT
        /// </summary>
        [SugarColumn(ColumnName = "vat_input_account_id", IsNullable = true)]
        public Guid? VATInputAccountId { get; set; }

        /// <summary>
        /// VAT Output Account ID - معرف حساب ضريبة المخرجات
        /// GL Account to credit when posting AR invoices with VAT
        /// </summary>
        [SugarColumn(ColumnName = "vat_output_account_id", IsNullable = true)]
        public Guid? VATOutputAccountId { get; set; }

        /// <summary>
        /// Default AR Control Account ID - معرف حساب الذمم المدينة الافتراضي
        /// Default GL Account for Accounts Receivable
        /// </summary>
        [SugarColumn(ColumnName = "default_ar_account_id", IsNullable = true)]
        public Guid? DefaultARAccountId { get; set; }

        /// <summary>
        /// Default AP Control Account ID - معرف حساب الذمم الدائنة الافتراضي
        /// Default GL Account for Accounts Payable
        /// </summary>
        [SugarColumn(ColumnName = "default_ap_account_id", IsNullable = true)]
        public Guid? DefaultAPAccountId { get; set; }

        /// <summary>
        /// Is Configured - هل تم التكوين
        /// True if the tax settings have been configured by the user
        /// </summary>
        [SugarColumn(ColumnName = "is_configured")]
        public bool IsConfigured { get; set; } = false;
    }
}
