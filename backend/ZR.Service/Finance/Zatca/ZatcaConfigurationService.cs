using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Repository.Finance.Zatca;
using ZR.Service.IService.Finance.Zatca;

namespace ZR.Service.Finance.Zatca
{
    /// <summary>
    /// ZATCA Configuration Service
    /// خدمة إعدادات زاتكا
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaConfigurationService), ServiceLifetime = LifeTime.Transient)]
    public class ZatcaConfigurationService : IZatcaConfigurationService
    {
        private readonly IZatcaConfigurationRepository _configRepository;
        private readonly IZatcaDeviceRepository _deviceRepository;

        public ZatcaConfigurationService(
            IZatcaConfigurationRepository configRepository,
            IZatcaDeviceRepository deviceRepository)
        {
            _configRepository = configRepository;
            _deviceRepository = deviceRepository;
        }

        public ZatcaConfigurationDto GetConfiguration(long? companyId = null)
        {
            var config = _configRepository.GetByCompanyId(companyId);
            if (config == null) return null;

            var dto = config.Adapt<ZatcaConfigurationDto>();

            // Get active device name
            if (config.ActiveDeviceId.HasValue)
            {
                var device = _deviceRepository.GetById(config.ActiveDeviceId.Value);
                dto.ActiveDeviceName = device?.DeviceName;
            }

            return dto;
        }

        public async Task<ZatcaConfigurationDto> GetConfigurationAsync(long? companyId = null)
        {
            var config = await _configRepository.GetByCompanyIdAsync(companyId);
            if (config == null) return null;

            var dto = config.Adapt<ZatcaConfigurationDto>();

            if (config.ActiveDeviceId.HasValue)
            {
                var device = await _deviceRepository.GetByIdAsync(config.ActiveDeviceId.Value);
                dto.ActiveDeviceName = device?.DeviceName;
            }

            return dto;
        }

        public ZatcaConfiguration SaveConfiguration(SaveZatcaConfigurationDto dto, string userName)
        {
            var existing = _configRepository.GetByCompanyId(null); // TODO: Get companyId from context

            if (existing == null)
            {
                // Create new configuration
                var config = new ZatcaConfiguration
                {
                    Id = Guid.NewGuid(),
                    SellerVatNumber = dto.SellerVatNumber,
                    SellerNameAr = dto.SellerNameAr,
                    SellerNameEn = dto.SellerNameEn,
                    SellerStreet = dto.SellerStreet,
                    SellerBuildingNumber = dto.SellerBuildingNumber,
                    SellerPlotIdentification = dto.SellerPlotIdentification,
                    SellerCitySubdivision = dto.SellerCitySubdivision,
                    SellerCity = dto.SellerCity,
                    SellerPostalCode = dto.SellerPostalCode,
                    SellerCountryCode = dto.SellerCountryCode ?? "SA",
                    SellerCountrySubentity = dto.SellerCountrySubentity,
                    SellerAdditionalStreet = dto.SellerAdditionalStreet,
                    Environment = dto.Environment,
                    ApiTimeout = dto.ApiTimeout,
                    MaxRetryAttempts = dto.MaxRetryAttempts,
                    RetryDelayMs = dto.RetryDelayMs,
                    DefaultPaymentMeansCode = dto.DefaultPaymentMeansCode,
                    AutoSubmitStandard = dto.AutoSubmitStandard,
                    AutoSubmitSimplified = dto.AutoSubmitSimplified,
                    QrForStandard = dto.QrForStandard,
                    QrForSimplified = dto.QrForSimplified,
                    DefaultCurrency = dto.DefaultCurrency ?? "SAR",
                    ActiveDeviceId = dto.ActiveDeviceId,
                    IsEnabled = dto.IsEnabled,
                    IsConfigured = true,
                    Create_by = userName,
                    Create_time = DateTime.Now
                };

                _configRepository.InsertConfiguration(config);
                return config;
            }
            else
            {
                // Update existing configuration
                existing.SellerVatNumber = dto.SellerVatNumber;
                existing.SellerNameAr = dto.SellerNameAr;
                existing.SellerNameEn = dto.SellerNameEn;
                existing.SellerStreet = dto.SellerStreet;
                existing.SellerBuildingNumber = dto.SellerBuildingNumber;
                existing.SellerPlotIdentification = dto.SellerPlotIdentification;
                existing.SellerCitySubdivision = dto.SellerCitySubdivision;
                existing.SellerCity = dto.SellerCity;
                existing.SellerPostalCode = dto.SellerPostalCode;
                existing.SellerCountryCode = dto.SellerCountryCode ?? "SA";
                existing.SellerCountrySubentity = dto.SellerCountrySubentity;
                existing.SellerAdditionalStreet = dto.SellerAdditionalStreet;
                existing.Environment = dto.Environment;
                existing.ApiTimeout = dto.ApiTimeout;
                existing.MaxRetryAttempts = dto.MaxRetryAttempts;
                existing.RetryDelayMs = dto.RetryDelayMs;
                existing.DefaultPaymentMeansCode = dto.DefaultPaymentMeansCode;
                existing.AutoSubmitStandard = dto.AutoSubmitStandard;
                existing.AutoSubmitSimplified = dto.AutoSubmitSimplified;
                existing.QrForStandard = dto.QrForStandard;
                existing.QrForSimplified = dto.QrForSimplified;
                existing.DefaultCurrency = dto.DefaultCurrency ?? "SAR";
                existing.ActiveDeviceId = dto.ActiveDeviceId;
                existing.IsEnabled = dto.IsEnabled;
                existing.IsConfigured = true;
                existing.Update_by = userName;
                existing.Update_time = DateTime.Now;

                _configRepository.UpdateConfiguration(existing);
                return existing;
            }
        }

        public async Task<ZatcaConfiguration> SaveConfigurationAsync(SaveZatcaConfigurationDto dto, string userName)
        {
            // Async version - similar to sync
            return await Task.Run(() => SaveConfiguration(dto, userName));
        }

        public ValidateConfigurationResponseDto ValidateConfiguration(long? companyId = null)
        {
            var response = new ValidateConfigurationResponseDto
            {
                IsValid = true,
                MissingFields = Array.Empty<string>(),
                Warnings = Array.Empty<string>(),
                Errors = Array.Empty<string>()
            };

            var config = _configRepository.GetByCompanyId(companyId);
            var missingFields = new List<string>();
            var warnings = new List<string>();
            var errors = new List<string>();

            if (config == null)
            {
                response.IsValid = false;
                errors.Add("ZATCA configuration not found - لم يتم العثور على إعدادات زاتكا");
                response.Errors = errors.ToArray();
                return response;
            }

            // Check seller information
            if (string.IsNullOrEmpty(config.SellerVatNumber))
                missingFields.Add("Seller VAT Number - الرقم الضريبي للبائع");
            else if (config.SellerVatNumber.Length != 15)
                errors.Add("Seller VAT Number must be 15 digits - الرقم الضريبي يجب أن يكون 15 رقم");

            if (string.IsNullOrEmpty(config.SellerNameAr))
                missingFields.Add("Seller Name (Arabic) - اسم البائع بالعربية");

            if (string.IsNullOrEmpty(config.SellerStreet))
                missingFields.Add("Seller Street - اسم الشارع");

            if (string.IsNullOrEmpty(config.SellerBuildingNumber))
                missingFields.Add("Seller Building Number - رقم المبنى");

            if (string.IsNullOrEmpty(config.SellerCitySubdivision))
                missingFields.Add("Seller District - الحي");

            if (string.IsNullOrEmpty(config.SellerCity))
                missingFields.Add("Seller City - المدينة");

            if (string.IsNullOrEmpty(config.SellerPostalCode))
                missingFields.Add("Seller Postal Code - الرمز البريدي");
            else if (config.SellerPostalCode.Length != 5)
                warnings.Add("Postal Code should be 5 digits - الرمز البريدي يجب أن يكون 5 أرقام");

            response.SellerInfoComplete = missingFields.Count == 0;

            // Check active device
            response.HasActiveDevice = config.ActiveDeviceId.HasValue;
            if (!config.ActiveDeviceId.HasValue)
            {
                warnings.Add("No active device configured - لم يتم تكوين جهاز نشط");
            }
            else
            {
                var device = _deviceRepository.GetById(config.ActiveDeviceId.Value);
                if (device == null)
                {
                    errors.Add("Active device not found - الجهاز النشط غير موجود");
                    response.HasActiveDevice = false;
                }
                else
                {
                    // Check certificate
                    if (config.Environment == ZatcaEnvironment.Production)
                    {
                        response.HasValidCertificate = device.IsProductionReady &&
                            !string.IsNullOrEmpty(device.ProductionCertificate) &&
                            device.CertificateExpiryDate > DateTime.Now;

                        if (!device.IsProductionReady)
                            errors.Add("Device is not production ready - الجهاز غير جاهز للإنتاج");
                    }
                    else
                    {
                        response.HasValidCertificate = device.IsComplianceCompleted &&
                            !string.IsNullOrEmpty(device.ComplianceCertificate);

                        if (!device.IsComplianceCompleted)
                            warnings.Add("Device has not completed compliance - الجهاز لم يكمل الامتثال");
                    }

                    // Check certificate expiry
                    if (device.CertificateExpiryDate.HasValue)
                    {
                        var daysUntilExpiry = (device.CertificateExpiryDate.Value - DateTime.Now).Days;
                        if (daysUntilExpiry < 0)
                            errors.Add("Certificate has expired - انتهت صلاحية الشهادة");
                        else if (daysUntilExpiry < 30)
                            warnings.Add($"Certificate expires in {daysUntilExpiry} days - تنتهي صلاحية الشهادة خلال {daysUntilExpiry} يوم");
                    }
                }
            }

            response.IsValid = errors.Count == 0 && missingFields.Count == 0;
            response.MissingFields = missingFields.ToArray();
            response.Warnings = warnings.ToArray();
            response.Errors = errors.ToArray();

            return response;
        }

        public async Task<ValidateConfigurationResponseDto> ValidateConfigurationAsync(long? companyId = null)
        {
            return await Task.Run(() => ValidateConfiguration(companyId));
        }

        public int SetActiveDevice(long? companyId, Guid deviceId)
        {
            return _configRepository.UpdateActiveDevice(companyId, deviceId);
        }

        public int ChangeEnvironment(long? companyId, ZatcaEnvironment environment)
        {
            return _configRepository.UpdateEnvironment(companyId, environment);
        }

        public ZatcaApiTestResponseDto TestApiConnection(ZatcaApiTestRequestDto request)
        {
            // TODO: Implement actual API test
            return new ZatcaApiTestResponseDto
            {
                IsConnected = true,
                StatusCode = 200,
                ResponseTimeMs = 150,
                ApiVersion = "1.0",
                Message = "Connection successful - الاتصال ناجح"
            };
        }

        public async Task<ZatcaApiTestResponseDto> TestApiConnectionAsync(ZatcaApiTestRequestDto request)
        {
            return await Task.Run(() => TestApiConnection(request));
        }

        public bool IsZatcaEnabled(long? companyId = null)
        {
            var config = _configRepository.GetByCompanyId(companyId);
            return config?.IsEnabled ?? false;
        }

        public ZatcaConfiguration GetSellerInfo(long? companyId = null)
        {
            return _configRepository.GetByCompanyId(companyId);
        }
    }
}
