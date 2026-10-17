using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Infrastructure.Extensions
{
    public static partial class Extensions
    {
        #region Convert enum members to dictionary type
        /// <summary>
        /// Convert to dictionary type
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary(this Type enumType)
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            Type typeDescription = typeof(DescriptionAttribute);
            FieldInfo[] fields = enumType.GetFields();
            int sValue = 0;
            string sText = string.Empty;
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    sValue = ((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null));
                    object[] arr = field.GetCustomAttributes(typeDescription, true);
                    if (arr.Length > 0)
                    {
                        DescriptionAttribute da = (DescriptionAttribute)arr[0];
                        sText = da.Description;
                    }
                    else
                    {
                        sText = field.Name;
                    }
                    dictionary.Add(sValue, sText);
                }
            }
            return dictionary;
        }
        /// <summary>
        /// Convert enum members to key-value pair JSON string
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        //public static string EnumToDictionaryString(this Type enumType)
        //{
        //    List<KeyValuePair<int, string>> dictionaryList = EnumToDictionary(enumType).ToList();
        //    var sJson = JsonConvert.SerializeObject(dictionaryList);
        //    return sJson;
        //}
        #endregion

        #region Get enum description
        /// <summary>
        /// Get description corresponding to enum value
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Enum enumType)
        {
            FieldInfo EnumInfo = enumType.GetType().GetField(enumType.ToString());
            if (EnumInfo != null)
            {
                DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (EnumAttributes.Length > 0)
                {
                    return EnumAttributes[0].Description;
                }
            }
            return enumType.ToString();
        }
        #endregion

        #region Get enum description by value
        public static string GetDescriptionByEnum<T>(this object obj)
        {
            var tEnum = System.Enum.Parse(typeof(T), obj.ParseToString()) as System.Enum;
            var description = tEnum.GetDescription();
            return description;
        }
        #endregion
    }
}

