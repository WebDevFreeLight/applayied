using System;

namespace ZR.Model.Contracts.Enums
{
    /// <summary>
    /// Contract Type - نوع العقد
    /// Defines the nature/category of the contract
    /// </summary>
    public enum ContractType
    {
        /// <summary>
        /// Rental Contract - عقد إيجار
        /// For property/equipment rental agreements
        /// </summary>
        Rental = 1,

        /// <summary>
        /// Service Contract - عقد خدمات
        /// For ongoing service agreements (maintenance, consulting, etc.)
        /// </summary>
        Service = 2,

        /// <summary>
        /// Subscription Contract - عقد اشتراك
        /// For recurring subscription services (software, memberships)
        /// </summary>
        Subscription = 3,

        /// <summary>
        /// Lease Contract - عقد تأجير تمويلي
        /// For financial leasing arrangements
        /// </summary>
        Lease = 4,

        /// <summary>
        /// Maintenance Contract - عقد صيانة
        /// For equipment/property maintenance agreements
        /// </summary>
        Maintenance = 5,

        /// <summary>
        /// Consulting Contract - عقد استشارات
        /// For professional consulting services
        /// </summary>
        Consulting = 6,

        /// <summary>
        /// License Contract - عقد ترخيص
        /// For software/IP licensing agreements
        /// </summary>
        License = 7,

        /// <summary>
        /// Other - أخرى
        /// For contracts not fitting other categories
        /// </summary>
        Other = 99
    }

    /// <summary>
    /// Contract Status - حالة العقد
    /// Tracks the lifecycle state of a contract
    /// </summary>
    public enum ContractStatus
    {
        /// <summary>
        /// Draft - مسودة
        /// Contract is being prepared, not yet active
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Pending Approval - في انتظار الموافقة
        /// Contract submitted for approval
        /// </summary>
        PendingApproval = 1,

        /// <summary>
        /// Active - نشط
        /// Contract is currently in effect
        /// </summary>
        Active = 2,

        /// <summary>
        /// Suspended - معلق
        /// Contract temporarily suspended (payment issues, disputes)
        /// </summary>
        Suspended = 3,

        /// <summary>
        /// Expired - منتهي الصلاحية
        /// Contract has reached its end date without renewal
        /// </summary>
        Expired = 4,

        /// <summary>
        /// Terminated - منهي
        /// Contract was terminated before its natural end date
        /// </summary>
        Terminated = 5,

        /// <summary>
        /// Renewed - مجدد
        /// Contract has been renewed (original contract status)
        /// </summary>
        Renewed = 6,

        /// <summary>
        /// Cancelled - ملغي
        /// Contract was cancelled before becoming active
        /// </summary>
        Cancelled = 7
    }

    /// <summary>
    /// Renewal Type - نوع التجديد
    /// Defines how the contract renews
    /// </summary>
    public enum RenewalType
    {
        /// <summary>
        /// Manual - يدوي
        /// Requires manual action to renew
        /// </summary>
        Manual = 0,

        /// <summary>
        /// Monthly - شهري
        /// Auto-renews every month
        /// </summary>
        Monthly = 1,

        /// <summary>
        /// Quarterly - ربع سنوي
        /// Auto-renews every 3 months
        /// </summary>
        Quarterly = 2,

        /// <summary>
        /// Semi-Annual - نصف سنوي
        /// Auto-renews every 6 months
        /// </summary>
        SemiAnnual = 3,

        /// <summary>
        /// Yearly - سنوي
        /// Auto-renews every year
        /// </summary>
        Yearly = 4
    }

    /// <summary>
    /// Billing Frequency - تكرار الفوترة
    /// Defines how often invoices are generated
    /// </summary>
    public enum BillingFrequency
    {
        /// <summary>
        /// One-Time - مرة واحدة
        /// Single billing at contract start or end
        /// </summary>
        OneTime = 0,

        /// <summary>
        /// Weekly - أسبوعي
        /// Billed every week
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// Bi-Weekly - كل أسبوعين
        /// Billed every two weeks
        /// </summary>
        BiWeekly = 2,

        /// <summary>
        /// Monthly - شهري
        /// Billed every month
        /// </summary>
        Monthly = 3,

        /// <summary>
        /// Quarterly - ربع سنوي
        /// Billed every 3 months
        /// </summary>
        Quarterly = 4,

        /// <summary>
        /// Semi-Annual - نصف سنوي
        /// Billed every 6 months
        /// </summary>
        SemiAnnual = 5,

        /// <summary>
        /// Yearly - سنوي
        /// Billed once per year
        /// </summary>
        Yearly = 6,

