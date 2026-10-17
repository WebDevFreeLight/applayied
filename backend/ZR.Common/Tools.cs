using Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ZR.Common
{
    public class Tools
    {
        /// <summary>
        /// String to be split eg: 1,3,10,00
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split">Split character string</param>
        /// <returns></returns>
        public static long[] SpitLongArrary(string str, char split = ',')
        {
            if (string.IsNullOrEmpty(str)) { return Array.Empty<long>(); }
            str = str.TrimStart(split).TrimEnd(split);
            string[] strIds = str.Split(split, (char)StringSplitOptions.RemoveEmptyEntries);
            long[] infoIdss = Array.ConvertAll(strIds, s => long.Parse(s));
            return infoIdss;
        }

        public static int[] SpitIntArrary(string str, char split = ',')
        {
            if (string.IsNullOrEmpty(str)) { return Array.Empty<int>(); }
            string[] strIds = str.Split(split, (char)StringSplitOptions.RemoveEmptyEntries);
            int[] infoIdss = Array.ConvertAll(strIds, s => int.Parse(s));
            return infoIdss;
        }
        public static T[] SplitAndConvert<T>(string input, char split = ',')
        {
            if (string.IsNullOrEmpty(input)) { return Array.Empty<T>(); }
            string[] parts = input.Split(split, (char)StringSplitOptions.RemoveEmptyEntries);
            T[] result =  Array.ConvertAll(parts, s => (T)Convert.ChangeType(s, typeof(T)));
            //for (int i = 0; i < parts.Length; i++)
            //{
            //    result[i] = (T)Convert.ChangeType(parts[i], typeof(T));
            //}

            return result;
        }

        /// <summary>
        /// Get day of week by date
        /// </summary>
        public static string GetWeekByDate(DateTime dt)
        {
            var day = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return day[Convert.ToInt32(dt.DayOfWeek.ToString("d"))];
        }

        /// <summary>
        /// Get which week of the month
        /// </summary>
        /// <param name="daytime">Year month day</param>
        /// <returns>Which week is the passed time</returns>
        public static int GetWeekNumInMonth(DateTime daytime)
        {
            int dayInMonth = daytime.Day;
            //First day of this month
            DateTime firstDay = daytime.AddDays(1 - daytime.Day);
            //What day of the week is the first day of this month
            int weekday = (int)firstDay.DayOfWeek == 0 ? 7 : (int)firstDay.DayOfWeek;
            //How many days in the first week of this month
            int firstWeekEndDay = 7 - (weekday - 1);
            //Difference between current date and first week
            int diffday = dayInMonth - firstWeekEndDay;
            diffday = diffday > 0 ? diffday : 1;
            //Current week number, if divisible by 7 then subtract one day
            int weekNumInMonth = ((diffday % 7) == 0
                ? (diffday / 7 - 1)
                : (diffday / 7)) + 1 + (dayInMonth > firstWeekEndDay ? 1 : 0);
            return weekNumInMonth;
        }
        /// <summary>
        /// Determine if a string is a url
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsUrl(string str)
        {
            try
            {
                string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
                return Regex.IsMatch(str, Url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static bool CheckUserName(string str)
        {
            try
            {
                string rg = @"^[a-z][a-z0-9-_]*$";
                return Regex.IsMatch(str, rg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }




        public static bool CheckUserName(string str, bool isEmail = false)
        {
            try
            {
                if (isEmail)
                {
                    // Email validation pattern
                    string emailPattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
                    return Regex.IsMatch(str, emailPattern);
                }
                else
                {
                    // Original username validation pattern
                    string usernamePattern = @"^[a-z][a-z0-9-_]*$";
                    return Regex.IsMatch(str, usernamePattern);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Calculate password strength
        /// </summary>
        /// <param name="password">Password string</param>
        /// <returns></returns>
        public static bool PasswordStrength(string password)
        {
            //Empty string strength value is 0
            if (string.IsNullOrEmpty(password)) return false;

            //Character statistics
            int iNum = 0, iLtt = 0, iSym = 0;
            foreach (char c in password)
            {
                if (c >= '0' && c <= '9') iNum++;
                else if (c >= 'a' && c <= 'z') iLtt++;
                else if (c >= 'A' && c <= 'Z') iLtt++;
                else iSym++;
            }

            if (iLtt == 0 && iSym == 0) return false; //Pure numeric password
            if (iNum == 0 && iLtt == 0) return false; //Pure symbol password
            if (iNum == 0 && iSym == 0) return false; //Pure letter password

            if (password.Length >= 6 && password.Length < 16) return true;//Password length not greater than 6

            if (iLtt == 0) return true; //Password composed of numbers and symbols
            if (iSym == 0) return true; //Password composed of numbers and letters
            if (iNum == 0) return true; //Password composed of letters and symbols

            return true; //Password composed of numbers, letters, and symbols
        }
        ///<summary>
        ///Generate random string 
        ///</summary>
        ///<param name="length">Target string length</param>
        ///<param name="useNum">Whether to include numbers, 1=include, default is include</param>
        ///<param name="useLow">Whether to include lowercase letters, 1=include, default is include</param>
        ///<param name="useUpp">Whether to include uppercase letters, 1=include, default is include</param>
        ///<param name="useSpe">Whether to include special characters, 1=include, default is not include</param>
        ///<param name="custom">Custom characters to include, directly input the character list to include</param>
        ///<returns>Random string of specified length</returns>
        public static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(b);
            Random r = new(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
    }
}

