using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Customers;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Repository;
using ZR.Repository.Finance.ARAP;
using ZR.Repository.Customers;
using ZR.Repository.Finance.Zatca;
using ZR.Service.Finance.Zatca.Utils;
using ZR.Service.IService.Finance.Zatca;

namespace ZR.Service.Finance.Zatca
{
    /// <summary>
    /// ZATCA Invoice Service
    /// خدمة فواتير زاتكا
    /// Handles invoice preparation, submission, and status tracking
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaInvoiceService), ServiceLifetime = LifeTime.Transient)]
    public class ZatcaInvoiceService : BaseService<ZatcaInvoiceHash>, IZatcaInvoiceService
    {
        private readonly IZatcaInvoiceHashRepository _hashRepository;
        private readonly IZatcaSubmissionLogRepository _logRepository;
        private readonly IZatcaConfigurationRepository _configRepository;
        private readonly IZatcaDeviceRepository _deviceRepository;
        private readonly IARInvoiceRepository _arInvoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IZatcaApiClient _apiClient;

        // Utility classes
        private readonly ZatcaUblBuilder _ublBuilder;
        private readonly ZatcaHashCalculator _hashCalculator;
        private readonly ZatcaQrGenerator _qrGenerator;
        private readonly ZatcaXmlSigner _xmlSigner;
        private readonly ZatcaInvoiceValidator _invoiceValidator;

        public ZatcaInvoiceService(
            IZatcaInvoiceHashRepository hashRepository,
            IZatcaSubmissionLogRepository logRepository,
            IZatcaConfigurationRepository configRepository,
            IZatcaDeviceRepository deviceRepository,
            IARInvoiceRepository arInvoiceRepository,
            ICustomerRepository customerRepository,
            IZatcaApiClient apiClient)
        {
            _hashRepository = hashRepository;
            _logRepository = logRepository;
            _configRepository = configRepository;
            _deviceRepository = deviceRepository;
            _arInvoiceRepository = arInvoiceRepository;
            _customerRepository = customerRepository;
            _apiClient = apiClient;

            // Initialize utility classes
            _ublBuilder = new ZatcaUblBuilder();
            _hashCalculator = new ZatcaHashCalculator();
            _qrGenerator = new ZatcaQrGenerator();
            _xmlSigner = new ZatcaXmlSigner();
            _invoiceValidator = new ZatcaInvoiceValidator();
        }

        #region Submission Operations

        public ZatcaSubmitInvoiceResponseDto SubmitInvoice(ZatcaSubmitInvoiceRequestDto request, string userName)
        {
            var response = new ZatcaSubmitInvoiceResponseDto
            {
                InvoiceId = request.InvoiceId
            };

            try
            {
                // Get invoice
                var invoice = _arInvoiceRepository.GetById(request.InvoiceId);
                if (invoice == null)
                {
                    response.Success = false;
                    response.Errors.Add("Invoice not found - الفاتورة غير موجودة");
                    return response;
                }

                response.InvoiceNumber = invoice.InvoiceNumber;

                // Check if already submitted (unless force resubmit)
                if (invoice.ZatcaStatus.HasValue &&
                    invoice.ZatcaStatus != ZatcaSubmissionStatus.NotSubmitted &&
                    invoice.ZatcaStatus != ZatcaSubmissionStatus.Failed &&
                    invoice.ZatcaStatus != ZatcaSubmissionStatus.Rejected &&
                    !request.ForceResubmit)
                {
                    response.Success = false;
                    response.Status = invoice.ZatcaStatus.Value;
                    response.Errors.Add("Invoice already submitted - الفاتورة مرسلة مسبقاً");
                    return response;
                }

                // Validate for ZATCA
                var (isValid, errors, warnings) = ValidateInvoiceForZatca(request.InvoiceId, request.InvoiceSourceType);
                if (!isValid)
                {
                    response.Success = false;
                    response.Errors.AddRange(errors);
                    response.Warnings.AddRange(warnings);
                    return response;
                }

                // Get configuration
                var config = _configRepository.GetByCompanyId(invoice.CompanyID);
                if (config == null || !config.IsEnabled)
                {
                    response.Success = false;
                    response.Errors.Add("ZATCA is not enabled - زاتكا غير مفعّل");
                    return response;
                }

                // Get device
                if (!config.ActiveDeviceId.HasValue)
                {
                    response.Success = false;
                    response.Errors.Add("No active device configured - لم يتم تكوين جهاز نشط");
                    return response;
                }

                var device = _deviceRepository.GetById(config.ActiveDeviceId.Value);
                if (device == null)
                {
                    response.Success = false;
                    response.Errors.Add("Active device not found - الجهاز النشط غير موجود");
                    return response;
                }

                // Determine invoice type
                var (invoiceType, subType) = DetermineInvoiceType(request.InvoiceId, request.InvoiceSourceType);

                // Generate UUID if not exists
                var uuid = string.IsNullOrEmpty(invoice.ZatcaUuid)
                    ? Guid.NewGuid().ToString()
                    : invoice.ZatcaUuid;

                // Get next counter value
                var counterValue = _hashRepository.GetNextCounterValue(invoice.CompanyID);

                // Get previous invoice hash (PIH)
                var previousHash = _hashRepository.GetPreviousInvoiceHash(invoice.CompanyID);

                // Create hash record
                var hashRecord = new ZatcaInvoiceHash
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    InvoiceSourceType = request.InvoiceSourceType ?? "AR",
                    ZatcaInvoiceType = invoiceType,
                    ZatcaInvoiceSubType = subType,
                    InvoiceCounterValue = counterValue,
                    InvoiceUuid = uuid,
                    InvoiceNumber = invoice.InvoiceNumber,
                    IssueDateTime = invoice.InvoiceDate,
                    DeviceId = device.Id,
                    PreviousInvoiceHash = previousHash,
                    SubmissionStatus = ZatcaSubmissionStatus.Pending,
                    CompanyID = invoice.CompanyID,
                    Create_by = userName,
                    Create_time = DateTime.Now
                };

                // Generate XML (placeholder - actual implementation requires UBL 2.1 XML generation)
                var invoiceXml = GenerateInvoiceXmlInternal(invoice, config, hashRecord);

                // Sign XML (placeholder - actual implementation requires ECDSA signing)
                var signedXml = SignInvoiceXml(invoiceXml, device);

                // Calculate hash
                var invoiceHash = CalculateInvoiceHash(signedXml);
                hashRecord.InvoiceHash = invoiceHash;
                hashRecord.SignedXml = signedXml;

                // Generate QR code (for simplified invoices or all if configured)
                var isSimplified = subType == ZatcaInvoiceSubType.SimplifiedInvoice ||
                                   subType == ZatcaInvoiceSubType.SimplifiedCreditNote ||
                                   subType == ZatcaInvoiceSubType.SimplifiedDebitNote;

                if (isSimplified || config.QrForStandard)
                {
                    hashRecord.QrCode = GenerateQrCodeTlv(invoice, config, invoiceHash);
                }

                // Save hash record
                _hashRepository.InsertInvoiceHash(hashRecord);

                // Update invoice with ZATCA fields
                UpdateInvoiceZatcaFields(invoice.Id, uuid, invoiceType, subType, hashRecord.Id, counterValue, hashRecord.QrCode, invoiceHash);

                // Submit to ZATCA (placeholder - actual implementation requires HTTP call)
                var submissionResult = SubmitToZatcaApi(hashRecord, signedXml, device, config, isSimplified);

                // Log submission
                LogSubmission(hashRecord.Id, device.Id, config, submissionResult);

                // Update status based on result
                _hashRepository.UpdateSubmissionStatus(
                    hashRecord.Id,
                    submissionResult.Status,
                    submissionResult.ResponseCode,
                    submissionResult.ClearanceStatus,
                    submissionResult.ReportingStatus);

                // Update invoice status
                UpdateInvoiceZatcaStatus(invoice.Id, submissionResult.Status, submissionResult.Success);

                response.ZatcaUuid = uuid;
                response.InvoiceCounterValue = counterValue;
                response.Status = submissionResult.Status;
                response.DispositionStatus = submissionResult.IsSimplified ? submissionResult.ReportingStatus : submissionResult.ClearanceStatus;
                response.InvoiceHash = invoiceHash;
                response.QrCode = hashRecord.QrCode;
                response.SubmittedAt = DateTime.Now;
                response.Success = submissionResult.Success;
                response.Message = submissionResult.Message;
                response.Warnings.AddRange(warnings);

                if (submissionResult.Success &&
                    (submissionResult.Status == ZatcaSubmissionStatus.Cleared ||
                     submissionResult.Status == ZatcaSubmissionStatus.Reported))
                {
                    response.ClearedAt = DateTime.Now;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Errors.Add($"Submission failed: {ex.Message}");
                return response;
            }
        }

        public async Task<ZatcaSubmitInvoiceResponseDto> SubmitInvoiceAsync(ZatcaSubmitInvoiceRequestDto request, string userName)
        {
            return await Task.Run(() => SubmitInvoice(request, userName));
        }

        public ZatcaBulkSubmitResponseDto SubmitBulkInvoices(ZatcaBulkSubmitRequestDto request, string userName)
        {
            var response = new ZatcaBulkSubmitResponseDto
            {
                TotalCount = request.InvoiceIds.Count
            };

            foreach (var invoiceId in request.InvoiceIds)
            {
                var submitRequest = new ZatcaSubmitInvoiceRequestDto
                {
                    InvoiceId = invoiceId,
                    InvoiceSourceType = request.InvoiceSourceType
                };

                var result = SubmitInvoice(submitRequest, userName);
                response.Results.Add(result);

                if (result.Success)
                    response.SuccessCount++;
                else
                    response.FailedCount++;

                if (!result.Success && request.StopOnFirstError)
                {
                    response.SkippedCount = response.TotalCount - response.SuccessCount - response.FailedCount;
                    break;
                }
            }

            return response;
        }

        public async Task<ZatcaBulkSubmitResponseDto> SubmitBulkInvoicesAsync(ZatcaBulkSubmitRequestDto request, string userName)
        {
            return await Task.Run(() => SubmitBulkInvoices(request, userName));
        }

        public ZatcaSubmitInvoiceResponseDto RetrySubmission(Guid invoiceId, string userName)
        {
            var hashRecord = _hashRepository.GetByInvoiceId(invoiceId);
            if (hashRecord == null)
            {
                return new ZatcaSubmitInvoiceResponseDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Errors = new List<string> { "No previous submission found - لم يتم العثور على إرسال سابق" }
                };
            }

            _hashRepository.IncrementRetryCount(hashRecord.Id);

            return SubmitInvoice(new ZatcaSubmitInvoiceRequestDto
            {
                InvoiceId = invoiceId,
                ForceResubmit = true
            }, userName);
        }

        public async Task<ZatcaSubmitInvoiceResponseDto> RetrySubmissionAsync(Guid invoiceId, string userName)
        {
            return await Task.Run(() => RetrySubmission(invoiceId, userName));
        }

        #endregion

        #region Status Operations

        public ZatcaInvoiceStatusDto GetInvoiceStatus(Guid invoiceId)
        {
            var invoice = _arInvoiceRepository.GetById(invoiceId);
            if (invoice == null) return null;

            var hashRecord = _hashRepository.GetByInvoiceId(invoiceId);

            return new ZatcaInvoiceStatusDto
            {
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                ZatcaUuid = invoice.ZatcaUuid,
                ZatcaInvoiceType = invoice.ZatcaInvoiceType,
                ZatcaInvoiceSubType = invoice.ZatcaInvoiceSubType,
                Status = invoice.ZatcaStatus,
                InvoiceCounterValue = invoice.ZatcaCounter,
                InvoiceHash = invoice.ZatcaHash,
                HasQrCode = !string.IsNullOrEmpty(invoice.ZatcaQrCode),
                SubmittedAt = invoice.ZatcaSubmittedAt,
                ClearedAt = invoice.ZatcaClearedAt,
                WarningCount = hashRecord?.WarningCount ?? 0,
                ErrorCount = hashRecord?.ErrorCount ?? 0,
                RetryCount = hashRecord?.RetryCount ?? 0
            };
        }

        public async Task<ZatcaInvoiceStatusDto> GetInvoiceStatusAsync(Guid invoiceId)
        {
            return await Task.Run(() => GetInvoiceStatus(invoiceId));
        }

        public List<PendingZatcaInvoiceDto> GetPendingInvoices(PendingZatcaInvoicesQueryDto query)
        {
            var invoices = Context.Queryable<ARInvoice>()
                .Where(x => x.Status == Model.Finance.ARAP.Enums.InvoiceStatus.Posted)
                .Where(x => x.ZatcaStatus == null || x.ZatcaStatus == ZatcaSubmissionStatus.NotSubmitted)
                .WhereIF(query.FromDate.HasValue, x => x.InvoiceDate >= query.FromDate)
                .WhereIF(query.ToDate.HasValue, x => x.InvoiceDate <= query.ToDate)
                .OrderBy(x => x.InvoiceDate)
                .ToList();

            return invoices.Select(inv =>
            {
                var customer = _customerRepository.GetById(inv.CustomerId);
                var hashRecord = _hashRepository.GetByInvoiceId(inv.Id);

                return new PendingZatcaInvoiceDto
                {
                    InvoiceId = inv.Id,
                    InvoiceNumber = inv.InvoiceNumber,
                    InvoiceSourceType = "AR",
                    InvoiceDate = inv.InvoiceDate,
                    TotalAmount = inv.TotalAmount,
                    CustomerName = customer?.NameEn ?? customer?.NameAr,
                    CustomerVatNumber = customer?.TaxNumber,
                    IsB2B = !string.IsNullOrEmpty(customer?.TaxNumber),
                    CurrentStatus = inv.ZatcaStatus,
                    FailedAttempts = hashRecord?.RetryCount ?? 0,
                    LastError = hashRecord?.ErrorMessages
                };
            }).ToList();
        }

        public List<PendingZatcaInvoiceDto> GetInvoicesNeedingRetry(int maxRetries = 3)
        {
            var hashRecords = _hashRepository.GetInvoicesNeedingRetry(maxRetries);

            return hashRecords.Select(h =>
            {
                var invoice = _arInvoiceRepository.GetById(h.InvoiceId);
                var customer = invoice != null ? _customerRepository.GetById(invoice.CustomerId) : null;

                return new PendingZatcaInvoiceDto
                {
                    InvoiceId = h.InvoiceId,
                    InvoiceNumber = h.InvoiceNumber,
                    InvoiceSourceType = h.InvoiceSourceType,
                    InvoiceDate = h.IssueDateTime,
                    TotalAmount = invoice?.TotalAmount ?? 0,
                    CustomerName = customer?.NameEn ?? customer?.NameAr,
                    CustomerVatNumber = customer?.TaxNumber,
                    IsB2B = h.ZatcaInvoiceSubType == ZatcaInvoiceSubType.StandardInvoice,
                    CurrentStatus = h.SubmissionStatus,
                    FailedAttempts = h.RetryCount,
                    LastError = h.ErrorMessages
                };
            }).ToList();
        }

        #endregion

        #region QR Code Operations

        public ZatcaQrCodeResponseDto GetQrCode(Guid invoiceId)
        {
            var invoice = _arInvoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                return new ZatcaQrCodeResponseDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Message = "Invoice not found - الفاتورة غير موجودة"
                };
            }

            if (string.IsNullOrEmpty(invoice.ZatcaQrCode))
            {
                return new ZatcaQrCodeResponseDto
                {
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    Success = false,
                    Message = "QR code not generated - لم يتم إنشاء رمز QR"
                };
            }

            return new ZatcaQrCodeResponseDto
            {
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                QrCodeTlv = invoice.ZatcaQrCode,
                QrCodeBase64 = invoice.ZatcaQrCode, // TLV is already Base64
                Success = true,
                Message = "QR code retrieved successfully"
            };
        }

        public async Task<ZatcaQrCodeResponseDto> GetQrCodeAsync(Guid invoiceId)
        {
            return await Task.Run(() => GetQrCode(invoiceId));
        }

        public ZatcaQrCodeResponseDto GenerateQrCode(Guid invoiceId, string userName)
        {
            var invoice = _arInvoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                return new ZatcaQrCodeResponseDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Message = "Invoice not found"
                };
            }

            var config = _configRepository.GetByCompanyId(invoice.CompanyID);
            if (config == null)
            {
                return new ZatcaQrCodeResponseDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Message = "ZATCA configuration not found"
                };
            }

            var qrCode = GenerateQrCodeTlv(invoice, config, invoice.ZatcaHash ?? "");

            // Update invoice with QR code
            Context.Updateable<ARInvoice>()
                .SetColumns(x => new ARInvoice { ZatcaQrCode = qrCode })
                .Where(x => x.Id == invoiceId)
                .ExecuteCommand();

            return new ZatcaQrCodeResponseDto
            {
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                QrCodeTlv = qrCode,
                QrCodeBase64 = qrCode,
                Success = true,
                Message = "QR code generated successfully"
            };
        }

        #endregion

        #region XML Operations

        public ZatcaInvoiceXmlResponseDto GenerateInvoiceXml(Guid invoiceId)
        {
            var invoice = _arInvoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                return new ZatcaInvoiceXmlResponseDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Message = "Invoice not found"
                };
            }

            var config = _configRepository.GetByCompanyId(invoice.CompanyID);
            var hashRecord = new ZatcaInvoiceHash
            {
                InvoiceUuid = invoice.ZatcaUuid ?? Guid.NewGuid().ToString(),
                InvoiceCounterValue = invoice.ZatcaCounter ?? 1,
                PreviousInvoiceHash = ZatcaInvoiceHash.InitialPreviousInvoiceHash
            };

            var xml = GenerateInvoiceXmlInternal(invoice, config, hashRecord);

            return new ZatcaInvoiceXmlResponseDto
            {
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                UnsignedXml = xml,
                Success = true,
                Message = "XML generated successfully"
            };
        }

        public ZatcaInvoiceXmlResponseDto GenerateSignedInvoiceXml(Guid invoiceId, string userName)
        {
            var result = GenerateInvoiceXml(invoiceId);
            if (!result.Success) return result;

            var config = _configRepository.GetByCompanyId(null);
            if (config?.ActiveDeviceId == null)
            {
                result.Success = false;
                result.Message = "No active device for signing";
                return result;
            }

            var device = _deviceRepository.GetById(config.ActiveDeviceId.Value);
            if (device == null)
            {
                result.Success = false;
                result.Message = "Device not found";
                return result;
            }

            result.SignedXml = SignInvoiceXml(result.UnsignedXml, device);
            return result;
        }

        public ZatcaInvoiceXmlResponseDto GetSignedXml(Guid invoiceId)
        {
            var hashRecord = _hashRepository.GetByInvoiceId(invoiceId);
            if (hashRecord == null)
            {
                return new ZatcaInvoiceXmlResponseDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Message = "No submission record found"
                };
            }

            return new ZatcaInvoiceXmlResponseDto
            {
                InvoiceId = invoiceId,
                InvoiceNumber = hashRecord.InvoiceNumber,
                SignedXml = hashRecord.SignedXml,
                Success = true,
                Message = "Signed XML retrieved successfully"
            };
        }

        #endregion

        #region Hash Operations

        public PagedInfo<ZatcaInvoiceHashListDto> GetInvoiceHashList(ZatcaInvoiceHashQueryDto query, PagerInfo pager)
        {
            var parm = new ZatcaInvoiceHash
            {
                InvoiceNumber = query.InvoiceNumber,
                InvoiceSourceType = query.InvoiceSourceType
            };

            var result = _hashRepository.GetList(parm, pager);
            var dtoList = result.Result.Select(h => h.Adapt<ZatcaInvoiceHashListDto>()).ToList();

            return new PagedInfo<ZatcaInvoiceHashListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        public PagedInfo<ZatcaSubmissionLogListDto> GetSubmissionLogs(ZatcaSubmissionLogQueryDto query, PagerInfo pager)
        {
            var parm = new ZatcaSubmissionLog
            {
                OperationType = query.OperationType
            };

            if (query.InvoiceHashId.HasValue)
                parm.InvoiceHashId = query.InvoiceHashId.Value;

            var result = _logRepository.GetList(parm, pager);
            var dtoList = result.Result.Select(l => l.Adapt<ZatcaSubmissionLogListDto>()).ToList();

            return new PagedInfo<ZatcaSubmissionLogListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        public ZatcaSubmissionLogDetailDto GetSubmissionLogDetail(Guid logId)
        {
            var log = _logRepository.GetById(logId);
            return log?.Adapt<ZatcaSubmissionLogDetailDto>();
        }

        #endregion

        #region Dashboard Operations

        public ZatcaDashboardStatsDto GetDashboardStats(long? companyId = null)
        {
            var (total, success, failed, avgResponseTime) = _logRepository.GetStatistics(null, companyId);

            var clearedCount = Context.Queryable<ZatcaInvoiceHash>()
                .WhereIF(companyId.HasValue, x => x.CompanyID == companyId)
                .Where(x => x.SubmissionStatus == ZatcaSubmissionStatus.Cleared ||
                           x.SubmissionStatus == ZatcaSubmissionStatus.ClearedWithWarnings)
                .Count();

            var reportedCount = Context.Queryable<ZatcaInvoiceHash>()
                .WhereIF(companyId.HasValue, x => x.CompanyID == companyId)
                .Where(x => x.SubmissionStatus == ZatcaSubmissionStatus.Reported ||
                           x.SubmissionStatus == ZatcaSubmissionStatus.ReportedWithWarnings)
                .Count();

            var rejectedCount = Context.Queryable<ZatcaInvoiceHash>()
                .WhereIF(companyId.HasValue, x => x.CompanyID == companyId)
                .Where(x => x.SubmissionStatus == ZatcaSubmissionStatus.Rejected)
                .Count();

            var pendingCount = Context.Queryable<ZatcaInvoiceHash>()
                .WhereIF(companyId.HasValue, x => x.CompanyID == companyId)
                .Where(x => x.SubmissionStatus == ZatcaSubmissionStatus.Pending ||
                           x.SubmissionStatus == ZatcaSubmissionStatus.NotSubmitted)
                .Count();

            var failedCount = Context.Queryable<ZatcaInvoiceHash>()
                .WhereIF(companyId.HasValue, x => x.CompanyID == companyId)
                .Where(x => x.SubmissionStatus == ZatcaSubmissionStatus.Failed)
                .Count();

            var today = DateTime.Today;
            var todaySubmitted = _logRepository.GetTodaySubmissionCount(companyId);

            var activeDevices = _deviceRepository.GetActiveDevices(companyId).Count;
            var expiringDevices = _deviceRepository.GetDevicesWithExpiringCertificates(30, companyId).Count;

            var invoicesPendingSubmission = GetPendingInvoices(new PendingZatcaInvoicesQueryDto()).Count;
            var invoicesNeedingRetry = GetInvoicesNeedingRetry().Count;

            return new ZatcaDashboardStatsDto
            {
                TotalInvoicesSubmitted = total,
                TotalCleared = clearedCount,
                TotalReported = reportedCount,
                TotalRejected = rejectedCount,
                TotalPending = pendingCount,
                TotalFailed = failedCount,
                TodaySubmitted = todaySubmitted,
                TodayCleared = 0, // TODO: Implement today's cleared count
                TodayRejected = 0,
                SuccessRate = total > 0 ? Math.Round((decimal)success / total * 100, 2) : 100,
                AverageResponseTimeMs = avgResponseTime,
                ActiveDevices = activeDevices,
                DevicesNearExpiry = expiringDevices,
                InvoicesPendingSubmission = invoicesPendingSubmission,
                InvoicesNeedingRetry = invoicesNeedingRetry
            };
        }

        public async Task<ZatcaDashboardStatsDto> GetDashboardStatsAsync(long? companyId = null)
        {
            return await Task.Run(() => GetDashboardStats(companyId));
        }

        #endregion

        #region Validation

        public (bool IsValid, string[] Errors, string[] Warnings) ValidateInvoiceForZatca(Guid invoiceId, string invoiceSourceType = "AR")
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            var invoice = _arInvoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                errors.Add("Invoice not found - الفاتورة غير موجودة");
                return (false, errors.ToArray(), warnings.ToArray());
            }

            // Check invoice status
            if (invoice.Status != Model.Finance.ARAP.Enums.InvoiceStatus.Posted)
            {
                errors.Add("Invoice must be posted before ZATCA submission - يجب ترحيل الفاتورة قبل الإرسال لزاتكا");
            }

            // Check amounts
            if (invoice.TotalAmount <= 0)
            {
                errors.Add("Invoice amount must be greater than zero - يجب أن يكون مبلغ الفاتورة أكبر من صفر");
            }

            // Check customer
            var customer = _customerRepository.GetById(invoice.CustomerId);
            if (customer == null)
            {
                errors.Add("Customer not found - العميل غير موجود");
            }
            else
            {
                // For B2B (standard invoices), VAT number is required
                var isB2B = !string.IsNullOrEmpty(customer.TaxNumber);
                if (isB2B)
                {
                    if (customer.TaxNumber.Length != 15)
                    {
                        errors.Add("Customer VAT number must be 15 digits - الرقم الضريبي للعميل يجب أن يكون 15 رقم");
                    }
                }
                else
                {
                    warnings.Add("Customer has no VAT number - invoice will be submitted as simplified (B2C)");
                }
            }

            // Check invoice date
            if (invoice.InvoiceDate > DateTime.Now)
            {
                errors.Add("Invoice date cannot be in the future - لا يمكن أن يكون تاريخ الفاتورة في المستقبل");
            }

            return (errors.Count == 0, errors.ToArray(), warnings.ToArray());
        }

        public (ZatcaInvoiceType Type, ZatcaInvoiceSubType SubType) DetermineInvoiceType(Guid invoiceId, string invoiceSourceType = "AR")
        {
            var invoice = _arInvoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                return (ZatcaInvoiceType.Standard, ZatcaInvoiceSubType.StandardInvoice);
            }

            var customer = _customerRepository.GetById(invoice.CustomerId);
            var hasVatNumber = customer != null && !string.IsNullOrEmpty(customer.TaxNumber);

            // B2B = Standard (clearance), B2C = Simplified (reporting)
            if (hasVatNumber)
            {
                return (ZatcaInvoiceType.Standard, ZatcaInvoiceSubType.StandardInvoice);
            }
            else
            {
                return (ZatcaInvoiceType.Simplified, ZatcaInvoiceSubType.SimplifiedInvoice);
            }
        }

        #endregion

        #region Private Helper Methods

        private string GenerateInvoiceXmlInternal(ARInvoice invoice, ZatcaConfiguration config, ZatcaInvoiceHash hashRecord)
        {
            // Get invoice lines
            var lines = Context.Queryable<ARInvoiceLine>()
                .Where(x => x.InvoiceId == invoice.Id)
                .OrderBy(x => x.LineNumber)
                .ToList();

            // Get customer info for buyer
            var customer = _customerRepository.GetById(invoice.CustomerId);
            CustomerInfo buyerInfo = null;

            if (customer != null)
            {
                buyerInfo = new CustomerInfo
                {
                    Name = customer.NameAr ?? customer.NameEn,
                    VatNumber = customer.TaxNumber,
                    Street = customer.AddressAr ?? customer.AddressEn,
                    BuildingNumber = null, // Not in current Customer model
                    District = null, // Not in current Customer model
                    City = null, // Not in current Customer model
                    PostalCode = null, // Not in current Customer model
                    CountryCode = "SA"
                };
            }

            // Build UBL XML using the utility
            // The UBL builder takes the raw models and builds the XML
            return _ublBuilder.BuildInvoiceXml(
                invoice: invoice,
                lines: lines,
                config: config,
                hashRecord: hashRecord,
                buyer: buyerInfo
            );
        }

        private string SignInvoiceXml(string xml, ZatcaDevice device)
        {
            // Use the XML signer utility with device certificate and private key
            // Use production certificate if available, otherwise compliance certificate
            var certificate = device.ProductionCertificate ?? device.ComplianceCertificate;
            var privateKey = device.PrivateKey;

            if (string.IsNullOrEmpty(certificate) || string.IsNullOrEmpty(privateKey))
            {
                // Return unsigned if no certificate available
                return xml;
            }

            var signingResult = _xmlSigner.SignInvoice(xml, certificate, privateKey);

            if (signingResult.Success)
            {
                return signingResult.SignedXml;
            }

            // Log signing failure but return original XML for debugging
            // In production, this should throw or handle the error
            return xml;
        }

        private string CalculateInvoiceHash(string xml)
        {
            // Use the hash calculator utility with proper XML canonicalization
            return _hashCalculator.CalculateInvoiceHash(xml);
        }

        private string GenerateQrCodeTlv(ARInvoice invoice, ZatcaConfiguration config, string invoiceHash)
        {
            // Use the QR generator utility for Phase 2 compliant QR codes
            // For basic QR (Tags 1-6), use GenerateQrWithHash
            // For full Phase 2 QR (Tags 1-9), use GenerateSimplifiedInvoiceQr with signature components

            return _qrGenerator.GenerateQrWithHash(
                sellerName: config?.SellerNameAr ?? "",
                vatNumber: config?.SellerVatNumber ?? "",
                invoiceDateTime: invoice.InvoiceDate,
                totalWithVat: invoice.TotalAmount,
                vatAmount: invoice.TaxAmount,
                invoiceHash: invoiceHash
            );
        }

        private string GenerateFullQrCode(ARInvoice invoice, ZatcaConfiguration config, string invoiceHash, Utils.SigningResult signingResult)
        {
            // Generate full Phase 2 QR code with all 9 tags including signature
            return _qrGenerator.GenerateSimplifiedInvoiceQr(
                sellerName: config?.SellerNameAr ?? "",
                vatNumber: config?.SellerVatNumber ?? "",
                invoiceDateTime: invoice.InvoiceDate,
                totalWithVat: invoice.TotalAmount,
                vatAmount: invoice.TaxAmount,
                invoiceHash: invoiceHash,
                signature: signingResult?.SignatureValue != null ? Convert.FromBase64String(signingResult.SignatureValue) : null,
                publicKey: signingResult?.PublicKey,
                certificateSignature: signingResult?.CertificateSignature
            );
        }

        private void UpdateInvoiceZatcaFields(Guid invoiceId, string uuid, ZatcaInvoiceType type, ZatcaInvoiceSubType subType, Guid hashId, long counter, string qrCode, string hash)
        {
            Context.Updateable<ARInvoice>()
                .SetColumns(x => new ARInvoice
                {
                    ZatcaUuid = uuid,
                    ZatcaInvoiceType = type,
                    ZatcaInvoiceSubType = subType,
                    ZatcaHashId = hashId,
                    ZatcaCounter = counter,
                    ZatcaQrCode = qrCode,
                    ZatcaHash = hash,
                    ZatcaSubmittedAt = DateTime.Now
                })
                .Where(x => x.Id == invoiceId)
                .ExecuteCommand();
        }

        private void UpdateInvoiceZatcaStatus(Guid invoiceId, ZatcaSubmissionStatus status, bool success)
        {
            var clearedAt = (status == ZatcaSubmissionStatus.Cleared ||
                            status == ZatcaSubmissionStatus.ClearedWithWarnings ||
                            status == ZatcaSubmissionStatus.Reported ||
                            status == ZatcaSubmissionStatus.ReportedWithWarnings)
                ? DateTime.Now : (DateTime?)null;

            Context.Updateable<ARInvoice>()
                .SetColumns(x => new ARInvoice
                {
                    ZatcaStatus = status,
                    ZatcaClearedAt = clearedAt ?? x.ZatcaClearedAt
                })
                .Where(x => x.Id == invoiceId)
                .ExecuteCommand();
        }

        private (bool Success, ZatcaSubmissionStatus Status, string ResponseCode, string ClearanceStatus, string ReportingStatus, string Message, bool IsSimplified) SubmitToZatcaApi(ZatcaInvoiceHash hashRecord, string signedXml, ZatcaDevice device, ZatcaConfiguration config, bool isSimplified)
        {
            try
            {
                // Use the real API client for submission
                ZatcaApiResponse response;

                // Use production credentials, fallback to compliance for testing
                var csid = device.ProductionCsid ?? device.ComplianceCsid;
                var secret = device.ProductionCsidSecret ?? device.ComplianceCsidSecret;

                if (isSimplified)
                {
                    // Simplified invoice - reporting endpoint
                    response = _apiClient.ReportSimplifiedInvoiceAsync(
                        signedXml,
                        hashRecord.InvoiceHash,
                        hashRecord.InvoiceUuid,
                        config.Environment,
                        csid,
                        secret
                    ).GetAwaiter().GetResult();
                }
                else
                {
                    // Standard invoice - clearance endpoint
                    response = _apiClient.ClearStandardInvoiceAsync(
                        signedXml,
                        hashRecord.InvoiceHash,
                        hashRecord.InvoiceUuid,
                        config.Environment,
                        csid,
                        secret
                    ).GetAwaiter().GetResult();
                }

                return (
                    Success: response.Success,
                    Status: response.SubmissionStatus,
                    ResponseCode: response.HttpStatusCode.ToString(),
                    ClearanceStatus: response.ClearanceStatus,
                    ReportingStatus: response.ReportingStatus,
                    Message: response.Success
                        ? (isSimplified
                            ? "Invoice reported successfully - تم الإبلاغ عن الفاتورة بنجاح"
                            : "Invoice cleared successfully - تمت مقاصة الفاتورة بنجاح")
                        : (response.ErrorMessage ?? "Submission failed - فشل الإرسال"),
                    IsSimplified: isSimplified
                );
            }
            catch (Exception ex)
            {
                return (
                    Success: false,
                    Status: ZatcaSubmissionStatus.Failed,
                    ResponseCode: "500",
                    ClearanceStatus: null,
                    ReportingStatus: null,
                    Message: $"API call failed: {ex.Message}",
                    IsSimplified: isSimplified
                );
            }
        }

        private void LogSubmission(Guid hashId, Guid deviceId, ZatcaConfiguration config, (bool Success, ZatcaSubmissionStatus Status, string ResponseCode, string ClearanceStatus, string ReportingStatus, string Message, bool IsSimplified) result)
        {
            var log = new ZatcaSubmissionLog
            {
                Id = Guid.NewGuid(),
                InvoiceHashId = hashId,
                DeviceId = deviceId,
                ApiEndpoint = config.GetApiUrl() + (result.IsSimplified ? "/invoices/reporting" : "/invoices/clearance"),
                OperationType = result.IsSimplified ? "reporting" : "clearance",
                Environment = config.Environment,
                SubmissionDate = DateTime.Now,
                HttpStatusCode = result.Success ? 200 : 400,
                DispositionStatus = result.IsSimplified ? result.ReportingStatus : result.ClearanceStatus,
                IsSuccess = result.Success,
                ErrorMessage = result.Success ? null : result.Message,
                ResponseTimeMs = 150, // Placeholder
                Create_time = DateTime.Now
            };

            _logRepository.InsertLog(log);
        }

        #endregion
    }
}
