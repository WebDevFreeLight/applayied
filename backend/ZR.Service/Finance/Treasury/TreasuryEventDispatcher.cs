using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using NLog;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Service.IService.Finance.Treasury;
using PaymentStatus = ZR.Model.Finance.PaymentHub.Enums.PaymentStatus;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury Event Dispatcher - موزع أحداث الخزينة
    /// Event-driven architecture for treasury operations (Phase 3.2)
    /// </summary>
    [AppService(ServiceType = typeof(ITreasuryEventDispatcher), ServiceLifetime = LifeTime.Singleton)]
    public class TreasuryEventDispatcher : ITreasuryEventDispatcher
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
        private readonly ConcurrentDictionary<Type, List<Delegate>> _asyncHandlers = new();
        private readonly ITreasuryAuditService _auditService;

        public TreasuryEventDispatcher(ITreasuryAuditService auditService)
        {
            _auditService = auditService;
            RegisterDefaultHandlers();
        }

        /// <summary>
        /// Register default event handlers - تسجيل معالجات الأحداث الافتراضية
        /// </summary>
        private void RegisterDefaultHandlers()
        {
            // Log all events by default
            Subscribe<TreasuryEvent>(LogEvent);
        }

        /// <summary>
        /// Default event logging handler - معالج تسجيل الأحداث الافتراضي
        /// </summary>
        private void LogEvent(TreasuryEvent e)
        {
            _logger.Info($"Treasury Event: {e.EventType} | Entity: {e.EntityType} | ID: {e.EntityId} | User: {e.UserId}");
        }

        #region Event Publishing

        public void Publish(TreasuryEvent treasuryEvent)
        {
            if (treasuryEvent == null)
            {
                _logger.Warn("Attempted to publish null treasury event");
                return;
            }

            var eventType = treasuryEvent.GetType();

            // Get handlers for this specific event type
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers.ToList())
                {
                    try
                    {
                        handler.DynamicInvoke(treasuryEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Error in event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }

            // Also check for base type handlers (TreasuryEvent)
            if (eventType != typeof(TreasuryEvent) && _handlers.TryGetValue(typeof(TreasuryEvent), out var baseHandlers))
            {
                foreach (var handler in baseHandlers.ToList())
                {
                    try
                    {
                        handler.DynamicInvoke(treasuryEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Error in base event handler: {ex.Message}");
                    }
                }
            }
        }

        public async Task PublishAsync(TreasuryEvent treasuryEvent)
        {
            if (treasuryEvent == null)
            {
                _logger.Warn("Attempted to publish null treasury event");
                return;
            }

            var eventType = treasuryEvent.GetType();
            var tasks = new List<Task>();

            // Execute sync handlers
            Publish(treasuryEvent);

            // Get async handlers for this specific event type
            if (_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers.ToList())
                {
                    try
                    {
                        var task = (Task)handler.DynamicInvoke(treasuryEvent);
                        if (task != null)
                        {
                            tasks.Add(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Error in async event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }

            // Also check for base type async handlers
            if (eventType != typeof(TreasuryEvent) && _asyncHandlers.TryGetValue(typeof(TreasuryEvent), out var baseHandlers))
            {
                foreach (var handler in baseHandlers.ToList())
                {
                    try
                    {
                        var task = (Task)handler.DynamicInvoke(treasuryEvent);
                        if (task != null)
                        {
                            tasks.Add(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Error in base async event handler: {ex.Message}");
                    }
                }
            }

            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
        }

        public async Task PublishBatchAsync(IEnumerable<TreasuryEvent> events)
        {
            var tasks = events.Select(e => PublishAsync(e));
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Event Subscription

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : TreasuryEvent
        {
            var eventType = typeof(TEvent);
            _handlers.AddOrUpdate(
                eventType,
                new List<Delegate> { handler },
                (_, existing) =>
                {
                    existing.Add(handler);
                    return existing;
                });

            _logger.Debug($"Subscribed handler for {eventType.Name}");
        }

        public void SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : TreasuryEvent
        {
            var eventType = typeof(TEvent);
            _asyncHandlers.AddOrUpdate(
                eventType,
                new List<Delegate> { handler },
                (_, existing) =>
                {
                    existing.Add(handler);
                    return existing;
                });

            _logger.Debug($"Subscribed async handler for {eventType.Name}");
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : TreasuryEvent
        {
            var eventType = typeof(TEvent);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                _logger.Debug($"Unsubscribed handler for {eventType.Name}");
            }
        }

        #endregion

        #region Common Event Publishers

        public void PublishPaymentCreated(Payment payment, string userId)
        {
            var eventObj = new PaymentCreatedEvent
            {
                EntityType = TreasuryEntityType.Payment,
                EntityId = payment.Id,
                EntityReference = payment.PaymentNumber,
                EventType = "PaymentCreated",
                UserId = userId,
                Payment = payment,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentType = payment.PaymentType,
                CompanyId = payment.CompanyID
            };

            Publish(eventObj);
        }

        public void PublishPaymentStatusChanged(Payment payment, PaymentStatus oldStatus, PaymentStatus newStatus, string userId)
        {
            var eventObj = new PaymentStatusChangedEvent
            {
                EntityType = TreasuryEntityType.Payment,
                EntityId = payment.Id,
                EntityReference = payment.PaymentNumber,
                EventType = "PaymentStatusChanged",
                UserId = userId,
                Payment = payment,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                CompanyId = payment.CompanyID
            };

            eventObj.Metadata["OldStatus"] = oldStatus.ToString();
            eventObj.Metadata["NewStatus"] = newStatus.ToString();

            Publish(eventObj);

            // Log status change in audit
            _auditService.LogStatusChange(
                payment.Id,
                TreasuryEntityType.Payment,
                payment.PaymentNumber,
                oldStatus.ToString(),
                newStatus.ToString(),
                userId);
        }

        public void PublishPaymentPosted(Payment payment, Guid journalId, string userId)
        {
            var eventObj = new PaymentPostedEvent
            {
                EntityType = TreasuryEntityType.Payment,
                EntityId = payment.Id,
                EntityReference = payment.PaymentNumber,
                EventType = "PaymentPosted",
                UserId = userId,
                Payment = payment,
                JournalId = journalId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CompanyId = payment.CompanyID
            };

            eventObj.Metadata["JournalId"] = journalId;

            Publish(eventObj);
        }

        public void PublishChequeStatusChanged(Cheque cheque, ChequeStatus oldStatus, ChequeStatus newStatus, string userId)
        {
            var eventObj = new ChequeStatusChangedEvent
            {
                EntityType = TreasuryEntityType.Cheque,
                EntityId = cheque.Id,
                EntityReference = cheque.ChequeNumber,
                EventType = "ChequeStatusChanged",
                UserId = userId,
                Cheque = cheque,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                CompanyId = cheque.CompanyID
            };

            eventObj.Metadata["OldStatus"] = oldStatus.ToString();
            eventObj.Metadata["NewStatus"] = newStatus.ToString();
            eventObj.Metadata["Amount"] = cheque.Amount;

            Publish(eventObj);

            // Log status change in audit
            _auditService.LogStatusChange(
                cheque.Id,
                TreasuryEntityType.Cheque,
                cheque.ChequeNumber,
                oldStatus.ToString(),
                newStatus.ToString(),
                userId);
        }

        public void PublishChequeBounced(Cheque cheque, string reason, decimal? bankFees, string userId)
        {
            var eventObj = new ChequeBouncedEvent
            {
                EntityType = TreasuryEntityType.Cheque,
                EntityId = cheque.Id,
                EntityReference = cheque.ChequeNumber,
                EventType = "ChequeBounced",
                UserId = userId,
                Cheque = cheque,
                Reason = reason,
                BankFees = bankFees,
                BounceCount = cheque.BounceCount,
                CompanyId = cheque.CompanyID
            };

            eventObj.Metadata["BounceReason"] = reason;
            if (bankFees.HasValue)
            {
                eventObj.Metadata["BankFees"] = bankFees.Value;
            }

            Publish(eventObj);
        }

        public void PublishBalanceChanged(Guid accountId, TreasuryEntityType accountType, decimal oldBalance, decimal newBalance, string reason, string userId)
        {
            var eventObj = new BalanceChangedEvent
            {
                EntityType = accountType,
                EntityId = accountId,
                EventType = "BalanceChanged",
                UserId = userId,
                AccountId = accountId,
                OldBalance = oldBalance,
                NewBalance = newBalance,
                ChangeAmount = newBalance - oldBalance,
                Reason = reason
            };

            eventObj.Metadata["OldBalance"] = oldBalance;
            eventObj.Metadata["NewBalance"] = newBalance;
            eventObj.Metadata["ChangeAmount"] = newBalance - oldBalance;

            Publish(eventObj);
        }

        public void PublishReconciliationCompleted(BankReconciliation reconciliation, string userId)
        {
            var eventObj = new ReconciliationCompletedEvent
            {
                EntityType = TreasuryEntityType.BankReconciliation,
                EntityId = reconciliation.Id,
                EntityReference = reconciliation.ReconciliationNumber,
                EventType = "ReconciliationCompleted",
                UserId = userId,
                Reconciliation = reconciliation,
                CompanyId = reconciliation.CompanyID
            };

            Publish(eventObj);
        }

        public void PublishAllocationCreated(PaymentAllocation allocation, string userId)
        {
            var eventObj = new AllocationCreatedEvent
            {
                EntityType = TreasuryEntityType.PaymentAllocation,
                EntityId = allocation.Id,
                EventType = "AllocationCreated",
                UserId = userId,
                Allocation = allocation,
                PaymentId = allocation.PaymentId,
                DocumentId = allocation.DocumentId,
                AllocatedAmount = allocation.AllocatedAmount,
                CompanyId = allocation.CompanyID
            };

            Publish(eventObj);
        }

        #endregion

        #region Alert Publishers

        /// <summary>
        /// Publish low balance alert - نشر تنبيه الرصيد المنخفض
        /// </summary>
        public void PublishLowBalanceAlert(Guid accountId, string accountNumber, decimal currentBalance, decimal minimumBalance, string userId)
        {
            var eventObj = new LowBalanceAlertEvent
            {
                EntityType = TreasuryEntityType.BankAccount,
                EntityId = accountId,
                EntityReference = accountNumber,
                EventType = "LowBalanceAlert",
                UserId = userId,
                AccountId = accountId,
                AccountNumber = accountNumber,
                CurrentBalance = currentBalance,
                MinimumBalance = minimumBalance,
                Shortfall = minimumBalance - currentBalance
            };

            Publish(eventObj);
        }

        /// <summary>
        /// Publish approval required alert - نشر تنبيه الموافقة المطلوبة
        /// </summary>
        public void PublishApprovalRequired(Guid entityId, TreasuryEntityType entityType, string entityReference, decimal amount, string currency, bool requiresDualApproval, string userId)
        {
            var eventObj = new ApprovalRequiredEvent
            {
                EntityType = entityType,
                EntityId = entityId,
                EntityReference = entityReference,
                EventType = "ApprovalRequired",
                UserId = userId,
                Amount = amount,
                Currency = currency,
                RequiresDualApproval = requiresDualApproval
            };

            Publish(eventObj);
        }

        #endregion
    }
}
