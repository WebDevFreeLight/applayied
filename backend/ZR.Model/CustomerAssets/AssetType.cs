using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.CustomerAssets
{
    /// <summary>
    /// Asset Type - نوع الأصل
    /// </summary>
    [SugarTable("customer_asset_type")]
    public class AssetType : SysBase
    {
        /// <summary>
        /// Asset Type ID - معرف نوع الأصل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Code - الرمز
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Code { get; set; }

        /// <summary>
        /// Name (English) - الاسم بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameEn { get; set; }

        /// <summary>
        /// Name (Arabic) - الاسم بالعربي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
