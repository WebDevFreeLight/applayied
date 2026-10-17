using Infrastructure.Constant;
using System.Collections.Generic;

namespace Infrastructure.Model
{
    public class ApiResult : Dictionary<string, object>
    {
        /// <summary>
        /// ???
        /// </summary>
        public static readonly string CODE_TAG = "code";

        /// <summary>
        /// ????
        /// </summary>
        public static readonly string MSG_TAG = "msg";

        /// <summary>
        /// ????
        /// </summary>
        public static readonly string DATA_TAG = "data";

        /// <summary>
        /// ?????????APIResult??,?????????
        /// </summary>
        public ApiResult()
        {
        }

        /// <summary>
        /// ????????? ApiResult ??
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public ApiResult(int code, string msg)
        {
            Add(CODE_TAG, code);
            Add(MSG_TAG, msg);
        }

        /// <summary>
        /// ????????? ApiResult ??
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public ApiResult(int code, string msg, object data)
        {
            Add(CODE_TAG, code);
            Add(MSG_TAG, msg);
            if (data != null)
            {
                Add(DATA_TAG, data);
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// < returns > ???? </ returns >
        public static ApiResult Success() { return new ApiResult(HttpStatus.SUCCESS, "success"); }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="data"></param>
        /// <returns> ???? </returns >
        public static ApiResult Success(object data) { return new ApiResult(HttpStatus.SUCCESS, "success", data); }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="msg">????</param>
        /// <returns>????</returns>
        public static ApiResult Success(string msg) { return new ApiResult(HttpStatus.SUCCESS, msg, null); }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="msg">????</param>
        /// <param name="data">????</param>
        /// <returns>????</returns>
        public static ApiResult Success(string msg, object data) { return new ApiResult(HttpStatus.SUCCESS, msg, data); }

        public static ApiResult Error(ResultCode code, string msg = "")
        {
            return Error((int)code, msg);
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult Error(int code, string msg) { return new ApiResult(code, msg); }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult Error(string msg) { return new ApiResult((int)ResultCode.CUSTOM_ERROR, msg); }


        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public bool IsSuccess()
        {
            return HttpStatus.SUCCESS == (int)this[CODE_TAG];
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ApiResult Put(string key, object value)
        {
            Add(key, value);
            return this;
        }
    }
}

