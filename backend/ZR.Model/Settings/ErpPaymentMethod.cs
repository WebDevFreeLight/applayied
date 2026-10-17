using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Settings
{
    /// <summary>
    /// Payment Method - طريقة الدفع
    /// </summary>
    [SugarTable("erp_payment_methods")]
    [SqlDict(
        dictType: "erp_payment_method",
        dictName: "Payment Method - طريقة الدفع",
        valueColumn: "paymentmethodid",
        labelColumns: "methodname",
        RemarkColumn = "methodnamear",
        OrderBy = "sortorder ASC",
        Description = "Auto-generated from ErpPaymentMethod table"
    )]
    public class ErpPaymentMethod : SysBase
    {
        /// <summary>
        /// Payment Method ID - معرف طريقة الدفع
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PaymentMethodId { get; set; }

        /// <summary>
        /// Method Code - كود الطريقة
        /// </summary>
        public string MethodCode { get; set; }

        /// <summary>
        /// Method Name - اسم الطريقة
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Method Name Arabic - اسم الطريقة بالعربي
        /// </summary>
        public string MethodNameAr { get; set; }

        /// <summary>
        /// Method Type - نوع الطريقة (cash/card/bank/online/wallet)
        /// </summary>
        public string MethodType { get; set; }

        /// <summary>
        /// Is Cash - نقدي
        /// </summary>
        public bool IsCash { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب المرتبط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccountId { get; set; }

        /// <summary>
        /// Default Treasury ID - الخزينة الافتراضية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DefaultTreasuryId { get; set; }

        /// <summary>
        /// Processing Fee Percent - نسبة رسوم المعالجة
        /// </summary>
        public decimal ProcessingFeePercent { get; set; }

        /// <summary>
        /// Processing Fee Fixed - رسوم معالجة ثابتة
        /// </summary>
        public decimal ProcessingFeeFixed { get; set; }

        /// <summary>
        /// Icon - الأيقونة
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Color - اللون
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Sort Order - ترتيب العرض
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Is Default - افتراضي
        /// </summary>
        public bool IsDefault { get; set; }

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

