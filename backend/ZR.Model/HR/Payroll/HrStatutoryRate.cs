using ZR.Model.System;
using Newtonsoft.Json;
using SqlSugar;
using Infrastructure.Converter;
using System;

namespace ZR.Model.HR.Payroll
{
    /// <summary>
    /// Statutory Rates (GOSI, etc.) - المعدلات النظامية
    /// Configurable rates for social insurance contributions
    /// </summary>
    [SugarTable("hr_statutory_rate")]
    public class HrStatutoryRate : SysBase
    {
        /// <summary>
        /// Rate ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long RateId { get; set; }

        /// <summary>
        /// Rate type: GOSI_SAUDI, GOSI_NONSAUDI, etc.
        /// </summary>
        [SugarColumn(Length = 50)]
        public string RateType { get; set; }

        /// <summary>
        /// Employee contribution rate (decimal, e.g. 0.10 = 10%)
        /// </summary>
        public decimal EmployeeRate { get; set; }

        /// <summary>
        /// Employer contribution rate (decimal, e.g. 0.12 = 12%)
        /// </summary>
        public decimal EmployerRate { get; set; }

        /// <summary>
        /// Effective from date
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective to date (null = currently active)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Country code
        /// </summary>
        [SugarColumn(Length = 10)]
        public string CountryCode { get; set; } = "SA";

        /// <summary>
        /// Description / notes
        /// </summary>
        [SugarColumn(Length = 500)]
        public string Description { get; set; }

        /// <summary>
        /// Is this rate currently active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
