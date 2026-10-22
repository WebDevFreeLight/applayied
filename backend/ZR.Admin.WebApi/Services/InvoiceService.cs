using Infrastructure.Extensions;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Zatca.EInvoice.SDK;
using Zatca.EInvoice.SDK.Contracts.Models;
using ZatcaIntegrationApp.Helpers;
using ZatcaIntegrationApp.Models;
using Org.BouncyCastle.Asn1.X509;
using System.IO;
using System.Runtime.ConstrainedExecution;

namespace ZatcaIntegrationApp.Services
{

    [AppService(ServiceType = typeof(InvoiceService))]

    public class InvoiceService
    {
        // File path constants
        private const string CertificatePath = "Data/Certificates/certificate.csr";
        private const string PrivateKeyPath = "Data/Certificates/private.key";
        private const string PihPath = "Data/Certificates/pih.txt";
        private const string InputPath = "Data/Invoice.xml";
        private const string SignedOutputPath = "Data/signed_invoice.xml";
        
        private readonly EInvoiceValidator _validator;
        private readonly EInvoiceSigner _signer;
        private readonly RequestGenerator _requestGenerator;
        public CsrGenerationDto _CSRDTO;
        private readonly EInvoiceHashGenerator _IHG;
        private readonly EInvoiceQRGenerator _QRdata;
        Complianceinvoice CI = new Complianceinvoice();
        public InvoiceService()
        {
            // Set console output encoding
            Console.OutputEncoding = Encoding.UTF8;
            
            _validator = new EInvoiceValidator();
            _signer = new EInvoiceSigner();
            _requestGenerator = new RequestGenerator();
            _IHG = new EInvoiceHashGenerator();
            _QRdata = new EInvoiceQRGenerator();
        }


        public CsrResult GenerateCSR()
        {
            Console.WriteLine("===== Please Press Any Key To Generate Data Company Certificate . ====\n");
            var generator = new CsrGenerator();

            _CSRDTO = new CsrGenerationDto(
                commonName: "TST-886431145-399999999900003",
                serialNumber: "1-Boomsandwitch|2-Easybos|3-" + Guid.NewGuid().ToString(),
                organizationIdentifier: "399999999900003",
                organizationUnitName: "3999999999",
                organizationName: "Boomsandwitch",
                countryName: "SA",
                invoiceType: "1100",
                locationAddress: "Heddah",
                industryBusinessCategory: "Foodes"
            );

            CsrResult result = generator.GenerateCsr(_CSRDTO, EnvironmentType.NonProduction, pemFormat: true);
           
            if (!result.IsValid)
            {
                Console.WriteLine("? Fail Generate Data Company Certificate");
                foreach (var error in result.ErrorMessages)
                    Console.WriteLine("- " + error);
            }
            string csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(result.Csr));
            string keyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(result.PrivateKey));

            var certificateDir = "Data/Certificates";
            if (!Directory.Exists(certificateDir))
            {
                Directory.CreateDirectory(certificateDir);
                Logger.LogInfo($"? Created directory: {certificateDir}");
            }


            File.WriteAllText("Data/Certificates/certificate.csr", csrBase64);
            File.WriteAllText("Data/Certificates/private.key", keyBase64);
            Logger.LogSuccess("? Done Generate Data Company Certificate And PrivateKey \n");
            Console.WriteLine("===== Please Press Any Key To Request Company data registration. ====\n");


