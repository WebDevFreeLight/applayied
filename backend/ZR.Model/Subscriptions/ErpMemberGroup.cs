using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Member Group - مجموعة العملاء/المشتركين
    /// </summary>
    [SugarTable("erp_member_groups")]
    [SqlDict(
        dictType: "erp_member_group",
        dictName: "Member Group - مجموعة العملاء",
        valueColumn: "groupid",
        labelColumns: "groupname",
        RemarkColumn = "groupnamear",
        OrderBy = "groupname ASC",
        Description = "Auto-generated from ErpMemberGroup table"
    )]
    public class ErpMemberGroup : SysBase
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
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Discount Percent - نسبة الخصم الافتراضية
        /// </summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Points Multiplier - مضاعف النقاط
        /// </summary>
        public decimal PointsMultiplier { get; set; } = 1;

        /// <summary>
        /// Color - اللون
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Member Count - عدد الأعضاء
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

