using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing
{
    /// <summary>
    /// Promo Code - كود الخصم / برومو كود
    /// </summary>
    [SugarTable("erp_promo_codes")]
    [SqlDict(
        dictType: "erp_promo_code",
        dictName: "Promo Code - برومو كود",
        valueColumn: "promocodeid",
        labelColumns: "code",
        RemarkColumn = "description",
        OrderBy = "createtime DESC",
        Description = "Auto-generated from ErpPromoCode table"
    )]
    public class ErpPromoCode : SysBase
    {
        /// <summary>
        /// Promo Code ID - معرف كود الخصم
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromoCodeId { get; set; }

        /// <summary>
        /// Code - الكود
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Name - الاسم
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name Arabic - الاسم بالعربي
        /// </summary>
        public string NameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Discount Type - نوع الخصم (percent/fixed)
        /// </summary>
        public string DiscountType { get; set; } = "percent";

        /// <summary>
        /// Discount Value - قيمة الخصم
        /// </summary>
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Max Discount Amount - الحد الأقصى للخصم
        /// </summary>
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>
        /// Minimum Order Amount - الحد الأدنى للطلب
        /// </summary>
        public decimal? MinimumOrderAmount { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Max Usage Count - الحد الأقصى للاستخدام
        /// </summary>
        public int? MaxUsageCount { get; set; }

        /// <summary>
        /// Current Usage Count - عدد الاستخدامات الحالية
        /// </summary>
        public int CurrentUsageCount { get; set; }

        /// <summary>
        /// Max Usage Per User - الحد الأقصى لكل مستخدم
        /// </summary>
        public int? MaxUsagePerUser { get; set; }

        /// <summary>
        /// Applicable To - يُطبق على (all/packages/products/services)
        /// </summary>
        public string ApplicableTo { get; set; } = "all";

        /// <summary>
        /// Applicable Items - العناصر المطبق عليها (JSON)
        /// </summary>
        public string ApplicableItems { get; set; }

        /// <summary>
        /// Applicable Branches - الفروع المطبق عليها (JSON)
        /// </summary>
        public string ApplicableBranches { get; set; }

        /// <summary>
        /// Is First Time Only - للمستخدمين الجدد فقط
        /// </summary>
        public bool IsFirstTimeOnly { get; set; }

        /// <summary>
        /// Status - الحالة (active/expired/disabled)
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

