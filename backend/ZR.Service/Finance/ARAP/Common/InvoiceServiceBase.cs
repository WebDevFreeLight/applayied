using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Tax;
using ZR.ServiceCore;

namespace ZR.Service.Finance.ARAP.Common
{
    /// <summary>
    /// Base class for AR/AP Invoice Services - الفئة الأساسية لخدمات الفواتير
    /// Eliminates code duplication between ARInvoiceService and APInvoiceService
    /// </summary>
    /// <typeparam name="TInvoice">Invoice entity type (ARInvoice or APInvoice)</typeparam>
    /// <typeparam name="TLine">Invoice line entity type (ARInvoiceLine or APInvoiceLine)</typeparam>
    /// <typeparam name="TCreateLineDto">DTO for creating invoice lines</typeparam>
    /// <typeparam name="TUpdateLineDto">DTO for updating invoice lines</typeparam>
    public abstract class InvoiceServiceBase<TInvoice, TLine, TCreateLineDto, TUpdateLineDto> : BaseService<TInvoice>
        where TInvoice : class, new()
        where TLine : class, new()
        where TCreateLineDto : class
        where TUpdateLineDto : class
    {
        protected InvoiceServiceBase()
        {
        }

        #region Validation Methods

        /// <summary>
        /// Validate invoice dates - التحقق من تواريخ الفاتورة
        /// </summary>
        protected void ValidateInvoiceDates(DateTime invoiceDate, DateTime dueDate)
        {
            if (dueDate < invoiceDate)
            {
                throw new Exception("Due date must be greater than or equal to invoice date - تاريخ الاستحقاق يجب أن يكون أكبر من أو يساوي تاريخ الفاتورة");
            }
        }

        /// <summary>
        /// Validate invoice lines exist - التحقق من وجود بنود الفاتورة
        /// </summary>
        protected void ValidateLines<T>(IList<T> lines) where T : class
        {
            if (lines == null || lines.Count == 0)
            {
                throw new Exception("At least one invoice line is required - يجب إضافة سطر واحد على الأقل");
            }
        }

        /// <summary>
        /// Validate invoice status for editing - التحقق من حالة الفاتورة للتعديل
        /// </summary>
        protected void ValidateInvoiceStatusForEdit(InvoiceStatus status)
        {
            if (status != InvoiceStatus.Draft)
            {
                throw new Exception("Only draft invoices can be edited - يمكن تعديل المسودات فقط");
            }
        }

        /// <summary>
        /// Validate invoice status for deletion - التحقق من حالة الفاتورة للحذف
        /// </summary>
        protected void ValidateInvoiceStatusForDelete(InvoiceStatus status, decimal paidAmount, string invoiceNumber)
        {
            if (status != InvoiceStatus.Draft)
            {
                throw new Exception($"Only draft invoices can be deleted - يمكن حذف المسودات فقط. Invoice: {invoiceNumber}");
            }

            if (paidAmount > 0)
            {
                throw new Exception($"Cannot delete invoice with payments - لا يمكن حذف فاتورة لها مدفوعات. Invoice: {invoiceNumber}");
            }
        }

        #endregion

        #region Calculation Methods

        /// <summary>
        /// Calculate line totals - حساب مجاميع السطر
        /// </summary>
        protected (decimal amount, decimal taxAmount, decimal total) CalculateLineTotals(
            decimal quantity,
            decimal unitPrice,
            decimal taxRate)
        {
            var amount = quantity * unitPrice;
            var taxAmount = amount * (taxRate / 100);
            var total = amount + taxAmount;

            return (
                Math.Round(amount, 4),
                Math.Round(taxAmount, 4),
                Math.Round(total, 4)
            );
        }

        /// <summary>
        /// Get tax rate from tax code - الحصول على نسبة الضريبة من رمز الضريبة
        /// </summary>
        protected async Task<decimal> GetTaxRateAsync(Guid? taxCodeId)
        {
            if (!taxCodeId.HasValue) return 0;

            var taxCode = await Context.Queryable<TaxCode>()
                .Where(t => t.Id == taxCodeId.Value)
                .FirstAsync();

            return taxCode?.Rate ?? 0;
        }

        /// <summary>
        /// Get tax rate from tax code (sync version for backward compatibility)
        /// </summary>
        protected decimal GetTaxRate(Guid? taxCodeId)
        {
            if (!taxCodeId.HasValue) return 0;

            var taxCode = Context.Queryable<TaxCode>()
                .Where(t => t.Id == taxCodeId.Value)
                .First();

            return taxCode?.Rate ?? 0;
        }

        #endregion

        #region Database Helper Methods

        /// <summary>
        /// Get GL account by ID - الحصول على الحساب المحاسبي بالمعرف
        /// </summary>
        protected async Task<GLAccount> GetAccountByIdAsync(Guid accountId)
        {
            return await Context.Queryable<GLAccount>()
                .Where(a => a.AccountId == accountId)
                .FirstAsync();
        }

        /// <summary>
        /// Get GL account by ID (sync version)
        /// </summary>
        protected GLAccount GetAccountById(Guid accountId)
        {
            return Context.Queryable<GLAccount>()
                .Where(a => a.AccountId == accountId)
                .First();
        }

        /// <summary>
        /// Delete invoice lines by invoice ID - حذف بنود الفاتورة بمعرف الفاتورة
        /// </summary>
        protected async Task DeleteLinesByInvoiceIdAsync(Guid invoiceId, string lineTableName)
        {
            await Context.Deleteable<TLine>()
                .Where($"InvoiceId = '{invoiceId}'")
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete invoice lines by invoice ID (sync version)
        /// </summary>
        protected void DeleteLinesByInvoiceId(Guid invoiceId, string lineTableName)
        {
            Context.Deleteable<TLine>()
                .Where($"InvoiceId = '{invoiceId}'")
                .ExecuteCommand();
        }

        #endregion

        #region Status Update Methods

        /// <summary>
        /// Update invoice status based on payment and due date
        /// تحديث حالة الفاتورة بناءً على الدفع وتاريخ الاستحقاق
        /// </summary>
        protected InvoiceStatus CalculateInvoiceStatus(
            InvoiceStatus currentStatus,
            decimal remainingAmount,
            decimal paidAmount,
            DateTime dueDate)
        {
            // Only update status for posted invoices (draft stays draft until posted)
            if (currentStatus == InvoiceStatus.Draft)
                return InvoiceStatus.Draft;

            if (remainingAmount <= 0)
            {
                return InvoiceStatus.Paid;
            }
            else if (paidAmount > 0)
            {
                return InvoiceStatus.PartiallyPaid;
            }
            else if (dueDate < DateTime.Now)
            {
                return InvoiceStatus.Overdue;
            }
            else
            {
                return InvoiceStatus.Posted;
            }
        }

        #endregion

        #region Validation for Posting

        /// <summary>
        /// Validate basic invoice posting requirements
        /// التحقق من متطلبات ترحيل الفاتورة الأساسية
        /// </summary>
        protected async Task<InvoiceValidationResultDto> ValidateBasicPostingRequirementsAsync(
            Guid invoiceId,
            Func<Guid, Task<object>> getInvoiceFunc,
            Func<object, InvoiceStatus> getStatusFunc,
            Func<object, Guid?> getGLJournalIdFunc,
            Func<object, decimal> getTotalAmountFunc,
            Func<Guid, Task<int>> getLineCountFunc)
        {
            var result = new InvoiceValidationResultDto { IsValid = true };

            var invoice = await getInvoiceFunc(invoiceId);
            if (invoice == null)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice not found - الفاتورة غير موجودة");
                return result;
            }

            // Check if draft
            if (getStatusFunc(invoice) != InvoiceStatus.Draft)
            {
                result.IsValid = false;
                result.Errors.Add("Only draft invoices can be posted - يمكن ترحيل المسودات فقط");
            }

            // Check if already posted
            if (getGLJournalIdFunc(invoice).HasValue)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice already posted - الفاتورة مرحّلة مسبقاً");
            }

            // Check if invoice amount is valid
            if (getTotalAmountFunc(invoice) <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice amount must be greater than zero - يجب أن يكون مبلغ الفاتورة أكبر من صفر");
            }

            // Check lines exist
            var lineCount = await getLineCountFunc(invoiceId);
            if (lineCount == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Invoice must have at least one line - يجب أن تحتوي الفاتورة على سطر واحد على الأقل");
            }

            // Period validation (placeholder - implement based on fiscal period logic)
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
                TotalDebit = getTotalAmountFunc(invoice),
                TotalCredit = getTotalAmountFunc(invoice),
                Difference = 0
            };

