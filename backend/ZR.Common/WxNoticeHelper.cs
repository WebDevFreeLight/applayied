using Infrastructure;
using System.Collections.Generic;
using System.Text.Json;
using ZR.Common.Model;

namespace ZR.Common
{
    public class WxNoticeHelper
    {
        //CorpID  ??ID 
        //AGENTID ???ID
        //Secret ???ID?????
        private static readonly string AGENTID = AppSettings.App(new string[] { "WxCorp", "AgentID" });
        private static readonly string CORPID = AppSettings.App(new string[] { "WxCorp", "CorpID" });
        private static readonly string CORPSECRET = AppSettings.App(new string[] { "WxCorp", "CorpSecret" });
        private static readonly string SEND_USER = AppSettings.App(new string[] { "WxCorp", "SendUser" });
        private static readonly string SendUrl = "https://qyapi.weixin.qq.com/cgi-bin/message/send";
        private static readonly string GetTokenUrl = "https://qyapi.weixin.qq.com/cgi-bin/gettoken";

        /// <summary>
        /// ????
        /// </summary>
        public enum MsgType { markdown, text, textcard, interactive_taskcard }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="toUser">??????id,??@all????????????</param>
        /// <param name="title">??</param>
        /// <param name="content">??</param>
        /// <param name="msgType">????</param>
        /// <returns></returns>
        public static (int, string) SendMsg(string title, string content, string toUser = "", MsgType msgType = MsgType.text)
        {
            if (string.IsNullOrEmpty(toUser))
            {
                toUser = SEND_USER;
            }
            if (string.IsNullOrEmpty(title))
            {
                return (0, "Title cannot be empty");
            }
            if (string.IsNullOrEmpty(CORPID))
            {
                System.Console.WriteLine("To receive WeChat exception messages, please complete WeChat Enterprise configuration");
                return (0, "Please complete WeChat Enterprise notification configuration");
            }
            WxTokenResult tokenResult = GetAccessToken();

            if (tokenResult == null || tokenResult.errcode != 0)
            {
                return (0, tokenResult?.errmsg);
            }

            Dictionary<string, object> dic = null;
            switch (msgType)
            {
                case MsgType.markdown:
                    dic = GetMarkdown(title, content, toUser);
                    break;
                case MsgType.text:
                    dic = GetText(title, content, toUser);
                    break;
                case MsgType.textcard:
                    break;
                case MsgType.interactive_taskcard:
                    break;
                default:
                    dic = GetText(title, content, toUser);
                    break;
            }
            string postData = JsonSerializer.Serialize(dic);
            string msgUrl = $"{SendUrl}?access_token={tokenResult.access_token}";

            //????
            //{"errcode":0,"errmsg":"ok","invaliduser":""}
            string msgResult = HttpHelper.HttpPost(msgUrl, postData, "contentType/json");
            WxTokenResult getTokenResult = JsonSerializer.Deserialize<WxTokenResult>(msgResult);
            System.Console.WriteLine(msgResult);
            return (getTokenResult?.errcode == 0 ? 100 : 0, getTokenResult?.errmsg);
        }
        public static (int, string) SendMsg(string title, string content, string toUser)
        {
            return SendMsg(title, content, toUser, MsgType.markdown);
        }

        /// <summary>
        /// ????token
        /// </summary>
        /// <returns>           
        /// {"errcode":0,"errmsg":"ok","access_token":"iCbcfE1OjfRhV0_io-CzqTNC0lnrudeW3oF5rhJKfmINaxLClLa1FoqAY_wEXtodYh_DTnrtAwZfzeb-NRXvwiOoqUTHx3i6QKLYcfBtF8y-xd5mvaeaf3e9mvTAPhmX0lkm1cLTwRLmoa1IwzgQ-QZEZcuIcntWdEMGseVYok3BwCGpC87bt6nNdgnekZdFVRp1uuaxoctDGlXpoQlQsA","expires_in":7200}
        /// </returns>
        private static WxTokenResult GetAccessToken()
        {
            string getTokenUrl = $"{GetTokenUrl}?corpid={CORPID}&corpsecret={CORPSECRET}";
            string getTokenResult = HttpHelper.HttpGet(getTokenUrl);
            System.Console.WriteLine(getTokenResult);
            WxTokenResult tokenResult = JsonSerializer.Deserialize<WxTokenResult>(getTokenResult);
            return tokenResult;
        }

        /// <summary>
        /// ??text
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="toUser"></param>
        /// <returns></returns>
        private static Dictionary<string, object> GetText(string title, string content, string toUser = "")
        {
            Dictionary<string, object> dic = new()
            {
                    { "msgtype", "text" },
                    { "touser", toUser },
                    { "agentid", AGENTID },
                    { "text", new Dictionary<string, string>
                    {
                        { "content",$"{title}\n\n{content}"
                    }
                }}
                };
            return dic;
        }

        /// <summary>
        /// ??markdown
        /// </summary>
        /// <param name="title">??????</param>
        /// <param name="content">?????</param>
        /// <param name="toUser">?????????,??ID??(??????‘|’??,????1000?)? ????:???”@all”,??????????????</param>
        /// <returns></returns>
        private static Dictionary<string, object> GetMarkdown(string title, string content, string toUser = "")
        {
            Dictionary<string, object> dic = new()
            {
                { "touser", toUser },
                { "msgtype", "markdown" },
                { "agentid", AGENTID },
                { "enable_duplicate_check", 1 },
                {
                    "markdown",
                    new Dictionary<string, string>
                {
                    { "content", $"**{title}**\n\n{content}" }
                }
                }
            };
            return dic;
        }
    }
}

