using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury Transfer - تحويل رصيد بين الخزائن
    /// </summary>
    [SugarTable("erp_treasury_transfers")]
    public class ErpTreasuryTransfer : SysBase
    {
        /// <summary>
        /// Transfer ID - معرف التحويل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransferId { get; set; }

        /// <summary>
        /// Transfer Number - رقم التحويل
        /// </summary>
        public string TransferNumber { get; set; }

        /// <summary>
        /// Transfer Date - تاريخ التحويل
        /// </summary>
        public DateTime TransferDate { get; set; }

        /// <summary>
        /// From Treasury ID - من الخزينة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long FromTreasuryId { get; set; }

        /// <summary>
        /// To Treasury ID - إلى الخزينة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ToTreasuryId { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status - الحالة (pending/approved/completed/cancelled)
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Approved By - وُوفق بواسطة
        /// </summary>
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Approval Date - تاريخ الموافقة
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Journal Entry ID - معرف القيد اليومي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? JournalEntryId { get; set; }
 

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

