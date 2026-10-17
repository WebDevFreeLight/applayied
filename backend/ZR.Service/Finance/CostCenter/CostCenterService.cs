using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.CostCenter.Dto;
using ZR.Repository.Finance.CostCenter;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance;
using CostCenterEntity = ZR.Model.Finance.CostCenter.CostCenter;

namespace ZR.Service.Finance.CostCenter
{
    /// <summary>
    /// Cost Center Service - خدمة مراكز التكلفة
    /// </summary>
    [AppService(ServiceType = typeof(ICostCenterService), ServiceLifetime = LifeTime.Transient)]
    public class CostCenterService : BaseService<CostCenterEntity>, ICostCenterService
    {
        private readonly ICostCenterRepository _costCenterRepository;
        private readonly IGLJournalEntryRepository _glEntryRepository;

        public CostCenterService(
            ICostCenterRepository costCenterRepository,
            IGLJournalEntryRepository glEntryRepository)
        {
            _costCenterRepository = costCenterRepository;
            _glEntryRepository = glEntryRepository;
        }

        /// <summary>
        /// Get paginated list of cost centers
        /// </summary>
        public PagedInfo<CostCenterListDto> GetList(CostCenterQueryDto query, PagerInfo pager)
        {
            var searchModel = new CostCenterEntity
            {
                CostCenterCode = query.CostCenterCode,
                CostCenterName = query.CostCenterName,
                CostCenterType = query.CostCenterType,
                ParentId = query.ParentId
            };

            var result = _costCenterRepository.GetList(searchModel, pager);
            var allCostCenters = _costCenterRepository.GetAllForTree();

            var dtoList = result.Result.Select(x => MapToListDto(x, allCostCenters)).ToList();

            return new PagedInfo<CostCenterListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get cost center by ID
        /// </summary>
        public CostCenterDetailDto GetById(long costCenterId)
        {
            var costCenter = _costCenterRepository.GetById(costCenterId);
            if (costCenter == null) return null;

            var allCostCenters = _costCenterRepository.GetAllForTree();
            return MapToDetailDto(costCenter, allCostCenters);
        }

        /// <summary>
        /// Create cost center
        /// </summary>
        public CostCenterEntity Create(CreateCostCenterDto dto, string userName)
        {
            // Check if code exists
            if (_costCenterRepository.CodeExists(dto.CostCenterCode))
            {
                throw new AppException(ExceptionCode.FINANCE_COSTCENTER_CODE_DUPLICATE)
                    .WithParameter("Code", dto.CostCenterCode);
            }

            var model = dto.Adapt<CostCenterEntity>();
            model.CostCenterId = SnowFlakeSingle.Instance.NextId();
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            // Calculate level based on parent
            if (dto.ParentId.HasValue && dto.ParentId.Value > 0)
            {
                var parent = _costCenterRepository.GetById(dto.ParentId.Value);
                if (parent != null)
                {
                    model.Level = parent.Level + 1;
                    // Update parent's IsParent flag
                    parent.IsParent = true;
                    _costCenterRepository.Update(parent);
                }
            }
            else
            {
                model.Level = 1;
                model.ParentId = null;
            }

            model.IsParent = false;

            _costCenterRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Update cost center
        /// </summary>
        public int Update(UpdateCostCenterDto dto, string userName)
        {
            // Check if code exists (excluding current)
            if (_costCenterRepository.CodeExists(dto.CostCenterCode, dto.CostCenterId))
            {
                throw new AppException(ExceptionCode.FINANCE_COSTCENTER_CODE_DUPLICATE)
                    .WithParameter("Code", dto.CostCenterCode);
            }

            // Prevent circular reference
            if (dto.ParentId == dto.CostCenterId)
            {
                throw new AppException(ExceptionCode.FINANCE_COSTCENTER_CIRCULAR_REFERENCE)
                    .WithParameter("CostCenterId", dto.CostCenterId);
            }

            var existing = _costCenterRepository.GetById(dto.CostCenterId);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_COSTCENTER_NOT_FOUND)
                    .WithParameter("CostCenterId", dto.CostCenterId);
            }

            // Check if trying to move under its own child
            if (dto.ParentId.HasValue && IsDescendant(dto.CostCenterId, dto.ParentId.Value))
            {
                throw new AppException(ExceptionCode.FINANCE_COSTCENTER_MOVE_TO_DESCENDANT)
                    .WithParameter("CostCenterId", dto.CostCenterId);
            }

            var oldParentId = existing.ParentId;

            existing.CostCenterCode = dto.CostCenterCode;
            existing.CostCenterName = dto.CostCenterName;
            existing.CostCenterNameAr = dto.CostCenterNameAr;
            existing.CostCenterType = dto.CostCenterType;
            existing.Description = dto.Description;
            existing.IsActive = dto.IsActive;
            existing.Update_by = userName;
            existing.Update_time = DateTime.Now;

            // Handle parent change
            if (dto.ParentId != oldParentId)
            {
                existing.ParentId = dto.ParentId;

                // Recalculate level
                if (dto.ParentId.HasValue && dto.ParentId.Value > 0)
                {
                    var newParent = _costCenterRepository.GetById(dto.ParentId.Value);
                    if (newParent != null)
                    {
                        existing.Level = newParent.Level + 1;
                        newParent.IsParent = true;
                        _costCenterRepository.Update(newParent);
                    }
                }
                else
                {
                    existing.Level = 1;
                    existing.ParentId = null;
                }

                // Update old parent's IsParent flag if needed
                if (oldParentId.HasValue)
                {
                    var oldParent = _costCenterRepository.GetById(oldParentId.Value);
                    if (oldParent != null && !_costCenterRepository.HasChildren(oldParentId.Value))
                    {
                        oldParent.IsParent = false;
                        _costCenterRepository.Update(oldParent);
                    }
                }
            }

            return _costCenterRepository.Update(existing);
        }

