using System;

namespace ZR.Model.Finance.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting an operation on a locked fiscal period
    /// استثناء يُلقى عند محاولة عملية على فترة مالية مقفلة نهائياً
    /// </summary>
    public class PeriodLockedException : BasePeriodException
    {
        public override string ErrorCode => "PERIOD_LOCKED";

        public PeriodLockedException(DateTime date)
            : base($"Cannot perform operation - Fiscal period is permanently locked for date {date:yyyy-MM-dd}")
        {
            Date = date;
            MessageAr = $"لا يمكن تنفيذ العملية — الفترة المالية مقفلة نهائياً للتاريخ {date:yyyy-MM-dd}";
            MessageEn = $"Cannot perform operation - Fiscal period is permanently locked for date {date:yyyy-MM-dd}";
        }

        public PeriodLockedException(DateTime date, string customMessageAr, string customMessageEn)
            : base(customMessageEn)
        {
            Date = date;
            MessageAr = customMessageAr;
            MessageEn = customMessageEn;
        }
    }
}
