using System;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Repository.Finance.Treasury;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace ZR.Service.Finance.Treasury
{
  
    /// <summary>
    /// Treasury Audit Service - خدمة تدقيق الخزينة
    /// </summary>
    [AppService(ServiceType = typeof(ITreasuryAuditService), ServiceLifetime = LifeTime.Transient)]
    public class TreasuryAuditService : ITreasuryAuditService
    {
        private readonly TreasuryAuditLogRepository _auditLogRepository;

        public TreasuryAuditService(TreasuryAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public void LogAudit(TreasuryAuditLog auditLog)
        {
            if (auditLog.Id == Guid.Empty)
            {
                auditLog.Id = Guid.NewGuid();
            }
            auditLog.Timestamp = DateTime.UtcNow;
            auditLog.Create_time = DateTime.Now;

            _auditLogRepository.Insert(auditLog);
        }

        public void LogCreate<T>(T entity, TreasuryEntityType entityType, string entityNumber, string userId, string userName = null, string ipAddress = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = GetEntityId(entity),
                EntityNumber = entityNumber,
                Action = TreasuryAuditAction.Create,
                ActionDescription = $"Created {entityType}: {entityNumber}",
                NewValues = SerializeEntity(entity),
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogUpdate<T>(T oldEntity, T newEntity, TreasuryEntityType entityType, string entityNumber, string userId, string userName = null, string ipAddress = null)
        {
            var changedFields = GetChangedFields(oldEntity, newEntity);

            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = GetEntityId(newEntity),
                EntityNumber = entityNumber,
                Action = TreasuryAuditAction.Update,
                ActionDescription = $"Updated {entityType}: {entityNumber}",
                OldValues = SerializeEntity(oldEntity),
                NewValues = SerializeEntity(newEntity),
                ChangedFields = changedFields,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogDelete<T>(T entity, TreasuryEntityType entityType, string entityNumber, string userId, string userName = null, string ipAddress = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = GetEntityId(entity),
                EntityNumber = entityNumber,
                Action = TreasuryAuditAction.Delete,
                ActionDescription = $"Deleted {entityType}: {entityNumber}",
                OldValues = SerializeEntity(entity),
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogStatusChange(Guid entityId, TreasuryEntityType entityType, string entityNumber, string oldStatus, string newStatus, string userId, string userName = null, string ipAddress = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EntityNumber = entityNumber,
                Action = TreasuryAuditAction.StatusChange,
                ActionDescription = $"Status changed from {oldStatus} to {newStatus}",
                OldValues = JsonSerializer.Serialize(new { Status = oldStatus }),
                NewValues = JsonSerializer.Serialize(new { Status = newStatus }),
                ChangedFields = "Status",
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogApproval(Guid entityId, TreasuryEntityType entityType, string entityNumber, decimal? amount, string currency, string userId, string userName = null, string ipAddress = null, string comments = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EntityNumber = entityNumber,
                Action = TreasuryAuditAction.Approve,
                ActionDescription = $"Approved {entityType}: {entityNumber}",
                Amount = amount,
                Currency = currency,
                AdditionalContext = !string.IsNullOrEmpty(comments) ? JsonSerializer.Serialize(new { Comments = comments }) : null,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogPost(Guid entityId, TreasuryEntityType entityType, string entityNumber, decimal? amount, string currency, Guid? journalId, string userId, string userName = null, string ipAddress = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EntityNumber = entityNumber,
                Action = TreasuryAuditAction.Post,
                ActionDescription = $"Posted {entityType}: {entityNumber}",
                Amount = amount,
                Currency = currency,
                AdditionalContext = journalId.HasValue ? JsonSerializer.Serialize(new { GLJournalId = journalId }) : null,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogReversal(Guid entityId, TreasuryEntityType entityType, string entityNumber, decimal? amount, string currency, Guid? reversalJournalId, string reason, string userId, string userName = null, string ipAddress = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EntityNumber = entityNumber,
                Action = TreasuryAuditAction.Reverse,
                ActionDescription = $"Reversed {entityType}: {entityNumber}. Reason: {reason}",
                Amount = amount,
                Currency = currency,
                AdditionalContext = JsonSerializer.Serialize(new { ReversalJournalId = reversalJournalId, Reason = reason }),
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogBalanceAdjustment(Guid accountId, TreasuryEntityType entityType, string accountNumber, decimal oldBalance, decimal newBalance, string currency, string userId, string userName = null, string ipAddress = null, string reason = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = accountId,
                EntityNumber = accountNumber,
                Action = TreasuryAuditAction.BalanceAdjustment,
                ActionDescription = $"Balance adjusted from {oldBalance} to {newBalance} {currency}",
                Amount = newBalance - oldBalance,
                Currency = currency,
                OldValues = JsonSerializer.Serialize(new { Balance = oldBalance }),
                NewValues = JsonSerializer.Serialize(new { Balance = newBalance }),
                AdditionalContext = !string.IsNullOrEmpty(reason) ? JsonSerializer.Serialize(new { Reason = reason }) : null,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogAllocation(Guid paymentId, Guid invoiceId, string paymentNumber, string invoiceNumber, decimal amount, string currency, string userId, string userName = null, string ipAddress = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = TreasuryEntityType.PaymentAllocation,
                EntityId = paymentId,
                EntityNumber = paymentNumber,
                Action = TreasuryAuditAction.Allocation,
                ActionDescription = $"Allocated {amount} {currency} from payment {paymentNumber} to invoice {invoiceNumber}",
                Amount = amount,
                Currency = currency,
                AdditionalContext = JsonSerializer.Serialize(new { PaymentId = paymentId, InvoiceId = invoiceId, InvoiceNumber = invoiceNumber }),
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        public void LogFailure(TreasuryEntityType entityType, TreasuryAuditAction action, string errorMessage, string userId, string userName = null, string ipAddress = null, object additionalContext = null)
        {
            var auditLog = new TreasuryAuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = Guid.Empty,
                Action = action,
                ActionDescription = $"Failed to {action}: {errorMessage}",
                ErrorMessage = errorMessage,
                AdditionalContext = additionalContext != null ? JsonSerializer.Serialize(additionalContext) : null,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                IsSuccessful = false,
                Timestamp = DateTime.UtcNow,
                Create_time = DateTime.Now
            };

            _auditLogRepository.Insert(auditLog);
        }

        #region Helper Methods

        private Guid GetEntityId<T>(T entity)
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                return (Guid)idProperty.GetValue(entity);
            }
            return Guid.Empty;
        }

        private string SerializeEntity<T>(T entity)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                return JsonSerializer.Serialize(entity, options);
            }
            catch
            {
                return null;
            }
        }

        private string GetChangedFields<T>(T oldEntity, T newEntity)
        {
            var changedFields = new System.Collections.Generic.List<string>();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                // Skip navigation properties and complex types
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    continue;

                var oldValue = prop.GetValue(oldEntity);
                var newValue = prop.GetValue(newEntity);

                if (!Equals(oldValue, newValue))
                {
                    changedFields.Add(prop.Name);
                }
            }

            return string.Join(",", changedFields);
        }

        #endregion
    }
}
