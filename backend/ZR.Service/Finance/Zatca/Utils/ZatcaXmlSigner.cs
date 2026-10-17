using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Infrastructure.Exceptions;

namespace ZR.Service.Finance.Zatca.Utils
{
    /// <summary>
    /// ZATCA XML Signer
    /// موقّع XML لزاتكا
    /// Signs invoice XML using ECDSA with XAdES-BES format
    /// </summary>
    public class ZatcaXmlSigner
    {
        private readonly ZatcaHashCalculator _hashCalculator;

        public ZatcaXmlSigner()
        {
            _hashCalculator = new ZatcaHashCalculator();
        }

        #region Public Methods

        /// <summary>
        /// Sign invoice XML with device certificate and private key
        /// </summary>
        /// <param name="invoiceXml">Unsigned invoice XML</param>
        /// <param name="certificateBase64">Base64 encoded X.509 certificate</param>
        /// <param name="privateKeyBase64">Base64 encoded private key (PKCS#8 or EC)</param>
        /// <returns>Signed invoice XML with embedded XAdES signature</returns>
        public SigningResult SignInvoice(string invoiceXml, string certificateBase64, string privateKeyBase64)
        {
            var result = new SigningResult();

            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(invoiceXml))
                    throw new ArgumentNullException(nameof(invoiceXml));
                if (string.IsNullOrEmpty(certificateBase64))
                    throw new ArgumentNullException(nameof(certificateBase64));
                if (string.IsNullOrEmpty(privateKeyBase64))
                    throw new ArgumentNullException(nameof(privateKeyBase64));

                // Load certificate
                var certificate = LoadCertificate(certificateBase64);

                // Load private key
                var privateKey = LoadPrivateKey(privateKeyBase64);

                // Calculate invoice hash
                var invoiceHash = _hashCalculator.CalculateInvoiceHash(invoiceXml);
                result.InvoiceHash = invoiceHash;

                // Build signed properties
                var signedProperties = BuildSignedProperties(certificate, invoiceXml);

                // Calculate signed properties hash
                var signedPropertiesHash = _hashCalculator.CalculateInvoiceHash(signedProperties);

                // Build SignedInfo for signing
                var signedInfo = BuildSignedInfo(invoiceHash, signedPropertiesHash);

                // Sign the SignedInfo
                var signatureValue = SignData(signedInfo, privateKey);
                result.SignatureValue = Convert.ToBase64String(signatureValue);

                // Extract public key for QR code
                result.PublicKey = ExtractPublicKey(privateKey);

                // Build complete signature element
                var signatureElement = BuildSignatureElement(
                    signedInfo,
                    signatureValue,
                    certificate,
                    signedProperties);

                // Insert signature into invoice XML
                var signedXml = InsertSignature(invoiceXml, signatureElement);
                result.SignedXml = signedXml;

                // Extract certificate signature for QR
                result.CertificateSignature = ExtractCertificateSignature(certificate);

