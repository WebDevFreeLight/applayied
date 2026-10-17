using Infrastructure.Converter;
using Newtonsoft.Json;
using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// DTO for viewing contract information
    /// </summary>
    public class ContractDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeContractId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ContractTypeDictCode { get; set; }

        public string ContractTypeName { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsCurrent { get; set; }

        public int? ContractDurationDays { get; set; }

        public bool IsProbationActive { get; set; }
    }

    /// <summary>
    /// DTO for creating contract record
    /// </summary>
    public class CreateContractDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ContractTypeDictCode { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? EffectiveFrom { get; set; }
    }

    /// <summary>
    /// DTO for updating contract record
    /// </summary>
    public class UpdateContractDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ContractTypeDictCode { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? EffectiveTo { get; set; }
    }
}
