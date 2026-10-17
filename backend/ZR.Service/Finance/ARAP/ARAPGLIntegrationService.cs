using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using ZR.Model.Finance.ARAP;
using ZR.Model.Customers;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Repository.Finance.ARAP;
using ZR.Repository.Customers;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;
using ZR.Service.IService.Finance.Configuration;
using ZR.Service.IService.Finance.GL;
using ZR.Service.Finance.Configuration;

namespace ZR.Service.Finance.ARAP
{
    /// <summary>
    /// ARAP GL Integration Service - خدمة تكامل الذمم المدينة والدائنة مع دفتر الأستاذ
    /// Creates GL journal entries for AR/AP invoice posting
    /// </summary>
    [AppService(ServiceType = typeof(IARAPGLIntegrationService), ServiceLifetime = LifeTime.Transient)]
    public class ARAPGLIntegrationService : IARAPGLIntegrationService
    {
        private readonly IGLJournalService _journalService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IARInvoiceLineRepository _arLineRepository;
        private readonly IAPInvoiceLineRepository _apLineRepository;
        private readonly IGLAccountRepository _accountRepository;
        private readonly ITaxConfigurationService _taxConfigService;
        private readonly IFiscalYearHelper _fiscalYearHelper;

        public ARAPGLIntegrationService(
            IGLJournalService journalService,
            ICustomerRepository customerRepository,
            IVendorRepository vendorRepository,
            IARInvoiceLineRepository arLineRepository,
            IAPInvoiceLineRepository apLineRepository,
            IGLAccountRepository accountRepository,
            ITaxConfigurationService taxConfigService,
            IFiscalYearHelper fiscalYearHelper)
        {
            _journalService = journalService;
            _customerRepository = customerRepository;
            _vendorRepository = vendorRepository;
            _arLineRepository = arLineRepository;
            _apLineRepository = apLineRepository;
            _accountRepository = accountRepository;
            _taxConfigService = taxConfigService;
            _fiscalYearHelper = fiscalYearHelper;
        }

        #region AR Invoice GL Integration

        /// <summary>
        /// Create GL Journal for AR Invoice posting
        /// AR Invoice Entry:
        ///   DR Accounts Receivable (Customer GL Account) - Total Amount
        ///   CR Revenue Account(s) - Line Amounts
        ///   CR VAT Output - Tax Amount (if applicable)
        /// </summary>
        public GLJournal CreateARInvoiceJournal(ARInvoice invoice, string userName)
        {
            var journalDto = PreviewARInvoiceJournal(invoice);

            // Create and post the journal
            var journal = _journalService.Create(journalDto, userName);

            // Optionally auto-post the journal
            _journalService.PostJournal(journal.Id, userName);

            return journal;
        }

        /// <summary>
        /// Preview GL Journal for AR Invoice (without creating)
        /// </summary>
        public CreateJournalDto PreviewARInvoiceJournal(ARInvoice invoice)
        {
            var customer = _customerRepository.GetById(invoice.CustomerId);
            if (customer == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_CUSTOMER_NOT_FOUND).WithParameter("CustomerId", invoice.CustomerId);
            }

