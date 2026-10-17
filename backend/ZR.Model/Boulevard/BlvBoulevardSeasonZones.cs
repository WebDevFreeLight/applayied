
namespace ZR.Model.Boulevard
{


    /// <summary>
    /// boulevard_season_zones
    /// </summary>
    [SugarTable("blv_boulevard_season_zones")]
    
    [SqlDict(
        dictType: "blv_boulevard_season_zone",
        dictName: "Blv Boulevard Season Zones",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BlvBoulevardSeasonZones table"
    )]
    public class BlvBoulevardSeasonZones :SysBase
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
        /// ImageFile 
        /// </summary>
        [SugarColumn(ColumnName = "image_file")]
       //  public string ImageFile { get; set; }
       public string  ImageFile { get; set; }

        /// <summary>
        /// Season (Event) Id - FK to blv_boulevard_seasons
        /// </summary>
        [SugarColumn(ColumnName = "season_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SeasonId { get; set; }
    }

     










    ///// <summary>
    ///// shop_category
    ///// </summary>
    //[SugarTable("blv_boulevard_shop_category")]
    //public class shop_category : SysBase
    //{
    //    /// <summary>
    //    /// id 
    //    /// </summary>

    //    [JsonConverter(typeof(ValueToStringConverter))]
    //    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    //    //  public long? Id { get; set; }
    //    public long Id { get; set; }

    //    /// <summary>
    //    /// Name 
    //    /// </summary> 
    //    public string Name { get; set; }

    //}







    ///// <summary>
    ///// shop_status
    ///// </summary>
    //[SugarTable("blv_boulevard_shop_status")]
    //public class shop_status : SysBase
    //{
    //    /// <summary>
    //    /// id 
    //    /// </summary>

    //    [JsonConverter(typeof(ValueToStringConverter))]
    //    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    //    //  public long? Id { get; set; }
    //    public long Id { get; set; }

    //    /// <summary>
    //    /// Name 
    //    /// </summary>
    //    public string Name { get; set; }
    //}








    ///// <summary>
    ///// shops
    ///// </summary>
    //[SugarTable("blv_boulevard_shops")]
    //public class shop  : SysBase
    //{
    //    /// <summary>
    //    /// id 
    //    /// </summary>

    //    [JsonConverter(typeof(ValueToStringConverter))]
    //    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]

    //    public long Id { get; set; }

    //    /// <summary>
    //    /// Name 
    //    /// </summary> 
    //    public string Name { get; set; }
    //    public long Zone_id { get; set; }


    //    /// <summary>
    //    /// Shop Status
    //    /// </summary>


    //    public long shop_status { get; set; }

    //    /// <summary>
    //    /// Shop Category
    //    /// </summary>
    //    public long shop_category { get; set; }





    //    /// <summary>
    //    /// Shop Area
    //    /// </summary>

    //    public decimal shop_area { get; set; }

    //    /// <summary>
    //    /// Meter Price 
    //    /// </summary>
    //    public decimal meter_price { get; set; }

    //    /// <summary>
    //    /// Total Price 
    //    /// </summary>


    //    public decimal total_price { get; set; }

    //    /// <summary>
    //    /// Image Files
    //    /// </summary>



    //    public string  image_files { get; set; }
    //}





}
