using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Model.Customers.Leads;
using ZR.Model.Sales;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.Audit;
using ZR.Repository;

namespace ZR.Service.Operations
{
    [AppService(ServiceType = typeof(ISiteVisitService), ServiceLifetime = LifeTime.Transient)]
    public class SiteVisitService : BaseService<SiteVisit>, ISiteVisitService
    {
        public async Task<long?> GetEmployeeIdByUserIdAsync(long userId)
        {
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.UserId.HasValue && e.UserId.Value == userId)
                .FirstAsync();
            return employee?.EmployeeId;
        }

        public async Task<PagedInfo<SiteVisit>> GetVisitsListAsync(SiteVisitQueryDto query)
        {
            long? filterStaffId = null;

            if (!query.IsCurrentUserAdmin && query.CurrentUserId.HasValue)
            {
                filterStaffId = await GetEmployeeIdByUserIdAsync(query.CurrentUserId.Value);
                if (!filterStaffId.HasValue)
                {
                    return new PagedInfo<SiteVisit>
                    {
                        Result = new List<SiteVisit>(),
                        TotalNum = 0,
                        PageIndex = query.PageNum,
                        PageSize = query.PageSize
                    };
                }
            }

            var now = DateTime.Now;
            var pageQuery = Context
                .Queryable<SiteVisit, BpLead, HrEmployeeCore, HrPersonName, SalesDeal>(
                    (v, lead, emp, name, deal) => new JoinQueryInfos(
                        JoinType.Left, v.LeadId == lead.LeadId,
                        JoinType.Left, v.AssignedStaffId == emp.EmployeeId,
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now),
                        JoinType.Left, v.DealId == deal.DealId))
                .WhereIF(filterStaffId.HasValue, (v, lead, emp, name, deal) => v.AssignedStaffId == filterStaffId.Value)
                .WhereIF(query.LeadId.HasValue, (v, lead, emp, name, deal) => v.LeadId == query.LeadId.Value)
                .WhereIF(query.DealId.HasValue, (v, lead, emp, name, deal) => v.DealId == query.DealId.Value)
                .WhereIF(!string.IsNullOrEmpty(query.VisitStatus), (v, lead, emp, name, deal) => v.VisitStatus == query.VisitStatus)
                .WhereIF(query.AssignedStaffId.HasValue && !filterStaffId.HasValue, (v, lead, emp, name, deal) => v.AssignedStaffId == query.AssignedStaffId.Value)
                .WhereIF(query.StartDate.HasValue, (v, lead, emp, name, deal) => v.ScheduledDate >= query.StartDate.Value)
                .WhereIF(query.EndDate.HasValue, (v, lead, emp, name, deal) => v.ScheduledDate <= query.EndDate.Value)
                .Select((v, lead, emp, name, deal) => new SiteVisit
                {
                    VisitId = v.VisitId,
                    LeadId = v.LeadId,
                    LeadName = lead.BrandName,
                    DealId = v.DealId,
                    DealTitle = deal.DealTitle,
                    ScheduledDate = v.ScheduledDate,
                    ScheduledTime = v.ScheduledTime,
                    AssignedStaffId = v.AssignedStaffId,
                    AssignedStaffName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    VisitStatus = v.VisitStatus,
                    VisitNotes = v.VisitNotes,
                    CompletionNotes = v.CompletionNotes,
                    CompletedDate = v.CompletedDate,
                    CancellationReason = v.CancellationReason,
                    Create_by = v.Create_by,
                    Create_time = v.Create_time,
                    Update_by = v.Update_by,
                    Update_time = v.Update_time
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<SiteVisit> GetVisitByIdAsync(long visitId)
        {
            var now = DateTime.Now;
            return await Context
                .Queryable<SiteVisit, BpLead, HrEmployeeCore, HrPersonName, SalesDeal>(
                    (v, lead, emp, name, deal) => new JoinQueryInfos(
                        JoinType.Left, v.LeadId == lead.LeadId,
                        JoinType.Left, v.AssignedStaffId == emp.EmployeeId,
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now),
                        JoinType.Left, v.DealId == deal.DealId))
                .Where((v, lead, emp, name, deal) => v.VisitId == visitId)
                .Select((v, lead, emp, name, deal) => new SiteVisit
                {
                    VisitId = v.VisitId,
                    LeadId = v.LeadId,
                    LeadName = lead.BrandName,
                    DealId = v.DealId,
                    DealTitle = deal.DealTitle,
                    DealStatus = deal.DealStatus,
                    ScheduledDate = v.ScheduledDate,
                    ScheduledTime = v.ScheduledTime,
                    AssignedStaffId = v.AssignedStaffId,
                    AssignedStaffName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    VisitStatus = v.VisitStatus,
                    VisitNotes = v.VisitNotes,
                    CompletionNotes = v.CompletionNotes,
                    CompletedDate = v.CompletedDate,
                    CancellationReason = v.CancellationReason,
                    Create_by = v.Create_by,
                    Create_time = v.Create_time,
                    Update_by = v.Update_by,
                    Update_time = v.Update_time,
                    // Full lead details
                    OwnerName = lead.OwnerName,
                    OwnerPhone = lead.OwnerPhone,
                    OwnerEmail = lead.OwnerEmail,
                    LeadStatus = lead.LeadStatus,
                    LeadSource = lead.LeadSource,
                    Priority = lead.Priority,
                    BudgetRange = lead.BudgetRange,
                    QualificationNotes = lead.QualificationNotes,
                    InternalNotes = lead.InternalNotes,
                    BrandImage = lead.BrandImage
                })
                .FirstAsync();
        }

        public async Task<List<SiteVisit>> GetVisitsByLeadAsync(long leadId)
        {
            return await GetListAsync(v => v.LeadId == leadId);
        }

        public async Task<List<SiteVisit>> GetTodayVisitsAsync(long? staffId)
        {
            var today = DateTime.Today;
            var query = Context.Queryable<SiteVisit>()
                .Where(v => v.ScheduledDate == today);

            if (staffId.HasValue)
                query = query.Where(v => v.AssignedStaffId == staffId.Value);

            return await query.OrderBy(v => v.ScheduledTime).ToListAsync();
        }

        public async Task<List<SiteVisit>> GetUpcomingVisitsAsync(int days, long? staffId)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(days);
            var query = Context.Queryable<SiteVisit>()
                .Where(v => v.ScheduledDate >= today && v.ScheduledDate <= endDate)
                .Where(v => v.VisitStatus == "Scheduled");

            if (staffId.HasValue)
                query = query.Where(v => v.AssignedStaffId == staffId.Value);

            return await query.OrderBy(v => v.ScheduledDate).OrderBy(v => v.ScheduledTime).ToListAsync();
        }

