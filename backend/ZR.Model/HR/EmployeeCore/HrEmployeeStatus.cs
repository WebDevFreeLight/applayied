using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee status history (effective-dated) - hr_employee_status
    /// </summary>
    [SugarTable("hr_employee_status")]
    public class HrEmployeeStatus : SysBase
    {
        [SugarColumn(ColumnName = "employee_status_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeStatusId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "employment_status_dict_code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmploymentStatusDictCode { get; set; }

        [SugarColumn(ColumnName = "status_reason_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusReasonDictCode { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }
    }
}














