using System;
using System.Collections.Generic;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    /// <summary>
    /// Query parameters for revenue reports
    /// </summary>
    public class RevenueReportQueryDto : PagerInfo
    {
        public Guid? ContractId { get; set; }
        public Guid? UnitId { get; set; }
        public RevenueReportStatus? Status { get; set; }
        public DateTime? PeriodStartFrom { get; set; }
        public DateTime? PeriodStartTo { get; set; }
    }

    /// <summary>
    /// Submit a revenue report for a contract unit
    /// </summary>
    public class CreateRevenueReportDto
    {
        public Guid ContractId { get; set; }
        public Guid UnitId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ReportedRevenue { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update reported revenue (only if status = Draft)
    /// </summary>
    public class UpdateRevenueReportDto
    {
        public Guid ReportId { get; set; }
        public decimal ReportedRevenue { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Response DTO with full calculated details
    /// </summary>
    public class RevenueReportResponseDto
    {
        public Guid ReportId { get; set; }
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public Guid UnitId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string CustomerName { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ReportedRevenue { get; set; }

        // Pricing snapshot
        public PricingType PricingType { get; set; }
        public string PricingTypeName { get; set; }
        public decimal? FixedRentAmount { get; set; }
        public decimal? RevenuePercentage { get; set; }
        public decimal? MinimumGuarantee { get; set; }

        // Calculated
        public decimal? RevenueBasedAmount { get; set; }
        public decimal CalculatedInvoiceAmount { get; set; }
        public bool MinimumGuaranteeApplied { get; set; }

        public string Currency { get; set; }
        public RevenueReportStatus Status { get; set; }
        public string StatusName { get; set; }
        public Guid? BillingScheduleId { get; set; }
        public string Notes { get; set; }

        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Bulk submit revenue reports for multiple units in one period
    /// </summary>
    public class BulkCreateRevenueReportDto
    {
        public Guid ContractId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<UnitRevenueEntryDto> Entries { get; set; } = new();
    }

    /// <summary>
    /// Single unit revenue entry inside a bulk submission
    /// </summary>
    public class UnitRevenueEntryDto
    {
        public Guid UnitId { get; set; }
        public decimal ReportedRevenue { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Summary of revenue reports for a contract
    /// </summary>
    public class RevenueReportSummaryDto
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public int TotalReports { get; set; }
        public int DraftReports { get; set; }
        public int SubmittedReports { get; set; }
        public int InvoicedReports { get; set; }
        public decimal TotalReportedRevenue { get; set; }
        public decimal TotalCalculatedInvoiceAmount { get; set; }
    }
}
