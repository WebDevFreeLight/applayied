using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{
    /// <summary>
    /// Booking Service - خدمة الحجوزات
    /// </summary>
    [AppService(ServiceType = typeof(IErpBookingService), ServiceLifetime = LifeTime.Transient)]
    public class ErpBookingService : BaseService<ErpBooking>, IErpBookingService
    {
         

        public PagedInfo<ErpBooking> GetList(ErpBooking parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpBooking>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.BookingNumber), it => it.BookingNumber.Contains(parm.BookingNumber));
            predicate = predicate.AndIF(parm.MemberId > 0, it => it.MemberId == parm.MemberId);
            predicate = predicate.AndIF(parm.ResourceId > 0, it => it.ResourceId == parm.ResourceId);
            predicate = predicate.AndIF(parm.ActivityId > 0, it => it.ActivityId == parm.ActivityId);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.BookingDate)
                .ToPage(pager);
        }

        public ErpBooking GetInfo(long bookingId)
        {
            return GetFirst(x => x.BookingId == bookingId);
        }

        public ErpBooking CreateBooking(ErpBooking model)
        {
            // Check availability
            if (model.ResourceId.HasValue && !IsSlotAvailable(model.ResourceId.Value, model.StartTime, model.EndTime, null))
            {
                throw new Exception("الموعد غير متاح - Slot is not available");
            }

            if (string.IsNullOrEmpty(model.BookingNumber))
            {
                model.BookingNumber = GenerateBookingNumber(model.BranchID);
            }

            model.BookingId = SnowFlakeSingle.Instance.NextId();
            model.Status = "pending";

            Insert(model);
            return model;
        }

        public int UpdateBooking(ErpBooking model)
        {
            // Check availability if time changed
            var existing = GetInfo(model.BookingId);
            if (existing != null && model.ResourceId.HasValue &&
                (model.StartTime != existing.StartTime || model.EndTime != existing.EndTime))
            {
                if (!IsSlotAvailable(model.ResourceId.Value, model.StartTime, model.EndTime, model.BookingId))
                {
                    throw new Exception("الموعد غير متاح - Slot is not available");
                }
            }

            return Update(model, it => new
            {
                it.BookingDate,
                it.StartTime,
                it.EndTime,
                it.ResourceId,
                it.ActivityId,
                it.ServiceStaffId,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.BookingId == model.BookingId);
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.BookingId))
                .ExecuteCommand();
        }

        public string GenerateBookingNumber(long? BranchID)
        {
            var prefix = BranchID.HasValue ? $"BKG-{BranchID}-" : "BKG-";
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = Count(x => x.BookingNumber.StartsWith(prefix + date)) + 1;
            return $"{prefix}{date}-{count:D4}";
        }

        public int ConfirmBooking(long bookingId, string userId)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => new ErpBooking
                {
                    Status = "confirmed",
                    Update_by = userId,
                    Update_time = DateTime.Now
                })
                .Where(it => it.BookingId == bookingId && it.Status == "pending")
                .ExecuteCommand();
        }

        public int CancelBooking(long bookingId, string reason)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => new ErpBooking
                {
                    Status = "cancelled",
                    CancellationReason = reason,
                    CancelledAt = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(it => it.BookingId == bookingId)
                .ExecuteCommand();
        }

        public int CheckIn(long bookingId)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => new ErpBooking
                {
                    Status = "completed",
                    ConfirmedAt = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(it => it.BookingId == bookingId && it.Status == "confirmed")
                .ExecuteCommand();
        }

        public int NoShow(long bookingId)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => new ErpBooking
                {
                    Status = "no_show",
                    Update_time = DateTime.Now
                })
                .Where(it => it.BookingId == bookingId)
                .ExecuteCommand();
        }

        public int CompleteBooking(long bookingId)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => new ErpBooking
                {
                    Status = "completed",
                    Update_time = DateTime.Now
                })
                .Where(it => it.BookingId == bookingId)
                .ExecuteCommand();
        }

        public List<ErpBooking> GetMemberBookings(long memberId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.MemberId == memberId && x.IsActive == true)
                .WhereIF(fromDate.HasValue, x => x.BookingDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.BookingDate <= toDate.Value)
                .OrderByDescending(x => x.BookingDate)
                .ToList();
        }

        public List<ErpBooking> GetResourceBookings(long resourceId, DateTime date)
        {
            return Queryable()
                .Where(x => x.ResourceId == resourceId && x.BookingDate.Date == date.Date)
                .Where(x => x.Status != "cancelled" && x.IsActive == true)
                .OrderBy(x => x.StartTime)
                .ToList();
        }

        public List<ErpBooking> GetActivityBookings(long activityId, DateTime date)
        {
            return Queryable()
                .Where(x => x.ActivityId == activityId && x.BookingDate.Date == date.Date)
                .Where(x => x.Status != "cancelled" && x.IsActive == true)
                .OrderBy(x => x.StartTime)
                .ToList();
        }

        public List<ErpBooking> GetStaffBookings(long staffId, DateTime date)
        {
            return Queryable()
                .Where(x => x.ServiceStaffId == staffId && x.BookingDate.Date == date.Date)
                .Where(x => x.Status != "cancelled" && x.IsActive == true)
                .OrderBy(x => x.StartTime)
                .ToList();
        }

        public bool IsSlotAvailable(long resourceId, DateTime startTime, DateTime endTime, long? excludeBookingId = null)
        {
            var query = Queryable()
                .Where(x => x.ResourceId == resourceId)
                .Where(x => x.Status != "cancelled" && x.IsActive == true)
                .Where(x => (x.StartTime < endTime && x.EndTime > startTime));

            if (excludeBookingId.HasValue)
            {
                query = query.Where(x => x.BookingId != excludeBookingId.Value);
            }

            return !query.Any();
        }

        public List<object> GetAvailableSlots(long resourceId, DateTime date)
        {
            var resource = Context.Queryable<ErpResource>().First(x => x.ResourceId == resourceId);
            if (resource == null) return new List<object>();

            var bookings = GetResourceBookings(resourceId, date);
            var slots = new List<object>();

            // Generate hourly slots (this would be customized based on resource settings)
            var startHour = 6; // 6 AM
            var endHour = 22; // 10 PM
            var slotDuration = 60; // minutes

            for (var hour = startHour; hour < endHour; hour++)
            {
                var slotStart = date.Date.AddHours(hour);
                var slotEnd = slotStart.AddMinutes(slotDuration);

                var isBooked = bookings.Any(b => b.StartTime < slotEnd && b.EndTime > slotStart);

                slots.Add(new
                {
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsAvailable = !isBooked
                });
            }

            return slots;
        }

        public List<ErpBooking> GetTodayBookings(long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.BookingDate.Date == DateTime.Today)
                .Where(x => x.Status != "cancelled" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .OrderBy(x => x.StartTime)
                .ToList();
        }

        public List<ErpBooking> GetUpcomingBookings(long? BranchID = null, int days = 7)
        {
            var endDate = DateTime.Today.AddDays(days);
            return Queryable()
                .Where(x => x.BookingDate >= DateTime.Today && x.BookingDate <= endDate)
                .Where(x => x.Status != "cancelled" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .OrderBy(x => x.BookingDate)
                .OrderBy(x => x.StartTime)
                .ToList();
        }

        public decimal GetTotalBookingRevenue(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.BookingDate >= fromDate && x.BookingDate <= toDate)
                .Where(x => x.Status == "completed" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .Sum(x => x.NetAmount);
        }

        public List<object> GetBookingsByActivity(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.BookingDate >= fromDate && x.BookingDate <= toDate)
                .Where(x => x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .GroupBy(x => new { x.ActivityId })
                .Select(x => new
                {
                    x.ActivityId,
                    TotalBookings = SqlFunc.AggregateCount(x.BookingId),
                    TotalRevenue = SqlFunc.AggregateSum(x.NetAmount)
                })
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public List<object> GetBookingsByStaff(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.BookingDate >= fromDate && x.BookingDate <= toDate)
                .Where(x => x.IsActive == true && x.ServiceStaffId != null)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .GroupBy(x => new { x.ServiceStaffId })
                .Select(x => new
                {
                    x.ServiceStaffId,
                    TotalBookings = SqlFunc.AggregateCount(x.BookingId),
                    TotalRevenue = SqlFunc.AggregateSum(x.NetAmount)
                })
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public int ProcessAdvancePayment(long bookingId, decimal amount, long paymentMethodId)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => new ErpBooking
                {
                    AdvanceAmount = it.AdvanceAmount + amount,
                    RemainingAmount = it.NetAmount - it.AdvanceAmount - amount,
                    Update_time = DateTime.Now
                })
                .Where(it => it.BookingId == bookingId)
                .ExecuteCommand();
        }

        public int ProcessRefund(long bookingId, decimal amount)
        {
            return Context.Updateable<ErpBooking>()
                .SetColumns(it => new ErpBooking
                {
                    Status = "cancelled",
                    Update_time = DateTime.Now
                })
                .Where(it => it.BookingId == bookingId && it.Status == "cancelled")
                .ExecuteCommand();
        }
    }
}