        /// <summary>
        /// Custom - مخصص
        /// Custom billing schedule defined in pricing rules
        /// </summary>
        Custom = 99
    }

    /// <summary>
    /// Pricing Rule Type - نوع قاعدة التسعير
    /// Defines how contract pricing is calculated
    /// </summary>
    public enum PricingRuleType
    {
        /// <summary>
        /// Fixed - ثابت
        /// Fixed amount per billing period
        /// </summary>
        Fixed = 1,

        /// <summary>
        /// Variable - متغير
        /// Variable based on usage or other factors
        /// </summary>
        Variable = 2,

        /// <summary>
        /// Seasonal - موسمي
        /// Different pricing for different seasons/periods
        /// </summary>
        Seasonal = 3,

        /// <summary>
        /// Area Based - على أساس المساحة
        /// Pricing calculated based on unit area
        /// </summary>
        AreaBased = 4,

        /// <summary>
        /// Percentage Discount - خصم نسبي
        /// Percentage reduction from base price
        /// </summary>
        PercentageDiscount = 5,

        /// <summary>
        /// Fixed Discount - خصم ثابت
        /// Fixed amount reduction from base price
        /// </summary>
        FixedDiscount = 6,

        /// <summary>
        /// Advance Payment Discount - خصم الدفع المقدم
        /// Discount for paying in advance
        /// </summary>
        AdvancePaymentDiscount = 7,

        /// <summary>
        /// Tiered - متدرج
        /// Different rates based on volume/usage tiers
        /// </summary>
        Tiered = 8,

        /// <summary>
        /// Escalation - تصاعدي
        /// Price increases over time (annual escalation)
        /// </summary>
        Escalation = 9
    }

    /// <summary>
    /// Billing Schedule Status - حالة جدول الفوترة
    /// Tracks the status of each billing schedule entry
    /// </summary>
    public enum BillingScheduleStatus
    {
        /// <summary>
        /// Pending - معلق
        /// Not yet due for invoicing
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Due - مستحق
        /// Ready to be invoiced
        /// </summary>
        Due = 1,

        /// <summary>
        /// Invoiced - تم إصدار فاتورة
        /// Invoice has been generated
        /// </summary>
        Invoiced = 2,

        /// <summary>
        /// Paid - مدفوع
        /// Invoice has been paid
        /// </summary>
        Paid = 3,

        /// <summary>
        /// Cancelled - ملغي
        /// Schedule entry was cancelled
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Skipped - تم تخطيه
        /// Schedule entry was skipped (e.g., contract suspended)
        /// </summary>
        Skipped = 5
    }

    /// <summary>
    /// Amendment Type - نوع التعديل
    /// Defines the type of contract amendment
    /// </summary>
    public enum AmendmentType
    {
        /// <summary>
        /// Price Change - تغيير السعر
        /// Modification to contract pricing
        /// </summary>
        PriceChange = 1,

        /// <summary>
        /// Duration Extension - تمديد المدة
        /// Extending the contract end date
        /// </summary>
        DurationExtension = 2,

        /// <summary>
        /// Duration Reduction - تقليص المدة
        /// Shortening the contract end date
        /// </summary>
        DurationReduction = 3,

        /// <summary>
        /// Unit Addition - إضافة وحدة
        /// Adding new units to the contract
        /// </summary>
        UnitAddition = 4,

        /// <summary>
        /// Unit Removal - إزالة وحدة
        /// Removing units from the contract
        /// </summary>
        UnitRemoval = 5,

        /// <summary>
        /// Unit Change - تغيير الوحدة
        /// Changing unit details (area, type)
        /// </summary>
        UnitChange = 6,

        /// <summary>
        /// Discount Addition - إضافة خصم
        /// Adding a discount to the contract
        /// </summary>
        DiscountAddition = 7,

        /// <summary>
        /// Discount Removal - إزالة خصم
        /// Removing a discount from the contract
        /// </summary>
        DiscountRemoval = 8,

        /// <summary>
        /// Terms Change - تغيير الشروط
        /// Changing payment terms or other conditions
        /// </summary>
        TermsChange = 9,

        /// <summary>
        /// Billing Frequency Change - تغيير تكرار الفوترة
        /// Changing how often billing occurs
        /// </summary>
        BillingFrequencyChange = 10,

        /// <summary>
        /// Other - أخرى
        /// Other types of amendments
        /// </summary>
        Other = 99
    }

    /// <summary>
    /// Amendment Status - حالة التعديل
    /// Tracks the approval status of amendments
    /// </summary>
    public enum AmendmentStatus
    {
        /// <summary>
        /// Draft - مسودة
        /// Amendment being prepared
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Pending Approval - في انتظار الموافقة
        /// Submitted for approval
        /// </summary>
        PendingApproval = 1,

