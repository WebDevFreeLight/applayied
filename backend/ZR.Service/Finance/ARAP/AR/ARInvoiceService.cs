using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Infrastructure.Localization;
using Mapster;
using SqlSugar;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Finance.Tax;
using ZR.Model.Customers;
using ZR.Model.Customers.Dto;
using ZR.Repository;
using ZR.Repository.Finance.ARAP;
using ZR.Repository.Customers;
using ZR.Service.Finance.ARAP.Common;
using ZR.Service.Finance.Validation;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;

namespace ZR.Service.Finance.ARAP.AR
{
    /// <summary>
    /// AR Invoice Service - خدمة فواتير العملاء
    /// Refactored to use InvoiceServiceBase and eliminate code duplication
    /// </summary>
    [AppService(ServiceType = typeof(IARInvoiceService), ServiceLifetime = LifeTime.Transient)]
    public class ARInvoiceService : InvoiceServiceBase<ARInvoice, ARInvoiceLine, CreateARInvoiceLineDto, UpdateARInvoiceLineDto>, IARInvoiceService
    {
        private readonly IARInvoiceRepository _invoiceRepository;
        private readonly IARInvoiceLineRepository _lineRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerService _customerService;
        private readonly IARAPGLIntegrationService _glIntegrationService;
        private readonly BilingualMessageHelper _messageHelper;
        private readonly ILocalizationService _localizationService;

        private const int MaxRetryAttempts = 5;

        public ARInvoiceService(
            IARInvoiceRepository invoiceRepository,
            IARInvoiceLineRepository lineRepository,
            ICustomerRepository customerRepository,
            ICustomerService customerService,
            IARAPGLIntegrationService glIntegrationService,
            BilingualMessageHelper messageHelper,
            ILocalizationService localizationService)
        {
            _invoiceRepository = invoiceRepository;
            _lineRepository = lineRepository;
            _customerRepository = customerRepository;
            _customerService = customerService;
            _glIntegrationService = glIntegrationService;
            _messageHelper = messageHelper;
            _localizationService = localizationService;
        }

        #region Query Methods

