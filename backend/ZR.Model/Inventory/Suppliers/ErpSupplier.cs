using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Inventory.Suppliers
{
    /// <summary>
    /// Supplier - المورد
    /// </summary>
    [SugarTable("erp_suppliers")]
    [SqlDict(
        dictType: "erp_supplier",
        dictName: "Supplier - المورد",
        valueColumn: "supplierid",
        labelColumns: "suppliername",
        RemarkColumn = "suppliernamear",
        OrderBy = "suppliercode ASC",
        Description = "Auto-generated from ErpSupplier table"
    )]
    public class ErpSupplier : SysBase
    {
        /// <summary>
        /// Supplier ID - معرف المورد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SupplierId { get; set; }

        /// <summary>
        /// Supplier Code - كود المورد
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// Supplier Name - اسم المورد
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// Supplier Name Arabic - اسم المورد بالعربي
        /// </summary>
        public string SupplierNameAr { get; set; }

        /// <summary>
        /// Contact Person - شخص التواصل
        /// </summary>
        public string ContactPerson { get; set; }

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
        /// Commercial Registration - السجل التجاري
        /// </summary>
        public string CommercialRegistration { get; set; }

        /// <summary>
        /// Credit Limit - الحد الائتماني
        /// </summary>
        public decimal CreditLimit { get; set; }

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Payment Terms Days - أيام السداد
        /// </summary>
        public int PaymentTermsDays { get; set; } = 30;

        /// <summary>
        /// Account ID - معرف الحساب المرتبط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccountId { get; set; }

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
        /// Bank Name - اسم البنك
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Bank Account Number - رقم الحساب البنكي
        /// </summary>
        public string BankAccountNumber { get; set; }

        /// <summary>
        /// IBAN - رقم الآيبان
        /// </summary>
        public string IBAN { get; set; }

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

