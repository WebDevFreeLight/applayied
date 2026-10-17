
namespace ZR.Model.Boulevard
{
    /// <summary>
    /// boulevard_seasons
    /// </summary>
    [SugarTable("set_admin_settings")]
    public class admin_settings  : SysBase
    {
        /// <summary>
        /// id 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       public long  Id { get; set; }
        /// <summary>
        //  أسم الشركة 
        /// </summary>
       public string  Name { get; set; }

        /// <summary>
        ///  شعار الشركة 
        /// </summary>
        public string image { get; set; }


        /// <summary>
        ///  الارقام 
        /// </summary>
        public string phones { get; set; }
        
        /// <summary>
        ///  عناوين 
        /// </summary>
        public string  address { get; set; }
        /// <summary>
        /// ايميل الشركة 
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// بعد كم دقيقة نحسب تاخير حضور
        /// </summary>
        public int after_miniute_calculate_delay { get; set; }
        /// <summary>
        /// بعد كم دقيقة نحسب انصراف مبكر
        /// </summary>
        public int after_miniute_caloulate_early_departure { get; set; }

        /// <summary>
        /// بعد كم المبكر او الحضور المتأخر نحصم ربع يوم
        /// </summary>
        public int after_miniute_guarterday { get; set; }

        /// <summary>
        /// كم مرة تأخير او انصارف مبكر نخصم نص يوم
        /// </summary>

        public int after_time_half_daycut { get; set; }


        /// <summary>
        /// كم مرة تأخير او انصارف مبكر نخصم نص يوم
        /// </summary>
        public int  after_tine_half_daycut { get; set; }








    }








    /// <summary>
    /// boulevard_seasons
    /// </summary>
    [SugarTable("blv_boulevard_seasons")]
    
    [SqlDict(
        dictType: "blv_boulevard_season",
        dictName: "Blv Boulevard Seasons",
        valueColumn: "id",
        labelColumns: "name",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from BlvBoulevardSeasons table"
    )]
    public class BlvBoulevardSeasons : SysBase
    {
        /// <summary>
        /// id 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        //  public long? Id { get; set; }
        public long Id { get; set; }
        /// <summary>
        /// Name 
        /// </summary>
        //  public string Name { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Start Date 
        /// </summary>
        [SugarColumn(ColumnName = "start_date")]
        //  public DateTime? StartDate { get; set; }
        public DateTime StartDate { get; set; }
        /// <summary>
        /// End Date 
        /// </summary>
        [SugarColumn(ColumnName = "end_date")]
        //  public DateTime? EndDate { get; set; }
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Image File
        /// </summary>
        [SugarColumn(ColumnName = "image_file", IsNullable = true)]
        public string ImageFile { get; set; }
    }






}
