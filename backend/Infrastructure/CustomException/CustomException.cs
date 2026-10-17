using System;

namespace Infrastructure
{
    public class CustomException : Exception
    {
        public int Code { get; set; }
        /// <summary>
        /// Frontend prompt message
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// Detailed content recorded to log
        /// </summary>
        public string LogMsg { get; set; }
        /// <summary>
        /// Whether to notify
        /// </summary>
        public bool Notice { get; set; } = true;

        public CustomException(string msg) : base(msg)
        {
        }
        public CustomException(int code, string msg) : base(msg)
        {
            Code = code;
            Msg = msg;
        }

        public CustomException(ResultCode resultCode, string msg, bool notice = true) : base(msg)
        {
            Code = (int)resultCode;
            Notice = notice;
        }

        /// <summary>
        /// Custom exception
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="msg"></param>
        /// <param name="errorMsg">Used to record detailed logs to output medium</param>
        public CustomException(ResultCode resultCode, string msg, object errorMsg) : base(msg)
        {
            Code = (int)resultCode;
            if (errorMsg != null)
            {
                LogMsg = errorMsg.ToString();
            }
        }
    }
}