        public async Task<SiteVisit> CreateVisitAsync(SiteVisit visit)
        {
            var lead = await Context.Queryable<BpLead>()
                .Where(l => l.LeadId == visit.LeadId)
                .Select(l => new BpLead { LeadId = l.LeadId, ReadyForOperations = l.ReadyForOperations })
                .FirstAsync();

            if (lead == null)
                throw new Exception("Lead not found");

            // Auto-set readyForOperations when a visit is created
            if (!lead.ReadyForOperations)
            {
                await Context.Updateable<BpLead>()
                    .SetColumns(l => new BpLead { ReadyForOperations = true })
                    .Where(l => l.LeadId == visit.LeadId)
                    .ExecuteCommandAsync();
            }

            visit.VisitId = SnowFlakeSingle.Instance.NextId();
            visit.Create_time = DateTime.Now;
            visit.VisitStatus = "Scheduled";

            Add(visit);
            return await GetVisitByIdAsync(visit.VisitId);
        }

        public async Task<int> UpdateVisitAsync(SiteVisit visit)
        {
            visit.Update_time = DateTime.Now;
            return await Task.FromResult(Update(visit, true));
        }

        public async Task<int> UpdateVisitStatusAsync(long visitId, string newStatus, string notes)
        {
            var visit = await GetByIdAsync(visitId);
            if (visit == null)
                throw new Exception("Visit not found");

            var oldStatus = visit.VisitStatus;

            var tranResult = await UseTranAsync(async () =>
            {
                visit.VisitStatus = newStatus;
                visit.Update_time = DateTime.Now;

                if (newStatus == "Completed")
                {
                    visit.CompletedDate = DateTime.Now;
                    visit.CompletionNotes = notes;
                }
                else if (newStatus == "Cancelled")
                {
                    visit.CancellationReason = notes;
                }

                Update(visit, true);

                // Cross-department: auto-update linked deal notes when visit is completed or cancelled
                if (visit.DealId.HasValue && visit.DealId.Value > 0
                    && (newStatus == "Completed" || newStatus == "Cancelled"))
                {
                    var deal = await Context.Queryable<SalesDeal>()
                        .Where(d => d.DealId == visit.DealId.Value)
                        .FirstAsync();

                    if (deal != null)
                    {
                        var visitNote = newStatus == "Completed"
                            ? $"[{DateTime.Now:yyyy-MM-dd HH:mm}] [Visit Completed] {notes}"
                            : $"[{DateTime.Now:yyyy-MM-dd HH:mm}] [Visit Cancelled] {notes}";

                        deal.Notes = string.IsNullOrEmpty(deal.Notes)
                            ? visitNote
                            : $"{deal.Notes}\n{visitNote}";
                        deal.Update_time = DateTime.Now;

                        await Context.Updateable(deal)
                            .UpdateColumns(d => new { d.Notes, d.Update_time })
                            .ExecuteCommandAsync();
                    }
                }

                // Audit log
                await Context.Insertable(new EntityStatusAudit
                {
                    Id = SnowFlakeSingle.Instance.NextId(),
                    EntityType = "Visit",
                    EntityId = visitId.ToString(),
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Notes = notes,
                    ChangedBy = visit.Update_by ?? "System",
                    ChangedAt = DateTime.Now
                }).ExecuteCommandAsync();
            });

            if (!tranResult.IsSuccess)
                throw new Exception($"Transaction failed: {tranResult.ErrorMessage}");

            return 1;
        }

