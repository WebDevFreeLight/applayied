
namespace ZR.Model.Boulevard
{
    /// <summary>
    /// boulevard_seasons_Shops
    /// </summary>
    [SugarTable("blv_boulevard_seasons_shops")]
    
    [SqlDict(
        dictType: "blv_boulevard_seasons_shop",
        dictName: "Blv Boulevard Seasons Shops",
        valueColumn: "seasonid",
        labelColumns: "remark",
        RemarkColumn = "remark",
        OrderBy = "seasonid ASC",
        Description = "Auto-generated from BlvBoulevardSeasonsShops table"
    )]
    public class BlvBoulevardSeasonsShops :SysBase
    {
        /// <summary>
        /// id 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long? Id { get; set; }
       public long  Id { get; set; }

        /// <summary>
        /// SeasonId 
        /// </summary>
        [SugarColumn(ColumnName = "season_id")]
       //  public long SeasonId { get; set; }
       public long  SeasonId { get; set; }

        /// <summary>
        /// ShopId 
        /// </summary>
        [SugarColumn(ColumnName = "shop_id")]
       //  public long ShopId { get; set; }
       public long  ShopId { get; set; }
         

    }
}
