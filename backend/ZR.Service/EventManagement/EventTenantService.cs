using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
 

    #region Event Tenant Service

    /// <summary>
    /// Event Tenant - المستأجرون
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventTenantService), ServiceLifetime = LifeTime.Transient)]
    public class EventTenantService : BaseService<EventTenant>, IEventTenantService
    {
        public PagedInfo<EventTenantDto> GetList(EventTenantQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .ToPage<EventTenant, EventTenantDto>(parm);
        }

        public EventTenant GetInfo(long tenantId)
        {
            return Queryable().Where(x => x.TenantId == tenantId).First();
        }

        public EventTenant AddEventTenant(EventTenant model)
        {
            model.Status = "active";
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateEventTenant(EventTenant model)
        {
            return Update(model, true, "Update Event Tenant");
        }

        public bool TruncateEventTenant()
        {
            var newTableName = $"event_tenants_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("event_tenants", newTableName);
            }
            return Truncate();
        }

        public (string, object, object) ImportEventTenant(List<EventTenant> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.CompanyName.IsEmpty(), "Company name cannot be empty")
                .SplitError(x => x.Item.CommercialRegistration.IsEmpty(), "Commercial reg number cannot be empty")
                .ToStorage();
            var result = x.AsInsertable.ExecuteCommand();

            string msg = $"Inserted {x.InsertList.Count} Updated {x.UpdateList.Count} Errors {x.ErrorList.Count} Ignored {x.IgnoreList.Count} Deleted {x.DeleteList.Count} Total {x.TotalList.Count}";
            Console.WriteLine(msg);

            return (msg, x.ErrorList, x.IgnoreList);
        }

        public PagedInfo<EventTenantDto> ExportList(EventTenantQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            return Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new EventTenantDto
                {
                    TenantId = it.TenantId,
                    CompanyName = it.CompanyName,
                    CommercialRegistration = it.CommercialRegistration,
                    Email = it.Email,
                    Phone = it.Phone,
                    Status = it.Status
                }, true)
                .ToPage(parm);
        }

        private static Expressionable<EventTenant> QueryExp(EventTenantQueryDto parm)
        {
            var predicate = Expressionable.Create<EventTenant>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CompanyName), it => it.CompanyName.Contains(parm.CompanyName) || it.CompanyNameAr.Contains(parm.CompanyName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CommercialRegNumber), it => it.CommercialRegistration.Contains(parm.CommercialRegNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Email), it => it.Email.Contains(parm.Email));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Phone), it => it.Phone.Contains(parm.Phone));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);

            return predicate;
        }
    }

    #endregion
}

