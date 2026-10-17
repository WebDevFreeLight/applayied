using System.ComponentModel;

namespace Infrastructure
{
    public enum ResultCode
    {
        [Description("success")]
        SUCCESS = 200,

        [Description("No more data")]
        NO_DATA = 210,

        [Description("Parameter error")]
        PARAM_ERROR = 101,

        [Description("Verification code error")]
        CAPTCHA_ERROR = 103,

        [Description("Login error")]
        LOGIN_ERROR = 105,

        [Description("Operation failed")]
        FAIL = 1,

        [Description("Server error occurred")]
        GLOBAL_ERROR = 500,

        [Description("Custom exception")]
        CUSTOM_ERROR = 110,

        [Description("Invalid request")]
        INVALID_REQUEST = 116,

        [Description("Authorization failed")]
        OAUTH_FAIL = 201,

        [Description("Please bind phone number first")]
        PHONE_BIND = 202,

        [Description("Unauthorized")]
        DENY = 401,

        [Description("Authorization access failed")]
        FORBIDDEN = 403,

        [Description("Bad Request")]
        BAD_REQUEST = 400
    }
}

