using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using SqlSugar.IOC;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;
using ZR.Service.IService.Contracts.Billing;

namespace ZR.Service.Contracts.Billing
{
    /// <summary>
    /// BillingEngine - Billing Engine Implementation
    /// محرك الفوترة
    /// Handles invoice generation, billing calculations, and revenue analytics
    /// Updated to use [AppService] for DI registration
    /// </summary>
    [AppService(ServiceType = typeof(IBillingEngine), ServiceLifetime = LifeTime.Transient)]
    public class BillingEngine : IBillingEngine
    {
        /// <summary>
        /// Database context using SqlSugar IOC pattern
        /// </summary>
        private ISqlSugarClient Db => DbScoped.SugarScope.GetConnectionScope(0);

        private readonly IPricingRuleEngine _pricingEngine;

        public BillingEngine(IPricingRuleEngine pricingEngine)
        {
            _pricingEngine = pricingEngine;
        }

        #region Invoice Generation

        public async Task<InvoiceGenerationResult> GenerateInvoicesAsync(InvoiceGenerationRequest request)
        {
            var result = new InvoiceGenerationResult();
            var asOfDate = request.AsOfDate ?? DateTime.Today;

            try
            {
                // Get schedules to process
                var query = Db.Queryable<ErpContractBillingSchedule>()
                    .Where(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due);

                if (request.ScheduleIds?.Any() == true)
                {
                    query = query.Where(s => request.ScheduleIds.Contains(s.ScheduleId));
                }
                else
                {
                    if (request.IncludeOverdue)
                    {
                        query = query.Where(s => s.DueDate <= asOfDate);
                    }
                    else
                    {
                        query = query.Where(s => s.DueDate == asOfDate);
                    }

                    if (request.ContractId.HasValue)
                    {
                        query = query.Where(s => s.ContractId == request.ContractId.Value);
                    }
                }

                var schedules = await query.ToListAsync();
                result.TotalSchedules = schedules.Count;

                foreach (var schedule in schedules)
                {
                    var invoiceResult = await GenerateInvoiceForScheduleAsync(schedule.ScheduleId, request.ProcessedBy ?? "System");
                    result.InvoiceResults.Add(invoiceResult);

                    if (invoiceResult.Success)
                    {
                        result.SuccessfulInvoices++;
                        result.TotalAmount += invoiceResult.TotalAmount;
                    }
                    else
                    {
                        result.FailedInvoices++;
                        result.Errors.Add($"Schedule {schedule.ScheduleId}: {invoiceResult.ErrorMessage}");
                    }
                }

                result.Success = result.FailedInvoices == 0;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Invoice generation failed: {ex.Message}");
            }

            return result;
        }

