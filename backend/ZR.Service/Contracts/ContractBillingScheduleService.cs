using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Repository.Contracts;
using ZR.Service.Contracts.IService;
using ZR.ServiceCore;

namespace ZR.Service.Contracts
{
    /// <summary>
    /// Contract Billing Schedule Service - خدمة جداول الفوترة
    /// Business logic layer for contract billing schedule operations
    /// Updated to follow Finance module pattern with BaseService and DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractBillingScheduleService), ServiceLifetime = LifeTime.Transient)]
    public class ContractBillingScheduleService : BaseService<ErpContractBillingSchedule>, IContractBillingScheduleService
    {
        private readonly IContractBillingScheduleRepository _billingScheduleRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IContractPricingRuleRepository _pricingRuleRepository;

        public ContractBillingScheduleService(
            IContractBillingScheduleRepository billingScheduleRepository,
            IContractRepository contractRepository,
            IContractPricingRuleRepository pricingRuleRepository)
        {
            _billingScheduleRepository = billingScheduleRepository;
            _contractRepository = contractRepository;
            _pricingRuleRepository = pricingRuleRepository;
        }

        /// <summary>
        /// Get billing schedules by contract ID
        /// </summary>
        public List<ErpContractBillingScheduleListDto> GetByContractId(Guid contractId, BillingScheduleStatus? status = null)
        {
            return _billingScheduleRepository.GetByContractId(contractId, status);
        }

        /// <summary>
        /// Get billing schedules by contract ID async
        /// </summary>
        public async Task<List<ErpContractBillingScheduleListDto>> GetByContractIdAsync(Guid contractId, BillingScheduleStatus? status = null)
        {
            return await _billingScheduleRepository.GetByContractIdAsync(contractId, status);
        }

        /// <summary>
        /// Get billing schedule by ID
        /// </summary>
        public ErpContractBillingSchedule GetById(Guid scheduleId)
        {
            return _billingScheduleRepository.GetById(scheduleId);
        }

        /// <summary>
        /// Get billing schedule by ID async
        /// </summary>
        public async Task<ErpContractBillingSchedule> GetByIdAsync(Guid scheduleId)
        {
            return await _billingScheduleRepository.GetByIdAsync(scheduleId);
        }

        /// <summary>
        /// Generate billing schedules for contract
        /// Uses extension tables for financial and billing config data
        /// </summary>
        public int GenerateBillingSchedule(GenerateBillingScheduleDto dto, string createdBy)
        {
            var contract = _contractRepository.GetEntityById(dto.ContractId);
            if (contract == null)
            {
                throw new InvalidOperationException("Contract not found / العقد غير موجود");
            }

            if (!contract.EndDate.HasValue)
            {
                throw new InvalidOperationException("Cannot generate billing schedule for a contract without end date / لا يمكن إنشاء جدول فوترة لعقد بدون تاريخ انتهاء");
            }

            // Check for existing schedules
            var existingSchedules = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == dto.ContractId)
                .ToList();

            if (existingSchedules.Any())
            {
                if (dto.RegeneratePending == true)
                {
                    // Delete only pending/due schedules (not invoiced or paid)
                    var schedulesToDelete = existingSchedules
                        .Where(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due)
                        .Select(s => s.ScheduleId)
                        .ToList();

                    if (schedulesToDelete.Any())
                    {
                        Context.Deleteable<ErpContractBillingSchedule>()
                            .Where(s => schedulesToDelete.Contains(s.ScheduleId))
                            .ExecuteCommand();
                    }
                }
                else
                {
                    // Count pending schedules
                    var pendingCount = existingSchedules.Count(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due);
                    if (pendingCount > 0)
                    {
                        throw new InvalidOperationException($"Billing schedules already exist for this contract ({pendingCount} pending). Set 'Regenerate Pending' to replace them. / جداول الفوترة موجودة بالفعل لهذا العقد ({pendingCount} معلقة). قم بتحديد 'إعادة الإنشاء' لاستبدالها.");
                    }
                }
            }

            // Load extension data using Context from BaseService
            var financial = Context.Queryable<ErpContractFinancial>().First(f => f.ContractId == dto.ContractId);
            var billingConfig = Context.Queryable<ErpContractBillingConfig>().First(b => b.ContractId == dto.ContractId);

