using Infrastructure.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// Compensation package with components (display DTO)
    /// </summary>
    public class CompensationPackageDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompPackageId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public Guid? CurrencyId { get; set; }
        public string CurrencyCode { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public string Status { get; set; }
        public bool IsCurrent { get; set; }

        public decimal TotalMonthly { get; set; }
        public decimal TotalAnnual { get; set; }

        public List<CompensationComponentDto> Components { get; set; } = new();
    }

    /// <summary>
    /// Individual compensation component (salary/allowance/deduction)
    /// </summary>
    public class CompensationComponentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompComponentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompPackageId { get; set; }

        public string ComponentType { get; set; } // Salary, Allowance, Deduction
        public string ComponentCode { get; set; } // Basic, Housing, Transport, etc.
        public string ComponentName { get; set; }
        public string ComponentNameAr { get; set; }

        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Create compensation package DTO
    /// </summary>
    public class CreateCompensationPackageDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public Guid? CurrencyId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public List<CreateCompensationComponentDto> Components { get; set; } = new();
    }

    /// <summary>
    /// Create compensation component DTO
    /// </summary>
    public class CreateCompensationComponentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentTypeDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentCodeDictCode { get; set; }

        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Update compensation package DTO
    /// </summary>
    public class UpdateCompensationPackageDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompPackageId { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public List<UpdateCompensationComponentDto> Components { get; set; } = new();
    }

    /// <summary>
    /// Update compensation component DTO
    /// </summary>
    public class UpdateCompensationComponentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CompComponentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentTypeDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentCodeDictCode { get; set; }

        public decimal Amount { get; set; }
    }
}