        /// <summary>
        /// Approved - موافق عليه
        /// Amendment approved and applied
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Rejected - مرفوض
        /// Amendment was rejected
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Cancelled - ملغي
        /// Amendment was cancelled
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// Termination Reason - سبب الإنهاء
    /// Defines why a contract was terminated
    /// </summary>
    public enum TerminationReason
    {
        /// <summary>
        /// Mutual Agreement - اتفاق متبادل
        /// Both parties agreed to terminate
        /// </summary>
        MutualAgreement = 1,

        /// <summary>
        /// Customer Request - طلب العميل
        /// Customer requested termination
        /// </summary>
        CustomerRequest = 2,

        /// <summary>
        /// Non-Payment - عدم الدفع
        /// Terminated due to payment default
        /// </summary>
        NonPayment = 3,

        /// <summary>
        /// Breach of Contract - خرق العقد
        /// Contract terms were violated
        /// </summary>
        BreachOfContract = 4,

        /// <summary>
        /// Force Majeure - قوة قاهرة
        /// Terminated due to unforeseen circumstances
        /// </summary>
        ForceMajeure = 5,

        /// <summary>
        /// Business Closure - إغلاق العمل
        /// Business ceased operations
        /// </summary>
        BusinessClosure = 6,

        /// <summary>
        /// Relocation - انتقال
        /// Customer/tenant relocating
        /// </summary>
        Relocation = 7,

        /// <summary>
        /// Other - أخرى
        /// Other termination reasons
        /// </summary>
        Other = 99
    }

    /// <summary>
    /// Unit Type - نوع الوحدة
    /// Defines the type of unit in a contract
    /// </summary>
    public enum ContractUnitType
    {
        /// <summary>
        /// Office - مكتب
        /// Office space
        /// </summary>
        Office = 1,

        /// <summary>
        /// Retail Shop - محل تجاري
        /// Retail/commercial shop
        /// </summary>
        RetailShop = 2,

        /// <summary>
        /// Warehouse - مستودع
        /// Storage/warehouse space
        /// </summary>
        Warehouse = 3,

        /// <summary>
        /// Apartment - شقة
        /// Residential apartment
        /// </summary>
        Apartment = 4,

        /// <summary>
        /// Villa - فيلا
        /// Residential villa
        /// </summary>
        Villa = 5,

        /// <summary>
        /// Parking - موقف سيارات
        /// Parking space
        /// </summary>
        Parking = 6,

        /// <summary>
        /// Land - أرض
        /// Land plot
        /// </summary>
        Land = 7,

        /// <summary>
        /// Equipment - معدات
        /// Equipment rental
        /// </summary>
        Equipment = 8,

        /// <summary>
        /// Vehicle - مركبة
        /// Vehicle rental
        /// </summary>
        Vehicle = 9,

        /// <summary>
        /// Service - خدمة
        /// Service-based unit (no physical space)
        /// </summary>
        Service = 10,

        /// <summary>
        /// Other - أخرى
        /// Other unit types
        /// </summary>
        Other = 99
    }

    /// <summary>
    /// Pricing Type - نوع التسعير
    /// Defines how the unit/contract is priced
    /// </summary>
    public enum PricingType
    {
        /// <summary>
        /// Fixed Rent - إيجار ثابت
        /// Fixed amount per billing period
        /// </summary>
        Fixed = 1,

        /// <summary>
        /// Revenue Share - مشاركة الإيرادات
        /// Percentage of reported revenue
        /// </summary>
        RevenueShare = 2,

        /// <summary>
        /// Hybrid - مختلط
        /// Combination of fixed rent and revenue share (future use)
        /// </summary>
        Hybrid = 3
    }

    /// <summary>
    /// Settlement Frequency - تكرار التسوية
    /// How often revenue share settlements are calculated
    /// </summary>
    public enum SettlementFrequency
    {
        /// <summary>
        /// Monthly - شهري
        /// Revenue settlement every month
        /// </summary>
        Monthly = 1,

        /// <summary>
        /// Quarterly - ربع سنوي
        /// Revenue settlement every quarter
        /// </summary>
        Quarterly = 2
    }

    /// <summary>
    /// Revenue Report Status - حالة تقرير الإيرادات
    /// Tracks the lifecycle of a revenue report entry
    /// </summary>
    public enum RevenueReportStatus
    {
        /// <summary>
        /// Draft - مسودة
        /// Report created but not finalized
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Submitted - مُقدَّم
        /// Report submitted for billing
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Invoiced - تم إصدار فاتورة
        /// Invoice generated from this report
        /// </summary>
        Invoiced = 2
    }
}
