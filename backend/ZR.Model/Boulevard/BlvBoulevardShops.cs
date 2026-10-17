
namespace ZR.Model.Boulevard
{
    /// <summary>
    /// shops
    /// </summary>
    [SugarTable("blv_boulevard_shops")]
    
    [SqlDict(
        dictType: "blv_boulevard_shop",
        dictName: "Blv Boulevard Shops",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BlvBoulevardShops table"
    )]
    public class BlvBoulevardShops :SysBase
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

        /// <summary>
        /// ZoneId 
        /// </summary>
        [SugarColumn(ColumnName = "zone_id")]
       //  public long ZoneId { get; set; }
       public long  ZoneId { get; set; }

        /// <summary>
        /// Shop Status 
        /// </summary>
        [SugarColumn(ColumnName = "shop_status")]
       //  public long ShopStatus { get; set; }
       public long  ShopStatus { get; set; }

        /// <summary>
        /// Shop Category 
        /// </summary>
        [SugarColumn(ColumnName = "shop_category")]
       //  public long ShopCategory { get; set; }
       public long  ShopCategory { get; set; }

        /// <summary>
        /// Shop Area 
        /// </summary>
        [SugarColumn(ColumnName = "shop_area")]
       //  public float ShopArea { get; set; }
       public float  ShopArea { get; set; }

        /// <summary>
        /// Meter Price 
        /// </summary>
        [SugarColumn(ColumnName = "meter_price")]
       //  public float MeterPrice { get; set; }
       public float  MeterPrice { get; set; }

        /// <summary>
        /// Total Price 
        /// </summary>
        [SugarColumn(ColumnName = "total_price")]
       //  public float TotalPrice { get; set; }
       public float  TotalPrice { get; set; }

        /// <summary>
        /// Image Files
        /// </summary>
        [SugarColumn(ColumnName = "image_files")]
       public string  ImageFiles { get; set; }

        /// <summary>
        /// Unit Type: 1=Kiosk, 2=Shop
        /// </summary>
        [SugarColumn(ColumnName = "unit_type")]
        public int UnitType { get; set; }

        /// <summary>
        /// Rental Type: 1=FullRent, 2=RevenueShare
        /// </summary>
        [SugarColumn(ColumnName = "rental_type")]
        public int RentalType { get; set; }

        /// <summary>
        /// Revenue Percentage (only for RevenueShare)
        /// </summary>
        [SugarColumn(ColumnName = "revenue_percentage", IsNullable = true)]
        public float? RevenuePercentage { get; set; }

    }
}
