using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee compensation components - hr_employee_compensation_component
    /// </summary>
    [SugarTable("hr_employee_compensation_component")]
    public class HrEmployeeCompensationComponent : SysBase
    {
        [SugarColumn(ColumnName = "comp_component_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompComponentId { get; set; }

        [SugarColumn(ColumnName = "comp_package_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompPackageId { get; set; }

        [SugarColumn(ColumnName = "component_type_dict_code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "component_code_dict_code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentCodeDictCode { get; set; }

        [SugarColumn(ColumnName = "amount", Length = 18, DecimalDigits = 4)]
        public decimal Amount { get; set; } = 0m;
    }
}














