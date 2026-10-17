using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Service.Finance.ARAP.Common
{
    /// <summary>
    /// Invoice Helper - Static utility methods for invoice operations
    /// مساعد الفواتير - طرق ثابتة للعمليات المشتركة
    /// </summary>
    public static class InvoiceHelper
    {
        #region Number Generation

        /// <summary>
        /// Format invoice number with prefix, year, and sequence
        /// تنسيق رقم الفاتورة مع البادئة والسنة والتسلسل
        /// </summary>
        /// <param name="prefix">Prefix (e.g., "AR-INV", "AP-INV")</param>
        /// <param name="year">Fiscal year</param>
        /// <param name="sequence">Sequence number</param>
        /// <returns>Formatted invoice number (e.g., "AR-INV-2024-0001")</returns>
        public static string FormatInvoiceNumber(string prefix, int year, int sequence)
        {
            return $"{prefix}-{year}-{sequence:D4}";
        }

        /// <summary>
        /// Parse invoice number to extract components
        /// تحليل رقم الفاتورة لاستخراج المكونات
        /// </summary>
        /// <param name="invoiceNumber">Invoice number (e.g., "AR-INV-2024-0001")</param>
        /// <returns>Tuple of (prefix, year, sequence)</returns>
        public static (string prefix, int year, int sequence) ParseInvoiceNumber(string invoiceNumber)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
                throw new ArgumentException("Invoice number cannot be null or empty");

            var parts = invoiceNumber.Split('-');
            if (parts.Length < 4)
                throw new ArgumentException($"Invalid invoice number format: {invoiceNumber}");

            var prefix = string.Join("-", parts.Take(parts.Length - 2));
            var year = int.Parse(parts[^2]);
            var sequence = int.Parse(parts[^1]);

            return (prefix, year, sequence);
        }

        #endregion

        #region Amount Calculations

        /// <summary>
        /// Calculate subtotal from line amounts
        /// حساب المبلغ الفرعي من مبالغ البنود
        /// </summary>
        public static decimal CalculateSubtotal(IEnumerable<decimal> lineAmounts)
        {
            return Math.Round(lineAmounts.Sum(), 4);
        }

        /// <summary>
        /// Calculate total tax from line tax amounts
        /// حساب إجمالي الضريبة من مبالغ الضريبة للبنود
        /// </summary>
        public static decimal CalculateTotalTax(IEnumerable<decimal> lineTaxAmounts)
        {
            return Math.Round(lineTaxAmounts.Sum(), 4);
        }

        /// <summary>
        /// Calculate total amount (subtotal + tax)
        /// حساب المبلغ الإجمالي (الفرعي + الضريبة)
        /// </summary>
        public static decimal CalculateTotalAmount(decimal subtotal, decimal tax)
        {
            return Math.Round(subtotal + tax, 4);
        }

        /// <summary>
        /// Calculate remaining amount (total - paid)
        /// حساب المبلغ المتبقي (الإجمالي - المدفوع)
        /// </summary>
        public static decimal CalculateRemainingAmount(decimal total, decimal paid)
        {
            return Math.Round(total - paid, 4);
        }

        #endregion

        #region Status Helpers

        /// <summary>
        /// Check if invoice can be edited
        /// التحقق إذا كانت الفاتورة قابلة للتعديل
        /// </summary>
        public static bool CanEditInvoice(InvoiceStatus status)
        {
            return status == InvoiceStatus.Draft;
        }

        /// <summary>
        /// Check if invoice can be deleted
        /// التحقق إذا كانت الفاتورة قابلة للحذف
        /// </summary>
        public static bool CanDeleteInvoice(InvoiceStatus status, decimal paidAmount)
        {
            return status == InvoiceStatus.Draft && paidAmount == 0;
        }

        /// <summary>
        /// Check if invoice can be posted
        /// التحقق إذا كانت الفاتورة قابلة للترحيل
        /// </summary>
        public static bool CanPostInvoice(InvoiceStatus status, bool hasGLJournal)
        {
            return status == InvoiceStatus.Draft && !hasGLJournal;
        }

        /// <summary>
        /// Check if invoice can be cancelled
        /// التحقق إذا كانت الفاتورة قابلة للإلغاء
        /// </summary>
        public static bool CanCancelInvoice(InvoiceStatus status, decimal paidAmount)
        {
            return status != InvoiceStatus.Cancelled && paidAmount == 0;
        }

        /// <summary>
        /// Get days overdue
        /// الحصول على عدد أيام التأخير
        /// </summary>
        public static int GetDaysOverdue(DateTime dueDate)
        {
            if (dueDate >= DateTime.Now)
                return 0;

            return (int)(DateTime.Now - dueDate).TotalDays;
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Validate currency code
        /// التحقق من صحة رمز العملة
        /// </summary>
        public static bool IsValidCurrency(string currency)
        {
            if (string.IsNullOrEmpty(currency))
                return false;

            // Common currencies in Saudi ERP systems
            var validCurrencies = new[] { "SAR", "USD", "EUR", "GBP", "AED", "KWD", "BHD", "QAR", "OMR" };
            return validCurrencies.Contains(currency.ToUpperInvariant());
        }

        /// <summary>
        /// Validate exchange rate
        /// التحقق من صحة سعر الصرف
        /// </summary>
        public static bool IsValidExchangeRate(decimal exchangeRate)
        {
            return exchangeRate > 0;
        }

        /// <summary>
        /// Normalize currency code
        /// توحيد رمز العملة
        /// </summary>
        public static string NormalizeCurrency(string currency)
        {
            return string.IsNullOrEmpty(currency) ? "SAR" : currency.ToUpperInvariant();
        }

        /// <summary>
        /// Ensure exchange rate is valid (returns 1 if invalid)
        /// التأكد من صحة سعر الصرف (يرجع 1 إذا كان غير صالح)
        /// </summary>
        public static decimal EnsureValidExchangeRate(decimal exchangeRate)
        {
            return exchangeRate > 0 ? exchangeRate : 1;
        }

        #endregion

        #region Date Helpers

        /// <summary>
        /// Calculate default due date based on payment terms
        /// حساب تاريخ الاستحقاق الافتراضي بناءً على شروط الدفع
        /// </summary>
        /// <param name="invoiceDate">Invoice date</param>
        /// <param name="paymentTermDays">Payment term in days</param>
        /// <returns>Due date</returns>
        public static DateTime CalculateDueDate(DateTime invoiceDate, int paymentTermDays)
        {
            return invoiceDate.AddDays(paymentTermDays);
        }

        /// <summary>
        /// Validate date range
        /// التحقق من نطاق التاريخ
        /// </summary>
        public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return endDate >= startDate;
        }

        #endregion

        #region Formatting Helpers

        /// <summary>
        /// Format amount with currency
        /// تنسيق المبلغ مع العملة
        /// </summary>
        public static string FormatAmountWithCurrency(decimal amount, string currency)
        {
            return $"{amount:N2} {currency}";
        }

        /// <summary>
        /// Format status for display
        /// تنسيق الحالة للعرض
        /// </summary>
        public static string FormatStatusDisplay(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "Draft - مسودة",
                InvoiceStatus.Posted => "Posted - مرحّلة",
                InvoiceStatus.Paid => "Paid - مدفوعة",
                InvoiceStatus.PartiallyPaid => "Partially Paid - مدفوعة جزئياً",
                InvoiceStatus.Overdue => "Overdue - متأخرة",
                InvoiceStatus.Cancelled => "Cancelled - ملغاة",
                _ => status.ToString()
            };
        }

        #endregion

        #region Collection Helpers

        /// <summary>
        /// Check if collection is null or empty
        /// التحقق إذا كانت المجموعة فارغة أو null
        /// </summary>
        public static bool IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Safe count - returns 0 if collection is null
        /// عد آمن - يرجع 0 إذا كانت المجموعة null
        /// </summary>
        public static int SafeCount<T>(IEnumerable<T> collection)
        {
            return collection?.Count() ?? 0;
        }

        #endregion
    }
}
