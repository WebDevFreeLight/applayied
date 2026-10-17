using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Extensions
{
    public static class StringExtension
    {

        /// <summary>
        /// SQL condition concatenation
        /// </summary>
        /// <param name="str"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static string If(this string str, bool condition)
        {
            return condition ? str : string.Empty;
        }
        /// <summary>
        /// Check if string is not empty
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IfNotEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Note: If the old value to be replaced contains special symbols, the replacement will fail. Solution: For example, if the special symbol is "(": add oldValue=oldValue.Replace("(","//("); before calling this method
        /// </summary>
        /// <param name="input"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string input, string oldValue, string newValue)
        {
            Regex regEx = new Regex(oldValue, RegexOptions.Multiline);
            return regEx.Replace(input, newValue == null ? "" : newValue, 1);
        }

        /// <summary>
        /// Convert camelCase to underscore
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ToSmallCamelCase(string name)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(name.Substring(0, 1).ToLower());

            for (var i = 0; i < name.Length; i++)
            {
                if (i == 0)
                {
                    stringBuilder.Append(name.Substring(0, 1).ToLower());
                }
                else
                {
                    if (name[i] >= 'A' && name[i] <= 'Z')
                    {
                        stringBuilder.Append($"_{name.Substring(i, 1).ToLower()}");
                    }
                    else
                    {
                        stringBuilder.Append(name[i]);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Convert underscore naming to camelCase
        /// </summary>
        /// <param name="underscore"></param>
        /// <returns></returns>
        public static string UnderScoreToCamelCase(this string underscore)
        {
            string[] ss = underscore.Split("_");
            if (ss.Length == 1)
            {
                return underscore;
            }

            StringBuilder sb = new();
            sb.Append(ss[0]);
            for (int i = 1; i < ss.Length; i++)
            {
                sb.Append(ss[i].FirstUpperCase());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert first letter to uppercase
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstUpperCase(this string str)
        {
            return string.IsNullOrEmpty(str) ? str : str[..1].ToUpper() + str[1..];
        }

        /// <summary>
        /// Convert first letter to lowercase
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstLowerCase(this string str)
        {
            return string.IsNullOrEmpty(str) ? str : str.Substring(0, 1).ToLower() + str[1..];
        }

        /// <summary>
        /// Extract content between specified strings
        /// </summary>
        /// <param name="sourse"></param>
        /// <param name="startstr"></param>
        /// <param name="endstr"></param>
        /// <returns></returns>
        public static string SubStringBetween(this string sourse, string startstr, string endstr)
        {
            string result = string.Empty;
            int startindex, endindex;
            try
            {
                startindex = sourse.IndexOf(startstr);
                if (startindex == -1)
                    return result;
                string tmpstr = sourse[(startindex + startstr.Length)..];
                endindex = tmpstr.IndexOf(endstr);
                if (endindex == -1)
                    return result;
                result = tmpstr.Remove(endindex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MidStrEx Err:" + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Convert to Pascal style - capitalize the first letter of each word
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="fieldDelimiter">Delimiter</param>
        /// <returns></returns>
        public static string ConvertToPascal(this string fieldName, string fieldDelimiter)
        {
            string result = string.Empty;
            if (fieldName.Contains(fieldDelimiter))
            {
                //All lowercase
                string[] array = fieldName.ToLower().Split(fieldDelimiter.ToCharArray());
                foreach (var t in array)
                {
                    //First letter uppercase
                    result += t.Substring(0, 1).ToUpper() + t[1..];
                }
            }
            else if (string.IsNullOrWhiteSpace(fieldName))
            {
                result = fieldName;
            }
            else if (fieldName.Length == 1)
            {
                result = fieldName.ToUpper();
            }
            else if (fieldName.Length == CountUpper(fieldName))
            {
                result = fieldName[..1].ToUpper() + fieldName[1..].ToLower();
            }
            else
            {
                result = fieldName[..1].ToUpper() + fieldName[1..];
            }
            return result;
        }

        /// <summary>
        /// Count of uppercase letters
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int CountUpper(this string str)
        {
            int count1 = 0;
            char[] chars = str.ToCharArray();
            foreach (char num in chars)
            {
                if (num >= 'A' && num <= 'Z')
                {
                    count1++;
                }
                //else if (num >= 'a' && num <= 'z')
                //{
                //    count2++;
                //}
            }
            return count1;
        }

        /// <summary>
        /// Convert to Camel style - first word lowercase, then capitalize the first letter of each subsequent word
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="fieldDelimiter">Delimiter</param>
        /// <returns></returns>
        public static string ConvertToCamel(this string fieldName, string fieldDelimiter)
        {
            //First Pascal
            string result = ConvertToPascal(fieldName, fieldDelimiter);
            //Then first letter lowercase
            if (result.Length == 1)
            {
                result = result.ToLower();
            }
            else
            {
                result = result[..1].ToLower() + result[1..];
            }

            return result;
        }
    }
}

