using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Settings
{
    /// <summary>
    /// Message Settings - إعدادات الرسائل
    /// </summary>
    [SugarTable("erp_message_settings")]
    [SqlDict(
        dictType: "erp_message_setting",
        dictName: "Message Setting - إعداد الرسائل",
        valueColumn: "settingid",
        labelColumns: "settingname",
        RemarkColumn = "description",
        OrderBy = "settingname ASC",
        Description = "Auto-generated from ErpMessageSettings table"
    )]
    public class ErpMessageSettings : SysBase
    {
        /// <summary>
        /// Setting ID - معرف الإعداد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SettingId { get; set; }

        /// <summary>
        /// Setting Code - كود الإعداد
        /// </summary>
        public string SettingCode { get; set; }

        /// <summary>
        /// Setting Name - اسم الإعداد
        /// </summary>
        public string SettingName { get; set; }

        /// <summary>
        /// Setting Name Arabic - اسم الإعداد بالعربي
        /// </summary>
        public string SettingNameAr { get; set; }

        /// <summary>
        /// Event Type - نوع الحدث (subscription_expiry/booking_reminder/payment_due/welcome/etc)
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// SMS Enabled - الرسائل النصية مفعلة
        /// </summary>
        public bool SmsEnabled { get; set; }

        /// <summary>
        /// SMS Template - قالب الرسالة النصية
        /// </summary>
        public string SmsTemplate { get; set; }

        /// <summary>
        /// SMS Template Arabic - قالب الرسالة النصية بالعربي
        /// </summary>
        public string SmsTemplateAr { get; set; }

        /// <summary>
        /// Email Enabled - البريد الإلكتروني مفعل
        /// </summary>
        public bool EmailEnabled { get; set; }

        /// <summary>
        /// Email Subject - عنوان البريد
        /// </summary>
        public string EmailSubject { get; set; }

        /// <summary>
        /// Email Subject Arabic - عنوان البريد بالعربي
        /// </summary>
        public string EmailSubjectAr { get; set; }

        /// <summary>
        /// Email Template - قالب البريد
        /// </summary>
        public string EmailTemplate { get; set; }

        /// <summary>
        /// Email Template Arabic - قالب البريد بالعربي
        /// </summary>
        public string EmailTemplateAr { get; set; }

        /// <summary>
        /// Push Enabled - الإشعارات مفعلة
        /// </summary>
        public bool PushEnabled { get; set; }

        /// <summary>
        /// Push Title - عنوان الإشعار
        /// </summary>
        public string PushTitle { get; set; }

        /// <summary>
        /// Push Message - رسالة الإشعار
        /// </summary>
        public string PushMessage { get; set; }

        /// <summary>
        /// WhatsApp Enabled - واتساب مفعل
        /// </summary>
        public bool WhatsAppEnabled { get; set; }

        /// <summary>
        /// WhatsApp Template - قالب واتساب
        /// </summary>
        public string WhatsAppTemplate { get; set; }

        /// <summary>
        /// Send Before Days - أرسل قبل (أيام)
        /// </summary>
        public int? SendBeforeDays { get; set; }

        /// <summary>
        /// Send Time - وقت الإرسال
        /// </summary>
        public TimeSpan? SendTime { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

