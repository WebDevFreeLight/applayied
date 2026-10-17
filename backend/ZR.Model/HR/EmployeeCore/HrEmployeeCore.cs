using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee core (employment anchor) - hr_employee_core
    /// </summary>
    [SugarTable("hr_employee_core")]
    public class HrEmployeeCore : SysBase
    {
        [SugarColumn(ColumnName = "employee_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "person_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [SugarColumn(ColumnName = "user_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }

        [SugarColumn(ColumnName = "employee_code", Length = 50)]
        public string EmployeeCode { get; set; }

        [SugarColumn(ColumnName = "hire_date", IsNullable = true)]
        public DateTime? HireDate { get; set; }

        [SugarColumn(ColumnName = "termination_date", IsNullable = true)]
        public DateTime? TerminationDate { get; set; }

        [SugarColumn(ColumnName = "employment_type_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmploymentTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}














