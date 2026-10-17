using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Service.Finance.Zatca.Utils
{
    /// <summary>
    /// ZATCA UBL 2.1 XML Builder
    /// بناء XML وفق معيار UBL 2.1 لزاتكا
    /// Generates compliant UBL 2.1 XML invoices for ZATCA Phase 2
    /// </summary>
    public class ZatcaUblBuilder
    {
        #region XML Namespaces

        private const string UblInvoiceNamespace = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        private const string CacNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private const string CbcNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private const string ExtNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        private const string SigNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2";
        private const string SacNamespace = "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2";
        private const string SbcNamespace = "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2";
        private const string DsNamespace = "http://www.w3.org/2000/09/xmldsig#";
        private const string XadesNamespace = "http://uri.etsi.org/01903/v1.3.2#";

        #endregion

        #region Public Methods

        /// <summary>
        /// Build UBL 2.1 Invoice XML for ZATCA
        /// </summary>
        public string BuildInvoiceXml(
            ARInvoice invoice,
            List<ARInvoiceLine> lines,
            ZatcaConfiguration config,
            ZatcaInvoiceHash hashRecord,
            CustomerInfo buyer)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                WriteInvoiceRoot(writer, invoice, lines, config, hashRecord, buyer);
                writer.WriteEndDocument();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Build Credit Note XML for ZATCA
        /// </summary>
        public string BuildCreditNoteXml(
            ARInvoice creditNote,
            List<ARInvoiceLine> lines,
            ZatcaConfiguration config,
            ZatcaInvoiceHash hashRecord,
            CustomerInfo buyer,
            string originalInvoiceNumber,
            DateTime originalInvoiceDate)
        {
            // Credit notes follow same structure with different root element and billing reference
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                WriteCreditNoteRoot(writer, creditNote, lines, config, hashRecord, buyer, originalInvoiceNumber, originalInvoiceDate);
                writer.WriteEndDocument();
            }

            return sb.ToString();
        }

        #endregion

        #region Invoice Building

        private void WriteInvoiceRoot(
            XmlWriter writer,
            ARInvoice invoice,
            List<ARInvoiceLine> lines,
            ZatcaConfiguration config,
            ZatcaInvoiceHash hashRecord,
            CustomerInfo buyer)
        {
            writer.WriteStartElement("Invoice", UblInvoiceNamespace);

            // Write namespace declarations
            WriteNamespaceDeclarations(writer);

            // UBL Extensions (for signature placeholder)
            WriteUblExtensions(writer);

            // Invoice Header
            WriteElement(writer, "cbc", CbcNamespace, "ProfileID", "reporting:1.0");
            WriteElement(writer, "cbc", CbcNamespace, "ID", invoice.InvoiceNumber);
            WriteElement(writer, "cbc", CbcNamespace, "UUID", hashRecord.InvoiceUuid);
            WriteElement(writer, "cbc", CbcNamespace, "IssueDate", invoice.InvoiceDate.ToString("yyyy-MM-dd"));
            WriteElement(writer, "cbc", CbcNamespace, "IssueTime", invoice.InvoiceDate.ToString("HH:mm:ss"));

            // Invoice Type Code with subtype
            WriteInvoiceTypeCode(writer, hashRecord.ZatcaInvoiceType, hashRecord.ZatcaInvoiceSubType);

            WriteElement(writer, "cbc", CbcNamespace, "DocumentCurrencyCode", invoice.Currency ?? "SAR");
            WriteElement(writer, "cbc", CbcNamespace, "TaxCurrencyCode", "SAR");

            // Additional Document References (ICV, PIH, QR placeholder)
            WriteAdditionalDocumentReferences(writer, hashRecord);

            // Signature placeholder
            WriteSignaturePlaceholder(writer);

            // Supplier Party (Seller)
            WriteSupplierParty(writer, config);

            // Customer Party (Buyer)
            WriteCustomerParty(writer, buyer, hashRecord.ZatcaInvoiceSubType);

            // Delivery (Supply Date)
            if (invoice.SupplyDate.HasValue)
            {
                WriteDelivery(writer, invoice.SupplyDate.Value, invoice.SupplyEndDate);
            }

            // Payment Means
            WritePaymentMeans(writer, invoice.ZatcaPaymentMeansCode ?? config.DefaultPaymentMeansCode);

            // Allowance/Charge (if any discounts)
            // WriteAllowanceCharge(writer, invoice);

            // Tax Total
            WriteTaxTotal(writer, invoice, lines);

            // Legal Monetary Total
            WriteLegalMonetaryTotal(writer, invoice);

            // Invoice Lines
            WriteInvoiceLines(writer, lines, invoice.Currency ?? "SAR");

            writer.WriteEndElement(); // Invoice
        }

        private void WriteCreditNoteRoot(
            XmlWriter writer,
            ARInvoice creditNote,
            List<ARInvoiceLine> lines,
            ZatcaConfiguration config,
            ZatcaInvoiceHash hashRecord,
            CustomerInfo buyer,
            string originalInvoiceNumber,
            DateTime originalInvoiceDate)
        {
            writer.WriteStartElement("CreditNote", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");

            WriteNamespaceDeclarations(writer);
            WriteUblExtensions(writer);

            WriteElement(writer, "cbc", CbcNamespace, "ProfileID", "reporting:1.0");
            WriteElement(writer, "cbc", CbcNamespace, "ID", creditNote.InvoiceNumber);
            WriteElement(writer, "cbc", CbcNamespace, "UUID", hashRecord.InvoiceUuid);
            WriteElement(writer, "cbc", CbcNamespace, "IssueDate", creditNote.InvoiceDate.ToString("yyyy-MM-dd"));
            WriteElement(writer, "cbc", CbcNamespace, "IssueTime", creditNote.InvoiceDate.ToString("HH:mm:ss"));

            WriteInvoiceTypeCode(writer, hashRecord.ZatcaInvoiceType, hashRecord.ZatcaInvoiceSubType);

            WriteElement(writer, "cbc", CbcNamespace, "DocumentCurrencyCode", creditNote.Currency ?? "SAR");
            WriteElement(writer, "cbc", CbcNamespace, "TaxCurrencyCode", "SAR");

            // Billing Reference (original invoice)
            WriteBillingReference(writer, originalInvoiceNumber, originalInvoiceDate);

            WriteAdditionalDocumentReferences(writer, hashRecord);
            WriteSignaturePlaceholder(writer);
            WriteSupplierParty(writer, config);
            WriteCustomerParty(writer, buyer, hashRecord.ZatcaInvoiceSubType);

            if (creditNote.SupplyDate.HasValue)
            {
                WriteDelivery(writer, creditNote.SupplyDate.Value, creditNote.SupplyEndDate);
            }

            WritePaymentMeans(writer, creditNote.ZatcaPaymentMeansCode ?? config.DefaultPaymentMeansCode);
            WriteTaxTotal(writer, creditNote, lines);
            WriteLegalMonetaryTotal(writer, creditNote);
            WriteInvoiceLines(writer, lines, creditNote.Currency ?? "SAR");

            writer.WriteEndElement();
        }

        #endregion

        #region Header Elements

        private void WriteNamespaceDeclarations(XmlWriter writer)
        {
            writer.WriteAttributeString("xmlns", "cac", null, CacNamespace);
            writer.WriteAttributeString("xmlns", "cbc", null, CbcNamespace);
            writer.WriteAttributeString("xmlns", "ext", null, ExtNamespace);
            writer.WriteAttributeString("xmlns", "sig", null, SigNamespace);
            writer.WriteAttributeString("xmlns", "sac", null, SacNamespace);
            writer.WriteAttributeString("xmlns", "sbc", null, SbcNamespace);
            writer.WriteAttributeString("xmlns", "ds", null, DsNamespace);
            writer.WriteAttributeString("xmlns", "xades", null, XadesNamespace);
        }

        private void WriteUblExtensions(XmlWriter writer)
        {
            writer.WriteStartElement("ext", "UBLExtensions", ExtNamespace);
            writer.WriteStartElement("ext", "UBLExtension", ExtNamespace);
            writer.WriteStartElement("ext", "ExtensionURI", ExtNamespace);
            writer.WriteString("urn:oasis:names:specification:ubl:dsig:enveloped:xades");
            writer.WriteEndElement();
            writer.WriteStartElement("ext", "ExtensionContent", ExtNamespace);
            // Signature will be inserted here by ZatcaXmlSigner
            writer.WriteComment("SIGNATURE_PLACEHOLDER");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteInvoiceTypeCode(XmlWriter writer, ZatcaInvoiceType type, ZatcaInvoiceSubType subType)
        {
            writer.WriteStartElement("cbc", "InvoiceTypeCode", CbcNamespace);
            writer.WriteAttributeString("name", GetInvoiceSubTypeCode(subType));
            writer.WriteString(((int)type).ToString());
            writer.WriteEndElement();
        }

        private string GetInvoiceSubTypeCode(ZatcaInvoiceSubType subType)
        {
            return subType switch
            {
                ZatcaInvoiceSubType.StandardInvoice => "0100000",
                ZatcaInvoiceSubType.SimplifiedInvoice => "0200000",
                ZatcaInvoiceSubType.StandardCreditNote => "0100000",
                ZatcaInvoiceSubType.StandardDebitNote => "0100000",
                ZatcaInvoiceSubType.SimplifiedCreditNote => "0200000",
                ZatcaInvoiceSubType.SimplifiedDebitNote => "0200000",
                _ => "0100000"
            };
        }

        private void WriteAdditionalDocumentReferences(XmlWriter writer, ZatcaInvoiceHash hashRecord)
        {
            // ICV (Invoice Counter Value)
            writer.WriteStartElement("cac", "AdditionalDocumentReference", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "ID", "ICV");
            WriteElement(writer, "cbc", CbcNamespace, "UUID", hashRecord.InvoiceCounterValue.ToString());
            writer.WriteEndElement();

            // PIH (Previous Invoice Hash)
            writer.WriteStartElement("cac", "AdditionalDocumentReference", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "ID", "PIH");
            writer.WriteStartElement("cac", "Attachment", CacNamespace);
            writer.WriteStartElement("cbc", "EmbeddedDocumentBinaryObject", CbcNamespace);
            writer.WriteAttributeString("mimeCode", "text/plain");
            writer.WriteString(hashRecord.PreviousInvoiceHash);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            // QR Code placeholder (will be filled after signing)
            writer.WriteStartElement("cac", "AdditionalDocumentReference", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "ID", "QR");
            writer.WriteStartElement("cac", "Attachment", CacNamespace);
            writer.WriteStartElement("cbc", "EmbeddedDocumentBinaryObject", CbcNamespace);
            writer.WriteAttributeString("mimeCode", "text/plain");
            writer.WriteComment("QR_CODE_PLACEHOLDER");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteSignaturePlaceholder(XmlWriter writer)
        {
            writer.WriteStartElement("cac", "Signature", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "ID", "urn:oasis:names:specification:ubl:signature:Invoice");
            WriteElement(writer, "cbc", CbcNamespace, "SignatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades");
            writer.WriteEndElement();
        }

        private void WriteBillingReference(XmlWriter writer, string originalInvoiceNumber, DateTime originalInvoiceDate)
        {
            writer.WriteStartElement("cac", "BillingReference", CacNamespace);
            writer.WriteStartElement("cac", "InvoiceDocumentReference", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "ID", originalInvoiceNumber);
            WriteElement(writer, "cbc", CbcNamespace, "IssueDate", originalInvoiceDate.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        #endregion

        #region Party Elements

        private void WriteSupplierParty(XmlWriter writer, ZatcaConfiguration config)
        {
            writer.WriteStartElement("cac", "AccountingSupplierParty", CacNamespace);
            writer.WriteStartElement("cac", "Party", CacNamespace);

            // Party Identification (VAT Number with scheme)
            writer.WriteStartElement("cac", "PartyIdentification", CacNamespace);
            writer.WriteStartElement("cbc", "ID", CbcNamespace);
            writer.WriteAttributeString("schemeID", "VAT");
            writer.WriteString(config.SellerVatNumber);
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Postal Address
            writer.WriteStartElement("cac", "PostalAddress", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "StreetName", config.SellerStreet);
            if (!string.IsNullOrEmpty(config.SellerAdditionalStreet))
                WriteElement(writer, "cbc", CbcNamespace, "AdditionalStreetName", config.SellerAdditionalStreet);
            WriteElement(writer, "cbc", CbcNamespace, "BuildingNumber", config.SellerBuildingNumber);
            if (!string.IsNullOrEmpty(config.SellerPlotIdentification))
                WriteElement(writer, "cbc", CbcNamespace, "PlotIdentification", config.SellerPlotIdentification);
            WriteElement(writer, "cbc", CbcNamespace, "CitySubdivisionName", config.SellerCitySubdivision);
            WriteElement(writer, "cbc", CbcNamespace, "CityName", config.SellerCity);
            WriteElement(writer, "cbc", CbcNamespace, "PostalZone", config.SellerPostalCode);
            if (!string.IsNullOrEmpty(config.SellerCountrySubentity))
                WriteElement(writer, "cbc", CbcNamespace, "CountrySubentity", config.SellerCountrySubentity);
            writer.WriteStartElement("cac", "Country", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "IdentificationCode", config.SellerCountryCode);
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Party Tax Scheme
            writer.WriteStartElement("cac", "PartyTaxScheme", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "CompanyID", config.SellerVatNumber);
            writer.WriteStartElement("cac", "TaxScheme", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "ID", "VAT");
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Party Legal Entity
            writer.WriteStartElement("cac", "PartyLegalEntity", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "RegistrationName", config.SellerNameAr);
            writer.WriteEndElement();

            writer.WriteEndElement(); // Party
            writer.WriteEndElement(); // AccountingSupplierParty
        }

        private void WriteCustomerParty(XmlWriter writer, CustomerInfo buyer, ZatcaInvoiceSubType subType)
        {
            writer.WriteStartElement("cac", "AccountingCustomerParty", CacNamespace);
            writer.WriteStartElement("cac", "Party", CacNamespace);

            // For B2B (Standard), include full buyer details
            // For B2C (Simplified), minimal or no buyer details required
            var isSimplified = subType == ZatcaInvoiceSubType.SimplifiedInvoice ||
                              subType == ZatcaInvoiceSubType.SimplifiedCreditNote ||
                              subType == ZatcaInvoiceSubType.SimplifiedDebitNote;

            if (!isSimplified && !string.IsNullOrEmpty(buyer?.VatNumber))
            {
                // Party Identification
                writer.WriteStartElement("cac", "PartyIdentification", CacNamespace);
                writer.WriteStartElement("cbc", "ID", CbcNamespace);
                writer.WriteAttributeString("schemeID", "VAT");
                writer.WriteString(buyer.VatNumber);
                writer.WriteEndElement();
                writer.WriteEndElement();

                // Postal Address
                if (!string.IsNullOrEmpty(buyer.Street))
                {
                    writer.WriteStartElement("cac", "PostalAddress", CacNamespace);
                    WriteElement(writer, "cbc", CbcNamespace, "StreetName", buyer.Street);
                    if (!string.IsNullOrEmpty(buyer.BuildingNumber))
                        WriteElement(writer, "cbc", CbcNamespace, "BuildingNumber", buyer.BuildingNumber);
                    if (!string.IsNullOrEmpty(buyer.District))
                        WriteElement(writer, "cbc", CbcNamespace, "CitySubdivisionName", buyer.District);
                    if (!string.IsNullOrEmpty(buyer.City))
                        WriteElement(writer, "cbc", CbcNamespace, "CityName", buyer.City);
                    if (!string.IsNullOrEmpty(buyer.PostalCode))
                        WriteElement(writer, "cbc", CbcNamespace, "PostalZone", buyer.PostalCode);
                    writer.WriteStartElement("cac", "Country", CacNamespace);
                    WriteElement(writer, "cbc", CbcNamespace, "IdentificationCode", buyer.CountryCode ?? "SA");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                // Party Tax Scheme
                writer.WriteStartElement("cac", "PartyTaxScheme", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "CompanyID", buyer.VatNumber);
                writer.WriteStartElement("cac", "TaxScheme", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "ID", "VAT");
                writer.WriteEndElement();
                writer.WriteEndElement();

                // Party Legal Entity
                writer.WriteStartElement("cac", "PartyLegalEntity", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "RegistrationName", buyer.Name);
                writer.WriteEndElement();
            }
            else
            {
                // Simplified invoice - minimal buyer info
                writer.WriteStartElement("cac", "PartyLegalEntity", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "RegistrationName", buyer?.Name ?? "Cash Customer");
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // Party
            writer.WriteEndElement(); // AccountingCustomerParty
        }

        #endregion

        #region Delivery & Payment

        private void WriteDelivery(XmlWriter writer, DateTime supplyDate, DateTime? supplyEndDate)
        {
            writer.WriteStartElement("cac", "Delivery", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "ActualDeliveryDate", supplyDate.ToString("yyyy-MM-dd"));
            if (supplyEndDate.HasValue)
            {
                WriteElement(writer, "cbc", CbcNamespace, "LatestDeliveryDate", supplyEndDate.Value.ToString("yyyy-MM-dd"));
            }
            writer.WriteEndElement();
        }

        private void WritePaymentMeans(XmlWriter writer, ZatcaPaymentMeansCode paymentMeansCode)
        {
            writer.WriteStartElement("cac", "PaymentMeans", CacNamespace);
            WriteElement(writer, "cbc", CbcNamespace, "PaymentMeansCode", ((int)paymentMeansCode).ToString());
            writer.WriteEndElement();
        }

        #endregion

        #region Tax Elements

        private void WriteTaxTotal(XmlWriter writer, ARInvoice invoice, List<ARInvoiceLine> lines)
        {
            // Group lines by tax rate for subtotals
            var taxGroups = new Dictionary<decimal, (decimal TaxableAmount, decimal TaxAmount, string CategoryCode)>();

            foreach (var line in lines)
            {
                var taxRate = line.TaxRate;
                var categoryCode = GetTaxCategoryCode(line.ZatcaTaxCategory);

                if (!taxGroups.ContainsKey(taxRate))
                {
                    taxGroups[taxRate] = (0, 0, categoryCode);
                }

                var current = taxGroups[taxRate];
                taxGroups[taxRate] = (
                    current.TaxableAmount + line.Amount,
                    current.TaxAmount + line.TaxAmount,
                    categoryCode
                );
            }

            // Write TaxTotal
            writer.WriteStartElement("cac", "TaxTotal", CacNamespace);
            WriteAmountElement(writer, "cbc", CbcNamespace, "TaxAmount", invoice.TaxAmount, invoice.Currency ?? "SAR");

            // Write TaxSubtotal for each tax rate
            foreach (var group in taxGroups)
            {
                writer.WriteStartElement("cac", "TaxSubtotal", CacNamespace);
                WriteAmountElement(writer, "cbc", CbcNamespace, "TaxableAmount", group.Value.TaxableAmount, invoice.Currency ?? "SAR");
                WriteAmountElement(writer, "cbc", CbcNamespace, "TaxAmount", group.Value.TaxAmount, invoice.Currency ?? "SAR");

                writer.WriteStartElement("cac", "TaxCategory", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "ID", group.Value.CategoryCode);
                WriteElement(writer, "cbc", CbcNamespace, "Percent", group.Key.ToString("F2", CultureInfo.InvariantCulture));
                writer.WriteStartElement("cac", "TaxScheme", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "ID", "VAT");
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            // Write TaxTotal in SAR if different currency
            if (invoice.Currency != "SAR")
            {
                writer.WriteStartElement("cac", "TaxTotal", CacNamespace);
                WriteAmountElement(writer, "cbc", CbcNamespace, "TaxAmount", invoice.TaxAmount * invoice.ExchangeRate, "SAR");
                writer.WriteEndElement();
            }
        }

        private string GetTaxCategoryCode(ZatcaTaxCategory? category)
        {
            return category switch
            {
                ZatcaTaxCategory.StandardRate => "S",
                ZatcaTaxCategory.ZeroRated => "Z",
                ZatcaTaxCategory.Exempt => "E",
                ZatcaTaxCategory.OutOfScope => "O",
                _ => "S"
            };
        }

        #endregion

        #region Monetary Total

        private void WriteLegalMonetaryTotal(XmlWriter writer, ARInvoice invoice)
        {
            var currency = invoice.Currency ?? "SAR";

            writer.WriteStartElement("cac", "LegalMonetaryTotal", CacNamespace);
            WriteAmountElement(writer, "cbc", CbcNamespace, "LineExtensionAmount", invoice.Subtotal, currency);
            WriteAmountElement(writer, "cbc", CbcNamespace, "TaxExclusiveAmount", invoice.Subtotal, currency);
            WriteAmountElement(writer, "cbc", CbcNamespace, "TaxInclusiveAmount", invoice.TotalAmount, currency);
            WriteAmountElement(writer, "cbc", CbcNamespace, "AllowanceTotalAmount", 0, currency);
            WriteAmountElement(writer, "cbc", CbcNamespace, "PayableAmount", invoice.TotalAmount, currency);
            writer.WriteEndElement();
        }

        #endregion

        #region Invoice Lines

        private void WriteInvoiceLines(XmlWriter writer, List<ARInvoiceLine> lines, string currency)
        {
            int lineNumber = 1;
            foreach (var line in lines)
            {
                writer.WriteStartElement("cac", "InvoiceLine", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "ID", lineNumber.ToString());
                WriteQuantityElement(writer, line.Quantity);
                WriteAmountElement(writer, "cbc", CbcNamespace, "LineExtensionAmount", line.Amount, currency);

                // Tax Total for line
                writer.WriteStartElement("cac", "TaxTotal", CacNamespace);
                WriteAmountElement(writer, "cbc", CbcNamespace, "TaxAmount", line.TaxAmount, currency);
                WriteElement(writer, "cbc", CbcNamespace, "RoundingAmount", line.TotalAmount.ToString("F2", CultureInfo.InvariantCulture));
                writer.WriteEndElement();

                // Item
                writer.WriteStartElement("cac", "Item", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "Name", line.Description ?? $"Item {lineNumber}");

                // Classified Tax Category
                writer.WriteStartElement("cac", "ClassifiedTaxCategory", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "ID", GetTaxCategoryCode(line.ZatcaTaxCategory));
                WriteElement(writer, "cbc", CbcNamespace, "Percent", line.TaxRate.ToString("F2", CultureInfo.InvariantCulture));

                // Tax exemption reason if applicable
                if (line.ZatcaTaxCategory == ZatcaTaxCategory.ZeroRated ||
                    line.ZatcaTaxCategory == ZatcaTaxCategory.Exempt ||
                    line.ZatcaTaxCategory == ZatcaTaxCategory.OutOfScope)
                {
                    if (!string.IsNullOrEmpty(line.TaxExemptionReasonCode))
                    {
                        WriteElement(writer, "cbc", CbcNamespace, "TaxExemptionReasonCode", line.TaxExemptionReasonCode);
                    }
                    if (!string.IsNullOrEmpty(line.TaxExemptionReason))
                    {
                        WriteElement(writer, "cbc", CbcNamespace, "TaxExemptionReason", line.TaxExemptionReason);
                    }
                }

                writer.WriteStartElement("cac", "TaxScheme", CacNamespace);
                WriteElement(writer, "cbc", CbcNamespace, "ID", "VAT");
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement(); // Item

                // Price
                writer.WriteStartElement("cac", "Price", CacNamespace);
                WriteAmountElement(writer, "cbc", CbcNamespace, "PriceAmount", line.UnitPrice, currency);
                writer.WriteEndElement();

                writer.WriteEndElement(); // InvoiceLine
                lineNumber++;
            }
        }

        #endregion

        #region Helper Methods

        private void WriteElement(XmlWriter writer, string prefix, string ns, string localName, string value)
        {
            writer.WriteStartElement(prefix, localName, ns);
            writer.WriteString(value ?? "");
            writer.WriteEndElement();
        }

        private void WriteAmountElement(XmlWriter writer, string prefix, string ns, string localName, decimal value, string currencyId)
        {
            writer.WriteStartElement(prefix, localName, ns);
            writer.WriteAttributeString("currencyID", currencyId);
            writer.WriteString(value.ToString("F2", CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        private void WriteQuantityElement(XmlWriter writer, decimal quantity)
        {
            writer.WriteStartElement("cbc", "InvoicedQuantity", CbcNamespace);
            writer.WriteAttributeString("unitCode", "PCE");
            writer.WriteString(quantity.ToString("F4", CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        #endregion
    }

    /// <summary>
    /// Customer/Buyer Information DTO for UBL building
    /// </summary>
    public class CustomerInfo
    {
        public string Name { get; set; }
        public string VatNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; } = "SA";
    }
}
