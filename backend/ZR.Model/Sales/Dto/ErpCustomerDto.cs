using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.Sales.Dto
{
    /// <summary>
    /// Customer Query DTO - استعلام العميل
    /// </summary>
    public class ErpCustomerQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CustomerId { get; set; }

        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
    }

    /// <summary>
    /// Customer DTO - بيانات العميل
    /// </summary>
    public class ErpCustomerDto
    {
        [ExcelColumn(Name = "Customer ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CustomerId { get; set; }

        [Required(ErrorMessage = "كود العميل مطلوب")]
        [ExcelColumn(Name = "Customer Code")]
        public string CustomerCode { get; set; }

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [ExcelColumn(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [ExcelColumn(Name = "Customer Name Arabic")]
        public string CustomerNameAr { get; set; }

        [ExcelColumn(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [ExcelColumn(Name = "Phone")]
        public string Phone { get; set; }

        [ExcelColumn(Name = "Mobile")]
        public string Mobile { get; set; }

        [ExcelColumn(Name = "Email")]
        public string Email { get; set; }

        [ExcelColumn(Name = "VAT Number")]
        public string VatNumber { get; set; }

        [ExcelColumn(Name = "National ID")]
        public string NationalId { get; set; }

        [ExcelColumn(Name = "Credit Limit")]
        public decimal CreditLimit { get; set; }

        [ExcelColumn(Name = "Current Balance")]
        public decimal CurrentBalance { get; set; }

        [ExcelColumn(Name = "Address")]
        public string Address { get; set; }

        [ExcelColumn(Name = "City")]
        public string City { get; set; }

        [ExcelColumn(Name = "Country")]
        public string Country { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccountId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SalespersonId { get; set; }

        public int PaymentTermsDays { get; set; }

        public string Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// Customer Balance Summary - ملخص رصيد العميل
    /// </summary>
    public class ErpCustomerBalanceSummaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal AvailableCredit { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalPayments { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}

