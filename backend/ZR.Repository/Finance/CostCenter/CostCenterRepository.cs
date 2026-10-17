using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using CostCenterEntity = ZR.Model.Finance.CostCenter.CostCenter;

namespace ZR.Repository.Finance.CostCenter
{
    /// <summary>
    /// Cost Center Repository - مستودع مراكز التكلفة
    /// </summary>
    [AppService(ServiceType = typeof(ICostCenterRepository), ServiceLifetime = LifeTime.Transient)]
    public class CostCenterRepository : BaseRepository<CostCenterEntity>, ICostCenterRepository
    {
        /// <summary>
        /// Get paginated list of cost centers
        /// </summary>
        public PagedInfo<CostCenterEntity> GetList(CostCenterEntity parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<CostCenterEntity>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CostCenterCode),
                it => it.CostCenterCode.Contains(parm.CostCenterCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CostCenterName),
                it => it.CostCenterName.Contains(parm.CostCenterName) ||
                      it.CostCenterNameAr.Contains(parm.CostCenterName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CostCenterType),
                it => it.CostCenterType == parm.CostCenterType);
            predicate = predicate.AndIF(parm.ParentId.HasValue,
                it => it.ParentId == parm.ParentId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.CostCenterCode)
                .ToPage(pager);
        }

        /// <summary>
        /// Get cost center by ID
        /// </summary>
        public CostCenterEntity GetById(long costCenterId)
        {
            return GetFirst(x => x.CostCenterId == costCenterId);
        }

        /// <summary>
        /// Get cost center by code
        /// </summary>
        public CostCenterEntity GetByCode(string code)
        {
            return GetFirst(x => x.CostCenterCode == code);
        }

        /// <summary>
        /// Check if cost center code exists
        /// </summary>
        public bool CodeExists(string code, long? excludeId = null)
        {
            var predicate = Expressionable.Create<CostCenterEntity>();
            predicate = predicate.And(it => it.CostCenterCode == code);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.CostCenterId != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Get all active cost centers
        /// </summary>
        public List<CostCenterEntity> GetActiveCostCenters(long? branchId = null)
        {
            var predicate = Expressionable.Create<CostCenterEntity>();
            predicate = predicate.And(it => it.IsActive);
            predicate = predicate.AndIF(branchId.HasValue,
                it => it.CostCenterType == "branch" && it.CostCenterId == branchId.Value);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Level)
                .OrderBy(x => x.CostCenterCode)
                .ToList();
        }

        /// <summary>
        /// Get cost centers by type
        /// </summary>
        public List<CostCenterEntity> GetByType(string costCenterType)
        {
            return Queryable()
                .Where(x => x.CostCenterType == costCenterType && x.IsActive)
                .OrderBy(x => x.CostCenterCode)
                .ToList();
        }

        /// <summary>
        /// Get children of a cost center
        /// </summary>
        public List<CostCenterEntity> GetChildren(long parentId)
        {
            return Queryable()
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.CostCenterCode)
                .ToList();
        }

        /// <summary>
        /// Get all cost centers for tree
        /// </summary>
        public List<CostCenterEntity> GetAllForTree()
        {
            return Queryable()
                .OrderBy(x => x.Level)
                .OrderBy(x => x.CostCenterCode)
                .ToList();
        }

        /// <summary>
        /// Check if cost center has children
        /// </summary>
        public bool HasChildren(long costCenterId)
        {
            return Any(x => x.ParentId == costCenterId);
        }

        /// <summary>
        /// Get next available code
        /// </summary>
        public string GetNextCode(string prefix)
        {
            var maxCode = Queryable()
                .Where(x => x.CostCenterCode.StartsWith(prefix))
                .OrderByDescending(x => x.CostCenterCode)
                .Select(x => x.CostCenterCode)
                .First();

            if (string.IsNullOrEmpty(maxCode))
            {
                return $"{prefix}001";
            }

            // Extract number part and increment
            var numPart = maxCode.Substring(prefix.Length);
            if (int.TryParse(numPart, out int num))
            {
                return $"{prefix}{(num + 1):D3}";
            }

            return $"{prefix}001";
        }
    }
}
