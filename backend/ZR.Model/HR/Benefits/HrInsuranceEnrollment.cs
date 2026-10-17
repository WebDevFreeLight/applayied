
using ZR.Model.System;

namespace ZR.Model.Hr
{
    /// <summary>
    /// insuranceenrollment
    /// </summary>
    [SugarTable("hr_insurance_enrollment")]
    
    [SqlDict(
        dictType: "insurance_enrollment",
        dictName: "Hr Insurance Enrollment",
        valueColumn: "Enrollmentid",
        labelColumns: "status",
        RemarkColumn = "remark",
        OrderBy = "Enrollmentid ASC",
        Description = "Auto-generated from HrInsuranceEnrollment table"
    )]
    public class HrInsuranceEnrollment : SysBase
    {
        /// <summary>
        /// Enrollmentid 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Enrollmentid { get; set; }

        /// <summary>
        /// معرف الموظف 
        /// </summary>
        public long Employeeid { get; set; }

        /// <summary>
        /// معرف الخطة 
        /// </summary>
        public long Planid { get; set; }

        /// <summary>
        /// تاريخ التسجيل 
        /// </summary>
        public DateTime? Enrollmentdate { get; set; }

        /// <summary>
        /// تاريخ الإنهاء 
        /// </summary>
        public DateTime? Terminationdate { get; set; }

        /// <summary>
        /// مبلغ التغطية 
        /// </summary>
        public float Coverageamount { get; set; }

        /// <summary>
        /// القسط الشهري 
        /// </summary>
        public float Monthlypremium { get; set; }

        /// <summary>
        /// حالة التأمين 
        /// </summary>
        public string Status { get; set; }
    }
}
