using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Customer - العملاء
    /// </summary>
    [SugarTable("erp_customers")]
    [SqlDict(
        dictType: "erp_customer",
        dictName: "Customer - العميل",
        valueColumn: "customerid",
        labelColumns: "customername",
        RemarkColumn = "customernameAr",
        OrderBy = "customercode ASC",
        Description = "Auto-generated from ErpCustomer table"
    )]
    public class ErpCustomer : SysBase
    {
        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CustomerId { get; set; }

        /// <summary>
        /// Customer Code - كود العميل
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Customer Name Arabic - اسم العميل بالعربي
        /// </summary>
        public string CustomerNameAr { get; set; }

        /// <summary>
        /// Customer Type - نوع العميل (individual/corporate)
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Mobile - الجوال
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// VAT Number - الرقم الضريبي
        /// </summary>
        public string VatNumber { get; set; }

        /// <summary>
        /// National ID - رقم الهوية
        /// </summary>
        public string NationalId { get; set; }

        /// <summary>
        /// Credit Limit - الحد الائتماني
        /// </summary>
        public decimal CreditLimit { get; set; }

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// </summary>
        public decimal CurrentBalance { get; set; }

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
        /// Account ID - معرف الحساب المرتبط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccountId { get; set; }

        /// <summary>
        /// Price List ID - قائمة الأسعار
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PriceListId { get; set; }

        /// <summary>
        /// Salesperson ID - معرف مندوب المبيعات
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SalespersonId { get; set; }

        /// <summary>
        /// Payment Terms Days - أيام السداد
        /// </summary>
        public int PaymentTermsDays { get; set; } = 0;

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

