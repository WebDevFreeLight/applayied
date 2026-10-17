namespace ZR.Model.Finance.Treasury.Enums
{
    /// <summary>
    /// Payment Type - نوع الدفع
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// Receipt - إيصال استلام
        /// </summary>
        Receipt = 0,

        /// <summary>
        /// Payment - دفع
        /// </summary>
        Payment = 1,

        /// <summary>
        /// Transfer - تحويل
        /// </summary>
        Transfer = 2,

        /// <summary>
        /// Intercompany Transfer - تحويل بين الشركات
        /// </summary>
        IntercompanyTransfer = 3
    }

    /// <summary>
    /// Payment Method - طريقة الدفع
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Cash - نقد
        /// </summary>
        Cash = 0,

        /// <summary>
        /// Bank Transfer - تحويل بنكي
        /// </summary>
        BankTransfer = 1,

        /// <summary>
        /// Cheque - شيك
        /// </summary>
        Cheque = 2,

        /// <summary>
        /// Card - بطاقة
        /// </summary>
        Card = 3,

        /// <summary>
        /// Other - أخرى
        /// </summary>
        Other = 4
    }

    /// <summary>
    /// Payment Status - حالة الدفع
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Draft - مسودة
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Submitted - مقدم
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Approved - معتمد
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Posted - مسجل
        /// </summary>
        Posted = 3,

        /// <summary>
        /// Cancelled - ملغي
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Reversed - معكوس
        /// </summary>
        Reversed = 5
    }

    /// <summary>
    /// Cheque Status - حالة الشيك (Full lifecycle - Aligned with global accounting standards)
    /// </summary>
    public enum ChequeStatus
    {
        /// <summary>
        /// Issued - صادر (Cheque created but not yet presented to bank)
        /// </summary>
        Issued = 0,

        /// <summary>
        /// Presented - مقدم (Cheque submitted to bank for clearing - merged UnderCollection + old Presented)
        /// </summary>
        Presented = 1,

        /// <summary>
        /// Cleared - مسدد (Cheque successfully cleared by bank)
        /// </summary>
        Cleared = 2,

        /// <summary>
        /// Bounced - مرتجع (Cheque returned by bank - insufficient funds, etc.)
        /// </summary>
        Bounced = 3,

        /// <summary>
        /// Cancelled - ملغي (Cheque cancelled before clearing)
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// Cheque Type - نوع الشيك
    /// </summary>
    public enum ChequeType
    {
        /// <summary>
        /// Outgoing - صادر
        /// </summary>
        Outgoing = 0,

        /// <summary>
        /// Incoming - وارد
        /// </summary>
        Incoming = 1
    }

    /// <summary>
    /// Reconciliation Status - حالة التسوية
    /// </summary>
    public enum ReconciliationStatus
    {
        /// <summary>
        /// In Progress - قيد التنفيذ
        /// </summary>
        InProgress = 0,

        /// <summary>
        /// Reconciled - مسوى
        /// </summary>
        Reconciled = 1,

        /// <summary>
        /// Pending Investigation - قيد التحقيق
        /// </summary>
        PendingInvestigation = 2
    }

    /// <summary>
    /// Account Type - نوع الحساب
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Bank - بنك
        /// </summary>
        Bank = 0,

        /// <summary>
        /// Cash Box - صندوق نقدي
        /// </summary>
        CashBox = 1,

        /// <summary>
        /// Petty Cash - صندوق صغير
        /// </summary>
        PettyCash = 2
    }

    /// <summary>
    /// Account Status - حالة الحساب
    /// </summary>
    public enum AccountStatus
    {
        /// <summary>
        /// Active - نشط
        /// </summary>
        Active = 0,

        /// <summary>
        /// Inactive - غير نشط
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Closed - مغلق
        /// </summary>
        Closed = 2
    }

    /// <summary>
    /// Bank Statement Import Source - مصدر استيراد كشف الحساب
    /// </summary>
    public enum BankStatementImportSource
    {
        /// <summary>
        /// Manual - يدوي
        /// </summary>
        Manual = 0,

        /// <summary>
        /// CSV - CSV
        /// </summary>
        CSV = 1,

        /// <summary>
        /// MT940 - MT940
        /// </summary>
        MT940 = 2,

        /// <summary>
        /// API - واجهة برمجية
        /// </summary>
        API = 3,

        /// <summary>
        /// OFX - Open Financial Exchange
        /// </summary>
        OFX = 4,

        /// <summary>
        /// QIF - Quicken Interchange Format
        /// </summary>
        QIF = 5
    }

    /// <summary>
    /// Treasury Audit Action Type - نوع إجراء التدقيق
    /// </summary>
    public enum TreasuryAuditAction
    {
        /// <summary>
        /// Create - إنشاء
        /// </summary>
        Create = 0,

        /// <summary>
        /// Update - تحديث
        /// </summary>
        Update = 1,

        /// <summary>
        /// Delete - حذف
        /// </summary>
        Delete = 2,

        /// <summary>
        /// Submit - تقديم
        /// </summary>
        Submit = 3,

        /// <summary>
        /// Approve - اعتماد
        /// </summary>
        Approve = 4,

        /// <summary>
        /// Reject - رفض
        /// </summary>
        Reject = 5,

        /// <summary>
        /// Post - تسجيل
        /// </summary>
        Post = 6,

        /// <summary>
        /// Reverse - عكس
        /// </summary>
        Reverse = 7,

        /// <summary>
        /// Balance Adjustment - تعديل الرصيد
        /// </summary>
        BalanceAdjustment = 8,

        /// <summary>
        /// Status Change - تغيير الحالة
        /// </summary>
        StatusChange = 9,

        /// <summary>
        /// Allocation - تخصيص
        /// </summary>
        Allocation = 10
    }

    /// <summary>
    /// Treasury Entity Type - نوع الكيان
    /// </summary>
    public enum TreasuryEntityType
    {
        BankAccount = 0,
        CashAccount = 1,
        Payment = 2,
        Cheque = 3,
        BankStatement = 4,
        BankReconciliation = 5,
        PaymentAllocation = 6,
        TreasuryConfig = 7
    }

    /// <summary>
    /// Reconciliation Line Status - حالة بند التسوية (requirement #51)
    /// </summary>
    public enum ReconciliationLineStatus
    {
        /// <summary>
        /// Unmatched - غير متطابق
        /// </summary>
        Unmatched = 0,

        /// <summary>
        /// Matched - متطابق
        /// </summary>
        Matched = 1,

        /// <summary>
        /// ManualReview - مراجعة يدوية (requirement #44)
        /// </summary>
        ManualReview = 2,

        /// <summary>
        /// Excluded - مستبعد
        /// </summary>
        Excluded = 3,

        /// <summary>
        /// PartialMatch - تطابق جزئي
        /// </summary>
        PartialMatch = 4,

        /// <summary>
        /// PendingApproval - قيد الاعتماد
        /// </summary>
        PendingApproval = 5
    }

    /// <summary>
    /// Match Type - نوع التطابق (requirement #52)
    /// </summary>
    public enum MatchType
    {
        /// <summary>
        /// Auto - تلقائي
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Manual - يدوي
        /// </summary>
        Manual = 1,

        /// <summary>
        /// Suggested - مقترح
        /// </summary>
        Suggested = 2,

        /// <summary>
        /// OneToOne - واحد لواحد
        /// </summary>
        OneToOne = 3,

        /// <summary>
        /// OneToMany - واحد للعديد
        /// </summary>
        OneToMany = 4,

        /// <summary>
        /// ManyToOne - العديد لواحد
        /// </summary>
        ManyToOne = 5
    }

    /// <summary>
    /// Treasury Error Code - رمز خطأ الخزينة (requirement #47)
    /// Structured error codes for API responses
    /// </summary>
    public enum TreasuryErrorCode
    {
        // Validation Errors (1000-1999)
        InvalidAmount = 1001,
        InvalidCurrency = 1002,
        InvalidDate = 1003,
        InvalidAccountId = 1004,
        InvalidPaymentType = 1005,
        MissingRequiredField = 1006,
        InvalidIBAN = 1007,
        InvalidSWIFT = 1008,
        InvalidChequeNumber = 1009,
        InvalidReference = 1010,

        // Business Rule Errors (2000-2999)
        InsufficientBalance = 2001,
        AccountClosed = 2002,
        AccountInactive = 2003,
        PaymentAlreadyPosted = 2004,
        PaymentAlreadyReversed = 2005,
        BalanceUpdateFailed = 2021,
        AvailableBalanceInsufficient = 2022,
        ChequeAlreadyCleared = 2006,
        ChequeAlreadyCancelled = 2007,
        ReconciliationAlreadyCompleted = 2008,
        ExceedsApprovalLimit = 2009,
        IntercompanyValidationFailed = 2010,
        CurrencyMismatch = 2011,
        DuplicateTransaction = 2012,
        NegativeBalanceNotAllowed = 2013,
        OverdraftLimitExceeded = 2014,
        ReconciliationToleranceExceeded = 2015,
        InvalidStatusTransition = 2016,
        HasRelatedTransactions = 2017,
        FiscalPeriodClosed = 2018,
        FutureDateNotAllowed = 2019,
        PastDateLocked = 2020,

        // GL Integration Errors (3000-3999)
        GLAccountNotConfigured = 3001,
        GLPostingFailed = 3002,
        GLReversalFailed = 3003,
        JournalNotFound = 3004,
        JournalAlreadyPosted = 3005,

        // Data Access Errors (4000-4999)
        RecordNotFound = 4001,
        ConcurrencyConflict = 4002,
        DatabaseError = 4003,
        DuplicateKey = 4004,

        // External Service Errors (5000-5999)
        BankAPIError = 5001,
        WebhookDeliveryFailed = 5002,
        ExternalValidationFailed = 5003,

        // System Errors (9000-9999)
        UnexpectedError = 9001,
        ConfigurationError = 9002,
        ServiceUnavailable = 9003
    }

    /// <summary>
    /// Bank Statement Import Format - تنسيق استيراد كشف الحساب (requirement #53)
    /// </summary>
    public enum BankStatementFormat
    {
        /// <summary>
        /// Generic CSV - CSV عام
        /// </summary>
        GenericCSV = 0,

        /// <summary>
        /// MT940 - SWIFT MT940
        /// </summary>
        MT940 = 1,

        /// <summary>
        /// CAMT053 - ISO 20022 camt.053
        /// </summary>
        CAMT053 = 2,

        /// <summary>
        /// OFX - Open Financial Exchange
        /// </summary>
        OFX = 3,

        /// <summary>
        /// QIF - Quicken Interchange Format
        /// </summary>
        QIF = 4,

        /// <summary>
        /// BAI2 - Bank Administration Institute
        /// </summary>
        BAI2 = 5,

        /// <summary>
        /// BankSpecific - Bank Specific Format (requirement #45)
        /// </summary>
        BankSpecific = 10
    }

    /// <summary>
    /// Webhook Event Type - نوع حدث الويب هوك (requirement #48)
    /// </summary>
    public enum TreasuryWebhookEvent
    {
        PaymentCreated = 0,
        PaymentApproved = 1,
        PaymentPosted = 2,
        PaymentReversed = 3,
        ChequeIssued = 10,
        ChequeCleared = 11,
        ChequeBounced = 12,
        ReconciliationStarted = 20,
        ReconciliationCompleted = 21,
        BalanceAlert = 30,
        OverdraftAlert = 31,
        ApprovalRequired = 40
    }
}






























