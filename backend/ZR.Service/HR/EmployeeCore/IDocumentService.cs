using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Model.System;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Document Management Service Interface
    /// </summary>
    public interface IDocumentService
    {
        // Document Type Management
        Task<List<DocumentTypeDto>> GetDocumentTypesAsync(bool? isActive = null);
        Task<DocumentTypeDto> GetDocumentTypeByIdAsync(long docTypeId);
        Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeDto docType);
        Task<bool> UpdateDocumentTypeAsync(DocumentTypeDto docType);
        Task<bool> DeleteDocumentTypeAsync(long docTypeId);
        
        // Employee Document Management
        Task<PagedInfo<EmployeeDocumentDto>> GetEmployeeDocumentsAsync(DocumentQueryDto query);
        Task<List<EmployeeDocumentDto>> GetDocumentsByEmployeeAsync(long employeeId, string category = null);
        Task<EmployeeDocumentDto> GetDocumentByIdAsync(long documentId);
        Task<EmployeeDocumentDto> UploadDocumentAsync(DocumentUploadDto upload);
        Task<List<EmployeeDocumentDto>> UploadMultipleDocumentsAsync(MultiDocumentUploadDto uploads);
        Task<bool> UpdateDocumentAsync(EmployeeDocumentDto document);
        Task<bool> DeleteDocumentAsync(long documentId);
        Task<bool> VerifyDocumentAsync(long documentId, string verifiedBy, bool isValid, string comments);
        
        // File Operations
        Task<byte[]> DownloadDocumentAsync(long documentId);
        Task<string> GetDocumentPreviewUrlAsync(long documentId);
        Task<bool> MoveDocumentAsync(long documentId, long newEmployeeId);
        Task<bool> ArchiveDocumentAsync(long documentId);
        Task<bool> RestoreDocumentAsync(long documentId);
        
        // Document Categories
        Task<List<DocumentCategoryDto>> GetDocumentCategoriesAsync();
        Task<DocumentCategoryDto> CreateCategoryAsync(DocumentCategoryDto category);
        Task<bool> UpdateCategoryAsync(DocumentCategoryDto category);
        
        // Document Validation & Compliance
        Task<DocumentComplianceDto> CheckEmployeeComplianceAsync(long employeeId);
        Task<List<ExpiringDocumentDto>> GetExpiringDocumentsAsync(int days = 30);
        Task<List<MissingDocumentDto>> GetMissingRequiredDocumentsAsync(long? employeeId = null);
        Task<bool> SendExpiryReminderAsync(long documentId);
        Task<bool> SendBulkExpiryRemindersAsync();
        
        // Document Templates
        Task<List<DocumentTemplateDto>> GetDocumentTemplatesAsync(string category = null);
        Task<DocumentTemplateDto> CreateTemplateAsync(DocumentTemplateDto template);
        Task<byte[]> GenerateDocumentFromTemplateAsync(long templateId, Dictionary<string, string> data);
        
        // Bulk Operations
        Task<BulkUploadResultDto> BulkUploadDocumentsAsync(BulkDocumentUploadDto bulkUpload);
        Task<bool> BulkVerifyDocumentsAsync(List<long> documentIds, string verifiedBy);
        Task<byte[]> ExportDocumentReportAsync(DocumentReportQueryDto query);
        Task<bool> BulkDeleteDocumentsAsync(List<long> documentIds);
        
        // Document Search & Analytics
        Task<List<EmployeeDocumentDto>> SearchDocumentsAsync(string searchTerm, string category = null);
        Task<DocumentStatisticsDto> GetDocumentStatisticsAsync(long? employeeId = null, long? departmentId = null);
        Task<List<DocumentAuditDto>> GetDocumentAuditTrailAsync(long documentId);
        
        // OCR & Processing
        Task<DocumentOCRResultDto> ProcessDocumentOCRAsync(long documentId);
        Task<bool> ExtractAndSaveMetadataAsync(long documentId);
        
        // Security & Access Control
        Task<bool> CheckDocumentAccessAsync(long documentId, string userId);
        Task<bool> ShareDocumentAsync(long documentId, List<string> userIds, DateTime? expiryDate = null);
        Task<bool> RevokeDocumentAccessAsync(long documentId, string userId);
    }
}