            // Use contract dates if not provided in DTO
            var startDate = dto.StartDate ?? contract.StartDate;
            var endDate = dto.EndDate ?? contract.EndDate.Value;

            var billingFrequency = billingConfig?.BillingFrequency ?? BillingFrequency.Monthly;
            var totalSeasonPrice = financial?.BaseAmount ?? 0; // Full season price
            var paymentDueDays = financial?.PaymentDueDays ?? 30;
            var currency = financial?.Currency ?? "SAR";

            // Already collected from invoiced/paid schedules (for regenerate scenario)
            var keptSchedules = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == dto.ContractId
                    && (s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid))
                .ToList();
            decimal alreadyCollected = keptSchedules.Sum(s => s.BaseAmount);
            int nextScheduleNum = keptSchedules.Any() ? keptSchedules.Max(s => s.ScheduleNumber) + 1 : 1;

            // Step 1: Count billing periods
            var periods = new List<(DateTime Start, DateTime End, DateTime DueDate)>();
            var currentDate = startDate;

            while (currentDate < endDate)
            {
                DateTime nextDate = CalculateNextDate(currentDate, billingFrequency);
                if (nextDate > endDate) nextDate = endDate;

                var billingDay = billingConfig?.BillingDay ?? currentDate.Day;
                var dueDate = new DateTime(currentDate.Year, currentDate.Month,
                    Math.Min(billingDay, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
                if (dueDate < currentDate) dueDate = dueDate.AddMonths(1);

                periods.Add((currentDate, nextDate.AddDays(-1), dueDate));
                currentDate = nextDate;
                if (billingFrequency == BillingFrequency.OneTime) break;
            }

            if (!periods.Any()) return 0;

            // Step 2: Divide remaining season price equally across periods
            decimal remaining = totalSeasonPrice - alreadyCollected;
            if (remaining < 0) remaining = 0;

            int periodCount = periods.Count;
            decimal perInstallment = Math.Round(remaining / periodCount, 4);
            decimal distributed = perInstallment * (periodCount - 1);
            decimal lastInstallment = remaining - distributed;

            // Step 3: Create schedule entries
            var schedules = new List<ErpContractBillingSchedule>();
            for (int i = 0; i < periodCount; i++)
            {
                var period = periods[i];
                decimal installmentAmount = (i == periodCount - 1) ? lastInstallment : perInstallment;

                // Apply pricing rules if any
                var adjustedAmount = _pricingRuleRepository.CalculateAmount(dto.ContractId, installmentAmount, period.DueDate);

                schedules.Add(new ErpContractBillingSchedule
                {
                    ContractId = dto.ContractId,
                    ScheduleNumber = nextScheduleNum + i,
                    DueDate = period.DueDate,
                    PaymentDueDate = period.DueDate.AddDays(paymentDueDays),
                    PeriodStart = period.Start,
                    PeriodEnd = period.End,
                    BaseAmount = installmentAmount,
                    Subtotal = adjustedAmount,
                    TotalAmount = adjustedAmount,
                    Currency = currency,
                    Status = BillingScheduleStatus.Pending,
                    Create_by = createdBy
                });
            }

            return _billingScheduleRepository.BulkCreate(schedules);
        }

        /// <summary>
        /// Generate billing schedules for contract async
        /// </summary>
        public async Task<int> GenerateBillingScheduleAsync(GenerateBillingScheduleDto dto, string createdBy)
        {
            var contract = await _contractRepository.GetEntityByIdAsync(dto.ContractId);
            if (contract == null)
            {
                throw new InvalidOperationException("Contract not found / العقد غير موجود");
            }

            if (!contract.EndDate.HasValue)
            {
                throw new InvalidOperationException("Cannot generate billing schedule for a contract without end date / لا يمكن إنشاء جدول فوترة لعقد بدون تاريخ انتهاء");
            }

            // Check for existing schedules
            var existingSchedules = await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == dto.ContractId)
                .ToListAsync();

            if (existingSchedules.Any())
            {
                if (dto.RegeneratePending == true)
                {
                    // Delete only pending/due schedules (not invoiced or paid)
                    var schedulesToDelete = existingSchedules
                        .Where(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due)
                        .Select(s => s.ScheduleId)
                        .ToList();

                    if (schedulesToDelete.Any())
                    {
                        await Context.Deleteable<ErpContractBillingSchedule>()
                            .Where(s => schedulesToDelete.Contains(s.ScheduleId))
                            .ExecuteCommandAsync();
                    }
                }
                else
                {
                    // Count pending schedules
                    var pendingCount = existingSchedules.Count(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due);
                    if (pendingCount > 0)
                    {
                        throw new InvalidOperationException($"Billing schedules already exist for this contract ({pendingCount} pending). Set 'Regenerate Pending' to replace them. / جداول الفوترة موجودة بالفعل لهذا العقد ({pendingCount} معلقة). قم بتحديد 'إعادة الإنشاء' لاستبدالها.");
                    }
                }
            }

            // Load extension data using Context from BaseService
            var financial = await Context.Queryable<ErpContractFinancial>().FirstAsync(f => f.ContractId == dto.ContractId);
            var billingConfig = await Context.Queryable<ErpContractBillingConfig>().FirstAsync(b => b.ContractId == dto.ContractId);

            // Use contract dates if not provided in DTO
            var startDate = dto.StartDate ?? contract.StartDate;
            var endDate = dto.EndDate ?? contract.EndDate.Value;

            var billingFrequency = billingConfig?.BillingFrequency ?? BillingFrequency.Monthly;
            var totalSeasonPrice = financial?.BaseAmount ?? 0; // Full season price
            var paymentDueDays = financial?.PaymentDueDays ?? 30;
            var currency = financial?.Currency ?? "SAR";

            // Already collected from invoiced/paid schedules (for regenerate scenario)
            var keptSchedules = await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == dto.ContractId
                    && (s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid))
                .ToListAsync();
            decimal alreadyCollected = keptSchedules.Sum(s => s.BaseAmount);
            int nextScheduleNum = keptSchedules.Any() ? keptSchedules.Max(s => s.ScheduleNumber) + 1 : 1;

