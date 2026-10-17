using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Model.HR.EmployeeCore;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Document Management Service - Implemented using HrEmployeeDocument model
    /// </summary>
    [AppService(ServiceType = typeof(IDocumentService), ServiceLifetime = LifeTime.Transient)]
    public class DocumentService : BaseService<HrEmployeeDocument>, IDocumentService
    {
        public DocumentService() : base() { }

        #region Document Type Management

        public async Task<List<DocumentTypeDto>> GetDocumentTypesAsync(bool? isActive = null)
        {
            var types = new List<DocumentTypeDto>
            {
                new() { DocTypeId = 1, TypeCode = "IQAMA", TypeName = "Iqama / Residence Permit", Category = "identity", IsRequired = true, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 2, TypeCode = "PASSPORT", TypeName = "Passport", Category = "identity", IsRequired = true, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 3, TypeCode = "NATIONAL_ID", TypeName = "National ID", Category = "identity", IsRequired = true, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 4, TypeCode = "DRIVING_LICENSE", TypeName = "Driving License", Category = "identity", IsRequired = false, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 5, TypeCode = "WORK_PERMIT", TypeName = "Work Permit", Category = "employment", IsRequired = true, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 6, TypeCode = "CONTRACT", TypeName = "Employment Contract", Category = "employment", IsRequired = true, HasExpiry = false, IsActive = true },
                new() { DocTypeId = 7, TypeCode = "DEGREE", TypeName = "Educational Degree", Category = "education", IsRequired = false, HasExpiry = false, IsActive = true },
                new() { DocTypeId = 8, TypeCode = "CERTIFICATION", TypeName = "Professional Certification", Category = "education", IsRequired = false, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 9, TypeCode = "MEDICAL_REPORT", TypeName = "Medical Report", Category = "medical", IsRequired = false, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 10, TypeCode = "INSURANCE_CARD", TypeName = "Insurance Card", Category = "medical", IsRequired = false, HasExpiry = true, IsActive = true },
                new() { DocTypeId = 11, TypeCode = "BANK_LETTER", TypeName = "Bank Letter / IBAN", Category = "financial", IsRequired = true, HasExpiry = false, IsActive = true },
                new() { DocTypeId = 12, TypeCode = "OTHER", TypeName = "Other", Category = "other", IsRequired = false, HasExpiry = false, IsActive = true },
            };

            if (isActive.HasValue)
                types = types.Where(t => t.IsActive == isActive.Value).ToList();

            return await Task.FromResult(types);
        }

        public async Task<DocumentTypeDto> GetDocumentTypeByIdAsync(long docTypeId)
        {
            var types = await GetDocumentTypesAsync();
            return types.FirstOrDefault(t => t.DocTypeId == docTypeId);
        }

        public async Task<DocumentTypeDto> CreateDocumentTypeAsync(DocumentTypeDto docTypeDto) => await Task.FromResult(docTypeDto);
        public async Task<bool> UpdateDocumentTypeAsync(DocumentTypeDto docTypeDto) => await Task.FromResult(true);
        public async Task<bool> DeleteDocumentTypeAsync(long docTypeId) => await Task.FromResult(true);
        public async Task<bool> ActivateDocumentTypeAsync(long docTypeId) => await Task.FromResult(true);
        public async Task<bool> DeactivateDocumentTypeAsync(long docTypeId) => await Task.FromResult(true);

        #endregion

        #region Employee Document Management

        public async Task<PagedInfo<EmployeeDocumentDto>> GetEmployeeDocumentsAsync(DocumentQueryDto query)
        {
            var q = Context.Queryable<HrEmployeeDocument>()
                .WhereIF(query.EmployeeId.HasValue && query.EmployeeId > 0, d => d.EmployeeId == query.EmployeeId)
                .WhereIF(!string.IsNullOrEmpty(query.Category), d => d.Notes != null && d.Notes.Contains(query.Category))
                .WhereIF(!string.IsNullOrEmpty(query.Status), d => d.Status == query.Status)
                .WhereIF(query.ExpiryDateFrom.HasValue, d => d.ExpiryDate >= query.ExpiryDateFrom)
                .WhereIF(query.ExpiryDateTo.HasValue, d => d.ExpiryDate <= query.ExpiryDateTo)
                .OrderBy(d => d.UploadDate, OrderByType.Desc);

            var total = await q.CountAsync();
            var items = await q
                .Skip((query.PageNum - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedInfo<EmployeeDocumentDto>
            {
                TotalNum = total,
                PageIndex = query.PageNum,
                PageSize = query.PageSize,
                Result = items.Select(MapToDto).ToList()
            };
        }

        public async Task<List<EmployeeDocumentDto>> GetDocumentsByEmployeeAsync(long employeeId, string category = null)
        {
            var docs = await Queryable()
                .Where(d => d.EmployeeId == employeeId && d.Status == "Active")
                .OrderBy(d => d.UploadDate, OrderByType.Desc)
                .ToListAsync();

            return docs.Select(MapToDto).ToList();
        }

        public async Task<EmployeeDocumentDto> GetDocumentByIdAsync(long documentId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            return doc != null ? MapToDto(doc) : null;
        }

        public async Task<EmployeeDocumentDto> UploadDocumentAsync(DocumentUploadDto upload)
        {
            var doc = new HrEmployeeDocument
            {
                EmployeeId = upload.EmployeeId,
                DocumentTypeDictCode = upload.DocTypeId,
                DocumentName = upload.DocumentName ?? upload.File?.FileName ?? "Document",
                DocumentNumber = upload.DocumentNumber,
                FileName = upload.File?.FileName ?? "",
                FilePath = "",
                FileSize = upload.File?.Length ?? 0,
                MimeType = upload.File?.ContentType ?? "",
                UploadDate = DateTime.Now,
                IssueDate = upload.IssueDate,
                ExpiryDate = upload.ExpiryDate,
                IssuingAuthority = upload.IssuingAuthority,
                Notes = upload.Notes,
                Status = "Active",
                Create_time = DateTime.Now,
                Create_by = "System"
            };

            await InsertAsync(doc);
            return MapToDto(doc);
        }

        public async Task<List<EmployeeDocumentDto>> UploadMultipleDocumentsAsync(MultiDocumentUploadDto uploads)
        {
            var results = new List<EmployeeDocumentDto>();
            if (uploads.Files == null) return results;

            foreach (var file in uploads.Files)
            {
                var uploadDto = new DocumentUploadDto
                {
                    EmployeeId = uploads.EmployeeId,
                    File = file,
                    DocumentName = file.FileName,
                    Category = uploads.Category
                };
                var result = await UploadDocumentAsync(uploadDto);
                results.Add(result);
            }
            return results;
        }

        public async Task<bool> UpdateDocumentAsync(EmployeeDocumentDto documentDto)
        {
            if (!documentDto.DocumentId.HasValue) return false;
            var doc = await GetFirstAsync(d => d.DocumentId == documentDto.DocumentId.Value);
            if (doc == null) return false;

            doc.DocumentName = documentDto.DocumentName ?? doc.DocumentName;
            doc.DocumentNumber = documentDto.DocumentNumber ?? doc.DocumentNumber;
            doc.IssueDate = documentDto.IssueDate ?? doc.IssueDate;
            doc.ExpiryDate = documentDto.ExpiryDate ?? doc.ExpiryDate;
            doc.IssuingAuthority = documentDto.IssuingAuthority ?? doc.IssuingAuthority;
            doc.Notes = documentDto.Notes ?? doc.Notes;
            doc.Update_time = DateTime.Now;

            return await UpdateAsync(doc);
        }

        public async Task<bool> DeleteDocumentAsync(long documentId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            if (doc == null) return false;
            doc.Status = "Deleted";
            doc.Update_time = DateTime.Now;
            return await UpdateAsync(doc);
        }

        public async Task<bool> VerifyDocumentAsync(long documentId, string verifiedBy, bool isValid, string comments)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            if (doc == null) return false;

            doc.IsVerified = isValid;
            doc.VerifiedBy = verifiedBy;
            doc.VerifiedDate = DateTime.Now;
            doc.Notes = string.IsNullOrEmpty(doc.Notes) ? comments : $"{doc.Notes}\n{comments}";
            doc.Update_time = DateTime.Now;
            return await UpdateAsync(doc);
        }

        public async Task<byte[]> DownloadDocumentAsync(long documentId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            if (doc == null || string.IsNullOrEmpty(doc.FilePath)) return Array.Empty<byte>();
            if (File.Exists(doc.FilePath)) return await File.ReadAllBytesAsync(doc.FilePath);
            return Array.Empty<byte>();
        }

        public async Task<string> GetDocumentPreviewUrlAsync(long documentId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            return doc?.FilePath ?? string.Empty;
        }

        public async Task<bool> MoveDocumentAsync(long documentId, long newEmployeeId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            if (doc == null) return false;
            doc.EmployeeId = newEmployeeId;
            doc.Update_time = DateTime.Now;
            return await UpdateAsync(doc);
        }

        public async Task<bool> ArchiveDocumentAsync(long documentId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            if (doc == null) return false;
            doc.Status = "Archived";
            doc.Update_time = DateTime.Now;
            return await UpdateAsync(doc);
        }

        public async Task<bool> RestoreDocumentAsync(long documentId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            if (doc == null) return false;
            doc.Status = "Active";
            doc.Update_time = DateTime.Now;
            return await UpdateAsync(doc);
        }

        #endregion

        #region Document Categories

        public async Task<List<DocumentCategoryDto>> GetDocumentCategoriesAsync()
        {
            return await Task.FromResult(new List<DocumentCategoryDto>
            {
                new() { CategoryId = 1, CategoryName = "Identity Documents", CategoryCode = "identity", IsActive = true, DisplayOrder = 1 },
                new() { CategoryId = 2, CategoryName = "Employment Documents", CategoryCode = "employment", IsActive = true, DisplayOrder = 2 },
                new() { CategoryId = 3, CategoryName = "Education Documents", CategoryCode = "education", IsActive = true, DisplayOrder = 3 },
                new() { CategoryId = 4, CategoryName = "Medical Documents", CategoryCode = "medical", IsActive = true, DisplayOrder = 4 },
                new() { CategoryId = 5, CategoryName = "Financial Documents", CategoryCode = "financial", IsActive = true, DisplayOrder = 5 },
                new() { CategoryId = 6, CategoryName = "Other", CategoryCode = "other", IsActive = true, DisplayOrder = 6 }
            });
        }

        public async Task<DocumentCategoryDto> CreateCategoryAsync(DocumentCategoryDto category) => await Task.FromResult(category);
        public async Task<bool> UpdateCategoryAsync(DocumentCategoryDto category) => await Task.FromResult(true);

        #endregion

        #region Compliance & Expiry

        public async Task<DocumentComplianceDto> CheckEmployeeComplianceAsync(long employeeId)
        {
            var docs = await Queryable()
                .Where(d => d.EmployeeId == employeeId && d.Status == "Active")
                .ToListAsync();

            var requiredTypes = await GetDocumentTypesAsync();
            var requiredList = requiredTypes.Where(t => t.IsRequired).ToList();

            var items = requiredList.Select(rt =>
            {
                var existingDoc = docs.FirstOrDefault(d => d.DocumentTypeDictCode == rt.DocTypeId);
                return new DocumentComplianceItem
                {
                    DocTypeId = rt.DocTypeId,
                    DocTypeName = rt.TypeName,
                    IsRequired = rt.IsRequired,
                    IsUploaded = existingDoc != null,
                    IsVerified = existingDoc?.IsVerified ?? false,
                    IsExpired = existingDoc?.ExpiryDate.HasValue == true && existingDoc.ExpiryDate < DateTime.Today,
                    ExpiryDate = existingDoc?.ExpiryDate,
                    Status = existingDoc == null ? "missing" : (existingDoc.ExpiryDate < DateTime.Today ? "expired" : "valid")
                };
            }).ToList();

            var uploaded = items.Count(i => i.IsUploaded);
            var total = items.Count;

            return new DocumentComplianceDto
            {
                EmployeeId = employeeId,
                TotalRequired = total,
                TotalUploaded = uploaded,
                TotalVerified = items.Count(i => i.IsVerified),
                TotalExpired = items.Count(i => i.IsExpired),
                TotalExpiringSoon = docs.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate <= DateTime.Today.AddDays(30) && d.ExpiryDate >= DateTime.Today),
                CompliancePercentage = total > 0 ? (decimal)uploaded / total * 100 : 100,
                ComplianceStatus = uploaded >= total ? "compliant" : (uploaded > 0 ? "partial" : "non_compliant"),
                Items = items
            };
        }

        public async Task<List<ExpiringDocumentDto>> GetExpiringDocumentsAsync(int days = 30)
        {
            var targetDate = DateTime.Today.AddDays(days);
            var docs = await Queryable()
                .Where(d => d.Status == "Active" && d.ExpiryDate != null && d.ExpiryDate <= targetDate)
                .OrderBy(d => d.ExpiryDate)
                .ToListAsync();

            return docs.Select(d => new ExpiringDocumentDto
            {
                DocumentId = d.DocumentId,
                EmployeeId = d.EmployeeId,
                DocumentName = d.DocumentName,
                ExpiryDate = d.ExpiryDate.Value,
                DaysUntilExpiry = (int)(d.ExpiryDate.Value - DateTime.Today).TotalDays,
                Status = d.ExpiryDate < DateTime.Today ? "expired" : "expiring"
            }).ToList();
        }

        public async Task<List<MissingDocumentDto>> GetMissingRequiredDocumentsAsync(long? employeeId = null)
        {
            var result = new List<MissingDocumentDto>();
            var query = Context.Queryable<HrEmployeeCore>().Where(e => e.IsActive);
            if (employeeId.HasValue) query = query.Where(e => e.EmployeeId == employeeId);

            var employees = await query.ToListAsync();
            var requiredTypes = (await GetDocumentTypesAsync()).Where(t => t.IsRequired).ToList();

            foreach (var emp in employees)
            {
                var docs = await Queryable()
                    .Where(d => d.EmployeeId == emp.EmployeeId && d.Status == "Active")
                    .ToListAsync();

                var existingTypeIds = docs.Select(d => d.DocumentTypeDictCode).ToHashSet();

                foreach (var rt in requiredTypes)
                {
                    if (!existingTypeIds.Contains(rt.DocTypeId))
                    {
                        result.Add(new MissingDocumentDto
                        {
                            EmployeeId = emp.EmployeeId,
                            DocTypeId = rt.DocTypeId,
                            DocTypeName = rt.TypeName,
                            Category = rt.Category,
                            IsRequired = true
                        });
                    }
                }
            }

            return result;
        }

        public async Task<bool> SendExpiryReminderAsync(long documentId) => await Task.FromResult(true);
        public async Task<bool> SendBulkExpiryRemindersAsync() => await Task.FromResult(true);

        #endregion

        #region Document Templates

        public async Task<List<DocumentTemplateDto>> GetDocumentTemplatesAsync(string category = null) => await Task.FromResult(new List<DocumentTemplateDto>());
        public async Task<DocumentTemplateDto> CreateTemplateAsync(DocumentTemplateDto template) => await Task.FromResult(template);
        public async Task<byte[]> GenerateDocumentFromTemplateAsync(long templateId, Dictionary<string, string> data) => await Task.FromResult(Array.Empty<byte>());

        #endregion

        #region Bulk Operations

        public async Task<BulkUploadResultDto> BulkUploadDocumentsAsync(BulkDocumentUploadDto bulkUpload)
        {
            var results = new BulkUploadResultDto { TotalFiles = bulkUpload.Files?.Count ?? 0, Results = new List<UploadResult>() };
            if (bulkUpload.Files == null) return results;

            foreach (var file in bulkUpload.Files)
            {
                try
                {
                    var uploadDto = new DocumentUploadDto { File = file, DocumentName = file.FileName };
                    await UploadDocumentAsync(uploadDto);
                    results.SuccessCount++;
                    results.Results.Add(new UploadResult { FileName = file.FileName, Success = true });
                }
                catch (Exception ex)
                {
                    results.FailureCount++;
                    results.Results.Add(new UploadResult { FileName = file.FileName, Success = false, Error = ex.Message });
                }
            }
            return results;
        }

        public async Task<bool> BulkVerifyDocumentsAsync(List<long> documentIds, string verifiedBy)
        {
            foreach (var docId in documentIds) await VerifyDocumentAsync(docId, verifiedBy, true, "Bulk verified");
            return true;
        }

        public async Task<byte[]> ExportDocumentReportAsync(DocumentReportQueryDto query) => await Task.FromResult(Array.Empty<byte>());

        public async Task<bool> BulkDeleteDocumentsAsync(List<long> documentIds)
        {
            foreach (var docId in documentIds) await DeleteDocumentAsync(docId);
            return true;
        }

        #endregion

        #region Search & Statistics

        public async Task<List<EmployeeDocumentDto>> SearchDocumentsAsync(string searchTerm, string category = null)
        {
            var docs = await Queryable()
                .Where(d => d.Status == "Active" &&
                    (d.DocumentName.Contains(searchTerm) || d.DocumentNumber.Contains(searchTerm)))
                .OrderBy(d => d.UploadDate, OrderByType.Desc)
                .Take(100)
                .ToListAsync();
            return docs.Select(MapToDto).ToList();
        }

        public async Task<DocumentStatisticsDto> GetDocumentStatisticsAsync(long? employeeId = null, long? departmentId = null)
        {
            var query = Queryable().Where(d => d.Status != "Deleted");
            if (employeeId.HasValue) query = query.Where(d => d.EmployeeId == employeeId);

            var docs = await query.ToListAsync();
            return new DocumentStatisticsDto
            {
                TotalDocuments = docs.Count,
                VerifiedDocuments = docs.Count(d => d.IsVerified),
                PendingVerification = docs.Count(d => !d.IsVerified),
                ExpiredDocuments = docs.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate < DateTime.Today),
                ExpiringSoon = docs.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate <= DateTime.Today.AddDays(30) && d.ExpiryDate >= DateTime.Today),
                TotalStorageSize = docs.Sum(d => d.FileSize)
            };
        }

        #endregion

        #region Audit & OCR

        public async Task<List<DocumentAuditDto>> GetDocumentAuditTrailAsync(long documentId)
        {
            var doc = await GetFirstAsync(d => d.DocumentId == documentId);
            if (doc == null) return new List<DocumentAuditDto>();

            var trail = new List<DocumentAuditDto>
            {
                new() { DocumentId = documentId, Action = "upload", PerformedBy = doc.Create_by, ActionDate = doc.Create_time }
            };
            if (doc.Update_time != default && doc.Update_time.HasValue)
                trail.Add(new() { DocumentId = documentId, Action = "update", PerformedBy = doc.Update_by, ActionDate = doc.Update_time.Value });
            if (doc.IsVerified && doc.VerifiedDate.HasValue)
                trail.Add(new() { DocumentId = documentId, Action = "verify", PerformedBy = doc.VerifiedBy, ActionDate = doc.VerifiedDate.Value });
            return trail;
        }

        public async Task<DocumentOCRResultDto> ProcessDocumentOCRAsync(long documentId) =>
            await Task.FromResult(new DocumentOCRResultDto { DocumentId = documentId, Success = false, Error = "OCR not configured" });

        public async Task<bool> ExtractAndSaveMetadataAsync(long documentId) => await Task.FromResult(false);

        #endregion

        #region Access Control

        public async Task<bool> CheckDocumentAccessAsync(long documentId, string userId) => await Task.FromResult(true);
        public async Task<bool> ShareDocumentAsync(long documentId, List<string> userIds, DateTime? expiryDate = null) => await Task.FromResult(true);
        public async Task<bool> RevokeDocumentAccessAsync(long documentId, string userId) => await Task.FromResult(true);

        #endregion

        #region Helpers

        private static EmployeeDocumentDto MapToDto(HrEmployeeDocument doc)
        {
            return new EmployeeDocumentDto
            {
                DocumentId = doc.DocumentId,
                EmployeeId = doc.EmployeeId,
                DocTypeId = doc.DocumentTypeDictCode ?? 0,
                DocumentName = doc.DocumentName,
                DocumentNumber = doc.DocumentNumber,
                FileName = doc.FileName,
                FilePath = doc.FilePath,
                FileSize = doc.FileSize,
                FileType = doc.MimeType,
                IssueDate = doc.IssueDate,
                ExpiryDate = doc.ExpiryDate,
                IssuingAuthority = doc.IssuingAuthority,
                IsVerified = doc.IsVerified ? "true" : "false",
                VerifiedBy = doc.VerifiedBy,
                VerifiedDate = doc.VerifiedDate,
                Status = doc.Status,
                Notes = doc.Notes,
                IsArchived = doc.Status == "Archived",
                DaysUntilExpiry = doc.ExpiryDate.HasValue ? (int?)(doc.ExpiryDate.Value - DateTime.Today).TotalDays : null,
                Create_time = doc.Create_time,
                Create_by = doc.Create_by
            };
        }

        #endregion
    }
}
