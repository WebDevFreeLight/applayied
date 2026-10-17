using System;
using Infrastructure.Converter;

namespace ZR.Model.Customers.DataMine
{
    [SugarTable("dm_phone_reveal")]
    public class DmPhoneReveal
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "reveal_id", IsPrimaryKey = true, IsIdentity = false)]
        public long RevealId { get; set; }

        [SugarColumn(ColumnName = "user_id")]
        public long UserId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "contact_id")]
        public long ContactId { get; set; }

        [SugarColumn(ColumnName = "revealed_at")]
        public DateTime RevealedAt { get; set; }
    }
}
