using System;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury Exception - استثناء الخزينة
    /// Structured exception with error codes for API responses (requirement #47)
    /// </summary>
    public class TreasuryException : Exception
    {
        /// <summary>
        /// Structured Error Code - رمز الخطأ المهيكل
        /// </summary>
        public TreasuryErrorCode ErrorCode { get; }

        /// <summary>
        /// Error Message in Arabic - رسالة الخطأ بالعربية
        /// </summary>
        public string MessageAr { get; }

        /// <summary>
        /// Error Message in English - رسالة الخطأ بالإنجليزية
        /// </summary>
        public string MessageEn { get; }

        /// <summary>
        /// Additional Context - سياق إضافي
        /// </summary>
        public object Context { get; }

        /// <summary>
        /// Entity ID related to the error - معرف الكيان المرتبط بالخطأ
        /// </summary>
        public Guid? EntityId { get; }

        /// <summary>
        /// Entity Type related to the error - نوع الكيان المرتبط بالخطأ
        /// </summary>
        public TreasuryEntityType? EntityType { get; }

        public TreasuryException(TreasuryErrorCode errorCode, string messageEn, string messageAr = null)
            : base(messageEn)
        {
            ErrorCode = errorCode;
            MessageEn = messageEn;
            MessageAr = messageAr ?? GetDefaultArabicMessage(errorCode);
        }

        public TreasuryException(TreasuryErrorCode errorCode, string messageEn, string messageAr, object context)
            : base(messageEn)
        {
            ErrorCode = errorCode;
            MessageEn = messageEn;
            MessageAr = messageAr ?? GetDefaultArabicMessage(errorCode);
            Context = context;
        }

        public TreasuryException(TreasuryErrorCode errorCode, string messageEn, string messageAr, Guid entityId, TreasuryEntityType entityType)
            : base(messageEn)
        {
            ErrorCode = errorCode;
            MessageEn = messageEn;
            MessageAr = messageAr ?? GetDefaultArabicMessage(errorCode);
            EntityId = entityId;
            EntityType = entityType;
        }

        /// <summary>
        /// Get default Arabic message for error code
        /// </summary>
        private static string GetDefaultArabicMessage(TreasuryErrorCode errorCode)
        {
            return errorCode switch
            {
                // Validation Errors
                TreasuryErrorCode.InvalidAmount => "المبلغ غير صالح",
                TreasuryErrorCode.InvalidCurrency => "العملة غير صالحة",
                TreasuryErrorCode.InvalidDate => "التاريخ غير صالح",
                TreasuryErrorCode.InvalidAccountId => "معرف الحساب غير صالح",
                TreasuryErrorCode.InvalidPaymentType => "نوع الدفعة غير صالح",
                TreasuryErrorCode.MissingRequiredField => "حقل مطلوب مفقود",
                TreasuryErrorCode.InvalidIBAN => "رقم IBAN غير صالح",
                TreasuryErrorCode.InvalidSWIFT => "رمز SWIFT غير صالح",
                TreasuryErrorCode.InvalidChequeNumber => "رقم الشيك غير صالح",
                TreasuryErrorCode.InvalidReference => "المرجع غير صالح",

                // Business Rule Errors
                TreasuryErrorCode.InsufficientBalance => "الرصيد غير كافٍ",
                TreasuryErrorCode.AccountClosed => "الحساب مغلق",
                TreasuryErrorCode.AccountInactive => "الحساب غير نشط",
                TreasuryErrorCode.PaymentAlreadyPosted => "الدفعة مسجلة بالفعل",
                TreasuryErrorCode.PaymentAlreadyReversed => "الدفعة معكوسة بالفعل",
                TreasuryErrorCode.ChequeAlreadyCleared => "الشيك مسدد بالفعل",
                TreasuryErrorCode.ChequeAlreadyCancelled => "الشيك ملغي بالفعل",
                TreasuryErrorCode.ReconciliationAlreadyCompleted => "التسوية مكتملة بالفعل",
                TreasuryErrorCode.ExceedsApprovalLimit => "يتجاوز حد الاعتماد",
                TreasuryErrorCode.IntercompanyValidationFailed => "فشل التحقق من التحويل بين الشركات",
                TreasuryErrorCode.CurrencyMismatch => "عدم تطابق العملة",
                TreasuryErrorCode.DuplicateTransaction => "معاملة مكررة",
                TreasuryErrorCode.NegativeBalanceNotAllowed => "الرصيد السالب غير مسموح",
                TreasuryErrorCode.OverdraftLimitExceeded => "تجاوز حد السحب على المكشوف",
                TreasuryErrorCode.ReconciliationToleranceExceeded => "تجاوز حد التسامح في التسوية",
                TreasuryErrorCode.InvalidStatusTransition => "انتقال حالة غير صالح",
                TreasuryErrorCode.HasRelatedTransactions => "يوجد معاملات مرتبطة",
                TreasuryErrorCode.FiscalPeriodClosed => "الفترة المالية مغلقة",
                TreasuryErrorCode.FutureDateNotAllowed => "التاريخ المستقبلي غير مسموح",
                TreasuryErrorCode.PastDateLocked => "التاريخ السابق مقفل",

                // GL Integration Errors
                TreasuryErrorCode.GLAccountNotConfigured => "حساب دفتر الأستاذ غير مهيأ",
                TreasuryErrorCode.GLPostingFailed => "فشل تسجيل القيد المحاسبي",
                TreasuryErrorCode.GLReversalFailed => "فشل عكس القيد المحاسبي",
                TreasuryErrorCode.JournalNotFound => "القيد المحاسبي غير موجود",
                TreasuryErrorCode.JournalAlreadyPosted => "القيد المحاسبي مسجل بالفعل",

                // Data Access Errors
                TreasuryErrorCode.RecordNotFound => "السجل غير موجود",
                TreasuryErrorCode.ConcurrencyConflict => "تعارض في التزامن",
                TreasuryErrorCode.DatabaseError => "خطأ في قاعدة البيانات",
                TreasuryErrorCode.DuplicateKey => "مفتاح مكرر",

                // External Service Errors
                TreasuryErrorCode.BankAPIError => "خطأ في واجهة البنك البرمجية",
                TreasuryErrorCode.WebhookDeliveryFailed => "فشل تسليم الويب هوك",
                TreasuryErrorCode.ExternalValidationFailed => "فشل التحقق الخارجي",

                // System Errors
                TreasuryErrorCode.UnexpectedError => "خطأ غير متوقع",
                TreasuryErrorCode.ConfigurationError => "خطأ في الإعدادات",
                TreasuryErrorCode.ServiceUnavailable => "الخدمة غير متاحة",

                _ => "حدث خطأ"
            };
        }

        /// <summary>
        /// Create a formatted error response object
        /// </summary>
        public TreasuryErrorResponse ToErrorResponse()
        {
            return new TreasuryErrorResponse
            {
                Code = (int)ErrorCode,
                ErrorCode = ErrorCode.ToString(),
                MessageEn = MessageEn,
                MessageAr = MessageAr,
                EntityId = EntityId,
                EntityType = EntityType?.ToString(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Treasury Error Response - استجابة خطأ الخزينة
    /// Structured error response for API (requirement #47)
    /// </summary>
    public class TreasuryErrorResponse
    {
        /// <summary>
        /// Numeric error code - رمز الخطأ الرقمي
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Error code name - اسم رمز الخطأ
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Error message in English
        /// </summary>
        public string MessageEn { get; set; }

        /// <summary>
        /// Error message in Arabic
        /// </summary>
        public string MessageAr { get; set; }

        /// <summary>
        /// Related Entity ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Related Entity Type
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Error timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Additional details
        /// </summary>
        public object Details { get; set; }
    }
}