                result.Success = true;
                result.Message = "Invoice signed successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Signing failed: {ex.Message}";
                result.ErrorDetails = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// Verify signature in signed invoice XML
        /// </summary>
        public bool VerifySignature(string signedXml, string certificateBase64)
        {
            try
            {
                // Extract signature value from XML
                var signatureValue = ExtractSignatureValue(signedXml);
                if (signatureValue == null) return false;

                // Extract SignedInfo
                var signedInfo = ExtractSignedInfo(signedXml);
                if (signedInfo == null) return false;

                // Load certificate
                var certificate = LoadCertificate(certificateBase64);

                // Get public key from certificate
                using var ecdsa = certificate.GetECDsaPublicKey();
                if (ecdsa == null) return false;

                // Verify signature
                var signedInfoBytes = Encoding.UTF8.GetBytes(signedInfo);
                return ecdsa.VerifyData(signedInfoBytes, signatureValue, HashAlgorithmName.SHA256);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extract signature components for QR code generation
        /// </summary>
        public QrSignatureComponents ExtractQrComponents(string signedXml, string certificateBase64)
        {
            var components = new QrSignatureComponents();

            try
            {
                // Extract signature value
                var signatureValueMatch = Regex.Match(signedXml, @"<ds:SignatureValue>([^<]+)</ds:SignatureValue>");
                if (signatureValueMatch.Success)
                {
                    components.SignatureValue = Convert.FromBase64String(signatureValueMatch.Groups[1].Value);
                }

                // Load certificate and extract public key
                var certificate = LoadCertificate(certificateBase64);
                using var ecdsa = certificate.GetECDsaPublicKey();
                if (ecdsa != null)
                {
                    var parameters = ecdsa.ExportParameters(false);
                    // Combine X and Y coordinates for QR
                    var publicKey = new byte[parameters.Q.X.Length + parameters.Q.Y.Length];
                    Buffer.BlockCopy(parameters.Q.X, 0, publicKey, 0, parameters.Q.X.Length);
                    Buffer.BlockCopy(parameters.Q.Y, 0, publicKey, parameters.Q.X.Length, parameters.Q.Y.Length);
                    components.PublicKey = publicKey;
                }

                // Extract certificate signature
                components.CertificateSignature = ExtractCertificateSignature(certificate);

                components.Success = true;
            }
            catch (Exception ex)
            {
                components.Success = false;
                components.Error = ex.Message;
            }

            return components;
        }

        #endregion

        #region Private Methods - Certificate & Key Handling

        private X509Certificate2 LoadCertificate(string base64Cert)
        {
            try
            {
                // Remove PEM headers if present
                var cleanBase64 = CleanPemContent(base64Cert);
                var certBytes = Convert.FromBase64String(cleanBase64);
                return new X509Certificate2(certBytes);
            }
            catch (Exception ex)
            {
                throw new AppException(ExceptionCode.FINANCE_ZATCA_CERTIFICATE_LOAD_FAILED, ex.Message);
            }
        }

        private ECDsa LoadPrivateKey(string base64Key)
        {
            try
            {
                var cleanBase64 = CleanPemContent(base64Key);
                var keyBytes = Convert.FromBase64String(cleanBase64);

                var ecdsa = ECDsa.Create();

                // Try to import as PKCS#8
                try
                {
                    ecdsa.ImportPkcs8PrivateKey(keyBytes, out _);
                    return ecdsa;
                }
                catch
                {
                    // Try EC private key format
                    ecdsa.ImportECPrivateKey(keyBytes, out _);
                    return ecdsa;
                }
            }
            catch (Exception ex)
            {
                throw new AppException(ExceptionCode.FINANCE_ZATCA_PRIVATE_KEY_LOAD_FAILED, ex.Message);
            }
        }

        private string CleanPemContent(string pem)
        {
            // Remove PEM headers and footers
            var result = pem
                .Replace("-----BEGIN CERTIFICATE-----", "")
                .Replace("-----END CERTIFICATE-----", "")
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("-----BEGIN EC PRIVATE KEY-----", "")
                .Replace("-----END EC PRIVATE KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();

            return result;
        }

        private byte[] ExtractPublicKey(ECDsa privateKey)
        {
            var parameters = privateKey.ExportParameters(false);
            var publicKey = new byte[parameters.Q.X.Length + parameters.Q.Y.Length];
            Buffer.BlockCopy(parameters.Q.X, 0, publicKey, 0, parameters.Q.X.Length);
            Buffer.BlockCopy(parameters.Q.Y, 0, publicKey, parameters.Q.X.Length, parameters.Q.Y.Length);
            return publicKey;
        }

        private byte[] ExtractCertificateSignature(X509Certificate2 certificate)
        {
            // The certificate's signature is part of the raw data
            // For ZATCA, we need the signature value from the certificate
            try
            {
                // Get the TBS (To Be Signed) certificate hash
                // This is a simplified extraction - in production, parse ASN.1 structure
                var rawData = certificate.RawData;

                // Return last 64-72 bytes which typically contain the ECDSA signature
                var signatureLength = Math.Min(72, rawData.Length / 4);
                var signature = new byte[signatureLength];
                Buffer.BlockCopy(rawData, rawData.Length - signatureLength, signature, 0, signatureLength);

                return signature;
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        #endregion

        #region Private Methods - Signature Building

        private string BuildSignedProperties(X509Certificate2 certificate, string invoiceXml)
        {
            var signingTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var certDigest = _hashCalculator.CalculateHash(certificate.RawData);
            var issuerName = certificate.IssuerName.Name;
            var serialNumber = certificate.SerialNumber;

            return $@"<xades:SignedProperties Id=""xadesSignedProperties"">
    <xades:SignedSignatureProperties>
        <xades:SigningTime>{signingTime}</xades:SigningTime>
        <xades:SigningCertificate>
            <xades:Cert>
                <xades:CertDigest>
                    <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
                    <ds:DigestValue>{certDigest}</ds:DigestValue>
                </xades:CertDigest>
                <xades:IssuerSerial>
                    <ds:X509IssuerName>{issuerName}</ds:X509IssuerName>
                    <ds:X509SerialNumber>{serialNumber}</ds:X509SerialNumber>
                </xades:IssuerSerial>
            </xades:Cert>
        </xades:SigningCertificate>
    </xades:SignedSignatureProperties>
</xades:SignedProperties>";
        }

        private string BuildSignedInfo(string invoiceHash, string signedPropertiesHash)
        {
            return $@"<ds:SignedInfo>
    <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2006/12/xml-c14n11""/>
    <ds:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256""/>
    <ds:Reference Id=""invoiceSignedData"" URI="""">
        <ds:Transforms>
            <ds:Transform Algorithm=""http://www.w3.org/TR/1999/REC-xpath-19991116"">
                <ds:XPath>not(//ancestor-or-self::ext:UBLExtensions)</ds:XPath>
            </ds:Transform>
            <ds:Transform Algorithm=""http://www.w3.org/TR/1999/REC-xpath-19991116"">
                <ds:XPath>not(//ancestor-or-self::cac:Signature)</ds:XPath>
            </ds:Transform>
            <ds:Transform Algorithm=""http://www.w3.org/TR/1999/REC-xpath-19991116"">
                <ds:XPath>not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])</ds:XPath>
            </ds:Transform>
            <ds:Transform Algorithm=""http://www.w3.org/2006/12/xml-c14n11""/>
        </ds:Transforms>
        <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
        <ds:DigestValue>{invoiceHash}</ds:DigestValue>
    </ds:Reference>
    <ds:Reference Type=""http://www.w3.org/2000/09/xmldsig#SignatureProperties"" URI=""#xadesSignedProperties"">
        <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
        <ds:DigestValue>{signedPropertiesHash}</ds:DigestValue>
    </ds:Reference>
</ds:SignedInfo>";
        }

        private byte[] SignData(string signedInfo, ECDsa privateKey)
        {
            var dataBytes = Encoding.UTF8.GetBytes(signedInfo);
            return privateKey.SignData(dataBytes, HashAlgorithmName.SHA256);
        }

        private string BuildSignatureElement(
            string signedInfo,
            byte[] signatureValue,
            X509Certificate2 certificate,
            string signedProperties)
        {
            var signatureValueBase64 = Convert.ToBase64String(signatureValue);
            var certificateBase64 = Convert.ToBase64String(certificate.RawData);

            return $@"<ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Id=""signature"">
    {signedInfo}
    <ds:SignatureValue>{signatureValueBase64}</ds:SignatureValue>
    <ds:KeyInfo>
        <ds:X509Data>
            <ds:X509Certificate>{certificateBase64}</ds:X509Certificate>
        </ds:X509Data>
    </ds:KeyInfo>
    <ds:Object>
        <xades:QualifyingProperties xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"" Target=""#signature"">
            {signedProperties}
        </xades:QualifyingProperties>
    </ds:Object>
</ds:Signature>";
        }

        private string InsertSignature(string invoiceXml, string signatureElement)
        {
            // Replace the signature placeholder with actual signature
            var result = invoiceXml.Replace(
                "<!--SIGNATURE_PLACEHOLDER-->",
                signatureElement);

            return result;
        }

        private byte[] ExtractSignatureValue(string signedXml)
        {
            var match = Regex.Match(signedXml, @"<ds:SignatureValue>([^<]+)</ds:SignatureValue>");
            if (match.Success)
            {
                return Convert.FromBase64String(match.Groups[1].Value);
            }
            return null;
        }

        private string ExtractSignedInfo(string signedXml)
        {
            var match = Regex.Match(signedXml, @"<ds:SignedInfo>[\s\S]*?</ds:SignedInfo>");
            if (match.Success)
            {
                return match.Value;
            }
            return null;
        }

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// Result of signing operation
    /// </summary>
    public class SigningResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }
        public string SignedXml { get; set; }
        public string InvoiceHash { get; set; }
        public string SignatureValue { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] CertificateSignature { get; set; }
    }

    /// <summary>
    /// Signature components needed for QR code
    /// </summary>
    public class QrSignatureComponents
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public byte[] SignatureValue { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] CertificateSignature { get; set; }
    }

    #endregion
}
