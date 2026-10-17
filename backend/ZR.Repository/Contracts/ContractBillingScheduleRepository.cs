using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Repository;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Billing Schedule Repository - مستودع جداول الفوترة
    /// Updated to follow Finance module pattern with DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractBillingScheduleRepository), ServiceLifetime = LifeTime.Transient)]
    public class ContractBillingScheduleRepository : BaseRepository<ErpContractBillingSchedule>, IContractBillingScheduleRepository
    {
        /// <summary>
        /// Get billing schedules by contract ID
        /// </summary>
        public List<ErpContractBillingScheduleListDto> GetByContractId(Guid contractId, BillingScheduleStatus? status = null)
        {
            var query = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId);

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return query
                .OrderBy(s => s.ScheduleNumber)
                .Select(s => new ErpContractBillingScheduleListDto
                {
                    ScheduleId = s.ScheduleId,
                    ScheduleNumber = s.ScheduleNumber,
                    DueDate = s.DueDate,
                    PaymentDueDate = s.PaymentDueDate,
                    PeriodStart = s.PeriodStart,
                    PeriodEnd = s.PeriodEnd,
                    BaseAmount = s.BaseAmount,
                    TotalAmount = s.TotalAmount,
                    Amount = s.TotalAmount,
                    Currency = s.Currency,
                    Status = s.Status,
                    InvoiceId = s.InvoiceId,
                    InvoiceNumber = s.InvoiceNumber,
                    Notes = s.Notes
                })
                .ToList();
        }

        /// <summary>
        /// Get billing schedule by ID
        /// </summary>
        public ErpContractBillingSchedule GetById(Guid scheduleId)
        {
            return Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ScheduleId == scheduleId)
                .First();
        }

        /// <summary>
        /// Bulk create schedules
        /// Also creates corresponding PaymentStatus extension records
        /// </summary>
        public int BulkCreate(List<ErpContractBillingSchedule> schedules)
        {
            var paymentStatuses = new List<ErpBillingPaymentStatus>();

            foreach (var schedule in schedules)
            {
                schedule.ScheduleId = Guid.NewGuid();
                schedule.Create_time = DateTime.Now;

                // Create payment status extension record
                paymentStatuses.Add(new ErpBillingPaymentStatus
                {
                    ScheduleId = schedule.ScheduleId,
                    PaidAmount = 0,
                    RemainingAmount = schedule.TotalAmount,
                    CollectionStatus = "Current",
                    IsOverdue = false,
                    Create_time = DateTime.Now
                });
            }

            var result = Context.Insertable(schedules).ExecuteCommand();

            // Insert payment status records
            if (paymentStatuses.Any())
            {
                Context.Insertable(paymentStatuses).ExecuteCommand();
            }

            return result;
        }

        /// <summary>
        /// Update schedule
        /// </summary>
        public int UpdateSchedule(ErpContractBillingSchedule schedule)
        {
            schedule.Update_time = DateTime.Now;
            return Context.Updateable(schedule)
                .IgnoreColumns(s => new { s.ScheduleId, s.ContractId, s.Create_by, s.Create_time })
                .Where(s => s.ScheduleId == schedule.ScheduleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete schedule
        /// </summary>
        public int Delete(Guid scheduleId)
        {
            return Context.Deleteable<ErpContractBillingSchedule>()
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete schedules by contract ID
        /// </summary>
        public int DeleteByContractId(Guid contractId)
        {
            return Context.Deleteable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Mark schedule as invoiced
        /// </summary>
        public int MarkAsInvoiced(Guid scheduleId, Guid invoiceId, string invoiceNumber)
        {
            return Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Invoiced)
                .SetColumns(s => s.InvoiceId == invoiceId)
                .SetColumns(s => s.InvoiceNumber == invoiceNumber)
                .SetColumns(s => s.InvoicedDate == DateTime.Now)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Mark schedule as paid
        /// Updates both schedule status and PaymentStatus extension record
        /// </summary>
        public int MarkAsPaid(Guid scheduleId, DateTime paidDate)
        {
            var schedule = GetById(scheduleId);
            if (schedule == null) return 0;

            // Update schedule status
            var result = Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Paid)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommand();

            // Update or create payment status extension
            var existingPaymentStatus = Context.Queryable<ErpBillingPaymentStatus>()
                .Where(p => p.ScheduleId == scheduleId)
                .First();

            if (existingPaymentStatus != null)
            {
                Context.Updateable<ErpBillingPaymentStatus>()
                    .SetColumns(p => p.PaidAmount == schedule.TotalAmount)
                    .SetColumns(p => p.RemainingAmount == 0)
                    .SetColumns(p => p.LastPaymentDate == paidDate)
                    .SetColumns(p => p.IsOverdue == false)
                    .SetColumns(p => p.CollectionStatus == "Paid")
                    .SetColumns(p => p.Update_time == DateTime.Now)
                    .Where(p => p.ScheduleId == scheduleId)
                    .ExecuteCommand();
            }
            else
            {
                Context.Insertable(new ErpBillingPaymentStatus
                {
                    ScheduleId = scheduleId,
                    PaidAmount = schedule.TotalAmount,
                    RemainingAmount = 0,
                    LastPaymentDate = paidDate,
                    IsOverdue = false,
                    CollectionStatus = "Paid",
                    Create_time = DateTime.Now
                }).ExecuteCommand();
            }

            return result;
        }

        /// <summary>
        /// Cancel schedule
        /// </summary>
        public int Cancel(Guid scheduleId, string cancelledBy)
        {
            return Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Cancelled)
                .SetColumns(s => s.CancelledDate == DateTime.Now)
                .SetColumns(s => s.CancelledBy == cancelledBy)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Skip schedule
        /// </summary>
        public int Skip(Guid scheduleId, string skippedBy)
        {
            return Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Skipped)
                .SetColumns(s => s.SkippedDate == DateTime.Now)
                .SetColumns(s => s.SkippedBy == skippedBy)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get due schedules
        /// </summary>
        public List<ErpContractBillingSchedule> GetDueSchedules(DateTime? asOfDate = null)
        {
            var date = asOfDate ?? DateTime.Today;
            return Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.Status == BillingScheduleStatus.Pending && s.DueDate <= date)
                .OrderBy(s => s.DueDate)
                .ToList();
        }

        /// <summary>
        /// Get upcoming schedules
        /// </summary>
        public List<ErpContractBillingScheduleListDto> GetUpcomingSchedules(Guid contractId, int count = 5)
        {
            return Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId && s.Status == BillingScheduleStatus.Pending)
                .OrderBy(s => s.DueDate)
                .Take(count)
                .Select(s => new ErpContractBillingScheduleListDto
                {
                    ScheduleId = s.ScheduleId,
                    ScheduleNumber = s.ScheduleNumber,
                    DueDate = s.DueDate,
                    PaymentDueDate = s.PaymentDueDate,
                    PeriodStart = s.PeriodStart,
                    PeriodEnd = s.PeriodEnd,
                    BaseAmount = s.BaseAmount,
                    TotalAmount = s.TotalAmount,
                    Amount = s.TotalAmount,
                    Currency = s.Currency,
                    Status = s.Status,
                    InvoiceId = s.InvoiceId,
                    InvoiceNumber = s.InvoiceNumber,
                    Notes = s.Notes
                })
                .ToList();
        }

        /// <summary>
        /// Cancel future schedules
        /// </summary>
        public int CancelFutureSchedules(Guid contractId, DateTime fromDate, string cancelledBy)
        {
            return Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Cancelled)
                .SetColumns(s => s.CancelledDate == DateTime.Now)
                .SetColumns(s => s.CancelledBy == cancelledBy)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ContractId == contractId && s.Status == BillingScheduleStatus.Pending && s.DueDate >= fromDate)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        public BillingScheduleStatisticsDto GetStatistics(Guid contractId)
        {
            var schedules = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ToList();

            return new BillingScheduleStatisticsDto
            {
                TotalCount = schedules.Count,
                PendingCount = schedules.Count(s => s.Status == BillingScheduleStatus.Pending),
                InvoicedCount = schedules.Count(s => s.Status == BillingScheduleStatus.Invoiced),
                PaidCount = schedules.Count(s => s.Status == BillingScheduleStatus.Paid),
                OverdueCount = schedules.Count(s => s.IsOverdue),
                DueCount = schedules.Count(s => s.Status == BillingScheduleStatus.Pending && s.DueDate <= DateTime.Today),
                TotalAmount = schedules.Sum(s => s.TotalAmount),
                PaidAmount = schedules.Sum(s => s.PaidAmount),
                PendingAmount = schedules.Where(s => s.Status == BillingScheduleStatus.Pending).Sum(s => s.TotalAmount)
            };
        }

        #region Async Methods

        /// <summary>
        /// Get billing schedules by contract ID async
        /// </summary>
        public async Task<List<ErpContractBillingScheduleListDto>> GetByContractIdAsync(Guid contractId, BillingScheduleStatus? status = null)
        {
            var query = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId);

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.ScheduleNumber)
                .Select(s => new ErpContractBillingScheduleListDto
                {
                    ScheduleId = s.ScheduleId,
                    ScheduleNumber = s.ScheduleNumber,
                    DueDate = s.DueDate,
                    PaymentDueDate = s.PaymentDueDate,
                    PeriodStart = s.PeriodStart,
                    PeriodEnd = s.PeriodEnd,
                    BaseAmount = s.BaseAmount,
                    TotalAmount = s.TotalAmount,
                    Amount = s.TotalAmount,
                    Currency = s.Currency,
                    Status = s.Status,
                    InvoiceId = s.InvoiceId,
                    InvoiceNumber = s.InvoiceNumber,
                    Notes = s.Notes
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get billing schedule by ID async
        /// </summary>
        public async Task<ErpContractBillingSchedule> GetByIdAsync(Guid scheduleId)
        {
            return await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ScheduleId == scheduleId)
                .FirstAsync();
        }

        /// <summary>
        /// Bulk create schedules async
        /// </summary>
        public async Task<int> BulkCreateAsync(List<ErpContractBillingSchedule> schedules)
        {
            var paymentStatuses = new List<ErpBillingPaymentStatus>();

            foreach (var schedule in schedules)
            {
                schedule.ScheduleId = Guid.NewGuid();
                schedule.Create_time = DateTime.Now;

                paymentStatuses.Add(new ErpBillingPaymentStatus
                {
                    ScheduleId = schedule.ScheduleId,
                    PaidAmount = 0,
                    RemainingAmount = schedule.TotalAmount,
                    CollectionStatus = "Current",
                    IsOverdue = false,
                    Create_time = DateTime.Now
                });
            }

            var result = await Context.Insertable(schedules).ExecuteCommandAsync();

            if (paymentStatuses.Any())
            {
                await Context.Insertable(paymentStatuses).ExecuteCommandAsync();
            }

            return result;
        }

        /// <summary>
        /// Update schedule async
        /// </summary>
        public async Task<int> UpdateScheduleAsync(ErpContractBillingSchedule schedule)
        {
            schedule.Update_time = DateTime.Now;
            return await Context.Updateable(schedule)
                .IgnoreColumns(s => new { s.ScheduleId, s.ContractId, s.Create_by, s.Create_time })
                .Where(s => s.ScheduleId == schedule.ScheduleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete schedule async
        /// </summary>
        public async Task<int> DeleteAsync(Guid scheduleId)
        {
            return await Context.Deleteable<ErpContractBillingSchedule>()
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete schedules by contract ID async
        /// </summary>
        public async Task<int> DeleteByContractIdAsync(Guid contractId)
        {
            return await Context.Deleteable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Mark schedule as invoiced async
        /// </summary>
        public async Task<int> MarkAsInvoicedAsync(Guid scheduleId, Guid invoiceId, string invoiceNumber)
        {
            return await Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Invoiced)
                .SetColumns(s => s.InvoiceId == invoiceId)
                .SetColumns(s => s.InvoiceNumber == invoiceNumber)
                .SetColumns(s => s.InvoicedDate == DateTime.Now)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Mark schedule as paid async
        /// </summary>
        public async Task<int> MarkAsPaidAsync(Guid scheduleId, DateTime paidDate)
        {
            var schedule = await GetByIdAsync(scheduleId);
            if (schedule == null) return 0;

            var result = await Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Paid)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommandAsync();

            var existingPaymentStatus = await Context.Queryable<ErpBillingPaymentStatus>()
                .Where(p => p.ScheduleId == scheduleId)
                .FirstAsync();

            if (existingPaymentStatus != null)
            {
                await Context.Updateable<ErpBillingPaymentStatus>()
                    .SetColumns(p => p.PaidAmount == schedule.TotalAmount)
                    .SetColumns(p => p.RemainingAmount == 0)
                    .SetColumns(p => p.LastPaymentDate == paidDate)
                    .SetColumns(p => p.IsOverdue == false)
                    .SetColumns(p => p.CollectionStatus == "Paid")
                    .SetColumns(p => p.Update_time == DateTime.Now)
                    .Where(p => p.ScheduleId == scheduleId)
                    .ExecuteCommandAsync();
            }
            else
            {
                await Context.Insertable(new ErpBillingPaymentStatus
                {
                    ScheduleId = scheduleId,
                    PaidAmount = schedule.TotalAmount,
                    RemainingAmount = 0,
                    LastPaymentDate = paidDate,
                    IsOverdue = false,
                    CollectionStatus = "Paid",
                    Create_time = DateTime.Now
                }).ExecuteCommandAsync();
            }

            return result;
        }

        /// <summary>
        /// Cancel schedule async
        /// </summary>
        public async Task<int> CancelAsync(Guid scheduleId, string cancelledBy)
        {
            return await Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Cancelled)
                .SetColumns(s => s.CancelledDate == DateTime.Now)
                .SetColumns(s => s.CancelledBy == cancelledBy)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Skip schedule async
        /// </summary>
        public async Task<int> SkipAsync(Guid scheduleId, string skippedBy)
        {
            return await Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Skipped)
                .SetColumns(s => s.SkippedDate == DateTime.Now)
                .SetColumns(s => s.SkippedBy == skippedBy)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ScheduleId == scheduleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Get due schedules async
        /// </summary>
        public async Task<List<ErpContractBillingSchedule>> GetDueSchedulesAsync(DateTime? asOfDate = null)
        {
            var date = asOfDate ?? DateTime.Today;
            return await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.Status == BillingScheduleStatus.Pending && s.DueDate <= date)
                .OrderBy(s => s.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get upcoming schedules async
        /// </summary>
        public async Task<List<ErpContractBillingScheduleListDto>> GetUpcomingSchedulesAsync(Guid contractId, int count = 5)
        {
            return await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId && s.Status == BillingScheduleStatus.Pending)
                .OrderBy(s => s.DueDate)
                .Take(count)
                .Select(s => new ErpContractBillingScheduleListDto
                {
                    ScheduleId = s.ScheduleId,
                    ScheduleNumber = s.ScheduleNumber,
                    DueDate = s.DueDate,
                    PaymentDueDate = s.PaymentDueDate,
                    PeriodStart = s.PeriodStart,
                    PeriodEnd = s.PeriodEnd,
                    BaseAmount = s.BaseAmount,
                    TotalAmount = s.TotalAmount,
                    Amount = s.TotalAmount,
                    Currency = s.Currency,
                    Status = s.Status,
                    InvoiceId = s.InvoiceId,
                    InvoiceNumber = s.InvoiceNumber,
                    Notes = s.Notes
                })
                .ToListAsync();
        }

        /// <summary>
        /// Cancel future schedules async
        /// </summary>
        public async Task<int> CancelFutureSchedulesAsync(Guid contractId, DateTime fromDate, string cancelledBy)
        {
            return await Context.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => s.Status == BillingScheduleStatus.Cancelled)
                .SetColumns(s => s.CancelledDate == DateTime.Now)
                .SetColumns(s => s.CancelledBy == cancelledBy)
                .SetColumns(s => s.Update_time == DateTime.Now)
                .Where(s => s.ContractId == contractId && s.Status == BillingScheduleStatus.Pending && s.DueDate >= fromDate)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Get statistics async
        /// </summary>
        public async Task<BillingScheduleStatisticsDto> GetStatisticsAsync(Guid contractId)
        {
            var schedules = await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ToListAsync();

            return new BillingScheduleStatisticsDto
            {
                TotalCount = schedules.Count,
                PendingCount = schedules.Count(s => s.Status == BillingScheduleStatus.Pending),
                InvoicedCount = schedules.Count(s => s.Status == BillingScheduleStatus.Invoiced),
                PaidCount = schedules.Count(s => s.Status == BillingScheduleStatus.Paid),
                OverdueCount = schedules.Count(s => s.IsOverdue),
                DueCount = schedules.Count(s => s.Status == BillingScheduleStatus.Pending && s.DueDate <= DateTime.Today),
                TotalAmount = schedules.Sum(s => s.TotalAmount),
                PaidAmount = schedules.Sum(s => s.PaidAmount),
                PendingAmount = schedules.Where(s => s.Status == BillingScheduleStatus.Pending).Sum(s => s.TotalAmount)
            };
        }

        #endregion
    }
}
