using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{
    /// <summary>
    /// Activity Service - خدمة الأنشطة
    /// </summary>
    [AppService(ServiceType = typeof(IErpActivityService), ServiceLifetime = LifeTime.Transient)]
    public class ErpActivityService : BaseService<ErpActivity>, IErpActivityService
    { 


        public PagedInfo<ErpActivity> GetList(ErpActivity parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpActivity>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ActivityName), it => it.ActivityName.Contains(parm.ActivityName));
            predicate = predicate.AndIF(parm.CategoryId > 0, it => it.CategoryId == parm.CategoryId);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.ActivityName)
                .ToPage(pager);
        }

        public ErpActivity GetInfo(long activityId)
        {
            return GetFirst(x => x.ActivityId == activityId);
        }

        public ErpActivity AddActivity(ErpActivity model)
        {
            model.ActivityId = SnowFlakeSingle.Instance.NextId();
            Insert(model);
            return model;
        }

        public int UpdateActivity(ErpActivity model)
        {
            return Update(model, it => new
            {
                it.ActivityName,
                it.ActivityNameAr,
                it.CategoryId,
                it.Description,
                it.DurationMinutes,
                it.MaxCapacity,
                it.Price,
                it.DefaultTrainerId,
                it.Update_by,
                it.Update_time
            }, f => f.ActivityId == model.ActivityId);
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpActivity>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.ActivityId))
                .ExecuteCommand();
        }

        public List<ErpActivity> GetActiveActivities(long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .OrderBy(x => x.ActivityName)
                .ToList();
        }

        public List<ErpActivity> GetActivitiesByCategory(long categoryId)
        {
            return Queryable()
                .Where(x => x.CategoryId == categoryId && x.IsActive == true)
                .ToList();
        }

        public List<ErpActivity> GetActivitiesByInstructor(long trainerId)
        {
            return Queryable()
                .Where(x => x.DefaultTrainerId == trainerId && x.IsActive == true)
                .ToList();
        }

        public List<ErpActivityGroup> GetActivityGroups(long activityId)
        {
            return Context.Queryable<ErpActivityGroup>()
                .Where(x => x.ActivityId == activityId && x.IsActive == true)
                .OrderBy(x => x.GroupName)
                .ToList();
        }

        public ErpActivityGroup AddActivityGroup(ErpActivityGroup group)
        {
            group.GroupId = SnowFlakeSingle.Instance.NextId();
            Context.Insertable(group).ExecuteCommand();
            return group;
        }

        public int UpdateActivityGroup(ErpActivityGroup group)
        {
            return Context.Updateable(group)
                .UpdateColumns(it => new
                {
                    it.GroupName,
                    it.StartTime,
                    it.ScheduleDays,
                    it.MaxMembers,
                    it.TrainerId,
                    it.Update_by,
                    it.Update_time
                })
                .Where(it => it.GroupId == group.GroupId)
                .ExecuteCommand();
        }

        public int DeleteActivityGroup(long groupId)
        {
            return Context.Updateable<ErpActivityGroup>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => it.GroupId == groupId)
                .ExecuteCommand();
        }

        public List<ErpActivityAttendance> GetGroupAttendance(long groupId, DateTime date)
        {
            return Context.Queryable<ErpActivityAttendance>()
                .Where(x => x.GroupId == groupId && x.SessionDate.Date == date.Date)
                .ToList();
        }

        public int RecordAttendance(long groupId, long memberId, DateTime date, string status)
        {
            var existing = Context.Queryable<ErpActivityAttendance>()
                .First(x => x.GroupId == groupId && x.MemberId == memberId && x.SessionDate.Date == date.Date);

            if (existing != null)
            {
                return UpdateAttendance(existing.AttendanceId, status);
            }

            var attendance = new ErpActivityAttendance
            {
                AttendanceId = SnowFlakeSingle.Instance.NextId(),
                GroupId = groupId,
                MemberId = memberId,
                SessionDate = date,
                Status = status,
                CheckInTime = status == "present" ? DateTime.Now : null,
                Create_time = DateTime.Now
            };

            return Context.Insertable(attendance).ExecuteCommand();
        }

        public int UpdateAttendance(long attendanceId, string status)
        {
            return Context.Updateable<ErpActivityAttendance>()
                .SetColumns(it => new ErpActivityAttendance
                {
                    Status = status,
                    CheckInTime = status == "present" ? DateTime.Now : null,
                    Update_time = DateTime.Now
                })
                .Where(it => it.AttendanceId == attendanceId)
                .ExecuteCommand();
        }

        public List<object> GetActivitySchedule(long activityId, DateTime fromDate, DateTime toDate)
        {
            var groups = GetActivityGroups(activityId);
            var schedule = new List<object>();

            foreach (var group in groups)
            {
                if (!string.IsNullOrEmpty(group.ScheduleDays))
                {
                    var days = group.ScheduleDays.Split(',');
                    for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                    {
                        if (days.Contains(date.DayOfWeek.ToString()))
                        {
                            schedule.Add(new
                            {
                                Date = date,
                                GroupId = group.GroupId,
                                GroupName = group.GroupName,
                                Time = group.StartTime,
                                TrainerId = group.TrainerId
                            });
                        }
                    }
                }
            }

            return schedule.OrderBy(s => ((dynamic)s).Date).ToList();
        }

        public List<object> GetDailySchedule(DateTime date, long? BranchID = null)
        {
            var groups = Context.Queryable<ErpActivityGroup, ErpActivity>((g, a) =>
                    new JoinQueryInfos(JoinType.Inner, g.ActivityId == a.ActivityId))
                .Where((g, a) => g.IsActive == true && a.IsActive == true)
                .WhereIF(BranchID.HasValue, (g, a) => a.BranchID == BranchID)
                .Select((g, a) => new
                {
                    g.GroupId,
                    g.GroupName,
                    g.ActivityId,
                    ActivityName = a.ActivityName,
                    g.StartTime,
                    g.ScheduleDays,
                    g.MaxMembers,
                    g.TrainerId
                })
                .ToList();

            var dayOfWeek = date.DayOfWeek.ToString();
            return groups
                .Where(g => !string.IsNullOrEmpty(g.ScheduleDays) && g.ScheduleDays.Contains(dayOfWeek))
                .Cast<object>()
                .ToList();
        }

        public int GetActivityEnrollmentCount(long activityId)
        {
            // Count members with subscriptions that include this activity
            return Context.Queryable<ErpActivityAttendance>()
                .Where(x => x.ActivityId == activityId)
                .Select(x => x.MemberId)
                .Distinct()
                .Count();
        }

        public List<ErpMember> GetActivityEnrolledMembers(long activityId)
        {
            var memberIds = Context.Queryable<ErpActivityAttendance>()
                .Where(x => x.ActivityId == activityId)
                .Select(x => x.MemberId)
                .Distinct()
                .ToList();

            return Context.Queryable<ErpMember>()
                .Where(m => memberIds.Contains(m.MemberId) && m.IsActive == true)
                .ToList();
        }

        public decimal GetActivityRevenue(long activityId, DateTime fromDate, DateTime toDate)
        {
            return Context.Queryable<ErpActivityAttendance>()
                .Where(x => x.ActivityId == activityId)
                .Where(x => x.SessionDate >= fromDate && x.SessionDate <= toDate)
                .Where(x => x.IsPaidSession == true)
                .Sum(x => x.AmountPaid);
        }

        #region Additional Methods

        public List<ErpActivity> GetActivitiesByResource(long resourceId)
        {
            return Queryable()
                .Where(x => x.ResourceId == resourceId && x.IsActive == true)
                .ToList();
        }

        public object GetActivitySchedule(long activityId, DateTime date)
        {
            var activity = GetInfo(activityId);
            var groups = GetActivityGroups(activityId);
            var dayOfWeek = date.DayOfWeek.ToString();

            var sessions = groups
                .Where(g => !string.IsNullOrEmpty(g.ScheduleDays) && g.ScheduleDays.Contains(dayOfWeek))
                .Select(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    g.StartTime,
                    g.MaxMembers,
                    g.TrainerId,
                    CurrentEnrollment = Context.Queryable<ErpActivityAttendance>()
                        .Count(a => a.GroupId == g.GroupId && a.SessionDate.Date == date.Date)
                })
                .ToList();

            return new
            {
                activity.ActivityId,
                activity.ActivityName,
                Date = date,
                Sessions = sessions
            };
        }

        public object GetWeeklySchedule(long? branchId, DateTime startDate)
        {
            var endDate = startDate.AddDays(7);
            var activities = GetActiveActivities(branchId);
            var schedule = new List<object>();

            foreach (var activity in activities)
            {
                var activitySchedule = GetActivitySchedule(activity.ActivityId, startDate, endDate);
                schedule.AddRange(activitySchedule.Select(s => new
                {
                    activity.ActivityId,
                    activity.ActivityName,
                    Session = s
                }));
            }

            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                BranchId = branchId,
                Schedule = schedule.OrderBy(s => ((dynamic)s).Session.Date).ToList()
            };
        }

        public int RegisterMember(long activityId, long memberId, DateTime sessionDate)
        {
            // Find available group for this activity on the given date
            var groups = GetActivityGroups(activityId);
            var dayOfWeek = sessionDate.DayOfWeek.ToString();

            var availableGroup = groups
                .Where(g => !string.IsNullOrEmpty(g.ScheduleDays) && g.ScheduleDays.Contains(dayOfWeek))
                .FirstOrDefault(g =>
                {
                    var currentCount = Context.Queryable<ErpActivityAttendance>()
                        .Count(a => a.GroupId == g.GroupId && a.SessionDate.Date == sessionDate.Date);
                    return currentCount < g.MaxMembers;
                });

            if (availableGroup == null)
                throw new Exception("لا توجد أماكن متاحة - No available slots");

            var attendance = new ErpActivityAttendance
            {
                AttendanceId = SnowFlakeSingle.Instance.NextId(),
                ActivityId = activityId,
                GroupId = availableGroup.GroupId,
                MemberId = memberId,
                SessionDate = sessionDate,
                Status = "registered",
                Create_time = DateTime.Now
            };

            return Context.Insertable(attendance).ExecuteCommand();
        }

        public int CancelRegistration(long activityId, long memberId, DateTime sessionDate)
        {
            return Context.Deleteable<ErpActivityAttendance>()
                .Where(x => x.ActivityId == activityId &&
                           x.MemberId == memberId &&
                           x.SessionDate.Date == sessionDate.Date &&
                           x.Status == "registered")
                .ExecuteCommand();
        }

        public int RecordActivityAttendance(long activityId, long memberId, DateTime sessionDate, string status)
        {
            var existing = Context.Queryable<ErpActivityAttendance>()
                .First(x => x.ActivityId == activityId &&
                           x.MemberId == memberId &&
                           x.SessionDate.Date == sessionDate.Date);

            if (existing != null)
            {
                return Context.Updateable<ErpActivityAttendance>()
                    .SetColumns(it => new ErpActivityAttendance
                    {
                        Status = status,
                        CheckInTime = status == "present" ? DateTime.Now : null,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.AttendanceId == existing.AttendanceId)
                    .ExecuteCommand();
            }

            var attendance = new ErpActivityAttendance
            {
                AttendanceId = SnowFlakeSingle.Instance.NextId(),
                ActivityId = activityId,
                MemberId = memberId,
                SessionDate = sessionDate,
                Status = status,
                CheckInTime = status == "present" ? DateTime.Now : null,
                Create_time = DateTime.Now
            };

            return Context.Insertable(attendance).ExecuteCommand();
        }

        public List<object> GetSessionAttendees(long activityId, DateTime sessionDate)
        {
            return Context.Queryable<ErpActivityAttendance, ErpMember>((a, m) =>
                    new JoinQueryInfos(JoinType.Inner, a.MemberId == m.MemberId))
                .Where((a, m) => a.ActivityId == activityId && a.SessionDate.Date == sessionDate.Date)
                .Select((a, m) => new
                {
                    a.AttendanceId,
                    a.MemberId,
                    m.FullName,
                    m.FullNameAr,
                    m.Phone,
                    a.Status,
                    a.CheckInTime,
                    a.CheckOutTime
                })
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public object GetActivityStatistics(long activityId)
        {
            var activity = GetInfo(activityId);
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);

            var attendance = Context.Queryable<ErpActivityAttendance>()
                .Where(x => x.ActivityId == activityId && x.SessionDate >= thirtyDaysAgo)
                .ToList();

            return new
            {
                activity.ActivityId,
                activity.ActivityName,
                TotalSessions = attendance.Select(x => x.SessionDate.Date).Distinct().Count(),
                TotalAttendance = attendance.Count(x => x.Status == "present"),
                UniqueMembers = attendance.Select(x => x.MemberId).Distinct().Count(),
                AverageAttendancePerSession = attendance.GroupBy(x => x.SessionDate.Date)
                    .Average(g => g.Count(x => x.Status == "present")),
                AttendanceRate = attendance.Any() ?
                    (decimal)attendance.Count(x => x.Status == "present") / attendance.Count * 100 : 0,
                PopularDays = attendance.GroupBy(x => x.SessionDate.DayOfWeek)
                    .OrderByDescending(g => g.Count())
                    .Select(g => new { Day = g.Key.ToString(), Count = g.Count() })
                    .Take(3)
                    .ToList()
            };
        }

        public int GetAvailableSlots(long activityId, DateTime sessionDate)
        {
            var groups = GetActivityGroups(activityId);
            var dayOfWeek = sessionDate.DayOfWeek.ToString();

            var totalSlots = 0;
            var usedSlots = 0;

            foreach (var group in groups.Where(g => !string.IsNullOrEmpty(g.ScheduleDays) && g.ScheduleDays.Contains(dayOfWeek)))
            {
                totalSlots += group.MaxMembers;
                usedSlots += Context.Queryable<ErpActivityAttendance>()
                    .Count(a => a.GroupId == group.GroupId && a.SessionDate.Date == sessionDate.Date);
            }

            return totalSlots - usedSlots;
        }

        #endregion
    }
}

