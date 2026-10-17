
namespace ZR.Model.Boulevard
{
    /// <summary>
    /// shop_category
    /// </summary>
    [SugarTable("blv_boulevard_shop_category")]
    
    [SqlDict(
        dictType: "blv_boulevard_shop_category",
        dictName: "Blv Boulevard Shop Category",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BlvBoulevardShopCategory table"
    )]
    public class BlvBoulevardShopCategory :SysBase
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
