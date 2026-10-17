using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Microsoft.Extensions.Logging;
using SqlSugar;
using ZR.Model;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Customers;
using ZR.Model.Finance.GL;
using ZR.Repository.Contracts;
using ZR.Service.Contracts.IService;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;
using ZR.ServiceCore;

namespace ZR.Service.Contracts
{
    /// <summary>
    /// Contract Invoice Service - خدمة فواتير العقود
    /// ORCHESTRATION SERVICE ONLY - Does NOT duplicate Finance logic
    ///
    /// Responsibilities:
    /// 1. Validate billing schedule data
    /// 2. Map contract data to existing Finance DTOs
    /// 3. Call existing Finance services (ARInvoiceService)
    /// 4. Update billing schedule status atomically
    ///
    /// This service does NOT:
    /// - Calculate accounting values
    /// - Implement GL logic
    /// - Apply tax rules manually
    /// </summary>
    [AppService(ServiceType = typeof(IContractInvoiceService), ServiceLifetime = LifeTime.Transient)]
    public class ContractInvoiceService : BaseService<ErpContractBillingSchedule>, IContractInvoiceService
    {
        private readonly IContractBillingScheduleRepository _scheduleRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IARInvoiceService _arInvoiceService;
        private readonly ILogger<ContractInvoiceService> _logger;

        public ContractInvoiceService(
            IContractBillingScheduleRepository scheduleRepository,
            IContractRepository contractRepository,
            IARInvoiceService arInvoiceService,
            ILogger<ContractInvoiceService> logger)
        {
            _scheduleRepository = scheduleRepository;
            _contractRepository = contractRepository;
            _arInvoiceService = arInvoiceService;
            _logger = logger;
        }

        #region Invoice Creation

