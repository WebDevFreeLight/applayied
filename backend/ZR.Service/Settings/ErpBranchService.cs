using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Settings;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.Sales;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Settings.IService;

namespace ZR.Service.Settings
{
    /// <summary>
    /// Branch Service - خدمة الفروع
    /// </summary>
    [AppService(ServiceType = typeof(IErpBranchService), ServiceLifetime = LifeTime.Transient)]
    public class ErpBranchService : BaseService<ErpBranch>, IErpBranchService
    {
        public PagedInfo<ErpBranch> GetList(ErpBranch parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpBranch>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.BranchName), it => it.BranchName.Contains(parm.BranchName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.BranchCode), it => it.BranchCode.Contains(parm.BranchCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.City), it => it.City == parm.City);
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.BranchName).ToPage(pager);
        }

        public List<ErpBranch> GetAllBranches()
        {
            return Queryable()
                .Where(x => x.IsActive == true)
                .OrderByDescending(x => x.IsMain)
                .OrderBy(x => x.BranchName)
                .ToList();
        }

        public ErpBranch GetInfo(long branchId)
        {
            return GetFirst(x => x.BranchID == branchId);
        }

        public ErpBranch AddBranch(ErpBranch model)
        {
            model.BranchID = SnowFlakeSingle.Instance.NextId();
            model.Create_time = DateTime.Now;

            // Generate branch code if not provided
            if (string.IsNullOrEmpty(model.BranchCode))
            {
                var count = Count(x => x.IsActive);
                model.BranchCode = $"BR{(count + 1):D3}";
            }

            Insert(model);
            return model;
        }

        public int UpdateBranch(ErpBranch model)
        {
            return Update(model, it => new
            {
                it.BranchName,
                it.BranchNameAr,
                it.BranchCode,
                it.BranchType,
                it.Address,
                it.City,
                it.Country,
                it.PostalCode,
                it.Phone,
                it.Email,
                it.ManagerId,
                it.DefaultWarehouseId,
                it.DefaultTreasuryId,
                it.OpeningTime,
                it.ClosingTime,
                it.WorkingDays,
                it.AutoShift,
                it.AutoDailyReport,
                it.LogoUrl,
                it.Latitude,
                it.Longitude,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.BranchID == model.BranchID);
        }

        public int Delete(long[] ids)
        {
            // Don't allow deleting main branch
            var mainBranch = GetMainBranch();
            if (mainBranch != null && ids.Contains(mainBranch.BranchID))
                return 0;

            return Context.Updateable<ErpBranch>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.BranchID))
                .ExecuteCommand();
        }

        public ErpBranch GetMainBranch()
        {
            return GetFirst(x => x.IsMain == true && x.IsActive == true);
        }

        public List<ErpBranch> GetActiveBranches()
        {
            return Queryable()
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.BranchName)
                .ToList();
        }

        public int SetMainBranch(long branchId)
        {
            return Context.Ado.UseTran(() =>
            {
                // Remove main status from all branches
                Context.Updateable<ErpBranch>()
                    .SetColumns(it => it.IsMain == false)
                    .Where(it => it.IsMain == true)
                    .ExecuteCommand();

                // Set the new main branch
                return Context.Updateable<ErpBranch>()
                    .SetColumns(it => it.IsMain == true)
                    .Where(it => it.BranchID == branchId)
                    .ExecuteCommand();
            }).Data;
        }

        public int ToggleStatus(long branchId)
        {
            var branch = GetInfo(branchId);
            if (branch == null) return 0;

            // Don't allow deactivating main branch
            if (branch.IsMain && branch.IsActive)
                return 0;

            return Context.Updateable<ErpBranch>()
                .SetColumns(it => it.IsActive == !branch.IsActive)
                .Where(it => it.BranchID == branchId)
                .ExecuteCommand();
        }

        public List<HrEmployeeCore> GetBranchEmployees(long branchId)
        {
            // Get employees linked to this branch via department assignment
            return Context.Queryable<HrEmployeeCore, HrEmployeeAssignment>((emp, assign) =>
                emp.EmployeeId == assign.EmployeeId)
                .Where((emp, assign) => assign.DepartmentId == branchId && emp.IsActive)
                .Select((emp, assign) => emp)
                .ToList();
        }

        public object GetBranchStatistics(long branchId)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Today's sales
            var todaySales = Context.Queryable<ErpSalesInvoice>()
                .Where(x => x.BranchID == branchId)
                .Where(x => x.InvoiceDate >= today && x.InvoiceDate < today.AddDays(1))
                .Where(x => x.Status != "cancelled")
                .Sum(x => x.TotalAmount);

            // Month's sales
            var monthSales = Context.Queryable<ErpSalesInvoice>()
                .Where(x => x.BranchID == branchId)
                .Where(x => x.InvoiceDate >= monthStart && x.InvoiceDate < today.AddDays(1))
                .Where(x => x.Status != "cancelled")
                .Sum(x => x.TotalAmount);

            // Active members (all members since no branch field)
            var activeMembers = Context.Queryable<ErpMember>()
                .Where(x => x.IsActive)
                .Count();

            // Today's visits
            var todayVisits = Context.Queryable<ErpMemberVisit>()
                .Where(x => x.CheckInTime >= today && x.CheckInTime < today.AddDays(1))
                .Count();

            // Employees count linked to this branch via department
            var employeeCount = Context.Queryable<HrEmployeeCore, HrEmployeeAssignment>((emp, assign) =>
                emp.EmployeeId == assign.EmployeeId)
                .Where((emp, assign) => assign.DepartmentId == branchId && emp.IsActive)
                .Count();

            return new
            {
                BranchId = branchId,
                TodaySales = todaySales,
                MonthSales = monthSales,
                ActiveMembers = activeMembers,
                TodayVisits = todayVisits,
                EmployeeCount = employeeCount,
                AsOf = DateTime.Now
            };
        }
    }
}

