
using ZR.Model.System;

namespace ZR.Model.HR.Business
{
    /// <summary>
    /// Insurance Plans Table
    /// </summary>
    [SugarTable("hr_insurance_plan")]
    
    [SqlDict(
        dictType: "insurance_plan",
        dictName: "Hr Insurance Plan",
        valueColumn: "Planid",
        labelColumns: "planname",
        RemarkColumn = "remark",
        OrderBy = "Planid ASC",
        Description = "Auto-generated from HrInsurancePlan table"
    )]
    public class HrInsurancePlan:SysBase
    {
        /// <summary>
        /// Planid 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Planid { get; set; }

        /// <summary>
        /// اسم الخطة 
        /// </summary>
        public string Planname { get; set; }

        /// <summary>
        /// مزود 
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// نوع الخطة 
        /// </summary>
        public string Plantype { get; set; }

        /// <summary>
        /// مستوى التغطية 
        /// </summary>
        public string Coveragelevel { get; set; }

        /// <summary>
        /// تفاصيل التغطية 
        /// </summary>
        public string Coveragedetails { get; set; }

        /// <summary>
        /// القسط الشهري 
        /// </summary>
        public float Monthlypremium { get; set; }

        /// <summary>
        /// مساهمة صاحب العمل 
        /// </summary>
        public float Employercontribution { get; set; }

        /// <summary>
        /// مساهمة الموظف 
        /// </summary>
        public float Employeecontribution { get; set; }
         

    }
}
