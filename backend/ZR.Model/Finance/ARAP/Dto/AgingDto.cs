using System;
using System.Collections.Generic;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Model.Finance.ARAP.Dto
{
    /// <summary>
    /// Aging Query DTO - استعلام التقادم
    /// </summary>
    public class AgingQueryDto
    {
        public Guid? CustomerId { get; set; }
        public Guid? VendorId { get; set; }
        public DateTime? AsOfDate { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Aging Report DTO - تقرير التقادم
    /// </summary>
    public class AgingReportDto
    {
        [ExcelColumn(Name = "Customer/Vendor ID")]
        public Guid? CustomerVendorId { get; set; }

        [ExcelColumn(Name = "Customer/Vendor Name")]
        public string CustomerVendorName { get; set; }

        [ExcelColumn(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [ExcelColumn(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        [ExcelColumn(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [ExcelColumn(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [ExcelColumn(Name = "Remaining Amount")]
        public decimal RemainingAmount { get; set; }

        [ExcelColumn(Name = "Days Overdue")]
        public int DaysOverdue { get; set; }

        [ExcelColumn(Name = "0-30 Days")]
        public decimal Age0To30 { get; set; }

        [ExcelColumn(Name = "31-60 Days")]
        public decimal Age31To60 { get; set; }

        [ExcelColumn(Name = "61-90 Days")]
        public decimal Age61To90 { get; set; }

        [ExcelColumn(Name = "90+ Days")]
        public decimal Age90Plus { get; set; }

        [ExcelColumn(Name = "Status")]
        public InvoiceStatus Status { get; set; }
    }

    /// <summary>
    /// Aging Summary DTO - ملخص التقادم
    /// </summary>
    public class AgingSummaryDto
    {
        public Guid? CustomerVendorId { get; set; }
        public string CustomerVendorName { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal Age0To30 { get; set; }
        public decimal Age31To60 { get; set; }
        public decimal Age61To90 { get; set; }
        public decimal Age90Plus { get; set; }
        public List<AgingReportDto> Invoices { get; set; }
    }
}


