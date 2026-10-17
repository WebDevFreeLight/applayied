using System;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Model.Finance.Zatca.Dto
{
    /// <summary>
    /// ZATCA Configuration DTO - إعدادات زاتكا
    /// </summary>
    public class ZatcaConfigurationDto
    {
        public Guid Id { get; set; }

        #region Seller Information

        public string SellerVatNumber { get; set; }
        public string SellerNameAr { get; set; }
        public string SellerNameEn { get; set; }
        public string SellerStreet { get; set; }
        public string SellerBuildingNumber { get; set; }
        public string SellerPlotIdentification { get; set; }
        public string SellerCitySubdivision { get; set; }
        public string SellerCity { get; set; }
        public string SellerPostalCode { get; set; }
        public string SellerCountryCode { get; set; }
        public string SellerCountrySubentity { get; set; }
        public string SellerAdditionalStreet { get; set; }

        #endregion

        #region API Configuration

        public ZatcaEnvironment Environment { get; set; }
        public string SandboxApiUrl { get; set; }
        public string SimulationApiUrl { get; set; }
        public string ProductionApiUrl { get; set; }
        public int ApiTimeout { get; set; }
        public int MaxRetryAttempts { get; set; }
        public int RetryDelayMs { get; set; }

        #endregion

        #region Invoice Settings

        public ZatcaPaymentMeansCode DefaultPaymentMeansCode { get; set; }
        public bool AutoSubmitStandard { get; set; }
        public bool AutoSubmitSimplified { get; set; }
        public bool QrForStandard { get; set; }
        public bool QrForSimplified { get; set; }
        public string DefaultCurrency { get; set; }

        #endregion

        #region Status

        public Guid? ActiveDeviceId { get; set; }
        public string ActiveDeviceName { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsConfigured { get; set; }
        public DateTime? LastConfigCheck { get; set; }

        #endregion
    }

    /// <summary>
    /// Create/Update ZATCA Configuration DTO - إنشاء/تحديث إعدادات زاتكا
    /// </summary>
    public class SaveZatcaConfigurationDto
    {
        #region Seller Information

        /// <summary>
        /// Seller VAT Registration Number (15 digits)
        /// </summary>
        public string SellerVatNumber { get; set; }

        /// <summary>
        /// Seller Name in Arabic (required)
        /// </summary>
        public string SellerNameAr { get; set; }

        /// <summary>
        /// Seller Name in English
        /// </summary>
        public string SellerNameEn { get; set; }

        /// <summary>
        /// Street Name
        /// </summary>
        public string SellerStreet { get; set; }

        /// <summary>
        /// Building Number (4 digits)
        /// </summary>
        public string SellerBuildingNumber { get; set; }

        /// <summary>
        /// Plot Identification (4 digits, optional)
        /// </summary>
        public string SellerPlotIdentification { get; set; }

        /// <summary>
        /// District/City Subdivision
        /// </summary>
        public string SellerCitySubdivision { get; set; }

        /// <summary>
        /// City Name
        /// </summary>
        public string SellerCity { get; set; }

        /// <summary>
        /// Postal Code (5 digits)
        /// </summary>
        public string SellerPostalCode { get; set; }

        /// <summary>
        /// Country Code (SA)
        /// </summary>
        public string SellerCountryCode { get; set; } = "SA";

        /// <summary>
        /// Region/Province
        /// </summary>
        public string SellerCountrySubentity { get; set; }

        /// <summary>
        /// Additional Street Name (optional)
        /// </summary>
        public string SellerAdditionalStreet { get; set; }

        #endregion

        #region API Configuration

        /// <summary>
        /// Current Environment
        /// </summary>
        public ZatcaEnvironment Environment { get; set; } = ZatcaEnvironment.Sandbox;

        /// <summary>
        /// API Timeout in seconds
        /// </summary>
        public int ApiTimeout { get; set; } = 30;

        /// <summary>
        /// Maximum retry attempts for failed requests
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay between retries in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        #endregion

        #region Invoice Settings

        /// <summary>
        /// Default Payment Means Code
        /// </summary>
        public ZatcaPaymentMeansCode DefaultPaymentMeansCode { get; set; } = ZatcaPaymentMeansCode.CreditTransfer;

        /// <summary>
        /// Auto-submit standard invoices after posting
        /// </summary>
        public bool AutoSubmitStandard { get; set; } = false;

        /// <summary>
        /// Auto-submit simplified invoices after posting
        /// </summary>
        public bool AutoSubmitSimplified { get; set; } = false;

        /// <summary>
        /// Generate QR code for standard invoices
        /// </summary>
        public bool QrForStandard { get; set; } = true;

        /// <summary>
        /// Generate QR code for simplified invoices
        /// </summary>
        public bool QrForSimplified { get; set; } = true;

        /// <summary>
        /// Default Currency
        /// </summary>
        public string DefaultCurrency { get; set; } = "SAR";

        #endregion

        /// <summary>
        /// Active Device ID
        /// </summary>
        public Guid? ActiveDeviceId { get; set; }

        /// <summary>
        /// Is ZATCA Integration Enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Validate Configuration Response DTO - استجابة التحقق من الإعدادات
    /// </summary>
    public class ValidateConfigurationResponseDto
    {
        public bool IsValid { get; set; }
        public bool HasActiveDevice { get; set; }
        public bool HasValidCertificate { get; set; }
        public bool SellerInfoComplete { get; set; }
        public string[] MissingFields { get; set; }
        public string[] Warnings { get; set; }
        public string[] Errors { get; set; }
    }
}
