using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Boulevard;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;
using ZR.Model.Customers;
using ZR.Model.Operations.Readiness;
using ZR.Model.Operations.Readiness.Dto;
using ZR.Repository;

namespace ZR.Service.Operations.Readiness
{
    [AppService(ServiceType = typeof(IOpsStoreHandoverService), ServiceLifetime = LifeTime.Transient)]
    public class OpsStoreHandoverService : BaseService<OpsStoreHandover>, IOpsStoreHandoverService
    {
        public async Task<PagedInfo<OpsStoreHandover>> GetHandoverListAsync(StoreHandoverQueryDto query)
        {
            var pageQuery = Context
                .Queryable<OpsStoreHandover, BlvBoulevardShops, BlvBoulevardSeasonZones, ErpContractUnit, ErpContract, Customer>(
                    (h, shop, zone, cu, c, cust) => new JoinQueryInfos(
                        JoinType.Left, h.ShopId == shop.Id,
                        JoinType.Left, shop.ZoneId == zone.Id,
                        JoinType.Left, cu.BlvShopId == h.ShopId && cu.IsActive == true,
                        JoinType.Left, c.ContractId == cu.ContractId && c.Status != ContractStatus.Terminated && c.Status != ContractStatus.Cancelled,
                        JoinType.Left, cust.Id == c.CustomerId))
                .WhereIF(query.ShopId.HasValue, (h, shop, zone, cu, c, cust) => h.ShopId == query.ShopId.Value)
                .WhereIF(query.SeasonId.HasValue, (h, shop, zone, cu, c, cust) => h.SeasonId == query.SeasonId.Value)
                .WhereIF(!string.IsNullOrEmpty(query.Status), (h, shop, zone, cu, c, cust) => h.Status == query.Status)
                .OrderBy((h, shop, zone, cu, c, cust) => h.Create_time, OrderByType.Desc)
                .Select((h, shop, zone, cu, c, cust) => new OpsStoreHandover
                {
                    HandoverId = h.HandoverId,
                    ShopId = h.ShopId,
                    SeasonId = h.SeasonId,
                    Progress = h.Progress,
                    Status = h.Status,
                    Notes = h.Notes,
                    RequestedBy = h.RequestedBy,
                    RequestedAt = h.RequestedAt,
                    ApprovedBy = h.ApprovedBy,
                    ApprovedAt = h.ApprovedAt,
                    RejectedBy = h.RejectedBy,
                    RejectedAt = h.RejectedAt,
                    RejectionReason = h.RejectionReason,
                    Create_by = h.Create_by,
                    Create_time = h.Create_time,
                    ShopName = shop.Name,
                    ZoneName = zone.Name,
                    UnitType = shop.UnitType,
                    BrandName = cust.BrandName,
                    BrandImage = cust.BrandImage,
                    CustomerNameAr = cust.NameAr
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<OpsStoreHandover> GetHandoverByIdAsync(long handoverId)
        {
            var handover = await Context
                .Queryable<OpsStoreHandover, BlvBoulevardShops, BlvBoulevardSeasonZones, ErpContractUnit, ErpContract, Customer>(
                    (h, shop, zone, cu, c, cust) => new JoinQueryInfos(
                        JoinType.Left, h.ShopId == shop.Id,
                        JoinType.Left, shop.ZoneId == zone.Id,
                        JoinType.Left, cu.BlvShopId == h.ShopId && cu.IsActive == true,
                        JoinType.Left, c.ContractId == cu.ContractId && c.Status != ContractStatus.Terminated && c.Status != ContractStatus.Cancelled,
                        JoinType.Left, cust.Id == c.CustomerId))
                .Where((h, shop, zone, cu, c, cust) => h.HandoverId == handoverId)
                .Select((h, shop, zone, cu, c, cust) => new OpsStoreHandover
                {
                    HandoverId = h.HandoverId,
                    ShopId = h.ShopId,
                    SeasonId = h.SeasonId,
                    Progress = h.Progress,
                    Status = h.Status,
                    Notes = h.Notes,
                    RequestedBy = h.RequestedBy,
                    RequestedAt = h.RequestedAt,
                    ApprovedBy = h.ApprovedBy,
                    ApprovedAt = h.ApprovedAt,
                    RejectedBy = h.RejectedBy,
                    RejectedAt = h.RejectedAt,
                    RejectionReason = h.RejectionReason,
                    Create_by = h.Create_by,
                    Create_time = h.Create_time,
                    ShopName = shop.Name,
                    ZoneName = zone.Name,
                    UnitType = shop.UnitType,
                    BrandName = cust.BrandName,
                    BrandImage = cust.BrandImage,
                    CustomerNameAr = cust.NameAr
                })
                .FirstAsync();

            if (handover != null)
            {
                // Auto-sync: add missing requirements / remove deactivated ones
                if (handover.Status != "Approved")
                {
                    await SyncSingleHandoverAsync(handover.HandoverId);
                }
                handover.Items = await GetHandoverItemsAsync(handoverId);
            }

            return handover;
        }

        public async Task<OpsStoreHandover> GetHandoverByShopAsync(long shopId)
        {
            var handover = await Context.Queryable<OpsStoreHandover>()
                .Where(h => h.ShopId == shopId)
                .OrderBy(h => h.Create_time, OrderByType.Desc)
                .FirstAsync();

            if (handover != null)
                return await GetHandoverByIdAsync(handover.HandoverId);

            return null;
        }

        public async Task<OpsStoreHandover> InitializeHandoverAsync(InitializeHandoverDto dto, string userName)
        {
            // Check if there's already a non-Approved handover for this shop
            var existing = await Context.Queryable<OpsStoreHandover>()
                .Where(h => h.ShopId == dto.ShopId && h.Status != "Approved" && h.Status != "Rejected")
                .FirstAsync();

            if (existing != null)
                return null; // Already has an active handover

            // Get all active requirements
            var requirements = await Context.Queryable<OpsHandoverRequirement>()
                .Where(r => r.IsActive)
                .OrderBy(r => r.SortOrder)
                .ToListAsync();

            if (requirements.Count == 0)
                return null; // No requirements configured

            // Create handover
            var handover = new OpsStoreHandover
            {
                HandoverId = SnowFlakeSingle.Instance.NextId(),
                ShopId = dto.ShopId,
                SeasonId = dto.SeasonId,
                Progress = 0,
                Status = "Pending",
                Notes = dto.Notes,
                Create_by = userName,
                Create_time = DateTime.Now
            };

            await Context.Insertable(handover).ExecuteCommandAsync();

            // Create items for each active requirement
            var items = requirements.Select(req => new OpsStoreHandoverItem
            {
                ItemId = SnowFlakeSingle.Instance.NextId(),
                HandoverId = handover.HandoverId,
                RequirementId = req.RequirementId,
                IsCompleted = false,
                Create_by = userName,
                Create_time = DateTime.Now
            }).ToList();

            await Context.Insertable(items).ExecuteCommandAsync();

            return await GetHandoverByIdAsync(handover.HandoverId);
        }

        public async Task<OpsStoreHandoverItem> CompleteItemAsync(long itemId, CompleteItemDto dto, string userName)
        {
            var item = await Context.Queryable<OpsStoreHandoverItem>()
                .Where(i => i.ItemId == itemId)
                .FirstAsync();

            if (item == null) return null;

            // Get the requirement to check if photo is required
            var requirement = await Context.Queryable<OpsHandoverRequirement>()
                .Where(r => r.RequirementId == item.RequirementId)
                .FirstAsync();

            if (requirement != null && requirement.PhotoRequired && string.IsNullOrEmpty(dto?.PhotoUrls))
                return null; // Photo required but not provided

            item.IsCompleted = true;
            item.PhotoUrls = dto?.PhotoUrls;
            item.Notes = dto?.Notes;
            item.CompletedAt = DateTime.Now;
            item.CompletedBy = userName;
            item.Update_by = userName;
            item.Update_time = DateTime.Now;

            await Context.Updateable(item).ExecuteCommandAsync();

            // Recalculate progress
            await RecalculateProgressAsync(item.HandoverId, userName);

            return item;
        }

        public async Task<OpsStoreHandoverItem> UncompleteItemAsync(long itemId, string userName)
        {
            var item = await Context.Queryable<OpsStoreHandoverItem>()
                .Where(i => i.ItemId == itemId)
                .FirstAsync();

            if (item == null) return null;

            item.IsCompleted = false;
            item.CompletedAt = null;
            item.CompletedBy = null;
            item.Update_by = userName;
            item.Update_time = DateTime.Now;

            await Context.Updateable(item).ExecuteCommandAsync();

            // Recalculate progress
            await RecalculateProgressAsync(item.HandoverId, userName);

            return item;
        }

        public async Task<int> RequestHandoverAsync(long handoverId, RequestHandoverDto dto, string userName)
        {
            var handover = await Context.Queryable<OpsStoreHandover>()
                .Where(h => h.HandoverId == handoverId)
                .FirstAsync();

            if (handover == null) return 0;
            if (handover.Status != "ReadyForHandover") return -1;

            // Validate: progress must be 100 and no blocking items incomplete
            var progress = await GetProgressAsync(handoverId);
            if (!progress.CanRequestHandover) return -2;

            handover.RequestedBy = userName;
            handover.RequestedAt = DateTime.Now;
            handover.Notes = dto?.Notes ?? handover.Notes;
            handover.Update_by = userName;
            handover.Update_time = DateTime.Now;

            return await Context.Updateable(handover).ExecuteCommandAsync();
        }

        public async Task<int> ApproveHandoverAsync(long handoverId, ApproveHandoverDto dto, string userName)
        {
            var handover = await Context.Queryable<OpsStoreHandover>()
                .Where(h => h.HandoverId == handoverId)
                .FirstAsync();

            if (handover == null) return 0;
            if (handover.Status != "ReadyForHandover") return -1;

            handover.Status = "Approved";
            handover.ApprovedBy = userName;
            handover.ApprovedAt = DateTime.Now;
            handover.Notes = dto?.Notes ?? handover.Notes;
            handover.Update_by = userName;
            handover.Update_time = DateTime.Now;

            return await Context.Updateable(handover).ExecuteCommandAsync();
        }

        public async Task<int> RejectHandoverAsync(long handoverId, RejectHandoverDto dto, string userName)
        {
            var handover = await Context.Queryable<OpsStoreHandover>()
                .Where(h => h.HandoverId == handoverId)
                .FirstAsync();

            if (handover == null) return 0;
            if (handover.Status != "ReadyForHandover") return -1;

            handover.Status = "Rejected";
            handover.RejectedBy = userName;
            handover.RejectedAt = DateTime.Now;
            handover.RejectionReason = dto?.RejectionReason;
            handover.Update_by = userName;
            handover.Update_time = DateTime.Now;

            return await Context.Updateable(handover).ExecuteCommandAsync();
        }

        public async Task<HandoverProgressDto> GetProgressAsync(long handoverId)
        {
            var items = await Context
                .Queryable<OpsStoreHandoverItem, OpsHandoverRequirement>(
                    (i, r) => new JoinQueryInfos(JoinType.Left, i.RequirementId == r.RequirementId))
                .Where((i, r) => i.HandoverId == handoverId)
                .Select((i, r) => new OpsStoreHandoverItem
                {
                    ItemId = i.ItemId,
                    IsCompleted = i.IsCompleted,
                    Percentage = r.Percentage,
                    IsBlocking = r.IsBlocking,
                    NameEn = r.NameEn,
                    Code = r.Code
                })
                .ToListAsync();

            var progress = items.Where(i => i.IsCompleted).Sum(i => i.Percentage);
            var blockingIncomplete = items
                .Where(i => i.IsBlocking && !i.IsCompleted)
                .Select(i => $"{i.Code}: {i.NameEn}")
                .ToArray();

            return new HandoverProgressDto
            {
                Progress = progress,
                CanRequestHandover = progress == 100.0m && blockingIncomplete.Length == 0,
                TotalItems = items.Count,
                CompletedItems = items.Count(i => i.IsCompleted),
                BlockingIncomplete = blockingIncomplete
            };
        }

        public async Task<int> SyncRequirementsForActiveHandoversAsync(string userName)
        {
            // Get all non-terminal handovers (Pending, InProgress, ReadyForHandover, Rejected)
            var activeHandovers = await Context.Queryable<OpsStoreHandover>()
                .Where(h => h.Status != "Approved")
                .ToListAsync();

            if (activeHandovers.Count == 0) return 0;

            // Get all active requirements
            var activeRequirements = await Context.Queryable<OpsHandoverRequirement>()
                .Where(r => r.IsActive)
                .OrderBy(r => r.SortOrder)
                .ToListAsync();

            var activeReqIds = activeRequirements.Select(r => r.RequirementId).ToHashSet();
            int totalChanges = 0;

            foreach (var handover in activeHandovers)
            {
                // Get existing items for this handover
                var existingItems = await Context.Queryable<OpsStoreHandoverItem>()
                    .Where(i => i.HandoverId == handover.HandoverId)
                    .ToListAsync();

                var existingReqIds = existingItems.Select(i => i.RequirementId).ToHashSet();

                // 1. Add missing items for new requirements
                var missingReqIds = activeReqIds.Except(existingReqIds).ToList();
                if (missingReqIds.Count > 0)
                {
                    var newItems = activeRequirements
                        .Where(r => missingReqIds.Contains(r.RequirementId))
                        .Select(req => new OpsStoreHandoverItem
                        {
                            ItemId = SnowFlakeSingle.Instance.NextId(),
                            HandoverId = handover.HandoverId,
                            RequirementId = req.RequirementId,
                            IsCompleted = false,
                            Create_by = userName,
                            Create_time = DateTime.Now
                        }).ToList();

                    await Context.Insertable(newItems).ExecuteCommandAsync();
                    totalChanges += newItems.Count;
                }

                // 2. Remove uncompleted items whose requirement was deactivated
                var orphanItems = existingItems
                    .Where(i => !activeReqIds.Contains(i.RequirementId) && !i.IsCompleted)
                    .ToList();

                if (orphanItems.Count > 0)
                {
                    var orphanIds = orphanItems.Select(i => i.ItemId).ToList();
                    await Context.Deleteable<OpsStoreHandoverItem>()
                        .Where(i => orphanIds.Contains(i.ItemId))
                        .ExecuteCommandAsync();
                    totalChanges += orphanItems.Count;
                }

                // 3. Recalculate progress if anything changed
                if (missingReqIds.Count > 0 || orphanItems.Count > 0)
                {
                    await RecalculateProgressAsync(handover.HandoverId, userName);
                }
            }

            return totalChanges;
        }

        // ─── Private Helpers ───

        private async Task<List<OpsStoreHandoverItem>> GetHandoverItemsAsync(long handoverId)
        {
            return await Context
                .Queryable<OpsStoreHandoverItem, OpsHandoverRequirement>(
                    (i, r) => new JoinQueryInfos(JoinType.Left, i.RequirementId == r.RequirementId))
                .Where((i, r) => i.HandoverId == handoverId)
                .OrderBy((i, r) => r.SortOrder)
                .Select((i, r) => new OpsStoreHandoverItem
                {
                    ItemId = i.ItemId,
                    HandoverId = i.HandoverId,
                    RequirementId = i.RequirementId,
                    IsCompleted = i.IsCompleted,
                    PhotoUrls = i.PhotoUrls,
                    CompletedAt = i.CompletedAt,
                    CompletedBy = i.CompletedBy,
                    Notes = i.Notes,
                    Create_time = i.Create_time,
                    NameEn = r.NameEn,
                    NameAr = r.NameAr,
                    Percentage = r.Percentage,
                    PhotoRequired = r.PhotoRequired,
                    IsBlocking = r.IsBlocking,
                    Code = r.Code,
                    SortOrder = r.SortOrder
                })
                .ToListAsync();
        }

        private async Task SyncSingleHandoverAsync(long handoverId)
        {
            var activeRequirements = await Context.Queryable<OpsHandoverRequirement>()
                .Where(r => r.IsActive)
                .ToListAsync();

            var existingItems = await Context.Queryable<OpsStoreHandoverItem>()
                .Where(i => i.HandoverId == handoverId)
                .ToListAsync();

            var activeReqIds = activeRequirements.Select(r => r.RequirementId).ToHashSet();
            var existingReqIds = existingItems.Select(i => i.RequirementId).ToHashSet();

            bool changed = false;

            // Add missing items for new requirements
            var missingReqIds = activeReqIds.Except(existingReqIds).ToList();
            if (missingReqIds.Count > 0)
            {
                var newItems = activeRequirements
                    .Where(r => missingReqIds.Contains(r.RequirementId))
                    .Select(req => new OpsStoreHandoverItem
                    {
                        ItemId = SnowFlakeSingle.Instance.NextId(),
                        HandoverId = handoverId,
                        RequirementId = req.RequirementId,
                        IsCompleted = false,
                        Create_by = "system",
                        Create_time = DateTime.Now
                    }).ToList();

                await Context.Insertable(newItems).ExecuteCommandAsync();
                changed = true;
            }

            // Remove uncompleted items whose requirement was deactivated
            var orphanItems = existingItems
                .Where(i => !activeReqIds.Contains(i.RequirementId) && !i.IsCompleted)
                .ToList();

            if (orphanItems.Count > 0)
            {
                var orphanIds = orphanItems.Select(i => i.ItemId).ToList();
                await Context.Deleteable<OpsStoreHandoverItem>()
                    .Where(i => orphanIds.Contains(i.ItemId))
                    .ExecuteCommandAsync();
                changed = true;
            }

            if (changed)
            {
                await RecalculateProgressAsync(handoverId, "system");
            }
        }

        private async Task RecalculateProgressAsync(long handoverId, string userName)
        {
            var progressInfo = await GetProgressAsync(handoverId);

            var handover = await Context.Queryable<OpsStoreHandover>()
                .Where(h => h.HandoverId == handoverId)
                .FirstAsync();

            if (handover == null) return;

            handover.Progress = progressInfo.Progress;

            // Auto-transition status
            if (progressInfo.CanRequestHandover && handover.Status != "Approved")
            {
                handover.Status = "ReadyForHandover";
            }
            else if (progressInfo.Progress > 0 && handover.Status == "Pending")
            {
                handover.Status = "InProgress";
            }
            else if (handover.Status == "Rejected" && progressInfo.Progress > 0)
            {
                handover.Status = "InProgress";
            }

            handover.Update_by = userName;
            handover.Update_time = DateTime.Now;

            await Context.Updateable(handover).ExecuteCommandAsync();
        }
    }
}
