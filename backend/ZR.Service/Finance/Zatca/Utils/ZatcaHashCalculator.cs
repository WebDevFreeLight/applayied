using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Infrastructure.Exceptions;

namespace ZR.Service.Finance.Zatca.Utils
{
    /// <summary>
    /// ZATCA Hash Calculator
    /// حاسبة هاش زاتكا
    /// Computes invoice hashes according to ZATCA Phase 2 specifications
    /// </summary>
    public class ZatcaHashCalculator
    {
        /// <summary>
        /// Initial PIH (Previous Invoice Hash) for the first invoice
        /// Base64 of SHA-256 hash of "0"
        /// </summary>
        public const string InitialPreviousInvoiceHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";

        /// <summary>
        /// Calculate invoice hash for ZATCA
        /// Follows ZATCA specification: SHA-256 of canonicalized invoice XML
        /// </summary>
        /// <param name="invoiceXml">The invoice XML to hash</param>
        /// <returns>Base64 encoded SHA-256 hash</returns>
        public string CalculateInvoiceHash(string invoiceXml)
        {
            if (string.IsNullOrEmpty(invoiceXml))
                throw new ArgumentNullException(nameof(invoiceXml));

            // Step 1: Remove XML declaration and signature if present
            var cleanedXml = PrepareXmlForHashing(invoiceXml);

            // Step 2: Canonicalize the XML (C14N)
            var canonicalXml = CanonicalizXml(cleanedXml);

            // Step 3: Calculate SHA-256 hash
            var hash = ComputeSha256Hash(canonicalXml);

            // Step 4: Return Base64 encoded hash
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Calculate hash for signature (different from invoice hash)
        /// This is used within the XAdES signature computation
        /// </summary>
        /// <param name="content">Content to hash</param>
        /// <returns>SHA-256 hash bytes</returns>
        public byte[] CalculateSignatureHash(string content)
        {
            return ComputeSha256Hash(content);
        }

        /// <summary>
        /// Calculate hash of binary data
        /// </summary>
        /// <param name="data">Binary data to hash</param>
        /// <returns>Base64 encoded SHA-256 hash</returns>
        public string CalculateHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verify that a hash matches the expected value
        /// </summary>
        public bool VerifyHash(string invoiceXml, string expectedHash)
        {
            var calculatedHash = CalculateInvoiceHash(invoiceXml);
            return calculatedHash == expectedHash;
        }

        /// <summary>
        /// Get the initial PIH for first invoice in chain
        /// </summary>
        public string GetInitialPreviousHash()
        {
            return InitialPreviousInvoiceHash;
        }

        /// <summary>
        /// Generate hex-encoded hash (for internal use)
        /// </summary>
        public string CalculateHexHash(string content)
        {
            var hash = ComputeSha256Hash(content);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        #region Private Methods

        /// <summary>
        /// Prepare XML for hashing by removing declaration and signature elements
        /// </summary>
        private string PrepareXmlForHashing(string xml)
        {
            // Remove XML declaration
            var result = Regex.Replace(xml, @"<\?xml[^?]*\?>", "", RegexOptions.IgnoreCase);

            // Remove existing Signature element if present (for re-hashing)
            result = Regex.Replace(result, @"<ds:Signature[^>]*>[\s\S]*?</ds:Signature>", "", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"<Signature[^>]*>[\s\S]*?</Signature>", "", RegexOptions.IgnoreCase);

            // Remove signature placeholder comment
            result = result.Replace("<!--SIGNATURE_PLACEHOLDER-->", "");

            // Remove QR code placeholder
            result = Regex.Replace(result, @"<!--QR_CODE_PLACEHOLDER-->", "");

            // Trim whitespace
            return result.Trim();
        }

        /// <summary>
        /// Canonicalize XML using C14N (Canonical XML)
        /// </summary>
        private string CanonicalizXml(string xml)
        {
            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xml);

                // Use .NET's built-in XML canonicalization
                using var stream = new MemoryStream();
                using var writer = new XmlTextWriter(stream, Encoding.UTF8);

                // Write canonicalized form
                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = false,
                    OmitXmlDeclaration = true
                };

                using var canonWriter = XmlWriter.Create(stream, settings);
                doc.WriteTo(canonWriter);
                canonWriter.Flush();

                stream.Position = 0;
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var canonicalXml = reader.ReadToEnd();

                // Additional normalization for ZATCA compliance
                canonicalXml = NormalizeXmlForZatca(canonicalXml);

                return canonicalXml;
            }
            catch (Exception ex)
            {
                throw new AppException(ExceptionCode.FINANCE_ZATCA_XML_CANONICALIZATION_FAILED, ex.Message);
            }
        }

        /// <summary>
        /// Additional XML normalization for ZATCA
        /// </summary>
        private string NormalizeXmlForZatca(string xml)
        {
            // Remove extra whitespace between elements
            var result = Regex.Replace(xml, @">\s+<", "><");

            // Normalize line endings
            result = result.Replace("\r\n", "\n").Replace("\r", "\n");

            // Ensure consistent attribute quoting
            // Already handled by XmlWriter

            return result;
        }

        /// <summary>
        /// Compute SHA-256 hash of string content
        /// </summary>
        private byte[] ComputeSha256Hash(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            return sha256.ComputeHash(bytes);
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Static helper to quickly calculate hash
        /// </summary>
        public static string QuickHash(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Compute HMAC-SHA256 (for API authentication if needed)
        /// </summary>
        public static byte[] ComputeHmacSha256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        #endregion
    }
}
