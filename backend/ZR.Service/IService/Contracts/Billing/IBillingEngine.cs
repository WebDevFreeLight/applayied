using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;

namespace ZR.Service.IService.Contracts.Billing
{
    /// <summary>
    /// IBillingEngine - Billing Engine Interface
    /// محرك الفوترة
    /// Handles invoice generation, billing calculations, and revenue analytics
    /// </summary>
    public interface IBillingEngine
    {
        #region Invoice Generation

        /// <summary>
        /// Generate invoices for due billing schedules
        /// </summary>
        Task<InvoiceGenerationResult> GenerateInvoicesAsync(InvoiceGenerationRequest request);

        /// <summary>
        /// Generate invoice for a specific billing schedule
        /// </summary>
        Task<InvoiceResult> GenerateInvoiceForScheduleAsync(Guid scheduleId, string processedBy);

        /// <summary>
        /// Preview invoice before generation
        /// </summary>
        Task<InvoicePreview> PreviewInvoiceAsync(Guid scheduleId);

        /// <summary>
        /// Get pending invoices for generation
        /// </summary>
        Task<List<PendingInvoiceInfo>> GetPendingInvoicesAsync(DateTime? asOfDate, Guid? contractId);

        /// <summary>
        /// Batch generate invoices for all due schedules
        /// </summary>
        Task<BatchInvoiceResult> BatchGenerateInvoicesAsync(DateTime asOfDate, string processedBy);

        #endregion

        #region Billing Analytics

        /// <summary>
        /// Get billing summary for a contract
        /// </summary>
        Task<BillingSummary> GetBillingSummaryAsync(Guid contractId);

        /// <summary>
        /// Get billing timeline for a contract
        /// </summary>
        Task<List<BillingTimelineEntry>> GetBillingTimelineAsync(Guid contractId, DateTime? fromDate, DateTime? toDate);

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        Task<List<OverdueInvoiceInfo>> GetOverdueInvoicesAsync(int minDaysOverdue, Guid? customerId);

        /// <summary>
        /// Get revenue forecast
        /// </summary>
        Task<RevenueForecast> GetRevenueForecastAsync(DateTime fromDate, DateTime toDate, Guid? contractId);

        #endregion
    }

    #region Request/Response DTOs

    /// <summary>
    /// Request for invoice generation
    /// </summary>
    public class InvoiceGenerationRequest
    {
        public DateTime? AsOfDate { get; set; }
        public List<Guid>? ScheduleIds { get; set; }
        public Guid? ContractId { get; set; }
        public bool IncludeOverdue { get; set; } = true;
        public string? ProcessedBy { get; set; }
    }

    /// <summary>
    /// Result of invoice generation batch
    /// </summary>
    public class InvoiceGenerationResult
    {
        public bool Success { get; set; }
        public int TotalSchedules { get; set; }
        public int SuccessfulInvoices { get; set; }
        public int FailedInvoices { get; set; }
        public decimal TotalAmount { get; set; }
        public List<InvoiceResult> InvoiceResults { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Result of single invoice generation
    /// </summary>
    public class InvoiceResult
    {
        public bool Success { get; set; }
        public Guid? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public Guid ScheduleId { get; set; }
        public Guid ContractId { get; set; }
        public string? ContractNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Invoice preview information
    /// </summary>
    public class InvoicePreview
    {
        public Guid ScheduleId { get; set; }
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal EscalationAmount { get; set; }
        public decimal SeasonalAdjustment { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<InvoiceLinePreview> LineItems { get; set; } = new();
        public List<PricingAdjustmentDetail> Adjustments { get; set; } = new();
    }

    /// <summary>
    /// Invoice line item preview
    /// </summary>
    public class InvoiceLinePreview
    {
        public string Description { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
    }

    /// <summary>
    /// Pending invoice information
    /// </summary>
    public class PendingInvoiceInfo
    {
        public Guid ScheduleId { get; set; }
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EstimatedAmount { get; set; }
        public int DaysUntilDue { get; set; }
        public bool IsOverdue { get; set; }
    }

    /// <summary>
    /// Result of batch invoice generation
    /// </summary>
    public class BatchInvoiceResult
    {
        public bool Success { get; set; }
        public DateTime ProcessedDate { get; set; }
        public int TotalProcessed { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<InvoiceResult> Results { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
    }

    /// <summary>
    /// Billing summary for a contract
    /// </summary>
    public class BillingSummary
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public decimal TotalContractValue { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalOverdue { get; set; }
        public int TotalSchedules { get; set; }
        public int InvoicedSchedules { get; set; }
        public int PaidSchedules { get; set; }
        public int PendingSchedules { get; set; }
        public int OverdueSchedules { get; set; }
        public decimal BillingProgress { get; set; } // Percentage
        public decimal PaymentProgress { get; set; } // Percentage
        public DateTime? NextBillingDate { get; set; }
        public decimal? NextBillingAmount { get; set; }
    }

    /// <summary>
    /// Billing timeline entry
    /// </summary>
    public class BillingTimelineEntry
    {
        public Guid ScheduleId { get; set; }
        public int ScheduleNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public Guid? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal PaidAmount { get; set; }
    }

    /// <summary>
    /// Overdue invoice information
    /// </summary>
    public class OverdueInvoiceInfo
    {
        public Guid ScheduleId { get; set; }
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountOutstanding { get; set; }
    }

    /// <summary>
    /// Revenue forecast
    /// </summary>
    public class RevenueForecast
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalForecastedRevenue { get; set; }
        public decimal ConfirmedRevenue { get; set; } // From existing invoices
        public decimal ProjectedRevenue { get; set; } // From pending schedules
        public List<MonthlyRevenueForecast> MonthlyBreakdown { get; set; } = new();
        public List<ContractRevenueForecast> ContractBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Monthly revenue forecast
    /// </summary>
    public class MonthlyRevenueForecast
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal ForecastedAmount { get; set; }
        public decimal ConfirmedAmount { get; set; }
        public decimal ProjectedAmount { get; set; }
        public int ScheduleCount { get; set; }
    }

    /// <summary>
    /// Contract revenue forecast
    /// </summary>
    public class ContractRevenueForecast
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal ForecastedAmount { get; set; }
        public int ScheduleCount { get; set; }
    }

    #endregion
}
