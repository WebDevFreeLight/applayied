using Infrastructure.Extensions;
using Infrastructure.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Infrastructure.Controllers
{
    /// <summary>
    /// Web layer common data processing
    /// </summary>
    //[ApiController]
    public class BaseController : ControllerBase
    {

        //protected IPdfRenderer _pdfRenderer => App.PdfRenderer;

        public static string TIME_FORMAT_FULL = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Return success wrapper
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeFormatStr"></param>
        /// <returns></returns>
        protected IActionResult SUCCESS(object data, string timeFormatStr = "yyyy-MM-dd HH:mm:ss")
        {
            string jsonStr = GetJsonStr(GetApiResult(data != null ? ResultCode.SUCCESS : ResultCode.NO_DATA, data), timeFormatStr);
            return Content(jsonStr, "application/json");
        }

        /// <summary>
        /// JSON output with time format
        /// </summary>
        /// <param name="apiResult"></param>
        /// <returns></returns>
        protected IActionResult ToResponse(ApiResult apiResult)
        {
            string jsonStr = GetJsonStr(apiResult, TIME_FORMAT_FULL);

            return Content(jsonStr, "application/json");
        }

        protected IActionResult ToResponse(long rows, string timeFormatStr = "yyyy-MM-dd HH:mm:ss")
        {
            string jsonStr = GetJsonStr(ToJson(rows), timeFormatStr);

            return Content(jsonStr, "application/json");
        }

        protected IActionResult ToResponse(ResultCode resultCode, string msg = "")
        {
            return ToResponse(new ApiResult((int)resultCode, msg));
        }

        /// <summary>
        /// Export Excel
        /// </summary>
        /// <param name="path">Complete file path</param>
        /// <param name="fileName">File name with extension</param>
        /// <returns></returns>
        protected IActionResult ExportExcel(string path, string fileName)
        {
            //var webHostEnvironment = App.WebHostEnvironment;
            if (!Path.Exists(path))
            {
                throw new CustomException(fileName + " file does not exist");
            }
            var stream = System.IO.File.OpenRead(path);  //?????

            Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode(fileName));
        }

        /// <summary>
        /// Download file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName">File name, must include extension</param>
        /// <returns></returns>
        protected IActionResult DownFile(string path, string fileName)
        {
            if (!System.IO.File.Exists(path))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "File does not exist");
            }
            var stream = System.IO.File.OpenRead(path);  //?????
            Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
            return File(stream, "application/octet-stream", HttpUtility.UrlEncode(fileName));
        }

        #region Methods

        /// <summary>
        /// Response return result
        /// </summary>
        /// <param name="rows">Affected rows</param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected ApiResult ToJson(long rows, object? data = null)
        {
            return rows > 0 ? ApiResult.Success("success", data) : GetApiResult(ResultCode.FAIL);
        }

        /// <summary>
        /// Global Code usage
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected ApiResult GetApiResult(ResultCode resultCode, object? data = null)
        {
            var msg = resultCode.GetDescription();

            return new ApiResult((int)resultCode, msg, data);
        }
        protected ApiResult Success()
        {
            return GetApiResult(ResultCode.SUCCESS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiResult"></param>
        /// <param name="timeFormatStr"></param>
        /// <returns></returns>
        private static string GetJsonStr(ApiResult apiResult, string timeFormatStr)
        {
            if (string.IsNullOrEmpty(timeFormatStr))
            {
                timeFormatStr = TIME_FORMAT_FULL;
            }
            var serializerSettings = new JsonSerializerSettings
            {
                // Set to camel case naming
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = timeFormatStr
            };

            return JsonConvert.SerializeObject(apiResult, Formatting.Indented, serializerSettings);
        }
        #endregion

        /// <summary>
        /// Export Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="sheetName"></param>
        /// <param name="fileName"></param>
        protected string ExportExcel<T>(List<T> list, string sheetName, string fileName)
        {
            return ExportExcelMini(list, sheetName, fileName).Item1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="sheetName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected (string, string) ExportExcelMini<T>(List<T> list, string sheetName, string fileName)
        {
            IWebHostEnvironment webHostEnvironment = (IWebHostEnvironment)App.ServiceProvider.GetService(typeof(IWebHostEnvironment));
            string sFileName = $"{fileName}_{DateTime.Now:MMdd_HHmmss}.xlsx";
            string fullPath = Path.Combine(webHostEnvironment.WebRootPath, "export", sFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            MiniExcel.SaveAs(fullPath, list, sheetName: sheetName);
            return (sFileName, fullPath);
        }
        protected async Task<(string, string)> ExportExcelMiniAsync<T>(List<T> list, string sheetName, string fileName)
        {
            IWebHostEnvironment webHostEnvironment = (IWebHostEnvironment)App.ServiceProvider.GetService(typeof(IWebHostEnvironment));
            string sFileName = $"{fileName}_{DateTime.Now:MMdd_HHmmss}.xlsx";
            string fullPath = Path.Combine(webHostEnvironment.WebRootPath, "export", sFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await MiniExcel.SaveAsAsync(fullPath, list, sheetName: sheetName);
            return (sFileName, fullPath);
        }
        /// <summary>
        /// Export multiple worksheets (Sheets)
        /// </summary>
        /// <param name="sheets"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected (string, string) ExportExcelMini(Dictionary<string, object> sheets, string fileName)
        {
            IWebHostEnvironment webHostEnvironment = (IWebHostEnvironment)App.ServiceProvider.GetService(typeof(IWebHostEnvironment));
            string sFileName = $"{fileName}{DateTime.Now:MM-dd-HHmmss}.xlsx";
            string fullPath = Path.Combine(webHostEnvironment.WebRootPath, "export", sFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            MiniExcel.SaveAs(fullPath, sheets);
            return (sFileName, fullPath);
        }

        /// <summary>
        /// Download import template
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="list">Empty data type collection</param>
        /// <param name="fileName">Download file name</param>
        /// <returns></returns>
        protected (string, string) DownloadImportTemplate<T>(List<T> list, string fileName)
        {
            IWebHostEnvironment webHostEnvironment = App.WebHostEnvironment;
            string sFileName = $"{fileName}.xlsx";
            string fullPath = Path.Combine(webHostEnvironment.WebRootPath, "ImportTemplate", sFileName);

            //Create template if it doesn't exist
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            }
            if (!Path.Exists(fullPath))
            {
                MiniExcel.SaveAs(fullPath, list, overwriteFile: true);
            }
            return (sFileName, fullPath);
        }

        /// <summary>
        /// Download specified file template
        /// </summary>
        /// <param name="fileName">Download file name</param>
        /// <returns></returns>
        protected (string, string) DownloadImportTemplate(string fileName)
        {
            IWebHostEnvironment webHostEnvironment = (IWebHostEnvironment)App.ServiceProvider.GetService(typeof(IWebHostEnvironment));
            string sFileName = $"{fileName}.xlsx";
            string fullPath = Path.Combine(webHostEnvironment.WebRootPath, "ImportTemplate", sFileName);

            return (sFileName, fullPath);
        }
    }
}