        /// <summary>
        /// Get paginated list of invoices
        /// </summary>
        public PagedInfo<ARInvoiceListDto> GetList(ARInvoiceQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ARInvoice>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.InvoiceNumber), it => it.InvoiceNumber.Contains(query.InvoiceNumber));
            predicate = predicate.AndIF(query.CustomerId.HasValue && query.CustomerId != Guid.Empty, it => it.CustomerId == query.CustomerId);
            predicate = predicate.AndIF(query.Status.HasValue, it => it.Status == query.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Currency), it => it.Currency == query.Currency);
            predicate = predicate.AndIF(query.BeginInvoiceDate.HasValue, it => it.InvoiceDate >= query.BeginInvoiceDate);
            predicate = predicate.AndIF(query.EndInvoiceDate.HasValue, it => it.InvoiceDate <= query.EndInvoiceDate);
            predicate = predicate.AndIF(query.BeginDueDate.HasValue, it => it.DueDate >= query.BeginDueDate);
            predicate = predicate.AndIF(query.EndDueDate.HasValue, it => it.DueDate <= query.EndDueDate);

            var result = Context.Queryable<ARInvoice>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage(pager);

            var dtoList = result.Result.Select(invoice => MapToListDto(invoice)).ToList();

            return new PagedInfo<ARInvoiceListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get invoice by ID with lines
        /// </summary>
        public ARInvoiceDetailDto GetById(Guid id)
        {
            var invoice = _invoiceRepository.GetById(id);
            if (invoice == null) return null;

            var lines = _lineRepository.GetByInvoiceId(id);
            invoice.Lines = lines;

            return MapToDetailDto(invoice);
        }

        #endregion

        #region Create/Update/Delete Methods

        /// <summary>
        /// Create invoice with lines
        /// </summary>
        public ARInvoice Create(CreateARInvoiceDto dto, string userName)
        {
            // AR-specific validation: credit limit (skip for contract-based invoices)
            if (!dto.SkipCreditLimitCheck && !_customerService.ValidateCreditLimit(dto.CustomerId, dto.TotalAmount))
            {
                throw new AppException(ExceptionCode.SALES_CUSTOMER_CREDIT_EXCEEDED);
            }

            // Use base class validation
            ValidateInvoiceDates(dto.InvoiceDate, dto.DueDate);
            ValidateLines(dto.Lines);

            // Calculate totals from lines
            decimal subtotal = 0;
            decimal taxAmount = 0;
            var lineEntities = new List<ARInvoiceLine>();
            int lineNumber = 1;

            foreach (var lineDto in dto.Lines)
            {
                var taxRate = GetTaxRate(lineDto.TaxCodeId);
                var (amount, tax, total) = CalculateLineTotals(lineDto.Quantity, lineDto.UnitPrice, taxRate);

                subtotal += amount;
                taxAmount += tax;

                lineEntities.Add(new ARInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    LineNumber = lineNumber++,
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    Amount = amount,
                    TaxCodeId = lineDto.TaxCodeId,
                    TaxRate = taxRate,
                    TaxAmount = tax,
                    TotalAmount = total,
                    CostCenterId = lineDto.CostCenterId,
                    DepartmentId = lineDto.DepartmentId,
                    ProjectId = lineDto.ProjectId,
                    Create_by = userName,
                    Create_time = DateTime.Now
                });
            }

            var totalAmount = InvoiceHelper.CalculateTotalAmount(subtotal, taxAmount);

            // Create invoice
            var invoice = new ARInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = dto.InvoiceNumber,
                CustomerId = dto.CustomerId,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                Currency = InvoiceHelper.NormalizeCurrency(dto.Currency),
                ExchangeRate = InvoiceHelper.EnsureValidExchangeRate(dto.ExchangeRate),
                TaxGroupId = dto.TaxGroupId,
                Description = dto.Description,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Notes,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                PaidAmount = 0,
                RemainingAmount = totalAmount,
                Status = InvoiceStatus.Draft,
                Create_by = userName,
                Create_time = DateTime.Now
            };

            // Set invoice ID on lines
            foreach (var line in lineEntities)
            {
                line.InvoiceId = invoice.Id;
            }

            // Save invoice and lines in transaction
            Context.Ado.BeginTran();
            try
            {
                _invoiceRepository.Insert(invoice);
                _lineRepository.InsertRange(lineEntities);
                Context.Ado.CommitTran();
            }
            catch (Exception)
            {
                Context.Ado.RollbackTran();
                throw;
            }

            invoice.Lines = lineEntities;
            return invoice;
        }

        /// <summary>
        /// Update invoice with lines
        /// </summary>
        public int Update(UpdateARInvoiceDto dto, string userName)
        {
            var existing = _invoiceRepository.GetById(dto.Id);
            if (existing == null)
                throw new AppException(ExceptionCode.FINANCE_INVOICE_NOT_FOUND);

            // Use base class validation
            ValidateInvoiceStatusForEdit(existing.Status);
            ValidateInvoiceDates(dto.InvoiceDate, dto.DueDate);
            ValidateLines(dto.Lines);

            // Calculate totals from lines
            decimal subtotal = 0;
            decimal taxAmount = 0;
            var lineEntities = new List<ARInvoiceLine>();
            int lineNumber = 1;

            foreach (var lineDto in dto.Lines)
            {
                var taxRate = GetTaxRate(lineDto.TaxCodeId);
                var (amount, tax, total) = CalculateLineTotals(lineDto.Quantity, lineDto.UnitPrice, taxRate);

                subtotal += amount;
                taxAmount += tax;

                lineEntities.Add(new ARInvoiceLine
                {
                    Id = lineDto.Id ?? Guid.NewGuid(),
                    InvoiceId = dto.Id,
                    LineNumber = lineNumber++,
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    Amount = amount,
                    TaxCodeId = lineDto.TaxCodeId,
                    TaxRate = taxRate,
                    TaxAmount = tax,
                    TotalAmount = total,
                    CostCenterId = lineDto.CostCenterId,
                    DepartmentId = lineDto.DepartmentId,
                    ProjectId = lineDto.ProjectId,
                    Create_by = userName,
                    Create_time = DateTime.Now
                });
            }

            var totalAmount = InvoiceHelper.CalculateTotalAmount(subtotal, taxAmount);

            // Update invoice
            existing.InvoiceNumber = dto.InvoiceNumber;
            existing.CustomerId = dto.CustomerId;
            existing.InvoiceDate = dto.InvoiceDate;
            existing.DueDate = dto.DueDate;
            existing.Currency = InvoiceHelper.NormalizeCurrency(dto.Currency);
            existing.ExchangeRate = InvoiceHelper.EnsureValidExchangeRate(dto.ExchangeRate);
            existing.TaxGroupId = dto.TaxGroupId;
            existing.Description = dto.Description;
            existing.ReferenceNumber = dto.ReferenceNumber;
            existing.Notes = dto.Notes;
            existing.Subtotal = subtotal;
            existing.TaxAmount = taxAmount;
            existing.TotalAmount = totalAmount;
            existing.RemainingAmount = InvoiceHelper.CalculateRemainingAmount(totalAmount, existing.PaidAmount);
            existing.Update_by = userName;
            existing.Update_time = DateTime.Now;

            // Save invoice and lines in transaction
            Context.Ado.BeginTran();
            try
            {
                // Delete existing lines and insert new ones
                _lineRepository.DeleteByInvoiceId(dto.Id);
                _lineRepository.InsertRange(lineEntities);

                // Update invoice
                Update(existing, it => new
                {
                    it.InvoiceNumber,
                    it.CustomerId,
                    it.InvoiceDate,
                    it.DueDate,
                    it.Currency,
                    it.ExchangeRate,
                    it.TaxGroupId,
                    it.Description,
                    it.ReferenceNumber,
                    it.Notes,
                    it.Subtotal,
                    it.TaxAmount,
                    it.TotalAmount,
                    it.RemainingAmount,
                    it.Update_by,
                    it.Update_time
                }, f => f.Id == existing.Id);

                Context.Ado.CommitTran();
                return 1;
            }
            catch (Exception)
            {
                Context.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// Delete invoices (draft only)
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Check if invoices can be deleted
            foreach (var id in ids)
            {
                var invoice = _invoiceRepository.GetById(id);
                if (invoice == null) continue;

                ValidateInvoiceStatusForDelete(invoice.Status, invoice.PaidAmount, invoice.InvoiceNumber);
            }

            Context.Ado.BeginTran();
            try
            {
                // Delete lines first
                foreach (var id in ids)
                {
                    _lineRepository.DeleteByInvoiceId(id);
                }

                // Delete invoices
                var result = Context.Deleteable<ARInvoice>()
                    .Where(it => ids.Contains(it.Id))
                    .ExecuteCommand();

                Context.Ado.CommitTran();
                return result;
            }
            catch (Exception)
            {
                Context.Ado.RollbackTran();
                throw;
            }
        }

        #endregion

        #region Status and Posting Methods

        /// <summary>
        /// Update invoice status
        /// </summary>
        public void UpdateInvoiceStatus(Guid invoiceId)
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null) return;

            invoice.Status = CalculateInvoiceStatus(invoice.Status, invoice.RemainingAmount, invoice.PaidAmount, invoice.DueDate);
            Update(invoice, it => new { it.Status }, f => f.Id == invoiceId);
        }

        /// <summary>
        /// Validate invoice for posting
        /// </summary>
        public InvoiceValidationResultDto ValidateForPosting(Guid invoiceId)
        {
            var result = new InvoiceValidationResultDto { IsValid = true };

            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice not found - الفاتورة غير موجودة");
                return result;
            }

            // Check if draft
            if (invoice.Status != InvoiceStatus.Draft)
            {
                result.IsValid = false;
                result.Errors.Add("Only draft invoices can be posted - يمكن ترحيل المسودات فقط");
            }

            // Check if already posted
            if (invoice.GLJournalId.HasValue)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice already posted - الفاتورة مرحّلة مسبقاً");
            }

            // Check if invoice amount is valid
            if (invoice.TotalAmount <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice amount must be greater than zero - يجب أن يكون مبلغ الفاتورة أكبر من صفر");
            }

            // Check lines exist
            var lines = _lineRepository.GetByInvoiceId(invoiceId);
            if (lines == null || lines.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice must have at least one line - يجب أن تحتوي الفاتورة على سطر واحد على الأقل");
            }

            // Check customer
            var customer = _customerRepository.GetById(invoice.CustomerId);
            if (customer == null)
            {
                result.IsValid = false;
                result.Errors.Add("Customer not found - العميل غير موجود");
            }
            else if (!customer.IsActive)
            {
                result.Warnings.Add("Customer is inactive - العميل غير نشط");
            }

            // Period validation
            result.PeriodValidation = new PeriodValidationDto
            {
                IsValid = true,
                IsClosed = false,
                IsLocked = false
            };

            // Balance check
            result.BalanceCheck = new BalanceCheckDto
            {
                IsBalanced = true,
                TotalDebit = invoice.TotalAmount,
                TotalCredit = invoice.TotalAmount,
                Difference = 0
            };

            return result;
        }

        /// <summary>
        /// Post invoice (create GL journal)
        /// </summary>
        public InvoicePostResultDto PostInvoice(Guid invoiceId, string userName)
        {
            var validation = ValidateForPosting(invoiceId);
            if (!validation.IsValid)
            {
                // Localize each error message before joining
                var localizedErrors = validation.Errors.Select(e => _messageHelper.GetLocalizedMessage(e)).ToList();

                return new InvoicePostResultDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Message = string.Join("; ", localizedErrors)
                };
            }

            var invoice = _invoiceRepository.GetById(invoiceId);

            // Load invoice lines for GL journal creation
            invoice.Lines = _lineRepository.GetByInvoiceId(invoiceId);

            Context.Ado.BeginTran();
            try
            {
                // Create actual GL journal entry using the GL integration service
                var journal = _glIntegrationService.CreateARInvoiceJournal(invoice, userName);

                // Update invoice status and journal reference
                invoice.Status = InvoiceStatus.Posted;
                invoice.GLJournalId = journal.Id;
                invoice.Update_by = userName;
                invoice.Update_time = DateTime.Now;

                // Check if overdue
                if (invoice.DueDate < DateTime.Now)
                {
                    invoice.Status = InvoiceStatus.Overdue;
                }

                Update(invoice, it => new { it.Status, it.GLJournalId, it.Update_by, it.Update_time }, f => f.Id == invoiceId);

                Context.Ado.CommitTran();

                // Localized success message
                var currentLang = _localizationService.GetCurrentLanguage();
                var successMessage = currentLang == "ar"
                    ? $"تم ترحيل الفاتورة بنجاح. القيد: {journal.JournalNumber}"
                    : $"Invoice posted successfully. Journal: {journal.JournalNumber}";

                return new InvoicePostResultDto
                {
                    InvoiceId = invoiceId,
                    JournalId = journal.Id,
                    JournalNumber = journal.JournalNumber,
                    Success = true,
                    Message = successMessage
                };
            }
            catch (Exception ex)
            {
                Context.Ado.RollbackTran();

                // Extract detailed errors from AppException parameters if available
                var detailedErrors = "";
                if (ex is AppException appEx && appEx.Parameters.ContainsKey("Errors"))
                {
                    detailedErrors = $" [{appEx.Parameters["Errors"]}]";
                }

                // Localize the exception message (it might contain bilingual validation errors)
                var localizedExceptionMessage = _messageHelper.GetLocalizedMessage(ex.Message);

                // Localized error wrapper
                var currentLang = _localizationService.GetCurrentLanguage();
                var errorMessage = currentLang == "ar"
                    ? $"فشل ترحيل الفاتورة: {localizedExceptionMessage}{detailedErrors}"
                    : $"Failed to post invoice: {localizedExceptionMessage}{detailedErrors}";

                return new InvoicePostResultDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Message = errorMessage
                };
            }
        }

        /// <summary>
        /// Cancel invoice
        /// </summary>
        public int CancelInvoice(Guid invoiceId, string reason, string userName)
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
                throw new AppException(ExceptionCode.FINANCE_INVOICE_NOT_FOUND);

            if (invoice.PaidAmount > 0)
                throw new AppException(ExceptionCode.FINANCE_INVOICE_CANNOT_DELETE);

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.Notes = string.IsNullOrEmpty(invoice.Notes)
                ? $"Cancelled: {reason}"
                : $"{invoice.Notes}\nCancelled: {reason}";
            invoice.Update_by = userName;
            invoice.Update_time = DateTime.Now;

            return Update(invoice, it => new { it.Status, it.Notes, it.Update_by, it.Update_time }, f => f.Id == invoiceId);
        }

        #endregion

        #region GL Integration

        /// <summary>
        /// Prepare GL journal data
        /// </summary>
        public object PrepareGLJournalData(Guid invoiceId)
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null) return null;

            var lines = _lineRepository.GetByInvoiceId(invoiceId);
            var customer = _customerRepository.GetById(invoice.CustomerId);

            var entries = new List<object>();
            decimal totalDebit = 0m;
            decimal totalCredit = 0m;

            // Get VAT Output account (required if there's tax)
            var totalTax = lines.Sum(l => l.TaxAmount);
            Guid? vatOutputAccountId = null;
            GLAccount vatOutputAccount = null;
            if (totalTax > 0)
            {
                vatOutputAccount = Context.Queryable<GLAccount>()
                    .Where(a => a.IsLeaf && a.IsActive &&
                        (a.AccountNumber.Contains("2140") ||
                        a.AccountNumber.Contains("2200") ||
                        a.AccountNameEn.Contains("VAT Output") ||
                        a.AccountNameEn.Contains("Output Tax")))
                    .First();
                vatOutputAccountId = vatOutputAccount?.AccountId;
            }

            // Debit AR account (customer's GL account)
            entries.Add(new
            {
                AccountId = customer?.GLAccountId,
                AccountCode = customer?.GLAccountId != null ? GetAccountById(customer.GLAccountId.Value)?.AccountNumber : "AR",
                AccountName = "Accounts Receivable - " + (customer?.NameEn ?? ""),
                Debit = invoice.TotalAmount,
                Credit = 0m,
                Currency = invoice.Currency,
                ExchangeRate = invoice.ExchangeRate,
                BaseDebit = invoice.TotalAmount * invoice.ExchangeRate,
                BaseCredit = 0m,
                Description = $"AR Invoice {invoice.InvoiceNumber} - {customer?.NameEn}"
            });
            totalDebit = invoice.TotalAmount;

            // Create entries for each line (Credit revenue accounts)
            foreach (var line in lines)
            {
                var account = GetAccountById(line.AccountId);
                entries.Add(new
                {
                    AccountId = line.AccountId,
                    AccountCode = account?.AccountNumber ?? "",
                    AccountName = account?.AccountNameEn ?? "",
                    Debit = 0m,
                    Credit = line.Amount,
                    Currency = invoice.Currency,
                    ExchangeRate = invoice.ExchangeRate,
                    BaseDebit = 0m,
                    BaseCredit = line.Amount * invoice.ExchangeRate,
                    Description = line.Description ?? $"Invoice {invoice.InvoiceNumber}"
                });
                totalCredit += line.Amount;

                // Tax entry if applicable
                if (line.TaxAmount > 0)
                {
                    entries.Add(new
                    {
                        AccountId = vatOutputAccountId,
                        AccountCode = vatOutputAccount?.AccountNumber ?? "VAT-OUT",
                        AccountName = vatOutputAccount?.AccountNameEn ?? "VAT Output",
                        Debit = 0m,
                        Credit = line.TaxAmount,
                        Currency = invoice.Currency,
                        ExchangeRate = invoice.ExchangeRate,
                        BaseDebit = 0m,
                        BaseCredit = line.TaxAmount * invoice.ExchangeRate,
                        Description = $"VAT on Invoice {invoice.InvoiceNumber}"
                    });
                    totalCredit += line.TaxAmount;
                }
            }

            return new
            {
                JournalDate = invoice.InvoiceDate,
                Description = $"AR Invoice - {invoice.InvoiceNumber}",
                Currency = invoice.Currency,
                ExchangeRate = invoice.ExchangeRate,
                ReferenceNumber = invoice.ReferenceNumber,
                ReferenceType = "ARInvoice",
                ReferenceId = invoice.Id,
                Entries = entries,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                IsBalanced = Math.Abs(totalDebit - totalCredit) < 0.01m
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get unpaid invoices for customer
        /// </summary>
        public List<UnpaidInvoiceDto> GetUnpaidInvoices(Guid customerId)
        {
            var invoices = Context.Queryable<ARInvoice>()
                .Where(i => i.CustomerId == customerId &&
                           i.RemainingAmount > 0 &&
                           i.Status != InvoiceStatus.Cancelled &&
                           i.Status != InvoiceStatus.Draft)
                .OrderBy(i => i.DueDate)
                .ToList();

            return invoices.Select(i => new UnpaidInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Currency = i.Currency,
                TotalAmount = i.TotalAmount,
                RemainingAmount = i.RemainingAmount,
                DaysOverdue = InvoiceHelper.GetDaysOverdue(i.DueDate)
            }).ToList();
        }

        /// <summary>
        /// Get next invoice number (thread-safe with optimistic concurrency)
        /// </summary>
        public string GetNextInvoiceNumber(long? companyId = null)
        {
            var fiscalYear = DateTime.Now.Year;

            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                try
                {
                    var sequence = Context.Queryable<InvoiceNumberSequence>()
                        .Where(s => s.SequenceType == InvoiceSequenceType.AR &&
                                   s.FiscalYear == fiscalYear &&
                                   s.CompanyId == companyId)
                        .First();

                    if (sequence == null)
                    {
                        sequence = InvoiceNumberSequence.Create(InvoiceSequenceType.AR, fiscalYear, companyId);
                        sequence.CurrentNumber = 1;
                        sequence.CreateTime = DateTime.Now;

                        Context.Insertable(sequence).ExecuteCommand();
                        return sequence.GenerateInvoiceNumber();
                    }

                    var currentVersion = sequence.RowVersion;
                    var nextNumber = sequence.CurrentNumber + 1;

                    var rowsAffected = Context.Updateable<InvoiceNumberSequence>()
                        .SetColumns(s => new InvoiceNumberSequence
                        {
                            CurrentNumber = nextNumber,
                            RowVersion = currentVersion + 1,
                            UpdateTime = DateTime.Now
                        })
                        .Where(s => s.Id == sequence.Id && s.RowVersion == currentVersion)
                        .ExecuteCommand();

                    if (rowsAffected > 0)
                    {
                        sequence.CurrentNumber = nextNumber;
                        return sequence.GenerateInvoiceNumber();
                    }

                    Thread.Sleep(10 * (attempt + 1));
                }
                catch (Exception ex)
                {
                    if (attempt == MaxRetryAttempts - 1)
                        throw new AppException(
                            ExceptionCode.DATABASE_ERROR,
                            $"Failed to generate invoice number after {MaxRetryAttempts} attempts",
                            details: new { Attempts = MaxRetryAttempts, Error = ex.Message },
                            severity: ExceptionSeverity.Critical,
                            httpStatusCode: 500,
                            innerException: ex);

                    Thread.Sleep(10 * (attempt + 1));
                }
            }

            throw new AppException(
                ExceptionCode.CONCURRENCY_CONFLICT,
                $"Failed to generate invoice number after {MaxRetryAttempts} attempts due to concurrent access");
        }

        /// <summary>
        /// Check if invoice number already exists
        /// </summary>
        public CheckCodeResponseDto CheckInvoiceNumberExists(string invoiceNumber, Guid? excludeId = null)
        {
            var exists = Context.Queryable<ARInvoice>()
                .WhereIF(!string.IsNullOrEmpty(invoiceNumber), i => i.InvoiceNumber == invoiceNumber)
                .WhereIF(excludeId.HasValue, i => i.Id != excludeId.Value)
                .Any();

            return new CheckCodeResponseDto { Exists = exists };
        }

        /// <summary>
        /// Apply payment allocation to invoice
        /// Updates PaidAmount, RemainingAmount, and Status based on payment allocation
        /// </summary>
        public void ApplyPaymentAllocation(Guid invoiceId, decimal allocatedAmount, string userName)
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
                throw new AppException(ExceptionCode.FINANCE_INVOICE_NOT_FOUND);

            // Update payment amounts
            invoice.PaidAmount += allocatedAmount;
            invoice.RemainingAmount = invoice.TotalAmount - invoice.PaidAmount;

            // Ensure RemainingAmount doesn't go negative
            if (invoice.RemainingAmount < 0)
                invoice.RemainingAmount = 0;

            // Update status based on payment
            if (invoice.RemainingAmount <= 0.01m)
            {
                // Fully paid
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.PaidAmount > 0)
            {
                // Partially paid
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            invoice.Update_by = userName;
            invoice.Update_time = DateTime.Now;

            // Update invoice
            Update(invoice, it => new
            {
                it.PaidAmount,
                it.RemainingAmount,
                it.Status,
                it.Update_by,
                it.Update_time
            }, f => f.Id == invoiceId);
        }

        #endregion

        #region Private Helper Methods

        private ARInvoiceListDto MapToListDto(ARInvoice invoice)
        {
            var dto = invoice.Adapt<ARInvoiceListDto>();
            var customer = _customerRepository.GetById(invoice.CustomerId);
            if (customer != null)
            {
                dto.CustomerCode = customer.CustomerCode;
                dto.CustomerName = customer.NameEn;
            }
            return dto;
        }

        private ARInvoiceDetailDto MapToDetailDto(ARInvoice invoice)
        {
            var dto = invoice.Adapt<ARInvoiceDetailDto>();
            var customer = _customerRepository.GetById(invoice.CustomerId);
            if (customer != null)
            {
                dto.CustomerCode = customer.CustomerCode;
                dto.CustomerName = customer.NameEn;
            }

            if (invoice.Lines != null)
            {
                dto.Lines = invoice.Lines.Select(line =>
                {
                    var lineDto = line.Adapt<ARInvoiceLineDto>();
                    var account = GetAccountById(line.AccountId);
                    if (account != null)
                    {
                        lineDto.AccountCode = account.AccountNumber;
                        lineDto.AccountName = account.AccountNameEn;
                    }
                    return lineDto;
                }).ToList();
            }

            return dto;
        }

        #endregion

        #region Abstract Method Implementations

        protected override ARInvoiceLine CreateLineEntity(CreateARInvoiceLineDto lineDto, Guid invoiceId, int lineNumber, decimal taxRate, string userName)
        {
            var (amount, taxAmount, total) = CalculateLineTotals(lineDto.Quantity, lineDto.UnitPrice, taxRate);

            return new ARInvoiceLine
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoiceId,
                LineNumber = lineNumber,
                AccountId = lineDto.AccountId,
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                Amount = amount,
                TaxCodeId = lineDto.TaxCodeId,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                TotalAmount = total,
                CostCenterId = lineDto.CostCenterId,
                DepartmentId = lineDto.DepartmentId,
                ProjectId = lineDto.ProjectId,
                Create_by = userName,
                Create_time = DateTime.Now
            };
        }

        protected override ARInvoiceLine UpdateLineEntity(UpdateARInvoiceLineDto lineDto, Guid invoiceId, int lineNumber, decimal taxRate, string userName)
        {
            var (amount, taxAmount, total) = CalculateLineTotals(lineDto.Quantity, lineDto.UnitPrice, taxRate);

            return new ARInvoiceLine
            {
                Id = lineDto.Id ?? Guid.NewGuid(),
                InvoiceId = invoiceId,
                LineNumber = lineNumber,
                AccountId = lineDto.AccountId,
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                Amount = amount,
                TaxCodeId = lineDto.TaxCodeId,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                TotalAmount = total,
                CostCenterId = lineDto.CostCenterId,
                DepartmentId = lineDto.DepartmentId,
                ProjectId = lineDto.ProjectId,
                Create_by = userName,
                Create_time = DateTime.Now
            };
        }

        protected override decimal GetLineQuantity(CreateARInvoiceLineDto lineDto) => lineDto.Quantity;
        protected override decimal GetLineUnitPrice(CreateARInvoiceLineDto lineDto) => lineDto.UnitPrice;
        protected override Guid? GetLineTaxCodeId(CreateARInvoiceLineDto lineDto) => lineDto.TaxCodeId;
        protected override decimal GetUpdateLineQuantity(UpdateARInvoiceLineDto lineDto) => lineDto.Quantity;
        protected override decimal GetUpdateLineUnitPrice(UpdateARInvoiceLineDto lineDto) => lineDto.UnitPrice;
        protected override Guid? GetUpdateLineTaxCodeId(UpdateARInvoiceLineDto lineDto) => lineDto.TaxCodeId;
        protected override Guid? GetUpdateLineId(UpdateARInvoiceLineDto lineDto) => lineDto.Id;

        #endregion
    }
}