        public async Task<InvoiceResult> GenerateInvoiceForScheduleAsync(Guid scheduleId, string processedBy)
        {
            var result = new InvoiceResult { ScheduleId = scheduleId };

            try
            {
                var schedule = await Db.Queryable<ErpContractBillingSchedule>()
                    .FirstAsync(s => s.ScheduleId == scheduleId);

                if (schedule == null)
                {
                    result.ErrorMessage = "Billing schedule not found";
                    return result;
                }

                if (schedule.Status == BillingScheduleStatus.Invoiced || schedule.Status == BillingScheduleStatus.Paid)
                {
                    result.ErrorMessage = "Schedule already invoiced";
                    return result;
                }

                var contract = await Db.Queryable<ErpContract>()
                    .FirstAsync(c => c.ContractId == schedule.ContractId);

                if (contract == null)
                {
                    result.ErrorMessage = "Contract not found";
                    return result;
                }

                // Calculate final amount with pricing rules
                var pricingResult = await _pricingEngine.CalculatePriceAsync(new PricingCalculationRequest
                {
                    ContractId = schedule.ContractId,
                    BaseAmount = schedule.BaseAmount,
                    PeriodStart = schedule.PeriodStart,
                    PeriodEnd = schedule.PeriodEnd,
                    BillingDate = DateTime.Today
                });

                // Update schedule with invoice info
                schedule.TotalAmount = pricingResult.FinalAmount;
                schedule.Status = BillingScheduleStatus.Invoiced;
                schedule.InvoicedDate = DateTime.Today;
                schedule.InvoicedBy = processedBy;
                schedule.InvoiceNumber = GenerateInvoiceNumber(contract.ContractNumber, schedule.ScheduleNumber);
                schedule.Update_by = processedBy;
                schedule.Update_time = DateTime.Now;

                await Db.Updateable(schedule).ExecuteCommandAsync();

                result.Success = true;
                result.ContractId = contract.ContractId;
                result.ContractNumber = contract.ContractNumber;
                result.Amount = pricingResult.BaseAmount;
                result.TaxAmount = schedule.TaxAmount;
                result.TotalAmount = pricingResult.FinalAmount;
                result.InvoiceDate = schedule.InvoicedDate;
                result.DueDate = schedule.PaymentDueDate;
                result.InvoiceNumber = schedule.InvoiceNumber;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<InvoicePreview> PreviewInvoiceAsync(Guid scheduleId)
        {
            var preview = new InvoicePreview { ScheduleId = scheduleId };

            var schedule = await Db.Queryable<ErpContractBillingSchedule>()
                .FirstAsync(s => s.ScheduleId == scheduleId);

            if (schedule == null)
            {
                throw new Exception("Billing schedule not found");
            }

            var contract = await Db.Queryable<ErpContract>()
                .FirstAsync(c => c.ContractId == schedule.ContractId);

            if (contract == null)
            {
                throw new Exception("Contract not found");
            }

            // Get customer name from contract
            var customerName = await GetCustomerNameAsync(contract.CustomerId);

            preview.ContractId = contract.ContractId;
            preview.ContractNumber = contract.ContractNumber;
            preview.CustomerName = customerName;
            preview.PeriodStart = schedule.PeriodStart;
            preview.PeriodEnd = schedule.PeriodEnd;
            preview.BaseAmount = schedule.BaseAmount;

            // Calculate pricing
            var pricingResult = await _pricingEngine.CalculatePriceAsync(new PricingCalculationRequest
            {
                ContractId = schedule.ContractId,
                BaseAmount = schedule.BaseAmount,
                PeriodStart = schedule.PeriodStart,
                PeriodEnd = schedule.PeriodEnd,
                BillingDate = DateTime.Today
            });

            preview.EscalationAmount = pricingResult.EscalationAmount;
            preview.SeasonalAdjustment = pricingResult.SeasonalAdjustment;
            preview.DiscountAmount = pricingResult.DiscountAmount;
            preview.SubTotal = pricingResult.FinalAmount;
            preview.TaxAmount = schedule.TaxAmount;
            preview.TotalAmount = pricingResult.FinalAmount + schedule.TaxAmount;
            preview.Adjustments = pricingResult.AdjustmentDetails;

            // Get unit details for line items
            var units = await Db.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contract.ContractId && u.IsActive)
                .ToListAsync();

            foreach (var unit in units)
            {
                preview.LineItems.Add(new InvoiceLinePreview
                {
                    Description = $"Rent for {unit.UnitName ?? unit.UnitCode}",
                    UnitName = unit.UnitName ?? unit.UnitCode,
                    Quantity = 1,
                    UnitPrice = unit.EffectivePrice ?? 0,
                    Amount = unit.EffectivePrice ?? 0,
                    TaxRate = 0,
                    TaxAmount = 0
                });
            }

            return preview;
        }

        public async Task<List<PendingInvoiceInfo>> GetPendingInvoicesAsync(DateTime? asOfDate, Guid? contractId)
        {
            var today = asOfDate ?? DateTime.Today;

            var query = Db.Queryable<ErpContractBillingSchedule, ErpContract>(
                (s, c) => new JoinQueryInfos(
                    JoinType.Inner, s.ContractId == c.ContractId
                ))
                .Where((s, c) => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due);

            if (contractId.HasValue)
            {
                query = query.Where((s, c) => s.ContractId == contractId.Value);
            }

            var schedules = await query.Select((s, c) => new PendingInvoiceInfo
            {
                ScheduleId = s.ScheduleId,
                ContractId = s.ContractId,
                ContractNumber = c.ContractNumber,
                CustomerName = "", // Will be populated below
                PeriodStart = s.PeriodStart,
                PeriodEnd = s.PeriodEnd,
                DueDate = s.DueDate,
                EstimatedAmount = s.TotalAmount
            }).ToListAsync();

            // Populate customer names
            var contractIds = schedules.Select(s => s.ContractId).Distinct().ToList();
            var contracts = await Db.Queryable<ErpContract>()
                .Where(c => contractIds.Contains(c.ContractId))
                .ToListAsync();

            var customerIds = contracts.Select(c => c.CustomerId).Distinct().ToList();
            var customerNames = await GetCustomerNamesAsync(customerIds);

            foreach (var schedule in schedules)
            {
                var contract = contracts.FirstOrDefault(c => c.ContractId == schedule.ContractId);
                if (contract != null && customerNames.TryGetValue(contract.CustomerId, out var name))
                {
                    schedule.CustomerName = name;
                }
                schedule.DaysUntilDue = (schedule.DueDate - today).Days;
                schedule.IsOverdue = schedule.DaysUntilDue < 0;
            }

            return schedules.OrderBy(s => s.DueDate).ToList();
        }

        public async Task<BatchInvoiceResult> BatchGenerateInvoicesAsync(DateTime asOfDate, string processedBy)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BatchInvoiceResult
            {
                ProcessedDate = asOfDate
            };

            try
            {
                var schedules = await Db.Queryable<ErpContractBillingSchedule>()
                    .Where(s => (s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due)
                        && s.DueDate <= asOfDate)
                    .ToListAsync();

                result.TotalProcessed = schedules.Count;

                foreach (var schedule in schedules)
                {
                    var invoiceResult = await GenerateInvoiceForScheduleAsync(schedule.ScheduleId, processedBy);
                    result.Results.Add(invoiceResult);

                    if (invoiceResult.Success)
                    {
                        result.Successful++;
                        result.TotalRevenue += invoiceResult.TotalAmount;
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add($"Schedule {schedule.ScheduleId}: {invoiceResult.ErrorMessage}");
                    }
                }

                result.Success = result.Failed == 0;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Batch processing failed: {ex.Message}");
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;

            return result;
        }

        #endregion

        #region Billing Analytics

        public async Task<BillingSummary> GetBillingSummaryAsync(Guid contractId)
        {
            var contract = await Db.Queryable<ErpContract>()
                .FirstAsync(c => c.ContractId == contractId);

            if (contract == null)
            {
                throw new Exception("Contract not found");
            }

            var schedules = await Db.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ToListAsync();

            var financial = await Db.Queryable<ErpContractFinancial>()
                .FirstAsync(f => f.ContractId == contractId);

            var summary = new BillingSummary
            {
                ContractId = contractId,
                ContractNumber = contract.ContractNumber,
                TotalContractValue = financial?.TotalAmount ?? 0,
                TotalSchedules = schedules.Count,
                InvoicedSchedules = schedules.Count(s => s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid),
                PaidSchedules = schedules.Count(s => s.Status == BillingScheduleStatus.Paid),
                PendingSchedules = schedules.Count(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due),
                OverdueSchedules = schedules.Count(s => (s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due || s.Status == BillingScheduleStatus.Invoiced)
                    && s.PaymentDueDate < DateTime.Today)
            };

            summary.TotalBilled = schedules
                .Where(s => s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid)
                .Sum(s => s.TotalAmount);

            summary.TotalPaid = schedules
                .Where(s => s.Status == BillingScheduleStatus.Paid)
                .Sum(s => s.TotalAmount);

            summary.TotalOutstanding = summary.TotalBilled - summary.TotalPaid;

            summary.TotalOverdue = schedules
                .Where(s => (s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Due)
                    && s.PaymentDueDate < DateTime.Today)
                .Sum(s => s.TotalAmount);

            if (summary.TotalContractValue > 0)
            {
                summary.BillingProgress = Math.Round(summary.TotalBilled / summary.TotalContractValue * 100, 2);
                summary.PaymentProgress = Math.Round(summary.TotalPaid / summary.TotalContractValue * 100, 2);
            }

            var nextSchedule = schedules
                .Where(s => s.Status == BillingScheduleStatus.Pending && s.DueDate >= DateTime.Today)
                .OrderBy(s => s.DueDate)
                .FirstOrDefault();

            if (nextSchedule != null)
            {
                summary.NextBillingDate = nextSchedule.DueDate;
                summary.NextBillingAmount = nextSchedule.TotalAmount;
            }

            return summary;
        }

        public async Task<List<BillingTimelineEntry>> GetBillingTimelineAsync(Guid contractId, DateTime? fromDate, DateTime? toDate)
        {
            var query = Db.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId);

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.PeriodStart >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.PeriodEnd <= toDate.Value);
            }

