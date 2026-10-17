using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Customers;
using ZR.Model.Customers.Dto;
using ZR.Repository.Finance.ARAP;
using ZR.Repository.Customers;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;

namespace ZR.Service.Finance.ARAP
{
    /// <summary>
    /// Adjustment Service - خدمة التعديلات
    /// </summary>
    [AppService(ServiceType = typeof(IAdjustmentService), ServiceLifetime = LifeTime.Transient)]
    public class AdjustmentService : BaseService<CreditDebitNote>, IAdjustmentService
    {
        private readonly ICreditDebitNoteRepository _noteRepository;
        private readonly IARInvoiceRepository _arInvoiceRepository;
        private readonly IAPInvoiceRepository _apInvoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IVendorRepository _vendorRepository;

        public AdjustmentService(
            ICreditDebitNoteRepository noteRepository,
            IARInvoiceRepository arInvoiceRepository,
            IAPInvoiceRepository apInvoiceRepository,
            ICustomerRepository customerRepository,
            IVendorRepository vendorRepository)
        {
            _noteRepository = noteRepository;
            _arInvoiceRepository = arInvoiceRepository;
            _apInvoiceRepository = apInvoiceRepository;
            _customerRepository = customerRepository;
            _vendorRepository = vendorRepository;
        }

        /// <summary>
        /// Get paginated list of notes
        /// </summary>
        public PagedInfo<CreditDebitNoteListDto> GetList(CreditDebitNoteQueryDto query, PagerInfo pager)
        {
            var searchModel = new CreditDebitNote
            {
                NoteNumber = query.NoteNumber,
                CustomerId = query.CustomerId,
                VendorId = query.VendorId,
                CompanyID = query.CompanyId
            };

            var result = _noteRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(note => MapToDto(note)).ToList();

            return new PagedInfo<CreditDebitNoteListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get note by ID
        /// </summary>
        public CreditDebitNoteDetailDto GetById(Guid id)
        {
            var note = _noteRepository.GetById(id);
            if (note == null) return null;

            return MapToDetailDto(note);
        }

        /// <summary>
        /// Create credit/debit note
        /// </summary>
        public CreditDebitNote Create(CreateCreditDebitNoteDto dto, string userName)
        {
            // Validate customer or vendor
            if (dto.CustomerId.HasValue && dto.VendorId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_CUSTOMER_AND_VENDOR_CONFLICT);
            }

            if (!dto.CustomerId.HasValue && !dto.VendorId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ARAP_CUSTOMER_OR_VENDOR_REQUIRED);
            }

            var model = dto.Adapt<CreditDebitNote>();
            model.Id = Guid.NewGuid();
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            _noteRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Apply adjustment to invoice
        /// </summary>
        public void ApplyAdjustmentToInvoice(Guid noteId, Guid invoiceId)
        {
            var note = _noteRepository.GetById(noteId);
            if (note == null)
                throw new AppException(ExceptionCode.FINANCE_ARAP_NOTE_NOT_FOUND).WithParameter("NoteId", noteId);

            if (note.CustomerId.HasValue)
            {
                var invoice = _arInvoiceRepository.GetById(invoiceId);
                if (invoice == null)
                    throw new AppException(ExceptionCode.FINANCE_INVOICE_NOT_FOUND).WithParameter("InvoiceId", invoiceId);

                if (note.NoteType == NoteType.Credit)
                {
                    // Credit note reduces amount owed
                    invoice.RemainingAmount = Math.Max(0, invoice.RemainingAmount - note.Amount);
                }
                else
                {
                    // Debit note increases amount owed
                    invoice.RemainingAmount += note.Amount;
                }

                invoice.TotalAmount = invoice.PaidAmount + invoice.RemainingAmount;
                UpdateARInvoiceStatus(invoice);
                _arInvoiceRepository.Update(invoice, it => new { it.TotalAmount, it.RemainingAmount, it.Status }, f => f.Id == invoiceId);
            }
            else if (note.VendorId.HasValue)
            {
                var invoice = _apInvoiceRepository.GetById(invoiceId);
                if (invoice == null)
                    throw new AppException(ExceptionCode.FINANCE_INVOICE_NOT_FOUND).WithParameter("InvoiceId", invoiceId);

                if (note.NoteType == NoteType.Credit)
                {
                    // Credit note reduces amount owed
                    invoice.RemainingAmount = Math.Max(0, invoice.RemainingAmount - note.Amount);
                }
                else
                {
                    // Debit note increases amount owed
                    invoice.RemainingAmount += note.Amount;
                }

                invoice.TotalAmount = invoice.PaidAmount + invoice.RemainingAmount;
                UpdateAPInvoiceStatus(invoice);
                _apInvoiceRepository.Update(invoice, it => new { it.TotalAmount, it.RemainingAmount, it.Status }, f => f.Id == invoiceId);
            }
        }