            var lines = invoice.Lines ?? _arLineRepository.GetByInvoiceId(invoice.Id);
            if (lines == null || lines.Count == 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_INVOICE_NO_LINES).WithParameter("InvoiceId", invoice.Id);
            }

            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            // Get customer's AR GL Account or use default from TaxConfiguration
            var arAccountId = customer.GLAccountId ?? _taxConfigService.GetDefaultARAccountId(invoice.CompanyID);
            if (!arAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_GL_ACCOUNT_NOT_CONFIGURED);
            }

            // DR: Accounts Receivable - Total Amount
            entries.Add(new CreateJournalEntryDto
            {
                LineNumber = lineNumber++,
                AccountId = arAccountId.Value,
                Debit = invoice.TotalAmount,
                Credit = 0,
                DescriptionAr = $"فاتورة عميل رقم {invoice.InvoiceNumber} - {customer.NameAr ?? customer.NameEn}",
                DescriptionEn = $"AR Invoice {invoice.InvoiceNumber} - {customer.NameEn}",
                Currency = invoice.Currency ?? "SAR",
                ExchangeRate = invoice.ExchangeRate,
                ReferenceNumber = invoice.InvoiceNumber
            });

            // CR: Revenue Accounts - Line Amounts (net of tax)
            foreach (var line in lines)
            {
                var account = _accountRepository.GetById(line.AccountId);
                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = line.AccountId,
                    Debit = 0,
                    Credit = line.Amount,
                    DescriptionAr = line.Description ?? $"بند فاتورة {invoice.InvoiceNumber}",
                    DescriptionEn = line.Description ?? $"Invoice line {invoice.InvoiceNumber}",
                    Currency = invoice.Currency ?? "SAR",
                    ExchangeRate = invoice.ExchangeRate,
                    ReferenceNumber = invoice.InvoiceNumber,
                    CostCenterId = line.CostCenterId,
                    DepartmentId = line.DepartmentId,
                    ProjectId = line.ProjectId
                });
            }

            // CR: VAT Output - Total Tax Amount (if any)
            var totalTax = lines.Sum(l => l.TaxAmount);
            if (totalTax > 0)
            {
                var vatOutputAccountId = _taxConfigService.GetVATOutputAccountId(invoice.CompanyID);
                if (!vatOutputAccountId.HasValue)
                {
                    throw new AppException(ExceptionCode.FINANCE_ARAP_VAT_OUTPUT_NOT_CONFIGURED);
                }
                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = vatOutputAccountId.Value,
                    Debit = 0,
                    Credit = totalTax,
                    DescriptionAr = $"ضريبة القيمة المضافة - فاتورة {invoice.InvoiceNumber}",
                    DescriptionEn = $"VAT Output - Invoice {invoice.InvoiceNumber}",
                    Currency = invoice.Currency ?? "SAR",
                    ExchangeRate = invoice.ExchangeRate,
                    ReferenceNumber = invoice.InvoiceNumber
                });
            }

            return new CreateJournalDto
            {
                DescriptionAr = $"ترحيل فاتورة عميل رقم {invoice.InvoiceNumber}",
                DescriptionEn = $"AR Invoice Posting - {invoice.InvoiceNumber}",
                JournalDate = invoice.InvoiceDate,
                Source = "ARAP",
                SourceDocumentType = "ARInvoice",
                SourceDocumentId = invoice.Id,
                Currency = invoice.Currency ?? "SAR",
                ReferenceNumber = invoice.InvoiceNumber,
                FiscalYear = _fiscalYearHelper.GetFiscalYearFromDate(invoice.InvoiceDate, invoice.CompanyID),
                FiscalPeriod = _fiscalYearHelper.GetFiscalPeriodFromDate(invoice.InvoiceDate, invoice.CompanyID),
                Entries = entries
            };
        }

        /// <summary>
        /// Reverse AR Invoice GL Journal
        /// </summary>
        public GLJournal ReverseARInvoiceJournal(ARInvoice invoice, string userName, string reason)
        {
            if (!invoice.GLJournalId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_NO_GL_JOURNAL_TO_REVERSE);
            }

            var reverseDto = new ReverseJournalDto
            {
                Id = invoice.GLJournalId.Value,
                ReversalDate = DateTime.Now,
                ReversalReason = reason ?? $"Reversal of AR Invoice {invoice.InvoiceNumber}"
            };

            return _journalService.ReverseJournal(reverseDto, userName);
        }

        #endregion

        #region AP Invoice GL Integration

        /// <summary>
        /// Create GL Journal for AP Invoice posting
        /// AP Invoice Entry:
        ///   DR Expense Account(s) - Line Amounts
        ///   DR VAT Input - Tax Amount (if applicable)
        ///   CR Accounts Payable (Vendor GL Account) - Total Amount
        /// </summary>
        public GLJournal CreateAPInvoiceJournal(APInvoice invoice, string userName)
        {
            var journalDto = PreviewAPInvoiceJournal(invoice);

            // Create and post the journal
            var journal = _journalService.Create(journalDto, userName);

            // Optionally auto-post the journal
            _journalService.PostJournal(journal.Id, userName);

            return journal;
        }

        /// <summary>
        /// Preview GL Journal for AP Invoice (without creating)
        /// </summary>
        public CreateJournalDto PreviewAPInvoiceJournal(APInvoice invoice)
        {
            var vendor = _vendorRepository.GetById(invoice.VendorId);
            if (vendor == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_VENDOR_NOT_FOUND).WithParameter("VendorId", invoice.VendorId);
            }

            var lines = invoice.Lines ?? _apLineRepository.GetByInvoiceId(invoice.Id);
            if (lines == null || lines.Count == 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_INVOICE_NO_LINES).WithParameter("InvoiceId", invoice.Id);
            }

            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            // DR: Expense Accounts - Line Amounts (net of tax)
            foreach (var line in lines)
            {
                var account = _accountRepository.GetById(line.AccountId);
                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = line.AccountId,
                    Debit = line.Amount,
                    Credit = 0,
                    DescriptionAr = line.Description ?? $"بند فاتورة مورد {invoice.InvoiceNumber}",
                    DescriptionEn = line.Description ?? $"AP Invoice line {invoice.InvoiceNumber}",
                    Currency = invoice.Currency ?? "SAR",
                    ExchangeRate = invoice.ExchangeRate,
                    ReferenceNumber = invoice.InvoiceNumber,
                    CostCenterId = line.CostCenterId,
                    DepartmentId = line.DepartmentId,
                    ProjectId = line.ProjectId
                });
            }

            // DR: VAT Input - Total Tax Amount (if any)
            var totalTax = lines.Sum(l => l.TaxAmount);
            if (totalTax > 0)
            {
                var vatInputAccountId = _taxConfigService.GetVATInputAccountId(invoice.CompanyID);
                if (!vatInputAccountId.HasValue)
                {
                    throw new AppException(ExceptionCode.FINANCE_ARAP_VAT_INPUT_NOT_CONFIGURED);
                }
                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = vatInputAccountId.Value,
                    Debit = totalTax,
                    Credit = 0,
                    DescriptionAr = $"ضريبة القيمة المضافة المدخلة - فاتورة {invoice.InvoiceNumber}",
                    DescriptionEn = $"VAT Input - Invoice {invoice.InvoiceNumber}",
                    Currency = invoice.Currency ?? "SAR",
                    ExchangeRate = invoice.ExchangeRate,
                    ReferenceNumber = invoice.InvoiceNumber
                });
            }

            // Get vendor's AP GL Account or use default from TaxConfiguration
            var apAccountId = vendor.GLAccountId ?? _taxConfigService.GetDefaultAPAccountId(invoice.CompanyID);
            if (!apAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_GL_ACCOUNT_NOT_CONFIGURED);
            }

            // CR: Accounts Payable - Total Amount
            entries.Add(new CreateJournalEntryDto
            {
                LineNumber = lineNumber++,
                AccountId = apAccountId.Value,
                Debit = 0,
                Credit = invoice.TotalAmount,
                DescriptionAr = $"فاتورة مورد رقم {invoice.InvoiceNumber} - {vendor.NameAr ?? vendor.NameEn}",
                DescriptionEn = $"AP Invoice {invoice.InvoiceNumber} - {vendor.NameEn}",
                Currency = invoice.Currency ?? "SAR",
                ExchangeRate = invoice.ExchangeRate,
                ReferenceNumber = invoice.InvoiceNumber
            });

            return new CreateJournalDto
            {
                DescriptionAr = $"ترحيل فاتورة مورد رقم {invoice.InvoiceNumber}",
                DescriptionEn = $"AP Invoice Posting - {invoice.InvoiceNumber}",
                JournalDate = invoice.InvoiceDate,
                Source = "ARAP",
                SourceDocumentType = "APInvoice",
                SourceDocumentId = invoice.Id,
                Currency = invoice.Currency ?? "SAR",
                ReferenceNumber = invoice.InvoiceNumber,
                FiscalYear = _fiscalYearHelper.GetFiscalYearFromDate(invoice.InvoiceDate, invoice.CompanyID),
                FiscalPeriod = _fiscalYearHelper.GetFiscalPeriodFromDate(invoice.InvoiceDate, invoice.CompanyID),
                Entries = entries
            };
        }

        /// <summary>
        /// Reverse AP Invoice GL Journal
        /// </summary>
        public GLJournal ReverseAPInvoiceJournal(APInvoice invoice, string userName, string reason)
        {
            if (!invoice.GLJournalId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_NO_GL_JOURNAL_TO_REVERSE);
            }

            var reverseDto = new ReverseJournalDto
            {
                Id = invoice.GLJournalId.Value,
                ReversalDate = DateTime.Now,
                ReversalReason = reason ?? $"Reversal of AP Invoice {invoice.InvoiceNumber}"
            };

            return _journalService.ReverseJournal(reverseDto, userName);
        }

        #endregion
    }
}