            var schedules = await query.OrderBy(s => s.ScheduleNumber).ToListAsync();

            // Get payment status for each schedule
            var scheduleIds = schedules.Select(s => s.ScheduleId).ToList();
            var paymentStatuses = await Db.Queryable<ErpBillingPaymentStatus>()
                .Where(p => scheduleIds.Contains(p.ScheduleId))
                .ToListAsync();

            return schedules.Select(s =>
            {
                var payment = paymentStatuses.FirstOrDefault(p => p.ScheduleId == s.ScheduleId);
                return new BillingTimelineEntry
                {
                    ScheduleId = s.ScheduleId,
                    ScheduleNumber = s.ScheduleNumber,
                    PeriodStart = s.PeriodStart,
                    PeriodEnd = s.PeriodEnd,
                    DueDate = s.DueDate,
                    Amount = s.TotalAmount,
                    Status = s.Status.ToString(),
                    InvoiceNumber = s.InvoiceNumber,
                    InvoiceDate = s.InvoicedDate,
                    PaidDate = payment?.LastPaymentDate,
                    PaidAmount = payment?.PaidAmount ?? 0
                };
            }).ToList();
        }

        public async Task<List<OverdueInvoiceInfo>> GetOverdueInvoicesAsync(int minDaysOverdue, Guid? customerId)
        {
            var cutoffDate = DateTime.Today.AddDays(-minDaysOverdue);

            var query = Db.Queryable<ErpContractBillingSchedule, ErpContract>(
                (s, c) => new JoinQueryInfos(
                    JoinType.Inner, s.ContractId == c.ContractId
                ))
                .Where((s, c) => (s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Due)
                    && s.PaymentDueDate <= cutoffDate);

            if (customerId.HasValue)
            {
                query = query.Where((s, c) => c.CustomerId == customerId.Value);
            }

            var overdueItems = await query.Select((s, c) => new
            {
                s.ScheduleId,
                s.ContractId,
                c.ContractNumber,
                c.CustomerId,
                s.InvoiceNumber,
                s.PaymentDueDate,
                s.TotalAmount
            }).ToListAsync();

            var customerIds = overdueItems.Select(o => o.CustomerId).Distinct().ToList();
            var customerNames = await GetCustomerNamesAsync(customerIds);

            var today = DateTime.Today;
            return overdueItems.Select(item => new OverdueInvoiceInfo
            {
                ScheduleId = item.ScheduleId,
                ContractId = item.ContractId,
                ContractNumber = item.ContractNumber,
                CustomerId = item.CustomerId,
                CustomerName = customerNames.GetValueOrDefault(item.CustomerId, "Unknown"),
                InvoiceNumber = item.InvoiceNumber,
                DueDate = item.PaymentDueDate,
                DaysOverdue = (today - item.PaymentDueDate).Days,
                AmountDue = item.TotalAmount,
                AmountPaid = 0, // Would come from payment status
                AmountOutstanding = item.TotalAmount
            }).OrderByDescending(i => i.DaysOverdue).ToList();
        }

        public async Task<RevenueForecast> GetRevenueForecastAsync(DateTime fromDate, DateTime toDate, Guid? contractId)
        {
            var forecast = new RevenueForecast
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var query = Db.Queryable<ErpContractBillingSchedule, ErpContract>(
                (s, c) => new JoinQueryInfos(
                    JoinType.Inner, s.ContractId == c.ContractId
                ))
                .Where((s, c) => s.DueDate >= fromDate && s.DueDate <= toDate
                    && s.Status != BillingScheduleStatus.Cancelled && s.Status != BillingScheduleStatus.Skipped);

            if (contractId.HasValue)
            {
                query = query.Where((s, c) => s.ContractId == contractId.Value);
            }

            var schedules = await query.Select((s, c) => new
            {
                s.ScheduleId,
                s.ContractId,
                c.ContractNumber,
                c.CustomerId,
                s.DueDate,
                s.TotalAmount,
                s.Status
            }).ToListAsync();

            // Get customer names
            var customerIds = schedules.Select(s => s.CustomerId).Distinct().ToList();
            var customerNames = await GetCustomerNamesAsync(customerIds);

            forecast.TotalForecastedRevenue = schedules.Sum(s => s.TotalAmount);
            forecast.ConfirmedRevenue = schedules.Where(s => s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid)
                .Sum(s => s.TotalAmount);
            forecast.ProjectedRevenue = schedules.Where(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due)
                .Sum(s => s.TotalAmount);

            // Monthly breakdown
            var monthlyGroups = schedules.GroupBy(s => new { s.DueDate.Year, s.DueDate.Month });
            foreach (var group in monthlyGroups.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month))
            {
                forecast.MonthlyBreakdown.Add(new MonthlyRevenueForecast
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Key.Month),
                    ForecastedAmount = group.Sum(s => s.TotalAmount),
                    ConfirmedAmount = group.Where(s => s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid)
                        .Sum(s => s.TotalAmount),
                    ProjectedAmount = group.Where(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due)
                        .Sum(s => s.TotalAmount),
                    ScheduleCount = group.Count()
                });
            }

            // Contract breakdown
            var contractGroups = schedules.GroupBy(s => new { s.ContractId, s.ContractNumber, s.CustomerId });
            foreach (var group in contractGroups.OrderByDescending(g => g.Sum(s => s.TotalAmount)))
            {
                forecast.ContractBreakdown.Add(new ContractRevenueForecast
                {
                    ContractId = group.Key.ContractId,
                    ContractNumber = group.Key.ContractNumber,
                    CustomerName = customerNames.GetValueOrDefault(group.Key.CustomerId, "Unknown"),
                    ForecastedAmount = group.Sum(s => s.TotalAmount),
                    ScheduleCount = group.Count()
                });
            }

            return forecast;
        }

        #endregion

        #region Private Methods

        private string GenerateInvoiceNumber(string contractNumber, int scheduleNumber)
        {
            return $"INV-{contractNumber}-{scheduleNumber:D4}";
        }

        private async Task<string> GetCustomerNameAsync(Guid customerId)
        {
            // This would typically query your customer/business partner table
            // For now, return a placeholder
            return $"Customer-{customerId.ToString().Substring(0, 8)}";
        }

        private async Task<Dictionary<Guid, string>> GetCustomerNamesAsync(List<Guid> customerIds)
        {
            // This would typically query your customer/business partner table
            // For now, return placeholders
            return customerIds.ToDictionary(
                id => id,
                id => $"Customer-{id.ToString().Substring(0, 8)}"
            );
        }

        #endregion
    }
}
