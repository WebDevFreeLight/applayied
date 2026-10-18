using System;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    /// <summary>Filter for the PMO employee directory (keyword over code/name, active flag).</summary>
    public class PmoEmployeeQueryDto : PagerInfo
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>Read model for the directory: employee fields + the linked user's display name.</summary>
    public class PmoEmployeeVo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>Create/update payload for a PMO employee record (over hr_employee_core).</summary>
    public class PmoEmployeeInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        /// <summary>Optional linked portal user (enables the login employee lookup).</summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }

        public string EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
