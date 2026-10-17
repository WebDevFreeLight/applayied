using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Permit-Worker Junction - عمال التصريح
    /// </summary>
    [SugarTable("ops_permit_worker")]
    public class OpsPermitWorker : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = false)]
        public long Id { get; set; }

        /// <summary>
        /// FK to ops_permit
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "permit_id")]
        public long PermitId { get; set; }

        /// <summary>
        /// FK to ops_customer_worker
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "worker_id")]
        public long WorkerId { get; set; }

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public string WorkerName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string WorkerNameAr { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string NationalId { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string Phone { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string IqamaImageUrl { get; set; }
    }
}
