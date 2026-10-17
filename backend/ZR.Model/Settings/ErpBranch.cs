using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Settings
{
    /// <summary>
    /// Branch - الفرع
    /// </summary>
    [SugarTable("erp_branches")]
    [SqlDict(
        dictType: "erp_branch",
        dictName: "Branch - الفرع",
        valueColumn: "BranchID",
        labelColumns: "branchname",
        RemarkColumn = "branchnamear",
        OrderBy = "branchcode ASC",
        Description = "Auto-generated from ErpBranch table"
    )]
    public class ErpBranch : SysBase
    {
        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BranchID { get; set; }

        /// <summary>
        /// Branch Code - كود الفرع
        /// </summary>
        public string BranchCode { get; set; }

        /// <summary>
        /// Branch Name - اسم الفرع
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Branch Name Arabic - اسم الفرع بالعربي
        /// </summary>
        public string BranchNameAr { get; set; }

        /// <summary>
        /// Branch Type - نوع الفرع (main/sub)
        /// </summary>
        public string BranchType { get; set; } = "main";

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Address - العنوان
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// City - المدينة
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Country - الدولة
        /// </summary>
        public string Country { get; set; } = "Saudi Arabia";

        /// <summary>
        /// Postal Code - الرمز البريدي
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Manager ID - معرف المدير
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerId { get; set; }

        /// <summary>
        /// Default Warehouse ID - المخزن الافتراضي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DefaultWarehouseId { get; set; }

        /// <summary>
        /// Default Treasury ID - الخزينة الافتراضية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DefaultTreasuryId { get; set; }

        /// <summary>
        /// Opening Time - وقت الفتح
        /// </summary>
        public TimeSpan? OpeningTime { get; set; }

        /// <summary>
        /// Closing Time - وقت الإغلاق
        /// </summary>
        public TimeSpan? ClosingTime { get; set; }

        /// <summary>
        /// Working Days - أيام العمل (JSON)
        /// </summary>
        public string WorkingDays { get; set; }

        /// <summary>
        /// Auto Shift - وردية آلية
        /// </summary>
        public bool AutoShift { get; set; }

        /// <summary>
        /// Auto Daily Report - يومية آلية
        /// </summary>
        public bool AutoDailyReport { get; set; }

        /// <summary>
        /// Logo URL - رابط الشعار
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// Latitude - خط العرض
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// Longitude - خط الطول
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// Is Main - فرع رئيسي
        /// </summary>
        public bool IsMain { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }



    public class ErpBranchDTO : PagerInfo
    {
        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BranchID { get; set; }

        /// <summary>
        /// Branch Code - كود الفرع
        /// </summary>
        public string BranchCode { get; set; }

        /// <summary>
        /// Branch Name - اسم الفرع
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Branch Name Arabic - اسم الفرع بالعربي
        /// </summary>
        public string BranchNameAr { get; set; }

        /// <summary>
        /// Branch Type - نوع الفرع (main/sub)
        /// </summary>
        public string BranchType { get; set; } = "main";

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Address - العنوان
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// City - المدينة
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Country - الدولة
        /// </summary>
        public string Country { get; set; } = "Saudi Arabia";

        /// <summary>
        /// Postal Code - الرمز البريدي
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Manager ID - معرف المدير
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerId { get; set; }

        /// <summary>
        /// Default Warehouse ID - المخزن الافتراضي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DefaultWarehouseId { get; set; }

        /// <summary>
        /// Default Treasury ID - الخزينة الافتراضية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DefaultTreasuryId { get; set; }

        /// <summary>
        /// Opening Time - وقت الفتح
        /// </summary>
        public TimeSpan? OpeningTime { get; set; }

        /// <summary>
        /// Closing Time - وقت الإغلاق
        /// </summary>
        public TimeSpan? ClosingTime { get; set; }

        /// <summary>
        /// Working Days - أيام العمل (JSON)
        /// </summary>
        public string WorkingDays { get; set; }

        /// <summary>
        /// Auto Shift - وردية آلية
        /// </summary>
        public bool AutoShift { get; set; }

        /// <summary>
        /// Auto Daily Report - يومية آلية
        /// </summary>
        public bool AutoDailyReport { get; set; }

        /// <summary>
        /// Logo URL - رابط الشعار
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// Latitude - خط العرض
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// Longitude - خط الطول
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// Is Main - فرع رئيسي
        /// </summary>
        public bool IsMain { get; set; }

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

