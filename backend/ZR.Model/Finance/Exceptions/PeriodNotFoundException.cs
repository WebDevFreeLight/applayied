using System;

namespace ZR.Model.Finance.Exceptions
{
    /// <summary>
    /// Exception thrown when no fiscal period is found for a given date
    /// استثناء يُلقى عندما لا توجد فترة مالية للتاريخ المحدد
    /// </summary>
    public class PeriodNotFoundException : BasePeriodException
    {
        public override string ErrorCode => "PERIOD_NOT_FOUND";

        public PeriodNotFoundException(DateTime date)
            : base($"No fiscal period found for date {date:yyyy-MM-dd}")
        {
            Date = date;
            MessageAr = $"لا توجد فترة مالية للتاريخ {date:yyyy-MM-dd}";
            MessageEn = $"No fiscal period found for date {date:yyyy-MM-dd}";
        }

        public PeriodNotFoundException(DateTime date, string customMessageAr, string customMessageEn)
            : base(customMessageEn)
        {
            Date = date;
            MessageAr = customMessageAr;
            MessageEn = customMessageEn;
        }
    }
}
