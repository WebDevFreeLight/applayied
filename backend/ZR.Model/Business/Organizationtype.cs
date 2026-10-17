
using ZR.Model.System;

namespace ZR.Model.Business
{
    /// <summary>
    /// Organizations Information Table
    /// </summary>
    [SugarTable("organizationtype")]
    
    [SqlDict(
        dictType: "organizationtype",
        dictName: "Organizationtype",
        valueColumn: "organizationtypeid",
        labelColumns: "organizationtypename",
        RemarkColumn = "remark",
        OrderBy = "organizationtypeid ASC",
        Description = "Auto-generated from Organizationtype table"
    )]
    public class Organizationtype:SysBase
    {
        /// <summary>
        /// Organization ID 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true )]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Organizationtypeid { get; set; }

        /// <summary>
        /// OrganizationType name 
        /// </summary>
        public string Organizationtypename { get; set; }
         

    } 



     




}

