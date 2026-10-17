using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee emergency contacts - hr_employee_emergency_contact
    /// </summary>
    [SugarTable("hr_employee_emergency_contact")]
    public class HrEmployeeEmergencyContact : SysBase
    {
        [SugarColumn(ColumnName = "emergency_contact_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmergencyContactId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "contact_name", Length = 200)]
        public string ContactName { get; set; }

        [SugarColumn(ColumnName = "relationship_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RelationshipDictCode { get; set; }

        [SugarColumn(ColumnName = "phone", Length = 30, IsNullable = true)]
        public string Phone { get; set; }

        [SugarColumn(ColumnName = "alternate_phone", Length = 30, IsNullable = true)]
        public string AlternatePhone { get; set; }

        [SugarColumn(ColumnName = "email", Length = 200, IsNullable = true)]
        public string Email { get; set; }

        [SugarColumn(ColumnName = "address", Length = 500, IsNullable = true)]
        public string Address { get; set; }

        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = false;

        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}














