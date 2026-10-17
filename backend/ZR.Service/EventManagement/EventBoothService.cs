using Infrastructure.Attribute;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    #region Event Booth Service

    /// <summary>
    /// Event Booth - ???????
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventBoothService), ServiceLifetime = LifeTime.Transient)]
    public class EventBoothService : BaseService<EventBooth>, IEventBoothService
    {
        private readonly IEventBoothHistoryService _historyService;
        private readonly IEventTenantService _tenantService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventBoothService(
            IEventBoothHistoryService historyService,
            IEventTenantService tenantService,
            IHttpContextAccessor httpContextAccessor)
        {
            _historyService = historyService;
            _tenantService = tenantService;
            _httpContextAccessor = httpContextAccessor;
        }
        public PagedInfo<EventBoothDto> GetList(EventBoothQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .ToPage<EventBooth, EventBoothDto>(parm);
        }

        public EventBooth GetInfo(long boothId)
        {
            return Queryable().Where(x => x.BoothId == boothId).First();
        }

        public EventBooth AddEventBooth(EventBooth model)
        {
            model.RentalStatus = "available";
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateEventBooth(EventBooth model)
        {
            return Update(model, true, "Update Event Booth");
        }

        public List<EventBooth> GetBoothsByEventId(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId && x.IsActive)
                .OrderBy(x => x.BoothCode)
                .ToList();
        }

        public List<EventBooth> GetAvailableBooths(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId && x.RentalStatus == "available" && x.IsActive)
                .OrderBy(x => x.BoothCode)
                .ToList();
        }

        public int AssignBoothToTenant(long boothId, long tenantId)
        {
            // Legacy method for backwards compatibility
            return AssignBoothToTenantWithContract(new AssignBoothToTenantRequest
            {
                BoothId = boothId,
                TenantId = tenantId,
                CreateContract = false
            });
        }

        public int AssignBoothToTenantWithContract(AssignBoothToTenantRequest request)
        {
            // Validate request
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.BoothId <= 0)
                throw new ArgumentException("Invalid BoothId");

            if (request.TenantId <= 0)
                throw new ArgumentException("Invalid TenantId");

            // Get current booth info before update
            var booth = GetInfo(request.BoothId);
            if (booth == null)
                throw new Exception($"Booth with ID {request.BoothId} not found");

            // Check if booth is already assigned (allow reassignment)
            if (booth.TenantId.HasValue && booth.TenantId.Value > 0)
            {
                // If already assigned, log a warning but allow reassignment
                Console.WriteLine($"[AssignBooth] WARNING: Booth {booth.BoothCode} is currently assigned to tenant ID {booth.TenantId.Value}. Reassigning to tenant ID {request.TenantId}");

            }

            var tenant = _tenantService.GetInfo(request.TenantId);
            if (tenant == null)
                throw new Exception($"Tenant with ID {request.TenantId} not found");

            // Update booth with tenant
            var result = Update(x => x.BoothId == request.BoothId, it => new EventBooth
            {
                TenantId = request.TenantId,
                RentalStatus = "rented"
            });

            // Log history
            if (result > 0)
            {
                var history = new EventBoothHistory
                {
                    BoothId = request.BoothId,
                    BoothCode = booth.BoothCode,
                    BoothName = booth.BoothName,
                    EventId = booth.EventId,
                    TenantId = request.TenantId,
                    TenantName = tenant?.CompanyName,
                    ActionType = "assigned",
                    PreviousStatus = booth.RentalStatus,
                    NewStatus = "rented",
                    PreviousTenantId = booth.TenantId,
                    RentalPrice = booth.RentalPrice,
                    PerformedByUserId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value != null
                        ? long.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId").Value)
                        : (long?)null,
                    PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Notes = $"Booth assigned to tenant: {tenant?.CompanyName}"
                };

                _historyService.LogHistory(history);
            }

            return result;
        }

        public int ReleaseBoothFromTenant(long boothId)
        {
            // Get current booth info before update
            var booth = GetInfo(boothId);
            EventTenant previousTenant = null;

            if (booth.TenantId.HasValue)
            {
                previousTenant = _tenantService.GetInfo(booth.TenantId.Value);
            }

            // Update booth - clear tenant
            var result = Update(x => x.BoothId == boothId, it => new EventBooth
            {
                TenantId = null,
                ContractId = null,
                RentalStatus = "available"
            });

            // Log history
            if (result > 0)
            {
                var history = new EventBoothHistory
                {
                    BoothId = boothId,
                    BoothCode = booth.BoothCode,
                    BoothName = booth.BoothName,
                    EventId = booth.EventId,
                    TenantId = null,
                    TenantName = null,
                    ActionType = "released",
                    PreviousStatus = booth.RentalStatus,
                    NewStatus = "available",
                    PreviousTenantId = booth.TenantId,
                    PreviousTenantName = previousTenant?.CompanyName,
                    RentalPrice = booth.RentalPrice,
                    PerformedByUserId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value != null
                        ? long.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId").Value)
                        : (long?)null,
                    PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Notes = $"Booth released from tenant: {previousTenant?.CompanyName}"
                };

                _historyService.LogHistory(history);
            }

            return result;
        }

        public int UnlockBooth(long boothId)
        {
            // Get current booth info before update
            var booth = GetInfo(boothId);
            EventTenant previousTenant = null;

            if (booth.TenantId.HasValue)
            {
                previousTenant = _tenantService.GetInfo(booth.TenantId.Value);
            }

            // Update booth - unlock and make available
            var result = Update(x => x.BoothId == boothId, it => new EventBooth
            {
                TenantId = null,
                RentalStatus = "available"
            });

            // Log history
            if (result > 0)
            {
                var history = new EventBoothHistory
                {
                    BoothId = boothId,
                    BoothCode = booth.BoothCode,
                    BoothName = booth.BoothName,
                    EventId = booth.EventId,
                    TenantId = null,
                    TenantName = null,
                    ActionType = "unlocked",
                    PreviousStatus = booth.RentalStatus,
                    NewStatus = "available",
                    PreviousTenantId = booth.TenantId,
                    PreviousTenantName = previousTenant?.CompanyName,
                    RentalPrice = booth.RentalPrice,
                    PerformedByUserId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value != null
                        ? long.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId").Value)
                        : (long?)null,
                    PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Notes = $"Booth unlocked and made available{(previousTenant != null ? $" (was assigned to: {previousTenant.CompanyName})" : "")}"
                };

                _historyService.LogHistory(history);
            }

            return result;
        }

        private static Expressionable<EventBooth> QueryExp(EventBoothQueryDto parm)
        {
            var predicate = Expressionable.Create<EventBooth>();

            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.BoothCode), it => it.BoothCode.Contains(parm.BoothCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.BoothName), it => it.BoothName.Contains(parm.BoothName) || it.BoothNameAr.Contains(parm.BoothName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.BoothType), it => it.BoothType == parm.BoothType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.RentalStatus), it => it.RentalStatus == parm.RentalStatus);
            predicate = predicate.AndIF(parm.TenantId != null, it => it.TenantId == parm.TenantId);

            return predicate;
        }
    }

    #endregion
}

