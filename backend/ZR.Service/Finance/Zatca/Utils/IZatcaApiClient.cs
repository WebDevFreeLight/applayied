using System;
using System.Threading.Tasks;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Service.Finance.Zatca.Utils
{
    /// <summary>
    /// ZATCA API Client Interface
    /// واجهة عميل API زاتكا
    /// </summary>
    public interface IZatcaApiClient
    {
        #region Invoice Submission

        /// <summary>
        /// Submit simplified invoice for reporting (B2C)
        /// </summary>
        Task<ZatcaApiResponse> ReportSimplifiedInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret);

        /// <summary>
        /// Submit standard invoice for clearance (B2B)
        /// </summary>
        Task<ZatcaApiResponse> ClearStandardInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret);

        /// <summary>
        /// Submit credit/debit note for reporting
        /// </summary>
        Task<ZatcaApiResponse> ReportCreditDebitNoteAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret,
            bool isSimplified);

        #endregion

        #region Compliance Operations

        /// <summary>
        /// Request compliance CSID (onboarding)
        /// </summary>
        Task<ComplianceCsidResponse> RequestComplianceCsidAsync(
            string csr,
            string otp,
            ZatcaEnvironment environment);

        /// <summary>
        /// Submit compliance check invoices
        /// </summary>
        Task<ZatcaApiResponse> SubmitComplianceCheckAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string complianceCsid,
            string complianceSecret);

        /// <summary>
        /// Request production CSID
        /// </summary>
        Task<ProductionCsidResponse> RequestProductionCsidAsync(
            string complianceRequestId,
            ZatcaEnvironment environment,
            string complianceCsid,
            string complianceSecret);

        /// <summary>
        /// Renew production CSID
        /// </summary>
        Task<ProductionCsidResponse> RenewProductionCsidAsync(
            string csr,
            string otp,
            ZatcaEnvironment environment,
            string existingCsid,
            string existingSecret);

        #endregion

        #region Status Operations

        /// <summary>
        /// Get invoice status from ZATCA
        /// </summary>
        Task<InvoiceStatusResponse> GetInvoiceStatusAsync(
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret);

        /// <summary>
        /// Check API connectivity
        /// </summary>
        Task<bool> TestConnectionAsync(ZatcaEnvironment environment);

        #endregion
    }

    #region Response Classes

    /// <summary>
    /// ZATCA API Response
    /// </summary>
    public class ZatcaApiResponse
    {
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// ZATCA disposition status (REPORTED, CLEARED, REJECTED, etc.)
        /// </summary>
        public string DispositionStatus { get; set; }

        /// <summary>
        /// Clearance status (for standard invoices)
        /// </summary>
        public string ClearanceStatus { get; set; }

        /// <summary>
        /// Reporting status (for simplified invoices)
        /// </summary>
        public string ReportingStatus { get; set; }

        /// <summary>
        /// Submission status enum
        /// </summary>
        public ZatcaSubmissionStatus SubmissionStatus { get; set; }

        /// <summary>
        /// Cleared/Reported invoice XML (returned by ZATCA for standard invoices)
        /// </summary>
        public string ClearedInvoiceXml { get; set; }

        /// <summary>
        /// Validation results from ZATCA
        /// </summary>
        public ZatcaValidationResult[] ValidationResults { get; set; }

        /// <summary>
        /// Warning count
        /// </summary>
        public int WarningCount { get; set; }

        /// <summary>
        /// Error count
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Info count
        /// </summary>
        public int InfoCount { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Raw response body
        /// </summary>
        public string RawResponse { get; set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public string ResponseHeaders { get; set; }

        /// <summary>
        /// Response time in milliseconds
        /// </summary>
        public long ResponseTimeMs { get; set; }
    }

    /// <summary>
    /// ZATCA Validation Result
    /// </summary>
    public class ZatcaValidationResult
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Compliance CSID Response
    /// </summary>
    public class ComplianceCsidResponse
    {
        public bool Success { get; set; }
        public string RequestId { get; set; }
        public string DispositionMessage { get; set; }
        public string BinarySecurityToken { get; set; }
        public string Secret { get; set; }
        public string Certificate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public ZatcaValidationResult[] ValidationResults { get; set; }
    }

    /// <summary>
    /// Production CSID Response
    /// </summary>
    public class ProductionCsidResponse
    {
        public bool Success { get; set; }
        public string RequestId { get; set; }
        public string DispositionMessage { get; set; }
        public string BinarySecurityToken { get; set; }
        public string Secret { get; set; }
        public string Certificate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public ZatcaValidationResult[] ValidationResults { get; set; }
    }

    /// <summary>
    /// Invoice Status Response
    /// </summary>
    public class InvoiceStatusResponse
    {
        public bool Success { get; set; }
        public string Uuid { get; set; }
        public string Status { get; set; }
        public string ClearanceStatus { get; set; }
        public string ReportingStatus { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public string ErrorMessage { get; set; }
    }

    #endregion
}
