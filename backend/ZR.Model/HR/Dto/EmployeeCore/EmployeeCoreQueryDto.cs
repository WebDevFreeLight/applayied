using Infrastructure.Converter;
using Newtonsoft.Json;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Employee Core - Person list query
    /// </summary>
    public class HrPersonQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PersonId { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Employee Core - Employee list query
    /// </summary>
    public class HrEmployeeCoreQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PersonId { get; set; }

        public string EmployeeCode { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Employee Core - Compensation list query (packages)
    /// </summary>
    public class HrEmployeeCompensationQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }

        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// Employee Core - Family list query (emergency contacts)
    /// </summary>
    public class HrEmployeeFamilyQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }

        public bool? IsActive { get; set; }
    }
}














