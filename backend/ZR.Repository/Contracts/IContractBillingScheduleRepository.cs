using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Billing Schedule Repository Interface - واجهة مستودع جداول الفوترة
    /// </summary>
    public interface IContractBillingScheduleRepository : IBaseRepository<ErpContractBillingSchedule>
    {
        /// <summary>
        /// Get billing schedules by contract ID
        /// </summary>
        List<ErpContractBillingScheduleListDto> GetByContractId(Guid contractId, BillingScheduleStatus? status = null);

        /// <summary>
        /// Get billing schedules by contract ID async
        /// </summary>
        Task<List<ErpContractBillingScheduleListDto>> GetByContractIdAsync(Guid contractId, BillingScheduleStatus? status = null);

        /// <summary>
        /// Get billing schedule by ID
        /// </summary>
        ErpContractBillingSchedule GetById(Guid scheduleId);

        /// <summary>
        /// Get billing schedule by ID async
        /// </summary>
        Task<ErpContractBillingSchedule> GetByIdAsync(Guid scheduleId);

        /// <summary>
        /// Bulk create schedules
        /// </summary>
        int BulkCreate(List<ErpContractBillingSchedule> schedules);

        /// <summary>
        /// Bulk create schedules async
        /// </summary>
        Task<int> BulkCreateAsync(List<ErpContractBillingSchedule> schedules);

        /// <summary>
        /// Update schedule
        /// </summary>
        int UpdateSchedule(ErpContractBillingSchedule schedule);

        /// <summary>
        /// Update schedule async
        /// </summary>
        Task<int> UpdateScheduleAsync(ErpContractBillingSchedule schedule);

        /// <summary>
        /// Delete schedule
        /// </summary>
        int Delete(Guid scheduleId);

        /// <summary>
        /// Delete schedule async
        /// </summary>
        Task<int> DeleteAsync(Guid scheduleId);

        /// <summary>
        /// Delete schedules by contract ID
        /// </summary>
        int DeleteByContractId(Guid contractId);

        /// <summary>
        /// Delete schedules by contract ID async
        /// </summary>
        Task<int> DeleteByContractIdAsync(Guid contractId);

        /// <summary>
        /// Mark schedule as invoiced
        /// </summary>
        int MarkAsInvoiced(Guid scheduleId, Guid invoiceId, string invoiceNumber);

        /// <summary>
        /// Mark schedule as invoiced async
        /// </summary>
        Task<int> MarkAsInvoicedAsync(Guid scheduleId, Guid invoiceId, string invoiceNumber);

        /// <summary>
        /// Mark schedule as paid
        /// </summary>
        int MarkAsPaid(Guid scheduleId, DateTime paidDate);

        /// <summary>
        /// Mark schedule as paid async
        /// </summary>
        Task<int> MarkAsPaidAsync(Guid scheduleId, DateTime paidDate);

        /// <summary>
        /// Cancel schedule
        /// </summary>
        int Cancel(Guid scheduleId, string cancelledBy);

        /// <summary>
        /// Cancel schedule async
        /// </summary>
        Task<int> CancelAsync(Guid scheduleId, string cancelledBy);

        /// <summary>
        /// Skip schedule
        /// </summary>
        int Skip(Guid scheduleId, string skippedBy);

        /// <summary>
        /// Skip schedule async
        /// </summary>
        Task<int> SkipAsync(Guid scheduleId, string skippedBy);

        /// <summary>
        /// Get due schedules
        /// </summary>
        List<ErpContractBillingSchedule> GetDueSchedules(DateTime? asOfDate = null);

        /// <summary>
        /// Get due schedules async
        /// </summary>
        Task<List<ErpContractBillingSchedule>> GetDueSchedulesAsync(DateTime? asOfDate = null);

        /// <summary>
        /// Get upcoming schedules
        /// </summary>
        List<ErpContractBillingScheduleListDto> GetUpcomingSchedules(Guid contractId, int count = 5);

        /// <summary>
        /// Get upcoming schedules async
        /// </summary>
        Task<List<ErpContractBillingScheduleListDto>> GetUpcomingSchedulesAsync(Guid contractId, int count = 5);

        /// <summary>
        /// Cancel future schedules
        /// </summary>
        int CancelFutureSchedules(Guid contractId, DateTime fromDate, string cancelledBy);

        /// <summary>
        /// Cancel future schedules async
        /// </summary>
        Task<int> CancelFutureSchedulesAsync(Guid contractId, DateTime fromDate, string cancelledBy);

        /// <summary>
        /// Get statistics
        /// </summary>
        BillingScheduleStatisticsDto GetStatistics(Guid contractId);

        /// <summary>
        /// Get statistics async
        /// </summary>
        Task<BillingScheduleStatisticsDto> GetStatisticsAsync(Guid contractId);
    }
}
