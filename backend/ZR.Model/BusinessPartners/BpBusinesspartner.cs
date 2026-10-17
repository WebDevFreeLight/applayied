
using ZR.Model.System;

namespace ZR.Model.BusinessPartners
{
    /// <summary>
    /// businesspartner
    /// </summary>
    [SugarTable("bp_businesspartner")]
    
    [SqlDict(
        dictType: "bp_businesspartner",
        dictName: "Bp Businesspartner",
        valueColumn: "Partnerid",
        labelColumns: "commercialname",
        RemarkColumn = "remark",
        OrderBy = "Partnerid ASC",
        Description = "Auto-generated from BpBusinesspartner table"
    )]
    public class BpBusinesspartner:SysBase
    {
        /// <summary>
        /// Partnerid 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Partnerid { get; set; }

        /// <summary>
        /// Type 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Type { get; set; }

        /// <summary>
        /// Status 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Status { get; set; }

        /// <summary>
        /// Kind 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Kind { get; set; }

        /// <summary>
        /// Assigneddepartment 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Assigneddepartment { get; set; }

        /// <summary>
        /// Assignedto 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Assignedto { get; set; }

        /// <summary>
        /// Entityid 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Entityid { get; set; }

        /// <summary>
        /// Legacyid 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Legacyid { get; set; }

        /// <summary>
        /// Partnerstage 
        /// </summary>
        public string Partnerstage { get; set; }

        /// <summary>
        /// Commercialname 
        /// </summary>
        public string Commercialname { get; set; }

        /// <summary>
        /// Legalname 
        /// </summary>
        public string Legalname { get; set; }

        /// <summary>
        /// Vatid 
        /// </summary>
        public string Vatid { get; set; }

        /// <summary>
        /// Crnumber 
        /// </summary>
        public string Crnumber { get; set; }

        /// <summary>
        /// Industry 
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// Subindustry 
        /// </summary>
        public string Subindustry { get; set; }

        /// <summary>
        /// Contactfirstname 
        /// </summary>
        public string Contactfirstname { get; set; }

        /// <summary>
        /// Contactlastname 
        /// </summary>
        public string Contactlastname { get; set; }

        /// <summary>
        /// Businessphone 
        /// </summary>
        public string Businessphone { get; set; }

        /// <summary>
        /// Contactphone 
        /// </summary>
        public string Contactphone { get; set; }

        /// <summary>
        /// Businessemail 
        /// </summary>
        public string Businessemail { get; set; }

        /// <summary>
        /// Contactemail 
        /// </summary>
        public string Contactemail { get; set; }

        /// <summary>
        /// Mobile 
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// Buildingnumber 
        /// </summary>
        public string Buildingnumber { get; set; }

        /// <summary>
        /// Streetaddress 
        /// </summary>
        public string Streetaddress { get; set; }

        /// <summary>
        /// Unitnumber 
        /// </summary>
        public string Unitnumber { get; set; }

        /// <summary>
        /// City 
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Province 
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// Postalcode 
        /// </summary>
        public string Postalcode { get; set; }

        /// <summary>
        /// Country 
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// ExtraAttributes 
        /// </summary>
        [SugarColumn(ColumnName = "extra_attributes",IsJson =true)]
        public string ExtraAttributes { get; set; }

        /// <summary>
        /// Compliancestatus 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Compliancestatus { get; set; }


        /// <summary>
        /// Preferredcontactmethod 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Preferredcontactmethod { get; set; }

        /// <summary>
        /// BudgetRange 
        /// </summary>
        [SugarColumn(ColumnName = "budget_range")]
        public string BudgetRange { get; set; }



        /// <summary>
        /// Image Files 
        /// </summary>
        [SugarColumn(ColumnName = "image_files")]
        public string ImageFiles { get; set; }

    }
}
