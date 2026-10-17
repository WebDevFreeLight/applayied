using Infrastructure;
using Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using ZR.Common.Model;

namespace ZR.Common
{
    public class WxHelper
    {
        private static readonly string GetTokenUrl = "https://api.weixin.qq.com/cgi-bin/token";
        private static readonly string GetTicketUrl = "https://api.weixin.qq.com/cgi-bin/ticket/getticket";
        private static readonly string AppID = AppSettings.App(new string[] { "WxOpen", "AppID" });
        private static readonly string AppSECRET = AppSettings.App(new string[] { "WxOpen", "AppSecret" });

        /// <summary>
        /// Get access token
        /// </summary>
        /// <returns>           
        /// {"errcode":0,"errmsg":"ok","access_token":"iCbcfE1OjfRhV0_io-CzqTNC0lnrudeW3oF5rhJKfmINaxLClLa1FoqAY_wEXtodYh_DTnrtAwZfzeb-NRXvwiOoqUTHx3i6QKLYcfBtF8y-xd5mvaeaf3e9mvTAPhmX0lkm1cLTwRLmoa1IwzgQ-QZEZcuIcntWdEMGseVYok3BwCGpC87bt6nNdgnekZdFVRp1uuaxoctDGlXpoQlQsA","expires_in":7200}
        /// </returns>
        private static WxTokenResult GetAccessToken()
        {
            if (AppID.IsEmpty() || AppSECRET.IsEmpty())
            {
                Console.WriteLine("WeChat Official Account configuration error");
                throw new ArgumentException("WeChat Official Account configuration error");
            };
            var Ck = "wx_token";
            string getTokenUrl = $"{GetTokenUrl}?grant_type=client_credential&appid={AppID}&secret={AppSECRET}";
            if (CacheHelper.Get(Ck) is WxTokenResult tokenResult)
            {
                return tokenResult;
            }
            else
            {
                string result = HttpHelper.HttpGet(getTokenUrl);

                tokenResult = JsonConvert.DeserializeObject<WxTokenResult>(result);

                if (tokenResult?.errcode == 0)
                {
                    CacheHelper.SetCache(Ck, tokenResult, 110);
                }
                else
                {
                    Console.WriteLine("GetAccessToken failed, result=" + result);
                    throw new Exception("Failed to get AccessToken");
                }
            }
            return tokenResult;
        }

        /// <summary>
        /// ??ticket
        /// </summary>
        /// <returns></returns>
        public static WxTokenResult GetTicket()
        {
            WxTokenResult token = GetAccessToken();
            string ticket = token?.access_token;
            var Ck = "wx_ticket";
            string getTokenUrl = $"{GetTicketUrl}?access_token={ticket}&type=jsapi";
            if (CacheHelper.Get(Ck) is WxTokenResult tokenResult)
            {
                return tokenResult;
            }
            else
            {
                string result = HttpHelper.HttpGet(getTokenUrl);
                tokenResult = JsonConvert.DeserializeObject<WxTokenResult>(result);

                if (tokenResult?.errcode == 0)
                {
                    CacheHelper.SetCache(Ck, tokenResult, 110);
                }
                else
                {
                    Console.WriteLine("GetTicket, result=" + result);
                    throw new Exception("Failed to get ticket");
                }
            }
            return tokenResult;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="jsapi_ticket"></param>
        /// <param name="timestamp"></param>
        /// <param name="noncestr"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetSignature(string jsapi_ticket, string timestamp, string noncestr, string url = null)
        {
            if (string.IsNullOrEmpty(jsapi_ticket) || string.IsNullOrEmpty(noncestr) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(url))
                return null;

            //??????????
            string[] arr = new[]
            {
                 string.Format("jsapi_ticket={0}",jsapi_ticket),
                 string.Format("noncestr={0}",noncestr),
                 string.Format("timestamp={0}",timestamp),
                 string.Format("url={0}",url)
             };
            //????
            Array.Sort(arr);
            //??URL????????????
            var temp = string.Join("&", arr);
            
            var sha1Arr = SHA1.HashData(Encoding.UTF8.GetBytes(temp));

            return BitConverter.ToString(sha1Arr).Replace("-", "").ToLower();
        }
    }
}

