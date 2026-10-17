
namespace ZR.Model.Boulevard
{
    /// <summary>
    /// CustomerRegistrationForm
    /// </summary>
    [SugarTable("blv_customerregistrationform")]
    
    [SqlDict(
        dictType: "blv_customerregistrationform",
        dictName: "Blv Customerregistrationform",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "notes",
        OrderBy = "id ASC",
        Description = "Auto-generated from BlvCustomerregistrationform table"
    )]
    public class BlvCustomerregistrationform :SysBase
    {
        /// <summary>
        /// id 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long Id { get; set; }
       public long  Id { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
       //  public string Name { get; set; }
       public string  Name { get; set; }

        /// <summary>
        /// Phone 
        /// </summary>
       //  public string Phone { get; set; }
       public string  Phone { get; set; }

        /// <summary>
        /// Email 
        /// </summary>
       //  public string Email { get; set; }
       public string  Email { get; set; }

        /// <summary>
        /// Notes 
        /// </summary>
       //  public string Notes { get; set; }
       public string  Notes { get; set; }
    }
}