            return result;

        }
        public XmlDocument LoadInvoiceFromString(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            return doc;
        }
        public async Task Sign(XmlDocument doc)
        {
            Console.WriteLine("===== Please Press Any Key To Sign UBL XML . ====\n");
            // Json File Read
            string Certjson = File.ReadAllText("Data/Certificates/CSR.Json");
            var certData = Certjson.ToObject<CSR>();
            // ?????
            string base64Cert = certData.binarySecurityToken;
            byte[] certBytes = Convert.FromBase64String(base64Cert);
            string certPem = Encoding.UTF8.GetString(certBytes);

            // ????? ???
            string base64Key = File.ReadAllText("Data/Certificates/private.key");
            byte[] keyBytes = Convert.FromBase64String(base64Key);
            string keyPem = Encoding.UTF8.GetString(keyBytes);

            // ????? ??????? ???????? ???????
            var certdata = new X509Certificate2(Encoding.UTF8.GetBytes(certPem));
            var issuer = certdata.Issuer;
            var serial = certdata.SerialNumber;
            Console.WriteLine(certdata + "\n" + issuer + "\n" + serial);
            var result = _signer.SignDocument(doc, certPem, keyPem);
            if (!result.IsValid)
            {
                Console.WriteLine("? Signing Failed");
                foreach (var error in result.Steps)
                    Console.WriteLine("- " + error.IsValid.ToString() + ":" + error.StepName + " : " + error.ErrorMessages);
            }
            else
            {
                Console.WriteLine("? Signed invoice is done scuccess ... ");
                foreach (var error in result.Steps)
                {
                    Console.WriteLine($"- {error.IsValid} : {error.StepName}");

                    if (error.ErrorMessages != null && error.ErrorMessages.Any())
                    {
                        foreach (var msg in error.ErrorMessages)
                        {
                            Console.WriteLine($"    ?? {msg}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    ? No error messages.");
                    }
                }
                //foreach (var error in result.Steps)
                //    Console.WriteLine("- " + error.IsValid.ToString() + ":" + error.StepName + " : " + error.ErrorMessages);
                //Console.ReadKey();
                File.WriteAllText("Data/Signedinvoice.xml", result.SignedEInvoice.OuterXml);
                Logger.LogSuccess("? Signed invoice saved to: " + "Data/Signedinvoice.xml");
                Console.WriteLine("===== Please Press Any Key To Send Zatca Invoice . ====\n");
                var xmlContent = File.ReadAllText("Data/Signedinvoice.xml");
                var invoiceDoc = LoadInvoiceFromString(xmlContent);
                await GenerateRequest(invoiceDoc);
            }
        }
        public async Task SendInvoiceAsync(string invoiceHash, string uuid, string base64Invoice)
        {
            using var client = new HttpClient();
            var payload = new
            {
                invoiceHash = invoiceHash,
                uuid = uuid,
                invoice = base64Invoice
            };
            // Json File Read
            string Certjson = File.ReadAllText("Data/Certificates/CSR.Json");
            var certData = Certjson.ToObject<CSR>( );

            // ????? ??????
            client.DefaultRequestHeaders.Add("accept", "application/json");
            client.DefaultRequestHeaders.Add("accept-language", "ar");
            client.DefaultRequestHeaders.Add("Clearance-Status", "0");
            client.DefaultRequestHeaders.Add("Accept-Version", "V2");
            string rawCredentials = $"{certData.binarySecurityToken}:{certData.secret}";
            string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCredentials));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
            var json =   payload.ToJson();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/invoices/reporting/single";

            var response = await client.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();
            var zatcaResponse = responseText.ToObject<ZATCAResponse>();
            Logger.LogSuccess(response.ReasonPhrase);
            if (zatcaResponse.reportingStatus == "REPORTED")
            {
                Logger.LogSuccess(zatcaResponse.reportingStatus);
            }
            else
            {
                Logger.LogError(zatcaResponse.reportingStatus);
            }
            Console.WriteLine("=================== The End =====================\n");
        }


        public async Task GenerateRequest(XmlDocument signedDoc)
        {
            // 1. ????? ????? ????? ?? SDK
            var result = _requestGenerator.GenerateRequest(signedDoc);

            // 2. ????? ??? NamespaceManager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(signedDoc.NameTable);
            nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            // 3. ????? ?? ????? ???? PIH ??? ICV ? QR
            var pihNode = signedDoc.SelectSingleNode("//cbc:EmbeddedDocumentBinaryObject[../cbc:ID='PIH']", nsmgr);

            if (pihNode != null)
            {
                pihNode.InnerText = result.InvoiceRequest.InvoiceHash;
                Logger.LogInfo("?? ?? ????? ???? PIH ???? ?????? ???????");
            }
            else
            {
                
                
                
                XmlElement pihRef = signedDoc.CreateElement("cac", "AdditionalDocumentReference", nsmgr.LookupNamespace("cac"));
                XmlElement pihId = signedDoc.CreateElement("cbc", "ID", nsmgr.LookupNamespace("cbc"));
                pihId.InnerText = "PIH";

                XmlElement attachment = signedDoc.CreateElement("cac", "Attachment", nsmgr.LookupNamespace("cac"));
                XmlElement embeddedObj = signedDoc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", nsmgr.LookupNamespace("cbc"));
                embeddedObj.SetAttribute("mimeCode", "text/plain");
                embeddedObj.InnerText = result.InvoiceRequest.InvoiceHash;

                attachment.AppendChild(embeddedObj);
                pihRef.AppendChild(pihId);
                pihRef.AppendChild(attachment);

                // ????? ??? ???? ICV
                var icvNode = signedDoc.SelectSingleNode("//cac:AdditionalDocumentReference[cbc:ID='ICV']", nsmgr);
                signedDoc.DocumentElement.InsertAfter(pihRef, icvNode);
            }

            // 4. ??? ?????? ???????
            string path = "Data/Signedinvoice.xml";
            signedDoc.Save(path);
            //Logger.LogInfo("? The amended invoice is saved in : " + path);
            string Oldpath = @"Data/Invoice.xml";
            if (File.Exists(Oldpath))
            {
                File.Delete(Oldpath);
            }

            var xmlContent = File.ReadAllText("Data/Signedinvoice.xml");
            var invoiceDoc = LoadInvoiceFromString(xmlContent);

            // Json File Read
            string Certjson = File.ReadAllText("Data/Certificates/CSR.Json");
            var certData = Certjson.ToObject<CSR>();

            // ???? 1: ???? ?????
            var validation = _validator.ValidateEInvoice(invoiceDoc, certData.binarySecurityToken, result.InvoiceRequest.InvoiceHash);
            if (!validation.IsValid)
            {
                Console.WriteLine("? Fail The Invoice Is not compliant ... ");
                foreach (var error in validation.ValidationSteps)
                {
                    Console.WriteLine($"- {error.IsValid} : {error.ValidationStepName}");

                    if (error.ErrorMessages != null && error.ErrorMessages.Any())
                    {
                        foreach (var msg in error.ErrorMessages)
                        {
                            Console.WriteLine($"    ? {msg}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    ? No error messages.");
                    }
                }

                foreach (var error in validation.ValidationSteps)
                    Console.WriteLine("- " + error.IsValid.ToString() + ":" + error.ValidationStepName + " : " + error.ErrorMessages);
                Logger.LogWarning("? Non-incriminating error ... ");

                Logger.LogInfo("Invoice Hash (PIH): " + result.InvoiceRequest.InvoiceHash);
                Logger.LogInfo("Invoice UUID: " + result.InvoiceRequest.Uuid);
                Logger.LogInfo("Invoice : " + result.InvoiceRequest.Invoice);


                //await SendInvoiceAsync(result.InvoiceRequest.InvoiceHash,
                //                 result.InvoiceRequest.Uuid,
                //                 result.InvoiceRequest.Invoice);
            }

            await SendInvoiceAsync(result.InvoiceRequest.InvoiceHash,
                          result.InvoiceRequest.Uuid,
                          result.InvoiceRequest.Invoice);
        }
        public void CreateSampleInvoiceFromCsrData()
        {
            Console.WriteLine("===== Enter Invoice Data (Press Enter for default values) =====");

            // Get invoice header data
            Console.Write("Enter Invoice ID (default: INV-001): ");
            string invoiceId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(invoiceId)) invoiceId = "INV-001";

            Console.Write("Enter Invoice Type Code (default: 388): ");
            string invoiceTypeCode = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(invoiceTypeCode)) invoiceTypeCode = "388";

            Console.Write("Enter Currency Code (default: SAR): ");
            string currencyCode = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(currencyCode)) currencyCode = "SAR";

            // Get supplier data
            Console.WriteLine("\n--- Supplier Information ---");
            Console.Write("Enter Supplier CRN (default: 1010010000): ");
            string supplierCrn = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(supplierCrn)) supplierCrn = "1010010000";

            Console.Write("Enter Supplier Street Name (default: ?????? ?????): ");
            string supplierStreet = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(supplierStreet)) supplierStreet = "?????? ?????";

            Console.Write("Enter Supplier Building Number (default: 2322): ");
            string supplierBuilding = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(supplierBuilding)) supplierBuilding = "2322";

            Console.Write("Enter Supplier City Subdivision (default: ??????): ");
            string supplierSubdivision = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(supplierSubdivision)) supplierSubdivision = "??????";

            Console.Write("Enter Supplier City (default: ??????): ");
            string supplierCity = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(supplierCity)) supplierCity = "??????";

            Console.Write("Enter Supplier Postal Code (default: 23333): ");
            string supplierPostal = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(supplierPostal)) supplierPostal = "23333";

            // Get customer data
            Console.WriteLine("\n--- Customer Information ---");
            Console.Write("Enter Customer CRN (default: 1010010000): ");
            string customerCrn = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerCrn)) customerCrn = "1010010000";

            Console.Write("Enter Customer Street Name (default: ?????? ?????): ");
            string customerStreet = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerStreet)) customerStreet = "?????? ?????";

            Console.Write("Enter Customer Building Number (default: 2322): ");
            string customerBuilding = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerBuilding)) customerBuilding = "2322";

            Console.Write("Enter Customer City Subdivision (default: ??????): ");
            string customerSubdivision = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerSubdivision)) customerSubdivision = "??????";

            Console.Write("Enter Customer City (default: ??????): ");
            string customerCity = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerCity)) customerCity = "??????";

            Console.Write("Enter Customer Postal Code (default: 23333): ");
            string customerPostal = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerPostal)) customerPostal = "23333";

            Console.Write("Enter Customer VAT ID (default: 399999999800003): ");
            string customerVatId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerVatId)) customerVatId = "399999999800003";

            // Get products data
            Console.WriteLine("\n--- Product Information ---");
            Console.Write("Enter number of products (default: 5 for sample products): ");
            string productCountStr = Console.ReadLine();
            int productCount = 5;
            if (!string.IsNullOrWhiteSpace(productCountStr) && int.TryParse(productCountStr, out int parsedCount))
            {
                productCount = parsedCount;
            }

            var products = new List<ProductLine>();

            if (productCount == 5 && string.IsNullOrWhiteSpace(productCountStr))
            {
                // Use default products
                products = new List<ProductLine>
                {
                    new ProductLine { Id = "1", Name = "??? ????", Quantity = 2, UnitPrice = 2 },
                    new ProductLine { Id = "2", Name = "????", Quantity = 1, UnitPrice = 5 },
                    new ProductLine { Id = "3", Name = "?????", Quantity = 3, UnitPrice = 1.5m },
                    new ProductLine { Id = "4", Name = "??? ?????", Quantity = 1, UnitPrice = 30 },
                    new ProductLine { Id = "5", Name = "?????", Quantity = 2, UnitPrice = 3 }
                };
                Console.WriteLine("Using default sample products.");
            }
            else
            {
                // Get custom products
                for (int i = 1; i <= productCount; i++)
                {
                    Console.WriteLine($"\n--- Product {i} ---");

                    Console.Write($"Enter Product {i} Name: ");
                    string productName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(productName)) productName = $"Product {i}";

                    Console.Write($"Enter Product {i} Quantity (default: 1): ");
                    string quantityStr = Console.ReadLine();
                    int quantity = 1;
                    if (!string.IsNullOrWhiteSpace(quantityStr) && int.TryParse(quantityStr, out int parsedQty))
                    {
                        quantity = parsedQty;
                    }

                    Console.Write($"Enter Product {i} Unit Price (default: 10): ");
                    string priceStr = Console.ReadLine();
                    decimal unitPrice = 10m;
                    if (!string.IsNullOrWhiteSpace(priceStr) && decimal.TryParse(priceStr, out decimal parsedPrice))
                    {
                        unitPrice = parsedPrice;
                    }

                    products.Add(new ProductLine
                    {
                        Id = i.ToString(),
                        Name = productName,
                        Quantity = quantity,
                        UnitPrice = unitPrice
                    });
                }
            }
            decimal totalExtension = 0;
            decimal totalTax = 0;
            decimal totalDiscount = 0;
            decimal total = 0;
            decimal totalPrice = 0;
            decimal totalAmount = 0;

            foreach (var p in products)
            {
                decimal netPrice = p.UnitPrice - p.Discount;
                decimal lineTotal = netPrice * p.Quantity;
                totalPrice += lineTotal;
                decimal taxAmount = Math.Round(lineTotal * p.TaxPercent / 100, 2);

                totalExtension += lineTotal;
                totalTax += taxAmount;
                totalDiscount += p.Discount * p.Quantity;
                total = totalTax + lineTotal;
                Console.WriteLine($"Product: {p.Name}, Quantity: {p.Quantity}, Unit Price: {p.UnitPrice}, Line Total: {lineTotal}, Tax Amount: {taxAmount}, Total Amount: {taxAmount + lineTotal}");
            }
            Console.WriteLine($"Price Amount : {totalPrice}");
            Console.WriteLine($"Tax Amount : {totalTax}");
            totalAmount = totalPrice + totalTax;
            Console.WriteLine($"Total : {totalPrice + totalTax}");
            if (_CSRDTO == null)
            {
                throw new InvalidOperationException("? CSR Data is missing. ???? ?? ??????? GenerateCSR() ?????.");
            }
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false) // ?? UTF-8 ???? BOM
            };

            using (var fs = new FileStream("Data/Invoice.xml", FileMode.Create, FileAccess.Write))
            using (var writer = XmlWriter.Create(fs, settings))
            {
                writer.WriteStartDocument();

                // Root element
                writer.WriteStartElement("Invoice", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");

                // Namespaces
                writer.WriteAttributeString("xmlns", "cac", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                writer.WriteAttributeString("xmlns", "cbc", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                writer.WriteAttributeString("xmlns", "ext", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

                // UBLExtensions - ???? ???????? ??????? ??????
                writer.WriteStartElement("ext", "UBLExtensions", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
                writer.WriteStartElement("ext", "UBLExtension", null);
                writer.WriteStartElement("ext", "ExtensionContent", null);
                // ??????? ?????? ??? ?????? ?? ???? SDK
                writer.WriteEndElement(); // ExtensionContent
                writer.WriteEndElement(); // UBLExtension
                writer.WriteEndElement(); // UBLExtensions

                // Header
                writer.WriteElementString("cbc", "ProfileID", null, "reporting:1.0");
                writer.WriteElementString("cbc", "ID", null, invoiceId);
                writer.WriteElementString("cbc", "UUID", null, Guid.NewGuid().ToString());
                writer.WriteElementString("cbc", "IssueDate", null, DateTime.Now.ToString("yyyy-MM-dd"));
                writer.WriteElementString("cbc", "IssueTime", null, DateTime.Now.ToString("HH:mm:ss"));

                // Invoice Type Code
                writer.WriteStartElement("cbc", "InvoiceTypeCode", null);
                writer.WriteAttributeString("name", "0100000");
                writer.WriteString(invoiceTypeCode);
                writer.WriteEndElement();

                writer.WriteElementString("cbc", "DocumentCurrencyCode", null, currencyCode);
                writer.WriteElementString("cbc", "TaxCurrencyCode", null, currencyCode);

                // ICV
                writer.WriteStartElement("cac", "AdditionalDocumentReference", null);

                writer.WriteStartElement("cbc", "ID", null);
                writer.WriteString("ICV");
                writer.WriteEndElement(); // ?? ????? ?????? ID

                writer.WriteStartElement("cbc", "UUID", null);
                writer.WriteString("10");
                writer.WriteEndElement(); // ?? ????? ?????? UUID

                writer.WriteEndElement(); // ?? ????? ?????? AdditionalDocumentReference

                // Supplier - AccountingSupplierParty
                writer.WriteStartElement("cac", "AccountingSupplierParty", null);
                writer.WriteStartElement("cac", "Party", null);

                // PartyIdentification
                writer.WriteStartElement("cac", "PartyIdentification", null);
                writer.WriteStartElement("cbc", "ID", null);
                writer.WriteAttributeString("schemeID", "CRN");
                writer.WriteString(supplierCrn);
                writer.WriteEndElement(); // cbc:ID
                writer.WriteEndElement(); // PartyIdentification

                // PostalAddress
                writer.WriteStartElement("cac", "PostalAddress", null);
                writer.WriteElementString("cbc", "StreetName", null, supplierStreet);
                writer.WriteElementString("cbc", "BuildingNumber", null, supplierBuilding);
                writer.WriteElementString("cbc", "CitySubdivisionName", null, supplierSubdivision);
                writer.WriteElementString("cbc", "CityName", null, supplierCity);
                writer.WriteElementString("cbc", "PostalZone", null, supplierPostal);
                writer.WriteStartElement("cac", "Country", null);
                writer.WriteElementString("cbc", "IdentificationCode", null, "SA");
                writer.WriteEndElement(); // Country
                writer.WriteEndElement(); // PostalAddress

                // PartyTaxScheme
                writer.WriteStartElement("cac", "PartyTaxScheme", null);
                writer.WriteElementString("cbc", "CompanyID", null, _CSRDTO.OrganizationIdentifier);
                writer.WriteStartElement("cac", "TaxScheme", null);
                writer.WriteElementString("cbc", "ID", null, "VAT");
                writer.WriteEndElement(); // TaxScheme
                writer.WriteEndElement(); // PartyTaxScheme

                // PartyLegalEntity
                writer.WriteStartElement("cac", "PartyLegalEntity", null);
                writer.WriteElementString("cbc", "RegistrationName", null, _CSRDTO.OrganizationName);
                writer.WriteEndElement(); // PartyLegalEntity

                writer.WriteEndElement(); // Party
                writer.WriteEndElement(); // AccountingSupplierParty

                // Customer - AccountingCustomerParty

                writer.WriteStartElement("cac", "AccountingCustomerParty", null);
                writer.WriteStartElement("cac", "Party", null);

                // PartyIdentification
                writer.WriteStartElement("cac", "PartyIdentification", null);
                writer.WriteStartElement("cbc", "ID", null);
                writer.WriteAttributeString("schemeID", "CRN");
                writer.WriteString(customerCrn);
                writer.WriteEndElement(); // cbc:ID
                writer.WriteEndElement(); // PartyIdentification

                // PostalAddress
                writer.WriteStartElement("cac", "PostalAddress", null);
                writer.WriteElementString("cbc", "StreetName", null, customerStreet);
                writer.WriteElementString("cbc", "BuildingNumber", null, customerBuilding);
                writer.WriteElementString("cbc", "CitySubdivisionName", null, customerSubdivision);
                writer.WriteElementString("cbc", "CityName", null, customerCity);
                writer.WriteElementString("cbc", "PostalZone", null, customerPostal);
                writer.WriteStartElement("cac", "Country", null);
                writer.WriteElementString("cbc", "IdentificationCode", null, "SA");
                writer.WriteEndElement(); // Country
                writer.WriteEndElement(); // PostalAddress

                // PartyTaxScheme
                writer.WriteStartElement("cac", "PartyTaxScheme", null);
                writer.WriteElementString("cbc", "CompanyID", null, customerVatId);
                writer.WriteStartElement("cac", "TaxScheme", null);
                writer.WriteElementString("cbc", "ID", null, "VAT");
                writer.WriteEndElement(); // TaxScheme
                writer.WriteEndElement(); // PartyTaxScheme

                // PartyLegalEntity
                writer.WriteStartElement("cac", "PartyLegalEntity", null);
                writer.WriteElementString("cbc", "RegistrationName", null, _CSRDTO.OrganizationName);
                writer.WriteEndElement(); // PartyLegalEntity

                writer.WriteEndElement(); // Party
                writer.WriteEndElement(); // AccountingCustomerParty

                // Delivery
                writer.WriteStartElement("cac", "Delivery", null);
                writer.WriteElementString("cbc", "ActualDeliveryDate", null, DateTime.Now.ToString("yyyy-MM-dd"));
                writer.WriteEndElement();

                //PaymentMeans
                writer.WriteStartElement("cac", "PaymentMeans", null);
                writer.WriteElementString("cbc", "PaymentMeansCode", null, "10");
                writer.WriteEndElement();

                writer.WriteStartElement("cac", "AllowanceCharge", null);

                writer.WriteElementString("cbc", "ChargeIndicator", null, "false");
                writer.WriteElementString("cbc", "AllowanceChargeReason", null, "discount");

                // Amount with currency attribute
                writer.WriteStartElement("cbc", "Amount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString("0.00");
                writer.WriteEndElement(); // Amount

                // TaxCategory
                writer.WriteStartElement("cac", "TaxCategory", null);

                // ID with attributes
                writer.WriteStartElement("cbc", "ID", null);
                writer.WriteAttributeString("schemeID", "UN/ECE 5305");
                writer.WriteAttributeString("schemeAgencyID", "6");
                writer.WriteString("S");
                writer.WriteEndElement(); // ID

                writer.WriteElementString("cbc", "Percent", null, "15");

                // TaxScheme
                writer.WriteStartElement("cac", "TaxScheme", null);
                writer.WriteStartElement("cbc", "ID", null);
                writer.WriteAttributeString("schemeID", "UN/ECE 5153");
                writer.WriteAttributeString("schemeAgencyID", "6");
                writer.WriteString("VAT");
                writer.WriteEndElement(); // ID inside TaxScheme
                writer.WriteEndElement(); // TaxScheme

                writer.WriteEndElement(); // TaxCategory
                writer.WriteEndElement(); // AllowanceCharge


                // Tax Total
                writer.WriteStartElement("cac", "TaxTotal", null);
                writer.WriteStartElement("cbc", "TaxAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalTax.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("cac", "TaxTotal", null);
                writer.WriteStartElement("cbc", "TaxAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalTax.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("cac", "TaxSubtotal", null);
                writer.WriteStartElement("cbc", "TaxableAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalPrice.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("cbc", "TaxAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalTax.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("cac", "TaxCategory", null);
                writer.WriteStartElement("cbc", "ID", null);
                writer.WriteAttributeString("schemeID", "UN/ECE 5305");
                writer.WriteAttributeString("schemeAgencyID", "6");
                writer.WriteString("S");
                writer.WriteEndElement();

                writer.WriteElementString("cbc", "Percent", null, "15.00");
                writer.WriteStartElement("cac", "TaxScheme", null);
                writer.WriteStartElement("cbc", "ID", null);
                writer.WriteAttributeString("schemeID", "UN/ECE 5153");
                writer.WriteAttributeString("schemeAgencyID", "6");
                writer.WriteString("VAT");
                writer.WriteEndElement();

                writer.WriteEndElement(); // TaxScheme
                writer.WriteEndElement(); // TaxCategory
                writer.WriteEndElement(); // TaxSubtotal
                writer.WriteEndElement(); // TaxTotal

                // Legal Monetary Total
                writer.WriteStartElement("cac", "LegalMonetaryTotal", null);

                writer.WriteStartElement("cbc", "LineExtensionAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalPrice.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("cbc", "TaxExclusiveAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalPrice.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("cbc", "TaxInclusiveAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalAmount.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("cbc", "AllowanceTotalAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString("0.00");
                writer.WriteEndElement();

                writer.WriteStartElement("cbc", "PrepaidAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString("0.00");
                writer.WriteEndElement();

                writer.WriteStartElement("cbc", "PayableAmount", null);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(totalAmount.ToString());
                writer.WriteEndElement();

                writer.WriteEndElement(); // LegalMonetaryTotal
                foreach (var p in products)
                {
                    decimal netPrice = p.UnitPrice - p.Discount;
                    decimal lineTotal = netPrice * p.Quantity;
                    decimal taxAmount = Math.Round(lineTotal * p.TaxPercent / 100, 2);
                    // Invoice Line
                    writer.WriteStartElement("cac", "InvoiceLine", null);

                    writer.WriteElementString("cbc", "ID", null, p.Id);

                    writer.WriteStartElement("cbc", "InvoicedQuantity", null);
                    writer.WriteAttributeString("unitCode", "PCE");
                    writer.WriteString(p.Quantity.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("cbc", "LineExtensionAmount", null);
                    writer.WriteAttributeString("currencyID", currencyCode);
                    writer.WriteString(p.UnitPrice.ToString());
                    writer.WriteEndElement();

                    // TaxTotal
                    writer.WriteStartElement("cac", "TaxTotal", null);
                    writer.WriteStartElement("cbc", "TaxAmount", null);
                    writer.WriteAttributeString("currencyID", currencyCode);
                    writer.WriteString(taxAmount.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("cbc", "RoundingAmount", null);
                    writer.WriteAttributeString("currencyID", currencyCode);
                    writer.WriteString((taxAmount + lineTotal).ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement(); // TaxTotal

                    // Item
                    writer.WriteStartElement("cac", "Item", null);
                    writer.WriteElementString("cbc", "Name", null, p.Name);

                    // ClassifiedTaxCategory
                    writer.WriteStartElement("cac", "ClassifiedTaxCategory", null);
                    writer.WriteElementString("cbc", "ID", null, "S");
                    writer.WriteElementString("cbc", "Percent", null, "15.00");

                    // TaxScheme
                    writer.WriteStartElement("cac", "TaxScheme", null);
                    writer.WriteElementString("cbc", "ID", null, "VAT");
                    writer.WriteEndElement(); // TaxScheme

                    writer.WriteEndElement(); // ClassifiedTaxCategory
                    writer.WriteEndElement(); // Item

                    // Price
                    writer.WriteStartElement("cac", "Price", null);
                    writer.WriteStartElement("cbc", "PriceAmount", null);
                    writer.WriteAttributeString("currencyID", currencyCode);
                    writer.WriteString(p.UnitPrice.ToString());
                    writer.WriteEndElement();

                    // AllowanceCharge inside Price
                    writer.WriteStartElement("cac", "AllowanceCharge", null);
                    writer.WriteElementString("cbc", "ChargeIndicator", null, "false");
                    writer.WriteElementString("cbc", "AllowanceChargeReason", null, "discount");

                    writer.WriteStartElement("cbc", "Amount", null);
                    writer.WriteAttributeString("currencyID", currencyCode);
                    writer.WriteString("0.00");
                    writer.WriteEndElement(); // Amount

                    writer.WriteEndElement(); // AllowanceCharge
                    writer.WriteEndElement(); // Price

                    writer.WriteEndElement(); // InvoiceLine
                }
                writer.WriteEndElement(); // Invoice
                writer.WriteEndDocument();

            }
            Logger.LogSuccess("? The invoice has been created .\n");
            Logger.LogSuccess("? Invoice XML saved to: " + "Data/Invoice.xml\n");
            //return sb.ToString();
        }
        public async Task UploadCsrAsync()
        {
            var data = File.ReadAllText("Data/Certificates/certificate.csr");
            var payload = new { csr = data };
            var json = payload.ToJson();

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept-Version", "V2");
            http.DefaultRequestHeaders.Add("OTP", "123345");
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var res = await http.PostAsync(
                "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine("? Fail Upload Data Company \n" + body);
                return;
            }

            Logger.LogSuccess("? Done Upload Data Company\n");
            //Console.WriteLine(body +"\n");
            var obj = body.ToJson( );
            File.WriteAllText("Data/Certificates/CSR.Json", body);
            Console.WriteLine("===== Please Press Any Key To Request Token. ====\n");
        }
        public async Task Getcertificate()
        {
            CSR csrt = new CSR();
            var data = File.ReadAllText("Data/Certificates/CSR.Json");
            var csrtdata = data.ToObject<CSR>();
            var payload = new { compliance_request_id = csrtdata.requestID };
            var json = payload.ToJson();
            string credentials = $"{csrtdata.binarySecurityToken}:{csrtdata.secret}";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept-Version", "V2");
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Token);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var res = await http.PostAsync(
                "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/production/csids",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                Logger.LogError("? Fail Generated Certification \n" + body);
                return;
            }
            Logger.LogSuccess("? Done Generated Token\n");
            var csrtbody = body.ToObject<CSR>();
            Console.WriteLine("===== Please Press Any Key To Save Token . ====\n");
            File.WriteAllText("Data/Certificates/CSR.Json", body);
            Logger.LogSuccess("? Done Save Token\n");
            Console.WriteLine("===== Make Data For Test . ====\n");
        }
    }
}

