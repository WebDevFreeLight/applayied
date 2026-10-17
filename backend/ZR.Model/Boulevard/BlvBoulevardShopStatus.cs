
namespace ZR.Model.Boulevard
{
    /// <summary>
    /// shop_status
    /// </summary>
    [SugarTable("blv_boulevard_shop_status")]
    
    [SqlDict(
        dictType: "blv_boulevard_shop_status",
        dictName: "Blv Boulevard Shop Status",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BlvBoulevardShopStatus table"
    )]
    public class BlvBoulevardShopStatus :SysBase
    {
        /// <summary>
        /// id 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long? Id { get; set; }
       public long  Id { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
       //  public string Name { get; set; }
       public string  Name { get; set; }
         
    }
}
