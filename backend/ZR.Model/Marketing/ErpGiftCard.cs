using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing
{
    /// <summary>
    /// Gift Card - بطاقة هدية
    /// </summary>
    [SugarTable("erp_gift_cards")]
    [SqlDict(
        dictType: "erp_gift_card",
        dictName: "Gift Card - بطاقة هدية",
        valueColumn: "giftcardid",
        labelColumns: "cardcode",
        RemarkColumn = "notes",
        OrderBy = "createtime DESC",
        Description = "Auto-generated from ErpGiftCard table"
    )]
    public class ErpGiftCard : SysBase
    {
        /// <summary>
        /// Gift Card ID - معرف بطاقة الهدية
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GiftCardId { get; set; }

        /// <summary>
        /// Card Code - كود البطاقة
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// Card Number - رقم البطاقة
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Initial Value - القيمة الأولية
        /// </summary>
        public decimal InitialValue { get; set; }

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Purchaser Name - اسم المشتري
        /// </summary>
        public string PurchaserName { get; set; }

        /// <summary>
        /// Purchaser Phone - هاتف المشتري
        /// </summary>
        public string PurchaserPhone { get; set; }

        /// <summary>
        /// Purchaser Email - بريد المشتري
        /// </summary>
        public string PurchaserEmail { get; set; }

        /// <summary>
        /// Recipient Name - اسم المستلم
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Recipient Phone - هاتف المستلم
        /// </summary>
        public string RecipientPhone { get; set; }

        /// <summary>
        /// Recipient Email - بريد المستلم
        /// </summary>
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Message - الرسالة
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Purchase Date - تاريخ الشراء
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Activation Date - تاريخ التفعيل
        /// </summary>
        public DateTime? ActivationDate { get; set; }

        /// <summary>
        /// Expiry Date - تاريخ الانتهاء
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Status - الحالة (pending/active/used/expired/cancelled)
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Is Redeemed - مستخدمة
        /// </summary>
        public bool IsRedeemed { get; set; }

        /// <summary>
        /// Redeemed By Member ID - استخدمت بواسطة العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RedeemedByMemberId { get; set; }

        /// <summary>
        /// Redeemed Date - تاريخ الاستخدام
        /// </summary>
        public DateTime? RedeemedDate { get; set; }

        /// <summary>
        /// Invoice ID - معرف فاتورة الشراء
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }
         
        /// <summary>
        /// Design Template - قالب التصميم
        /// </summary>
        public string DesignTemplate { get; set; }

        /// <summary>
        /// QR Code - رمز QR
        /// </summary>
        public string QrCode { get; set; }

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

