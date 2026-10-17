using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee dependents link (employee -> dependent person) - hr_employee_dependent_link
    /// </summary>
    [SugarTable("hr_employee_dependent_link")]
    public class HrEmployeeDependentLink : SysBase
    {
        [SugarColumn(ColumnName = "employee_dependent_link_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeDependentLinkId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "dependent_person_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DependentPersonId { get; set; }

        [SugarColumn(ColumnName = "relationship_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RelationshipDictCode { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}














