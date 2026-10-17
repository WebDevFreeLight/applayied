using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Finance.CostCenter
{
    /// <summary>
    /// Cost Center - مركز التكلفة
    /// </summary>
    [SugarTable("finance_cost_centers")]
    [SqlDict(
        dictType: "finance_cost_center",
        dictName: "Cost Center - مركز التكلفة",
        valueColumn: "costcenterid",
        labelColumns: "costcentername",
        RemarkColumn = "costcenternamear",
        OrderBy = "costcentercode ASC",
        Description = "Auto-generated from CostCenter table"
    )]
    public class CostCenter : SysBase
    {
        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CostCenterId { get; set; }

        /// <summary>
        /// Parent ID - معرف الأب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentId { get; set; }

        /// <summary>
        /// Cost Center Code - كود مركز التكلفة
        /// </summary>
        public string CostCenterCode { get; set; }

        /// <summary>
        /// Cost Center Name - اسم مركز التكلفة
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// Cost Center Name Arabic - اسم مركز التكلفة بالعربي
        /// </summary>
        public string CostCenterNameAr { get; set; }

        /// <summary>
        /// Cost Center Type - نوع مركز التكلفة (branch/department/project)
        /// </summary>
        public string CostCenterType { get; set; }

        /// <summary>
        /// Level - المستوى
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Is Parent - رئيسي
        /// </summary>
        public bool IsParent { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

