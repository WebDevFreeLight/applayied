using Infrastructure.Converter;
using Newtonsoft.Json;
using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// DTO for viewing statutory/compliance information
    /// </summary>
    public class StatutoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeStatutoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        public string CountryName { get; set; }

        public bool HasGosi { get; set; }

        public string GosiNumber { get; set; }

        public bool IsSaudi { get; set; }

        public string MedicalInsuranceNumber { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsCurrent { get; set; }
    }

    /// <summary>
    /// DTO for creating statutory record
    /// </summary>
    public class CreateStatutoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        public bool HasGosi { get; set; }

        public string GosiNumber { get; set; }

        public bool IsSaudi { get; set; }

        public string MedicalInsuranceNumber { get; set; }

        public DateTime? EffectiveFrom { get; set; }
    }

    /// <summary>
    /// DTO for updating statutory record
    /// </summary>
    public class UpdateStatutoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        public bool? HasGosi { get; set; }

        public string GosiNumber { get; set; }

        public bool? IsSaudi { get; set; }

        public string MedicalInsuranceNumber { get; set; }

        public DateTime? EffectiveTo { get; set; }
    }
}
