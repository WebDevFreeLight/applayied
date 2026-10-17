using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee assignment (effective-dated) - hr_employee_assignment
    /// </summary>
    [SugarTable("hr_employee_assignment")]
    public class HrEmployeeAssignment : SysBase
    {
        [SugarColumn(ColumnName = "employee_assignment_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeAssignmentId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "department_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [SugarColumn(ColumnName = "position_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        [SugarColumn(ColumnName = "manager_employee_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerEmployeeId { get; set; }

        [SugarColumn(ColumnName = "work_location_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkLocationId { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = false;
    }
}














