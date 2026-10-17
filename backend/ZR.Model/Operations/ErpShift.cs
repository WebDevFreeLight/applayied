using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Shift - الوردية
    /// </summary>
    [SugarTable("erp_shifts")]
    [SqlDict(
        dictType: "erp_shift",
        dictName: "Shift - الوردية",
        valueColumn: "shiftid",
        labelColumns: "shiftnumber",
        RemarkColumn = "notes",
        OrderBy = "starttime DESC",
        Description = "Auto-generated from ErpShift table"
    )]
    public class ErpShift : SysBase
    {
        /// <summary>
        /// Shift ID - معرف الوردية
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }

        /// <summary>
        /// Shift Number - رقم الوردية
        /// </summary>
        public string ShiftNumber { get; set; } 
        /// <summary>
        /// Treasury ID - معرف الخزينة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TreasuryId { get; set; }

        /// <summary>
        /// Employee ID - معرف الموظف
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Start Time - وقت البداية
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End Time - وقت النهاية
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Opening Balance - الرصيد الافتتاحي
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Closing Balance - الرصيد الختامي
        /// </summary>
        public decimal ClosingBalance { get; set; }

        /// <summary>
        /// Expected Balance - الرصيد المتوقع
        /// </summary>
        public decimal ExpectedBalance { get; set; }

        /// <summary>
        /// Difference - الفرق
        /// </summary>
        public decimal Difference { get; set; }

        /// <summary>
        /// Total Sales - إجمالي المبيعات
        /// </summary>
        public decimal TotalSales { get; set; }

        /// <summary>
        /// Total Returns - إجمالي المرتجعات
        /// </summary>
        public decimal TotalReturns { get; set; }

        /// <summary>
        /// Total Receipts - إجمالي المقبوضات
        /// </summary>
        public decimal TotalReceipts { get; set; }

        /// <summary>
        /// Total Payments - إجمالي المدفوعات
        /// </summary>
        public decimal TotalPayments { get; set; }

        /// <summary>
        /// Total Expenses - إجمالي المصروفات
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// Cash Sales - مبيعات نقدية
        /// </summary>
        public decimal CashSales { get; set; }

        /// <summary>
        /// Card Sales - مبيعات بطاقات
        /// </summary>
        public decimal CardSales { get; set; }

        /// <summary>
        /// Transfer Sales - مبيعات تحويل
        /// </summary>
        public decimal TransferSales { get; set; }

        /// <summary>
        /// Invoices Count - عدد الفواتير
        /// </summary>
        public int InvoicesCount { get; set; }

        /// <summary>
        /// Returns Count - عدد المرتجعات
        /// </summary>
        public int ReturnsCount { get; set; }

        /// <summary>
        /// Status - الحالة (open/closed)
        /// </summary>
        public string Status { get; set; } = "open";

        /// <summary>
        /// Closed By - أغلقت بواسطة
        /// </summary>
        public string ClosedBy { get; set; }

        /// <summary>
        /// Is Auto - آلية
        /// </summary>
        public bool IsAuto { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