            // Step 1: Count billing periods
            var periods = new List<(DateTime Start, DateTime End, DateTime DueDate)>();
            var currentDate = startDate;

            while (currentDate < endDate)
            {
                DateTime nextDate = CalculateNextDate(currentDate, billingFrequency);
                if (nextDate > endDate) nextDate = endDate;

                var billingDay = billingConfig?.BillingDay ?? currentDate.Day;
                var dueDate = new DateTime(currentDate.Year, currentDate.Month,
                    Math.Min(billingDay, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
                if (dueDate < currentDate) dueDate = dueDate.AddMonths(1);

                periods.Add((currentDate, nextDate.AddDays(-1), dueDate));
                currentDate = nextDate;
                if (billingFrequency == BillingFrequency.OneTime) break;
            }

            if (!periods.Any()) return 0;

            // Step 2: Divide remaining season price equally across periods
            decimal remaining = totalSeasonPrice - alreadyCollected;
            if (remaining < 0) remaining = 0;

            int periodCount = periods.Count;
            decimal perInstallment = Math.Round(remaining / periodCount, 4);
            decimal distributed = perInstallment * (periodCount - 1);
            decimal lastInstallment = remaining - distributed;

            // Step 3: Create schedule entries
            var schedules = new List<ErpContractBillingSchedule>();
            for (int i = 0; i < periodCount; i++)
            {
                var period = periods[i];
                decimal installmentAmount = (i == periodCount - 1) ? lastInstallment : perInstallment;

                // Apply pricing rules if any
                var adjustedAmount = await _pricingRuleRepository.CalculateAmountAsync(dto.ContractId, installmentAmount, period.DueDate);

                schedules.Add(new ErpContractBillingSchedule
                {
                    ContractId = dto.ContractId,
                    ScheduleNumber = nextScheduleNum + i,
                    DueDate = period.DueDate,
                    PaymentDueDate = period.DueDate.AddDays(paymentDueDays),
                    PeriodStart = period.Start,
                    PeriodEnd = period.End,
                    BaseAmount = installmentAmount,
                    Subtotal = adjustedAmount,
                    TotalAmount = adjustedAmount,
                    Currency = currency,
                    Status = BillingScheduleStatus.Pending,
                    Create_by = createdBy
                });
            }

            if (schedules.Any())
            {
                return await _billingScheduleRepository.BulkCreateAsync(schedules);
            }

            return 0;
        }

        /// <summary>
        /// Update billing schedule
        /// </summary>
        public int Update(UpdateBillingScheduleDto dto, string updatedBy)
        {
            var existing = _billingScheduleRepository.GetById(dto.ScheduleId);
            if (existing == null) return 0;

            // Only allow updates to pending schedules
            if (existing.Status != BillingScheduleStatus.Pending)
            {
                throw new InvalidOperationException("Can only update pending billing schedules");
            }

            var schedule = dto.Adapt<ErpContractBillingSchedule>();
            schedule.ContractId = existing.ContractId;
            schedule.Update_by = updatedBy;

            return _billingScheduleRepository.UpdateSchedule(schedule);
        }

        /// <summary>
        /// Update billing schedule async
        /// </summary>
        public async Task<int> UpdateAsync(UpdateBillingScheduleDto dto, string updatedBy)
        {
            var existing = await _billingScheduleRepository.GetByIdAsync(dto.ScheduleId);
            if (existing == null) return 0;

            if (existing.Status != BillingScheduleStatus.Pending)
            {
                throw new InvalidOperationException("Can only update pending billing schedules");
            }

            var schedule = dto.Adapt<ErpContractBillingSchedule>();
            schedule.ContractId = existing.ContractId;
            schedule.Update_by = updatedBy;

            return await _billingScheduleRepository.UpdateScheduleAsync(schedule);
        }

        /// <summary>
        /// Delete billing schedule
        /// </summary>
        public int Delete(Guid scheduleId)
        {
            var existing = _billingScheduleRepository.GetById(scheduleId);
            if (existing == null) return 0;

            // Only allow deletion of pending schedules
            if (existing.Status != BillingScheduleStatus.Pending)
            {
                throw new InvalidOperationException("Can only delete pending billing schedules");
            }

            return _billingScheduleRepository.Delete(scheduleId);
        }

        /// <summary>
        /// Delete billing schedule async
        /// </summary>
        public async Task<int> DeleteAsync(Guid scheduleId)
        {
            var existing = await _billingScheduleRepository.GetByIdAsync(scheduleId);
            if (existing == null) return 0;

            if (existing.Status != BillingScheduleStatus.Pending)
            {
                throw new InvalidOperationException("Can only delete pending billing schedules");
            }

            return await _billingScheduleRepository.DeleteAsync(scheduleId);
        }

        /// <summary>
        /// Mark schedule as invoiced
        /// </summary>
        public int MarkAsInvoiced(Guid scheduleId, Guid invoiceId, string invoiceNumber)
        {
            return _billingScheduleRepository.MarkAsInvoiced(scheduleId, invoiceId, invoiceNumber);
        }

        /// <summary>
        /// Mark schedule as invoiced async
        /// </summary>
        public async Task<int> MarkAsInvoicedAsync(Guid scheduleId, Guid invoiceId, string invoiceNumber)
        {
            return await _billingScheduleRepository.MarkAsInvoicedAsync(scheduleId, invoiceId, invoiceNumber);
        }

        /// <summary>
        /// Mark schedule as paid
        /// </summary>
        public int MarkAsPaid(Guid scheduleId, DateTime paidDate)
        {
            return _billingScheduleRepository.MarkAsPaid(scheduleId, paidDate);
        }

        /// <summary>
        /// Mark schedule as paid async
        /// </summary>
        public async Task<int> MarkAsPaidAsync(Guid scheduleId, DateTime paidDate)
        {
            return await _billingScheduleRepository.MarkAsPaidAsync(scheduleId, paidDate);
        }

        /// <summary>
        /// Cancel schedule
        /// </summary>
        public int Cancel(Guid scheduleId, string cancelledBy)
        {
            var existing = _billingScheduleRepository.GetById(scheduleId);
            if (existing == null) return 0;

            // Cannot cancel paid schedules
            if (existing.Status == BillingScheduleStatus.Paid)
            {
                throw new InvalidOperationException("Cannot cancel paid billing schedules");
            }

            return _billingScheduleRepository.Cancel(scheduleId, cancelledBy);
        }

        /// <summary>
        /// Cancel schedule async
        /// </summary>
        public async Task<int> CancelAsync(Guid scheduleId, string cancelledBy)
        {
            var existing = await _billingScheduleRepository.GetByIdAsync(scheduleId);
            if (existing == null) return 0;

            if (existing.Status == BillingScheduleStatus.Paid)
            {
                throw new InvalidOperationException("Cannot cancel paid billing schedules");
            }

            return await _billingScheduleRepository.CancelAsync(scheduleId, cancelledBy);
        }

        /// <summary>
        /// Skip schedule
        /// </summary>
        public int Skip(Guid scheduleId, string skippedBy)
        {
            var existing = _billingScheduleRepository.GetById(scheduleId);
            if (existing == null) return 0;

            // Only allow skipping pending schedules
            if (existing.Status != BillingScheduleStatus.Pending)
            {
                throw new InvalidOperationException("Can only skip pending billing schedules");
            }

            return _billingScheduleRepository.Skip(scheduleId, skippedBy);
        }

        /// <summary>
        /// Skip schedule async
        /// </summary>
        public async Task<int> SkipAsync(Guid scheduleId, string skippedBy)
        {
            var existing = await _billingScheduleRepository.GetByIdAsync(scheduleId);
            if (existing == null) return 0;

            if (existing.Status != BillingScheduleStatus.Pending)
            {
                throw new InvalidOperationException("Can only skip pending billing schedules");
            }

            return await _billingScheduleRepository.SkipAsync(scheduleId, skippedBy);
        }

        /// <summary>
        /// Get due schedules for invoicing
        /// </summary>
        public List<ErpContractBillingSchedule> GetDueSchedules(DateTime? asOfDate = null)
        {
            return _billingScheduleRepository.GetDueSchedules(asOfDate);
        }

        /// <summary>
        /// Get due schedules for invoicing async
        /// </summary>
        public async Task<List<ErpContractBillingSchedule>> GetDueSchedulesAsync(DateTime? asOfDate = null)
        {
            return await _billingScheduleRepository.GetDueSchedulesAsync(asOfDate);
        }

        /// <summary>
        /// Get upcoming schedules
        /// </summary>
        public List<ErpContractBillingScheduleListDto> GetUpcomingSchedules(Guid contractId, int count = 5)
        {
            return _billingScheduleRepository.GetUpcomingSchedules(contractId, count);
        }

        /// <summary>
        /// Get upcoming schedules async
        /// </summary>
        public async Task<List<ErpContractBillingScheduleListDto>> GetUpcomingSchedulesAsync(Guid contractId, int count = 5)
        {
            return await _billingScheduleRepository.GetUpcomingSchedulesAsync(contractId, count);
        }

        /// <summary>
        /// Cancel future schedules
        /// </summary>
        public int CancelFutureSchedules(Guid contractId, DateTime fromDate, string cancelledBy)
        {
            return _billingScheduleRepository.CancelFutureSchedules(contractId, fromDate, cancelledBy);
        }

        /// <summary>
        /// Cancel future schedules async
        /// </summary>
        public async Task<int> CancelFutureSchedulesAsync(Guid contractId, DateTime fromDate, string cancelledBy)
        {
            return await _billingScheduleRepository.CancelFutureSchedulesAsync(contractId, fromDate, cancelledBy);
        }

        /// <summary>
        /// Get schedule statistics
        /// </summary>
        public BillingScheduleStatisticsDto GetStatistics(Guid contractId)
        {
            return _billingScheduleRepository.GetStatistics(contractId);
        }

        /// <summary>
        /// Get schedule statistics async
        /// </summary>
        public async Task<BillingScheduleStatisticsDto> GetStatisticsAsync(Guid contractId)
        {
            return await _billingScheduleRepository.GetStatisticsAsync(contractId);
        }

        #region Private Helper Methods

        private DateTime CalculateNextDate(DateTime currentDate, BillingFrequency frequency)
        {
            switch (frequency)
            {
                case BillingFrequency.Weekly:
                    return currentDate.AddDays(7);
                case BillingFrequency.BiWeekly:
                    return currentDate.AddDays(14);
                case BillingFrequency.Monthly:
                    return currentDate.AddMonths(1);
                case BillingFrequency.Quarterly:
                    return currentDate.AddMonths(3);
                case BillingFrequency.SemiAnnual:
                    return currentDate.AddMonths(6);
                case BillingFrequency.Yearly:
                    return currentDate.AddYears(1);
                case BillingFrequency.OneTime:
                default:
                    return currentDate.AddYears(100); // Far future date for one-time
            }
        }

        #endregion
    }
}
