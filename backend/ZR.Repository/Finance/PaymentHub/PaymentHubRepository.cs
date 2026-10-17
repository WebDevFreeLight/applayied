using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.PaymentHub.Enums;
using ZR.Repository;

namespace ZR.Repository.Finance.PaymentHub
{
    /// <summary>
    /// Payment Hub Repository - مستودع مركز الدفعات
    /// THE single source of truth for payment data access
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PaymentHubRepository : BaseRepository<Payment>
    {
        #region Payment Queries

        /// <summary>
        /// Get payment by ID with allocations - الحصول على دفعة بالمعرف مع التخصيصات
        /// </summary>
        public Payment GetPaymentWithAllocations(Guid paymentId)
        {
            var payment = Context.Queryable<Payment>()
                .Where(p => p.Id == paymentId)
                .First();

            return payment;
        }

        /// <summary>
        /// Get payment by payment number - الحصول على دفعة برقم الدفعة
        /// </summary>
        public Payment GetByPaymentNumber(string paymentNumber)
        {
            return Context.Queryable<Payment>()
                .Where(p => p.PaymentNumber == paymentNumber)
                .First();
        }

        /// <summary>
        /// Get payment by idempotency key - الحصول على دفعة بمفتاح منع التكرار
        /// </summary>
        public Payment GetByIdempotencyKey(string idempotencyKey)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return null;

            return Context.Queryable<Payment>()
                .Where(p => p.IdempotencyKey == idempotencyKey)
                .First();
        }

        /// <summary>
        /// Get payments by counterparty - الحصول على دفعات الطرف المقابل
        /// </summary>
        public List<Payment> GetByCounterparty(Guid counterpartyId, PaymentStatus? status = null)
        {
            var query = Context.Queryable<Payment>()
                .Where(p => p.CounterpartyId == counterpartyId);

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            return query.OrderByDescending(p => p.PaymentDate).ToList();
        }

        /// <summary>
        /// Get payments with unallocated balance - الحصول على الدفعات غير المخصصة
        /// </summary>
        public List<Payment> GetPaymentsWithUnallocatedBalance(Guid? counterpartyId = null, PaymentType? paymentType = null)
        {
            var query = Context.Queryable<Payment>()
                .Where(p => p.Status == PaymentStatus.Posted)
                .Where(p => !p.IsReversed)
                .Where(p => p.UnallocatedAmount > 0.0001m);

            if (counterpartyId.HasValue)
            {
                query = query.Where(p => p.CounterpartyId == counterpartyId.Value);
            }

            if (paymentType.HasValue)
            {
                query = query.Where(p => p.PaymentType == paymentType.Value);
            }

            return query.OrderBy(p => p.PaymentDate).ToList();
        }

