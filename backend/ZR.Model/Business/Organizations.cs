
using ZR.Model.System;

namespace ZR.Model.Business
{
    /// <summary>
    /// Organizations Information Table
    /// </summary>
    [SugarTable("organizations")]
    
    [SqlDict(
        dictType: "organization",
        dictName: "Organizations",
        valueColumn: "Organizationsid",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from Organizations table"
    )]
    public class Organizations:SysBase
    {
        /// <summary>
        /// Organization ID 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]


        [JsonConverter(typeof(ValueToStringConverter))]

         public long Organizationsid { get; set; }

        /// <summary>
        /// Organization name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Organizationtypeid 
        /// </summary>
        public string Organizationtypeid { get; set; }

        /// <summary>
        /// logo_url 
        /// </summary>
        [SugarColumn(ColumnName = "logo_url")]
        public string LogoUrl { get; set; }

        /// <summary>
        /// display_order 
        /// </summary>
        [SugarColumn(ColumnName = "display_order")]
        public int? DisplayOrder { get; set; }

        /// <summary>
        /// Parentid 
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Parentid { get; set; } = 0;

   

        [SugarColumn(IsIgnore = true)]
        public List<Organizations> Children { get; set; }
    }
}

