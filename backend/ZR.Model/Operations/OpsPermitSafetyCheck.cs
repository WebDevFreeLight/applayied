using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Permit Safety Check - فحص سلامة التصريح
    /// </summary>
    [SugarTable("ops_permit_safety_check")]
    public class OpsPermitSafetyCheck : SysBase
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
        /// FK to ops_safety_precaution
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "precaution_id")]
        public long PrecautionId { get; set; }

        /// <summary>
        /// Whether this precaution is checked/applied
        /// </summary>
        [SugarColumn(ColumnName = "is_checked")]
        public bool IsChecked { get; set; }

        /// <summary>
        /// Optional notes for this precaution
        /// </summary>
        [SugarColumn(ColumnName = "notes", Length = 500, IsNullable = true)]
        public string Notes { get; set; }

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public string PrecautionDescription { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string PrecautionDescriptionAr { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string PrecautionCode { get; set; }
    }
}
