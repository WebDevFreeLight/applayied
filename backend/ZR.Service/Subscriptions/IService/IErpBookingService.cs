using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Service;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// Booking Service Interface - خدمة الحجوزات
    /// </summary>
    public interface IErpBookingService : IBaseService<ErpBooking>
    {
        PagedInfo<ErpBooking> GetList(ErpBooking parm, PagerInfo pager);
        ErpBooking GetInfo(long bookingId);
        ErpBooking CreateBooking(ErpBooking model);
        int UpdateBooking(ErpBooking model);
        int Delete(long[] ids);
        string GenerateBookingNumber(long? BranchID);
        int ConfirmBooking(long bookingId, string userId);
        int CancelBooking(long bookingId, string reason);
        int CheckIn(long bookingId);
        int NoShow(long bookingId);
        int CompleteBooking(long bookingId);
        List<ErpBooking> GetMemberBookings(long memberId, DateTime? fromDate, DateTime? toDate);
        List<ErpBooking> GetResourceBookings(long resourceId, DateTime date);
        List<ErpBooking> GetActivityBookings(long activityId, DateTime date);
        List<ErpBooking> GetStaffBookings(long staffId, DateTime date);
        bool IsSlotAvailable(long resourceId, DateTime startTime, DateTime endTime, long? excludeBookingId = null);
        List<object> GetAvailableSlots(long resourceId, DateTime date);
        List<ErpBooking> GetTodayBookings(long? BranchID = null);
        List<ErpBooking> GetUpcomingBookings(long? BranchID = null, int days = 7);
        decimal GetTotalBookingRevenue(DateTime fromDate, DateTime toDate, long? BranchID = null);
        List<object> GetBookingsByActivity(DateTime fromDate, DateTime toDate, long? BranchID = null);
        List<object> GetBookingsByStaff(DateTime fromDate, DateTime toDate, long? BranchID = null);
        int ProcessAdvancePayment(long bookingId, decimal amount, long paymentMethodId);
        int ProcessRefund(long bookingId, decimal amount);
    }
}

