using ZR.Model.System;
using Newtonsoft.Json;
using SqlSugar;
using Infrastructure.Converter;
using ZR.Model.Finance.Zatca.Enums;
using System;

namespace ZR.Model.HR.Payroll
{
    /// <summary>
    /// Salary Compliance - امتثال الرواتب
    /// </summary>
    [SugarTable("hr_salary_compliance")]
    
    [SqlDict(
        dictType: "salary_compliance",
        dictName: "Hr Salary Compliance",
        valueColumn: "Complianceid",
        labelColumns: "country",
        RemarkColumn = "notes",
        OrderBy = "Complianceid ASC",
        Description = "Auto-generated from HrSalaryCompliance table"
    )]
    public class HrSalaryCompliance:SysBase
    {
        /// <summary>
        /// Compliance ID 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Complianceid { get; set; }

        /// <summary>
        /// Country - البلد 
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Social Insurance Rate - معدل التأمينات الاجتماعية 
        /// </summary>
        public float Socialinsurancerate { get; set; }

        /// <summary>
        /// End of Service Formula - صيغة نهاية الخدمة 
        /// </summary>
        public string Endofserviceformula { get; set; }

        /// <summary>
        /// VAT Requirements - متطلبات ضريبة القيمة المضافة 
        /// </summary>
        public string Vatrequirements { get; set; }

        /// <summary>
        /// Procedures - الإجراءات 
        /// </summary>
        public string Procedures { get; set; }

        /// <summary>
        /// Currency Code 
        /// </summary>
        public string Currencycode { get; set; }

        /// <summary>
        /// Minimum Wage 
        /// </summary>
        public float Minimumwage { get; set; }

        /// <summary>
        /// Maximum Working Hours Per Week 
        /// </summary>
        public int? Maxworkinghoursperweek { get; set; }

        /// <summary>
        /// Overtime Rate Multiplier 
        /// </summary>
        public float Overtimeratemultiplier { get; set; }

        /// <summary>
        /// Annual Leave Days 
        /// </summary>
        public int? Annualleavedays { get; set; }

        /// <summary>
        /// Sick Leave Days 
        /// </summary>
        public int? Sickleavedays { get; set; }

        /// <summary>
        /// Maternity Leave Days 
        /// </summary>
        public int? Maternityleavedays { get; set; }

        /// <summary>
        /// Paternity Leave Days 
        /// </summary>
        public int? Paternityleavedays { get; set; }

        /// <summary>
        /// Tax Brackets 
        /// </summary>
        public string Taxbrackets { get; set; }

        /// <summary>
        /// Compliance Notes 
        /// </summary>
        public string Notes { get; set; } 

        /// <summary>
        /// Tax Configuration ID (FK -> tax_configuration.id)
        /// </summary>
        public Guid? TaxConfigId { get; set; }

        /// <summary>
        /// ZATCA Environment (enum)
        /// </summary>
        public ZatcaEnvironment ZatcaEnvironment { get; set; } = ZatcaEnvironment.Sandbox;

    }
}