        /// <summary>
        /// Prepare GL journal data
        /// </summary>
        public object PrepareGLJournalData(Guid noteId)
        {
            var note = _noteRepository.GetById(noteId);
            if (note == null) return null;

            string descriptionAr = string.Empty;
            string descriptionEn = string.Empty;
            Guid? accountId = null;

            if (note.CustomerId.HasValue)
            {
                var customer = _customerRepository.GetById(note.CustomerId.Value);
                accountId = customer?.GLAccountId;
                descriptionAr = $"إشعار {(note.NoteType == NoteType.Credit ? "دائن" : "مدين")} عميل - {note.NoteNumber}";
                descriptionEn = $"Customer {(note.NoteType == NoteType.Credit ? "Credit" : "Debit")} Note - {note.NoteNumber}";
            }
            else if (note.VendorId.HasValue)
            {
                var vendor = _vendorRepository.GetById(note.VendorId.Value);
                accountId = vendor?.GLAccountId;
                descriptionAr = $"إشعار {(note.NoteType == NoteType.Credit ? "دائن" : "مدين")} مورد - {note.NoteNumber}";
                descriptionEn = $"Vendor {(note.NoteType == NoteType.Credit ? "Credit" : "Debit")} Note - {note.NoteNumber}";
            }

            return new
            {
                JournalDate = note.NoteDate,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn,
                Entries = new[]
                {
                    new
                    {
                        AccountId = accountId,
                        Debit = note.NoteType == NoteType.Debit ? note.Amount : 0,
                        Credit = note.NoteType == NoteType.Credit ? note.Amount : 0,
                        DescriptionAr = descriptionAr,
                        DescriptionEn = descriptionEn
                    }
                }
            };
        }

        private void UpdateARInvoiceStatus(ARInvoice invoice)
        {
            if (invoice.RemainingAmount <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.Status = InvoiceStatus.Posted;
            }

            if (invoice.RemainingAmount > 0 && invoice.DueDate < DateTime.Now)
            {
                invoice.Status = InvoiceStatus.Overdue;
            }
        }

        private void UpdateAPInvoiceStatus(APInvoice invoice)
        {
            if (invoice.RemainingAmount <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.Status = InvoiceStatus.Posted;
            }

            if (invoice.RemainingAmount > 0 && invoice.DueDate < DateTime.Now)
            {
                invoice.Status = InvoiceStatus.Overdue;
            }
        }

        private CreditDebitNoteListDto MapToDto(CreditDebitNote note)
        {
            var dto = note.Adapt<CreditDebitNoteListDto>();
            if (note.CustomerId.HasValue)
            {
                var customer = _customerRepository.GetById(note.CustomerId.Value);
                dto.CustomerName = customer != null ? customer.NameEn : string.Empty;
            }
            if (note.VendorId.HasValue)
            {
                var vendor = _vendorRepository.GetById(note.VendorId.Value);
                dto.VendorName = vendor != null ? vendor.NameEn : string.Empty;
            }
            return dto;
        }

        private CreditDebitNoteDetailDto MapToDetailDto(CreditDebitNote note)
        {
            var dto = note.Adapt<CreditDebitNoteDetailDto>();
            if (note.CustomerId.HasValue)
            {
                var customer = _customerRepository.GetById(note.CustomerId.Value);
                dto.CustomerName = customer != null ? customer.NameEn : string.Empty;
            }
            if (note.VendorId.HasValue)
            {
                var vendor = _vendorRepository.GetById(note.VendorId.Value);
                dto.VendorName = vendor != null ? vendor.NameEn : string.Empty;
            }
            return dto;
        }
    }
}


