namespace ZR.Mall.Model
{
    /// <summary>
    /// ???
    /// </summary>
    public class OMSOrderItemDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ItemId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OrderId { get; set; }
        /// <summary>
        /// ??ID(??)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }
        /// <summary>
        /// ???(??)
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// ????(??)
        /// </summary>
        public string ProductPic { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SkuId { get; set; }
        /// <summary>
        /// ??????????
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// ??(?? * ??)
        /// </summary>
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }
        /// <summary>
        /// ????(??:??:??;??:XL)
        /// </summary>
        public string SkuSpec { get; set; }
    }
}

