using Microsoft.AspNetCore.Mvc;
using ZatcaIntegrationApp.Models;
using ZatcaIntegrationApp.Services;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;
using ZatcaIntegrationApp.Helpers;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Invoice Controller for ZATCA Integration
    /// Converts console application functionality to web API endpoints
    /// </summary>
  
        [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(InvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate Certificate Signing Request (CSR)
        /// </summary>
        /// <returns>CSR generation result</returns>
        [HttpPost("generate-csr")]
        public async Task<ActionResult<ApiResponse<CsrGenerationResponseDto>>> GenerateCSR()
        {

            try
            {
                _logger.LogInformation("Starting CSR generation");
                
           var CsrResult=     _invoiceService.GenerateCSR();
                
                var response = new ApiResponse<CsrGenerationResponseDto>
                {
                    Success = true,
                    Message = "CSR generated successfully",
                    Data = new CsrGenerationResponseDto
                    {
                        csrResult=CsrResult,
                        IsValid = true,
                        CsrBase64 = "",
                        PrivateKeyBase64 = "",
                        ErrorMessages = new List<string>()
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSR");
                return StatusCode(500, new ApiResponse<CsrGenerationResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while generating CSR",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Load invoice from XML string
        /// </summary>
        /// <param name="request">Invoice XML content</param>
        /// <returns>Load operation result</returns>
        [HttpPost("load-invoice")]
        public async Task<ActionResult<ApiResponse<InvoiceOperationResponseDto>>> LoadInvoice([FromBody] LoadInvoiceRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<InvoiceOperationResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                _logger.LogInformation("Loading invoice from XML string");
                
                var result = _invoiceService.LoadInvoiceFromString(request.InvoiceXml);
                
                var response = new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = result != null,
                    Message = result != null ? "Invoice loaded successfully" : "Failed to load invoice",// dd
                    Data = new InvoiceOperationResponseDto
                    {
                        Success = result != null,
                        Message = result != null ? "Invoice loaded and validated" : "Invalid invoice XML"
                    }
                };



                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoice from XML");
                return StatusCode(500, new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while loading invoice",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Sign invoice
        /// </summary>
        /// <param name="request">Invoice file path</param>
        /// <returns>Sign operation result</returns>
        [HttpPost("sign-invoice")]
        public async Task<ActionResult<ApiResponse<InvoiceOperationResponseDto>>> SignInvoice([FromBody] SignInvoiceRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<InvoiceOperationResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                _logger.LogInformation("Signing invoice: {FilePath}", request.InvoiceFilePath);
                
                // Load the XML document from file path
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(request.InvoiceFilePath);
                
                await _invoiceService.Sign(xmlDoc);
                
                var response = new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = true,
                    Message = "Invoice signed successfully",
                    Data = new InvoiceOperationResponseDto
                    {
                        Success = true,
                        Message = "Invoice signed successfully",
                        FilePath = request.InvoiceFilePath
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing invoice");
                return StatusCode(500, new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while signing invoice",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Send invoice to ZATCA
        /// </summary>
        /// <param name="request">Invoice file path</param>
        /// <returns>Send operation result</returns>
        [HttpPost("send-invoice")]
        public async Task<ActionResult<ApiResponse<InvoiceOperationResponseDto>>> SendInvoice([FromBody] SignInvoiceRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<InvoiceOperationResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                _logger.LogInformation("Sending invoice to ZATCA: {FilePath}", request.InvoiceFilePath);
                
                await _invoiceService.SendInvoiceAsync(request.InvoiceHash, request.Uuid ?? Guid.NewGuid().ToString(), request.Base64Invoice);
                
                var response = new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = true,
                    Message = "Invoice sent to ZATCA successfully",
                    Data = new InvoiceOperationResponseDto
                    {
                        Success = true,
                        Message = "Invoice submitted to ZATCA"
                    }
                };



                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice to ZATCA");
                return StatusCode(500, new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while sending invoice to ZATCA",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Create sample invoice from CSR data
        /// </summary>
        /// <param name="request">Invoice creation request</param>
        /// <returns>Invoice creation result</returns>
        [HttpPost("create-sample-invoice")]
        public async Task<ActionResult<ApiResponse<InvoiceOperationResponseDto>>> CreateSampleInvoice([FromBody] CreateInvoiceRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<InvoiceOperationResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                _logger.LogInformation("Creating sample invoice");
                
                // Convert DTO to the format expected by the service
                // Note: The original service method uses console input, so we need to adapt it
                _invoiceService.CreateSampleInvoiceFromCsrData();
                
                var response = new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = true,
                    Message = "Sample invoice created successfully",
                    Data = new InvoiceOperationResponseDto
                    {
                        Success = true,
                        Message = "Sample invoice created from CSR data"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample invoice");
                return StatusCode(500, new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while creating sample invoice",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Upload CSR to ZATCA compliance portal
        /// </summary>
        /// <param name="request">CSR content</param>
        /// <returns>Upload operation result</returns>
        [HttpPost("upload-csr")]
        public async Task<ActionResult<ApiResponse<InvoiceOperationResponseDto>>> UploadCsr([FromBody] UploadCsrRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<InvoiceOperationResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                _logger.LogInformation("Uploading CSR to ZATCA compliance portal");
                
              await  _invoiceService.UploadCsrAsync();
                
                var response = new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = true,
                    Message = "CSR uploaded successfully",
                    Data = new InvoiceOperationResponseDto
                    {
                        Success = true,
                        Message = "CSR submitted to ZATCA compliance portal",
                        ZatcaResponse = null
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CSR");
                return StatusCode(500, new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while uploading CSR",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Get certificate using compliance request ID
        /// </summary>
        /// <param name="request">Request ID</param>
        /// <returns>Certificate retrieval result</returns>
        [HttpPost("get-certificate")]
        public async Task<ActionResult<ApiResponse<CertificateResponseDto>>> GetCertificate([FromBody] GetCertificateRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CertificateResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                _logger.LogInformation("Getting certificate for request ID: {RequestId}", request.RequestId);
                
                _invoiceService.Getcertificate();
                
                var response = new ApiResponse<CertificateResponseDto>
                {
                    Success = true,
                    Message = "Certificate retrieved successfully",
                    Data = new CertificateResponseDto
                    {
                        Certificate = "Certificate retrieved successfully",
                        RequestId = request.RequestId,
                        Status = "Completed"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificate");
                return StatusCode(500, new ApiResponse<CertificateResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while getting certificate",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Complete ZATCA E-Invoice Integration Workflow
        /// Handles the entire process: CSR generation, upload, certificate retrieval, invoice creation, and signing
        /// </summary>
        /// <returns>Complete workflow result</returns>
      
        [AllowAnonymous]
        
        [HttpPost("complete-workflow")]
        public async Task<ActionResult<ApiResponse<InvoiceOperationResponseDto>>> CompleteWorkflow()
        {
            try
            {
                _logger.LogInformation("===============================================");
                _logger.LogInformation("     ZATCA E-Invoice Integration - Sandbox");
                _logger.LogInformation("===============================================");

                var response = new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = true,
                    Message = "ZATCA integration workflow completed successfully",
                    Data = new InvoiceOperationResponseDto
                    {
                        Success = true,
                        Message = "Complete workflow executed successfully"
                    }
                };

                // ============ 1. Generate CSR and PrivateKey ============
                _logger.LogInformation("Step 1: Generating CSR and PrivateKey...");
                var csrResult = _invoiceService.GenerateCSR();
                if (!csrResult.IsValid)
                {
                    return BadRequest(new ApiResponse<InvoiceOperationResponseDto>
                    {
                        Success = false,
                        Message = "Failed to generate CSR",
                        Errors = csrResult.ErrorMessages?.ToList() ?? new List<string>()
                    });
                }

                // ============ 2. Upload CSR ======================
                _logger.LogInformation("Step 2: Uploading CSR to ZATCA...");
                await _invoiceService.UploadCsrAsync();

                // ============ 3. Get Certificate ======================
                _logger.LogInformation("Step 3: Retrieving certificate from ZATCA...");
                await _invoiceService.Getcertificate();

                // ============ 4. Create UBL Invoice ======================
                _logger.LogInformation("Step 4: Creating sample invoice...");
                _invoiceService.CreateSampleInvoiceFromCsrData();

                // ============ 5. Sign UBL Invoice ======================
                _logger.LogInformation("Step 5: Loading and signing invoice...");
                var xmlContent = global::System.IO.File.ReadAllText("Data/Invoice.xml");
                var invoiceDoc = _invoiceService.LoadInvoiceFromString(xmlContent);
                await _invoiceService.Sign(invoiceDoc);

                _logger.LogInformation("🎉 The process was completed successfully.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in complete ZATCA workflow");
                return StatusCode(500, new ApiResponse<InvoiceOperationResponseDto>
                {
                    Success = false,
                    Message = "Internal server error occurred during ZATCA workflow",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Helper method to create sample invoice from request DTO
        /// This replaces the console input logic from the original service
        /// </summary>
        private string CreateSampleInvoiceFromRequest(CreateInvoiceRequestDto request)
        {
            try
            {
                // This is a simplified version that would need to be adapted
                // based on the actual CreateSampleInvoiceFromCsrData implementation
                // For now, we'll return a placeholder indicating the method needs implementation
                
                _logger.LogInformation("Creating invoice with ID: {InvoiceId}", request.InvoiceId);
                
                // TODO: Implement the actual invoice creation logic
                // This should mirror the CreateSampleInvoiceFromCsrData method
                // but use the request DTO instead of console input
                
                return "sample_invoice_path.xml"; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateSampleInvoiceFromRequest");
                return null;
            }
        }
    }
}
