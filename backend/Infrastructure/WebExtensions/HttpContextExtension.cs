using Infrastructure.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UAParser;
using ZR.Common;
using ZR.Infrastructure.IPTools;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// HttpContext Extension Class
    /// </summary>
    public static partial class HttpContextExtension
    {
        /// <summary>
        /// Check if it's an AJAX request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            //return request.Headers.ContainsKey("X-Requested-With") &&
            //       request.Headers["X-Requested-With"].Equals("XMLHttpRequest");

            return request.Headers["X-Requested-With"] == "XMLHttpRequest" || request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientUserIp(this HttpContext context)
        {
            if (context == null) return "";
            var result = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                result = context.Connection.RemoteIpAddress?.ToString();
            }
            if (string.IsNullOrEmpty(result))
                throw new Exception("Failed to get IP address");

            if (result.Contains("::1"))
                result = "127.0.0.1";

            result = result.Replace("::ffff:", "");
            result = result.Split(':')?.FirstOrDefault() ?? "127.0.0.1";
            result = IsIP(result) ? result : "127.0.0.1";
            return result;
        }

        /// <summary>
        /// Check if the string is a valid IP address
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        /// <summary>
        /// Get logged in user ID
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static long GetUId(this HttpContext context)
        {
            var uid = context.User.FindFirstValue(ClaimTypes.PrimarySid);
            return !string.IsNullOrEmpty(uid) ? long.Parse(uid) : 0;
        }



        public static int GetCompanyId(this HttpContext context)
        {
            if (context is null)
            {
                return 0;
            }
            // Prefer request-scoped injected value (e.g., FixedCompanyBranchScopingMiddleware)
            if (context.Items != null && context.Items.TryGetValue("CompanyId", out var companyObj) && companyObj != null)
            {
                if (companyObj is long companyLong) return (int)companyLong;
                if (companyObj is int companyInt) return companyInt;
                if (companyObj is string companyStr && int.TryParse(companyStr, out var parsedCompany)) return parsedCompany;
            }
            var CompanyId = context.User.FindFirstValue("CompanyId");
            return !string.IsNullOrEmpty(CompanyId) ? int.Parse(CompanyId) : 0;
        }

        public static int GetBranchID(this HttpContext context)
        {
            if (context is null)
            {
                return 0;
            }
            // Prefer request-scoped injected value (e.g., FixedCompanyBranchScopingMiddleware)
            if (context.Items != null && context.Items.TryGetValue("BranchId", out var branchObj) && branchObj != null)
            {
                if (branchObj is long branchLong) return (int)branchLong;
                if (branchObj is int branchInt) return branchInt;
                if (branchObj is string branchStr && int.TryParse(branchStr, out var parsedBranch)) return parsedBranch;
            }
            var BranchID = context.User.FindFirstValue("BranchID");
            return !string.IsNullOrEmpty(BranchID) ? int.Parse(BranchID) : 0;
        }

        public static string? GetFinancialYearID(this HttpContext context, string type = "FinancialyearID")
        {
            if (context == null)
            {
                return null;
            }

            var user = context.User;

            var financialYearID = user.FindFirstValue(type);
            return !string.IsNullOrEmpty(financialYearID) ? financialYearID.ToString() : null;
        }





        /// <summary>
        /// Get department ID
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static long GetDeptId(this HttpContext context)
        {
            var deptId = context.User.FindFirstValue(ClaimTypes.GroupSid);
            return !string.IsNullOrEmpty(deptId) ? long.Parse(deptId) : 0;
        }

        /// <summary>
        /// Get logged in username
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetName(this HttpContext context)
        {
            var uid = context.User?.Identity?.Name;
            return uid;
        }

        /// <summary>
        /// Check if user is admin
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsAdmin(this HttpContext context)
        {
            var userName = context.GetName();
            return userName == GlobalConstant.AdminRole;
        }

        /// <summary>
        /// ClaimsIdentity
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<ClaimsIdentity> GetClaims(this HttpContext context)
        {
            return context.User?.Identities;
        }
        //public static int GetRole(this HttpContext context)
        //{
        //    var roleid = context.User.FindFirstValue(ClaimTypes.Role) ?? "0";

        //    return int.Parse(roleid);
        //}

        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request.Headers["User-Agent"];
        }

        /// <summary>
        /// Get request token
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetToken(this HttpContext context)
        {
            return context.Request.Headers["Authorization"];
        }

        /// <summary>
        /// Get tenant ID
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetTenantId(this HttpContext context)
        {
            return context.Request.Headers["tenantId"];
        }

        /// <summary>
        /// Get request URL
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRequestUrl(this HttpContext context)
        {
            return context != null ? context.Request.Path.Value : "";
        }

        /// <summary>
        /// Get request parameters
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetQueryString(this HttpContext context)
        {
            return context != null ? context.Request.QueryString.Value : "";
        }

        /// <summary>
        /// Get body request parameters
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetBody(this HttpContext context)
        {
            context.Request.EnableBuffering();
            //context.Request.Body.Seek(0, SeekOrigin.Begin);
            //using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            ////Need to use async method to get data
            //return reader.ReadToEndAsync().Result;
            string body = string.Empty;
            var buffer = new MemoryStream();
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            context.Request.Body.CopyToAsync(buffer);
            buffer.Position = 0;
            try
            {
                using StreamReader streamReader = new(buffer, Encoding.UTF8);
                body = streamReader.ReadToEndAsync().Result;
            }
            finally
            {
                buffer?.Dispose();
            }
            return body;
        }

        /// <summary>
        /// Get body request parameters (async)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<string> GetBodyAsync(this HttpContext context)
        {
            // Allow multiple reads of the request body
            context.Request.EnableBuffering();

            // Reset the request body stream position to ensure reading from the beginning
            context.Request.Body.Position = 0;

            // Read request body content
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            // After reading, reset the stream position to 0 to ensure subsequent operations can continue reading
            context.Request.Body.Position = 0;

            return body;
        }

        /// <summary>
        /// Get browser information
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ClientInfo GetClientInfo(this HttpContext context)
        {
            var str = context.GetUserAgent();
            var uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(str);

            return c;
        }

        /// <summary>
        /// Get geographic location by IP
        /// </summary>
        /// <returns></returns>
        public static string GetIpInfo(string IP)
        {
            var ipInfo = IpTool.Search(IP);
            return ipInfo?.Province + "-" + ipInfo?.City + "-" + ipInfo?.NetworkOperator;
        }

        /// <summary>
        /// Set request parameters
        /// </summary>
        /// <param name="reqMethod"></param>
        /// <param name="context"></param>
        public static string GetRequestValue(this HttpContext context, string reqMethod)
        {
            string param = string.Empty;

            if (HttpMethods.IsPost(reqMethod) || HttpMethods.IsPut(reqMethod) || HttpMethods.IsDelete(reqMethod))
            {
                param = context.GetBody();
                string regex = "(?<=\"password\":\")[^\",]*";
                param = Regex.Replace(param, regex, "***");
            }
            if (param.IsEmpty())
            {
                param = context.GetQueryString();
            }
            return param;
        }

        /// <summary>
        /// Get current user login information
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TokenModel GetCurrentUser(this HttpContext context)
        {
            var tokenModel = JwtUtil.GetLoginUser(context);
            if (tokenModel != null)
            {
                tokenModel.Permissions = (List<string>)CacheHelper.GetCache(GlobalConstant.UserPermKEY + tokenModel.UserId);
            }
            return tokenModel;
        }

        /// <summary>
        /// Check if user has sensitive data permissions
        /// </summary>
        /// <param name="context"></param>
        /// <param name="perm"></param>
        /// <returns></returns>
        public static bool HasSensitivePerm(this HttpContext context, string perm)
        {
            if (IsAdmin(context)) return true;
            var perms = context.User?.FindFirst("sensitivePerms")?.Value?.Split(',') ?? [];
            return perms.Contains(perm);
        }
    }
}

