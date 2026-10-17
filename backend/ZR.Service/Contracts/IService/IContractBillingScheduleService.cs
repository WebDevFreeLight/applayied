using System;
using System.Collections.Generic;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;

namespace ZR.Service.Contracts.IService
{
    /// <summary>
    /// Contract Billing Schedule Service Interface - واجهة خدمة جداول الفوترة
    /// </summary>
    public interface IContractBillingScheduleService
    {
        /// <summary>
        /// Get billing schedules by contract ID
        /// الحصول على جداول الفوترة بمعرف العقد
        /// </summary>
        List<ErpContractBillingScheduleListDto> GetByContractId(Guid contractId, BillingScheduleStatus? status = null);

        /// <summary>
        /// Get billing schedule by ID
        /// الحصول على جدول الفوترة بالمعرف
        /// </summary>
        ErpContractBillingSchedule GetById(Guid scheduleId);

        /// <summary>
        /// Generate billing schedules for contract
        /// إنشاء جداول الفوترة للعقد
        /// </summary>
        int GenerateBillingSchedule(GenerateBillingScheduleDto dto, string createdBy);

        /// <summary>
        /// Update billing schedule
        /// تحديث جدول الفوترة
        /// </summary>
        int Update(UpdateBillingScheduleDto dto, string updatedBy);

        /// <summary>
        /// Delete billing schedule
        /// حذف جدول الفوترة
        /// </summary>
        int Delete(Guid scheduleId);

        /// <summary>
        /// Mark schedule as invoiced
        /// تحديد الجدول كمفوتر
        /// </summary>
        int MarkAsInvoiced(Guid scheduleId, Guid invoiceId, string invoiceNumber);

        /// <summary>
        /// Mark schedule as paid
        /// تحديد الجدول كمدفوع
        /// </summary>
        int MarkAsPaid(Guid scheduleId, DateTime paidDate);

        /// <summary>
        /// Cancel schedule
        /// إلغاء الجدول
        /// </summary>
        int Cancel(Guid scheduleId, string cancelledBy);

        /// <summary>
        /// Skip schedule
        /// تخطي الجدول
        /// </summary>
        int Skip(Guid scheduleId, string skippedBy);

        /// <summary>
        /// Get due schedules for invoicing
        /// الحصول على الجداول المستحقة للفوترة
        /// </summary>
        List<ErpContractBillingSchedule> GetDueSchedules(DateTime? asOfDate = null);

        /// <summary>
        /// Get upcoming schedules
        /// الحصول على الجداول القادمة
        /// </summary>
        List<ErpContractBillingScheduleListDto> GetUpcomingSchedules(Guid contractId, int count = 5);

        /// <summary>
        /// Cancel future schedules
        /// إلغاء الجداول المستقبلية
        /// </summary>
        int CancelFutureSchedules(Guid contractId, DateTime fromDate, string cancelledBy);

        /// <summary>
        /// Get schedule statistics
        /// الحصول على إحصائيات الجداول
        /// </summary>
        BillingScheduleStatisticsDto GetStatistics(Guid contractId);
    }
}
