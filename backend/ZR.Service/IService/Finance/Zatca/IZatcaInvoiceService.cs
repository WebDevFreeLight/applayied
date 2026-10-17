using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Service.IService.Finance.Zatca
{
    /// <summary>
    /// ZATCA Invoice Service Interface
    /// واجهة خدمة فواتير زاتكا
    /// Handles invoice submission, reporting, and clearance
    /// </summary>
    public interface IZatcaInvoiceService
    {
        #region Submission Operations

        /// <summary>
        /// Submit invoice to ZATCA
        /// </summary>
        ZatcaSubmitInvoiceResponseDto SubmitInvoice(ZatcaSubmitInvoiceRequestDto request, string userName);

        /// <summary>
        /// Submit invoice to ZATCA (async)
        /// </summary>
        Task<ZatcaSubmitInvoiceResponseDto> SubmitInvoiceAsync(ZatcaSubmitInvoiceRequestDto request, string userName);

        /// <summary>
        /// Submit multiple invoices to ZATCA
        /// </summary>
        ZatcaBulkSubmitResponseDto SubmitBulkInvoices(ZatcaBulkSubmitRequestDto request, string userName);

        /// <summary>
        /// Submit multiple invoices to ZATCA (async)
        /// </summary>
        Task<ZatcaBulkSubmitResponseDto> SubmitBulkInvoicesAsync(ZatcaBulkSubmitRequestDto request, string userName);

        /// <summary>
        /// Retry failed invoice submission
        /// </summary>
        ZatcaSubmitInvoiceResponseDto RetrySubmission(Guid invoiceId, string userName);

        /// <summary>
        /// Retry failed invoice submission (async)
        /// </summary>
        Task<ZatcaSubmitInvoiceResponseDto> RetrySubmissionAsync(Guid invoiceId, string userName);

        #endregion

        #region Status Operations

        /// <summary>
        /// Get ZATCA status for invoice
        /// </summary>
        ZatcaInvoiceStatusDto GetInvoiceStatus(Guid invoiceId);

        /// <summary>
        /// Get ZATCA status for invoice (async)
        /// </summary>
        Task<ZatcaInvoiceStatusDto> GetInvoiceStatusAsync(Guid invoiceId);

        /// <summary>
        /// Get pending invoices for ZATCA submission
        /// </summary>
        List<PendingZatcaInvoiceDto> GetPendingInvoices(PendingZatcaInvoicesQueryDto query);

        /// <summary>
        /// Get invoices needing retry
        /// </summary>
        List<PendingZatcaInvoiceDto> GetInvoicesNeedingRetry(int maxRetries = 3);

        #endregion

        #region QR Code Operations

        /// <summary>
        /// Get QR code for invoice
        /// </summary>
        ZatcaQrCodeResponseDto GetQrCode(Guid invoiceId);

        /// <summary>
        /// Get QR code for invoice (async)
        /// </summary>
        Task<ZatcaQrCodeResponseDto> GetQrCodeAsync(Guid invoiceId);

        /// <summary>
        /// Generate QR code for invoice (without submission)
        /// </summary>
        ZatcaQrCodeResponseDto GenerateQrCode(Guid invoiceId, string userName);

        #endregion

        #region XML Operations

        /// <summary>
        /// Generate invoice XML (without signing)
        /// </summary>
        ZatcaInvoiceXmlResponseDto GenerateInvoiceXml(Guid invoiceId);

        /// <summary>
        /// Generate signed invoice XML
        /// </summary>
        ZatcaInvoiceXmlResponseDto GenerateSignedInvoiceXml(Guid invoiceId, string userName);

        /// <summary>
        /// Get signed XML for submitted invoice
        /// </summary>
        ZatcaInvoiceXmlResponseDto GetSignedXml(Guid invoiceId);

        #endregion

        #region Hash Operations

        /// <summary>
        /// Get invoice hash list
        /// </summary>
        PagedInfo<ZatcaInvoiceHashListDto> GetInvoiceHashList(ZatcaInvoiceHashQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get submission logs for invoice
        /// </summary>
        PagedInfo<ZatcaSubmissionLogListDto> GetSubmissionLogs(ZatcaSubmissionLogQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get submission log detail
        /// </summary>
        ZatcaSubmissionLogDetailDto GetSubmissionLogDetail(Guid logId);

        #endregion

        #region Dashboard Operations

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        ZatcaDashboardStatsDto GetDashboardStats(long? companyId = null);

        /// <summary>
        /// Get dashboard statistics (async)
        /// </summary>
        Task<ZatcaDashboardStatsDto> GetDashboardStatsAsync(long? companyId = null);

        #endregion

        #region Validation

        /// <summary>
        /// Validate invoice for ZATCA submission
        /// </summary>
        (bool IsValid, string[] Errors, string[] Warnings) ValidateInvoiceForZatca(Guid invoiceId, string invoiceSourceType = "AR");

        /// <summary>
        /// Determine invoice type (B2B/B2C)
        /// </summary>
        (ZatcaInvoiceType Type, ZatcaInvoiceSubType SubType) DetermineInvoiceType(Guid invoiceId, string invoiceSourceType = "AR");

        #endregion
    }
}
