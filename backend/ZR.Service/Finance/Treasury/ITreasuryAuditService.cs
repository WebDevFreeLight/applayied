using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury Audit Service Interface - واجهة خدمة تدقيق الخزينة
    /// Implements requirement #22: treasury_audit_log for all sensitive operations
    /// </summary>
    public interface ITreasuryAuditService
    {
        /// <summary>
        /// Log a treasury audit event
        /// </summary>
        void LogAudit(TreasuryAuditLog auditLog);

        /// <summary>
        /// Log creation of an entity
        /// </summary>
        void LogCreate<T>(T entity, TreasuryEntityType entityType, string entityNumber, string userId, string userName = null, string ipAddress = null);

        /// <summary>
        /// Log update of an entity
        /// </summary>
        void LogUpdate<T>(T oldEntity, T newEntity, TreasuryEntityType entityType, string entityNumber, string userId, string userName = null, string ipAddress = null);

        /// <summary>
        /// Log deletion of an entity
        /// </summary>
        void LogDelete<T>(T entity, TreasuryEntityType entityType, string entityNumber, string userId, string userName = null, string ipAddress = null);

        /// <summary>
        /// Log status change
        /// </summary>
        void LogStatusChange(Guid entityId, TreasuryEntityType entityType, string entityNumber, string oldStatus, string newStatus, string userId, string userName = null, string ipAddress = null);

        /// <summary>
        /// Log approval action
        /// </summary>
        void LogApproval(Guid entityId, TreasuryEntityType entityType, string entityNumber, decimal? amount, string currency, string userId, string userName = null, string ipAddress = null, string comments = null);

        /// <summary>
        /// Log posting action
        /// </summary>
        void LogPost(Guid entityId, TreasuryEntityType entityType, string entityNumber, decimal? amount, string currency, Guid? journalId, string userId, string userName = null, string ipAddress = null);

        /// <summary>
        /// Log reversal action
        /// </summary>
        void LogReversal(Guid entityId, TreasuryEntityType entityType, string entityNumber, decimal? amount, string currency, Guid? reversalJournalId, string reason, string userId, string userName = null, string ipAddress = null);

        /// <summary>
        /// Log balance adjustment
        /// </summary>
        void LogBalanceAdjustment(Guid accountId, TreasuryEntityType entityType, string accountNumber, decimal oldBalance, decimal newBalance, string currency, string userId, string userName = null, string ipAddress = null, string reason = null);

        /// <summary>
        /// Log allocation action
        /// </summary>
        void LogAllocation(Guid paymentId, Guid invoiceId, string paymentNumber, string invoiceNumber, decimal amount, string currency, string userId, string userName = null, string ipAddress = null);

        /// <summary>
        /// Log failed operation
        /// </summary>
        void LogFailure(TreasuryEntityType entityType, TreasuryAuditAction action, string errorMessage, string userId, string userName = null, string ipAddress = null, object additionalContext = null);
    }

}
