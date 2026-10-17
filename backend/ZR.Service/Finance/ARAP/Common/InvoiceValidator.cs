using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Service.Finance.ARAP.Common
{
    /// <summary>
    /// Invoice Validator - Validation logic for invoices
    /// E-BB 'DAH'*J1 - EF7B 'D*-BB EF 5-) 'DAH'*J1
    /// </summary>
    public static class InvoiceValidator
    {
        #region Basic Validations

        /// <summary>
        /// Validate invoice number - 'D*-BB EF 1BE 'DA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidateInvoiceNumber(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                return (false, "Invoice number is required - 1BE 'DA'*H1) E7DH(");

            if (invoiceNumber.Length > 50)
                return (false, "Invoice number cannot exceed 50 characters - 1BE 'DA'*H1) D' JECF #F J*,'H2 50 -1A");

            return (true, null);
        }

        /// <summary>
        /// Validate invoice dates - 'D*-BB EF *H'1J. 'DA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidateDates(DateTime invoiceDate, DateTime dueDate)
        {
            if (dueDate < invoiceDate)
                return (false, "Due date must be greater than or equal to invoice date - *'1J. 'D'3*-B'B J,( #F JCHF #C(1 EF #H J3'HJ *'1J. 'DA'*H1)");

            // Optional: Check if invoice date is not too far in the past
            var maxPastDays = 365; // 1 year
            if (invoiceDate < DateTime.Now.AddDays(-maxPastDays))
                return (false, $"Invoice date cannot be more than {maxPastDays} days in the past - *'1J. 'DA'*H1) D' JECF #F JCHF #C+1 EF {maxPastDays} JHE AJ 'DE'6J");

            // Optional: Check if invoice date is not too far in the future
            var maxFutureDays = 30;
            if (invoiceDate > DateTime.Now.AddDays(maxFutureDays))
                return (false, $"Invoice date cannot be more than {maxFutureDays} days in the future - *'1J. 'DA'*H1) D' JECF #F JCHF #C+1 EF {maxFutureDays} JHE AJ 'DE3*B(D");

            return (true, null);
        }

        /// <summary>
        /// Validate currency and exchange rate
        /// 'D*-BB EF 'D9ED) H391 'D51A
        /// </summary>
        public static (bool isValid, string error) ValidateCurrencyAndRate(string currency, decimal exchangeRate)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return (false, "Currency is required - 'D9ED) E7DH()");

            if (!InvoiceHelper.IsValidCurrency(currency))
                return (false, $"Invalid currency code: {currency} - 1E2 9ED) :J1 5'D-");

            if (exchangeRate <= 0)
                return (false, "Exchange rate must be greater than zero - 391 'D51A J,( #F JCHF #C(1 EF 5A1");

            return (true, null);
        }

        /// <summary>
        /// Validate invoice amount - 'D*-BB EF E(D: 'DA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidateAmount(decimal amount, string fieldName = "Amount")
        {
            if (amount < 0)
                return (false, $"{fieldName} cannot be negative - {fieldName} D' JECF #F JCHF 3'D(");

            // Optional: Max amount validation (adjust as needed)
            var maxAmount = 999999999999.9999m; // 12 digits + 4 decimals
            if (amount > maxAmount)
                return (false, $"{fieldName} exceeds maximum allowed value - {fieldName} J*,'H2 'D-/ 'D#B5I 'DE3EH-");

            return (true, null);
        }

        #endregion

        #region Line Validations

        /// <summary>
        /// Validate invoice lines exist
        /// 'D*-BB EF H,H/ (FH/ 'DA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidateLinesExist<T>(IList<T> lines) where T : class
        {
            if (lines == null || lines.Count == 0)
                return (false, "At least one invoice line is required - J,( %6'A) 371 H'-/ 9DI 'D#BD");

            return (true, null);
        }

        /// <summary>
        /// Validate line quantity and unit price
        /// 'D*-BB EF CEJ) H391 'DH-/) DD371
        /// </summary>
        public static (bool isValid, string error) ValidateLineAmounts(decimal quantity, decimal unitPrice, int lineNumber)
        {
            if (quantity <= 0)
                return (false, $"Line {lineNumber}: Quantity must be greater than zero - 'D371 {lineNumber}: 'DCEJ) J,( #F *CHF #C(1 EF 5A1");

            if (unitPrice < 0)
                return (false, $"Line {lineNumber}: Unit price cannot be negative - 'D371 {lineNumber}: 391 'DH-/) D' JECF #F JCHF 3'D(");

            return (true, null);
        }

        /// <summary>
        /// Validate line account ID
        /// 'D*-BB EF E91A 'D-3'( DD371
        /// </summary>
        public static (bool isValid, string error) ValidateLineAccountId(Guid? accountId, int lineNumber)
        {
            if (!accountId.HasValue || accountId == Guid.Empty)
                return (false, $"Line {lineNumber}: Account is required - 'D371 {lineNumber}: 'D-3'( E7DH(");

            return (true, null);
        }

        #endregion

        #region Status Validations

        /// <summary>
        /// Validate invoice status for operation
        /// 'D*-BB EF -'D) 'DA'*H1) DD9EDJ)
        /// </summary>
        public static (bool isValid, string error) ValidateStatusForEdit(InvoiceStatus status)
        {
            if (status != InvoiceStatus.Draft)
                return (false, "Only draft invoices can be edited - JECF *9/JD 'DE3H/'* AB7");

            return (true, null);
        }

        /// <summary>
        /// Validate invoice can be deleted
        /// 'D*-BB EF %EC'FJ) -0A 'DA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidateStatusForDelete(InvoiceStatus status, decimal paidAmount)
        {
            if (status != InvoiceStatus.Draft)
                return (false, "Only draft invoices can be deleted - JECF -0A 'DE3H/'* AB7");

            if (paidAmount > 0)
                return (false, "Cannot delete invoice with payments - D' JECF -0A A'*H1) DG' E/AH9'*");

            return (true, null);
        }

        /// <summary>
        /// Validate invoice can be posted
        /// 'D*-BB EF %EC'FJ) *1-JD 'DA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidateStatusForPost(InvoiceStatus status, bool hasGLJournal)
        {
            if (status != InvoiceStatus.Draft)
                return (false, "Only draft invoices can be posted - JECF *1-JD 'DE3H/'* AB7");

            if (hasGLJournal)
                return (false, "Invoice already posted - 'DA'*H1) E1-QD) E3(B'K");

            return (true, null);
        }

        /// <summary>
        /// Validate invoice can be cancelled
        /// 'D*-BB EF %EC'FJ) %D:'! 'DA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidateStatusForCancel(InvoiceStatus status, decimal paidAmount)
        {
            if (status == InvoiceStatus.Cancelled)
                return (false, "Invoice is already cancelled - 'DA'*H1) ED:') ('DA9D");

            if (paidAmount > 0)
                return (false, "Cannot cancel invoice with payments - D' JECF %D:'! A'*H1) DG' E/AH9'*");

            return (true, null);
        }

        #endregion

        #region Batch Validations

        /// <summary>
        /// Validate all invoice fields
        /// 'D*-BB EF ,EJ9 -BHD 'DA'*H1)
        /// </summary>
        public static List<string> ValidateInvoice(
            string invoiceNumber,
            DateTime invoiceDate,
            DateTime dueDate,
            string currency,
            decimal exchangeRate,
            decimal totalAmount,
            int lineCount)
        {
            var errors = new List<string>();

            var (numberValid, numberError) = ValidateInvoiceNumber(invoiceNumber);
            if (!numberValid) errors.Add(numberError);

            var (datesValid, datesError) = ValidateDates(invoiceDate, dueDate);
            if (!datesValid) errors.Add(datesError);

            var (currencyValid, currencyError) = ValidateCurrencyAndRate(currency, exchangeRate);
            if (!currencyValid) errors.Add(currencyError);

            var (amountValid, amountError) = ValidateAmount(totalAmount, "Total amount");
            if (!amountValid) errors.Add(amountError);

            if (lineCount == 0)
                errors.Add("At least one invoice line is required - J,( %6'A) 371 H'-/ 9DI 'D#BD");

            return errors;
        }

        /// <summary>
        /// Validate invoice line fields
        /// 'D*-BB EF -BHD 371 'DA'*H1)
        /// </summary>
        public static List<string> ValidateInvoiceLine(
            int lineNumber,
            Guid? accountId,
            decimal quantity,
            decimal unitPrice)
        {
            var errors = new List<string>();

            var (accountValid, accountError) = ValidateLineAccountId(accountId, lineNumber);
            if (!accountValid) errors.Add(accountError);

            var (amountsValid, amountsError) = ValidateLineAmounts(quantity, unitPrice, lineNumber);
            if (!amountsValid) errors.Add(amountsError);

            return errors;
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Validate payment amount against invoice remaining amount
        /// 'D*-BB EF E(D: 'D/A9 EB'(D 'DE(D: 'DE*(BJ DDA'*H1)
        /// </summary>
        public static (bool isValid, string error) ValidatePaymentAmount(decimal paymentAmount, decimal remainingAmount)
        {
            if (paymentAmount <= 0)
                return (false, "Payment amount must be greater than zero - E(D: 'D/A9 J,( #F JCHF #C(1 EF 5A1");

            if (paymentAmount > remainingAmount)
                return (false, $"Payment amount ({paymentAmount}) exceeds remaining amount ({remainingAmount}) - E(D: 'D/A9 J*,'H2 'DE(D: 'DE*(BJ");

            return (true, null);
        }

        /// <summary>
        /// Validate fiscal period is open
        /// 'D*-BB EF #F 'DA*1) 'DE-'3(J) EA*H-)
        /// </summary>
        public static (bool isValid, string error) ValidateFiscalPeriod(bool isClosed, bool isLocked, string periodName)
        {
            if (isClosed)
                return (false, $"Fiscal period '{periodName}' is closed - 'DA*1) 'DE-'3(J) '{periodName}' E:DB)");

            if (isLocked)
                return (false, $"Fiscal period '{periodName}' is locked - 'DA*1) 'DE-'3(J) '{periodName}' EBAD)");

            return (true, null);
        }

        /// <summary>
        /// Validate journal entries are balanced
        /// 'D*-BB EF *H'2F BJH/ 'DJHEJ)
        /// </summary>
        public static (bool isValid, string error) ValidateJournalBalance(decimal totalDebit, decimal totalCredit)
        {
            var difference = Math.Abs(totalDebit - totalCredit);
            if (difference > 0.01m) // Allow 1 cent difference due to rounding
                return (false, $"Journal entries are not balanced. Difference: {difference:N4} - BJH/ 'DJHEJ) :J1 E*H'2F). 'DA1B: {difference:N4}");

            return (true, null);
        }

        #endregion
    }
}
