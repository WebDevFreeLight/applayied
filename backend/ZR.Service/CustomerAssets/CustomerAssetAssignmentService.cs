using System;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;
using ZR.Model.CustomerAssets.Enums;
using ZR.Model.Finance.ARAP;
using ZR.Model.Customers;
using ZR.Model.Contracts;
using ZR.Repository;
using ZR.Service.IService.CustomerAssets;

namespace ZR.Service.CustomerAssets
{
    [AppService(ServiceType = typeof(ICustomerAssetAssignmentService), ServiceLifetime = LifeTime.Transient)]
    public class CustomerAssetAssignmentService : BaseService<CustomerAssetAssignment>, ICustomerAssetAssignmentService
    {
        public PagedInfo<AssignmentListDto> GetList(AssignmentQueryDto query, PagerInfo pager)
        {
            var result = Context.Queryable<CustomerAssetAssignment, CustomerAsset, Customer, ErpContract>(
                (a, asset, cust, contract) => new JoinQueryInfos(
                    JoinType.Left, a.AssetId == asset.Id,
                    JoinType.Left, a.CustomerId == cust.Id,
                    JoinType.Left, a.ContractId == contract.ContractId))
                .WhereIF(query.AssetId.HasValue && query.AssetId != Guid.Empty, (a, asset, cust, contract) => a.AssetId == query.AssetId)
                .WhereIF(query.CustomerId.HasValue && query.CustomerId != Guid.Empty, (a, asset, cust, contract) => a.CustomerId == query.CustomerId)
                .WhereIF(query.ContractId.HasValue && query.ContractId != Guid.Empty, (a, asset, cust, contract) => a.ContractId == query.ContractId)
                .WhereIF(query.Status.HasValue, (a, asset, cust, contract) => a.Status == query.Status)
                .WhereIF(!string.IsNullOrEmpty(query.CustomerName), (a, asset, cust, contract) =>
                    cust.NameEn.Contains(query.CustomerName) || cust.NameAr.Contains(query.CustomerName))
                .WhereIF(!string.IsNullOrEmpty(query.AssetCode), (a, asset, cust, contract) => asset.AssetCode.Contains(query.AssetCode))
                .OrderByDescending((a, asset, cust, contract) => a.Create_time)
                .Select((a, asset, cust, contract) => new AssignmentListDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssetCode = asset.AssetCode,
                    AssetNameEn = asset.NameEn,
                    AssetNameAr = asset.NameAr,
                    CustomerId = a.CustomerId,
                    CustomerCode = cust.CustomerCode,
                    CustomerNameEn = cust.NameEn,
                    CustomerNameAr = cust.NameAr,
                    ContractId = a.ContractId,
                    ContractTitle = contract.ContractTitle,
                    AssignedDate = a.AssignedDate,
                    ExpectedReturnDate = a.ExpectedReturnDate,
                    ActualReturnDate = a.ActualReturnDate,
                    Status = a.Status,
                    AssignedBy = a.AssignedBy,
                    Create_time = a.Create_time
                }).ToPage(pager);

            return result;
        }

