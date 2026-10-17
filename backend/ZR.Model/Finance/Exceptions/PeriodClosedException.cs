using System;

namespace ZR.Model.Finance.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting an operation on a closed fiscal period
    /// استثناء يُلقى عند محاولة عملية على فترة مالية مغلقة
    /// </summary>
    public class PeriodClosedException : BasePeriodException
    {
        public override string ErrorCode => "PERIOD_CLOSED";

        public PeriodClosedException(DateTime date)
            : base($"Cannot perform operation - Fiscal period is closed for date {date:yyyy-MM-dd}")
        {
            Date = date;
            MessageAr = $"لا يمكن تنفيذ العملية — الفترة المالية مغلقة للتاريخ {date:yyyy-MM-dd}";
            MessageEn = $"Cannot perform operation - Fiscal period is closed for date {date:yyyy-MM-dd}";
        }

        public PeriodClosedException(DateTime date, string customMessageAr, string customMessageEn)
            : base(customMessageEn)
        {
            Date = date;
            MessageAr = customMessageAr;
            MessageEn = customMessageEn;
        }
    }
}
