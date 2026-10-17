using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using ZR.Model.Finance.Zatca.Enums;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using JsonNamingPolicy = System.Text.Json.JsonNamingPolicy;

namespace ZR.Service.Finance.Zatca.Utils
{
    /// <summary>
    /// ZATCA API Client Implementation
    /// تنفيذ عميل API زاتكا
    /// Handles all HTTP communication with ZATCA e-invoicing APIs
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaApiClient), ServiceLifetime = LifeTime.Singleton)]
    public class ZatcaApiClient : IZatcaApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        #region API Endpoints

        private const string SandboxBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal";
        private const string SimulationBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation";
        private const string ProductionBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/core";

        // Endpoints
        private const string ComplianceCsidEndpoint = "/compliance";
        private const string ComplianceCheckEndpoint = "/compliance/invoices";
        private const string ProductionCsidEndpoint = "/production/csids";
        private const string ReportingEndpoint = "/invoices/reporting/single";
        private const string ClearanceEndpoint = "/invoices/clearance/single";

        #endregion

        public ZatcaApiClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public ZatcaApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #region Invoice Submission

        public async Task<ZatcaApiResponse> ReportSimplifiedInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret)
        {
            var baseUrl = GetBaseUrl(environment);
            var endpoint = $"{baseUrl}{ReportingEndpoint}";

            var request = new ZatcaInvoiceRequest
            {
                InvoiceHash = invoiceHash,
                Uuid = uuid,
                Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml))
            };

            return await SendInvoiceRequestAsync(endpoint, request, csid, secret);
        }

        public async Task<ZatcaApiResponse> ClearStandardInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret)
        {
            var baseUrl = GetBaseUrl(environment);
            var endpoint = $"{baseUrl}{ClearanceEndpoint}";

            var request = new ZatcaInvoiceRequest
            {
                InvoiceHash = invoiceHash,
                Uuid = uuid,
                Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml))
            };

            return await SendInvoiceRequestAsync(endpoint, request, csid, secret);
        }

        public async Task<ZatcaApiResponse> ReportCreditDebitNoteAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret,
            bool isSimplified)
        {
            // Credit/Debit notes use same endpoints as invoices
            if (isSimplified)
            {
                return await ReportSimplifiedInvoiceAsync(signedXml, invoiceHash, uuid, environment, csid, secret);
            }
            else
            {
                return await ClearStandardInvoiceAsync(signedXml, invoiceHash, uuid, environment, csid, secret);
            }
        }

        #endregion

        #region Compliance Operations

        public async Task<ComplianceCsidResponse> RequestComplianceCsidAsync(
            string csr,
            string otp,
            ZatcaEnvironment environment)
        {
            var baseUrl = GetBaseUrl(environment);
            var endpoint = $"{baseUrl}{ComplianceCsidEndpoint}";

            var response = new ComplianceCsidResponse();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("OTP", otp);
                request.Headers.Add("Accept-Version", "V2");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new { csr };
                var jsonContent = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(jsonContent, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var httpResponse = await _httpClient.SendAsync(request);
                var responseBody = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var zatcaResponse = JsonSerializer.Deserialize<ZatcaComplianceCsidApiResponse>(responseBody, _jsonOptions);

                    response.Success = true;
                    response.RequestId = zatcaResponse?.RequestId;
                    response.BinarySecurityToken = zatcaResponse?.BinarySecurityToken;
                    response.Secret = zatcaResponse?.Secret;
                    response.DispositionMessage = zatcaResponse?.DispositionMessage;

                    // Parse certificate from BST
                    if (!string.IsNullOrEmpty(zatcaResponse?.BinarySecurityToken))
                    {
                        response.Certificate = zatcaResponse.BinarySecurityToken;
                        // TODO: Parse certificate to extract expiry date
                        response.ExpiryDate = DateTime.UtcNow.AddYears(1);
                    }
                }
                else
                {
                    response.Success = false;
                    response.ErrorCode = httpResponse.StatusCode.ToString();
                    response.ErrorMessage = $"ZATCA API error: {responseBody}";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Request failed: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
            }

            return response;
        }

        public async Task<ZatcaApiResponse> SubmitComplianceCheckAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            ZatcaEnvironment environment,
            string complianceCsid,
            string complianceSecret)
        {
            var baseUrl = GetBaseUrl(environment);
            var endpoint = $"{baseUrl}{ComplianceCheckEndpoint}";

            var request = new ZatcaInvoiceRequest
            {
                InvoiceHash = invoiceHash,
                Uuid = uuid,
                Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml))
            };

            return await SendInvoiceRequestAsync(endpoint, request, complianceCsid, complianceSecret);
        }

        public async Task<ProductionCsidResponse> RequestProductionCsidAsync(
            string complianceRequestId,
            ZatcaEnvironment environment,
            string complianceCsid,
            string complianceSecret)
        {
            var baseUrl = GetBaseUrl(environment);
            var endpoint = $"{baseUrl}{ProductionCsidEndpoint}";

            var response = new ProductionCsidResponse();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                SetAuthorizationHeader(request, complianceCsid, complianceSecret);
                request.Headers.Add("Accept-Version", "V2");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new { compliance_request_id = complianceRequestId };
                request.Content = new StringContent(
                    JsonSerializer.Serialize(content, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var httpResponse = await _httpClient.SendAsync(request);
                var responseBody = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var zatcaResponse = JsonSerializer.Deserialize<ZatcaProductionCsidApiResponse>(responseBody, _jsonOptions);

                    response.Success = true;
                    response.RequestId = zatcaResponse?.RequestId;
                    response.BinarySecurityToken = zatcaResponse?.BinarySecurityToken;
                    response.Secret = zatcaResponse?.Secret;
                    response.DispositionMessage = zatcaResponse?.DispositionMessage;

                    if (!string.IsNullOrEmpty(zatcaResponse?.BinarySecurityToken))
                    {
                        response.Certificate = zatcaResponse.BinarySecurityToken;
                        response.ExpiryDate = DateTime.UtcNow.AddYears(1);
                    }
                }
                else
                {
                    response.Success = false;
                    response.ErrorCode = httpResponse.StatusCode.ToString();
                    response.ErrorMessage = $"ZATCA API error: {responseBody}";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Request failed: {ex.Message}";
            }

            return response;
        }

        public async Task<ProductionCsidResponse> RenewProductionCsidAsync(
            string csr,
            string otp,
            ZatcaEnvironment environment,
            string existingCsid,
            string existingSecret)
        {
            var baseUrl = GetBaseUrl(environment);
            var endpoint = $"{baseUrl}{ProductionCsidEndpoint}";

            var response = new ProductionCsidResponse();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
                SetAuthorizationHeader(request, existingCsid, existingSecret);
                request.Headers.Add("OTP", otp);
                request.Headers.Add("Accept-Version", "V2");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new { csr };
                var jsonContent = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(jsonContent, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var httpResponse = await _httpClient.SendAsync(request);
                var responseBody = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var zatcaResponse = JsonSerializer.Deserialize<ZatcaProductionCsidApiResponse>(responseBody, _jsonOptions);

                    response.Success = true;
                    response.RequestId = zatcaResponse?.RequestId;
                    response.BinarySecurityToken = zatcaResponse?.BinarySecurityToken;
                    response.Secret = zatcaResponse?.Secret;
                    response.DispositionMessage = zatcaResponse?.DispositionMessage;

                    if (!string.IsNullOrEmpty(zatcaResponse?.BinarySecurityToken))
                    {
                        response.Certificate = zatcaResponse.BinarySecurityToken;
                        response.ExpiryDate = DateTime.UtcNow.AddYears(1);
                    }
                }
                else
                {
                    response.Success = false;
                    response.ErrorCode = httpResponse.StatusCode.ToString();
                    response.ErrorMessage = $"ZATCA API error: {responseBody}";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Request failed: {ex.Message}";
            }

            return response;
        }

        #endregion

        #region Status Operations

        public async Task<InvoiceStatusResponse> GetInvoiceStatusAsync(
            string uuid,
            ZatcaEnvironment environment,
            string csid,
            string secret)
        {
            // Note: ZATCA may not have a direct status check endpoint
            // This is a placeholder for potential future implementation
            return new InvoiceStatusResponse
            {
                Success = false,
                Uuid = uuid,
                ErrorMessage = "Status check not implemented. Status is returned during submission."
            };
        }

        public async Task<bool> TestConnectionAsync(ZatcaEnvironment environment)
        {
            try
            {
                var baseUrl = GetBaseUrl(environment);
                var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);

                // Even a 401/403 indicates the server is reachable
                return response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable &&
                       response.StatusCode != System.Net.HttpStatusCode.GatewayTimeout;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Private Methods

        private async Task<ZatcaApiResponse> SendInvoiceRequestAsync(
            string endpoint,
            ZatcaInvoiceRequest request,
            string csid,
            string secret)
        {
            var response = new ZatcaApiResponse();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
                SetAuthorizationHeader(httpRequest, csid, secret);
                httpRequest.Headers.Add("Accept-Version", "V2");
                httpRequest.Headers.Add("Accept-Language", "en");
                httpRequest.Headers.Add("Clearance-Status", "1"); // 1 = request clearance, 0 = report only
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
                httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var httpResponse = await _httpClient.SendAsync(httpRequest);
                stopwatch.Stop();

                response.HttpStatusCode = (int)httpResponse.StatusCode;
                response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                response.ResponseHeaders = httpResponse.Headers.ToString();

                var responseBody = await httpResponse.Content.ReadAsStringAsync();
                response.RawResponse = responseBody;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var zatcaResponse = JsonSerializer.Deserialize<ZatcaInvoiceApiResponse>(responseBody, _jsonOptions);

                    response.Success = true;
                    response.DispositionStatus = zatcaResponse?.ReportingStatus ?? zatcaResponse?.ClearanceStatus;
                    response.ClearanceStatus = zatcaResponse?.ClearanceStatus;
                    response.ReportingStatus = zatcaResponse?.ReportingStatus;
                    response.ClearedInvoiceXml = zatcaResponse?.ClearedInvoice;
                    response.WarningCount = zatcaResponse?.WarningMessages?.Length ?? 0;
                    response.InfoCount = zatcaResponse?.InfoMessages?.Length ?? 0;

                    // Determine submission status
                    response.SubmissionStatus = DetermineSubmissionStatus(
                        zatcaResponse?.ClearanceStatus,
                        zatcaResponse?.ReportingStatus,
                        response.WarningCount > 0);

                    // Parse validation results
                    response.ValidationResults = ParseValidationResults(zatcaResponse);
                }
                else
                {
                    response.Success = false;
                    response.ErrorCode = httpResponse.StatusCode.ToString();

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ZatcaErrorResponse>(responseBody, _jsonOptions);
                        response.ErrorMessage = errorResponse?.Error ?? responseBody;
                        response.ValidationResults = errorResponse?.ValidationResults;
                        response.ErrorCount = errorResponse?.ErrorMessages?.Length ?? 1;
                    }
                    catch
                    {
                        response.ErrorMessage = responseBody;
                        response.ErrorCount = 1;
                    }

                    response.SubmissionStatus = response.HttpStatusCode == 400
                        ? ZatcaSubmissionStatus.Rejected
                        : ZatcaSubmissionStatus.Failed;
                }
            }
            catch (TaskCanceledException)
            {
                stopwatch.Stop();
                response.Success = false;
                response.ErrorMessage = "Request timed out";
                response.ErrorCode = "TIMEOUT";
                response.SubmissionStatus = ZatcaSubmissionStatus.Failed;
                response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                response.Success = false;
                response.ErrorMessage = $"Request failed: {ex.Message}";
                response.ErrorCode = "EXCEPTION";
                response.SubmissionStatus = ZatcaSubmissionStatus.Failed;
                response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            }

            return response;
        }

        private string GetBaseUrl(ZatcaEnvironment environment)
        {
            return environment switch
            {
                ZatcaEnvironment.Sandbox => SandboxBaseUrl,
                ZatcaEnvironment.Simulation => SimulationBaseUrl,
                ZatcaEnvironment.Production => ProductionBaseUrl,
                _ => SandboxBaseUrl
            };
        }

        private void SetAuthorizationHeader(HttpRequestMessage request, string csid, string secret)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{csid}:{secret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        private ZatcaSubmissionStatus DetermineSubmissionStatus(string clearanceStatus, string reportingStatus, bool hasWarnings)
        {
            if (!string.IsNullOrEmpty(clearanceStatus))
            {
                return clearanceStatus.ToUpperInvariant() switch
                {
                    "CLEARED" => hasWarnings ? ZatcaSubmissionStatus.ClearedWithWarnings : ZatcaSubmissionStatus.Cleared,
                    "NOT_CLEARED" or "REJECTED" => ZatcaSubmissionStatus.Rejected,
                    _ => ZatcaSubmissionStatus.Pending
                };
            }

            if (!string.IsNullOrEmpty(reportingStatus))
            {
                return reportingStatus.ToUpperInvariant() switch
                {
                    "REPORTED" => hasWarnings ? ZatcaSubmissionStatus.ReportedWithWarnings : ZatcaSubmissionStatus.Reported,
                    "NOT_REPORTED" or "REJECTED" => ZatcaSubmissionStatus.Rejected,
                    _ => ZatcaSubmissionStatus.Pending
                };
            }

            return ZatcaSubmissionStatus.Pending;
        }

        private ZatcaValidationResult[] ParseValidationResults(ZatcaInvoiceApiResponse response)
        {
            var results = new System.Collections.Generic.List<ZatcaValidationResult>();

            if (response?.WarningMessages != null)
            {
                foreach (var msg in response.WarningMessages)
                {
                    results.Add(new ZatcaValidationResult
                    {
                        Type = "WARNING",
                        Code = msg.Code,
                        Category = msg.Category,
                        Message = msg.Message,
                        Status = msg.Status
                    });
                }
            }

            if (response?.InfoMessages != null)
            {
                foreach (var msg in response.InfoMessages)
                {
                    results.Add(new ZatcaValidationResult
                    {
                        Type = "INFO",
                        Code = msg.Code,
                        Category = msg.Category,
                        Message = msg.Message,
                        Status = msg.Status
                    });
                }
            }

            return results.ToArray();
        }

        #endregion

        #region Internal Request/Response Models

        private class ZatcaInvoiceRequest
        {
            public string InvoiceHash { get; set; }
            public string Uuid { get; set; }
            public string Invoice { get; set; }
        }

        private class ZatcaInvoiceApiResponse
        {
            public string ClearanceStatus { get; set; }
            public string ReportingStatus { get; set; }
            public string ClearedInvoice { get; set; }
            public ZatcaMessageItem[] WarningMessages { get; set; }
            public ZatcaMessageItem[] InfoMessages { get; set; }
            public ZatcaMessageItem[] ErrorMessages { get; set; }
        }

        private class ZatcaMessageItem
        {
            public string Type { get; set; }
            public string Code { get; set; }
            public string Category { get; set; }
            public string Message { get; set; }
            public string Status { get; set; }
        }

        private class ZatcaErrorResponse
        {
            public string Error { get; set; }
            public ZatcaMessageItem[] ErrorMessages { get; set; }
            public ZatcaValidationResult[] ValidationResults { get; set; }
        }

        private class ZatcaComplianceCsidApiResponse
        {
            public string RequestId { get; set; }
            public string DispositionMessage { get; set; }
            public string BinarySecurityToken { get; set; }
            public string Secret { get; set; }
        }

        private class ZatcaProductionCsidApiResponse
        {
            public string RequestId { get; set; }
            public string DispositionMessage { get; set; }
            public string BinarySecurityToken { get; set; }
            public string Secret { get; set; }
        }

        #endregion
    }
}
