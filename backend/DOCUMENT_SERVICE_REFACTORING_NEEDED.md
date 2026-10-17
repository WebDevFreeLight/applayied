# DocumentService Refactoring Required

## Status: STUBBED OUT - Requires Complete Redesign

### Issue
The DocumentService was using deleted models:
- `HrEmployeeDocument` - Deleted as part of employee system normalization
- `HrDocumentType` - Deleted as part of employee system normalization

### Current State
All methods have been stubbed to throw `NotImplementedException` to allow the build to succeed.

### Methods Requiring Implementation (19 total)

#### Document Type Management
1. `GetDocumentTypesAsync` - Get all document types
2. `GetDocumentTypeByIdAsync` - Get document type by ID
3. `CreateDocumentTypeAsync` - Create new document type
4. `UpdateDocumentTypeAsync` - Update document type
5. `DeleteDocumentTypeAsync` - Delete document type
6. `ActivateDocumentTypeAsync` - Activate document type
7. `DeactivateDocumentTypeAsync` - Deactivate document type

#### Employee Document Management
8. `GetEmployeeDocumentsAsync` - Get all documents for employee
9. `GetEmployeeDocumentByIdAsync` - Get specific employee document
10. `UploadEmployeeDocumentAsync` - Upload new employee document
11. `UpdateEmployeeDocumentAsync` - Update employee document
12. `DeleteEmployeeDocumentAsync` - Delete employee document
13. `DownloadEmployeeDocumentAsync` - Download employee document
14. `GetExpiringDocumentsAsync` - Get documents expiring soon
15. `GetExpiredDocumentsAsync` - Get expired documents
16. `RenewEmployeeDocumentAsync` - Renew expired document

#### Document Verification
17. `VerifyDocumentAsync` - Mark document as verified
18. `RejectDocumentAsync` - Reject document
19. `RequestDocumentRevisionAsync` - Request document revision

### Required Actions

1. **Design New Document System**
   - Decide if documents should be linked to PersonId (from HrPerson) or EmployeeId (from HrEmployeeCore)
   - Create new `HrDocument` model
   - Create new `HrDocumentType` model compatible with new structure
   - Design relationship with normalized employee system

2. **Implement New Models**
   ```csharp
   // Suggested structure:
   public class HrDocument {
       public long DocumentId { get; set; }
       public long PersonId { get; set; }  // or EmployeeId?
       public long DocTypeId { get; set; }
       public string FileName { get; set; }
       public string FilePath { get; set; }
       public DateTime? ExpiryDate { get; set; }
       public string Status { get; set; }  // Pending, Verified, Rejected, Expired
       // ... audit fields
   }

   public class HrDocumentType {
       public long DocTypeId { get; set; }
       public string TypeCode { get; set; }
       public string TypeName { get; set; }
       public bool IsRequired { get; set; }
       public bool HasExpiry { get; set; }
       public int? ReminderDays { get; set; }
       // ... audit fields
   }
   ```

3. **Implement File Upload/Download**
   - File storage strategy (local vs cloud)
   - Security and access control
   - File validation

4. **Update DTOs**
   - Create DTOs compatible with new structure
   - Ensure frontend compatibility

5. **Testing**
   - Unit tests for document operations
   - Integration tests with employee system
   - File upload/download tests

## Estimated Effort
- Design: 2-4 hours
- Implementation: 1-2 days
- Testing: 4-8 hours

## Priority
Medium - Document management is important but not blocking core employee functionality
