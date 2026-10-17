using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// Shipping Address Table
    /// </summary>
    [SugarTable("mms_address")]
    [Tenant("1")]
    
    [SqlDict(
        dictType: "mms_address",
        dictName: "M M S User Address",
        valueColumn: "Id",
        labelColumns: "username",
        RemarkColumn = "remark",
        OrderBy = "Id ASC",
        Description = "Auto-generated from MMSUserAddress table"
    )]
    public class MMSUserAddress : SysBase
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        /// <summary>
        /// User ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }
        /// <summary>
        /// Recipient Name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Phone Number
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Phone Area Code
        /// </summary>
        public string PhoneCode { get; set; }
        /// <summary>
        /// Province City District Address
        /// </summary>
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        /// <summary>
        /// Detailed Address
        /// </summary>
        public string DetailAddress { get; set; }
        /// <summary>
        /// Is Default: 1.Yes 0.No
        /// </summary>
        public int IsDefault { get; set; }
        /// <summary>
        /// Add Time
        /// </summary>
        public DateTime AddTime { get; set; }
    }
}