            return result;
        }

        #endregion

        #region Abstract Methods - Must be implemented by derived classes

        /// <summary>
        /// Create invoice line entity from DTO
        /// </summary>
        protected abstract TLine CreateLineEntity(
            TCreateLineDto lineDto,
            Guid invoiceId,
            int lineNumber,
            decimal taxRate,
            string userName);

        /// <summary>
        /// Update invoice line entity from DTO
        /// </summary>
        protected abstract TLine UpdateLineEntity(
            TUpdateLineDto lineDto,
            Guid invoiceId,
            int lineNumber,
            decimal taxRate,
            string userName);

        /// <summary>
        /// Get line quantity from DTO
        /// </summary>
        protected abstract decimal GetLineQuantity(TCreateLineDto lineDto);

        /// <summary>
        /// Get line unit price from DTO
        /// </summary>
        protected abstract decimal GetLineUnitPrice(TCreateLineDto lineDto);

        /// <summary>
        /// Get line tax code ID from DTO
        /// </summary>
        protected abstract Guid? GetLineTaxCodeId(TCreateLineDto lineDto);

        /// <summary>
        /// Get update line quantity from DTO
        /// </summary>
        protected abstract decimal GetUpdateLineQuantity(TUpdateLineDto lineDto);

        /// <summary>
        /// Get update line unit price from DTO
        /// </summary>
        protected abstract decimal GetUpdateLineUnitPrice(TUpdateLineDto lineDto);

        /// <summary>
        /// Get update line tax code ID from DTO
        /// </summary>
        protected abstract Guid? GetUpdateLineTaxCodeId(TUpdateLineDto lineDto);

        /// <summary>
        /// Get update line ID from DTO
        /// </summary>
        protected abstract Guid? GetUpdateLineId(TUpdateLineDto lineDto);

        #endregion
    }
}