        public async Task<PagedInfo<SiteVisit>> GetVisitsByCreatorAsync(SiteVisitQueryDto query, string createdBy)
        {
            var now = DateTime.Now;
            var pageQuery = Context
                .Queryable<SiteVisit, BpLead, HrEmployeeCore, HrPersonName, SalesDeal>(
                    (v, lead, emp, name, deal) => new JoinQueryInfos(
                        JoinType.Left, v.LeadId == lead.LeadId,
                        JoinType.Left, v.AssignedStaffId == emp.EmployeeId,
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now),
                        JoinType.Left, v.DealId == deal.DealId))
                .Where((v, lead, emp, name, deal) => v.Create_by == createdBy)
                .WhereIF(query.LeadId.HasValue, (v, lead, emp, name, deal) => v.LeadId == query.LeadId.Value)
                .WhereIF(query.DealId.HasValue, (v, lead, emp, name, deal) => v.DealId == query.DealId.Value)
                .WhereIF(!string.IsNullOrEmpty(query.VisitStatus), (v, lead, emp, name, deal) => v.VisitStatus == query.VisitStatus)
                .WhereIF(query.StartDate.HasValue, (v, lead, emp, name, deal) => v.ScheduledDate >= query.StartDate.Value)
                .WhereIF(query.EndDate.HasValue, (v, lead, emp, name, deal) => v.ScheduledDate <= query.EndDate.Value)
                .Select((v, lead, emp, name, deal) => new SiteVisit
                {
                    VisitId = v.VisitId,
                    LeadId = v.LeadId,
                    LeadName = lead.BrandName,
                    DealId = v.DealId,
                    DealTitle = deal.DealTitle,
                    ScheduledDate = v.ScheduledDate,
                    ScheduledTime = v.ScheduledTime,
                    AssignedStaffId = v.AssignedStaffId,
                    AssignedStaffName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    VisitStatus = v.VisitStatus,
                    VisitNotes = v.VisitNotes,
                    CompletionNotes = v.CompletionNotes,
                    CompletedDate = v.CompletedDate,
                    CancellationReason = v.CancellationReason,
                    Create_by = v.Create_by,
                    Create_time = v.Create_time,
                    Update_by = v.Update_by,
                    Update_time = v.Update_time
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<int> DeleteVisitsAsync(long[] visitIds)
        {
            return await Context.Updateable<SiteVisit>()
                .SetColumns(v => new SiteVisit { IsDeleted = true, DeletedAt = DateTime.Now })
                .Where(v => visitIds.Contains(v.VisitId))
                .ExecuteCommandAsync();
        }

        public async Task<VisitDashboardDto> GetDashboardAsync(long? staffId)
        {
            var query = Context.Queryable<SiteVisit>();
            if (staffId.HasValue)
                query = query.Where(v => v.AssignedStaffId == staffId.Value);

            var visits = await query.ToListAsync();
            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);

            return new VisitDashboardDto
            {
                ScheduledCount = visits.Count(v => v.VisitStatus == "Scheduled"),
                InProgressCount = visits.Count(v => v.VisitStatus == "InProgress"),
                CompletedCount = visits.Count(v => v.VisitStatus == "Completed"),
                CancelledCount = visits.Count(v => v.VisitStatus == "Cancelled"),
                TodayVisits = visits.Count(v => v.ScheduledDate.Date == today),
                UpcomingVisits = visits.Count(v => v.ScheduledDate.Date > today && v.ScheduledDate.Date <= nextWeek && v.VisitStatus == "Scheduled"),
                TotalVisits = visits.Count
            };
        }
    }
}
