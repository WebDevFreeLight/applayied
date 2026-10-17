using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Model.Finance.ARAP;
using ZR.Model.Customers;
using ZR.Repository;
using ZR.Service.Contracts.IService;

namespace ZR.Service.Contracts
{
    /// <summary>
    /// Contract Revenue Report Service - خدمة تقارير إيرادات العقود
    /// Deterministic calculation engine:
    ///   Fixed → invoice = fixed_rent_amount
    ///   RevenueShare → invoice = MAX(reported_revenue × percentage, minimum_guarantee)
    /// </summary>
    [AppService(ServiceType = typeof(IContractRevenueReportService), ServiceLifetime = LifeTime.Transient)]
    public class ContractRevenueReportService : BaseService<ErpContractRevenueReport>, IContractRevenueReportService
    {
        public async Task<PagedInfo<RevenueReportResponseDto>> GetListAsync(RevenueReportQueryDto query)
        {
            var pageQuery = Context
                .Queryable<ErpContractRevenueReport, ErpContract, ErpContractUnit, Customer>(
                    (rr, c, u, cust) => new JoinQueryInfos(
                        JoinType.Left, rr.ContractId == c.ContractId,
                        JoinType.Left, rr.UnitId == u.UnitId,
                        JoinType.Left, c.CustomerId == cust.Id))
                .WhereIF(query.ContractId.HasValue, (rr, c, u, cust) => rr.ContractId == query.ContractId.Value)
                .WhereIF(query.UnitId.HasValue, (rr, c, u, cust) => rr.UnitId == query.UnitId.Value)
                .WhereIF(query.Status.HasValue, (rr, c, u, cust) => rr.Status == query.Status.Value)
                .WhereIF(query.PeriodStartFrom.HasValue, (rr, c, u, cust) => rr.PeriodStart >= query.PeriodStartFrom.Value)
                .WhereIF(query.PeriodStartTo.HasValue, (rr, c, u, cust) => rr.PeriodStart <= query.PeriodStartTo.Value)
                .OrderByDescending((rr, c, u, cust) => rr.PeriodStart)
                .Select((rr, c, u, cust) => new RevenueReportResponseDto
                {
                    ReportId = rr.ReportId,
                    ContractId = rr.ContractId,
                    ContractNumber = c.ContractNumber,
                    UnitId = rr.UnitId,
                    UnitCode = u.UnitCode,
                    UnitName = u.UnitName,
                    CustomerName = cust.NameEn,
                    PeriodStart = rr.PeriodStart,
                    PeriodEnd = rr.PeriodEnd,
                    ReportedRevenue = rr.ReportedRevenue,
                    PricingType = rr.PricingType,
                    PricingTypeName = rr.PricingType.ToString(),
                    FixedRentAmount = rr.FixedRentAmount,
                    RevenuePercentage = rr.RevenuePercentage,
                    MinimumGuarantee = rr.MinimumGuarantee,
                    RevenueBasedAmount = rr.RevenueBasedAmount,
                    CalculatedInvoiceAmount = rr.CalculatedInvoiceAmount,
                    MinimumGuaranteeApplied = rr.MinimumGuaranteeApplied,
                    Currency = rr.Currency,
                    Status = rr.Status,
                    StatusName = rr.Status.ToString(),
                    BillingScheduleId = rr.BillingScheduleId,
                    Notes = rr.Notes,
                    Create_by = rr.Create_by,
                    Create_time = rr.Create_time
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<ErpContractRevenueReport> GetByIdAsync(Guid reportId)
        {
            return await GetFirstAsync(r => r.ReportId == reportId);
        }

        public async Task<List<RevenueReportResponseDto>> GetByContractIdAsync(Guid contractId)
        {
            return await Context
                .Queryable<ErpContractRevenueReport, ErpContractUnit>(
                    (rr, u) => new JoinQueryInfos(JoinType.Left, rr.UnitId == u.UnitId))
                .Where((rr, u) => rr.ContractId == contractId)
                .OrderByDescending((rr, u) => rr.PeriodStart)
                .Select((rr, u) => new RevenueReportResponseDto
                {
                    ReportId = rr.ReportId,
                    ContractId = rr.ContractId,
                    UnitId = rr.UnitId,
                    UnitCode = u.UnitCode,
                    UnitName = u.UnitName,
                    PeriodStart = rr.PeriodStart,
                    PeriodEnd = rr.PeriodEnd,
                    ReportedRevenue = rr.ReportedRevenue,
                    PricingType = rr.PricingType,
                    PricingTypeName = rr.PricingType.ToString(),
                    FixedRentAmount = rr.FixedRentAmount,
                    RevenuePercentage = rr.RevenuePercentage,
                    MinimumGuarantee = rr.MinimumGuarantee,
                    RevenueBasedAmount = rr.RevenueBasedAmount,
                    CalculatedInvoiceAmount = rr.CalculatedInvoiceAmount,
                    MinimumGuaranteeApplied = rr.MinimumGuaranteeApplied,
                    Currency = rr.Currency,
                    Status = rr.Status,
                    StatusName = rr.Status.ToString(),
                    BillingScheduleId = rr.BillingScheduleId,
                    Notes = rr.Notes,
                    Create_by = rr.Create_by,
                    Create_time = rr.Create_time
                })
                .ToListAsync();
        }

        public async Task<ErpContractRevenueReport> CreateAsync(CreateRevenueReportDto dto, string createdBy)
        {
            // Validate contract exists
            var contract = await Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == dto.ContractId)
                .FirstAsync();
            if (contract == null)
                throw new Exception("Contract not found");

            // Validate unit exists and belongs to contract
            var unit = await Context.Queryable<ErpContractUnit>()
                .Where(u => u.UnitId == dto.UnitId && u.ContractId == dto.ContractId)
                .FirstAsync();
            if (unit == null)
                throw new Exception("Contract unit not found or does not belong to this contract");

            // Check for duplicate period
            var duplicateExists = await Context.Queryable<ErpContractRevenueReport>()
                .Where(r => r.ContractId == dto.ContractId
                    && r.UnitId == dto.UnitId
                    && r.PeriodStart == dto.PeriodStart
                    && r.PeriodEnd == dto.PeriodEnd)
                .AnyAsync();
            if (duplicateExists)
                throw new Exception("A revenue report already exists for this unit and period");

            // Build report with pricing snapshot from contract unit
            var report = new ErpContractRevenueReport
            {
                ReportId = Guid.NewGuid(),
                ContractId = dto.ContractId,
                UnitId = dto.UnitId,
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                ReportedRevenue = dto.ReportedRevenue,
                // Snapshot pricing from contract unit (source of financial truth)
                PricingType = unit.PricingType,
                FixedRentAmount = unit.FixedRentAmount,
                RevenuePercentage = unit.RevenuePercentage,
                MinimumGuarantee = unit.MinimumGuarantee,
                Currency = unit.Currency ?? "SAR",
                Notes = dto.Notes,
                Status = RevenueReportStatus.Draft,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            // Deterministic calculation
            CalculateInvoiceAmount(report);

            await InsertAsync(report);
            return report;
        }

        public async Task<List<ErpContractRevenueReport>> BulkCreateAsync(BulkCreateRevenueReportDto dto, string createdBy)
        {
            var results = new List<ErpContractRevenueReport>();
            foreach (var entry in dto.Entries)
            {
                var createDto = new CreateRevenueReportDto
                {
                    ContractId = dto.ContractId,
                    UnitId = entry.UnitId,
                    PeriodStart = dto.PeriodStart,
                    PeriodEnd = dto.PeriodEnd,
                    ReportedRevenue = entry.ReportedRevenue,
                    Notes = entry.Notes
                };
                results.Add(await CreateAsync(createDto, createdBy));
            }
            return results;
        }

        public async Task<int> UpdateAsync(UpdateRevenueReportDto dto, string updatedBy)
        {
            var report = await GetFirstAsync(r => r.ReportId == dto.ReportId);
            if (report == null)
                throw new Exception("Revenue report not found");
            if (report.Status != RevenueReportStatus.Draft)
                throw new Exception("Only draft reports can be updated");

            report.ReportedRevenue = dto.ReportedRevenue;
            if (dto.Notes != null) report.Notes = dto.Notes;
            report.Update_by = updatedBy;
            report.Update_time = DateTime.Now;

            // Recalculate
            CalculateInvoiceAmount(report);

            return await UpdateAsync(report) ? 1 : 0;
        }

        public async Task<int> SubmitAsync(Guid reportId, string submittedBy)
        {
            var report = await GetFirstAsync(r => r.ReportId == reportId);
            if (report == null)
                throw new Exception("Revenue report not found");
            if (report.Status != RevenueReportStatus.Draft)
                throw new Exception("Only draft reports can be submitted");

            report.Status = RevenueReportStatus.Submitted;
            report.Update_by = submittedBy;
            report.Update_time = DateTime.Now;

            return await UpdateAsync(report) ? 1 : 0;
        }

        public async Task<int> DeleteAsync(Guid reportId)
        {
            var report = await GetFirstAsync(r => r.ReportId == reportId);
            if (report == null)
                throw new Exception("Revenue report not found");
            if (report.Status != RevenueReportStatus.Draft)
                throw new Exception("Only draft reports can be deleted");

            return await Context.Deleteable<ErpContractRevenueReport>()
                .Where(r => r.ReportId == reportId)
                .ExecuteCommandAsync();
        }

        public async Task<RevenueReportSummaryDto> GetSummaryAsync(Guid contractId)
        {
            var reports = await GetListAsync(r => r.ContractId == contractId);
            var contract = await Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == contractId)
                .FirstAsync();

            return new RevenueReportSummaryDto
            {
                ContractId = contractId,
                ContractNumber = contract?.ContractNumber,
                TotalReports = reports.Count,
                DraftReports = reports.Count(r => r.Status == RevenueReportStatus.Draft),
                SubmittedReports = reports.Count(r => r.Status == RevenueReportStatus.Submitted),
                InvoicedReports = reports.Count(r => r.Status == RevenueReportStatus.Invoiced),
                TotalReportedRevenue = reports.Sum(r => r.ReportedRevenue),
                TotalCalculatedInvoiceAmount = reports.Sum(r => r.CalculatedInvoiceAmount)
            };
        }

        #region Deterministic Calculation Engine

        /// <summary>
        /// Core calculation logic. Deterministic and stateless.
        /// Fixed → invoice = fixed_rent_amount
        /// RevenueShare → calculated = reported_revenue × (revenue_percentage / 100)
        ///                  if minimum_guarantee exists → invoice = MAX(calculated, minimum_guarantee)
        ///                  else → invoice = calculated
        /// </summary>
        private static void CalculateInvoiceAmount(ErpContractRevenueReport report)
        {
            switch (report.PricingType)
            {
                case PricingType.Fixed:
                    report.RevenueBasedAmount = null;
                    report.CalculatedInvoiceAmount = report.FixedRentAmount ?? 0;
                    report.MinimumGuaranteeApplied = false;
                    break;

                case PricingType.RevenueShare:
                    var revenueBasedAmount = report.ReportedRevenue * (report.RevenuePercentage ?? 0) / 100m;
                    report.RevenueBasedAmount = Math.Round(revenueBasedAmount, 2);

                    if (report.MinimumGuarantee.HasValue && report.MinimumGuarantee.Value > revenueBasedAmount)
                    {
                        report.CalculatedInvoiceAmount = report.MinimumGuarantee.Value;
                        report.MinimumGuaranteeApplied = true;
                    }
                    else
                    {
                        report.CalculatedInvoiceAmount = Math.Round(revenueBasedAmount, 2);
                        report.MinimumGuaranteeApplied = false;
                    }
                    break;

                case PricingType.Hybrid:
                    // Future: base rent + revenue share above threshold
                    var hybridBase = report.FixedRentAmount ?? 0;
                    var hybridRevenue = report.ReportedRevenue * (report.RevenuePercentage ?? 0) / 100m;
                    report.RevenueBasedAmount = Math.Round(hybridRevenue, 2);
                    report.CalculatedInvoiceAmount = Math.Round(hybridBase + hybridRevenue, 2);
                    report.MinimumGuaranteeApplied = false;
                    break;

                default:
                    report.CalculatedInvoiceAmount = 0;
                    report.MinimumGuaranteeApplied = false;
                    break;
            }
        }

        #endregion
    }
}
