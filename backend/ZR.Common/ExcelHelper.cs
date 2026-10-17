using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ZR.Common
{
    public class ExcelHelper<T> where T : new()
    {
        /// <summary>
        /// Import data
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        //public static IEnumerable<T> ImportData(Stream stream)
        //{
        //    using ExcelPackage package = new(stream);
        //    //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];//???1?sheet
        //    //Get table column and row count

        //    int colStart = worksheet.Dimension.Start.Column;
        //    int colEnd = worksheet.Dimension.End.Column;
        //    int rowStart = worksheet.Dimension.Start.Row;
        //    int rowEnd = worksheet.Dimension.End.Row;
        //    //int rowCount = worksheet.Dimension.Rows;
        //    //int ColCount = worksheet.Dimension.Columns;

        //    List<T> resultList = new();
        //    List<PropertyInfo> propertyInfos = new();// new(typeof(T).GetProperties());
        //    Dictionary<string, int> dictHeader = new();
        //    for (int i = colStart; i < colEnd; i++)
        //    {
        //        var name = worksheet.Cells[rowStart, i].Value?.ToString();
        //        dictHeader[name] = i;

        //        PropertyInfo propertyInfo = MapPropertyInfo(name);
        //        if (propertyInfo != null)
        //        {
        //            propertyInfos.Add(propertyInfo);
        //        }
        //    }
        //    for (int row = rowStart + 1; row <= rowEnd; row++)
        //    {
        //        T result = new();

        //        foreach (PropertyInfo p in propertyInfos)
        //        {
        //            try
        //            {
        //                ExcelRange cell = worksheet.Cells[row, dictHeader[p.Name]];
        //                if (cell.Value == null)
        //                {
        //                    continue;
        //                }
        //                switch (p.PropertyType.Name.ToLower())
        //                {
        //                    case "string":
        //                        p.SetValue(result, cell.GetValue<string>());
        //                        break;
        //                    case "int16":
        //                        p.SetValue(result, cell.GetValue<short>()); break;
        //                    case "int32":
        //                        p.SetValue(result, cell.GetValue<int>()); break;
        //                    case "int64":
        //                        p.SetValue(result, cell.GetValue<long>()); break;
        //                    case "decimal":
        //                        p.SetValue(result, cell.GetValue<decimal>());
        //                        break;
        //                    case "double":
        //                        p.SetValue(result, cell.GetValue<double>()); break;
        //                    case "datetime":
        //                        p.SetValue(result, cell.GetValue<DateTime>()); break;
        //                    case "boolean":
        //                        p.SetValue(result, cell.GetValue<bool>()); break;
        //                    case "char":
        //                        p.SetValue(result, cell.GetValue<string>()); break;
        //                    default:
        //                        break;
        //                }
        //            }
        //            catch (KeyNotFoundException ex)
        //            {
        //                Console.WriteLine("Column not found, will continue loop, " + ex.Message);
        //                continue;
        //            }
        //        }
        //        resultList.Add(result);
        //    }

        //    return resultList;
        //}

        /// <summary>
        /// Find entity property corresponding to Excel column name
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static PropertyInfo MapPropertyInfo(string columnName)
        {
            PropertyInfo[] propertyList = GetProperties(typeof(T));
            PropertyInfo propertyInfo = propertyList.Where(p => p.Name == columnName).FirstOrDefault();
            if (propertyInfo != null)
            {
                return propertyInfo;
            }
            else
            {
                foreach (PropertyInfo tempPropertyInfo in propertyList)
                {
                    System.ComponentModel.DescriptionAttribute[] attributes = (System.ComponentModel.DescriptionAttribute[])tempPropertyInfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                    if (attributes.Length > 0)
                    {
                        if (attributes[0].Description == columnName)
                        {
                            return tempPropertyInfo;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="type"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(Type type, string[] columns = null)
        {
            PropertyInfo[] properties = null;
            properties = type.GetProperties();

            if (columns != null && columns.Length > 0)
            {
                //  ?columns??????
                var columnPropertyList = new List<PropertyInfo>();
                foreach (var column in columns)
                {
                    var columnProperty = properties.Where(p => p.Name == column).FirstOrDefault();
                    if (columnProperty != null)
                    {
                        columnPropertyList.Add(columnProperty);
                    }
                }
                return columnPropertyList.ToArray();
            }
            else
            {
                return properties;
            }
        }
    }
}