        public AssignmentDetailDto GetById(Guid id)
        {
            return Context.Queryable<CustomerAssetAssignment, CustomerAsset, Customer, ErpContract, AssetType>(
                (a, asset, cust, contract, atype) => new JoinQueryInfos(
                    JoinType.Left, a.AssetId == asset.Id,
                    JoinType.Left, a.CustomerId == cust.Id,
                    JoinType.Left, a.ContractId == contract.ContractId,
                    JoinType.Left, asset.AssetTypeId == atype.Id))
                .Where((a, asset, cust, contract, atype) => a.Id == id)
                .Select((a, asset, cust, contract, atype) => new AssignmentDetailDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssetCode = asset.AssetCode,
                    AssetNameEn = asset.NameEn,
                    AssetNameAr = asset.NameAr,
                    CustomerId = a.CustomerId,
                    CustomerCode = cust.CustomerCode,
                    CustomerNameEn = cust.NameEn,
                    CustomerNameAr = cust.NameAr,
                    ContractId = a.ContractId,
                    ContractTitle = contract.ContractTitle,
                    AssignedDate = a.AssignedDate,
                    ExpectedReturnDate = a.ExpectedReturnDate,
                    ActualReturnDate = a.ActualReturnDate,
                    Status = a.Status,
                    AssignedBy = a.AssignedBy,
                    ReturnedBy = a.ReturnedBy,
                    ReturnCondition = a.ReturnCondition,
                    Notes = a.Notes,
                    AssetTypeName = atype.NameEn,
                    SerialNumber = asset.SerialNumber,
                    Create_time = a.Create_time
                }).First();
        }

        public CustomerAssetAssignment AssignAsset(AssignAssetDto dto, string userName)
        {
            var result = Context.Ado.UseTran(() =>
            {
                // Verify asset is available
                var asset = Context.Queryable<CustomerAsset>()
                    .Where(x => x.Id == dto.AssetId && x.CurrentStatus == InventoryAssetStatus.Available)
                    .First();
                if (asset == null)
                    throw new Exception("Asset is not available for assignment - الأصل غير متاح للتعيين");

                // Create assignment
                var assignment = new CustomerAssetAssignment
                {
                    Id = Guid.NewGuid(),
                    AssetId = dto.AssetId,
                    CustomerId = dto.CustomerId,
                    ContractId = dto.ContractId,
                    AssignedDate = dto.AssignedDate,
                    ExpectedReturnDate = dto.ExpectedReturnDate,
                    Status = AssignmentStatus.Active,
                    AssignedBy = userName,
                    Notes = dto.Notes,
                    Create_by = userName
                };
                Context.Insertable(assignment).ExecuteCommand();

                // Update asset status to Assigned
                Context.Updateable<CustomerAsset>()
                    .SetColumns(x => x.CurrentStatus == InventoryAssetStatus.Assigned)
                    .SetColumns(x => x.Update_by == userName)
                    .Where(x => x.Id == dto.AssetId)
                    .ExecuteCommand();

                return assignment;
            });

            if (!result.IsSuccess)
                throw new Exception(result.ErrorMessage);

            return result.Data;
        }

        public int ReturnAsset(ReturnAssetDto dto, string userName)
        {
            var result = Context.Ado.UseTran(() =>
            {
                // Verify assignment is active
                var assignment = Context.Queryable<CustomerAssetAssignment>()
                    .Where(x => x.Id == dto.AssignmentId && x.Status == AssignmentStatus.Active)
                    .First();
                if (assignment == null)
                    throw new Exception("Assignment is not active - التعيين غير نشط");

                // Update assignment
                Context.Updateable<CustomerAssetAssignment>()
                    .SetColumns(x => x.Status == AssignmentStatus.Returned)
                    .SetColumns(x => x.ActualReturnDate == dto.ActualReturnDate)
                    .SetColumns(x => x.ReturnCondition == dto.ReturnCondition)
                    .SetColumns(x => x.ReturnedBy == userName)
                    .SetColumns(x => x.Notes == dto.Notes)
                    .SetColumns(x => x.Update_by == userName)
                    .Where(x => x.Id == dto.AssignmentId)
                    .ExecuteCommand();

                // Update asset status based on return condition
                var newStatus = dto.ReturnCondition == AssetCondition.Damaged
                    ? InventoryAssetStatus.Damaged
                    : InventoryAssetStatus.Available;
                var newCondition = dto.ReturnCondition;

                Context.Updateable<CustomerAsset>()
                    .SetColumns(x => x.CurrentStatus == newStatus)
                    .SetColumns(x => x.CurrentCondition == newCondition)
                    .SetColumns(x => x.Update_by == userName)
                    .Where(x => x.Id == assignment.AssetId)
                    .ExecuteCommand();
            });

            return result.IsSuccess ? 1 : 0;
        }

        public int MarkLost(MarkLostDamagedDto dto, string userName)
        {
            var result = Context.Ado.UseTran(() =>
            {
                var assignment = Context.Queryable<CustomerAssetAssignment>()
                    .Where(x => x.Id == dto.AssignmentId && x.Status == AssignmentStatus.Active)
                    .First();
                if (assignment == null)
                    throw new Exception("Assignment is not active - التعيين غير نشط");

                Context.Updateable<CustomerAssetAssignment>()
                    .SetColumns(x => x.Status == AssignmentStatus.Lost)
                    .SetColumns(x => x.Notes == dto.Notes)
                    .SetColumns(x => x.Update_by == userName)
                    .Where(x => x.Id == dto.AssignmentId)
                    .ExecuteCommand();

                Context.Updateable<CustomerAsset>()
                    .SetColumns(x => x.CurrentStatus == InventoryAssetStatus.Lost)
                    .SetColumns(x => x.Update_by == userName)
                    .Where(x => x.Id == assignment.AssetId)
                    .ExecuteCommand();
            });

            return result.IsSuccess ? 1 : 0;
        }

        public int MarkDamaged(MarkLostDamagedDto dto, string userName)
        {
            var result = Context.Ado.UseTran(() =>
            {
                var assignment = Context.Queryable<CustomerAssetAssignment>()
                    .Where(x => x.Id == dto.AssignmentId && x.Status == AssignmentStatus.Active)
                    .First();
                if (assignment == null)
                    throw new Exception("Assignment is not active - التعيين غير نشط");

                Context.Updateable<CustomerAssetAssignment>()
                    .SetColumns(x => x.Status == AssignmentStatus.Damaged)
                    .SetColumns(x => x.Notes == dto.Notes)
                    .SetColumns(x => x.Update_by == userName)
                    .Where(x => x.Id == dto.AssignmentId)
                    .ExecuteCommand();

                Context.Updateable<CustomerAsset>()
                    .SetColumns(x => x.CurrentStatus == InventoryAssetStatus.Damaged)
                    .SetColumns(x => x.CurrentCondition == AssetCondition.Damaged)
                    .SetColumns(x => x.Update_by == userName)
                    .Where(x => x.Id == assignment.AssetId)
                    .ExecuteCommand();
            });

            return result.IsSuccess ? 1 : 0;
        }
    }
}