        /// <summary>
        /// Delete cost centers
        /// </summary>
        public int Delete(long[] ids)
        {
            // Check if any has children
            foreach (var id in ids)
            {
                if (_costCenterRepository.HasChildren(id))
                {
                    throw new AppException(ExceptionCode.FINANCE_COSTCENTER_HAS_CHILDREN)
                        .WithParameter("CostCenterId", id);
                }
            }

            // TODO: Check if cost center is used in transactions

            var count = 0;
            foreach (var id in ids)
            {
                var costCenter = _costCenterRepository.GetById(id);
                if (costCenter != null)
                {
                    count += _costCenterRepository.Delete(costCenter);

                    // Update parent's IsParent flag if needed
                    if (costCenter.ParentId.HasValue &&
                        !_costCenterRepository.HasChildren(costCenter.ParentId.Value))
                    {
                        var parent = _costCenterRepository.GetById(costCenter.ParentId.Value);
                        if (parent != null)
                        {
                            parent.IsParent = false;
                            _costCenterRepository.Update(parent);
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Get active cost centers for dropdown
        /// </summary>
        public List<CostCenterSelectItemDto> GetActiveCostCenters(long? branchId = null)
        {
            var costCenters = _costCenterRepository.GetActiveCostCenters(branchId);
            return costCenters.Select(x => new CostCenterSelectItemDto
            {
                CostCenterId = x.CostCenterId,
                CostCenterCode = x.CostCenterCode,
                CostCenterName = x.CostCenterName,
                CostCenterNameAr = x.CostCenterNameAr,
                CostCenterType = x.CostCenterType,
                ParentId = x.ParentId,
                Level = x.Level
            }).ToList();
        }

        /// <summary>
        /// Get cost centers by type
        /// </summary>
        public List<CostCenterSelectItemDto> GetCostCentersByType(string costCenterType)
        {
            var costCenters = _costCenterRepository.GetByType(costCenterType);
            return costCenters.Select(x => new CostCenterSelectItemDto
            {
                CostCenterId = x.CostCenterId,
                CostCenterCode = x.CostCenterCode,
                CostCenterName = x.CostCenterName,
                CostCenterNameAr = x.CostCenterNameAr,
                CostCenterType = x.CostCenterType,
                ParentId = x.ParentId,
                Level = x.Level
            }).ToList();
        }

        /// <summary>
        /// Get cost center tree
        /// </summary>
        public List<CostCenterListDto> GetCostCenterTree()
        {
            var allCostCenters = _costCenterRepository.GetAllForTree();
            var rootItems = allCostCenters.Where(x => !x.ParentId.HasValue || x.ParentId == 0).ToList();

            return rootItems.Select(x => BuildTreeNode(x, allCostCenters)).ToList();
        }

        /// <summary>
        /// Get cost center balance
        /// Note: Budget tracking has been moved to the Budget module.
        /// This method now returns basic cost center info with zero budget values.
        /// Use the Budget module for budget vs actual tracking.
        /// </summary>
        public CostCenterBalanceDto GetCostCenterBalance(long costCenterId, DateTime? asOfDate = null)
        {
            var costCenter = _costCenterRepository.GetById(costCenterId);
            if (costCenter == null)
            {
                throw new AppException(ExceptionCode.FINANCE_COSTCENTER_NOT_FOUND)
                    .WithParameter("CostCenterId", costCenterId);
            }

            var date = asOfDate ?? DateTime.Now;

            // Budget tracking has been moved to the Budget module
            // Return zeros here - use Budget module for budget vs actual tracking
            return new CostCenterBalanceDto
            {
                CostCenterId = costCenterId,
                CostCenterCode = costCenter.CostCenterCode,
                CostCenterName = costCenter.CostCenterName,
                BudgetAmount = 0,
                ActualAmount = 0,
                Variance = 0,
                VariancePercentage = 0,
                AsOfDate = date
            };
        }

        /// <summary>
        /// Get cost center report
        /// Note: Budget tracking has been moved to the Budget module.
        /// This method now returns basic cost center info with zero budget values.
        /// Use the Budget module for budget vs actual tracking.
        /// </summary>
        public List<CostCenterReportItemDto> GetCostCenterReport(DateTime fromDate, DateTime toDate)
        {
            var costCenters = _costCenterRepository.GetActiveCostCenters();
            var result = new List<CostCenterReportItemDto>();

            foreach (var cc in costCenters)
            {
                // Budget tracking has been moved to the Budget module
                // Return zeros here - use Budget module for budget vs actual tracking
                result.Add(new CostCenterReportItemDto
                {
                    CostCenterId = cc.CostCenterId,
                    CostCenterCode = cc.CostCenterCode,
                    CostCenterName = cc.CostCenterName,
                    CostCenterNameAr = cc.CostCenterNameAr,
                    CostCenterType = cc.CostCenterType,
                    BudgetAmount = 0,
                    TotalDebit = 0,
                    TotalCredit = 0,
                    NetAmount = 0,
                    Variance = 0,
                    UtilizationPercentage = 0
                });
            }

            return result.OrderBy(x => x.CostCenterCode).ToList();
        }

        /// <summary>
        /// Check if cost center code exists
        /// </summary>
        public bool CodeExists(string code, long? excludeId = null)
        {
            return _costCenterRepository.CodeExists(code, excludeId);
        }

        /// <summary>
        /// Get next cost center code
        /// </summary>
        public string GetNextCode(string prefix)
        {
            return _costCenterRepository.GetNextCode(prefix);
        }

        #region Private Helper Methods

        private CostCenterListDto MapToListDto(CostCenterEntity model, List<CostCenterEntity> allCostCenters)
        {
            var dto = model.Adapt<CostCenterListDto>();

            if (model.ParentId.HasValue)
            {
                var parent = allCostCenters.FirstOrDefault(x => x.CostCenterId == model.ParentId.Value);
                dto.ParentName = parent?.CostCenterName;
            }

            return dto;
        }

        private CostCenterDetailDto MapToDetailDto(CostCenterEntity model, List<CostCenterEntity> allCostCenters)
        {
            var dto = model.Adapt<CostCenterDetailDto>();

            if (model.ParentId.HasValue)
            {
                var parent = allCostCenters.FirstOrDefault(x => x.CostCenterId == model.ParentId.Value);
                dto.ParentName = parent?.CostCenterName;
            }

            dto.CreateBy = model.Create_by;
            dto.CreateTime = model.Create_time;
            dto.UpdateBy = model.Update_by;
            dto.UpdateTime = model.Update_time;

            return dto;
        }

        private CostCenterListDto BuildTreeNode(CostCenterEntity model, List<CostCenterEntity> allCostCenters)
        {
            var dto = MapToListDto(model, allCostCenters);
            var children = allCostCenters.Where(x => x.ParentId == model.CostCenterId).ToList();

            dto.Children = children.Count > 0
                ? children.Select(x => BuildTreeNode(x, allCostCenters)).ToList()
                : null;

            return dto;
        }

        private bool IsDescendant(long costCenterId, long potentialDescendantId)
        {
            var children = _costCenterRepository.GetChildren(costCenterId);
            if (children.Any(x => x.CostCenterId == potentialDescendantId))
                return true;

            foreach (var child in children)
            {
                if (IsDescendant(child.CostCenterId, potentialDescendantId))
                    return true;
            }

            return false;
        }

        #endregion
    }
}
