using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Mapster;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto.Attendance;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.Attendance
{
    /// <summary>
    /// HR Leave Type Service - Business logic for leave type management
    /// </summary>
    [AppService(ServiceType = typeof(IHrLeaveTypeService), ServiceLifetime = LifeTime.Transient)]
    public class HrLeaveTypeService : BaseService<HrLeaveType>, IHrLeaveTypeService
    {
        /// <summary>
        /// Get paginated list of leave types
        /// </summary>
        public PagedInfo<HrLeaveTypeDto> GetList(HrLeaveTypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .ToPage<HrLeaveType, HrLeaveTypeDto>(parm);

            return response;
        }

        /// <summary>
        /// Get all active leave types
        /// </summary>
        public async Task<List<HrLeaveTypeDto>> GetActiveTypesAsync()
        {
            var entities = await Queryable()
                .Where(t => t.IsActive)
                .OrderBy(t => t.TypeCode)
                .ToListAsync();

            return entities.Adapt<List<HrLeaveTypeDto>>();
        }

        /// <summary>
        /// Get leave type details by ID
        /// </summary>
        public HrLeaveTypeDto GetInfo(long leaveTypeId)
        {
            var entity = Queryable()
                .Where(x => x.LeaveTypeId == leaveTypeId)
                .First();

            return entity.Adapt<HrLeaveTypeDto>();
        }

        /// <summary>
        /// Add new leave type
        /// </summary>
        public HrLeaveTypeDto AddHrLeaveType(CreateHrLeaveTypeDto dto)
        {
            // Check if type code already exists
            var exists = Context.Queryable<HrLeaveType>()
                .Any(t => t.TypeCode == dto.TypeCode);

            if (exists)
            {
                throw new Exception($"Leave type with code {dto.TypeCode} already exists");
            }

            var entity = dto.Adapt<HrLeaveType>();
            var inserted = Insertable(entity).ExecuteReturnEntity();
            return inserted.Adapt<HrLeaveTypeDto>();
        }

        /// <summary>
        /// Update existing leave type
        /// </summary>
        public int UpdateHrLeaveType(UpdateHrLeaveTypeDto dto)
        {
            // Check if type code already exists for another record
            var exists = Context.Queryable<HrLeaveType>()
                .Any(t => t.TypeCode == dto.TypeCode && t.LeaveTypeId != dto.LeaveTypeId);

            if (exists)
            {
                throw new Exception($"Leave type with code {dto.TypeCode} already exists");
            }

            var entity = dto.Adapt<HrLeaveType>();
            return Update(entity, true, "Update Leave Type");
        }

        /// <summary>
        /// Delete leave type (soft delete if in use, hard delete otherwise)
        /// </summary>
        public async Task<int> DeleteAsync(long leaveTypeId)
        {
            // Check if leave type is being used in leave balances
            var hasBalances = await Context.Queryable<HrLeaveBalance>()
                .AnyAsync(b => b.LeaveTypeId == leaveTypeId);

            if (hasBalances)
            {
                // Soft delete - just mark as inactive
                return await Context.Updateable<HrLeaveType>()
                    .SetColumns(t => t.IsActive == false)
                    .SetColumns(t => t.Update_time == DateTime.Now)
                    .Where(t => t.LeaveTypeId == leaveTypeId)
                    .ExecuteCommandAsync();
            }

            // Hard delete if not used
            return await Context.Deleteable<HrLeaveType>()
                .Where(t => t.LeaveTypeId == leaveTypeId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Truncate leave type table (with backup)
        /// </summary>
        public bool TruncateHrLeaveType()
        {
            var newTableName = $"hr_leave_type_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_leave_type", newTableName);
            }

            return Truncate();
        }

        /// <summary>
        /// Import leave types from list
        /// </summary>
        public (string, object, object) ImportHrLeaveType(List<CreateHrLeaveTypeDto> list)
        {
            // Map DTOs to entities
            var entities = list.Adapt<List<HrLeaveType>>();

            var x = Context.Storageable(entities)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.TypeCode.IsEmpty(), "Type code cannot be empty")
                .SplitError(x => x.Item.TypeName.IsEmpty(), "Type name cannot be empty")
                .SplitError(x => x.Item.TypeNameAr.IsEmpty(), "Type name (Arabic) cannot be empty")
                .ToStorage();

            var result = x.AsInsertable.ExecuteCommand();

            string msg = $"Inserted: {x.InsertList.Count}, Updated: {x.UpdateList.Count}, Errors: {x.ErrorList.Count}, Ignored: {x.IgnoreList.Count}, Deleted: {x.DeleteList.Count}, Total: {x.TotalList.Count}";
            Console.WriteLine(msg);

            // Output error information
            foreach (var item in x.ErrorList)
            {
                Console.WriteLine("Error: " + item.StorageMessage);
            }
            foreach (var item in x.IgnoreList)
            {
                Console.WriteLine("Ignored: " + item.StorageMessage);
            }

            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// Export leave types to list
        /// </summary>
        public PagedInfo<HrLeaveTypeDto> ExportList(HrLeaveTypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrLeaveTypeDto()
                {
                    LeaveTypeId = it.LeaveTypeId,
                    TypeCode = it.TypeCode,
                    TypeName = it.TypeName,
                    TypeNameAr = it.TypeNameAr,
                    DaysPerYear = it.DaysPerYear,
                    IsPaid = it.IsPaid,
                    RequiresApproval = it.RequiresApproval,
                    RequiresDocument = it.RequiresDocument,
                    MinDays = it.MinDays,
                    MaxDays = it.MaxDays,
                    AdvanceNoticeDays = it.AdvanceNoticeDays,
                    IsActive = it.IsActive,
                    LeaveCode = it.LeaveCode,
                    DefaultDays = it.DefaultDays,
                    AllowCarryForward = it.AllowCarryForward,
                    MaxCarryForward = it.MaxCarryForward,
                    CreateBy = it.Create_by,
                    CreateTime = it.Create_time,
                    UpdateBy = it.Update_by,
                    UpdateTime = it.Update_time,
                    Remark = it.Remark
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// Build query expression for filtering
        /// </summary>
        private static Expressionable<HrLeaveType> QueryExp(HrLeaveTypeQueryDto parm)
        {
            var predicate = Expressionable.Create<HrLeaveType>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TypeCode), it => it.TypeCode.Contains(parm.TypeCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TypeName), it => it.TypeName.Contains(parm.TypeName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TypeNameAr), it => it.TypeNameAr.Contains(parm.TypeNameAr));
            predicate = predicate.AndIF(parm.IsPaid.HasValue, it => it.IsPaid == parm.IsPaid.Value);
            predicate = predicate.AndIF(parm.RequiresApproval.HasValue, it => it.RequiresApproval == parm.RequiresApproval.Value);
            predicate = predicate.AndIF(parm.RequiresDocument.HasValue, it => it.RequiresDocument == parm.RequiresDocument.Value);
            predicate = predicate.AndIF(parm.MinDays != null, it => it.MinDays == parm.MinDays);
            predicate = predicate.AndIF(parm.MaxDays != null, it => it.MaxDays == parm.MaxDays);
            predicate = predicate.AndIF(parm.AdvanceNoticeDays != null, it => it.AdvanceNoticeDays == parm.AdvanceNoticeDays);
            predicate = predicate.AndIF(parm.IsActive.HasValue, it => it.IsActive == parm.IsActive.Value);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.LeaveCode), it => it.LeaveCode.Contains(parm.LeaveCode));
            predicate = predicate.AndIF(parm.AllowCarryForward.HasValue, it => it.AllowCarryForward == parm.AllowCarryForward.Value);
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));

            return predicate;
        }
    }
}
