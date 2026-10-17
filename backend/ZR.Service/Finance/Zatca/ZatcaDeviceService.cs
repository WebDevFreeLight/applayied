using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using ZR.Model;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Repository.Finance.Zatca;
using ZR.Service.IService.Finance.Zatca;

namespace ZR.Service.Finance.Zatca
{
    /// <summary>
    /// ZATCA Device Service
    /// خدمة أجهزة زاتكا
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaDeviceService), ServiceLifetime = LifeTime.Transient)]
    public class ZatcaDeviceService : IZatcaDeviceService
    {
        private readonly IZatcaDeviceRepository _deviceRepository;

        public ZatcaDeviceService(IZatcaDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        #region CRUD Operations

        public PagedInfo<ZatcaDeviceListDto> GetList(ZatcaDeviceQueryDto query, PagerInfo pager)
        {
            var parm = new ZatcaDevice
            {
                DeviceSerialNumber = query.DeviceSerialNumber,
                DeviceName = query.DeviceName,
                OrganizationIdentifier = query.OrganizationIdentifier
            };

            var result = _deviceRepository.GetList(parm, pager);
            var dtoList = result.Result.Select(MapToListDto).ToList();

            return new PagedInfo<ZatcaDeviceListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        public ZatcaDeviceDetailDto GetById(Guid id)
        {
            var device = _deviceRepository.GetById(id);
            return device == null ? null : MapToDetailDto(device);
        }

        public async Task<ZatcaDeviceDetailDto> GetByIdAsync(Guid id)
        {
            var device = await _deviceRepository.GetByIdAsync(id);
            return device == null ? null : MapToDetailDto(device);
        }

        public ZatcaDevice Create(CreateZatcaDeviceDto dto, string userName)
        {
            // Validate serial number doesn't exist
            if (_deviceRepository.SerialNumberExists(dto.DeviceSerialNumber, null, null))
            {
                throw new AppException(ExceptionCode.FINANCE_ZATCA_DEVICE_SERIAL_DUPLICATE)
                    .WithParameter("SerialNumber", dto.DeviceSerialNumber);
            }

            var device = new ZatcaDevice
            {
                Id = Guid.NewGuid(),
                DeviceSerialNumber = dto.DeviceSerialNumber,
                DeviceName = dto.DeviceName,
                OrganizationIdentifier = dto.OrganizationIdentifier,
                OrganizationUnitName = dto.OrganizationUnitName,
                OrganizationNameEn = dto.OrganizationNameEn,
                OrganizationNameAr = dto.OrganizationNameAr,
                CountryCode = dto.CountryCode ?? "SA",
                InvoiceType = dto.InvoiceType ?? "1100",
                LocationAddress = dto.LocationAddress,
                IndustryBusinessCategory = dto.IndustryBusinessCategory,
                Environment = dto.Environment,
                IsActive = true,
                IsComplianceCompleted = false,
                IsProductionReady = false,
                Create_by = userName,
                Create_time = DateTime.Now
            };

            _deviceRepository.InsertDevice(device);
            return device;
        }

        public async Task<ZatcaDevice> CreateAsync(CreateZatcaDeviceDto dto, string userName)
        {
            if (await _deviceRepository.SerialNumberExistsAsync(dto.DeviceSerialNumber, null, null))
            {
                throw new AppException(ExceptionCode.FINANCE_ZATCA_DEVICE_SERIAL_DUPLICATE)
                    .WithParameter("SerialNumber", dto.DeviceSerialNumber);
            }

            var device = new ZatcaDevice
            {
                Id = Guid.NewGuid(),
                DeviceSerialNumber = dto.DeviceSerialNumber,
                DeviceName = dto.DeviceName,
                OrganizationIdentifier = dto.OrganizationIdentifier,
                OrganizationUnitName = dto.OrganizationUnitName,
                OrganizationNameEn = dto.OrganizationNameEn,
                OrganizationNameAr = dto.OrganizationNameAr,
                CountryCode = dto.CountryCode ?? "SA",
                InvoiceType = dto.InvoiceType ?? "1100",
                LocationAddress = dto.LocationAddress,
                IndustryBusinessCategory = dto.IndustryBusinessCategory,
                Environment = dto.Environment,
                IsActive = true,
                Create_by = userName,
                Create_time = DateTime.Now
            };

            await _deviceRepository.InsertDeviceAsync(device);
            return device;
        }

        public int Update(UpdateZatcaDeviceDto dto, string userName)
        {
            var existing = _deviceRepository.GetById(dto.Id);
            if (existing == null)
                throw new AppException(ExceptionCode.FINANCE_ZATCA_DEVICE_NOT_FOUND)
                    .WithParameter("DeviceId", dto.Id);

            // Check serial number uniqueness
            if (_deviceRepository.SerialNumberExists(dto.DeviceSerialNumber, null, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_ZATCA_DEVICE_SERIAL_DUPLICATE)
                    .WithParameter("SerialNumber", dto.DeviceSerialNumber);
            }

            existing.DeviceSerialNumber = dto.DeviceSerialNumber;
            existing.DeviceName = dto.DeviceName;
            existing.OrganizationIdentifier = dto.OrganizationIdentifier;
            existing.OrganizationUnitName = dto.OrganizationUnitName;
            existing.OrganizationNameEn = dto.OrganizationNameEn;
            existing.OrganizationNameAr = dto.OrganizationNameAr;
            existing.CountryCode = dto.CountryCode ?? "SA";
            existing.InvoiceType = dto.InvoiceType ?? "1100";
            existing.LocationAddress = dto.LocationAddress;
            existing.IndustryBusinessCategory = dto.IndustryBusinessCategory;
            existing.Environment = dto.Environment;
            existing.IsActive = dto.IsActive;
            existing.Update_by = userName;
            existing.Update_time = DateTime.Now;

            return _deviceRepository.UpdateDevice(existing);
        }

        public int Delete(Guid[] ids)
        {
            return _deviceRepository.DeleteDevices(ids);
        }

        #endregion

        #region Query Operations

        public List<ZatcaDeviceListDto> GetActiveDevices(long? companyId = null)
        {
            var devices = _deviceRepository.GetActiveDevices(companyId);
            return devices.Select(MapToListDto).ToList();
        }

        public List<ZatcaDeviceListDto> GetProductionReadyDevices(long? companyId = null)
        {
            var devices = _deviceRepository.GetProductionReadyDevices(companyId);
            return devices.Select(MapToListDto).ToList();
        }

        public List<ZatcaDeviceListDto> GetDevicesWithExpiringCertificates(int daysBeforeExpiry = 30)
        {
            var devices = _deviceRepository.GetDevicesWithExpiringCertificates(daysBeforeExpiry);
            return devices.Select(MapToListDto).ToList();
        }

        #endregion

        #region CSR Operations

        public GenerateCsrResponseDto GenerateCsr(Guid deviceId, string userName)
        {
            var device = _deviceRepository.GetById(deviceId);
            if (device == null)
            {
                return new GenerateCsrResponseDto
                {
                    DeviceId = deviceId,
                    Success = false,
                    Message = "Device not found - الجهاز غير موجود"
                };
            }

            try
            {
                // Generate EC key pair for ZATCA (secp256k1)
                // Note: In production, use proper EC key generation
                // This is a placeholder for the CSR generation logic
                var csr = GenerateCsrForDevice(device);
                var privateKey = "PLACEHOLDER_PRIVATE_KEY"; // In production, store securely

                _deviceRepository.UpdateCsr(deviceId, csr, privateKey);

                return new GenerateCsrResponseDto
                {
                    DeviceId = deviceId,
                    Csr = csr,
                    Success = true,
                    Message = "CSR generated successfully - تم إنشاء CSR بنجاح"
                };
            }
            catch (Exception ex)
            {
                return new GenerateCsrResponseDto
                {
                    DeviceId = deviceId,
                    Success = false,
                    Message = $"Failed to generate CSR: {ex.Message}"
                };
            }
        }

        public async Task<GenerateCsrResponseDto> GenerateCsrAsync(Guid deviceId, string userName)
        {
            return await Task.Run(() => GenerateCsr(deviceId, userName));
        }

        private string GenerateCsrForDevice(ZatcaDevice device)
        {
            // ZATCA CSR format requires specific fields
            // This is a simplified placeholder - actual implementation needs proper X.509 CSR generation
            var csrTemplate = @"-----BEGIN CERTIFICATE REQUEST-----
[CSR_CONTENT]
-----END CERTIFICATE REQUEST-----";

            // CSR would include:
            // CN = Device Serial Number
            // OU = Organization Unit Name (10 digits)
            // O = Organization Name
            // C = Country Code (SA)
            // Custom OIDs for ZATCA:
            // - 2.5.4.97 = VAT Registration Number (15 digits)
            // - [Invoice Type binary flags]

            return csrTemplate.Replace("[CSR_CONTENT]",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"Device:{device.DeviceSerialNumber}|VAT:{device.OrganizationIdentifier}")));
        }

        #endregion

        #region Onboarding Operations

        public OnboardingResponseDto RequestComplianceCsid(OnboardingRequestDto request, string userName)
        {
            var device = _deviceRepository.GetById(request.DeviceId);
            if (device == null)
            {
                return new OnboardingResponseDto
                {
                    DeviceId = request.DeviceId,
                    Success = false,
                    Message = "Device not found - الجهاز غير موجود"
                };
            }

            if (string.IsNullOrEmpty(device.Csr))
            {
                return new OnboardingResponseDto
                {
                    DeviceId = request.DeviceId,
                    Success = false,
                    Message = "CSR not generated. Please generate CSR first - لم يتم إنشاء CSR. يرجى إنشاء CSR أولاً"
                };
            }

            try
            {
                // TODO: Call ZATCA Compliance API
                // POST to /compliance endpoint with CSR and OTP
                // For now, return placeholder response

                var complianceCsid = $"COMPLIANCE-{Guid.NewGuid():N}".ToUpper();
                var secret = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                var certificate = "PLACEHOLDER_CERTIFICATE";
                var expiryDate = DateTime.Now.AddYears(1);

                _deviceRepository.UpdateComplianceCertificate(
                    request.DeviceId,
                    complianceCsid,
                    secret,
                    certificate,
                    expiryDate);

                return new OnboardingResponseDto
                {
                    DeviceId = request.DeviceId,
                    ComplianceCsid = complianceCsid,
                    CertificateExpiryDate = expiryDate,
                    Success = true,
                    Message = "Compliance CSID obtained successfully - تم الحصول على معرف الامتثال بنجاح",
                    Warnings = Array.Empty<string>(),
                    Errors = Array.Empty<string>()
                };
            }
            catch (Exception ex)
            {
                return new OnboardingResponseDto
                {
                    DeviceId = request.DeviceId,
                    Success = false,
                    Message = $"Failed to obtain compliance CSID: {ex.Message}",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<OnboardingResponseDto> RequestComplianceCsidAsync(OnboardingRequestDto request, string userName)
        {
            return await Task.Run(() => RequestComplianceCsid(request, userName));
        }

        public ProductionCsidResponseDto RequestProductionCsid(ProductionCsidRequestDto request, string userName)
        {
            var device = _deviceRepository.GetById(request.DeviceId);
            if (device == null)
            {
                return new ProductionCsidResponseDto
                {
                    DeviceId = request.DeviceId,
                    Success = false,
                    Message = "Device not found - الجهاز غير موجود"
                };
            }

            if (!device.IsComplianceCompleted)
            {
                return new ProductionCsidResponseDto
                {
                    DeviceId = request.DeviceId,
                    Success = false,
                    Message = "Compliance not completed. Please complete compliance first - لم يكتمل الامتثال. يرجى إكمال الامتثال أولاً"
                };
            }

            try
            {
                // TODO: Call ZATCA Production API
                // POST to /production/csids endpoint
                // For now, return placeholder response

                var productionCsid = $"PRODUCTION-{Guid.NewGuid():N}".ToUpper();
                var secret = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                var certificate = "PLACEHOLDER_PRODUCTION_CERTIFICATE";
                var expiryDate = DateTime.Now.AddYears(1);

                _deviceRepository.UpdateProductionCertificate(
                    request.DeviceId,
                    productionCsid,
                    secret,
                    certificate,
                    expiryDate);

                return new ProductionCsidResponseDto
                {
                    DeviceId = request.DeviceId,
                    ProductionCsid = productionCsid,
                    CertificateExpiryDate = expiryDate,
                    Success = true,
                    Message = "Production CSID obtained successfully - تم الحصول على معرف الإنتاج بنجاح",
                    Warnings = Array.Empty<string>(),
                    Errors = Array.Empty<string>()
                };
            }
            catch (Exception ex)
            {
                return new ProductionCsidResponseDto
                {
                    DeviceId = request.DeviceId,
                    Success = false,
                    Message = $"Failed to obtain production CSID: {ex.Message}",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ProductionCsidResponseDto> RequestProductionCsidAsync(ProductionCsidRequestDto request, string userName)
        {
            return await Task.Run(() => RequestProductionCsid(request, userName));
        }

        #endregion

        #region Validation

        public bool SerialNumberExists(string serialNumber, long? companyId, Guid? excludeId = null)
        {
            return _deviceRepository.SerialNumberExists(serialNumber, companyId, excludeId);
        }

        public (bool IsValid, string[] Errors) ValidateDeviceForSubmission(Guid deviceId)
        {
            var errors = new List<string>();
            var device = _deviceRepository.GetById(deviceId);

            if (device == null)
            {
                errors.Add("Device not found - الجهاز غير موجود");
                return (false, errors.ToArray());
            }

            if (!device.IsActive)
                errors.Add("Device is not active - الجهاز غير نشط");

            if (device.Environment == ZatcaEnvironment.Production)
            {
                if (!device.IsProductionReady)
                    errors.Add("Device is not production ready - الجهاز غير جاهز للإنتاج");

                if (string.IsNullOrEmpty(device.ProductionCertificate))
                    errors.Add("Production certificate not found - لم يتم العثور على شهادة الإنتاج");

                if (string.IsNullOrEmpty(device.ProductionCsid))
                    errors.Add("Production CSID not found - لم يتم العثور على معرف الإنتاج");
            }
            else
            {
                if (!device.IsComplianceCompleted)
                    errors.Add("Compliance not completed - لم يكتمل الامتثال");

                if (string.IsNullOrEmpty(device.ComplianceCertificate))
                    errors.Add("Compliance certificate not found - لم يتم العثور على شهادة الامتثال");

                if (string.IsNullOrEmpty(device.ComplianceCsid))
                    errors.Add("Compliance CSID not found - لم يتم العثور على معرف الامتثال");
            }

            if (device.CertificateExpiryDate.HasValue && device.CertificateExpiryDate < DateTime.Now)
                errors.Add("Certificate has expired - انتهت صلاحية الشهادة");

            return (errors.Count == 0, errors.ToArray());
        }

        #endregion

        #region Mapping

        private ZatcaDeviceListDto MapToListDto(ZatcaDevice device)
        {
            return new ZatcaDeviceListDto
            {
                Id = device.Id,
                DeviceSerialNumber = device.DeviceSerialNumber,
                DeviceName = device.DeviceName,
                OrganizationIdentifier = device.OrganizationIdentifier,
                OrganizationUnitName = device.OrganizationUnitName,
                OrganizationNameEn = device.OrganizationNameEn,
                OrganizationNameAr = device.OrganizationNameAr,
                InvoiceType = device.InvoiceType,
                Environment = device.Environment,
                IsActive = device.IsActive,
                IsComplianceCompleted = device.IsComplianceCompleted,
                IsProductionReady = device.IsProductionReady,
                CertificateExpiryDate = device.CertificateExpiryDate,
                LastUsedAt = device.LastUsedAt,
                Create_time = device.Create_time
            };
        }

        private ZatcaDeviceDetailDto MapToDetailDto(ZatcaDevice device)
        {
            return new ZatcaDeviceDetailDto
            {
                Id = device.Id,
                DeviceSerialNumber = device.DeviceSerialNumber,
                DeviceName = device.DeviceName,
                OrganizationIdentifier = device.OrganizationIdentifier,
                OrganizationUnitName = device.OrganizationUnitName,
                OrganizationNameEn = device.OrganizationNameEn,
                OrganizationNameAr = device.OrganizationNameAr,
                InvoiceType = device.InvoiceType,
                Environment = device.Environment,
                IsActive = device.IsActive,
                IsComplianceCompleted = device.IsComplianceCompleted,
                IsProductionReady = device.IsProductionReady,
                CertificateExpiryDate = device.CertificateExpiryDate,
                LastUsedAt = device.LastUsedAt,
                Create_time = device.Create_time,
                CountryCode = device.CountryCode,
                LocationAddress = device.LocationAddress,
                IndustryBusinessCategory = device.IndustryBusinessCategory,
                HasCsr = !string.IsNullOrEmpty(device.Csr),
                HasComplianceCertificate = !string.IsNullOrEmpty(device.ComplianceCertificate),
                HasProductionCertificate = !string.IsNullOrEmpty(device.ProductionCertificate),
                ComplianceCompletedAt = device.ComplianceCompletedAt,
                ProductionActivatedAt = device.ProductionActivatedAt
            };
        }

        #endregion
    }
}
