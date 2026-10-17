using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ZR.Service.Finance.Zatca.Utils
{
    /// <summary>
    /// ZATCA QR Code Generator
    /// مولد رمز QR لزاتكا
    /// Generates TLV (Tag-Length-Value) encoded QR codes for ZATCA Phase 2
    /// </summary>
    public class ZatcaQrGenerator
    {
        #region TLV Tags

        /// <summary>
        /// TLV Tag definitions for ZATCA QR Code
        /// </summary>
        public static class Tags
        {
            public const byte SellerName = 1;
            public const byte VatRegistrationNumber = 2;
            public const byte InvoiceTimestamp = 3;
            public const byte InvoiceTotal = 4;
            public const byte VatTotal = 5;
            public const byte InvoiceHash = 6;
            public const byte EcdsaSignature = 7;
            public const byte EcdsaPublicKey = 8;
            public const byte CertificateSignature = 9; // Certificate stamp (for Phase 2)
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generate QR code for Simplified Invoice (B2C) - Phase 2
        /// Includes all 9 tags
        /// </summary>
        public string GenerateSimplifiedInvoiceQr(
            string sellerName,
            string vatNumber,
            DateTime invoiceDateTime,
            decimal totalWithVat,
            decimal vatAmount,
            string invoiceHash,
            byte[] signature,
            byte[] publicKey,
            byte[] certificateSignature)
        {
            var tlvData = new List<byte>();

            // Tag 1: Seller Name (Arabic)
            AddTlv(tlvData, Tags.SellerName, sellerName);

            // Tag 2: VAT Registration Number (15 digits)
            AddTlv(tlvData, Tags.VatRegistrationNumber, vatNumber);

            // Tag 3: Invoice Timestamp (ISO 8601 format)
            AddTlv(tlvData, Tags.InvoiceTimestamp, FormatTimestamp(invoiceDateTime));

            // Tag 4: Invoice Total with VAT
            AddTlv(tlvData, Tags.InvoiceTotal, FormatAmount(totalWithVat));

            // Tag 5: VAT Total
            AddTlv(tlvData, Tags.VatTotal, FormatAmount(vatAmount));

            // Tag 6: Invoice Hash (SHA-256, Base64)
            AddTlv(tlvData, Tags.InvoiceHash, invoiceHash);

            // Tag 7: ECDSA Signature
            if (signature != null && signature.Length > 0)
            {
                AddTlvBytes(tlvData, Tags.EcdsaSignature, signature);
            }

            // Tag 8: ECDSA Public Key
            if (publicKey != null && publicKey.Length > 0)
            {
                AddTlvBytes(tlvData, Tags.EcdsaPublicKey, publicKey);
            }

            // Tag 9: Certificate Signature (Stamp)
            if (certificateSignature != null && certificateSignature.Length > 0)
            {
                AddTlvBytes(tlvData, Tags.CertificateSignature, certificateSignature);
            }

            return Convert.ToBase64String(tlvData.ToArray());
        }

        /// <summary>
        /// Generate QR code for Standard Invoice (B2B) - Phase 2
        /// Standard invoices also require full QR for printed copies
        /// </summary>
        public string GenerateStandardInvoiceQr(
            string sellerName,
            string vatNumber,
            DateTime invoiceDateTime,
            decimal totalWithVat,
            decimal vatAmount,
            string invoiceHash,
            byte[] signature,
            byte[] publicKey,
            byte[] certificateSignature)
        {
            // Standard invoices use the same QR format as simplified
            return GenerateSimplifiedInvoiceQr(
                sellerName,
                vatNumber,
                invoiceDateTime,
                totalWithVat,
                vatAmount,
                invoiceHash,
                signature,
                publicKey,
                certificateSignature);
        }

        /// <summary>
        /// Generate basic QR code (Tags 1-5 only) - for preview/testing
        /// NOT compliant with Phase 2 requirements
        /// </summary>
        public string GenerateBasicQr(
            string sellerName,
            string vatNumber,
            DateTime invoiceDateTime,
            decimal totalWithVat,
            decimal vatAmount)
        {
            var tlvData = new List<byte>();

            AddTlv(tlvData, Tags.SellerName, sellerName);
            AddTlv(tlvData, Tags.VatRegistrationNumber, vatNumber);
            AddTlv(tlvData, Tags.InvoiceTimestamp, FormatTimestamp(invoiceDateTime));
            AddTlv(tlvData, Tags.InvoiceTotal, FormatAmount(totalWithVat));
            AddTlv(tlvData, Tags.VatTotal, FormatAmount(vatAmount));

            return Convert.ToBase64String(tlvData.ToArray());
        }

        /// <summary>
        /// Generate Phase 2 QR code with hash (Tags 1-6) - interim format
        /// </summary>
        public string GenerateQrWithHash(
            string sellerName,
            string vatNumber,
            DateTime invoiceDateTime,
            decimal totalWithVat,
            decimal vatAmount,
            string invoiceHash)
        {
            var tlvData = new List<byte>();

            AddTlv(tlvData, Tags.SellerName, sellerName);
            AddTlv(tlvData, Tags.VatRegistrationNumber, vatNumber);
            AddTlv(tlvData, Tags.InvoiceTimestamp, FormatTimestamp(invoiceDateTime));
            AddTlv(tlvData, Tags.InvoiceTotal, FormatAmount(totalWithVat));
            AddTlv(tlvData, Tags.VatTotal, FormatAmount(vatAmount));
            AddTlv(tlvData, Tags.InvoiceHash, invoiceHash);

            return Convert.ToBase64String(tlvData.ToArray());
        }

        /// <summary>
        /// Parse/decode a ZATCA QR code
        /// </summary>
        public Dictionary<int, string> ParseQrCode(string base64Qr)
        {
            var result = new Dictionary<int, string>();

            try
            {
                var data = Convert.FromBase64String(base64Qr);
                var index = 0;

                while (index < data.Length)
                {
                    if (index + 2 > data.Length) break;

                    var tag = data[index++];
                    var length = data[index++];

                    if (index + length > data.Length) break;

                    var value = new byte[length];
                    Array.Copy(data, index, value, 0, length);
                    index += length;

                    // Tags 1-6 are strings, 7-9 are binary
                    if (tag <= 6)
                    {
                        result[tag] = Encoding.UTF8.GetString(value);
                    }
                    else
                    {
                        result[tag] = Convert.ToBase64String(value);
                    }
                }
            }
            catch
            {
                // Invalid QR format
            }

            return result;
        }

        /// <summary>
        /// Validate QR code has required tags for Phase 2
        /// </summary>
        public (bool IsValid, string[] Errors) ValidatePhase2Qr(string base64Qr)
        {
            var errors = new List<string>();
            var parsed = ParseQrCode(base64Qr);

            // Check required tags
            if (!parsed.ContainsKey(Tags.SellerName))
                errors.Add("Missing Tag 1: Seller Name");

            if (!parsed.ContainsKey(Tags.VatRegistrationNumber))
                errors.Add("Missing Tag 2: VAT Registration Number");

            if (!parsed.ContainsKey(Tags.InvoiceTimestamp))
                errors.Add("Missing Tag 3: Invoice Timestamp");

            if (!parsed.ContainsKey(Tags.InvoiceTotal))
                errors.Add("Missing Tag 4: Invoice Total");

            if (!parsed.ContainsKey(Tags.VatTotal))
                errors.Add("Missing Tag 5: VAT Total");

            if (!parsed.ContainsKey(Tags.InvoiceHash))
                errors.Add("Missing Tag 6: Invoice Hash");

            if (!parsed.ContainsKey(Tags.EcdsaSignature))
                errors.Add("Missing Tag 7: ECDSA Signature");

            if (!parsed.ContainsKey(Tags.EcdsaPublicKey))
                errors.Add("Missing Tag 8: ECDSA Public Key");

            // Tag 9 is optional for some scenarios
            // if (!parsed.ContainsKey(Tags.CertificateSignature))
            //     errors.Add("Missing Tag 9: Certificate Signature");

            return (errors.Count == 0, errors.ToArray());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add string value as TLV
        /// </summary>
        private void AddTlv(List<byte> data, byte tag, string value)
        {
            if (string.IsNullOrEmpty(value)) value = "";

            var valueBytes = Encoding.UTF8.GetBytes(value);
            data.Add(tag);

            // Handle length > 255 bytes (use multiple bytes for length)
            if (valueBytes.Length > 255)
            {
                // For ZATCA, values shouldn't exceed 255 bytes typically
                // If needed, implement extended length encoding
                throw new ArgumentException($"TLV value too long for tag {tag}: {valueBytes.Length} bytes");
            }

            data.Add((byte)valueBytes.Length);
            data.AddRange(valueBytes);
        }

        /// <summary>
        /// Add binary value as TLV
        /// </summary>
        private void AddTlvBytes(List<byte> data, byte tag, byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                data.Add(tag);
                data.Add(0);
                return;
            }

            // Handle length > 255 bytes
            if (value.Length > 255)
            {
                // Extended length encoding for binary data
                // ZATCA signatures can exceed 255 bytes
                data.Add(tag);

                // Use two-byte length encoding
                if (value.Length > 65535)
                    throw new ArgumentException($"TLV value too long for tag {tag}: {value.Length} bytes");

                // For ZATCA Phase 2, we need to handle longer signatures
                // Using standard TLV with multi-byte length
                data.Add((byte)(value.Length >> 8));
                data.Add((byte)(value.Length & 0xFF));
            }
            else
            {
                data.Add(tag);
                data.Add((byte)value.Length);
            }

            data.AddRange(value);
        }

        /// <summary>
        /// Format timestamp for ZATCA (ISO 8601)
        /// </summary>
        private string FormatTimestamp(DateTime dateTime)
        {
            // ZATCA requires: yyyy-MM-ddTHH:mm:ssZ format
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Format amount for ZATCA
        /// </summary>
        private string FormatAmount(decimal amount)
        {
            // ZATCA requires: decimal with 2 decimal places
            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }

        #endregion

        #region Static Helper

        /// <summary>
        /// Quick generate basic QR (for testing)
        /// </summary>
        public static string QuickGenerateBasicQr(
            string sellerName,
            string vatNumber,
            DateTime invoiceDateTime,
            decimal totalWithVat,
            decimal vatAmount)
        {
            var generator = new ZatcaQrGenerator();
            return generator.GenerateBasicQr(sellerName, vatNumber, invoiceDateTime, totalWithVat, vatAmount);
        }

        #endregion
    }
}