        /// <summary>
        /// Get payments by account - الحصول على دفعات الحساب
        /// </summary>
        public List<Payment> GetByAccount(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = Context.Queryable<Payment>()
                .Where(p => p.SourceAccountId == accountId || p.DestinationAccountId == accountId);

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= toDate.Value);
            }

            return query.OrderByDescending(p => p.PaymentDate).ToList();
        }

        /// <summary>
        /// Get payments in fiscal period - الحصول على دفعات الفترة المالية
        /// </summary>
        public List<Payment> GetByFiscalPeriod(int fiscalYear, int fiscalPeriod, PaymentStatus? status = null)
        {
            var query = Context.Queryable<Payment>()
                .Where(p => p.FiscalYear == fiscalYear && p.FiscalPeriod == fiscalPeriod);

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            return query.OrderBy(p => p.PaymentDate).ToList();
        }

        /// <summary>
        /// Check if idempotency key exists - التحقق من وجود مفتاح منع التكرار
        /// </summary>
        public bool IdempotencyKeyExists(string idempotencyKey)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return false;

            return Context.Queryable<Payment>()
                .Where(p => p.IdempotencyKey == idempotencyKey)
                .Any();
        }

        /// <summary>
        /// Update payment status atomically - تحديث حالة الدفعة ذرياً
        /// </summary>
        public int UpdateStatus(Guid paymentId, PaymentStatus newStatus, string userId, int expectedRowVersion)
        {
            return Context.Updateable<Payment>()
                .SetColumns(p => new Payment
                {
                    Status = newStatus,
                    Update_time = DateTime.Now,
                    Update_by = userId,
                    RowVersion = expectedRowVersion + 1
                })
                .Where(p => p.Id == paymentId)
                .Where(p => p.RowVersion == expectedRowVersion)
                .ExecuteCommand();
        }

        /// <summary>
        /// Update allocated amounts - تحديث المبالغ المخصصة
        /// </summary>
        public int UpdateAllocatedAmounts(Guid paymentId, decimal allocatedAmount, decimal unallocatedAmount, string userId)
        {
            return Context.Updateable<Payment>()
                .SetColumns(p => new Payment
                {
                    AllocatedAmount = allocatedAmount,
                    UnallocatedAmount = unallocatedAmount,
                    Update_time = DateTime.Now,
                    Update_by = userId
                })
                .Where(p => p.Id == paymentId)
                .ExecuteCommand();
        }

        #endregion
    }

    /// <summary>
    /// Payment Allocation Repository - مستودع تخصيصات الدفعات
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PaymentAllocationRepository : BaseRepository<PaymentAllocation>
    {
        #region Allocation Queries

        /// <summary>
        /// Get allocations by payment ID - الحصول على تخصيصات الدفعة
        /// </summary>
        public List<PaymentAllocation> GetByPaymentId(Guid paymentId, bool includeReversed = false)
        {
            var query = Context.Queryable<PaymentAllocation>()
                .Where(a => a.PaymentId == paymentId);

            if (!includeReversed)
            {
                query = query.Where(a => !a.IsReversed);
            }

            return query.OrderBy(a => a.AllocationDate).ToList();
        }

        /// <summary>
        /// Get allocations by document ID - الحصول على تخصيصات المستند
        /// </summary>
        public List<PaymentAllocation> GetByDocumentId(Guid documentId, bool includeReversed = false)
        {
            var query = Context.Queryable<PaymentAllocation>()
                .Where(a => a.DocumentId == documentId);

            if (!includeReversed)
            {
                query = query.Where(a => !a.IsReversed);
            }

            return query.OrderBy(a => a.AllocationDate).ToList();
        }

        /// <summary>
        /// Get total allocated amount for payment - المبلغ الإجمالي المخصص للدفعة
        /// </summary>
        public decimal GetTotalAllocatedForPayment(Guid paymentId)
        {
            return Context.Queryable<PaymentAllocation>()
                .Where(a => a.PaymentId == paymentId)
                .Where(a => !a.IsReversed)
                .Sum(a => a.AllocatedAmount);
        }

        /// <summary>
        /// Get total allocated amount for document - المبلغ الإجمالي المخصص للمستند
        /// </summary>
        public decimal GetTotalAllocatedForDocument(Guid documentId)
        {
            return Context.Queryable<PaymentAllocation>()
                .Where(a => a.DocumentId == documentId)
                .Where(a => !a.IsReversed)
                .Sum(a => a.AllocatedAmountDocCurrency);
        }

        /// <summary>
        /// Reverse allocation - عكس التخصيص
        /// </summary>
        public int ReverseAllocation(Guid allocationId, string reason, string userId)
        {
            return Context.Updateable<PaymentAllocation>()
                .SetColumns(a => new PaymentAllocation
                {
                    Status = AllocationStatus.Reversed,
                    IsReversed = true,
                    ReversalDate = DateTime.Now,
                    ReversalReason = reason,
                    ReversedBy = userId,
                    Update_time = DateTime.Now,
                    Update_by = userId
                })
                .Where(a => a.Id == allocationId)
                .ExecuteCommand();
        }

        #endregion
    }

    /// <summary>
    /// Payment Event Repository - مستودع أحداث الدفعات
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PaymentEventRepository : BaseRepository<PaymentEvent>
    {
        #region Event Queries

        /// <summary>
        /// Get events by payment ID - الحصول على أحداث الدفعة
        /// </summary>
        public List<PaymentEvent> GetByPaymentId(Guid paymentId)
        {
            return Context.Queryable<PaymentEvent>()
                .Where(e => e.PaymentId == paymentId)
                .OrderBy(e => e.EventTimestamp)
                .ToList();
        }

        /// <summary>
        /// Get next sequence number for payment - الرقم التسلسلي التالي للدفعة
        /// </summary>
        public int GetNextSequenceNumber(Guid paymentId)
        {
            var maxSequence = Context.Queryable<PaymentEvent>()
                .Where(e => e.PaymentId == paymentId)
                .Max(e => e.SequenceNumber);

            return maxSequence + 1;
        }

        /// <summary>
        /// Append event (insert only, never update) - إضافة حدث
        /// </summary>
        public void AppendEvent(PaymentEvent evt)
        {
            evt.SequenceNumber = GetNextSequenceNumber(evt.PaymentId);
            Context.Insertable(evt).ExecuteCommand();
        }

        /// <summary>
        /// Get events by type - الحصول على أحداث حسب النوع
        /// </summary>
        public List<PaymentEvent> GetByEventType(PaymentEventType eventType, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = Context.Queryable<PaymentEvent>()
                .Where(e => e.EventType == eventType);

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.EventTimestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.EventTimestamp <= toDate.Value);
            }

            return query.OrderByDescending(e => e.EventTimestamp).ToList();
        }

        #endregion
    }
}