        /// <summary>
        /// Create AR Invoice from a single billing schedule
        /// CRITICAL: Full transactional integrity required
        /// </summary>
        public async Task<CreateInvoiceFromScheduleResultDto> CreateInvoiceFromScheduleAsync(
            CreateInvoiceFromScheduleRequestDto request,
            string userName)
        {
            var result = new CreateInvoiceFromScheduleResultDto
            {
                ScheduleId = request.ScheduleId,
                Success = false,
                Warnings = new List<string>()
            };

            try
            {
                _logger.LogInformation("Starting invoice creation for schedule {ScheduleId}", request.ScheduleId);

                // ============================================================
                // STEP 1: IDEMPOTENCY CHECK - Prevent duplicate invoice creation
                // ============================================================
                var existingSchedule = await Context.Queryable<ErpContractBillingSchedule>()
                    .Where(s => s.ScheduleId == request.ScheduleId)
                    .FirstAsync();

                if (existingSchedule == null)
                {
                    result.ErrorMessage = "Billing schedule not found - جدول الفوترة غير موجود";
                    _logger.LogWarning("Schedule {ScheduleId} not found", request.ScheduleId);
                    return result;
                }

                // Idempotency: Already invoiced - return existing invoice info
                if (existingSchedule.InvoiceId.HasValue)
                {
                    result.Success = true;
                    result.InvoiceId = existingSchedule.InvoiceId;
                    result.InvoiceNumber = existingSchedule.InvoiceNumber;
                    result.Warnings.Add("Schedule already invoiced - تم فوترة الجدول مسبقاً");
                    _logger.LogInformation("Schedule {ScheduleId} already invoiced as {InvoiceNumber}",
                        request.ScheduleId, existingSchedule.InvoiceNumber);
                    return result;
                }

                // ============================================================
                // STEP 2: PRE-VALIDATION (Before starting transaction)
                // ============================================================
                var validationResult = await ValidateScheduleForInvoicingInternalAsync(existingSchedule);
                if (!validationResult.IsValid)
                {
                    result.ErrorMessage = string.Join("; ", validationResult.Errors);
                    _logger.LogWarning("Validation failed for schedule {ScheduleId}: {Errors}",
                        request.ScheduleId, result.ErrorMessage);
                    return result;
                }
                result.Warnings = validationResult.Warnings;

                // ============================================================
                // STEP 3: Load contract and financial data
                // ============================================================
                var contract = await Context.Queryable<ErpContract>()
                    .Includes(c => c.Financial)
                    .Where(c => c.ContractId == existingSchedule.ContractId)
                    .FirstAsync();

                if (contract == null)
                {
                    result.ErrorMessage = "Contract not found - العقد غير موجود";
                    return result;
                }

                // Load schedule lines
                var scheduleLines = await Context.Queryable<ErpContractBillingScheduleLine>()
                    .Where(l => l.ScheduleId == request.ScheduleId)
                    .OrderBy(l => l.LineNumber)
                    .ToListAsync();

                result.ContractId = contract.ContractId;
                result.ContractNumber = contract.ContractNumber;
                result.CustomerId = contract.CustomerId;

                // ============================================================
                // STEP 4: BEGIN ATOMIC TRANSACTION
                // ============================================================
                Context.Ado.BeginTran();
                try
                {
                    // STEP 4.1: Lock schedule row (pessimistic lock via re-read with lock)
                    var lockedSchedule = await Context.Queryable<ErpContractBillingSchedule>()
                        .With(SqlWith.UpdLock) // Pessimistic lock
                        .Where(s => s.ScheduleId == request.ScheduleId)
                        .Where(s => s.InvoiceId == null) // Double-check not invoiced
                        .Where(s => s.Status == BillingScheduleStatus.Pending ||
                                   s.Status == BillingScheduleStatus.Due)
                        .FirstAsync();

                    if (lockedSchedule == null)
                    {
                        Context.Ado.RollbackTran();
                        result.ErrorMessage = "Schedule no longer available for invoicing (may have been invoiced by another process) - الجدول لم يعد متاحاً للفوترة";
                        _logger.LogWarning("Schedule {ScheduleId} locked/unavailable - concurrent invoicing detected", request.ScheduleId);
                        return result;
                    }

                    // STEP 4.2: Map contract data to Finance DTO
                    var invoiceDto = MapScheduleToInvoiceDto(contract, lockedSchedule, scheduleLines, request, userName);

                    // STEP 4.3: Call existing Finance service to create invoice
                    // ARInvoiceService.Create() handles all Finance logic internally
                    ARInvoice createdInvoice;
                    try
                    {
                        createdInvoice = _arInvoiceService.Create(invoiceDto, userName);
                    }
                    catch (Exception createEx)
                    {
                        Context.Ado.RollbackTran();
                        result.ErrorMessage = $"Invoice creation failed: {createEx.Message} - فشل إنشاء الفاتورة";
                        _logger.LogError(createEx, "ARInvoiceService.Create failed for schedule {ScheduleId}", request.ScheduleId);
                        return result;
                    }

                    // STEP 4.4: Update billing schedule status (IN SAME TRANSACTION)
                    var updateResult = await Context.Updateable<ErpContractBillingSchedule>()
                        .SetColumns(s => new ErpContractBillingSchedule
                        {
                            Status = BillingScheduleStatus.Invoiced,
                            InvoiceId = createdInvoice.Id,
                            InvoiceNumber = createdInvoice.InvoiceNumber,
                            InvoicedDate = DateTime.Now,
                            InvoicedBy = userName,
                            Update_time = DateTime.Now,
                            Update_by = userName
                        })
                        .Where(s => s.ScheduleId == lockedSchedule.ScheduleId)
                        .ExecuteCommandAsync();

                    if (updateResult <= 0)
                    {
                        Context.Ado.RollbackTran();
                        result.ErrorMessage = "Failed to update schedule status - فشل تحديث حالة الجدول";
                        _logger.LogError("Failed to update schedule {ScheduleId} after invoice creation", request.ScheduleId);
                        return result;
                    }

                    // STEP 4.5: COMMIT TRANSACTION
                    Context.Ado.CommitTran();
                    _logger.LogInformation("Invoice {InvoiceNumber} created successfully for schedule {ScheduleId}",
                        createdInvoice.InvoiceNumber, request.ScheduleId);

                    // Populate result
                    result.Success = true;
                    result.InvoiceId = createdInvoice.Id;
                    result.InvoiceNumber = createdInvoice.InvoiceNumber;
                    result.InvoiceDate = createdInvoice.InvoiceDate;
                    result.DueDate = createdInvoice.DueDate;
                    result.TotalAmount = createdInvoice.TotalAmount;
                    result.Currency = createdInvoice.Currency;
                    result.BillingPeriod = $"{lockedSchedule.PeriodStart:MMM yyyy}";

                    // STEP 5: POST-TRANSACTION OPERATIONS (optional, non-critical)
                    if (request.AutoPost && createdInvoice.Status == InvoiceStatus.Draft)
                    {
                        try
                        {
                            var postResult = _arInvoiceService.PostInvoice(createdInvoice.Id, userName);
                            result.IsPosted = postResult.Success;
                            result.GLJournalId = postResult.JournalId;
                            if (!postResult.Success)
                            {
                                result.Warnings.Add($"Invoice created but GL posting failed: {postResult.Message}");
                                _logger.LogWarning("GL posting failed for invoice {InvoiceId}: {Message}",
                                    createdInvoice.Id, postResult.Message);
                            }
                        }
                        catch (Exception postEx)
                        {
                            result.Warnings.Add($"Invoice created but GL posting failed: {postEx.Message}");
                            _logger.LogError(postEx, "GL posting failed for invoice {InvoiceId}", createdInvoice.Id);
                        }
                    }

                    return result;
                }
                catch (Exception txEx)
                {
                    Context.Ado.RollbackTran();
                    result.ErrorMessage = $"Transaction failed: {txEx.Message}";
                    _logger.LogError(txEx, "Transaction failed for schedule {ScheduleId}", request.ScheduleId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
                _logger.LogError(ex, "Unexpected error creating invoice from schedule {ScheduleId}", request.ScheduleId);
                return result;
            }
        }

        /// <summary>
        /// Bulk create invoices from multiple schedules
        /// </summary>
        public async Task<BulkCreateInvoicesResultDto> BulkCreateInvoicesFromSchedulesAsync(
            BulkCreateInvoicesFromSchedulesRequestDto request,
            string userName)
        {
            var result = new BulkCreateInvoicesResultDto
            {
                TotalRequested = request.ScheduleIds?.Count ?? 0,
                Results = new List<CreateInvoiceFromScheduleResultDto>(),
                Errors = new List<BulkInvoiceErrorDto>()
            };

            if (request.ScheduleIds == null || !request.ScheduleIds.Any())
            {
                return result;
            }

            _logger.LogInformation("Starting bulk invoice creation for {Count} schedules", request.ScheduleIds.Count);

            foreach (var scheduleId in request.ScheduleIds)
            {
                var singleRequest = new CreateInvoiceFromScheduleRequestDto
                {
                    ScheduleId = scheduleId,
                    InvoiceDate = request.InvoiceDate,
                    AutoPost = request.AutoPost,
                    SubmitToZatca = request.SubmitToZatca,
                    SendToCustomer = request.SendToCustomer
                };

                try
                {
                    var invoiceResult = await CreateInvoiceFromScheduleAsync(singleRequest, userName);
                    result.Results.Add(invoiceResult);

                    if (invoiceResult.Success)
                    {
                        result.TotalSuccess++;
                        result.TotalAmount += invoiceResult.TotalAmount;
                        result.Currency = invoiceResult.Currency; // Use last currency
                    }
                    else
                    {
                        result.TotalFailed++;
                        result.Errors.Add(new BulkInvoiceErrorDto
                        {
                            ScheduleId = scheduleId,
                            ContractNumber = invoiceResult.ContractNumber,
                            ErrorMessage = invoiceResult.ErrorMessage,
                            ErrorCode = "INVOICE_CREATION_FAILED"
                        });

                        if (request.StopOnError)
                        {
                            _logger.LogWarning("Bulk invoicing stopped on error at schedule {ScheduleId}", scheduleId);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.TotalFailed++;
                    result.Errors.Add(new BulkInvoiceErrorDto
                    {
                        ScheduleId = scheduleId,
                        ErrorMessage = ex.Message,
                        ErrorCode = "EXCEPTION"
                    });
                    _logger.LogError(ex, "Exception during bulk invoice creation for schedule {ScheduleId}", scheduleId);

                    if (request.StopOnError)
                    {
                        break;
                    }
                }
            }

            _logger.LogInformation("Bulk invoice creation completed: {Success}/{Total} successful",
                result.TotalSuccess, result.TotalRequested);

            return result;
        }

        /// <summary>
        /// Auto-generate invoices for all due billing schedules
        /// </summary>
        public async Task<BulkCreateInvoicesResultDto> AutoGenerateDueInvoicesAsync(
            DateTime? asOfDate,
            bool autoPost,
            Guid? contractId,
            string userName)
        {
            var effectiveDate = asOfDate ?? DateTime.Today;
            _logger.LogInformation("Auto-generating invoices for schedules due by {Date}", effectiveDate);

            // Find all due schedules
            var dueSchedules = await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.DueDate <= effectiveDate)
                .Where(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due)
                .Where(s => s.InvoiceId == null)
                .WhereIF(contractId.HasValue, s => s.ContractId == contractId.Value)
                .Select(s => s.ScheduleId)
                .ToListAsync();

            if (!dueSchedules.Any())
            {
                _logger.LogInformation("No due schedules found for auto-generation");
                return new BulkCreateInvoicesResultDto
                {
                    TotalRequested = 0,
                    TotalSuccess = 0,
                    TotalFailed = 0,
                    Results = new List<CreateInvoiceFromScheduleResultDto>(),
                    Errors = new List<BulkInvoiceErrorDto>()
                };
            }

            var request = new BulkCreateInvoicesFromSchedulesRequestDto
            {
                ScheduleIds = dueSchedules,
                AutoPost = autoPost,
                StopOnError = false // Continue processing all due schedules
            };

            return await BulkCreateInvoicesFromSchedulesAsync(request, userName);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Public validation method
        /// </summary>
        public async Task<ScheduleInvoiceValidationResultDto> ValidateScheduleForInvoicingAsync(Guid scheduleId)
        {
            var schedule = await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ScheduleId == scheduleId)
                .FirstAsync();

            if (schedule == null)
            {
                return new ScheduleInvoiceValidationResultDto
                {
                    IsValid = false,
                    Errors = new List<string> { "Schedule not found - جدول الفوترة غير موجود" }
                };
            }

            return await ValidateScheduleForInvoicingInternalAsync(schedule);
        }

        /// <summary>
        /// Validate multiple schedules
        /// </summary>
        public async Task<Dictionary<Guid, ScheduleInvoiceValidationResultDto>> ValidateSchedulesForInvoicingAsync(
            List<Guid> scheduleIds)
        {
            var results = new Dictionary<Guid, ScheduleInvoiceValidationResultDto>();

            foreach (var scheduleId in scheduleIds)
            {
                results[scheduleId] = await ValidateScheduleForInvoicingAsync(scheduleId);
            }

            return results;
        }

        /// <summary>
        /// Internal validation logic - MUST validate BEFORE creating invoice
        /// </summary>
        private async Task<ScheduleInvoiceValidationResultDto> ValidateScheduleForInvoicingInternalAsync(
            ErpContractBillingSchedule schedule)
        {
            var result = new ScheduleInvoiceValidationResultDto
            {
                IsValid = true,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            // 1. Check schedule status
            if (schedule.Status == BillingScheduleStatus.Invoiced)
            {
                result.IsValid = false;
                result.Errors.Add("Schedule is already invoiced - الجدول مفوتر مسبقاً");
                return result;
            }

            if (schedule.Status == BillingScheduleStatus.Cancelled)
            {
                result.IsValid = false;
                result.Errors.Add("Cannot invoice cancelled schedule - لا يمكن فوترة جدول ملغي");
                return result;
            }

            if (schedule.Status == BillingScheduleStatus.Skipped)
            {
                result.IsValid = false;
                result.Errors.Add("Cannot invoice skipped schedule - لا يمكن فوترة جدول متخطى");
                return result;
            }

            // 2. Check schedule already has invoice
            if (schedule.InvoiceId.HasValue)
            {
                result.IsValid = false;
                result.Errors.Add($"Schedule already linked to invoice {schedule.InvoiceNumber} - الجدول مرتبط بفاتورة مسبقاً");
                return result;
            }

            // 3. Check total amount
            if (schedule.TotalAmount <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Schedule total amount must be greater than zero - مبلغ الجدول يجب أن يكون أكبر من صفر");
                return result;
            }

            // 4. Load and validate contract
            var contract = await Context.Queryable<ErpContract>()
                .Includes(c => c.Financial)
                .Where(c => c.ContractId == schedule.ContractId)
                .FirstAsync();

            if (contract == null)
            {
                result.IsValid = false;
                result.Errors.Add("Contract not found - العقد غير موجود");
                return result;
            }

            if (contract.Status != ContractStatus.Active)
            {
                result.IsValid = false;
                result.Errors.Add($"Contract is not active (status: {contract.Status}) - العقد غير نشط");
                return result;
            }

            // 5. Validate customer exists
            var customerExists = await Context.Queryable<Customer>()
                .Where(c => c.Id == contract.CustomerId)
                .AnyAsync();

            if (!customerExists)
            {
                result.IsValid = false;
                result.Errors.Add("Customer not found - العميل غير موجود");
                return result;
            }

            // 6. Check currency
            if (string.IsNullOrEmpty(schedule.Currency))
            {
                result.IsValid = false;
                result.Errors.Add("Schedule currency is required - عملة الجدول مطلوبة");
                return result;
            }

            // 7. Load and validate schedule lines
            var lines = await Context.Queryable<ErpContractBillingScheduleLine>()
                .Where(l => l.ScheduleId == schedule.ScheduleId)
                .ToListAsync();

            if (lines == null || !lines.Any())
            {
                // Warning only - schedule might have header-only amount
                result.Warnings.Add("Schedule has no line items - will use header amount - الجدول بدون بنود تفصيلية");
            }
            else
            {
                // Validate each line has required GL account
                foreach (var line in lines)
                {
                    if (!line.RevenueAccountId.HasValue)
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Line {line.LineNumber} missing revenue account - البند {line.LineNumber} بدون حساب إيرادات");
                    }
                }
            }

            // 8. Warning for future-dated schedules
            if (schedule.DueDate > DateTime.Today)
            {
                result.Warnings.Add($"Schedule is not yet due (due: {schedule.DueDate:yyyy-MM-dd}) - الجدول لم يحن موعده بعد");
            }

            return result;
        }

        #endregion

        #region Void/Cancel

        /// <summary>
        /// Void an invoice and optionally reopen the billing schedule
        /// </summary>
        public async Task<int> VoidContractInvoiceAsync(VoidContractInvoiceRequestDto request, string userName)
        {
            _logger.LogInformation("Voiding invoice {InvoiceId}, reopen schedule: {Reopen}",
                request.InvoiceId, request.ReopenSchedule);

            // Find the billing schedule linked to this invoice
            var schedule = await Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.InvoiceId == request.InvoiceId)
                .FirstAsync();

            Context.Ado.BeginTran();
            try
            {
                // Cancel the invoice using existing Finance service
                var cancelResult = _arInvoiceService.CancelInvoice(request.InvoiceId, request.VoidReason, userName);

                // Reopen the billing schedule if requested
                if (request.ReopenSchedule && schedule != null)
                {
                    await Context.Updateable<ErpContractBillingSchedule>()
                        .SetColumns(s => new ErpContractBillingSchedule
                        {
                            Status = BillingScheduleStatus.Pending,
                            InvoiceId = null,
                            InvoiceNumber = null,
                            InvoicedDate = null,
                            InvoicedBy = null,
                            Notes = $"{schedule.Notes}\nVoided: {request.VoidReason} by {userName} at {DateTime.Now:yyyy-MM-dd HH:mm}",
                            Update_time = DateTime.Now,
                            Update_by = userName
                        })
                        .Where(s => s.ScheduleId == schedule.ScheduleId)
                        .ExecuteCommandAsync();

                    _logger.LogInformation("Schedule {ScheduleId} reopened after invoice void", schedule.ScheduleId);
                }

                Context.Ado.CommitTran();
                return cancelResult;
            }
            catch (Exception ex)
            {
                Context.Ado.RollbackTran();
                _logger.LogError(ex, "Failed to void invoice {InvoiceId}", request.InvoiceId);
                throw;
            }
        }

        #endregion

        #region Query

        /// <summary>
        /// Get invoices by contract ID
        /// </summary>
        public List<ContractInvoiceListDto> GetInvoicesByContractId(Guid contractId)
        {
            // Get all schedule IDs for this contract
            var scheduleInvoiceIds = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .Where(s => s.InvoiceId != null)
                .Select(s => s.InvoiceId)
                .ToList();

            if (!scheduleInvoiceIds.Any())
            {
                return new List<ContractInvoiceListDto>();
            }

            // Get invoices with contract info
            var invoices = Context.Queryable<ARInvoice>()
                .Where(i => scheduleInvoiceIds.Contains(i.Id))
                .OrderByDescending(i => i.InvoiceDate)
                .ToList();

            var contract = _contractRepository.GetEntityById(contractId);
            var customer = Context.Queryable<Customer>()
                .Where(c => c.Id == contract.CustomerId)
                .First();

            return invoices.Select(inv => new ContractInvoiceListDto
            {
                InvoiceId = inv.Id,
                InvoiceNumber = inv.InvoiceNumber,
                ContractId = contractId,
                ContractNumber = contract?.ContractNumber,
                CustomerId = inv.CustomerId,
                CustomerName = customer?.NameEn,
                InvoiceDate = inv.InvoiceDate,
                DueDate = inv.DueDate,
                Subtotal = inv.Subtotal,
                TaxAmount = inv.TaxAmount,
                TotalAmount = inv.TotalAmount,
                PaidAmount = inv.PaidAmount,
                RemainingAmount = inv.RemainingAmount,
                Currency = inv.Currency,
                Status = (int)inv.Status,
                StatusName = inv.Status.ToString(),
                IsRecurring = true,
                Create_time = inv.Create_time
            }).ToList();
        }

        /// <summary>
        /// Get paginated list of contract invoices
        /// </summary>
        public PagedInfo<ContractInvoiceListDto> GetContractInvoices(ContractInvoiceQueryDto query, PagerInfo pager)
        {
            // Get invoices that are linked to billing schedules
            var invoiceQuery = Context.Queryable<ARInvoice, ErpContractBillingSchedule, ErpContract>(
                (inv, sch, con) => new JoinQueryInfos(
                    JoinType.Inner, inv.Id == sch.InvoiceId,
                    JoinType.Inner, sch.ContractId == con.ContractId
                ))
                .WhereIF(query.InvoiceId.HasValue, (inv, sch, con) => inv.Id == query.InvoiceId)
                .WhereIF(!string.IsNullOrEmpty(query.InvoiceNumber), (inv, sch, con) => inv.InvoiceNumber.Contains(query.InvoiceNumber))
                .WhereIF(query.ContractId.HasValue, (inv, sch, con) => con.ContractId == query.ContractId)
                .WhereIF(!string.IsNullOrEmpty(query.ContractNumber), (inv, sch, con) => con.ContractNumber.Contains(query.ContractNumber))
                .WhereIF(query.CustomerId.HasValue, (inv, sch, con) => inv.CustomerId == query.CustomerId)
                .WhereIF(query.InvoiceStatus.HasValue, (inv, sch, con) => (int)inv.Status == query.InvoiceStatus)
                .WhereIF(query.InvoiceDateFrom.HasValue, (inv, sch, con) => inv.InvoiceDate >= query.InvoiceDateFrom)
                .WhereIF(query.InvoiceDateTo.HasValue, (inv, sch, con) => inv.InvoiceDate <= query.InvoiceDateTo)
                .WhereIF(query.BillingPeriodFrom.HasValue, (inv, sch, con) => sch.PeriodStart >= query.BillingPeriodFrom)
                .WhereIF(query.BillingPeriodTo.HasValue, (inv, sch, con) => sch.PeriodEnd <= query.BillingPeriodTo)
                .Select((inv, sch, con) => new ContractInvoiceListDto
                {
                    InvoiceId = inv.Id,
                    InvoiceNumber = inv.InvoiceNumber,
                    ContractId = con.ContractId,
                    ContractNumber = con.ContractNumber,
                    CustomerId = inv.CustomerId,
                    InvoiceDate = inv.InvoiceDate,
                    DueDate = inv.DueDate,
                    BillingPeriodStart = sch.PeriodStart,
                    BillingPeriodEnd = sch.PeriodEnd,
                    Subtotal = inv.Subtotal,
                    TaxAmount = inv.TaxAmount,
                    TotalAmount = inv.TotalAmount,
                    PaidAmount = inv.PaidAmount,
                    RemainingAmount = inv.RemainingAmount,
                    Currency = inv.Currency,
                    Status = (int)inv.Status,
                    IsRecurring = true,
                    Create_time = inv.Create_time
                })
                .OrderByDescending(x => x.InvoiceDate);

            // Use local variable for ref parameter (properties cannot be passed by ref)
            var totalCount = 0;
            var pagedResult = invoiceQuery.ToPageList(pager.PageNum, pager.PageSize, ref totalCount);

            // Return paginated result
            return new PagedInfo<ContractInvoiceListDto>
            {
                TotalNum = totalCount,
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize,
                Result = pagedResult
            };
        }

        #endregion

        #region Private Mapping Methods

        /// <summary>
        /// Map billing schedule to AR Invoice DTO
        /// CRITICAL: Uses EXISTING Finance DTOs - no new fields
        /// </summary>
        private CreateARInvoiceDto MapScheduleToInvoiceDto(
            ErpContract contract,
            ErpContractBillingSchedule schedule,
            List<ErpContractBillingScheduleLine> scheduleLines,
            CreateInvoiceFromScheduleRequestDto request,
            string userName)
        {
            // Get next invoice number from Finance service
            var invoiceNumber = _arInvoiceService.GetNextInvoiceNumber(contract.CompanyID);

            // Determine dates
            var invoiceDate = request.InvoiceDate ?? schedule.DueDate;
            var dueDate = request.OverrideDueDate ?? schedule.PaymentDueDate;

            // Get financial settings
            var financial = contract.Financial;
            var currency = schedule.Currency ?? financial?.Currency ?? "SAR";
            var taxGroupId = financial?.TaxGroupId;

            // Build invoice lines from schedule lines
            var invoiceLines = new List<CreateARInvoiceLineDto>();

            if (scheduleLines != null && scheduleLines.Any())
            {
                foreach (var line in scheduleLines)
                {
                    invoiceLines.Add(new CreateARInvoiceLineDto
                    {
                        AccountId = line.RevenueAccountId ?? Guid.Empty,
                        Description = line.Description ?? $"Contract {contract.ContractNumber} - {schedule.PeriodStart:MMM yyyy}",
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice,
                        TaxCodeId = line.TaxGroupId,
                        CostCenterId = line.CostCenterId
                    });
                }
            }
            else
            {
                // No lines - create single line from schedule header
                // Use default revenue account (must be configured)
                var defaultRevenueAccountId = GetDefaultRevenueAccountId(contract);

                invoiceLines.Add(new CreateARInvoiceLineDto
                {
                    AccountId = defaultRevenueAccountId,
                    Description = $"Contract {contract.ContractNumber} - Period {schedule.PeriodStart:MMM yyyy} to {schedule.PeriodEnd:MMM yyyy}",
                    Quantity = 1,
                    UnitPrice = schedule.Subtotal,
                    TaxCodeId = taxGroupId,
                    CostCenterId = null
                });
            }

            // Build reference number from contract and schedule
            var referenceNumber = $"{contract.ContractNumber}-{schedule.ScheduleNumber:D4}";

            // Build description
            var description = !string.IsNullOrEmpty(schedule.Description)
                ? schedule.Description
                : $"{contract.ContractTitle ?? "Contract"} - Billing Period {schedule.PeriodStart:MMM yyyy}";

            // Build notes
            var notes = request.Notes ?? schedule.Notes ?? "";
            notes = $"Contract: {contract.ContractNumber}\nSchedule #: {schedule.ScheduleNumber}\nPeriod: {schedule.PeriodStart:yyyy-MM-dd} to {schedule.PeriodEnd:yyyy-MM-dd}\n{notes}";

            return new CreateARInvoiceDto
            {
                InvoiceNumber = invoiceNumber,
                CustomerId = contract.CustomerId,
                InvoiceDate = invoiceDate,
                DueDate = dueDate,
                Currency = currency,
                ExchangeRate = 1, // Assuming SAR base
                TaxGroupId = taxGroupId,
                IsTaxInclusive = financial?.IsTaxInclusive ?? false,
                Description = description,
                ReferenceNumber = referenceNumber,
                Notes = notes,
                Lines = invoiceLines,
                Subtotal = schedule.Subtotal,
                TotalAmount = schedule.TotalAmount,
                SkipCreditLimitCheck = true // Contract itself is the credit approval
            };
        }

        /// <summary>
        /// Get default revenue account for contract invoicing
        /// </summary>
        private Guid GetDefaultRevenueAccountId(ErpContract contract)
        {
            // Try to find a default revenue account
            // This should be configured in system settings
            var defaultAccount = Context.Queryable<GLAccount>()
                .Where(a => a.IsLeaf && a.IsActive)
                .Where(a => a.AccountNumber.StartsWith("41") || // Revenue accounts typically start with 4
                           a.AccountNameEn.Contains("Revenue") ||
                           a.AccountNameEn.Contains("Sales"))
                .First();

            if (defaultAccount != null)
            {
                return defaultAccount.AccountId;
            }

            // If no default found, throw - this is a configuration error
            throw new InvalidOperationException(
                "No default revenue account configured. Please configure a revenue account or add line items to billing schedule. " +
                "لم يتم تكوين حساب إيرادات افتراضي");
        }

        #endregion
    }
}
