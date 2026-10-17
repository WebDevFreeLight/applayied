using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Service;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// Activity Service Interface - خدمة الأنشطة
    /// </summary>
    public interface IErpActivityService : IBaseService<ErpActivity>
    {
        PagedInfo<ErpActivity> GetList(ErpActivity parm, PagerInfo pager);
        ErpActivity GetInfo(long activityId);
        ErpActivity AddActivity(ErpActivity model);
        int UpdateActivity(ErpActivity model);
        int Delete(long[] ids);
        List<ErpActivity> GetActiveActivities(long? BranchID = null);
        List<ErpActivity> GetActivitiesByCategory(long categoryId);
        List<ErpActivity> GetActivitiesByInstructor(long instructorId);
        List<ErpActivityGroup> GetActivityGroups(long activityId);
        ErpActivityGroup AddActivityGroup(ErpActivityGroup group);
        int UpdateActivityGroup(ErpActivityGroup group);
        int DeleteActivityGroup(long groupId);
        List<ErpActivityAttendance> GetGroupAttendance(long groupId, DateTime date);
        int RecordAttendance(long groupId, long memberId, DateTime date, string status);
        int UpdateAttendance(long attendanceId, string status);
        List<object> GetActivitySchedule(long activityId, DateTime fromDate, DateTime toDate);
        List<object> GetDailySchedule(DateTime date, long? BranchID = null);
        int GetActivityEnrollmentCount(long activityId);
        List<ErpMember> GetActivityEnrolledMembers(long activityId);
        decimal GetActivityRevenue(long activityId, DateTime fromDate, DateTime toDate);

        // Additional methods for controller
        List<ErpActivity> GetActivitiesByResource(long resourceId);
        object GetActivitySchedule(long activityId, DateTime date);
        object GetWeeklySchedule(long? branchId, DateTime startDate);
        int RegisterMember(long activityId, long memberId, DateTime sessionDate);
        int CancelRegistration(long activityId, long memberId, DateTime sessionDate);
        int RecordActivityAttendance(long activityId, long memberId, DateTime sessionDate, string status);
        List<object> GetSessionAttendees(long activityId, DateTime sessionDate);
        object GetActivityStatistics(long activityId);
        int GetAvailableSlots(long activityId, DateTime sessionDate);
    }
}

