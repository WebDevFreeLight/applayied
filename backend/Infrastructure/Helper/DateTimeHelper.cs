using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public class DateTimeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetBeginTime(DateTime? dateTime, int days = 0)
        {
            if (dateTime == DateTime.MinValue || dateTime == null)
            {
                return DateTime.Now.AddDays(days);
            }
            return dateTime ?? DateTime.Now;
        }
        #region Timestamp conversion

        /// <summary>
        ///  Timestamp to local time - timestamp accurate to seconds
        /// </summary> 
        public static DateTime ToLocalTimeDateBySeconds(long unix)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(unix);
            return dto.ToLocalTime().DateTime;
        }

        /// <summary>
        ///  Time to Unix timestamp - timestamp accurate to seconds
        /// </summary> 
        public static long ToUnixTimestampBySeconds(DateTime dt)
        {
            DateTimeOffset dto = new DateTimeOffset(dt);
            return dto.ToUnixTimeSeconds();
        }

        /// <summary>
        ///  Timestamp to local time - timestamp accurate to milliseconds
        /// </summary> 
        public static DateTime ToLocalTimeDateByMilliseconds(long unix)
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(unix);
            return dto.ToLocalTime().DateTime;
        }

        /// <summary>
        ///  Time to Unix timestamp - timestamp accurate to milliseconds
        /// </summary> 
        public static long ToUnixTimestampByMilliseconds(DateTime dt)
        {
            DateTimeOffset dto = new DateTimeOffset(dt);
            return dto.ToUnixTimeMilliseconds();
        }

        #endregion

        #region Milliseconds to days, hours, minutes, seconds
        /// <summary>
        /// Milliseconds to days, hours, minutes, seconds
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static string FormatTime(long ms)
        {
            int ss = 1000;
            int mi = ss * 60;
            int hh = mi * 60;
            int dd = hh * 24;

            long day = ms / dd;
            long hour = (ms - day * dd) / hh;
            long minute = (ms - day * dd - hour * hh) / mi;
            long second = (ms - day * dd - hour * hh - minute * mi) / ss;
            long milliSecond = ms - day * dd - hour * hh - minute * mi - second * ss;

            string sDay = day < 10 ? "0" + day : "" + day; //days
            string sHour = hour < 10 ? "0" + hour : "" + hour;//hours
            string sMinute = minute < 10 ? "0" + minute : "" + minute;//minutes
            string sSecond = second < 10 ? "0" + second : "" + second;//seconds
            string sMilliSecond = milliSecond < 10 ? "0" + milliSecond : "" + milliSecond;//milliseconds
            sMilliSecond = milliSecond < 100 ? "0" + sMilliSecond : "" + sMilliSecond;

            return string.Format("{0} days {1} hours {2} minutes {3} seconds", sDay, sHour, sMinute, sSecond);
        }
        #endregion

        #region Get unix timestamp
        /// <summary>
        /// Get unix timestamp (milliseconds)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetUnixTimeStamp(DateTime dt)
        {
            long unixTime = ((DateTimeOffset)dt).ToUnixTimeMilliseconds();
            return unixTime;
        }

        public static long GetUnixTimeSeconds(DateTime dt)
        {
            long unixTime = ((DateTimeOffset)dt).ToUnixTimeSeconds();
            return unixTime;
        }
        #endregion

        #region Get minimum time of date
        public static DateTime GetDayMinDate(DateTime dt)
        {
            DateTime min = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            return min;
        }
        #endregion

        #region Get maximum time of date
        public static DateTime GetDayMaxDate(DateTime dt)
        {
            DateTime max = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
            return max;
        }
        #endregion

        #region Format date time
        public static string FormatDateTime(DateTime? dt)
        {
            if (dt != null)
            {
                if (dt.Value.Year == DateTime.Now.Year)
                {
                    return dt.Value.ToString("MM-dd HH:mm");
                }
                else
                {
                    return dt.Value.ToString("yyyy-MM-dd HH:mm");
                }
            }
            return string.Empty;
        }
        #endregion
    }
}

