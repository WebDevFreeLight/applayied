namespace ZR.Model.Finance.PaymentHub.Enums
{
    /// <summary>
    /// Payment Type - نوع الدفعة
    /// </summary>
    public enum PaymentType
    {
        /// <summary>Customer Receipt - إيصال من عميل</summary>
        CustomerReceipt = 0,
        /// <summary>Vendor Payment - دفعة لمورد</summary>
        VendorPayment = 1,
        /// <summary>Internal Transfer - تحويل داخلي</summary>
        InternalTransfer = 2,
        /// <summary>Intercompany Transfer - تحويل بين الشركات</summary>
        IntercompanyTransfer = 3,
        /// <summary>Refund to Customer - استرداد للعميل</summary>
        CustomerRefund = 4,
        /// <summary>Refund from Vendor - استرداد من مورد</summary>
        VendorRefund = 5
    }

    /// <summary>
    /// Payment Method - طريقة الدفع
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>Cash - نقدي</summary>
        Cash = 0,
        /// <summary>Bank Transfer - تحويل بنكي</summary>
        BankTransfer = 1,
        /// <summary>Cheque - شيك</summary>
        Cheque = 2,
        /// <summary>Credit Card - بطاقة ائتمان</summary>
        CreditCard = 3,
        /// <summary>Debit Card - بطاقة سحب</summary>
        DebitCard = 4,
        /// <summary>Digital Wallet - محفظة رقمية</summary>
        DigitalWallet = 5,
        /// <summary>Other - أخرى</summary>
        Other = 99
    }

    /// <summary>
    /// Payment Status - حالة الدفعة
    /// Immutable state machine: Draft → Approved → Posted → (Reversed)
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>Draft - مسودة (editable)</summary>
        Draft = 0,
        /// <summary>Approved - معتمدة (ready for posting)</summary>
        Approved = 1,
        /// <summary>Posted - مرحّلة (GL journal created, balances updated)</summary>
        Posted = 2,
        /// <summary>Reversed - معكوسة (fully reversed)</summary>
        Reversed = 3,
        /// <summary>Cancelled - ملغاة (before posting only)</summary>
        Cancelled = 4
    }

    /// <summary>
    /// Allocation Status - حالة التخصيص
    /// </summary>
    public enum AllocationStatus
    {
        /// <summary>Active - نشط</summary>
        Active = 0,
        /// <summary>Reversed - معكوس</summary>
        Reversed = 1
    }

    /// <summary>
    /// Document Type for Allocation - نوع المستند للتخصيص
    /// </summary>
    public enum AllocatableDocumentType
    {
        /// <summary>AR Invoice - فاتورة عميل</summary>
        ARInvoice = 0,
        /// <summary>AP Invoice - فاتورة مورد</summary>
        APInvoice = 1,
        /// <summary>Credit Note - إشعار دائن</summary>
        CreditNote = 2,
        /// <summary>Debit Note - إشعار مدين</summary>
        DebitNote = 3
    }

    /// <summary>
    /// Payment Event Type - نوع حدث الدفعة (append-only audit)
    /// </summary>
    public enum PaymentEventType
    {
        /// <summary>Created - تم الإنشاء</summary>
        Created = 0,
        /// <summary>Updated - تم التحديث</summary>
        Updated = 1,
        /// <summary>Approved - تم الاعتماد</summary>
        Approved = 2,
        /// <summary>Posted - تم الترحيل</summary>
        Posted = 3,
        /// <summary>Allocated - تم التخصيص</summary>
        Allocated = 4,
        /// <summary>AllocationReversed - تم عكس التخصيص</summary>
        AllocationReversed = 5,
        /// <summary>Reversed - تم العكس</summary>
        Reversed = 6,
        /// <summary>Cancelled - تم الإلغاء</summary>
        Cancelled = 7,
        /// <summary>BalanceUpdated - تم تحديث الرصيد</summary>
        BalanceUpdated = 8,
        /// <summary>GLJournalCreated - تم إنشاء قيد محاسبي</summary>
        GLJournalCreated = 9,
        /// <summary>GLJournalReversed - تم عكس القيد المحاسبي</summary>
        GLJournalReversed = 10
    }

    /// <summary>
    /// Counterparty Type - نوع الطرف المقابل
    /// </summary>
    public enum CounterpartyType
    {
        /// <summary>Customer - عميل</summary>
        Customer = 0,
        /// <summary>Vendor - مورد</summary>
        Vendor = 1,
        /// <summary>Employee - موظف</summary>
        Employee = 2,
        /// <summary>Other - أخرى</summary>
        Other = 99
    }

    /// <summary>
    /// Account Type for Payment - نوع الحساب للدفعة
    /// </summary>
    public enum PaymentAccountType
    {
        /// <summary>Bank Account - حساب بنكي</summary>
        BankAccount = 0,
        /// <summary>Cash Account - حساب نقدي</summary>
        CashAccount = 1
    }
}
