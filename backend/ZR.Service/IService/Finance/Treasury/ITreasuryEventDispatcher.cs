using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using PaymentStatus = ZR.Model.Finance.PaymentHub.Enums.PaymentStatus;
using PaymentType = ZR.Model.Finance.PaymentHub.Enums.PaymentType;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Treasury Event Dispatcher Interface - واجهة موزع أحداث الخزينة
    /// Event-driven architecture for treasury operations (Phase 3.2)
    /// </summary>
    public interface ITreasuryEventDispatcher
    {
        #region Event Publishing

        /// <summary>
        /// Publish a treasury event - نشر حدث خزينة
        /// </summary>
        void Publish(TreasuryEvent treasuryEvent);

        /// <summary>
        /// Publish a treasury event asynchronously - نشر حدث خزينة بشكل غير متزامن
        /// </summary>
        Task PublishAsync(TreasuryEvent treasuryEvent);

        /// <summary>
        /// Publish multiple events atomically - نشر أحداث متعددة بشكل ذري
        /// </summary>
        Task PublishBatchAsync(IEnumerable<TreasuryEvent> events);

        #endregion

        #region Event Subscription

        /// <summary>
        /// Subscribe to treasury events - الاشتراك في أحداث الخزينة
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : TreasuryEvent;

        /// <summary>
        /// Subscribe to treasury events asynchronously - الاشتراك في أحداث الخزينة بشكل غير متزامن
        /// </summary>
        void SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : TreasuryEvent;

        /// <summary>
        /// Unsubscribe from treasury events - إلغاء الاشتراك من أحداث الخزينة
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : TreasuryEvent;

        #endregion

        #region Common Event Publishers

        /// <summary>
        /// Publish payment created event - نشر حدث إنشاء الدفعة
        /// </summary>
        void PublishPaymentCreated(Payment payment, string userId);

        /// <summary>
        /// Publish payment status changed event - نشر حدث تغيير حالة الدفعة
        /// </summary>
        void PublishPaymentStatusChanged(Payment payment, PaymentStatus oldStatus, PaymentStatus newStatus, string userId);

        /// <summary>
        /// Publish payment posted event - نشر حدث ترحيل الدفعة
        /// </summary>
        void PublishPaymentPosted(Payment payment, Guid journalId, string userId);

        /// <summary>
        /// Publish cheque status changed event - نشر حدث تغيير حالة الشيك
        /// </summary>
        void PublishChequeStatusChanged(Cheque cheque, ChequeStatus oldStatus, ChequeStatus newStatus, string userId);

        /// <summary>
        /// Publish cheque bounced event - نشر حدث ارتجاع الشيك
        /// </summary>
        void PublishChequeBounced(Cheque cheque, string reason, decimal? bankFees, string userId);

        /// <summary>
        /// Publish balance changed event - نشر حدث تغيير الرصيد
        /// </summary>
        void PublishBalanceChanged(Guid accountId, TreasuryEntityType accountType, decimal oldBalance, decimal newBalance, string reason, string userId);

        /// <summary>
        /// Publish reconciliation completed event - نشر حدث اكتمال التسوية
        /// </summary>
        void PublishReconciliationCompleted(BankReconciliation reconciliation, string userId);

        /// <summary>
        /// Publish allocation created event - نشر حدث إنشاء التخصيص
        /// </summary>
        void PublishAllocationCreated(PaymentAllocation allocation, string userId);

        #endregion
    }

    #region Event Classes

    /// <summary>
    /// Base treasury event - حدث الخزينة الأساسي
    /// </summary>
    public abstract class TreasuryEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; }
        public TreasuryEntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public string EntityReference { get; set; }
        public string EventType { get; set; }
        public long? CompanyId { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Payment created event - حدث إنشاء الدفعة
    /// </summary>
    public class PaymentCreatedEvent : TreasuryEvent
    {
        public Payment Payment { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentType PaymentType { get; set; }
    }

    /// <summary>
    /// Payment status changed event - حدث تغيير حالة الدفعة
    /// </summary>
    public class PaymentStatusChangedEvent : TreasuryEvent
    {
        public Payment Payment { get; set; }
        public PaymentStatus OldStatus { get; set; }
        public PaymentStatus NewStatus { get; set; }
    }

    /// <summary>
    /// Payment posted event - حدث ترحيل الدفعة
    /// </summary>
    public class PaymentPostedEvent : TreasuryEvent
    {
        public Payment Payment { get; set; }
        public Guid JournalId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }

    /// <summary>
    /// Cheque status changed event - حدث تغيير حالة الشيك
    /// </summary>
    public class ChequeStatusChangedEvent : TreasuryEvent
    {
        public Cheque Cheque { get; set; }
        public ChequeStatus OldStatus { get; set; }
        public ChequeStatus NewStatus { get; set; }
    }

    /// <summary>
    /// Cheque bounced event - حدث ارتجاع الشيك
    /// </summary>
    public class ChequeBouncedEvent : TreasuryEvent
    {
        public Cheque Cheque { get; set; }
        public string Reason { get; set; }
        public decimal? BankFees { get; set; }
        public int BounceCount { get; set; }
    }

    /// <summary>
    /// Balance changed event - حدث تغيير الرصيد
    /// </summary>
    public class BalanceChangedEvent : TreasuryEvent
    {
        public Guid AccountId { get; set; }
        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }
        public decimal ChangeAmount { get; set; }
        public string Currency { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// Reconciliation completed event - حدث اكتمال التسوية
    /// </summary>
    public class ReconciliationCompletedEvent : TreasuryEvent
    {
        public BankReconciliation Reconciliation { get; set; }
        public int MatchedTransactions { get; set; }
        public decimal Difference { get; set; }
    }

    /// <summary>
    /// Allocation created event - حدث إنشاء التخصيص
    /// </summary>
    public class AllocationCreatedEvent : TreasuryEvent
    {
        public PaymentAllocation Allocation { get; set; }
        public Guid PaymentId { get; set; }
        public Guid DocumentId { get; set; }
        public decimal AllocatedAmount { get; set; }
    }

    /// <summary>
    /// Approval required event - حدث طلب الموافقة
    /// </summary>
    public class ApprovalRequiredEvent : TreasuryEvent
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool RequiresDualApproval { get; set; }
        public List<long> ApproverRoleIds { get; set; } = new();
    }

    /// <summary>
    /// Low balance alert event - حدث تنبيه الرصيد المنخفض
    /// </summary>
    public class LowBalanceAlertEvent : TreasuryEvent
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal MinimumBalance { get; set; }
        public decimal Shortfall { get; set; }
    }

    #endregion
}
