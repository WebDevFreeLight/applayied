using System.ComponentModel.DataAnnotations;
using Zatca.EInvoice.SDK.Contracts.Models;
using ZatcaIntegrationApp.Models;

namespace ZatcaIntegrationApp.Models
{
    /// <summary>
    /// Request DTO for creating sample invoice
    /// </summary>
    public class CreateInvoiceRequestDto
    {
        public string InvoiceId { get; set; } = "INV-001";
        public string InvoiceTypeCode { get; set; } = "388";
        public string CurrencyCode { get; set; } = "SAR";
        
        public SupplierInfoDto Supplier { get; set; } = new SupplierInfoDto();
        public CustomerInfoDto Customer { get; set; } = new CustomerInfoDto();
        public List<ProductLineDto> Products { get; set; } = new List<ProductLineDto>();
    }

    /// <summary>
    /// Supplier information DTO
    /// </summary>
    public class SupplierInfoDto
    {
        public string CRN { get; set; } = "1010010000";
        public string StreetName { get; set; } = "الامير سلطان";
        public string BuildingNumber { get; set; } = "2322";
        public string CitySubdivision { get; set; } = "المربع";
        public string City { get; set; } = "الرياض";
        public string PostalCode { get; set; } = "23333";
    }

    /// <summary>
    /// Customer information DTO
    /// </summary>
    public class CustomerInfoDto
    {
        public string CRN { get; set; } = "1010010000";
        public string StreetName { get; set; } = "الامير سلطان";
        public string BuildingNumber { get; set; } = "2322";
        public string CitySubdivision { get; set; } = "المربع";
        public string City { get; set; } = "الرياض";
        public string PostalCode { get; set; } = "23333";
        public string VatId { get; set; } = "399999999800003";
    }

    /// <summary>
    /// Product line DTO for invoice creation
    /// </summary>
    public class ProductLineDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be positive")]
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = "PCE";
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be positive")]
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal TaxPercent { get; set; } = 15;
    }

    /// <summary>
    /// Request DTO for loading invoice from string
    /// </summary>
    public class LoadInvoiceRequestDto
    {
        [Required]
        public string InvoiceXml { get; set; }
    }

    /// <summary>
    /// Request DTO for signing invoice
    /// </summary>
    public class SignInvoiceRequestDto
    {
        [Required]
        public string InvoiceFilePath { get; set; }
        public string Uuid { get; set; }
        public string Base64Invoice { get; set; }
        public string InvoiceHash { get; set; }
    }

    /// <summary>
    /// Request DTO for uploading CSR
    /// </summary>
    public class UploadCsrRequestDto
    {
        [Required]
        public string CsrContent { get; set; }
    }

    /// <summary>
    /// Request DTO for getting certificate
    /// </summary>
    public class GetCertificateRequestDto
    {
        [Required]
        public string RequestId { get; set; }
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response DTO for CSR generation
    /// </summary>
    public class CsrGenerationResponseDto
    {
        public bool IsValid { get; set; }
        public string CsrBase64 { get; set; }
        public string PrivateKeyBase64 { get; set; }

        public CsrResult csrResult { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response DTO for invoice operations
    /// </summary>
    public class InvoiceOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public ZATCAResponse ZatcaResponse { get; set; }
    }

    /// <summary>
    /// Response DTO for certificate operations
    /// </summary>
    public class CertificateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Certificate { get; set; }
        public string RequestId { get; set; }
        public string Status { get; set; }
        public string CertificateContent { get; set; }
        public string TokenContent { get; set; }
    }
}
