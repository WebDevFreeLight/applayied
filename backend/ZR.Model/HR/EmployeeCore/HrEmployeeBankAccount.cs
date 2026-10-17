using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee bank accounts (effective-dated; encrypted) - hr_employee_bank_account
    /// </summary>
    [SugarTable("hr_employee_bank_account")]
    public class HrEmployeeBankAccount : SysBase
    {
        [SugarColumn(ColumnName = "employee_bank_account_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeBankAccountId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "bank_name", Length = 100, IsNullable = true)]
        public string BankName { get; set; }

        [SugarColumn(ColumnName = "iban_encrypted", IsNullable = true)]
        public string IbanEncrypted { get; set; }

        [SugarColumn(ColumnName = "account_number_encrypted", IsNullable = true)]
        public string AccountNumberEncrypted { get; set; }

        [SugarColumn(ColumnName = "bank_account_encrypted", IsNullable = true)]
        public string BankAccountEncrypted { get; set; }

        [SugarColumn(ColumnName = "currency_id", IsNullable = true)]
        public Guid? CurrencyId { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = false;
    }
}














