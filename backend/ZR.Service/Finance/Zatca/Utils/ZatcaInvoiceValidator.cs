using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Service.Finance.Zatca.Utils
{
    /// <summary>
    /// ZATCA Invoice Validator
    /// مدقق فواتير زاتكا
    /// Validates invoices against ZATCA Phase 2 business rules
    /// </summary>
    public class ZatcaInvoiceValidator
    {
        #region Public Methods

        /// <summary>
        /// Validate invoice for ZATCA submission
        /// </summary>
        public ValidationResult ValidateInvoice(
            ARInvoice invoice,
            List<ARInvoiceLine> lines,
            ZatcaConfiguration config,
            CustomerInfo buyer,
            bool isB2B)
        {
            var result = new ValidationResult();

            // Header validation
            ValidateInvoiceHeader(invoice, result);

            // Seller validation
            ValidateSellerInfo(config, result);

            // Buyer validation
            ValidateBuyerInfo(buyer, isB2B, result);

            // Lines validation
            ValidateInvoiceLines(lines, result);

            // Tax validation
            ValidateTaxCalculations(invoice, lines, result);

            // Amount validation
            ValidateAmounts(invoice, lines, result);

            // ZATCA specific validations
            ValidateZatcaRequirements(invoice, config, result);

            return result;
        }

        /// <summary>
        /// Validate XML structure for ZATCA compliance
        /// </summary>
        public ValidationResult ValidateXml(string invoiceXml)
        {
            var result = new ValidationResult();

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(invoiceXml);

                // Check root element
                if (doc.DocumentElement?.LocalName != "Invoice" &&
                    doc.DocumentElement?.LocalName != "CreditNote" &&
                    doc.DocumentElement?.LocalName != "DebitNote")
                {
                    result.AddError("BR-XML-01", "Invalid root element. Expected Invoice, CreditNote, or DebitNote");
                }

                // Check required namespaces
                ValidateXmlNamespaces(doc, result);

                // Check required elements
                ValidateRequiredXmlElements(doc, result);

                // Check ICV and PIH
                ValidateIcvAndPih(doc, result);

                // Check signature placeholder
                ValidateSignaturePlaceholder(doc, result);
            }
            catch (XmlException ex)
            {
                result.AddError("BR-XML-PARSE", $"XML parsing error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validate signed XML
        /// </summary>
        public ValidationResult ValidateSignedXml(string signedXml)
        {
            var result = ValidateXml(signedXml);

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(signedXml);

                // Check signature element exists
                var signatureNodes = doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
                if (signatureNodes.Count == 0)
                {
                    result.AddError("BR-SIG-01", "Missing digital signature");
                }

                // Check SignatureValue
                var signatureValue = doc.GetElementsByTagName("SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
                if (signatureValue.Count == 0 || string.IsNullOrEmpty(signatureValue[0]?.InnerText))
                {
                    result.AddError("BR-SIG-02", "Missing signature value");
                }

                // Check X509Certificate
                var certificate = doc.GetElementsByTagName("X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
                if (certificate.Count == 0 || string.IsNullOrEmpty(certificate[0]?.InnerText))
                {
                    result.AddError("BR-SIG-03", "Missing X509 certificate");
                }

                // Check QR code is populated
                ValidateQrCode(doc, result);
            }
            catch (Exception ex)
            {
                result.AddError("BR-SIG-PARSE", $"Signed XML validation error: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Private Validation Methods

        private void ValidateInvoiceHeader(ARInvoice invoice, ValidationResult result)
        {
            // Invoice Number
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
            {
                result.AddError("BR-KSA-01", "Invoice number is required - رقم الفاتورة مطلوب");
            }
            else if (invoice.InvoiceNumber.Length > 127)
            {
                result.AddError("BR-KSA-02", "Invoice number exceeds maximum length of 127 characters");
            }

            // Invoice Date
            if (invoice.InvoiceDate == default)
            {
                result.AddError("BR-KSA-03", "Invoice date is required - تاريخ الفاتورة مطلوب");
            }
            else if (invoice.InvoiceDate > DateTime.Now.AddDays(1))
            {
                result.AddError("BR-KSA-04", "Invoice date cannot be in the future - لا يمكن أن يكون تاريخ الفاتورة في المستقبل");
            }

            // Supply Date
            if (!invoice.SupplyDate.HasValue)
            {
                result.AddWarning("BR-KSA-05", "Supply date is recommended - يُنصح بتاريخ التوريد");
            }

            // Currency
            if (string.IsNullOrWhiteSpace(invoice.Currency))
            {
                result.AddError("BR-KSA-06", "Currency code is required - رمز العملة مطلوب");
            }
            else if (invoice.Currency.Length != 3)
            {
                result.AddError("BR-KSA-07", "Currency code must be 3 characters (ISO 4217)");
            }

            // Invoice Status
            if (invoice.Status != Model.Finance.ARAP.Enums.InvoiceStatus.Posted)
            {
                result.AddError("BR-KSA-08", "Only posted invoices can be submitted to ZATCA - يمكن إرسال الفواتير المرحّلة فقط لزاتكا");
            }
        }

        private void ValidateSellerInfo(ZatcaConfiguration config, ValidationResult result)
        {
            if (config == null)
            {
                result.AddError("BR-KSA-10", "ZATCA configuration is required - إعدادات زاتكا مطلوبة");
                return;
            }

            // VAT Number
            if (string.IsNullOrWhiteSpace(config.SellerVatNumber))
            {
                result.AddError("BR-KSA-11", "Seller VAT number is required - الرقم الضريبي للبائع مطلوب");
            }
            else if (!Regex.IsMatch(config.SellerVatNumber, @"^3\d{13}3$"))
            {
                result.AddError("BR-KSA-12", "Seller VAT number must be 15 digits starting and ending with 3");
            }

            // Seller Name (Arabic)
            if (string.IsNullOrWhiteSpace(config.SellerNameAr))
            {
                result.AddError("BR-KSA-13", "Seller name (Arabic) is required - اسم البائع بالعربية مطلوب");
            }

            // Address fields
            if (string.IsNullOrWhiteSpace(config.SellerStreet))
            {
                result.AddError("BR-KSA-14", "Seller street is required - شارع البائع مطلوب");
            }

            if (string.IsNullOrWhiteSpace(config.SellerBuildingNumber))
            {
                result.AddError("BR-KSA-15", "Seller building number is required - رقم مبنى البائع مطلوب");
            }
            else if (!Regex.IsMatch(config.SellerBuildingNumber, @"^\d{4}$"))
            {
                result.AddError("BR-KSA-16", "Seller building number must be 4 digits");
            }

            if (string.IsNullOrWhiteSpace(config.SellerCity))
            {
                result.AddError("BR-KSA-17", "Seller city is required - مدينة البائع مطلوبة");
            }

            if (string.IsNullOrWhiteSpace(config.SellerCitySubdivision))
            {
                result.AddError("BR-KSA-18", "Seller district is required - حي البائع مطلوب");
            }

            if (string.IsNullOrWhiteSpace(config.SellerPostalCode))
            {
                result.AddError("BR-KSA-19", "Seller postal code is required - الرمز البريدي للبائع مطلوب");
            }
            else if (!Regex.IsMatch(config.SellerPostalCode, @"^\d{5}$"))
            {
                result.AddError("BR-KSA-20", "Seller postal code must be 5 digits");
            }
        }

        private void ValidateBuyerInfo(CustomerInfo buyer, bool isB2B, ValidationResult result)
        {
            if (isB2B)
            {
                // B2B requires full buyer details
                if (buyer == null)
                {
                    result.AddError("BR-KSA-25", "Buyer information is required for B2B invoices");
                    return;
                }

                if (string.IsNullOrWhiteSpace(buyer.VatNumber))
                {
                    result.AddError("BR-KSA-26", "Buyer VAT number is required for B2B invoices");
                }
                else if (!Regex.IsMatch(buyer.VatNumber, @"^3\d{13}3$"))
                {
                    result.AddError("BR-KSA-27", "Buyer VAT number must be 15 digits starting and ending with 3");
                }

                if (string.IsNullOrWhiteSpace(buyer.Name))
                {
                    result.AddError("BR-KSA-28", "Buyer name is required for B2B invoices");
                }
            }
            else
            {
                // B2C - minimal buyer info required
                // No specific validation needed
            }
        }

        private void ValidateInvoiceLines(List<ARInvoiceLine> lines, ValidationResult result)
        {
            if (lines == null || lines.Count == 0)
            {
                result.AddError("BR-KSA-30", "At least one invoice line is required - مطلوب سطر فاتورة واحد على الأقل");
                return;
            }

            int lineNumber = 1;
            foreach (var line in lines)
            {
                // Line quantity
                if (line.Quantity <= 0)
                {
                    result.AddError("BR-KSA-31", $"Line {lineNumber}: Quantity must be greater than zero");
                }

                // Line amount
                if (line.Amount < 0)
                {
                    result.AddError("BR-KSA-32", $"Line {lineNumber}: Amount cannot be negative");
                }

                // Description
                if (string.IsNullOrWhiteSpace(line.Description))
                {
                    result.AddWarning("BR-KSA-33", $"Line {lineNumber}: Description is recommended");
                }

                // Tax rate validation
                if (line.TaxRate < 0 || line.TaxRate > 100)
                {
                    result.AddError("BR-KSA-34", $"Line {lineNumber}: Tax rate must be between 0 and 100");
                }

                // Tax exemption reason for non-standard rates
                if (line.ZatcaTaxCategory != null &&
                    line.ZatcaTaxCategory != ZatcaTaxCategory.StandardRate &&
                    string.IsNullOrWhiteSpace(line.TaxExemptionReasonCode))
                {
                    result.AddError("BR-KSA-35", $"Line {lineNumber}: Tax exemption reason code is required for non-standard tax categories");
                }

                lineNumber++;
            }
        }

        private void ValidateTaxCalculations(ARInvoice invoice, List<ARInvoiceLine> lines, ValidationResult result)
        {
            // Calculate expected totals
            var calculatedSubtotal = lines.Sum(l => l.Amount);
            var calculatedTax = lines.Sum(l => l.TaxAmount);
            var calculatedTotal = calculatedSubtotal + calculatedTax;

            // Tolerance for rounding
            const decimal tolerance = 0.01m;

            // Validate subtotal
            if (Math.Abs(invoice.Subtotal - calculatedSubtotal) > tolerance)
            {
                result.AddError("BR-KSA-40", $"Subtotal mismatch. Invoice: {invoice.Subtotal}, Calculated: {calculatedSubtotal}");
            }

            // Validate tax amount
            if (Math.Abs(invoice.TaxAmount - calculatedTax) > tolerance)
            {
                result.AddError("BR-KSA-41", $"Tax amount mismatch. Invoice: {invoice.TaxAmount}, Calculated: {calculatedTax}");
            }

            // Validate total
            if (Math.Abs(invoice.TotalAmount - calculatedTotal) > tolerance)
            {
                result.AddError("BR-KSA-42", $"Total amount mismatch. Invoice: {invoice.TotalAmount}, Calculated: {calculatedTotal}");
            }

            // Validate line-level tax calculations
            int lineNumber = 1;
            foreach (var line in lines)
            {
                var expectedTax = line.Amount * line.TaxRate / 100m;
                if (Math.Abs(line.TaxAmount - expectedTax) > tolerance)
                {
                    result.AddWarning("BR-KSA-43", $"Line {lineNumber}: Tax amount may be incorrect. Expected: {expectedTax:F2}, Actual: {line.TaxAmount}");
                }
                lineNumber++;
            }
        }

        private void ValidateAmounts(ARInvoice invoice, List<ARInvoiceLine> lines, ValidationResult result)
        {
            // Total must be positive
            if (invoice.TotalAmount <= 0)
            {
                result.AddError("BR-KSA-50", "Invoice total amount must be greater than zero");
            }

            // For foreign currency, check exchange rate
            if (invoice.Currency != "SAR" && invoice.ExchangeRate <= 0)
            {
                result.AddError("BR-KSA-51", "Exchange rate is required for foreign currency invoices");
            }
        }

        private void ValidateZatcaRequirements(ARInvoice invoice, ZatcaConfiguration config, ValidationResult result)
        {
            // Check if ZATCA is enabled
            if (config != null && !config.IsEnabled)
            {
                result.AddWarning("BR-KSA-60", "ZATCA integration is disabled");
            }

            // Check for UUID if already assigned
            if (!string.IsNullOrEmpty(invoice.ZatcaUuid))
            {
                if (!Guid.TryParse(invoice.ZatcaUuid, out _))
                {
                    result.AddError("BR-KSA-61", "Invalid ZATCA UUID format");
                }
            }

            // Payment means code
            if (invoice.ZatcaPaymentMeansCode == null && config?.DefaultPaymentMeansCode == null)
            {
                result.AddWarning("BR-KSA-62", "Payment means code is recommended");
            }
        }

        private void ValidateXmlNamespaces(XmlDocument doc, ValidationResult result)
        {
            var requiredNamespaces = new[]
            {
                "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2",
                "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
            };

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);

            foreach (var ns in requiredNamespaces)
            {
                var hasNamespace = doc.DocumentElement.Attributes
                    .Cast<XmlAttribute>()
                    .Any(a => a.Value == ns);

                if (!hasNamespace)
                {
                    result.AddError("BR-XML-NS", $"Missing required namespace: {ns}");
                }
            }
        }

        private void ValidateRequiredXmlElements(XmlDocument doc, ValidationResult result)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Required elements
            var requiredElements = new[]
            {
                ("//cbc:ID", "Invoice ID"),
                ("//cbc:UUID", "Invoice UUID"),
                ("//cbc:IssueDate", "Issue Date"),
                ("//cbc:InvoiceTypeCode", "Invoice Type Code"),
                ("//cbc:DocumentCurrencyCode", "Document Currency Code"),
                ("//cac:AccountingSupplierParty", "Supplier Party"),
                ("//cac:LegalMonetaryTotal", "Legal Monetary Total")
            };

            foreach (var (xpath, name) in requiredElements)
            {
                var node = doc.SelectSingleNode(xpath, ns);
                if (node == null)
                {
                    result.AddError("BR-XML-REQ", $"Missing required element: {name}");
                }
            }
        }

        private void ValidateIcvAndPih(XmlDocument doc, ValidationResult result)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Check ICV reference
            var icvNode = doc.SelectSingleNode("//cac:AdditionalDocumentReference[cbc:ID='ICV']/cbc:UUID", ns);
            if (icvNode == null || string.IsNullOrEmpty(icvNode.InnerText))
            {
                result.AddError("BR-KSA-ICV", "Invoice Counter Value (ICV) is required");
            }
            else if (!long.TryParse(icvNode.InnerText, out var icv) || icv <= 0)
            {
                result.AddError("BR-KSA-ICV-FORMAT", "ICV must be a positive integer");
            }

            // Check PIH reference
            var pihNode = doc.SelectSingleNode("//cac:AdditionalDocumentReference[cbc:ID='PIH']//cbc:EmbeddedDocumentBinaryObject", ns);
            if (pihNode == null || string.IsNullOrEmpty(pihNode.InnerText))
            {
                result.AddError("BR-KSA-PIH", "Previous Invoice Hash (PIH) is required");
            }
        }

        private void ValidateSignaturePlaceholder(XmlDocument doc, ValidationResult result)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            var signatureRef = doc.SelectSingleNode("//cac:Signature", ns);
            if (signatureRef == null)
            {
                result.AddWarning("BR-XML-SIG", "Signature reference element not found");
            }
        }

        private void ValidateQrCode(XmlDocument doc, ValidationResult result)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            var qrNode = doc.SelectSingleNode("//cac:AdditionalDocumentReference[cbc:ID='QR']//cbc:EmbeddedDocumentBinaryObject", ns);
            if (qrNode == null || string.IsNullOrEmpty(qrNode.InnerText) || qrNode.InnerText.Contains("PLACEHOLDER"))
            {
                result.AddError("BR-KSA-QR", "QR code is required in signed invoice");
            }
        }

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// Validation result containing errors and warnings
    /// </summary>
    public class ValidationResult
    {
        public List<ValidationMessage> Errors { get; } = new();
        public List<ValidationMessage> Warnings { get; } = new();

        public bool IsValid => Errors.Count == 0;
        public bool HasWarnings => Warnings.Count > 0;

        public void AddError(string code, string message)
        {
            Errors.Add(new ValidationMessage { Code = code, Message = message });
        }

        public void AddWarning(string code, string message)
        {
            Warnings.Add(new ValidationMessage { Code = code, Message = message });
        }

        public string[] GetErrorMessages() => Errors.Select(e => $"[{e.Code}] {e.Message}").ToArray();
        public string[] GetWarningMessages() => Warnings.Select(w => $"[{w.Code}] {w.Message}").ToArray();
    }

    /// <summary>
    /// Individual validation message
    /// </summary>
    public class ValidationMessage
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    #endregion
}
