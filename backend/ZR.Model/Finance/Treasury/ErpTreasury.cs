using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury - الخزينة
    /// </summary>
    [SugarTable("erp_treasuries")]
    [SqlDict(
        dictType: "erp_treasury",
        dictName: "Treasury - الخزينة",
        valueColumn: "treasuryid",
        labelColumns: "treasuryname",
        RemarkColumn = "treasurynamear",
        OrderBy = "treasurycode ASC",
        Description = "Auto-generated from ErpTreasury table"
    )]
    public class ErpTreasury : SysBase
    {
        /// <summary>
        /// Treasury ID - معرف الخزينة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TreasuryId { get; set; }

        /// <summary>
        /// Treasury Code - كود الخزينة
        /// </summary>
        public string TreasuryCode { get; set; }

        /// <summary>
        /// Treasury Name - اسم الخزينة
        /// </summary>
        public string TreasuryName { get; set; }

        /// <summary>
        /// Treasury Name Arabic - اسم الخزينة بالعربي
        /// </summary>
        public string TreasuryNameAr { get; set; }

        /// <summary>
        /// Treasury Type - نوع الخزينة (cash/bank/pos)
        /// </summary>
        public string TreasuryType { get; set; } = "cash";

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Account ID - معرف الحساب المرتبط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccountId { get; set; }
 

        /// <summary>
        /// Responsible Employee ID - معرف الموظف المسؤول
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ResponsibleEmployeeId { get; set; }

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
        /// Is Default - افتراضي
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Allow Receipts - يسمح بالمقبوضات
        /// </summary>
        public bool AllowReceipts { get; set; } = true;

        /// <summary>
        /// Allow Payments - يسمح بالمدفوعات
        /// </summary>
        public bool AllowPayments { get; set; } = true;

        /// <summary>
        /// Minimum Balance - الحد الأدنى للرصيد
        /// </summary>
        public decimal MinimumBalance { get; set; }

        /// <summary>
        /// Maximum Balance - الحد الأقصى للرصيد
        /// </summary>
        public decimal MaximumBalance { get; set; }

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

