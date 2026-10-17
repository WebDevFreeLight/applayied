using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Payroll
{
    /// <summary>
    /// Effective-dated salary history (source of truth for payroll calculations)
    /// </summary>
    [SugarTable("hr_salary_history")]
    public class HrSalaryHistory : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SalaryHistoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Effective from date (inclusive)
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective to date (inclusive). NULL = current.
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal OtherAllowances { get; set; }

        /// <summary>
        /// Finance Currency ID (FK -> currencies.id)
        /// </summary>
        public Guid? CurrencyId { get; set; }

        /// <summary>
        /// Exchange rate vs base currency at time of salary assignment
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 10)]
        public decimal ExchangeRate { get; set; } = 1.0m;
    }
}














