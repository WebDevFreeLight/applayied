using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Person (identity) - hr_person
    /// </summary>
    [SugarTable("hr_person")]
    public class HrPerson : SysBase
    {
        [SugarColumn(ColumnName = "person_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}














