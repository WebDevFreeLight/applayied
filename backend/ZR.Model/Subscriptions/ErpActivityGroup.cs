using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Activity Group - مجموعة النشاط
    /// </summary>
    [SugarTable("erp_activity_groups")]
    [SqlDict(
        dictType: "erp_activity_group",
        dictName: "Activity Group - مجموعة النشاط",
        valueColumn: "groupid",
        labelColumns: "groupname",
        RemarkColumn = "groupnamear",
        OrderBy = "groupname ASC",
        Description = "Auto-generated from ErpActivityGroup table"
    )]
    public class ErpActivityGroup : SysBase
    {
        /// <summary>
        /// Group ID - معرف المجموعة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GroupId { get; set; }

        /// <summary>
        /// Group Code - كود المجموعة
        /// </summary>
        public string GroupCode { get; set; }

        /// <summary>
        /// Group Name - اسم المجموعة
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Group Name Arabic - اسم المجموعة بالعربي
        /// </summary>
        public string GroupNameAr { get; set; }

        /// <summary>
        /// Activity ID - معرف النشاط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActivityId { get; set; }

        /// <summary>
        /// Trainer ID - معرف المدرب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TrainerId { get; set; }

        /// <summary>
        /// Max Members - الحد الأقصى للأعضاء
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// Current Members Count - عدد الأعضاء الحالي
        /// </summary>
        public int CurrentMembersCount { get; set; }

        /// <summary>
        /// Schedule Days - أيام الجدول (JSON)
        /// </summary>
        public string ScheduleDays { get; set; }

        /// <summary>
        /// Start Time - وقت البداية
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// End Time - وقت النهاية
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Resource ID - معرف المورد/الغرفة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ResourceId { get; set; }

    

        /// <summary>
        /// Price - السعر
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

